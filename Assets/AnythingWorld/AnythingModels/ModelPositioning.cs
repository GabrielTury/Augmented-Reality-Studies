using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class ModelPositioning
    {
        public static void ApplyTransforms(ModelData data, Action<ModelData> onSuccess)
        {
            ApplyParentTransform(data.model, data.parameters.parentTransform);
            ApplyRotationAccordingToSpace(data);

            if (data.parameters.position.value != Vector3.zero || !data.parameters.placeOnGrid)
            {
#if UNITY_EDITOR
                //this is a drag and drop model getting the position
                data.model.transform.position = TransformSettings.DragPosition;
#endif
                ApplyPositionAccordingToSpace(data);
                AdjustGroundModelPositioning(data);
            }
            else
            {
                AdjustIntraModelPositioning(data);
                ApplyGridPositionAccordingToSpace(data);
                if (!data.parameters.useGridArea)
                    AdjustGroundModelPositioning(data);
            }

            onSuccess?.Invoke(data);
        }

        private static void AdjustIntraModelPositioning(ModelData data)
        {
            if (data.parameters.placeOnGround)
            {
                //Debug.Log($"Bounds ({data.loadedData.boundsYOffset}) * localScale ({data.model.transform.localScale.y}) = {data.loadedData.boundsYOffset/2 * data.model.transform.localScale.y}");
                data.model.transform.localPosition += new Vector3(0, data.loadedData.boundsYOffset, 0);
            }
        }

        private static void AdjustGroundModelPositioning(ModelData data)
        {
            if (data.parameters.placeOnGround)
            {
                //put the object in the ground
                //check if there ground under the object
                RaycastHit hit;
                //SprereCast to get the ground is more accurate than RayCast
                if (Physics.SphereCast(data.model.transform.position + Vector3.up * 5, 0.2f, Vector3.down, out hit, 10f))
                {
                    //get the lowest lowest bounding box point of the all children of the object
                    float lowestY = data.model.GetComponentInChildren<Renderer>().bounds.min.y;
                    foreach (Renderer child in data.model.GetComponentsInChildren<Renderer>())
                    {
                        if (child != null)
                        {
                            float childLowestY = child.bounds.min.y;
                            if (childLowestY < lowestY)
                            {
                                lowestY = childLowestY;
                            }
                        }
                    }
                    //get the distance between the lowest lowest bounding box point of the object and the ground
                    var distance = hit.point.y - lowestY;

                    //put the object in the ground respecting bounding box
                    data.model.transform.localPosition = new Vector3(data.model.transform.localPosition.x, hit.point.y + distance, data.model.transform.localPosition.z);
                }
                else
                {
                    //put the object in the ground respecting bounding box genericly (not accurate)
                    data.model.transform.localPosition += new Vector3(0, data.loadedData.boundsYOffset, 0);
                }
            }
        }

        private static void ApplyGridPositionAccordingToSpace(ModelData data)
        {
            Vector3 gridPosition;

            if (!data.parameters.useGridArea)
                gridPosition = SimpleGrid.AddCell();
            else
                gridPosition = GridArea.AddModel(data.model);

            switch (data.parameters.transformSpace)
            {
                case Utilities.TransformSpace.World:
                    ApplyWorldSpacePosition(data.model, gridPosition);
                    break;

                case Utilities.TransformSpace.Local:
                    ApplyLocalSpacePosition(data.model, gridPosition);
                    break;
            }
        }
        private static void ApplyPositionAccordingToSpace(ModelData data)
        {
            switch (data.parameters.transformSpace)
            {
                case Utilities.TransformSpace.World:
                    ApplyWorldSpacePosition(data.model, data.parameters.position.value);
                    break;

                case Utilities.TransformSpace.Local:
                    ApplyLocalSpacePosition(data.model, data.parameters.position.value);
                    break;
            }
        }

        private static void ApplyRotationAccordingToSpace(ModelData data)
        {

            switch (data.parameters.transformSpace)
            {
                case Utilities.TransformSpace.World:
                    ApplyWorldSpaceRotation(data.model, data.parameters.rotation);
                    break;

                case Utilities.TransformSpace.Local:
                    ApplyLocalSpaceRotation(data.model, data.parameters.rotation);
                    break;
            }
        }

        private static void ApplyLocalSpacePosition(GameObject model, Vector3 position)
        {
            model.transform.localPosition = position;
        }

        private static void ApplyLocalSpaceRotation(GameObject model, Quaternion rotation)
        {
            model.transform.localRotation = rotation;
        }

        private static void ApplyWorldSpacePosition(GameObject model, Vector3 position)
        {
            model.transform.position = position;
        }

        private static void ApplyWorldSpaceRotation(GameObject model, Quaternion rotation)
        {
            model.transform.rotation = rotation;
        }

        private static void ApplyParentTransform(GameObject model, Transform parentTransform)
        {
            model.transform.parent = parentTransform;
        }
    }
}
