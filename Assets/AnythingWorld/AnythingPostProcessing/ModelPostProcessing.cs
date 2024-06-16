using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using UnityEngine;

namespace AnythingWorld.Core
{
    public static class ModelPostProcessing
    {
        public static void FinishMakeProcess(ModelData data)
        {
            // Invoke factory actions stored for successful creation of model.
            foreach (var action in data.actions.onSuccess)
            {
                action?.Invoke(data, "Successfully made");
            }
            // If collider specified add collider
            if (data.parameters.addCollider)
            {
                AddCollider(data);
            }
            // If rigidbody specified, add rigibody
            if (data.parameters.addRigidbody)
            {
                AddRigidbody(data);
            }

            // If serializing parameter passed, attempt to serialize.
            if (data.parameters.serializeAsset)
            {
                AssetSaver.CreateAssetFromData(new CallbackInfo(data));
            }

            //dirty scene on succesful completion
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

#endif
        }
        private static void AddCollider(ModelData data)
        {
            var skinnedRenderer = data.model.GetComponentInChildren<SkinnedMeshRenderer>();
            Bounds bounds;
            if (skinnedRenderer != null)
            {
                bounds = GetBoundsSkinnedMeshRenderer(data);
            }
            else
            {
                if (data.animationPipeline == AnimationPipeline.PropellorVehicle)
                {
                    bounds = GetBoundsMeshFilterFromMainBodyOnly(data);
                }
                else
                {
                    bounds = GetBoundsMeshFilter(data);
                }
            }

            switch (data.animationPipeline)
            {
                case AnimationPipeline.Static: //TODO: Add an option to allow users for a simplified collider instead.
                    foreach (var mf in data.model.GetComponentsInChildren<MeshFilter>())
                    {
                        var mfRb = mf.gameObject.AddComponent<MeshCollider>();
                        mfRb.convex = true;
                    }
                    break;

                case AnimationPipeline.WheeledVehicle:
                    var carCollider = data.model.AddComponent<BoxCollider>();
                    carCollider.size = bounds.size;
                    carCollider.center = bounds.center;
                    break;

                default:
                    var modelCollider = data.model.AddComponent<CapsuleCollider>();
                    bounds = DetermineAxis(bounds, modelCollider);
                    modelCollider.center = bounds.center;

                    switch (modelCollider.direction)
                    {
                        case 0:
                            modelCollider.height = bounds.extents.x * 2;
                            modelCollider.radius = Math.Max(bounds.extents.y, bounds.extents.z);
                            break;
                        case 1:
                            modelCollider.height = bounds.extents.y * 2;
                            modelCollider.radius = Math.Max(bounds.extents.x, bounds.extents.z);
                            break;
                        case 2:
                            modelCollider.height = bounds.extents.z * 2;
                            modelCollider.radius = Math.Max(bounds.extents.x, bounds.extents.y);
                            break;
                    }
                    break;
            }
        }

        private static Bounds DetermineAxis(Bounds bounds, CapsuleCollider modelCollider)
        {
            if (bounds.extents.y > bounds.extents.x)
            {
                if (bounds.extents.y > bounds.extents.z)
                {
                    modelCollider.direction = 1;
                }
                else
                {
                    modelCollider.direction = 2;
                }
            }
            else if (bounds.extents.x > bounds.extents.z)
            {
                modelCollider.direction = 0;
            }
            else modelCollider.direction = 2;
            return bounds;
        }

        private static Bounds GetBoundsSkinnedMeshRenderer(ModelData data)
        {
            // Have to do a complex process because the root bone is rotated
            // which means the automatically generated bounds for the SMR
            // are also rotated, causing them to be inaccurate.
            var skinnedRenderer = data.model.GetComponentInChildren<SkinnedMeshRenderer>();
            Bounds bounds;
            const float waistShoulderRatio = 1.5f;
            var rot = skinnedRenderer.rootBone.localRotation;
            skinnedRenderer.rootBone.localRotation = Quaternion.Euler(0, 0, 0);
            skinnedRenderer.updateWhenOffscreen = true;
            skinnedRenderer.sharedMesh.RecalculateBounds();

            bounds = new Bounds();
            Vector3 center = skinnedRenderer.sharedMesh.bounds.center;
            Vector3 extents = new Vector3(skinnedRenderer.sharedMesh.bounds.extents.x, skinnedRenderer.sharedMesh.bounds.extents.y, skinnedRenderer.sharedMesh.bounds.extents.z);

            // for bipeds we need to adjust the bounds because they use the mesh neutral pose
            if (data.json.type.ToLower() == "biped_human")
            {
                extents.x = extents.z;
                extents.z *= waistShoulderRatio;
            }

            bounds = skinnedRenderer.bounds;
            bounds.center = center;
            bounds.extents = extents;

            skinnedRenderer.updateWhenOffscreen = false;
            skinnedRenderer.rootBone.localRotation = rot;
            return bounds;
        }
        private static Bounds GetBoundsMeshFilter(ModelData data)
        {
            Bounds bounds;
            var meshfilters = data.model.GetComponentsInChildren<MeshFilter>();
            bounds = GetObjectBounds(meshfilters);
            return bounds;
        }

        /// <summary>
        /// Only get the bounds of the main body of a model.
        /// </summary>
        /// <param name="data">The model data.</param>
        /// <returns>The bounds of the main body of a model.</returns>
        private static Bounds GetBoundsMeshFilterFromMainBodyOnly(ModelData data)
        {
            Bounds bounds;
            var meshfilters = new MeshFilter[] { data.model.GetComponentInChildren<MeshFilter>() };
            bounds = GetObjectBounds(meshfilters);
            return bounds;
        }
        private static void AddRigidbody(ModelData data)
        {
            var rb = data.model.AddComponent<Rigidbody>();
            rb.mass = data.json.mass;
        }

        private static Vector3 GetObjectBounds(Renderer[] renderers)
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var objRenderer in renderers)
            {
                if (bounds.size == Vector3.zero)
                    bounds = objRenderer.bounds;
                else
                    bounds.Encapsulate(objRenderer.bounds);
            }
            return bounds.size;
        }

        private static Bounds GetObjectBounds(MeshFilter[] meshFilters)
        {
            var totalBounds = new Bounds(Vector3.zero, Vector3.zero);
            var meshCenter = Vector3.zero;

            foreach (var mFilter in meshFilters)
            {
                var mMesh = mFilter.sharedMesh;
                meshCenter += mMesh.bounds.center;
            }
            meshCenter /= meshFilters.Length;
            totalBounds.center = meshCenter;

            foreach (var mFilter in meshFilters)
            {
                var mMesh = mFilter.sharedMesh;
                if (totalBounds.size == Vector3.zero)
                    totalBounds = mMesh.bounds;
                else
                    totalBounds.Encapsulate(mMesh.bounds);
            }

            return totalBounds;
        }
    }

}
