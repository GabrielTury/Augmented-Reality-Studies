using AnythingWorld.Utilities.Data;
using System.Collections;
#if UNITY_EDITOR
#endif
using UnityEngine.Networking;
using System;
using AnythingWorld.Utilities;

namespace AnythingWorld.Models
{
    public static class GltfRequester
    {
        public static void RequestRiggedAnimationBytes(ModelData data, Action<ModelData> onSuccess)
        {
            CoroutineExtension.StartCoroutine(RequestRiggedAnimationBytesCoroutine(data, onSuccess), data.loadingScript);
        }

        /// <summary>
        /// Fetch and load rigged animated model.
        /// </summary>
        /// <param name="data">Model data</param>
        /// <param name="sourceScript">Coroutine parent script.</param>
        /// <returns></returns>
        private static IEnumerator RequestRiggedAnimationBytesCoroutine(ModelData data, Action<ModelData> onSuccess)
        {
            if (data.model == null)
            {
                data.actions.onFailure?.Invoke(data, $"Object parent has been destroyed, returning.");
            }

            foreach (var kvp in data.json.model.rig.animations)
            {
                using var www = UnityWebRequest.Get(kvp.Value.GLB);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var fetchedBytes = www.downloadHandler.data;
                    data.loadedData.gltf.animationBytes.Add(kvp.Key,fetchedBytes);
                    data.Debug($"Successfully fetched rig bytes from {data.guid} Animation Clip:{kvp.Key} @ {kvp.Value}");
                }
                else
                {
                    data.actions.onFailure?.Invoke(data, $"Failed while loading model animation clip {kvp.Key} for model {data.guid}");
                    //Break out of enumerator, will not invoke success action.
                    yield break;
                }
            }
            onSuccess?.Invoke(data);
        }
    }
}