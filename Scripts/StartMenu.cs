using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Facebook.Unity;

public class StartMenu : MonoBehaviour {
    public static StartMenu self;

    [Header("Home Buttons")]
    public Button StartButton;
    public Text StartButtonText;
    public Button LeaderboardButton;
    public Button SettingsButton;
    public Button ExitButton;
    public Button Facebook;
    public Text FacebookUser;

    [Header("Mode Buttons")]
    public Button ModeCancelButton;
    public Button ModeClassicButton;
    public Button ModePowerupsButton;
    public Text ModeClassicText;
    public Text ModePowerupsText;

    [Header("Setting Buttons + Options")]
    public Button SettingsCancelButton;
    public Button SettingsSaveButton;
    public Toggle HalfResolutionToggle;
    public Toggle BloomToggle;
    public Toggle ChromaticAberrationToggle;
    public Toggle LimitFpsToggle;
    public Slider MusicVolumeSlider;
    public Slider SoundVolumeSlider;

    [Header("Leaderboard Buttons + UI")]
    public Button LeaderboardCancel;
    public GameObject LeaderboardHeader;
    public Transform TopBox;
    public Transform BottomBox;
    public Transform LeftBox;
    public Transform RightBox;
    public Image LoadingImage;

    [Header("UI Groups")]
    public GameObject MainButtonGroup;
    public GameObject SettingsButtonGroup;
    public GameObject SettingsOptionGroup;
    public GameObject LeaderboardButtonGroup;
    public GameObject LeaderboardGroup;
    public GameObject LeaderboardElementGroup;
    public GameObject ModesButtonGroup;
    public GameObject ModesOptionGroup;

    [Header("Other References")]
    public Text Title;
    public Text Matek;
    public Image Flash;
    public ParticleSystem BackgroundParticleSystem;
    public FxPro PostProccessingEffects;

    [HideInInspector]
    private List<Image> Animatable;
    private List<Text> AnimatableLeader;
    private Transform[] SettingLabels;
    private Transform[] SettingOptions;
    private float timeInCycle = 5f;
    ParticleSystem.Particle[] BackgroundParticles;
    private GameObject LeaderboardElement;

    void Awake() {
        self = this;

        // Initialize FB SDK
        if (!FB.IsInitialized) {
            FB.Init(InitCallback);
        }
    }

    void OnApplicationPause(bool pauseStatus) {
        if (Application.isEditor) {
            return;
        }

        if (!pauseStatus) {
            if (FB.IsInitialized) {
                FBAppEvents.LaunchEvent();
            } else {
                FB.Init(InitCallback);
            }
        }
    }

#region Facebook
    private void InitCallback() {
        Debug.Log("InitCallback");

        FBAppEvents.LaunchEvent();

        if (FB.IsLoggedIn) {
            Debug.Log("Already logged in");
            OnLoginComplete();
        }
    }

    public void OnLoginClick() {
        Debug.Log("OnLoginClick");

        // Disable the Login Button
        Facebook.interactable = false;

        FBLogin.PromptForLogin(OnLoginComplete);
    }

    private void OnLoginComplete() {
        Debug.Log("OnLoginComplete");

        if (!FB.IsLoggedIn) {
            Facebook.interactable = true;
            return;
        }

        // Begin querying the Graph API for Facebook data
        FBGraph.GetPlayerInfo();
        //FBGraph.GetInvitableFriends();
    }
#endregion

    void Start () {
        //Check Player Pref Initalised + Update Player Pref
        if (!PlayerPrefs.HasKey("HR"))
            PlayerPrefs.SetInt("HR", 0);
        if (!PlayerPrefs.HasKey("B"))
            PlayerPrefs.SetInt("B", 1);
        if (!PlayerPrefs.HasKey("CA"))
            PlayerPrefs.SetInt("CA", 1);
        if (!PlayerPrefs.HasKey("LF"))
            PlayerPrefs.SetInt("LF", 0);
        if (!PlayerPrefs.HasKey("MV"))
            PlayerPrefs.SetFloat("MV", 0.7f);
        if (!PlayerPrefs.HasKey("SV"))
            PlayerPrefs.SetFloat("SV", 0.7f);
        if (!PlayerPrefs.HasKey("VN"))
            PlayerPrefs.SetInt("VN", 8);
        PlayerPrefs.Save();

        //Get Game Settings and set them
        GameSettings gameSettings = Game.Settings;
        Game.InitaliseGameSettings(PostProccessingEffects, gameSettings);

        //Facebook Group
        Facebook.onClick.AddListener(OnLoginClick);
        FacebookUser.text = Game.Username != null ? "Welcome " + Game.Username : "Log In";

        //Main Button Group
        StartButton.onClick.AddListener(OpenModes);
        LeaderboardButton.onClick.AddListener(() => {
            LeanTween.cancelAll(false);
            CloseMain(true);
            OpenLeaderboard();
            LoadLeaderboard();
        });
        SettingsButton.onClick.AddListener(() => {
            LeanTween.cancelAll(false);
            CloseMain();
            OpenSettings();
        });
        ExitButton.onClick.AddListener(() => {
            CloseMain(true);
            CloseParticles();
            LeanTween.delayedCall(1f, Game.Quit);
        });

        //Mode Buttons Group 
        ModeCancelButton.onClick.AddListener(() => {
            OpenMain();
            CloseModes();
        });
        ModeClassicButton.onClick.AddListener(() => {
            Game.gameMode = GameMode.Classic;
            CloseMain(true);
            CloseParticles();
            LeanTween.delayedCall(1f, () => { Game.LoadByIndex(1); });
        });
        ModePowerupsButton.onClick.AddListener(() => {
            Game.gameMode = GameMode.Powerups;
            CloseMain(true);
            CloseParticles();
            LeanTween.delayedCall(1f, () => { Game.LoadByIndex(1); });
        });

        //Settings Button Group
        SettingsCancelButton.onClick.AddListener(() => {

            //Reset Toggle Switches
            HalfResolutionToggle.isOn = gameSettings.HalfResolution;
            BloomToggle.isOn = gameSettings.Bloom;
            ChromaticAberrationToggle.isOn = gameSettings.ChromaticAberration;

            Game.InitaliseGameSettings(PostProccessingEffects, gameSettings);

            LeanTween.cancelAll(false);
            OpenMain();
            CloseSettings();
        });
        SettingsSaveButton.onClick.AddListener(() => {

        //Take switch states and create new Game Settings and set it 
            GameSettings newGameSettings = new GameSettings(
                HalfResolutionToggle.isOn ? 1 : 0,
                BloomToggle.isOn ? 1 : 0,
                ChromaticAberrationToggle.isOn ? 1 : 0,
                LimitFpsToggle ? 1 : 0,
                MusicVolumeSlider.value,
                SoundVolumeSlider.value,
                gameSettings.versionNumber
            );
            Game.Settings = newGameSettings;

            Game.InitaliseGameSettings(PostProccessingEffects, newGameSettings);

            LeanTween.cancelAll(false);
            OpenMain();
            CloseSettings();
        });

        //Settings Options Group
        HalfResolutionToggle.isOn = gameSettings.HalfResolution;
        HalfResolutionToggle.onValueChanged.AddListener((e) => {
            //PostProccessingEffects.HalfResolution = e;
            PostProccessingEffects.Quality = e ? FxProNS.EffectsQuality.Fastest : FxProNS.EffectsQuality.Fast;
            PostProccessingEffects.Init();
        });
        BloomToggle.isOn = gameSettings.Bloom;
        BloomToggle.onValueChanged.AddListener((e) => {
            if(!BloomToggle.isOn && !ChromaticAberrationToggle.isOn) {
                PostProccessingEffects.enabled = false;
            }
            else {
                PostProccessingEffects.enabled = true;
                PostProccessingEffects.BloomEnabled = e;
                PostProccessingEffects.Init();
            }
        });
        ChromaticAberrationToggle.isOn = gameSettings.ChromaticAberration;
        ChromaticAberrationToggle.onValueChanged.AddListener((e) => {
            if (!BloomToggle.isOn && !ChromaticAberrationToggle.isOn) {
                PostProccessingEffects.enabled = false;
            } else {
                PostProccessingEffects.enabled = true;
                PostProccessingEffects.ChromaticAberration = e;
                PostProccessingEffects.Init();
            }
        });
        LimitFpsToggle.isOn = gameSettings.LimitFps;
        MusicVolumeSlider.value = gameSettings.MusicVolume;
        SoundVolumeSlider.value = gameSettings.SoundVolume;

        //Leaderboard Buttons Group 
        LeaderboardCancel.onClick.AddListener(() => {
            LeanTween.cancelAll(false);
            CloseLeaderboard();
            LeanTween.delayedCall(0.8f, () => { OpenMain(); });
        });

        //Find Animatable Objects
        Animatable = new List<Image>();
        AnimatableLeader = new List<Text>();
        foreach (Image i in GetComponentsInChildren<Image>(true)) {
            if (i != null && i.tag != "IgnoreUIScan")
                Animatable.Add(i);
        }

        SettingOptions = new Transform[] {
            HalfResolutionToggle.transform,
            BloomToggle.transform,
            ChromaticAberrationToggle.transform,
            LimitFpsToggle.transform,
            MusicVolumeSlider.transform,
            SoundVolumeSlider.transform
        };
        SettingLabels = new Transform[SettingOptions.Length];
        for(int i = 0; i < SettingOptions.Length; i++) {
            SettingLabels[i] = SettingOptions[i].parent.GetChild(0);
        }
        LeaderboardElement = Resources.Load<GameObject>("LeaderboardElement");

        //Optimize Scene
        SettingsButtonGroup.SetActive(false);
        SettingsOptionGroup.SetActive(false);

        //Startup Animation
        if (!Application.isEditor && !Game.IntroShown) {
            Title.gameObject.SetActive(false);
            Matek.gameObject.SetActive(false);
            Facebook.gameObject.SetActive(false);
            FacebookUser.gameObject.SetActive(false);
            StartButton.gameObject.SetActive(false);
            MainButtonGroup.gameObject.SetActive(false);
            OpenParticles();
        }
    }
	
	void Update () {
        //Update Colors
        timeInCycle = (timeInCycle + Time.deltaTime) % 14;
        Color newColor = Color.HSVToRGB(timeInCycle / 14, .64f, 1);
        foreach(Image i in Animatable) {
            i.color = newColor;
        }
        foreach (Text i in AnimatableLeader) {
            i.color = newColor;
        }
        Title.color = newColor;
        Matek.color = newColor;
        FacebookUser.color = newColor;

        //Rotate Loading
        LoadingImage.transform.rotation = Quaternion.Euler(0, 0, LoadingImage.transform.rotation.eulerAngles.z + 180 * Time.deltaTime);

        //Update Particles with drift; Color from color by lifetime 
        if (BackgroundParticles == null || BackgroundParticles.Length < BackgroundParticleSystem.main.maxParticles)
            BackgroundParticles = new ParticleSystem.Particle[BackgroundParticleSystem.main.maxParticles];

        int numParticlesAlive = BackgroundParticleSystem.GetParticles(BackgroundParticles);

        for (int i = 0; i < numParticlesAlive; i++) {
            BackgroundParticles[i].startColor = newColor;
        }

        BackgroundParticleSystem.SetParticles(BackgroundParticles, numParticlesAlive);
    }
    public void OpenMain() {

        //Start Button
        StartButton.gameObject.SetActive(true);
        RectTransform startButtonRect = StartButton.GetComponent<RectTransform>();
        LeanTween.value(startButtonRect.anchorMin.x, 0.5f, 0.7f).setEaseOutCubic().setDelay(0.5f).setOnUpdate((float f) => {
            startButtonRect.anchorMin = new Vector2(f, 0.5f);
            startButtonRect.anchorMax = new Vector2(f, 0.5f);
        });

        //Main Button Group
        MainButtonGroup.SetActive(true);
        RectTransform mainButtonsRect = MainButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(mainButtonsRect.anchorMax.x, 1f, 0.7f).setEaseOutCubic().setDelay(0.5f).setOnUpdate((float f) => {
            mainButtonsRect.anchorMax = new Vector2(f, 1f);
        });

        //Title
        Title.gameObject.SetActive(true);
        RectTransform TitleRect = Title.GetComponent<RectTransform>();
        LeanTween.value(TitleRect.localPosition.y, 80f, 1f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
            TitleRect.localPosition = new Vector2(0, f);
        });

        //Matek
        Matek.gameObject.SetActive(true);
        LeanTween.value(Matek.gameObject, Matek.gameObject.transform.localPosition.y, -505f, 0.7f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
            Matek.gameObject.transform.localPosition = new Vector3(Matek.gameObject.transform.localPosition.x, f, 0);
        });

        //Facebook Button
        Facebook.gameObject.SetActive(true);
        LeanTween.value(Facebook.gameObject, Facebook.transform.localPosition.y, 460f, 0.7f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
            Facebook.transform.localPosition = new Vector3(Facebook.transform.localPosition.x, f, 0);
        });

        //Facebook Username
        FacebookUser.gameObject.SetActive(true);
        LeanTween.value(FacebookUser.gameObject, FacebookUser.transform.localPosition.y, 458f, 0.7f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
            FacebookUser.transform.localPosition = new Vector3(FacebookUser.transform.localPosition.x, f, 0);
        });
    }
    public void CloseMain(bool headerOut = false) {

        //Start Button
        RectTransform startButtonRect = StartButton.GetComponent<RectTransform>();
        LeanTween.value(startButtonRect.anchorMin.x, 1.2f, 0.7f).setEaseInCubic().setOnUpdate((float f) => {
            startButtonRect.anchorMin = new Vector2(f, 0.5f);
            startButtonRect.anchorMax = new Vector2(f, 0.5f);
        }).setOnComplete(() => { StartButton.gameObject.SetActive(false); });

        //Main Button Group
        RectTransform mainButtonsRect = MainButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(mainButtonsRect.anchorMax.x, 1.3f, 0.7f).setEaseInCubic().setOnUpdate((float f) => {
            mainButtonsRect.anchorMax = new Vector2(f, 1f);
        }).setOnComplete(() => {MainButtonGroup.SetActive(false); });

        //Title
        RectTransform TitleRect = Title.GetComponent<RectTransform>();
        LeanTween.value(TitleRect.localPosition.y, headerOut ? 690f : 435f, 1f).setEaseOutCubic().setOnUpdate((float f) => {
            TitleRect.localPosition = new Vector2(0, f);
        }).setOnComplete(() => {
            if (headerOut) Title.gameObject.SetActive(false);
        });

        //Matek
        LeanTween.value(Matek.gameObject, Matek.gameObject.transform.localPosition.y, -565f, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
            Matek.gameObject.transform.localPosition = new Vector3(Matek.gameObject.transform.localPosition.x, f, 0);
        }).setOnComplete(() => { Matek.gameObject.SetActive(false); });

        //Facebook Button
        LeanTween.value(Facebook.gameObject, Facebook.transform.localPosition.y, 580f, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
            Facebook.transform.localPosition = new Vector3(Facebook.transform.localPosition.x, f, 0);
        }).setOnComplete(() => { Facebook.gameObject.SetActive(false); });

        //Facebook Username
        LeanTween.value(FacebookUser.gameObject, FacebookUser.transform.localPosition.y, 565f, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
            FacebookUser.transform.localPosition = new Vector3(FacebookUser.transform.localPosition.x, f, 0);
        }).setOnComplete(() => { FacebookUser.gameObject.SetActive(false); });
    }
    public void OpenParticles() {
        var bps = BackgroundParticleSystem.gameObject;
        bps.transform.localPosition = new Vector3(0, 0, 1200);
        bps.transform.rotation = Quaternion.Euler(90, 90, 0);
        LeanTween.moveLocalZ(bps, 80, 5f).setEaseOutBack();
        LeanTween.rotate(bps, new Vector3(0, 0, 0), 3f).setDelay(2.5f).setEaseInOutCubic();
        LeanTween.value(1.5f, 5.5f, 0.2f).setDelay(5f).setOnUpdate(ChangeParticleSizes);
        LeanTween.value(5.5f, 1.5f, 0.5f).setDelay(5.2f).setOnUpdate(ChangeParticleSizes);

        Flash.gameObject.SetActive(true);
        LeanTween.value(0, 0.75f, 0.2f).setDelay(5f).setOnUpdate((f) => { 
            Flash.color = new Color(Matek.color.r, Matek.color.g, Matek.color.b, f);
        }).setOnComplete(() => {
            Title.gameObject.SetActive(true);
            Title.fontSize = 250;
            Matek.gameObject.SetActive(true);
            StartButton.gameObject.SetActive(true);
            MainButtonGroup.gameObject.SetActive(true);
            Facebook.gameObject.SetActive(true);
            FacebookUser.gameObject.SetActive(true);

            LeanTween.delayedCall(1.5f, () => {
                if (PlayerPrefs.GetInt("VN") < 10) {//9 current version 
                    PlayerPrefs.SetInt("VN", 9);
                    PlayerPrefs.Save();

                    Notice.OpenNotice(this.gameObject, "Patch Notes",
                        "New fancy animation at startup",
                        "UI animations accross game revamped (rip life)",
                        "Added Facebook intergration!",
                        "Leaderboard facebook intergration works!!!",
                        "Added two different game modes",
                        "Ignore the music settings...",
                        "I have patch notes popup :O",
                        "Many other thing I forgot!"
                    );
                }
            });
        });
        LeanTween.value(0.75f, 0, 0.4f).setDelay(5.25f).setOnUpdate((f) => {
            Flash.color = new Color(Matek.color.r, Matek.color.g, Matek.color.b, f);
        }).setOnComplete(() => { Flash.gameObject.SetActive(false); });
        //LeanTween.value(244, 234, 1.2f).setDelay(5f).setOnUpdate((f) => {
        //    Title.fontSize = (int)f;
        //});
    }
    private void ChangeParticleSizes(float f) {
        if (BackgroundParticles == null || BackgroundParticles.Length < BackgroundParticleSystem.main.maxParticles)
            BackgroundParticles = new ParticleSystem.Particle[BackgroundParticleSystem.main.maxParticles];
        int numParticlesAlive = BackgroundParticleSystem.GetParticles(BackgroundParticles);
        for (int i = 0; i < numParticlesAlive; i++) {
            BackgroundParticles[i].startSize = f;
        }
        BackgroundParticleSystem.SetParticles(BackgroundParticles, numParticlesAlive);
    }
    public void CloseParticles() {
        LeanTween.scale(BackgroundParticleSystem.gameObject, new Vector3(20, 20, 20), 1f).setEaseInOutCubic();

        LeanTween.moveZ(ModesOptionGroup, 8500, 1f).setEaseInOutCubic();

        if (ModesButtonGroup.activeSelf) {
            RectTransform modeButtonsRect = ModesButtonGroup.GetComponent<RectTransform>();
            LeanTween.value(modeButtonsRect.anchorMax.x, 1.1f, 0.7f).setEaseInCubic().setOnUpdate((float f) => {
                modeButtonsRect.anchorMax = new Vector2(f, 1f);
            }).setOnComplete(() => { ModesButtonGroup.SetActive(false); });
        }
    }
    public void OpenModes() {

        //Main Button Group
        RectTransform mainButtonsRect = MainButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(mainButtonsRect.anchorMax.x, 1.3f, 0.7f).setEaseInOutCubic().setOnUpdate((float f) => {
            mainButtonsRect.anchorMax = new Vector2(f, 1f);
        }).setOnComplete(() => { MainButtonGroup.SetActive(false); });

        //Title
        RectTransform TitleRect = Title.GetComponent<RectTransform>();
        LeanTween.value(TitleRect.localPosition.y, 392f, 1f).setEaseInOutCubic().setOnUpdate((float f) => {
            TitleRect.localPosition = new Vector2(0, f);
        });

        //Mode Button Group
        ModesButtonGroup.SetActive(true);
        RectTransform modeButtonsRect = ModesButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(modeButtonsRect.anchorMax.x, 1f, 0.7f).setEaseInOutCubic().setOnUpdate((float f) => {
            modeButtonsRect.anchorMax = new Vector2(f, 1f);
        });

        //Start Button
        RectTransform startButtonRect = StartButton.GetComponent<RectTransform>();
        LeanTween.value(startButtonRect.anchorMin.x, 1.2f, 0.7f).setEaseInOutCubic().setOnUpdate((float f) => {
            startButtonRect.anchorMin = new Vector2(f, 0.5f);
            startButtonRect.anchorMax = new Vector2(f, 0.5f);
        }).setOnComplete(() => { StartButton.gameObject.SetActive(false); });

        //Facebook Button
        LeanTween.value(Facebook.gameObject, Facebook.transform.localPosition.y, 580f, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
            Facebook.transform.localPosition = new Vector3(Facebook.transform.localPosition.x, f, 0);
        }).setOnComplete(() => { Facebook.gameObject.SetActive(false); });

        //Facebook Username
        LeanTween.value(FacebookUser.gameObject, FacebookUser.transform.localPosition.y, 565f, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
            FacebookUser.transform.localPosition = new Vector3(FacebookUser.transform.localPosition.x, f, 0);
        }).setOnComplete(() => { FacebookUser.gameObject.SetActive(false); });

        //Classic Buton
        ModesOptionGroup.SetActive(true);
        float startPos = -0.2f;
        RectTransform classicButtonRect = ModeClassicButton.GetComponent<RectTransform>();
        LeanTween.value(startPos, 0.5f, 0.7f).setDelay(0.3f).setEaseInOutCubic().setOnUpdate((float f) => {
            classicButtonRect.anchorMin = new Vector2(f, 0.5f);
            classicButtonRect.anchorMax = new Vector2(f, 0.5f);
        });
        classicButtonRect.anchorMin = new Vector2(startPos, 0.5f);
        classicButtonRect.anchorMax = new Vector2(startPos, 0.5f);

        //Powerups Button
        RectTransform PowerupsButtonRect = ModePowerupsButton.GetComponent<RectTransform>();
        LeanTween.value(startPos, 0.5f, 0.7f).setDelay(0.3f).setEaseInOutCubic().setOnUpdate((float f) => {
            PowerupsButtonRect.anchorMin = new Vector2(f, 0.5f);
            PowerupsButtonRect.anchorMax = new Vector2(f, 0.5f);
        });
        PowerupsButtonRect.anchorMin = new Vector2(startPos, 0.5f);
        PowerupsButtonRect.anchorMax = new Vector2(startPos, 0.5f);
    }
    public void CloseModes() {
        //Mode Button Group
        if (ModesButtonGroup.activeSelf) {
            RectTransform modeButtonsRect = ModesButtonGroup.GetComponent<RectTransform>();
            LeanTween.value(modeButtonsRect.anchorMax.x, 1.1f, 0.7f).setEaseInCubic().setOnUpdate((float f) => {
                modeButtonsRect.anchorMax = new Vector2(f, 1f);
            }).setOnComplete(() => { ModesButtonGroup.SetActive(false); });
        }

        //Classic Buton
        RectTransform classicButtonRect = ModeClassicButton.GetComponent<RectTransform>();
        LeanTween.value(classicButtonRect.anchorMin.x, -0.2f, 0.6f).setEaseInCubic().setOnUpdate((float f) => {
            classicButtonRect.anchorMin = new Vector2(f, 0.5f);
            classicButtonRect.anchorMax = new Vector2(f, 0.5f);
        });

        //Powerups Button
        RectTransform PowerupsButtonRect = ModePowerupsButton.GetComponent<RectTransform>();
        LeanTween.value(PowerupsButtonRect.anchorMin.x, -0.2f, 0.6f).setEaseInCubic().setOnUpdate((float f) => {
            PowerupsButtonRect.anchorMin = new Vector2(f, 0.5f);
            PowerupsButtonRect.anchorMax = new Vector2(f, 0.5f);
        }).setOnComplete(() => { ModesOptionGroup.SetActive(false); });
    }

    public void OpenSettings() {

        //Settings Button Group
        SettingsButtonGroup.SetActive(true);
        RectTransform settingsButtonsRect = SettingsButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(settingsButtonsRect.anchorMax.x, 1f, 0.7f).setEaseOutCubic().setDelay(0.5f).setOnUpdate((float f) => {
            settingsButtonsRect.anchorMax = new Vector2(f, 1f);
        });

        //Animate Setting Options
        SettingsOptionGroup.SetActive(true);
        int l = SettingLabels.Length;
        for (int i = 0; i < l; i++) {
            var t = i;
            SettingLabels[t].gameObject.SetActive(false);
            LeanTween.value(SettingLabels[t].gameObject, -1050, 0, 0.7f).setEaseOutCubic().setDelay(0.5f + t * 0.07f).setOnUpdate((f) => {
                SettingLabels[t].localPosition = new Vector3(f, 0, 0);
            }).setOnStart(()=> {SettingLabels[t].gameObject.SetActive(true); });
        }

        //Animate Setting Options
        for (int i = 0; i < l; i++) {
            var t = i;
            SettingOptions[t].gameObject.SetActive(false);
            LeanTween.value(SettingOptions[t].gameObject, -800 + t * 100, 0, 0.7f).setEaseOutCubic().setDelay(0.5f + t * 0.07f).setOnUpdate((f) => {
                SettingOptions[t].localPosition = new Vector3(45, f, 0);
            }).setOnStart(() => { SettingOptions[t].gameObject.SetActive(true); });
        }
    }

    public void CloseSettings() {

        //Settings Button Group
        RectTransform settingsButtonsRect = SettingsButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(settingsButtonsRect.anchorMax.x, 1.2f, 0.7f).setEaseInCubic().setOnUpdate((float f) => {
            settingsButtonsRect.anchorMax = new Vector2(f, 1f);
        }).setOnComplete(() => { SettingsButtonGroup.SetActive(false); });

        //Animate Setting Options
        SettingsOptionGroup.SetActive(true);
        int l = SettingLabels.Length;
        for (int i = 0; i < l; i++) {
            var t = i;
            LeanTween.value(SettingLabels[t].gameObject, SettingLabels[t].localPosition.x, -1050, 0.7f).setEaseOutCubic().setDelay((l - t) * 0.07f).setOnUpdate((f) => {
                SettingLabels[t].localPosition = new Vector3(f, 0, 0);
            }).setOnComplete(() => { SettingLabels[t].gameObject.SetActive(false); });
        }

        //Animate Setting Options
        for (int i = 0; i < l; i++) {
            var t = i;
            LeanTween.value(SettingOptions[t].gameObject, SettingOptions[t].localPosition.y, -800 + t * 100, 0.7f).setEaseOutCubic().setDelay((l- t) * 0.07f).setOnUpdate((f) => {
                SettingOptions[t].localPosition = new Vector3(45, f, 0);
            }).setOnComplete(() => { SettingOptions[t].gameObject.SetActive(false); });
        }
    }
    public void OpenLeaderboard() {

        //Leaderboard Button Group
        LeaderboardButtonGroup.SetActive(true);
        RectTransform leaderboardButtonsRect = LeaderboardButtonGroup.GetComponent<RectTransform>();
        LeanTween.value(leaderboardButtonsRect.anchorMax.x, 1f, 0.7f).setEaseOutCubic().setDelay(0.5f).setOnUpdate((float f) => {
            leaderboardButtonsRect.anchorMax = new Vector2(f, 1f);
        });

        LeaderboardGroup.SetActive(true);

        //Leaderboard Header
        LeanTween.value(LeaderboardHeader.transform.localPosition.y, 450, 0.7f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
            LeaderboardHeader.transform.localPosition = new Vector3(0, f, 0);
        });

        //Leaderboard TopBox
        LeanTween.value(TopBox.localPosition.x, 0, 0.7f).setEaseOutCubic().setDelay(0.85f).setOnUpdate((float f) => {
            TopBox.localPosition = new Vector3(f, TopBox.localPosition.y, 0);
        });

        //Leaderboard BottomBox
        LeanTween.value(BottomBox.localPosition.x, 0, 0.7f).setEaseOutCubic().setDelay(0.7f).setOnUpdate((float f) => {
            BottomBox.localPosition = new Vector3(f, BottomBox.localPosition.y, 0);
        });

        //Leaderboard LeftBox
        LeanTween.value(LeftBox.localPosition.y, 0, 0.7f).setEaseOutCubic().setDelay(0.55f).setOnUpdate((float f) => {
            LeftBox.localPosition = new Vector3(LeftBox.localPosition.x, f, 0);
        });

        //Leaderboard RightBox
        LeanTween.value(RightBox.localPosition.y, 0, 0.7f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
            RightBox.localPosition = new Vector3(RightBox.localPosition.x, f, 0);
        });

        //Loading Image
        LoadingImage.gameObject.SetActive(true);
        LoadingImage.transform.rotation = Quaternion.Euler(0, 0, 0);
        LoadingImage.transform.localScale = new Vector3(0, 0, 1);
        LeanTween.scale(LoadingImage.gameObject, new Vector3(1, 1, 1), 0.7f).setEaseInOutElastic().setDelay(0.553f);
    }
    public void CloseLeaderboard() {
        //ClearContainer
        int l = LeaderboardElementGroup.transform.childCount;
        if(LeaderboardElementGroup.transform.GetChild(0).name == "LoginPleaseFacebook(Clone)") {
            var e = LeaderboardElementGroup.transform.GetChild(0);
            LeanTween.moveX(e.gameObject, 1000, 0.7f).setDelay(0.1f).setEaseInOutCubic().setOnComplete(() => {
                Destroy(e.gameObject);
            });
        } else {
            for (int i = 0; i < l; i++) {
                var displacer = LeaderboardElementGroup.transform.GetChild(i).GetChild(0);
                //LeanTween.moveX(displacer.gameObject, -1000, 0.7f).setDelay(0.1f * i).setEaseInOutCubic().setOnComplete(() => {
                LeanTween.moveX(displacer.gameObject, 1000, 0.7f).setDelay(0.1f * (l - i)).setEaseInOutCubic().setOnComplete(() => {
                    Destroy(displacer.transform.parent.gameObject);
                });
            }
        }
        
        LeanTween.delayedCall(0.7f, () => {
            AnimatableLeader.Clear();

            //Leaderboard Button Group
            RectTransform leaderboardButtonsRect = LeaderboardButtonGroup.GetComponent<RectTransform>();
            LeanTween.value(leaderboardButtonsRect.anchorMax.x, 1.2f, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
                leaderboardButtonsRect.anchorMax = new Vector2(f, 1f);
            });

            //Leaderboard Header
            LeanTween.value(LeaderboardHeader.transform.localPosition.y, 570, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
                LeaderboardHeader.transform.localPosition = new Vector3(0, f, 0);
            }).setOnComplete(() => { LeaderboardGroup.SetActive(false); });

            //Leaderboard TopBox
            LeanTween.value(TopBox.localPosition.x, -1650, 0.7f).setEaseOutCubic().setOnUpdate((float f) => {
                TopBox.localPosition = new Vector3(f, TopBox.localPosition.y, 0);
            });

            //Leaderboard BottomBox
            LeanTween.value(BottomBox.localPosition.x, 1650, 0.7f).setEaseOutCubic().setDelay(0.15f).setOnUpdate((float f) => {
                BottomBox.localPosition = new Vector3(f, BottomBox.localPosition.y, 0);
            });

            //Leaderboard LeftBox
            LeanTween.value(LeftBox.localPosition.y, -920, 0.7f).setEaseOutCubic().setDelay(0.4f).setOnUpdate((float f) => {
                LeftBox.localPosition = new Vector3(LeftBox.localPosition.x, f, 0);
            });

            //Leaderboard RightBox
            LeanTween.value(RightBox.localPosition.y, 920, 0.7f).setEaseOutCubic().setDelay(0.45f).setOnUpdate((float f) => {
                RightBox.localPosition = new Vector3(RightBox.localPosition.x, f, 0);
            }).setOnComplete(() => { LeaderboardButtonGroup.SetActive(false); });

            //Loading Image
            LeanTween.scale(LoadingImage.gameObject, new Vector3(0, 0, 1), 0.7f).setEaseInOutElastic().setOnComplete(() => {
                LoadingImage.gameObject.SetActive(false);
            });
        });
    }

    public void LeaderboardStopLoading() {
        LeanTween.scale(LoadingImage.gameObject, new Vector3(0, 0, 1), 0.7f).setEaseInOutElastic().setOnComplete(() => {
            LoadingImage.gameObject.SetActive(false);
        });
    }

    public void LeaderboardStartLoading() {
        LoadingImage.gameObject.SetActive(true);
        LoadingImage.transform.rotation = Quaternion.Euler(0, 0, 0);
        LoadingImage.transform.localScale = new Vector3(0, 0, 1);
        LeanTween.scale(LoadingImage.gameObject, new Vector3(1, 1, 1), 0.7f).setEaseInOutElastic().setDelay(0.553f);
    }

    public void LoadLeaderboard() {
        //Results = new List<FBresult>() { FAKE RESULTS
        //    new FBresult(Resources.Load<Texture>("Profile1"), 1),
        //    new FBresult(Resources.Load<Texture>("Profile2"), 2),
        //    new FBresult(Resources.Load<Texture>("Profile3"), 3),
        //    new FBresult(Resources.Load<Texture>("Profile4"), 4),
        //    new FBresult(Resources.Load<Texture>("Profile5"), 5),
        //    new FBresult(Resources.Load<Texture>("Profile6"), 6),
        //    new FBresult(Resources.Load<Texture>("Profile7"), 7),
        //    new FBresult(Resources.Load<Texture>("Profile8"), 8)
        //};
        List<FBresult> Results = null;
        if (FB.IsLoggedIn) {
            FBGraph.GetFriends();
            FBGraph.GetScores(() => {
            Results = new List<FBresult>();
                for (int i = 0; i < Game.Scores.Count; i++) {
                    var entry = (Dictionary<string, object>)Game.Scores[i];
                    var user = (Dictionary<string, object>)entry["user"];
                    //((string) user["name"]).Split(new char[]{' '})[0]

                    Texture picture;
                    if (Game.FriendImages.TryGetValue((string)user["id"], out picture)) {
                        Results.Add(new FBresult(GraphUtil.GetScoreFromEntry(entry), picture));
                    } else {
                        Results.Add(new FBresult(GraphUtil.GetScoreFromEntry(entry), (string)user["id"]));
                    }

                }
                Results = Results.OrderByDescending(c => c.score).ToList();
                for (int i = 0; i < Results.Count; i++) {
                    var e = Instantiate(LeaderboardElement);
                    e.transform.SetParent(LeaderboardElementGroup.transform, false);

                    LeaderboardElement refer = e.GetComponent<LeaderboardElement>();
                    LeaderboardElementGroup.SetActive(true);
                    if (Results[i].image == null) {
                        refer.initalise(Results[i].score, Results[i].id);
                    } else {
                        refer.initalise(Results[i].score, Results[i].image);
                    }

                    refer.Displacer.position = new Vector3(-1000, 0, 0);
                    LeanTween.value(refer.Displacer.gameObject, refer.Displacer.localPosition.x, 0f, 1f).setEaseInOutCubic().setDelay(0.1f * (Results.Count - i)).setOnUpdate((f) => {
                        refer.Displacer.localPosition = new Vector3(f, 0, 0);
                    });
                }
                LeaderboardStopLoading();
            });
        } else {
            LeaderboardStopLoading();

            LeaderboardElementGroup.SetActive(true);
            var e = Instantiate(Resources.Load<GameObject>("LoginPleaseFacebook"));
            e.transform.SetParent(LeaderboardElementGroup.transform, false);
            var dif = e.transform.GetChild(0);
            AnimatableLeader.Add(dif.GetComponent<Text>());

            dif.transform.position = new Vector3(-1100, 0, 0);
            LeanTween.value(dif.gameObject, dif.localPosition.x, 0f, 1f).setEaseInOutCubic().setOnUpdate((f) => {
                dif.localPosition = new Vector3(f, 0, 0);
            });
        }
    }

    public struct FBresult {
        public Texture image;
        public int score;
        public string id;

        public FBresult(int _score, Texture _image) {
            image = _image;
            score = _score;
            id = null;
        }
        public FBresult(int _score, string _id) {
            image = null;
            score = _score;
            id = _id;
        }
    }
}
