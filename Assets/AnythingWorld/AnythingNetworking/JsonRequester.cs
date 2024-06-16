using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using AnythingWorld.Utilities.Networking;
using System;

namespace AnythingWorld.Networking
{
    public class JsonRequester
    {
        /// <summary>
        /// Launch fetch json coroutine.
        /// </summary>
        /// <param name="data">Model data object.</param>
        /// <param name="sourceScript">Coroutine parent script.</param>
        public static void FetchJson(ModelData data)
        {
            CoroutineExtension.StartCoroutine(FetchJsonCoroutine(data), data.loadingScript);

        }
        /// <summary>
        /// Launch fetch json coroutine.
        /// </summary>
        /// <param name="data">Model data object.</param>
        /// <param name="sourceScript">Coroutine parent script.</param>
        public static void FetchProcessedJson(ModelData data)
        {
            CoroutineExtension.StartCoroutine(FetchProcessedJsonCoroutine(data), data.loadingScript);

        }

        public static void FetchJson(string searchTerm, Action<ModelJson> callback, MonoBehaviour parentScript)
        {
            CoroutineExtension.StartCoroutine(FetchJsonCoroutine(searchTerm, callback), parentScript);
        }

        /// <summary>
        /// Request Json data for model and populate data container, on success invoke loading rig delegate.
        /// </summary>
        /// <param name="data">Model data object</param>
        /// <param name="sourceScript">Coroutine parent script</param>
        /// <returns></returns>
        private static IEnumerator FetchJsonCoroutine(ModelData data)
        {
            if (AnythingSettings.APIKey == "")
            {
                data.actions.onFailure?.Invoke(data, $"No API key when attempting to fetch model data for {data.searchTerm}, returning.");
            }

            if (data.model == null)
            {
                data.actions.onFailure?.Invoke(data, $"Object parent has been destroyed, returning.");
            }

            var uri = NetworkConfig.GetNameEndpointUri(data.searchTerm);
            data.Debug("Requesting json from " + uri);
            using (var www = UnityWebRequest.Get(uri))
            {
                www.timeout = 30;
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    data.actions.onFailure?.Invoke(data, $"Error fetching model data for {data.searchTerm}, returning.");
                    var error = new NetworkErrorMessage(www);
                    NetworkErrorHandler.HandleError(error);
                    yield break;
                }

                data.json = DeserializeStringJson(www.downloadHandler.text);
            }

            data.actions.processJsonDelegate?.Invoke(data);
        }

        /// <summary>
        /// Request JSON data for model and populate data container, on success invoke loading rig delegate.
        /// </summary>
        /// <param name="data">Model data object</param>
        /// <param name="sourceScript">Coroutine parent script</param>
        /// <returns></returns>
        private static IEnumerator FetchProcessedJsonCoroutine(ModelData data)
        {
            if (AnythingSettings.APIKey == "")
            {
                data.actions.onFailure?.Invoke(data, $"No API key when attempting to fetch model data for {data.searchTerm}, returning.");
            }
            var uri = NetworkConfig.GetUserProcessed(data.json._id);
            data.Debug("Requesting json from " + uri);
            using (var www = UnityWebRequest.Get(uri))
            {
                www.timeout = 10;
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    data.actions.onFailure?.Invoke(data, $"Error fetching model data for {data.searchTerm}, returning.");
                    var error = new NetworkErrorMessage(www);
                    NetworkErrorHandler.HandleError(error);
                    yield break;
                }

                data.json = DeserializeStringJson(www.downloadHandler.text);
            }

            data.actions.processJsonDelegate?.Invoke(data);
        }
        /// <summary>
        /// Request Json data for model and populate data container, on success invoke custom callback accepting ModelData.
        /// </summary>
        /// <param name="data">Model data object</param>
        /// <param name="callback">Callback called ons succesful request.</param>
        /// <returns></returns>
        public static IEnumerator FetchJsonCoroutine(ModelData data, Action<ModelData> callback)
        {
            var uri = NetworkConfig.GetNameEndpointUri(data.searchTerm);
            data.Debug("Requesting json from " + uri);
            using (var www = UnityWebRequest.Get(uri))
            {
                www.timeout = 10;
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    data.actions.onFailure?.Invoke(data,
                        $"Error fetching model data for {data.searchTerm}, returning.");
                    yield break;
                }

                data.json = DeserializeStringJson(www.downloadHandler.text);
            }

            callback?.Invoke(data);
        }

        /// <summary>
        /// Request json matching request term, on success invoke custom callback accepting ModelJson.
        /// </summary>
        /// <param name="requestTerm">Term to search for.</param>
        /// <param name="callback">Action that will be called on successful completion, requires ModelJson param</param>
        /// <returns></returns>
        public static IEnumerator FetchJsonCoroutine(string requestTerm, Action<ModelJson> callback)
        {
            var uri = NetworkConfig.GetNameEndpointUri(requestTerm);
            //data.Debug("Requesting json from " + uri);
            Debug.Log(uri);

            using var www = UnityWebRequest.Get(uri);
            www.timeout = 10;
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Error fetching model data for {requestTerm}, returning.");
                yield break;
            }

            ModelJson modelJson = DeserializeStringJson(www.downloadHandler.text);
            callback?.Invoke(modelJson);
        }

        /// <summary>
        /// Deserialize Json string into ModelJson data container.
        /// </summary>
        /// <param name="www">Web </param>
        /// <returns></returns>
        private static ModelJson DeserializeStringJson(string stringJson)
        {
            string objectJsonString = TrimJson(stringJson);
            var modelJson = Newtonsoft.Json.JsonConvert.DeserializeObject<ModelJson>(objectJsonString);
            return modelJson;
        }

        /// <summary>
        /// Trim JSON of array brackets.
        /// </summary>
        private static string TrimJson(string result)
        {
            result = result.TrimStart('[');
            result = result.TrimEnd(']');
            return result;
        }


    }
}
