using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class Game
{
    public static bool GameActive = true;
    public static GameMode gameMode;
    public static bool IntroShown = false;

    //Score Values
    public static int Score;
    public static int HighScore;

    //FB Vars
    public static string Username;
    public static Texture UserTexture;
    public static List<object> Friends;
    public static Dictionary<string, Texture> FriendImages = new Dictionary<string, Texture>();
    public static bool ScoresReady;
    private static List<object> scores;
    public static List<object> Scores {
        get { return scores; }
        set { scores = value; ScoresReady = true; }
    }

    //Shader Hashes
    public static readonly int _V_WIRE_COLOR = Shader.PropertyToID("_V_WIRE_Color");

    public static void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit ();
        #endif
    }
    public static void LoadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n+1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static GameSettings Settings {
        get {
            return new GameSettings(
                PlayerPrefs.GetInt("HR"), PlayerPrefs.GetInt("B"), 
                PlayerPrefs.GetInt("CA"), PlayerPrefs.GetInt("LF"), 
                PlayerPrefs.GetFloat("MV"), PlayerPrefs.GetFloat("SV"),
                PlayerPrefs.GetInt("VN"));
        }
        set {
            PlayerPrefs.SetInt("HR", value.HalfResolution ? 1 : 0);
            PlayerPrefs.SetInt("B", value.Bloom ? 1 : 0);
            PlayerPrefs.SetInt("CA", value.ChromaticAberration ? 1 : 0);
            PlayerPrefs.SetInt("LF", value.LimitFps ? 1 : 0);
            PlayerPrefs.SetFloat("MV", value.MusicVolume);
            PlayerPrefs.SetFloat("SV", value.SoundVolume);
            PlayerPrefs.SetInt("VN", value.versionNumber);
            PlayerPrefs.Save();
        }
    }

    public static void InitaliseGameSettings(FxPro PostProccessingEffects, GameSettings gameSettings) {
        //PostProccessingEffects.HalfResolution = gameSettings.HalfResolution;
        PostProccessingEffects.Quality = gameSettings.HalfResolution ? FxProNS.EffectsQuality.Fastest : FxProNS.EffectsQuality.Fast;
        PostProccessingEffects.BloomEnabled = gameSettings.Bloom;
        PostProccessingEffects.ChromaticAberration = gameSettings.ChromaticAberration;
        if (!gameSettings.Bloom && !gameSettings.ChromaticAberration) {
            PostProccessingEffects.enabled = false;
        }
        else {
            PostProccessingEffects.enabled = true;
        }
        PostProccessingEffects.Init();
    }
}

public struct GameSettings {
    public bool HalfResolution;
    public bool Bloom;
    public bool ChromaticAberration;
    public bool LimitFps;
    public float MusicVolume;
    public float SoundVolume;
    public int versionNumber;

    public GameSettings(int _HalfResolution, int _Bloom, int _ChromaticAberration, int _LimitFps, float _MusicVolume, float _SoundVolume, int _versionNumber) {
        HalfResolution = _HalfResolution == 1;
        Bloom = _Bloom == 1;
        ChromaticAberration = _ChromaticAberration == 1;
        LimitFps = _LimitFps == 1;
        MusicVolume = _MusicVolume;
        SoundVolume = _SoundVolume;
        versionNumber = _versionNumber;
    }
}

public enum GameMode {
    Classic,
    Powerups
}