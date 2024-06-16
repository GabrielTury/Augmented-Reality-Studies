using AnythingWorld.Utilities.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class ModelScaling
    {
        public static void Scale(ModelData data, Action<ModelData> onSuccess)
        {
            //Parse dimension dictionary from db into vector 3
            LoadDimensionsVectorFromDB(data);
            if (ParseScaleType(data))
            {
                ModelPositioning.ApplyTransforms(data, onSuccess);
            }
        }
        private static bool ParseScaleType(ModelData data)
        {
            if (data.parameters.scale.IsSet)
            {
                data.loadedData.bounds = data.parameters.scale.value * data.parameters.scaleMultiplier;
                switch (data.parameters.scaleType)
                {
                    case Utilities.ScaleType.SetRealWorld:
                        return TryCenterAndSizeObjectToBounds(data, data.loadedData.bounds);

                    case Utilities.ScaleType.ScaleRealWorld:
                        return TryCenterAndSizeObjectToBounds(data, data.loadedData.bounds);

                    case Utilities.ScaleType.Absolute:
                        data.model.transform.localScale = data.loadedData.bounds;
                        return true;
                }
            }
            else
            {
                data.loadedData.bounds = data.loadedData.dbDimensionsVector * data.parameters.scaleMultiplier;

                return TryCenterAndSizeObjectToBounds(data, data.loadedData.bounds);
            }
            return false;
        }

        private static bool TryCenterAndSizeObjectToBounds(ModelData data, Vector3 targetBoundingDimensions)
        {
            //targetBoundingDimensions = new Vector3(Minimum(targetBoundingDimensions.x), Minimum(targetBoundingDimensions.y), Minimum(targetBoundingDimensions.z));
            //Get renderers and object bounds from renderers.
            var renderers = data.model.GetComponentsInChildren<Renderer>();
            var objectBounds = GetObjectBounds(renderers);
            var skinnedRenderer = data.model.GetComponentInChildren<SkinnedMeshRenderer>();
            Bounds bounds;

            if (skinnedRenderer != null)
            {
                var rot = skinnedRenderer.rootBone.localRotation;
                skinnedRenderer.rootBone.localRotation = Quaternion.Euler(0, 0, 0);
                skinnedRenderer.updateWhenOffscreen = true;

                bounds = new Bounds();
                Vector3 center = skinnedRenderer.localBounds.center;
                Vector3 extents = skinnedRenderer.localBounds.extents;

                bounds.center = center;
                bounds.extents = extents;
                skinnedRenderer.updateWhenOffscreen = false;
                skinnedRenderer.localBounds = bounds;
                bounds = skinnedRenderer.bounds;
                skinnedRenderer.rootBone.localRotation = rot;

            }
            else
            {
                var meshfilters = data.model.GetComponentsInChildren<MeshFilter>();
                bounds = GetObjectBounds(meshfilters);
            }

            var relativeScale = CalculateRelativeScaleForDimension(objectBounds, targetBoundingDimensions);
            if (float.IsNaN(relativeScale) || float.IsPositiveInfinity(relativeScale) || float.IsNegativeInfinity(relativeScale))
            {
                //Debug.Log($"Bounds: {objectBounds}, target bounds: {targetBoundingDimensions}");
                string error = $"Error while calculating bounds scale for {data.guid}, calculated bound was {relativeScale.ToString()}";
                data?.actions?.onFailure?.Invoke(data, error);
                return false;
            }

            var pivotCenterDifference = (bounds.center - data.model.transform.position) * relativeScale;

            if (!data.json.preserveOriginalScale)
            {
                data.model.transform.localScale = new Vector3(relativeScale, relativeScale, relativeScale);
            }
            //Center object by moving bounds center down.
            if (!data.json.preserveOriginalPosition)
            {
                data.model.transform.position = data.model.transform.position - (pivotCenterDifference);
            }
            var yOffset = (bounds.size.y/2)*relativeScale;
            data.loadedData.boundsYOffset = yOffset;
            return true;
        }

        #region Utility Operations

        private static float CalculateRelativeScaleForDimension(Vector3 currentDimensions, Vector3 desiredDimensions)
        {
            var maxDesiredDimension = GetObjectScalarDimension(desiredDimensions);
            switch (maxDesiredDimension)
            {
                case ScalingDimension.x:
                    return GetRelativeScale(currentDimensions.x, desiredDimensions.x);

                case ScalingDimension.y:
                    return GetRelativeScale(currentDimensions.y, desiredDimensions.y);

                case ScalingDimension.z:
                    return GetRelativeScale(currentDimensions.z, desiredDimensions.z);

                case ScalingDimension.none:
                default:
                    //If scalar is none, default to 1m on the x axis.
                    return GetRelativeScale(currentDimensions.x, 1);
            }
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

        private static void LoadDimensionsVectorFromDB(ModelData data)
        {
            data.loadedData.dbDimensionsVector = ParseDimension(data.json.scale);
            if (data.parameters.clampDbScale)
            {
                data.loadedData.dbDimensionsVector = ClampVector3(data.loadedData.dbDimensionsVector, data.parameters.clampDbScaleLowerBounds.value, data.parameters.clampDbScaleUpperBounds.value);
            }
        }
        /// <summary>
        /// Clamp vector value-wise between two Vector bounds.
        /// </summary>
        /// <param name="input">Values to clamp.</param>
        /// <param name="lower">Lower bounds of values.</param>
        /// <param name="upper">Upper bounds of values.</param>
        /// <returns></returns>
        private static Vector3 ClampVector3(Vector3 input, Vector3 lower, Vector3 upper)
        {
            Vector3 output = Vector3.zero;
            output.x = Mathf.Clamp(input.x, lower.x, upper.x);
            output.y = Mathf.Clamp(input.y, lower.y, upper.y);
            output.z = Mathf.Clamp(input.z, lower.z, upper.z);
            return output;
        }
        private static float GetRelativeScale(float currentDimension, float desiredDimension)
        {
            return (desiredDimension) / currentDimension;
        }

        private static ScalingDimension GetObjectScalarDimension(Vector3 dimensions)
        {
            var maxDimension = Math.Max(dimensions.z, Math.Max(dimensions.x, dimensions.y));

            if (maxDimension == 0)
            {
                return ScalingDimension.none;
            }

            if (maxDimension == dimensions.x)
            {
                return ScalingDimension.x;
            }
            else if (maxDimension == dimensions.y)
            {
                return ScalingDimension.y;
            }
            else if (maxDimension == dimensions.z)
            {
                return ScalingDimension.z;
            }

            return ScalingDimension.none;
        }

        private enum ScalingDimension
        {
            x,
            y,
            z,
            none
        }

        private static Vector3 ParseDimension(Dictionary<string, float> dictionary)
        {
            Vector3 dimensions = Vector3.one;
            dictionary.TryGetValue("width", out dimensions.x);
            dictionary.TryGetValue("height", out dimensions.y);
            dictionary.TryGetValue("length", out dimensions.z);

            dimensions = SetMinimumDimensions(dimensions);


            return dimensions;
        }

        private static Vector3 SetMinimumDimensions(Vector3 _vector)
        {
            return new Vector3(Minimum(_vector.x), Minimum(_vector.y), Minimum(_vector.z));
        }
        private static float Minimum(float _float)
        {
            const float minimumDimension = 0.2f; //Minimum dimension of 20cm
            return Mathf.Max(minimumDimension, _float);
        }
        #endregion Utility Operations
    }
}
