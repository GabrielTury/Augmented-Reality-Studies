using AnythingWorld.ObjUtility;
using AnythingWorld.Utilities.Data;
using System;
using System.IO;
using UnityEngine;

namespace AnythingWorld.Models
{
    public class ObjSingletonLoader
    {
        public static void Load(ModelData data)
        {
            RequestTextures(data);
        }
        public static void RequestTextures(ModelData data)
        {
            ObjTextureRequester.Request(data, RequestMtl);
        }
        public static void RequestMtl(ModelData data)
        {
            ObjMtlRequester.Request(data, RequestMeshes);
        }
        public static void RequestMeshes(ModelData data)
        {
            ObjBytesRequester.RequestSingleStatic(data, BuildObjSingle);
        }

        private static void BuildObjSingle(ModelData data)
        {
            foreach (var kvp in data.loadedData.obj.partsBytes)
            {
                var loader = new OBJLoader();
                Stream stream = new MemoryStream(kvp.Value);
                try
                {
                    GameObject partGameObject = loader.Load(stream, new MemoryStream(data.loadedData.obj.mtlString), data.loadedData.obj.loadedTextures);
                    partGameObject.name = kvp.Key;
                    data.loadedData.obj.loadedParts.Add(kvp.Key, partGameObject as GameObject);
                    partGameObject.transform.parent = data.model.transform;

                }
                catch (Exception e)
                {
                    data.actions?.onFailureException(data, e, "Exception generated while loading OBJ.");
                    return;
                }

            }
            ModelScaling.Scale(data, data.actions.loadAnimationDelegate);
        }
    }
}
