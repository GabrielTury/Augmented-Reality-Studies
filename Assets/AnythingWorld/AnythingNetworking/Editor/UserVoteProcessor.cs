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
    public class VotedModels
    {
        public List<ModelJson> upvoted;
        public List<ModelJson> downvoted;
    }

    public static class UserVoteProcessor
    {
        public delegate void SearchCompleteDelegate(SearchResult[] results);
        private static SearchCompleteDelegate searchDelegate;

        public delegate void VoteChanged();
        public static VoteChanged voteChangeDelegate;

        public delegate void OnErrorDelegate(NetworkErrorMessage errorMessage);
        private static OnErrorDelegate failDelegate;

        public static void FlipUserVote(SearchResult searchResult, OnErrorDelegate onErrorDelegate, object owner)
        {
            CoroutineExtension.StartEditorCoroutine(ChangeUserVoteCoroutine(searchResult, onErrorDelegate), owner);
        }
        public static void GetVoteCards(SearchCompleteDelegate searchCompleteDelegate, Action onThumbnailLoaded, OnErrorDelegate onErrorDelegate, object owner)
        {
            //SaveLoadLocals.GetLocalUserVoted(searchCompleteDelegate, onThumbnailLoaded, owner);
            CoroutineExtension.StartEditorCoroutine(GetUserVotedCoroutine(searchCompleteDelegate, onThumbnailLoaded, onErrorDelegate, owner), owner);
        }
        public static IEnumerator ChangeUserVoteCoroutine(SearchResult searchResult, OnErrorDelegate onErrorDelegate)
        {
            //Set vote type
            string voteType;
            if (searchResult.data.userVote == "none") voteType = "upvote";
            else voteType = "revoke";

            //split guid into name and id
            var nameSplit = searchResult.data.name.Split('#');
            
            //Make network post to vote endpoint
            UnityWebRequest www;
            var apiCall = NetworkConfig.VoteUri(voteType, nameSplit[0], nameSplit[1]);
#if UNITY_2022
            www = UnityWebRequest.PostWwwForm(apiCall, "");
#else
            www = UnityWebRequest.Post(apiCall, "");
#endif
            www.timeout = 5;
            yield return www.SendWebRequest();

            //Process response and update search result
            if (www.result == UnityWebRequest.Result.Success)
            {
                switch (searchResult.data.userVote)
                {
                    case "upvote":
                        searchResult.data.voteScore--;
                        searchResult.data.userVote = "none";
                        break;
                    case "none":
                        searchResult.data.voteScore++;
                        searchResult.data.userVote = "upvote";
                        break;
                }
                voteChangeDelegate?.Invoke();
                
            }
            else
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

        public static IEnumerator GetUserVotedCoroutine(SearchCompleteDelegate delegateFunc, Action onThumbnailLoaded, OnErrorDelegate onErrorDelegate, object owner)
        {
            var searchResultArray = new SearchResult[0];

            searchDelegate += delegateFunc;
            var apiCall = NetworkConfig.MyLikesUri();
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
                VotedModels resultsList;
                try
                {
                    resultsList = JsonConvert.DeserializeObject<VotedModels>(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not fetch the search results: {e}");
                    resultsList = new VotedModels();
                }

                if (resultsList == null) resultsList = new VotedModels();

                searchResultArray = new SearchResult[resultsList.upvoted.Count];
                for (var i = 0; i < searchResultArray.Length; i++)
                {
                    try
                    {
                        searchResultArray[i] = new SearchResult(resultsList.upvoted[i]);
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
                var results = searchResultArray.ToList();
                CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailsIndividually(results, onThumbnailLoaded, owner), owner);
            }
            //CoroutineExtension.StartEditorCoroutine(ThumbnailRequester.LoadThumbnailBatch(results), owner);
            www.Dispose();
            //Turn JSON into AWThing data format.
            searchDelegate?.Invoke(searchResultArray);
            //Unsubscribe search delegate
            searchDelegate -= delegateFunc;
        }
    }
}

