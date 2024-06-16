using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System.Collections;

namespace AnythingWorld.Models
{
    /// <summary>
    /// Provides a set of static methods for loading models.
    /// </summary>
    public static class ModelLoader
    {
        /// <summary>
        /// Loads a model using the specified <paramref name="data"/> object.
        /// </summary>
        /// <param name="data">The model data to load.</param>
        public static void Load(ModelData data)
        {
            CoroutineExtension.StartCoroutine(StartModelLoadingPipeline(data), data.loadingScript);
        }

        /// <summary>
        /// Starts the model loading pipeline using the specified <paramref name="data"/> object.
        /// </summary>
        /// <param name="data">The model data to load.</param>
        private static IEnumerator StartModelLoadingPipeline(ModelData data)
        {
            if (data.model == null)
            {
                data.actions.onFailure?.Invoke(data, $"Object parent has been destroyed, returning.");
            }

            data.Debug($"Loading model with {data.modelLoadingPipeline} pipeline.");

            // Switch statement to determine which pipeline to use based on the specified ModelLoadingPipeline value
            switch (data.modelLoadingPipeline)
            {
                // Use the RiggedGLB pipeline to load the model
                case Utilities.ModelLoadingPipeline.RiggedGLB:
                    GltfLoader.Load(data);
                    break;

                // Use the GLTF pipeline to load the model
                case Utilities.ModelLoadingPipeline.GLTF:
                    // TODO: Invoke the loadAnimationDelegate action, if it is not null
                    break;

                // Use the OBJ_Static pipeline to load the model
                case Utilities.ModelLoadingPipeline.OBJ_Static:
                    ObjSingletonLoader.Load(data);
                    break;

                // Use the OBJ_Part_Based pipeline to load the model
                case Utilities.ModelLoadingPipeline.OBJ_Part_Based:
                    ObjPartsLoader.Load(data);
                    break;
            }

            // Return null to indicate that the coroutine has completed
            yield return null;
        }
    }
}