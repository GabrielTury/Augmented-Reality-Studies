using AnythingWorld.Utilities.Data;
using UnityEngine;

namespace AnythingWorld.Animation
{
    public static class AnimationShaderLoader
    {

        public static void Load(ModelData data)
        {
            data.Debug($"Finding shader for behaviour {data.json.behaviour}");
            var shaderAnimationType = ParseAnimationType(data.json.behaviour);
            data.Debug(shaderAnimationType.ToString());
            SwitchShader(data.model, shaderAnimationType);
        }

        private static Shader ParseAnimationType(string behaviour)
        {
            switch (behaviour)
            {
                case "swim":
                case "swim3":
                case "swim2":
                    return Shader.Find("Anything World/Animation/Fish Vertical Animation");
                case "wriggle":
                    return Shader.Find("Anything World/Animation/Wriggle Animation");
                case "crawl":
                    return Shader.Find("Anything World/Animation/Crawler Animation");
                case "slither":
                    return Shader.Find("Anything World/Animation/Slither Animation");
                case "slithervertical":
                    return Shader.Find("Anything World/Animation/Slither Vertical Animation");
                default:
                    return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sRenderer"></param>
        /// <param name="sShader"></param>
        private static void SwitchShader(GameObject model, Shader inputShader)
        {
            var meshRenderer = model.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                foreach (var mat in meshRenderer.sharedMaterials)
                {
                    mat.shader = inputShader;
                }
            }

        }

        private static void SwitchShader<T>(GameObject model, Shader inputShader, ShaderEditableProperty<T> editableProperty)
        {
            var meshRenderer = model.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null) return;

            foreach (var material in meshRenderer.sharedMaterials)
            {
                material.shader = inputShader;
                switch (editableProperty.variable)
                {
                    case float f:
                        material.SetFloat(editableProperty.property, f);
                        break;
                    case int i:
                        material.SetInt(editableProperty.property, i);
                        break;
                    case Color c:
                        material.SetColor(editableProperty.property, c);
                        break;
                    default:
                        Debug.LogWarning($"Shader Property Editing of type {typeof(T).Name} is not supported");
                        break;
                }

            }
        }
        private struct ShaderEditableProperty<T>
        {
            public string property;
            public T variable;
        }
    }
}
