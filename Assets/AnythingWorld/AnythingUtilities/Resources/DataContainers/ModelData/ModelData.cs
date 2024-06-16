using UnityEngine;
using System;

namespace AnythingWorld.Utilities.Data
{
    [Serializable]
    public class ModelData
    {
        public string guid => json?.name;

        //Search term entered by user.
        public string searchTerm;
        //Parameters passed in by user.
        public RequestParamObject parameters = new RequestParamObject();
        /// <summary>
        /// JSON data requested from server
        /// </summary>
        [SerializeField]
        public ModelJson json;

        /// <summary>
        /// Top level game object for this model, 
        /// Created at beginning of request and passed back to user.
        /// </summary>
        [SerializeField]
        public GameObject model;

        //Maybe make obsolete and move this reference to some other part of data
        /// <summary>
        /// Model rig if animated GLB.
        /// </summary>
        [SerializeField]
        public GameObject rig;

        /// <summary>
        /// Type of animation pipeline used by this model.
        /// </summary>
        [SerializeField]
        public AnimationPipeline animationPipeline;
        
        /// <summary>
        /// Type of model creation pipeline used by this model.
        /// </summary>
        [SerializeField]
        public ModelLoadingPipeline modelLoadingPipeline;

        [SerializeField]
        public DefaultBehaviourType defaultBehaviourType;
        [SerializeField]
        public RequestType requestType; 
        ///Monobehaviour script to anchor coroutines to, attached to anchor GameObject
        [SerializeField]
        public LoadingObject loadingScript;
        /// <summary>
        /// Class containing action callbacks that can be subscribed to/invoked.
        /// </summary>
        [SerializeField]
        public ModelActions actions = new ModelActions();
        /// <summary>
        /// Data loaded from database.
        /// </summary>
        [SerializeField]
        public LoadedData loadedData = new LoadedData();

        public void Debug(string message)
        {
            actions.factoryDebug(this, message); 
        }
        public void Debug(object obj)
        {
            actions.factoryDebug(this, obj.ToString());
        }
    }
}
