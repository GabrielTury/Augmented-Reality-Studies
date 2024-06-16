using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using AnythingWorld.Utilities.Networking;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Networking.Editor
{
    /// <summary>
    /// Class for fetching user processed models from the database and converting them into AWThing data format. 
    /// </summary>
    public static class UserProcessedModels
    {
        //reuse the search result class for the user processed models
        public delegate void SearchCompleteDelegate(SearchResult[] results);
        private static SearchCompleteDelegate searchDelegate;

        public delegate void RefreshCompleteDelegate(CollectionResult result);
        private static RefreshCompleteDelegate refreshDelegate;

        public delegate void NameFetchDelegate(string[] results);
        private static NameFetchDelegate nameDelegate;

        public delegate void OnErrorDelegate(NetworkErrorMessage errorMessage);
        private static OnErrorDelegate failDelegate;
        /// <summary>
        /// Get the names of all the models processed in the database using a delegate of type searchCompleteDelegate.
        /// </summary>
        public static void GetProcessedModels(SearchCompleteDelegate searchCompleteDelegate, Action onThumbnailLoaded, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserProcessedCoroutine(searchCompleteDelegate, onThumbnailLoaded, onErrorDelegate, parent));
        }
        /// <summary>
        /// Get the names of all the models processed in the database.
        /// </summary>
        public static IEnumerator GetUserProcessedCoroutine(SearchCompleteDelegate delegateFunc, Action onThumbnailLoaded, OnErrorDelegate onErrorDelegate, object owner)
        {
            var searchResultArray = new SearchResult[0];
            searchDelegate += delegateFunc;
            var apiCall = NetworkConfig.GetUserProcessedUri(false);
            UnityWebRequest www = UnityWebRequest.Get(apiCall);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www);
#if UNITY_EDITOR
                    failDelegate += onErrorDelegate;
                    failDelegate(error);
                    failDelegate -= onErrorDelegate;
#else
                    NetworkErrorHandler.HandleError(error);
#endif
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                List<ModelJson> resultsList;
                try
                {
                    resultsList = JsonConvert.DeserializeObject<List<ModelJson>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not fetch the search results: {e}");
                    resultsList = new List<ModelJson>();
                }

                if (resultsList == null) resultsList = new List<ModelJson>();

                searchResultArray = new SearchResult[resultsList.Count];
                for (var i = 0; i < searchResultArray.Length; i++)
                {
                    try
                    {
                        searchResultArray[i] = new SearchResult(resultsList[i]);
                        var madeResult = searchResultArray[i];
                        madeResult.IsProcessedResult = true;
                        //Set JSON and MongoID
                        madeResult.json = resultsList[i];
                        madeResult.mongoId = resultsList[i]._id;
                        var animationPipeline = JsonProcessor.ParseAnimationPipeline(madeResult.data);
                        //Set if model is animated through our standards, used for filtering.
                        if (!(animationPipeline == AnimationPipeline.Static)) madeResult.isAnimated = true;
                        else
                        {
                            madeResult.isAnimated = false;
                        }
                    }
                    catch
                    {
                        Debug.Log($"Error setting value at index {i}");
                    }
                }
                var results = searchResultArray.ToList();
                CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailsIndividually(results, onThumbnailLoaded, owner), owner);
            }
            www.Dispose();
            //Turn JSON into AWThing data format.
            searchDelegate?.Invoke(searchResultArray);
            //Unsubscribe search delegate
            searchDelegate -= delegateFunc;
        }
    }

}

