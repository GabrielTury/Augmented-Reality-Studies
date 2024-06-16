using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections;
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Models
{
    public static class ObjBytesRequester
    {
        public static void RequestParts(ModelData data, Action<ModelData> onSuccess)
        {
            CoroutineExtension.StartCoroutine(RequestPartsCoroutine(data, onSuccess), data.loadingScript);
        }

        public static void RequestSingleStatic(ModelData data, Action<ModelData> onSuccess)
        {
            CoroutineExtension.StartCoroutine(RequestSingleStaticCoroutine(data, onSuccess), data.loadingScript);
        }
        private static IEnumerator RequestPartsCoroutine(ModelData data, Action<ModelData> onSuccess)
        {
            foreach (var kvp in data.json.model.parts)
            {
                using var www = UnityWebRequest.Get(kvp.Value);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var fetchedBytes = www.downloadHandler.data;
                    data.loadedData.obj.partsBytes.Add(kvp.Key, fetchedBytes);
                }
                else
                {
                    Debug.Log($"Could not fetch part bytes from {data.guid}:{kvp.Key} @ {kvp.Value}");
                    data.actions.onFailure?.Invoke(data, $"Failed while loading model part \"{kvp.Key}\" for model \"{data.json.name}\"");
                    yield break;
                }
            }
            onSuccess?.Invoke(data);
        }

        private static IEnumerator RequestSingleStaticCoroutine(ModelData data, Action<ModelData> onSuccess)
        {
            var uri = data.json.model.other.model;

            using (var www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var fetchedBytes = www.downloadHandler.data;
                    data.loadedData.obj.partsBytes.Add("model", fetchedBytes);
                }
                else
                {
                    data.actions.onFailure?.Invoke(data, $"Failed while loading model part \"model \" for model \"{data.json.name}\"");
                    yield break;
                }
            }
            
            onSuccess?.Invoke(data);
        }
    }
}
