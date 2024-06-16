using AnythingWorld.Utilities.Data;
using System.Collections.Generic;
using UnityEngine;
namespace AnythingWorld.Animation.Vehicles
{
    public static class VehiclePropellorAnimationLoader
    {
        public static void Load(ModelData data)
        {
            var animationScript = data.model.AddComponent<PropellorVehicleAnimator>();
            List<Transform> rawBlades = new List<Transform>();
            foreach (var part in data.loadedData.obj.loadedParts)
            {
                if (part.Key.Contains("wing"))
                {
                    rawBlades.Add(part.Value.transform);
                }
            }
            var center = GetCenterOfPropellor(rawBlades);
            foreach(var blade in rawBlades)
            {
                var go = CenterWheelPivot.CenterMeshCustomPivot(blade.gameObject, center);
                animationScript.propellorBlades.Add(go.transform.GetChild(0).gameObject);
            }

            

        }
        public static Vector3 GetCenterOfPropellor(List<Transform> propellors)
        {
            //Get primitive centroid
            Vector3 centerAggregate = Vector3.zero;
            foreach (var blade in propellors)
            {
                var center = blade.GetComponentInChildren<Renderer>().bounds.center;
                centerAggregate += center;
            }

            return (centerAggregate / propellors.Count);
        }
        public static void ParentBladesToHolder(ModelData data)
        {
            var newParent = new GameObject("Propellor");
            newParent.transform.parent = data.model.transform;

            foreach (var part in data.loadedData.obj.loadedParts)
            {
                if (part.Key.Contains("wing"))
                {
                    var centrePivot = CenterWheelPivot.CenterWheel(part.Value);
                }
            }
        }
    }
}
