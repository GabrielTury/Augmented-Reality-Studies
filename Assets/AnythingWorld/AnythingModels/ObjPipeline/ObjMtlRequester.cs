using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections;
#if UNITY_EDITOR
#endif
using UnityEngine.Networking;

namespace AnythingWorld.Models
{
    public static class ObjMtlRequester
    {
        public static void Request(ModelData data, Action<ModelData> onSuccess)
        {
            CoroutineExtension.StartCoroutine(RequestCoroutine(data, onSuccess), data.loadingScript);
        }
        private static IEnumerator RequestCoroutine(ModelData data, Action<ModelData> onSuccess)
        {
            var mtlUrl = data.json.model.other.material;
            
            using var www = UnityWebRequest.Get(mtlUrl);
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                data.loadedData.obj.mtlString = www.downloadHandler.data;
                onSuccess?.Invoke(data);
            }
            else
            {
                data.actions.onFailure(data, $"Failed to load mtl data for {data.json.name}: {mtlUrl}");
            }
        }
    }
}
