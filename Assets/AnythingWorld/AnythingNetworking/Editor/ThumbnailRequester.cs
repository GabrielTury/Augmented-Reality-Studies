using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Networking
{
    public static class ThumbnailRequester
    {
        public static IEnumerator LoadThumbnailBatch(List<SearchResult> searchResults, int batchSize = 20)
        {
            var chunks = searchResults.ChunkBy(batchSize);
            foreach(var chunk in chunks)
            {
                yield return LoadThumbnailsBatch(chunk.ToArray());
            }
        }
        public static IEnumerator LoadThumbnailsIndividually(List<SearchResult> searchResultArray, Action onComplete, object owner)
        {
            var requests = new List<UnityWebRequestAsyncOperation>(searchResultArray.Count);
            // Start all requests
            foreach (var result in searchResultArray)
            {
                CoroutineExtension.StartEditorCoroutine(GetThumbnail(result, onComplete), owner);
            }

            yield return null;
        }


        public static IEnumerator GetThumbnail(SearchResult result, Action onComplete)
        {
            using (var www = UnityWebRequestTexture.GetTexture(result.thumbnailUrl))
            {
                // starts the request but doesn't wait for it for now
                var request = www.SendWebRequest();
                yield return new WaitUntil(()=>request.isDone);

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Else if successful
                    var myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    result.Thumbnail = myTexture;
                    result.ResultHasThumbnail = true;
                }
                else
                {
                    // If failed
                    result.ResultHasThumbnail = false;
                }
            }            

            onComplete?.Invoke();
        }
        public static IEnumerator LoadThumbnailsBatch(SearchResult[] searchResultArray)
        {
            var requests = new List<UnityWebRequestAsyncOperation>(searchResultArray.Length);
            // Start all requests
            foreach (var result in searchResultArray)
            {
                //var url = result.data.thumbnails.aw_thumbnail_transparent ?? result.data.model.other.aw_thumbnail;
                var www = UnityWebRequestTexture.GetTexture(result.thumbnailUrl);
                // starts the request but doesn't wait for it for now
                requests.Add(www.SendWebRequest());
            }

            // Now wait for all requests parallel
            yield return new WaitUntil(() => AllRequestsDone(requests));
            
            // Now evaluate all results
            HandleAllRequestsWhenFinished(requests, searchResultArray);

            foreach (var request in requests)
            {
                try
                {
                    request.webRequest.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Problem disposing of async web request");
                    Debug.LogException(e);
                }
            }
        }
     
        private static bool AllRequestsDone(IEnumerable<UnityWebRequestAsyncOperation> requests)
        {
            return requests.All(r => r.isDone);
        }

        private static void HandleAllRequestsWhenFinished(IReadOnlyList<UnityWebRequestAsyncOperation> requests, SearchResult[] searchResult)
        {
            for (var i = 0; i < requests.Count; i++)
            {
                var www = requests[i].webRequest;
                if (www.result==UnityWebRequest.Result.Success)
                {
                    // Else if successful
                    var myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    searchResult[i].Thumbnail = myTexture;
                    searchResult[i].ResultHasThumbnail = true;
                }
                else
                {
                    // If failed
                    searchResult[i].ResultHasThumbnail = false;
                }
            }
        }
    }
}
