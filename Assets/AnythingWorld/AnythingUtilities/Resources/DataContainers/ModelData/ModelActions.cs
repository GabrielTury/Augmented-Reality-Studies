using System;
using System.Collections.Generic;
using System.Linq;

namespace AnythingWorld.Utilities.Data
{
    [Serializable]
    public class ModelActions
    {
        public Action<ModelData> startPipeline;
        public Action<ModelData> loadJsonDelegate;
        public Action<ModelData> processJsonDelegate;
        public Action<ModelData> loadModelDelegate;
        public Action<ModelData> loadAnimationDelegate;
        public Action<ModelData> addBehavioursDelegate;
        public Action<ModelData> postProcessingDelegate;
        public Action<ModelData, string> factoryDebug;

        public Action<ModelData, string> onSuccessfulStage;
        public List<Action<ModelData, string>> onSuccess = new List<Action<ModelData, string>>();
        public Action<ModelData, string> onFailure;
        public Action<ModelData, Exception, string> onFailureException;

        public List<Action> onSuccessUserActions = new List<Action>();
        public List<Action> onFailureUserActions = new List<Action>();


        public List<Action<CallbackInfo>> onSuccessUserParamActions = new List<Action<CallbackInfo>>();
        public List<Action<CallbackInfo>> onFailureUserParamActions = new List<Action<CallbackInfo>>();

      
    }
}
