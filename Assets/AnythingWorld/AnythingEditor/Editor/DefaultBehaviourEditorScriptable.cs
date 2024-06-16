using UnityEditor;
using UnityEngine;
namespace AnythingWorld.Editor
{
    //[CreateAssetMenu(fileName = "DefaultBehaviours", menuName = "ScriptableObjects/DefaultBehaviour", order = 1)]
    public class DefaultBehaviourEditorScriptable : ScriptableObject
    {
        public MonoScript defaultRigged;
        public MonoScript defaultWheeledVehicle;
        public MonoScript defaultPropellorVehicle;
        public MonoScript defaultFlyingAnimal;
        public MonoScript defaultSwimmingAnimal;
    }
}

