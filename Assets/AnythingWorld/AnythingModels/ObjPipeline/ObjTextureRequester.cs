using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using AnythingWorld.Utilities.Data;
using AnythingWorld.Utilities;

namespace AnythingWorld.Models
{
    public static class ObjTextureRequester
    {
        public static void Request(ModelData data, Action<ModelData> onSuccess)
        {
            CoroutineExtension.StartCoroutine(RequestCoroutine(data, onSuccess), data.loadingScript);
        }
       
        /// <summary>
        /// Iterates through the Json.Model.Other.Textures array and loads them to a list.
        /// </summary>
        /// <param name="data">Model request data.</param>
        public static IEnumerator RequestCoroutine(ModelData data, Action<ModelData> onSuccess)
        {
            if(data.json.model.other.texture == null)
            {
                data.actions.onFailure(data, $"No textures found for {data.json.name}");
                yield break;
            }
            foreach (var textureUrl in data.json.model.other.texture)
            {
                
                using var www = UnityWebRequestTexture.GetTexture(textureUrl);
                www.timeout = 40;
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture requestedTexture = DownloadHandlerTexture.GetContent(www);
                    var textureName = System.Uri.UnescapeDataString(ParseTextureNameFromUrl(textureUrl));
                    data.loadedData.obj.loadedTextures.Add(textureName, requestedTexture);
                }
                else
                {
                    data.actions.onFailure(data, $"Failed to load texture for {data.json.name}: {textureUrl}");
                    yield break;
                }
            }
            onSuccess?.Invoke(data);
            yield return null;
        }

        /// <summary>
        /// Parses the name of a texture from the textures URL.
        /// </summary>
        /// <param name="textureAddress">URL of the texture to parse.</param>
        /// <returns>Returns parsed texture name (including file format) eg. basecolor.png</returns>
        private static string ParseTextureNameFromUrl(string textureAddress)
        {
            try
            {
                if (textureAddress.Contains("?"))
                {
                    var splitAddress = textureAddress.Split('?');
                    var addressWithTexName = splitAddress[0];
                    var splitAddressWithTexName = addressWithTexName.Split('/');
                    var textureName = splitAddressWithTexName[splitAddressWithTexName.Length - 1];
                    return textureName;
                }
                else
                {
                    Debug.LogError($"No name parameter detected in texture URL: {textureAddress}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: trying to load texture name from texture address:{textureAddress}");
                Debug.LogException(e);
                return null;
            }
        }
    }
}
