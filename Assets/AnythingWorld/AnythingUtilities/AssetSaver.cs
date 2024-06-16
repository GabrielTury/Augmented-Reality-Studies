using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AnythingWorld.Utilities
{
    public static class AssetSaver
    {
        public static string rootPath = "Assets/SavedAssets";

        public static void CreateAssetFromData(CallbackInfo callbackData)
        {
#if UNITY_EDITOR
            CreateAssetFromGameObject(callbackData.linkedObject, callbackData);
#endif
        }

#if UNITY_EDITOR
        public static void CreateAssetFromGameObject(GameObject streamedObject, CallbackInfo callbackData)
        {
            CreateDefaultFolder();
            CreateFolder(rootPath, streamedObject.name);

            if (!File.Exists(@$"{Application.dataPath}/SavedAssets/{streamedObject.name}/.flag"))
            {
                SerializeAnimator(streamedObject);
                SerializeSkinnedMeshRenderers(streamedObject);
                SerializeMeshRenderers(streamedObject);

                PrefabUtility.SaveAsPrefabAssetAndConnect(streamedObject, $"{rootPath}/{streamedObject.name}/{streamedObject.name}.prefab", InteractionMode.AutomatedAction);
                File.Create(@$"{Application.dataPath}/SavedAssets/{streamedObject.name}/.flag");
                Debug.Log($"Saved asset to {rootPath}/{streamedObject.name}");
            }
            else
            {
                Debug.LogWarning($"Asset has already been serialized!");
            }
        }

        private static void SerializeAnimator(GameObject streamedObject)
        {
            if(streamedObject.GetComponentInChildren<Animator>())
            {
                var controller = streamedObject.GetComponentInChildren<Animator>().runtimeAnimatorController;
                TryCreateAsset<RuntimeAnimatorController>(controller, streamedObject.name, streamedObject.name, "Animation Clips", out var path, out var serializedAnimator);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static void SerializeAnimationClips(Dictionary<string, AnimationClip> animationClips, string name)
        {
            Debug.Log("Serializing modern animation clips");

            List<string> paths = new List<string>();
            foreach (var kvp in animationClips)
            {
                if (kvp.Value == null) continue;
                //kvp.Value.legacy = true;
                TryCreateAsset<AnimationClip>(kvp.Value, kvp.Key, name, "Animation Clips", out var path, out var serializedAnimationClip);
                if (path != null) paths.Add(path);
            }
           // EditorUtility.SetDirty(animationComponent);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SerializeLegacyAnimations(GameObject streamedObject, CallbackInfo callbackData)
        {
            Debug.Log("Serializing legacy animations");
            if (streamedObject.GetComponentInChildren<Animation>())
            {
                var animationComponent = streamedObject.GetComponentInChildren<Animation>();
                var animationContainer = animationComponent.gameObject;
                GameObject.DestroyImmediate(animationComponent);

                List<string> paths = new List<string>();
                foreach (var kvp in callbackData.data.loadedData.gltf.animationClipsLegacy)
                {
                    if (kvp.Value == null) continue;
                    kvp.Value.legacy = true;
                    TryCreateAsset<AnimationClip>(kvp.Value, kvp.Key, streamedObject.name, "Legacy Animations", out var path, out var serializedAnimationClip);
                    if (path != null) paths.Add(path);
                }

                animationComponent = animationContainer.AddComponent<Animation>();
                foreach (string path in paths)
                {
                    var clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
                    animationComponent.AddClip(clip, clip.name);
                }

                EditorUtility.SetDirty(animationComponent);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void SerializeMeshRenderers(GameObject streamedObject)
        {
            if (streamedObject.GetComponentsInChildren<MeshRenderer>()?.Length > 0)
            {
                foreach (var meshRenderer in streamedObject.GetComponentsInChildren<MeshRenderer>())
                {
                    var gameObject = meshRenderer.gameObject;

                    List<Material> serializedSharedMaterials = new List<Material>();
                    foreach (Material mat in meshRenderer.sharedMaterials)
                    {
                        SerializeMaterialTextures(streamedObject, mat);
                        TryCreateAsset<Material>(mat, mat.name, streamedObject.name, "Materials", out var materialPath, out var material, false);
                        serializedSharedMaterials.Add(material);
                    }
                    meshRenderer.sharedMaterials = serializedSharedMaterials.ToArray();

                    if (meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
                    {
                        var mesh = meshFilter.sharedMesh;
                        TryCreateAsset<Mesh>(mesh, mesh.name, streamedObject.name, "Meshes", out var meshPath, out var serializedMesh);
                        meshFilter.sharedMesh = serializedMesh;
                    }
                    else
                    {
                        Debug.LogWarning("Could not find mesh filter for mesh renderer:", meshRenderer.gameObject);
                    }
                }

            }

        }

        private static void SerializeSkinnedMeshRenderers(GameObject streamedObject)
        {
            if (streamedObject.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                var smRenderer = streamedObject.GetComponentInChildren<SkinnedMeshRenderer>();
                List<Material> serializedSharedMaterials = new List<Material>();
                foreach (Material mat in smRenderer.sharedMaterials)
                {
                    SerializeMaterialTextures(streamedObject, mat);
                    TryCreateAsset<Material>(mat, mat.name, streamedObject.name, "Materials", out var path, out var serializedMaterial, false);
                    serializedSharedMaterials.Add(serializedMaterial);
                }

                smRenderer.sharedMaterials = serializedSharedMaterials.ToArray();

                if (streamedObject.GetComponentInChildren<SkinnedMeshRenderer>() && smRenderer.sharedMesh != null)
                {
                    var mesh = streamedObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                    TryCreateAsset<Mesh>(mesh, mesh.name, streamedObject.name, "Meshes", out var path, out var serializedMesh);
                    streamedObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = serializedMesh;
                }
            }
        }

        private static void SerializeMaterialTextures(GameObject streamedObject, Material mat)
        {
            List<Tuple<Texture, string>> allTexture = GetTextures(mat);
            for (int i = 0; i < allTexture.Count; i++)
            {
                if (allTexture[i].Item1 == null) continue;
                TryCreateAsset<Texture>(allTexture[i].Item1, allTexture[i].Item2, streamedObject.name, "Textures", out var texturePath, out var texture);
                mat.SetTexture(allTexture[i].Item2, texture);
            }
        }

        private static List<Tuple<Texture, string>> GetTextures(Material mat)
        {
            List<Tuple<Texture, string>> allTexture = new List<Tuple<Texture, string>>();
            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    var textureName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = mat.GetTexture(textureName);

                    allTexture.Add(new Tuple<Texture, string>(texture, textureName));
                }
            }
            return allTexture;
        }

        private static bool TryCreateAsset<T>(UnityEngine.Object asset, string name, string guid, string subFolder, out string path, out T loadedAsset, bool allowDuplicate = true) where T : UnityEngine.Object
        {
            path = "";
            loadedAsset = null;

            if (asset == null || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(subFolder))
            {
                Debug.LogError("Invalid input parameters for asset creation.");
                return false;
            }

            try
            {
                CreateDefaultFolder();
                string guidFolder = CreateFolder(rootPath, guid);
                string subFolderPath = CreateFolder(guidFolder, subFolder);
                string originalAssetPath = BuildAssetPath<T>(guid, subFolder, name, allowDuplicate);

                if (!IsAssetCreationAllowed<T>(originalAssetPath, asset, allowDuplicate, out path))
                {
                    loadedAsset = LoadAssetAtPath<T>(path);
                    return true;
                }

                if (CheckIfAssetExists(originalAssetPath))
                {
                    path = originalAssetPath;
                    loadedAsset = LoadAssetAtPath<T>(path);
                    return true;
                }

                string assetPath = CreateUniqueAssetPath(originalAssetPath);
                CreateAndSaveAsset(asset, assetPath);
                path = assetPath;
                loadedAsset = LoadAssetAtPath<T>(path);

                return true;
            }   
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static string BuildAssetPath<T>(string guid, string subFolder, string name, bool uniquePaths)
        {
            string extension = typeof(T) == typeof(AnimationClip) ? ".anim" : ".asset";
            string safeFilterName = GenerateSafeFilePath(name);
            string path = $"{rootPath}/{guid}/{subFolder}/{safeFilterName}{extension}";
            if(uniquePaths)
            {
                return AssetDatabase.GenerateUniqueAssetPath(path);
            }
            return path;
        }

        private static bool IsAssetCreationAllowed<T>(string originalAssetPath, UnityEngine.Object asset, bool allowDuplicate, out string existingPath) where T : UnityEngine.Object
        {
            existingPath = "";
            var isExistingAsset = CheckIfAssetExists(originalAssetPath);
            if (AssetDatabase.Contains(asset) || (!allowDuplicate && isExistingAsset))
            {
                existingPath = AssetDatabase.Contains(asset) ? AssetDatabase.GetAssetPath(asset) : originalAssetPath;
                Debug.Log($"{asset} already serialized within database.");
                return false;
            }

            return true;
        }

        private static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void CreateAndSaveAsset(UnityEngine.Object asset, string assetPath)
        {
            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
        }

        private static string CreateUniqueAssetPath(string originalAssetPath)
        {
#if UNITY_2021_1_OR_NEWER
            return AssetDatabase.GenerateUniqueAssetPath(originalAssetPath);
#else
    return originalAssetPath;
#endif
        }

        private static bool CheckIfAssetExists(string assetPath)
        {
#if UNITY_2021_1_OR_NEWER
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath, AssetPathToGUIDOptions.OnlyExistingAssets));
#else
    return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null;
#endif
        }

        private static string CreateDefaultFolder()
        {
            if (AssetDatabase.IsValidFolder(rootPath)) return AssetDatabase.AssetPathToGUID(rootPath);
            else return CreateFolder("Assets", "SavedAssets");
        }

        static string CreateFolder(string rootDirectory, string folderName)
        {
            string newDirectory = rootDirectory + "/" + folderName;
            if (AssetDatabase.IsValidFolder(newDirectory)) return newDirectory;
            string guid = AssetDatabase.CreateFolder(rootDirectory, folderName);
            string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);

            return newDirectory;
        }

        private static string GenerateSafeFilePath(string inputPath)
        {
            string illegalChars = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(illegalChars)));
            var safePath = r.Replace(inputPath, "");
            return safePath;
        }

        private static bool TryGetPath<T>(T asset, string name, string guid, out string path) where T : UnityEngine.Object
        {
            //CreateDefaultFolder();
            //CreateFolder(rootPath, guid);

            var safeFilterName = GenerateSafeFilePath(name);
            string extension = ".asset";
            if (typeof(T) == typeof(AnimationClip)) extension = ".anim";
            var assetPath = $"{rootPath}/{guid}/{safeFilterName}{extension}";

            if (AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)))
            {
                path = AssetDatabase.GetAssetPath(asset);
                return true;
            }
            else
            {
                path = null;
                return false;
            }

        }


#endif
    }
}


