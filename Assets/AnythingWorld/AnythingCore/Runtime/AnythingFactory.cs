using AnythingWorld.Animation;
using AnythingWorld.Behaviour;
using AnythingWorld.Models;
using AnythingWorld.Networking;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using UnityEngine;

namespace AnythingWorld.Core
{
    public static class AnythingFactory
    {
        /// <summary>
        /// Request model that matches search term.
        /// </summary>
        /// <param name="searchTerm">Search term to find closest match to.</param>
        /// <returns></returns>
        public static GameObject RequestModel(string searchTerm, RequestParamObject userParams)
        {
            if (!UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline)
            {
                Debug.LogWarning("Warning: Standard RP detected, HDRP or URP must be installed to use Anything World.");
                return null;
            }

            var data = ConstructModelDataContainer(searchTerm, userParams);
            var anchorGameObject = CreateModelGameObject(data);
            FactoryCallbacks.Subscribe(data);
            UserCallbacks.Subscribe(data);
            BeginModelRequest(data);
            return anchorGameObject;
        }

        /// <summary>
        /// Request model using pre-fetched JSON.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="userParams"></param>
        /// <returns></returns>
        public static GameObject RequestModel(ModelJson json, RequestParamObject userParams)
        {
            if (!UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline)
            {
                Debug.LogWarning("Warning: Standard RP detected, HDRP or URP must be installed to use Anything World.");
                return null;
            }

            var data = ConstructModelDataContainer(json, userParams);
            var anchorGameObject = CreateModelGameObject(data);
            FactoryCallbacks.Subscribe(data);
            UserCallbacks.Subscribe(data);
            BeginModelRequest(data);
            return anchorGameObject;
        }

        /// <summary>
        /// Constructs model data container, sets search term.
        /// </summary>
        /// <param name="searchTerm">Name of search tem linked to this ModelData container.</param>
        /// <returns></returns>
        private static ModelData ConstructModelDataContainer(string searchTerm, RequestParamObject userParams)
        {
            var data = new ModelData
            {
                searchTerm = searchTerm,
                requestType = RequestType.Search,
                parameters = userParams ?? new RequestParamObject()
            };

            return data;
        }

        private static ModelData ConstructModelDataContainer(ModelJson json, RequestParamObject userParams)
        {
            var data = new ModelData
            {
                searchTerm = json.name,
                json = json,
                requestType = RequestType.Json,
                parameters = userParams ?? new RequestParamObject()
            };

            return data;
        }

        /// <summary>
        /// Create "anchor" game object returned to user immediately,
        /// all components and meshes will be added onto or as a child of this GameObject.
        /// </summary>
        /// <param name="data">Model data that will be linked to this GameObject.</param>
        /// <returns>Anchor GameObject</returns>
        private static GameObject CreateModelGameObject(ModelData data)
        {
            data.model = new GameObject(data.searchTerm);
            data.loadingScript = data.model.AddComponent<LoadingObject>();
            return data.model;
        }

        /// <summary>
        /// Sets up request pipeline depending on type of request and invokes start process.
        /// </summary>
        /// <param name="data">Model data for this request.</param>
        private static void BeginModelRequest(ModelData data)
        {
            switch (data.requestType)
            {
                case RequestType.Search:
                    SetupRequestFromSearchPipeline(data);
                    break;

                case RequestType.Json:
                    SetupRequestFromJsonPipeline(data);
                    break;
            }
            data.actions.startPipeline?.Invoke(data);
        }

        // Assign methods to the loading pipeline delegates within ModelData
        private static void SetupRequestFromSearchPipeline(ModelData data)
        {
            // Fetch json data
            data.actions.startPipeline = JsonRequester.FetchJson;
            SetSharedRequestDelegates(data);
        }

        private static void SetupRequestFromJsonPipeline(ModelData data)
        {
            // Skip fetching json and go straight to processing data
            data.actions.startPipeline = JsonProcessor.ProcessData;
            SetSharedRequestDelegates(data);
        }

        private static void SetSharedRequestDelegates(ModelData data)
        {
            // Action to load JSON
            data.actions.loadJsonDelegate = JsonRequester.FetchJson;
            // Do some post processing on JSON to calculate secondary variables,
            // (animation pipeline, model loading pipeline, inspector script) 
            data.actions.processJsonDelegate = JsonProcessor.ProcessData;
            
            // Action to load Model + extract animations if needed
            data.actions.loadModelDelegate = ModelLoader.Load;

            data.actions.loadAnimationDelegate = AnimationFactory.Load;

            data.actions.addBehavioursDelegate = BehaviourHandler.AddBehaviours;

            data.actions.postProcessingDelegate = ModelPostProcessing.FinishMakeProcess;
        }
        /// <summary>
        /// Request all models that was processed by the user.
        /// </summary>
        public static GameObject RequestProcessedModel(ModelJson datain, RequestParamObject requestParams)
        {
            if (!UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline)
            {
                Debug.LogWarning("Warning: Standard RP detected, HDRP or URP must be installed to use Anything World.");
                return null;
            }
            Debug.Log("Requesting processed model");
            var data = ConstructModelDataContainer(datain, requestParams);
            var anchorGameObject = CreateModelGameObject(data);
            FactoryCallbacks.Subscribe(data);
            UserCallbacks.Subscribe(data);
            data.actions.startPipeline = JsonRequester.FetchProcessedJson;
            SetSharedRequestDelegates(data);
            data.actions.startPipeline?.Invoke(data);
            return anchorGameObject;
        }
    }
}