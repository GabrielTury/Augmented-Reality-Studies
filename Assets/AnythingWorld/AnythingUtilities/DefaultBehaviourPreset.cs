using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Utilities
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "DefaultBehaviours", menuName = "ScriptableObjects/DefaultBehaviour", order = 1)]
    public class DefaultBehaviourPreset : ScriptableObject
    {
        [SerializeField] public List<BehaviourRule> behaviourRules = new List<BehaviourRule>();
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            AssetDatabase.importPackageCompleted += SetupDefaultBehaviourPreset;
#else
            SetupDefaultBehaviourPreset("");
#endif
        }

        private void SetupDefaultBehaviourPreset(string packageName)
        {
            TransformSettings.GetInstance();
            behaviourRules = new List<BehaviourRule>();
#if UNITY_EDITOR
            if (TransformSettings.StaticBehaviourScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.Static, TransformSettings.StaticBehaviourScript.name));
            if (TransformSettings.ShaderBehaviourScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.Shader, TransformSettings.ShaderBehaviourScript.name));
            if (TransformSettings.RiggedAnimalScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.WalkingAnimal, TransformSettings.RiggedAnimalScript.name));
            if (TransformSettings.GroundVehicleScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.WheeledVehicle, TransformSettings.GroundVehicleScript.name));
            if (TransformSettings.FlyingVehicleScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.FlyingVehicle, TransformSettings.FlyingVehicleScript.name));
            if (TransformSettings.FlyingAnimalScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.FlyingAnimal, TransformSettings.FlyingAnimalScript.name));
            if (TransformSettings.SwimmingAnimalScript != null) behaviourRules.Add(new BehaviourRule(DefaultBehaviourType.SwimmingAnimal, TransformSettings.SwimmingAnimalScript.name));
#endif
        }
    }
    [System.Serializable]
    public class BehaviourRule
    {
        [SerializeField]
        public DefaultBehaviourType behaviourType;
        [SerializeField]
        public string scriptName;
        public BehaviourRule(DefaultBehaviourType _behaviourType, string _scriptName)
        {
            behaviourType = _behaviourType;
            scriptName = _scriptName;
        }
    }
}
