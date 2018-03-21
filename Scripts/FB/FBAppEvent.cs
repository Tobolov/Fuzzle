using UnityEngine;
using System.Collections.Generic;
using Facebook.Unity;

public static class FBAppEvents {
    private static readonly string EVENT_NAME_GAME_PLAYED = "game_played";
    private static readonly string EVENT_PARAM_SCORE = "score";

    public static void LaunchEvent() {
        // Use ActivateApp to log activation events in your game enabling measurement for App Ads and Facebook Analytics for Apps.
        FB.ActivateApp();
    }

    public static void GameComplete(int score) {
        var param = new Dictionary<string, object>();
        param[EVENT_PARAM_SCORE] = score;
        FB.LogAppEvent(EVENT_NAME_GAME_PLAYED, null, param);
    }
}