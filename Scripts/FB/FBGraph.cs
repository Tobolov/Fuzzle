using UnityEngine;
using System;
using System.Collections.Generic;
using Facebook.Unity;

public static class FBGraph {
    //#region PlayerInfo
    public static void GetPlayerInfo() {
        string queryString = "/me?fields=id,first_name,picture.width(120).height(120)";
        FB.API(queryString, HttpMethod.GET, GetPlayerInfoCallback);
    }

    private static void GetPlayerInfoCallback(IGraphResult result) {
        Debug.Log("GetPlayerInfoCallback");
        if (result.Error != null) {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);

        // Save player name
        string name;
        if (result.ResultDictionary.TryGetValue("first_name", out name)) {
            Game.Username = name;
        }

        //Fetch player profile picture from the URL returned
        string playerImgUrl = GraphUtil.DeserializePictureURL(result.ResultDictionary);
        GraphUtil.LoadImgFromURL(playerImgUrl, delegate (Texture pictureTexture) {
            // Setup the User's profile picture
            if (pictureTexture != null) {
                Game.UserTexture = pictureTexture;
            }

            if (StartMenu.self != null)
                StartMenu.self.FacebookUser.text = "Welcome " + Game.Username;
        });

    }
    //#endregion

    #region Friends
    public static void GetFriends() {
        string queryString = "/me/friends?fields=id,first_name,picture.width(128).height(128)&limit=100";
        FB.API(queryString, HttpMethod.GET, GetFriendsCallback);
    }

    private static void GetFriendsCallback(IGraphResult result) {
        Debug.Log("GetFriendsCallback");
        if (result.Error != null) {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);

        // Store /me/friends result
        object dataList;
        if (result.ResultDictionary.TryGetValue("data", out dataList)) {
            var friendsList = (List<object>)dataList;
            CacheFriends(friendsList);
        }
    }

    public static void GetInvitableFriends() {
        string queryString = "/me/invitable_friends?fields=id,first_name,picture.width(128).height(128)&limit=100";
        FB.API(queryString, HttpMethod.GET, GetInvitableFriendsCallback);
    }

    private static void GetInvitableFriendsCallback(IGraphResult result) {
        Debug.Log("GetInvitableFriendsCallback");
        if (result.Error != null) {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);

        // Store /me/invitable_friends result
        object dataList;
        if (result.ResultDictionary.TryGetValue("data", out dataList)) {
            var invitableFriendsList = (List<object>)dataList;
            CacheFriends(invitableFriendsList);
        }
    }

    private static void CacheFriends(List<object> newFriends) {
        if (Game.Friends != null && Game.Friends.Count > 0) {
            Game.Friends.AddRange(newFriends);
        } else {
            Game.Friends = newFriends;
        }
    }
    #endregion

    #region Scores
    public static void GetScores(Action callback) {
        FB.API("/app/scores?fields=score,user.limit(8)", HttpMethod.GET, (gr) => { GetScoresCallback(gr, callback); });
    }

    private static void GetScoresCallback(IGraphResult result, Action callback) {
        Debug.Log("GetScoresCallback");
        if (result.Error != null) {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);

        // Parse scores info
        var scoresList = new List<object>();

        object scoresh;
        if (result.ResultDictionary.TryGetValue("data", out scoresh)) {
            scoresList = (List<object>)scoresh;
        }

        // Parse score data
        HandleScoresData(scoresList);
        callback();
    }

    private static void HandleScoresData(List<object> scoresResponse) {
        var structuredScores = new List<object>();
        foreach (object scoreItem in scoresResponse) {
            // Score JSON format
            // {
            //   "score": 4,
            //   "user": {
            //      "name": "Chris Lewis",
            //      "id": "10152646005463795"
            //   }
            // }

            var entry = (Dictionary<string, object>)scoreItem;
            var user = (Dictionary<string, object>)entry["user"];
            string userId = (string)user["id"];

            if (string.Equals(userId, AccessToken.CurrentAccessToken.UserId)) {
                // This entry is the current player
                int playerHighScore = GraphUtil.GetScoreFromEntry(entry);
                Debug.Log("Local players score on server is " + playerHighScore);
                if (playerHighScore < Game.Score) {
                    Debug.Log("Locally overriding with just acquired score: " + Game.Score);
                    playerHighScore = Game.Score;
                }

                entry["score"] = playerHighScore.ToString();
                Game.HighScore = playerHighScore;
            }

            structuredScores.Add(entry);
            if (!Game.FriendImages.ContainsKey(userId)) {
                // We don't have this players image yet, request it now
                LoadFriendImgFromID(userId, pictureTexture => {
                    if (pictureTexture != null) {
                        Game.FriendImages.Add(userId, pictureTexture);
                    }
                });
            }
        }

        Game.Scores = structuredScores;
    }

    // Graph API call to fetch friend picture from user ID returned from FBGraph.GetScores()
    //
    // Note: /me/invitable_friends returns invite tokens instead of user ID's,
    // which will NOT work with this /{user-id}/picture Graph API call.
    private static void LoadFriendImgFromID(string userID, Action<Texture> callback) {
        FB.API(GraphUtil.GetPictureQuery(userID, 128, 128),
               HttpMethod.GET,
               delegate (IGraphResult result)
               {
                   if (result.Error != null) {
                       Debug.LogError(result.Error + ": for friend " + userID);
                       return;
                   }
                   if (result.Texture == null) {
                       Debug.Log("LoadFriendImg: No Texture returned");
                       return;
                   }
                   callback(result.Texture);
               });
    }
    #endregion
}