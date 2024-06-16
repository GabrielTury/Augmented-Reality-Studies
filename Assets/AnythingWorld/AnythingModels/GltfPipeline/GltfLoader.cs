using AnythingWorld.GLTFUtility;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System.Linq;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class GltfLoader
    {
        public static void Load(ModelData data)
        {
            RequestModel(data);
        }
        public static void RequestModel(ModelData data)
        {
            GltfRequester.RequestRiggedAnimationBytes(data, LoadAnimatedGlb);
        }
        /// <summary>
        /// Request bytes and animation clips and assign to variables in LModelData.LoadedData
        /// </summary>
        /// <param name="data">Request model data</param>
        private static void LoadAnimatedGlb(ModelData data)
        {
            //Load default animation state
            var animationBytesDict = data?.loadedData?.gltf?.animationBytes;
            if (animationBytesDict == null || animationBytesDict.Count == 0)
            {
                data.actions.onFailure?.Invoke(data, "Failed to load animation dictionary.");
                return;
            }

            if (data.model == null)
            {
                data.actions.onFailure?.Invoke(data, $"Object parent has been destroyed, returning.");
                return;
            }
            //If default state not found, use first state
            string defaultState = "idle";
            if (!data.loadedData.gltf.animationBytes.ContainsKey(defaultState))
            {
                defaultState = data.loadedData.gltf.animationBytes.ToArray()[0].Key;
            }

            //load legacy
            foreach (var kvp in data.loadedData.gltf.animationBytes)
            {
                //If default state don't load (loaded above), else load animation into model data.
                if (kvp.Key == defaultState)
                {
                    var createdObject = LoadGlbAndAnimationLegacy(data, defaultState);
                    createdObject.transform.parent = data.model.transform;
                    data.rig = createdObject;
                }
                else
                {
                    LoadGlbAnimationOnlyLegacy(data, kvp.Key);
                }
            }
            //load normal
            foreach(var kvp in data.loadedData.gltf.animationBytes)
            {
                LoadGlbAnimationOnly(data, kvp.Key);
            }
            
            ModelScaling.Scale(data, data.actions.loadAnimationDelegate);
        }

        private static void LoadGlbAnimationOnly(ModelData data, string key)
        {
            ImportSettings setting = new ImportSettings();
            setting.animationSettings.interpolationMode = InterpolationMode.LINEAR;
            setting.animationSettings.useLegacyClips = false;
            setting.animationSettings.looping = true;
            var loadedGlb = Importer.LoadFromBytes(data.loadedData.gltf.animationBytes[key], out var clips, setting);

            foreach (var (clip, index) in clips.WithIndex())
            {
                clip.legacy = false;
                clip.wrapMode = WrapMode.Loop;
                var clipName = key;
                clip.EnsureQuaternionContinuity();
                if (index != 0) clipName += index.ToString();
                data.loadedData.gltf.animationClips.Add(clipName, clip);
            }
            Utilities.Destroy.GameObject(loadedGlb);
        }
        /// <summary>
        /// Load model into game object and and animations into data container, return game object.
        /// </summary>
        /// <param name="data">Model request data.</param>
        /// <param name="key">Animation name</param>
        /// <returns></returns>
        private static GameObject LoadGlbAndAnimationLegacy(ModelData data, string key)
        {
            ImportSettings setting = new ImportSettings();
            setting.animationSettings.interpolationMode = InterpolationMode.LINEAR;
            setting.animationSettings.useLegacyClips = true;
            setting.animationSettings.looping = true;
            var loadedGlb = Importer.LoadFromBytes(data.loadedData.gltf.animationBytes[key], out var clips, setting);
            foreach (var (clip, index) in clips.WithIndex())
            {
                clip.wrapMode = WrapMode.Loop;
                var clipName = key;
                clip.EnsureQuaternionContinuity();
                if (index != 0) clipName += index.ToString();
                data.loadedData.gltf.animationClipsLegacy.Add(clipName, clip);
            }
            return loadedGlb;
        }


        /// <summary>
        /// Load animation clips into model data and delete glb game object after.
        /// </summary>
        /// <param name="data">Model request data.</param>
        /// <param name="key">Animation name.</param>
        private static void LoadGlbAnimationOnlyLegacy(ModelData data, string key)
        {
            ImportSettings setting = new ImportSettings();
            setting.animationSettings.interpolationMode = InterpolationMode.LINEAR;
            setting.animationSettings.useLegacyClips = true;
            setting.animationSettings.looping = true;
            var loadedGlb = Importer.LoadFromBytes(data.loadedData.gltf.animationBytes[key], out var clips, setting);
            foreach (var (clip, index) in clips.WithIndex())
            {
                clip.wrapMode = WrapMode.Loop;
                var clipName = key;
                clip.EnsureQuaternionContinuity();
                if (index != 0) clipName += index.ToString();
                data.loadedData.gltf.animationClipsLegacy.Add(clipName, clip);
            }
            Utilities.Destroy.GameObject(loadedGlb);
        }
    }
}
