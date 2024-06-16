using AnythingWorld.ObjUtility;
using AnythingWorld.Utilities.Data;
using System.IO;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class ObjPartsLoader
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
            switch (data.modelLoadingPipeline)
            {
                case Utilities.ModelLoadingPipeline.Unset:
                    break;
                case Utilities.ModelLoadingPipeline.RiggedGLB:
                    break;
                case Utilities.ModelLoadingPipeline.GLTF:
                    break;
                case Utilities.ModelLoadingPipeline.OBJ_Static:
                    ObjBytesRequester.RequestSingleStatic(data, BuildObjParts);
                    break;
                case Utilities.ModelLoadingPipeline.OBJ_Part_Based:
                    ObjBytesRequester.RequestParts(data, BuildObjParts);
                    break;
            }

        }
        private static void BuildObjParts(ModelData data)
        {
            foreach (var kvp in data.loadedData.obj.partsBytes)
            {
                var loader = new OBJLoader();
                Stream stream = new MemoryStream(kvp.Value);

                GameObject partGameObject = loader.Load(stream, new MemoryStream(data.loadedData.obj.mtlString), data.loadedData.obj.loadedTextures);
                partGameObject.name = kvp.Key;
                Debug.Log($"{partGameObject.name} | {partGameObject.transform.position}");

                //UnityEngine.Debug.Log($"Adding {kvp.Key},{kvp.Value} to parts dict");

                data.loadedData.obj.loadedParts.Add(kvp.Key, partGameObject as GameObject);
                partGameObject.transform.parent = data.model.transform;
            }
            ModelScaling.Scale(data, data.actions.loadAnimationDelegate);
        }
    }
}
