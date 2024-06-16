using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Behaviour
{
    public static class BehaviourHandler
    {
        public static void AddBehaviours(ModelData data)
        {
            var wasDefaultBehaviourSet = false;
            
            if (data.parameters.categorizedBehaviours != null)
            {
                wasDefaultBehaviourSet = TrySetBehaviour(data, data.parameters.categorizedBehaviours);
            }

            if (data.parameters.setDefaultBehaviourPreset && !wasDefaultBehaviourSet)
            {
                if (data.parameters.defaultBehaviourPreset != null)
                {
                    wasDefaultBehaviourSet = TrySetBehaviour(data, data.parameters.defaultBehaviourPreset);
                }
                
                if (!wasDefaultBehaviourSet)
                {
                    var firstInstance = Resources.LoadAll<DefaultBehaviourPreset>("").FirstOrDefault();

                    if (firstInstance != null)
                    {
                        wasDefaultBehaviourSet = TrySetBehaviour(data, firstInstance);
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't find DefaultBehaviourPreset in Resources to apply to model " +
                                  "(Do you need to create a preset in resources?)");
                    }

                    if (wasDefaultBehaviourSet)
                    {
                        Debug.LogWarning("Couldn't find a behaviour matching model's DefaultBehaviourType in " +
                                         "DefaultBehaviourPreset to apply to model. " +
                                         "Check if scripts for all behaviour types were set.");
                    }
                }
            }

            if (data.parameters.behaviours != null)
            {
                foreach (var behaviour in data.parameters.behaviours)
                {
                    data.model.AddComponent(behaviour);
                }
            }

            data.actions.postProcessingDelegate?.Invoke(data);
        }

        private static bool TrySetBehaviour(ModelData data, Dictionary<DefaultBehaviourType, System.Type> dict)
        {
            if (!dict.TryGetValue(data.defaultBehaviourType, out var scriptType))
            {
                return false;
            }
            
            data.model.AddComponent(scriptType);
            return true;
        }
        
       
        private static bool TrySetBehaviour(ModelData data, DefaultBehaviourPreset preset)
        {
            foreach (var rule in preset.behaviourRules)
            {
                if (rule.behaviourType != data.defaultBehaviourType)
                {
                    continue;
                }
                String scriptName = rule.scriptName;
#if UNITY_EDITOR               
                //Trying to add behaviour in editor "fastest way"
                foreach (var t in TypeCache.GetTypesDerivedFrom<UnityEngine.Component>())
                {
                    if (t.Name == scriptName)
                    {
                        data.model.AddComponent(t);
                        return true;
                    }
                }
#else
                //Trying to add behaviour in run time "compatible way"
                scriptName = "AnythingWorld.Behaviour." + scriptName;
                Debug.Log("Trying to add behaviour: " + scriptName);
                var type = Type.GetType(scriptName, false, true);
                Debug.Log("Found type: " + type);
                if (type != null)
                {
                    data.model.AddComponent(type);
                    return true;
                }
#endif            
            }
            return false;
        }
    }

}
