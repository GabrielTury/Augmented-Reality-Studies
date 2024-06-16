using UnityEngine;

namespace AnythingWorld.Core
{
    using AnythingWorld.Networking;
    using AnythingWorld.Utilities.Data;
    using System;
    using System.Collections.Generic;

    public class JsonRequestTest : MonoBehaviour
    {
        public string requestObject = "cat#0000";
        
        public void RequestJsonWithCallback(Action<ModelJson> callback)
        {
            JsonRequester.FetchJson(requestObject, callback, this);
        }
        
        public void IncorrectAnimationUrls(ModelJson json)
        {
            if (json?.model?.rig?.animations != null)
            {
                foreach (var kvp in json.model.rig.animations)
                {
                    kvp.Value.GLB = $"ERASED GLB RIG URL FOR {kvp.Key}";
                }
            }
           
            AnythingFactory.RequestModel(json, null);
        }
        
        public void IncorrectObjTextureUrls(ModelJson json)
        {
            List<string> modifiedTextureList = new List<string>();
            foreach(var url in json.model.other.texture)
            {
                modifiedTextureList.Add("ERASED TEXTURE TEST URL");
            }
            json.model.other.texture = modifiedTextureList.ToArray();
            AnythingFactory.RequestModel(json, null);
        }
        
        public void IncorrectMtlUrl(ModelJson json)
        {
            json.model.other.material = "ERASED MTL TEST URL";
            AnythingFactory.RequestModel(json, null);
        }
        
        public void IncorrectPartUrl(ModelJson json)
        {
            Dictionary<string, string> modifiedDictionary = new Dictionary<string, string>();
            foreach (var url in json.model.parts)
            {
                modifiedDictionary.Add(url.Key, "ERASED OBJ PART URL");
            }
            json.model.parts = modifiedDictionary;
            AnythingFactory.RequestModel(json, null);
        }
    }
}
