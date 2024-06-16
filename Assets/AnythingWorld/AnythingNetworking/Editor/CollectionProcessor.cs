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
    public static class CollectionProcessor
    {
        public delegate void SearchCompleteDelegate(CollectionResult[] results);
        private static SearchCompleteDelegate searchDelegate;

        public delegate void RefreshCompleteDelegate(CollectionResult result);
        private static RefreshCompleteDelegate refreshDelegate;

        public delegate void NameFetchDelegate(string[] results);
        private static NameFetchDelegate nameDelegate;

        public delegate void OnErrorDelegate(NetworkErrorMessage errorMessage);
        private static OnErrorDelegate failDelegate;

        public static void AddToCollection(SearchCompleteDelegate searchCompleteDelegate, SearchResult searchResult, string collectionNames, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(AddToCollectionCoroutine(searchCompleteDelegate, collectionNames, searchResult, onErrorDelegate, parent), parent);
        }

        public static void RemoveFromCollection(SearchCompleteDelegate searchCompleteDelegate, SearchResult searchResult, CollectionResult collection, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(RemoveFromCollectionCoroutine(searchCompleteDelegate, collection, searchResult, onErrorDelegate, parent), parent);
        }

        public static void CreateNewCollection(SearchCompleteDelegate searchCompleteDelegate, CollectionResult collection, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(CreateCollectionCoroutine(searchCompleteDelegate, collection, onErrorDelegate, parent), parent);
        }

        public static void DeleteCollection(SearchCompleteDelegate searchCompleteDelegate, CollectionResult collection, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(DeleteCollectionCoroutine(searchCompleteDelegate, collection, onErrorDelegate, parent), parent);
        }

        public static void GetCollectionNames(NameFetchDelegate nameFetchDelegate, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserCollectionNamesCoroutine(nameFetchDelegate, onErrorDelegate), parent);
        }

        public static void GetCollections(SearchCompleteDelegate searchCompleteDelegate, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserCollectionsCoroutine(searchCompleteDelegate, onErrorDelegate), parent);
        }

        public static void GetCollection(RefreshCompleteDelegate refreshCompleteDelegate, CollectionResult collection, OnErrorDelegate onErrorDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserCollectionCoroutine(refreshCompleteDelegate, collection, onErrorDelegate), parent);
        }

        private static IEnumerator GetUserCollectionNamesCoroutine(NameFetchDelegate delegateFunc, OnErrorDelegate onErrorDelegate)
        {
            var collectionNames = new string[0];

            nameDelegate += delegateFunc;
            
            var apiCall = NetworkConfig.UserCollectionsUri(true);
            using var www = UnityWebRequest.Get(apiCall);
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
                Dictionary<string, List<string>> resultsDictionary;
                try
                {
                    resultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not fetch the search results: {e}");
                    resultsDictionary = new Dictionary<string, List<string>>();
                }

                collectionNames = resultsDictionary.Keys.ToArray();
            }
            www.Dispose();
            nameDelegate(collectionNames);
            nameDelegate -= delegateFunc;
        }

        private static IEnumerator GetUserCollectionsCoroutine(SearchCompleteDelegate delegateFunc, OnErrorDelegate onErrorDelegate)
        {
            var collectionResults = new CollectionResult[0];

            searchDelegate += delegateFunc;

            UnityWebRequest www;
            var apiCall = NetworkConfig.UserCollectionsUri(false);
            www = UnityWebRequest.Get(apiCall);
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

                Dictionary<string, List<ModelJson>> resultsDictionary;
                try
                {
                    resultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<ModelJson>>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not fetch the search results: {e}");
                    resultsDictionary = new Dictionary<string, List<ModelJson>>();
                }
                collectionResults = new CollectionResult[resultsDictionary.Count];
                for (int i = 0; i < collectionResults.Length; i++)
                {
                    KeyValuePair<string, List<ModelJson>> kvp = resultsDictionary.ElementAt(i);
                    collectionResults[i] = new CollectionResult(kvp.Key, kvp.Value);
                    yield return ThumbnailRequester.LoadThumbnailsBatch(collectionResults[i].Results.ToArray());
                }
            }
            www.Dispose();
            //Turn JSON into AWThing data format.
            searchDelegate(collectionResults);

            //Unsubscribe search delegate
            searchDelegate -= delegateFunc;
        }

        private static IEnumerator GetUserCollectionCoroutine(RefreshCompleteDelegate delegateFunc, CollectionResult collection, OnErrorDelegate onErrorDelegate)
        {
            CollectionResult collectionResult = null;

            refreshDelegate += delegateFunc;

            UnityWebRequest www;
            var apiCall = NetworkConfig.UserCollectionsUri(false);
            www = UnityWebRequest.Get(apiCall);
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

                Dictionary<string, List<ModelJson>> resultsDictionary = new Dictionary<string, List<ModelJson>>();
                try
                {
                    resultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<ModelJson>>>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not fetch the search results: {e}");
                    resultsDictionary = new Dictionary<string, List<ModelJson>>();
                }

                KeyValuePair<string, List<ModelJson>> kvp = resultsDictionary.FirstOrDefault(x => x.Key == collection.Name);
                collectionResult = new CollectionResult(kvp.Key, kvp.Value);
                yield return ThumbnailRequester.LoadThumbnailsBatch(collectionResult.Results.ToArray());
            }
            www.Dispose();
            //Turn JSON into AWThing data format.
            refreshDelegate(collectionResult);

            //Unsubscribe search delegate
            refreshDelegate -= delegateFunc;
        }

        private static IEnumerator CreateCollectionCoroutine(SearchCompleteDelegate delegateFunc, CollectionResult collection, OnErrorDelegate onErrorDelegate, object parent)
        {
            UnityWebRequest www;
            var apiCall = NetworkConfig.AddCollectionUri(collection.Name);
#if UNITY_2022
            www = UnityWebRequest.PostWwwForm(apiCall, "");
#else
            www = UnityWebRequest.Post(apiCall, "");
#endif
            www.timeout = 5;
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
            if (www.result == UnityWebRequest.Result.Success) CoroutineExtension.StartEditorCoroutine(GetUserCollectionsCoroutine(delegateFunc, onErrorDelegate), parent);

            www.Dispose();
        }

        private static IEnumerator DeleteCollectionCoroutine(SearchCompleteDelegate delegateFunc, CollectionResult collection, OnErrorDelegate onErrorDelegate, object parent)
        {
            searchDelegate += delegateFunc;

            UnityWebRequest www;
            var apiCall = NetworkConfig.RemoveCollectionUri(collection.Name);
#if UNITY_2022
            www = UnityWebRequest.PostWwwForm(apiCall, "");
#else
            www = UnityWebRequest.Post(apiCall, "");
#endif
            www.timeout = 5;
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
            if (www.result == UnityWebRequest.Result.Success) CoroutineExtension.StartEditorCoroutine(GetUserCollectionsCoroutine(delegateFunc, onErrorDelegate), parent);
            
            www.Dispose();
        }

        private static IEnumerator AddToCollectionCoroutine(SearchCompleteDelegate delegateFunc, string collectionName, SearchResult searchResult, OnErrorDelegate onErrorDelegate, object parent)
        {
            var nameSplit = searchResult.data.name.Split('#');

            UnityWebRequest www;
            var apiCall = NetworkConfig.AddToCollectionUri(collectionName, nameSplit[0], nameSplit[1]);
#if UNITY_2022
            www = UnityWebRequest.PostWwwForm(apiCall, "");
#else
            www = UnityWebRequest.Post(apiCall, "");
#endif
            www.timeout = 5;
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
            if (www.result == UnityWebRequest.Result.Success) CoroutineExtension.StartEditorCoroutine(GetUserCollectionsCoroutine(delegateFunc, onErrorDelegate), parent);
            
            www.Dispose();
        }

        private static IEnumerator RemoveFromCollectionCoroutine(SearchCompleteDelegate delegateFunc, CollectionResult collection, SearchResult searchResult, OnErrorDelegate onErrorDelegate, object parent)
        {
            var nameSplit = searchResult.data.name.Split('#');

            UnityWebRequest www;
            var apiCall = NetworkConfig.RemoveFromCollectionUri(collection.Name, nameSplit[0], nameSplit[1]);
#if UNITY_2022
            www = UnityWebRequest.PostWwwForm(apiCall, "");
#else
            www = UnityWebRequest.Post(apiCall, "");
#endif
            www.timeout = 5;
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
            www.Dispose();
        }
    }
}
