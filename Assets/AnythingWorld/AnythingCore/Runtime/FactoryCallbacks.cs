using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using UnityEngine;

namespace AnythingWorld.Core
{
    public static class FactoryCallbacks
    {
        /// <summary>
        /// Set references to callback actions that can be called from factory.
        /// </summary>
        /// <param name="data"></param>
        public static void Subscribe(ModelData data)
        {
            if (data.requestType == RequestType.Json)
            {
                data.actions.onFailure = OnJsonLoadFailure;
                data.actions.onFailureException = OnJsonLoadFailure;
            }
            else
            {
                data.actions.onFailure = OnFailure;
                data.actions.onFailureException = OnFailure;
            }

            data.actions.onSuccessfulStage = OnSuccessfulLoadingStage;
            data.actions.onSuccess.Add(OnSuccess);
            data.actions.factoryDebug = OnFactoryDebug;
        }

        private static void OnFailure(ModelData data, Exception e, string message)
        {
            Debug.LogException(e);
            OnFailure(data,message);
        }
        
        private static void OnFailure(ModelData data, string message)
        {
            //Run the user defined actions for failure
            foreach(var action in data.actions.onFailureUserActions)
            {
                action?.Invoke();
            }
            foreach(var action in data.actions.onFailureUserParamActions){
                action?.Invoke(new CallbackInfo(data, message));
            }
            //Display log for failed object creation
            Debug.LogWarning($"Failed to make {data.searchTerm}: {message}");
            //Destroy game object
            Destroy.GameObject(data.model);
        }
        
        private static void OnJsonLoadFailure(ModelData data, Exception e, string message)
        {
            Debug.LogException(e);
            OnJsonLoadFailure(data, message);
        }
        
        private static void OnJsonLoadFailure(ModelData data, string message)
        {
            Debug.LogWarning($"Failed to make {data.searchTerm} from JSON: {message}, retrying via search");
            data.requestType = RequestType.Search;
            data.loadedData = new LoadedData();
            data.json = null;
            data.actions.onFailure = OnFailure;
            data.actions.onFailureException = OnFailure;
            data.actions.loadJsonDelegate?.Invoke(data);
        }
        
        private static void OnSuccess(ModelData data, string message = null)
        {
            foreach (var action in data.actions.onSuccessUserActions)
            {
                action?.Invoke();
            }
            foreach (var action in data.actions.onSuccessUserParamActions)
            {
                action?.Invoke(new CallbackInfo(data, message));
            }
            data.Debug(message);
            Destroy.MonoBehaviour(data.loadingScript);
        }
        
        private static void OnFactoryDebug(ModelData data, string message = null)
        {
            if (AnythingSettings.DebugEnabled)
            {
                Debug.Log($"Debug ({data?.guid}): {message}", data?.model);
            }
        }
        
        private static void OnSuccessfulLoadingStage(ModelData data, string message)
        {
            Debug.Log($"{data.guid}:{message}");
        }
    }
}
