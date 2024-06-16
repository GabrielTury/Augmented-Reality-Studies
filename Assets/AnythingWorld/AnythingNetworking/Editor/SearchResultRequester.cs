using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using AnythingWorld.Utilities.Networking;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.EditorCoroutines.Editor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Networking
{
    public static class SearchResultRequester
    {
        public delegate void SearchCompleteDelegate(SearchResult[] results, string onFail);
        private static SearchCompleteDelegate searchDelegate;

        public delegate void OnErrorDelegate(NetworkErrorMessage errorMessage);
        private static OnErrorDelegate failDelegate;

        public static void RequestCategorySearchResults(string searchTerm, SearchCompleteDelegate searchCompleteDelegate, Action refreshWindow, OnErrorDelegate onErrorDelegate, object owner)
        {
            try
            {
#if UNITY_EDITOR
                EditorCoroutineUtility.StartCoroutine(CategorySearch(searchTerm, searchCompleteDelegate, refreshWindow, onErrorDelegate, owner), owner);
#endif
            }
            catch (Exception e)
            {
                var emptyList = new List<SearchResult>();
                searchDelegate += searchCompleteDelegate;
                searchDelegate(null, "An error has occurred!");
                searchDelegate -= searchCompleteDelegate;

                Debug.LogError($"Exception thrown while attempting category search: {e}");
            }
        }

        public static void RequestFeaturedResults(SearchCompleteDelegate searchCompleteDelegate, Action refreshWindow, OnErrorDelegate onErrorDelegate, object owner)
        {
            try
            {
#if UNITY_EDITOR
                EditorCoroutineUtility.StartCoroutine(FeaturedSearch(searchCompleteDelegate, refreshWindow, onErrorDelegate, owner), owner);
#endif
            }
            catch (Exception e)
            {
                var emptyList = new List<SearchResult>();
                searchDelegate += searchCompleteDelegate;
                searchDelegate(null, "An error has occurred!");
                searchDelegate -= searchCompleteDelegate;

                Debug.LogError($"Exception thrown while attempting featured search: {e}");
            }
        }

        public static IEnumerator CategorySearch(string searchTerm, SearchCompleteDelegate delegateFunc, Action onThumbnailLoad, OnErrorDelegate onErrorDelegate, object owner)
        {
            var searchResultArray = new SearchResult[0];

            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (searchTerm == "")
            {
                var emptyList = new List<SearchResult>();
                searchDelegate += delegateFunc;
                searchDelegate(emptyList.ToArray(), "The search term was empty. Try again by searching for something else!");
                searchDelegate -= delegateFunc;
                yield break;
            }

            searchDelegate += delegateFunc;

            #region Get Result JSON Array
            var apiKey = AnythingSettings.APIKey;
            var appName = AnythingSettings.AppName;

            if (string.IsNullOrEmpty(appName))
            {
                Debug.LogWarning("App name missing! Setting app name to \"My Anything WWorld App\"");
                appName = AnythingSettings.AppName = "My Anything World App";
            }
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("Please enter an API Key in AnythingSettings!");
                yield break;
            }
            string sortingSubstring = $"&sort=default&descending=true&fuzzy=true";

            var apiCall = NetworkConfig.SearchUri(searchTerm, sortingSubstring);
            var www = UnityWebRequest.Get(apiCall);
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
                    //Turn JSON into AWThing data format.
                    searchDelegate(new SearchResult[] { }, $"Sorry! We didn't find any results for \"{searchTerm}\".");

                    //Unsubscribe search delegate
                    searchDelegate -= delegateFunc;
                    yield break;
                }
            }
            else
            {
                //Convert response to json format
                var result = www.downloadHandler.text;
                List<ModelJson> resultsList;
                try
                {
                    resultsList = JsonConvert.DeserializeObject<List<ModelJson>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not deserialize into result lists: {e}");
                    resultsList = new List<ModelJson>();
                }
                #endregion Get Result JSON Array
                searchResultArray = new SearchResult[resultsList.Count];
                for (var i = 0; i < searchResultArray.Length; i++)
                {
                    try
                    {
                        searchResultArray[i] = new SearchResult(resultsList[i]);
                        var madeResult = searchResultArray[i];
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

                CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailsIndividually(searchResultArray.ToList(), onThumbnailLoad, owner), owner);
                //CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailBatch(searchResultArray.ToList()), owner);
            }
            www.Dispose();
            //Turn JSON into AWThing data format.
            searchDelegate(searchResultArray, $"Sorry! We didn't find any results for \"{ searchTerm}\".");

            //Unsubscribe search delegate
            searchDelegate -= delegateFunc;
        }

        public static IEnumerator FeaturedSearch(SearchCompleteDelegate delegateFunc, Action onThumbnailLoad, OnErrorDelegate onErrorDelegate, object owner)
        {
            var searchResultArray = new SearchResult[0];

            var sw = System.Diagnostics.Stopwatch.StartNew();
            searchDelegate += delegateFunc;

            #region Get Result JSON Array
            var apiKey = AnythingSettings.APIKey;
            var appName = AnythingSettings.AppName;

            if (string.IsNullOrEmpty(appName))
            {
                Debug.LogWarning("App name missing! Setting app name to \"My Anything World App\"");
                appName = AnythingSettings.AppName = "My Anything World App";
            }
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("Please enter an API Key in AnythingSettings!");
                yield break;
            }

            var apiCall = NetworkConfig.FeaturedUri();

            var www = UnityWebRequest.Get(apiCall);
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
                    //Turn JSON into AWThing data format.
                    searchDelegate(new SearchResult[] { }, $"Sorry! We didn't find anything on the featured tab.");

                    //Unsubscribe search delegate
                    searchDelegate -= delegateFunc;
                    yield break;
                }
            }
            else
            {
                //Convert response to json format
                var result = www.downloadHandler.text;
                List<ModelJson> resultsList;
                try
                {
                    resultsList = JsonConvert.DeserializeObject<List<ModelJson>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not deserialize into result lists: {e}");
                    resultsList = new List<ModelJson>();
                }

                #endregion Get Result JSON Array
                searchResultArray = new SearchResult[resultsList.Count];
                for (var i = 0; i < searchResultArray.Length; i++)
                {
                    try
                    {
                        searchResultArray[i] = new SearchResult(resultsList[i]);
                        var madeResult = searchResultArray[i];
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

                CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailsIndividually(searchResultArray.ToList(), onThumbnailLoad, owner), owner);
                //CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailBatch(searchResultArray.ToList()), owner);
            }
            www.Dispose();
            //Turn JSON into AWThing data format.
            searchDelegate(searchResultArray, $"Sorry! We didn't find anything on the featured tab.");

            //Unsubscribe search delegate
            searchDelegate -= delegateFunc;
        }
    }
}