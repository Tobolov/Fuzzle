using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using VolumetricLines;
using Facebook.Unity;

public class GamePlay : MonoBehaviour {

    [Header("Graphics")]
    public int SecondsPerCycle = 30;

    [Header("Hall Renderer References")]
    public VolumetricLineBehavior LineTopLeft;
    public VolumetricLineBehavior LineTopRight;
    public VolumetricLineBehavior LineBottomLeft;
    public VolumetricLineBehavior LineBottomRight;

    [Header("Hall Particle References")]
    public ParticleSystem PulseTopLeft;
    public ParticleSystem PulseTopRight;
    public ParticleSystem PulseBottomLeft;
    public ParticleSystem PulseBottomRight;

    [Header("Other References")]
    public FxPro PostProcessingEffects;

    [HideInInspector]
    public float secondsIntoCycle = 0;          //Color Cycle
    private VolumetricLineBehavior[] WallComponenets;
    private ParticleSystem[] PulseComponenets;
    private int _Score = 0;                     //Score Variable
    private int SpawnNum = 0;                   //The current number of spawned walls
    private Cutout Aggressor;
    public static GamePlay self;

    [HideInInspector]
    public CutoutType CurrentPowerup;
    public float RemainingPowerup;
    private float MaxPowerupTime;

    //Reused Objects
    ParticleSystem.Particle[] R_PulseParticlesToUpdate;

    [HideInInspector]
    public int Score {
        get {
            return _Score;
        }
        set {
            _Score = value;
            GamePlayControls.self.ScoreTextField.text = "Score: " + _Score;
        }
    }

    void Start() {
        self = this;

        Game.IntroShown = true;
        Game.GameActive = true;
        Game.InitaliseGameSettings(PostProcessingEffects, Game.Settings);

        //Initalise Consts
        Cutout.CutoutTypes = new List<CutoutType> { CutoutType.L, CutoutType.H, CutoutType.T, CutoutType.E, CutoutType.F, CutoutType.Exclaim, CutoutType.P, CutoutType.N, CutoutType.V, CutoutType.A, CutoutType.S };
        Cutout.CutoutToGameobject = new Dictionary<CutoutType, GameObject> {
            { CutoutType.L, Resources.Load<GameObject>("Aggressors/Cutout_L") },
            { CutoutType.H, Resources.Load<GameObject>("Aggressors/Cutout_H") },
            { CutoutType.T, Resources.Load<GameObject>("Aggressors/Cutout_T") },
            { CutoutType.E, Resources.Load<GameObject>("Aggressors/Cutout_E") },
            { CutoutType.F, Resources.Load<GameObject>("Aggressors/Cutout_F") },
            { CutoutType.Exclaim, Resources.Load<GameObject>("Aggressors/Cutout_Exclaim") },
            { CutoutType.P, Resources.Load<GameObject>("Aggressors/Cutout_P") },
            { CutoutType.N, Resources.Load<GameObject>("Aggressors/Cutout_N") },
            { CutoutType.V, Resources.Load<GameObject>("Aggressors/Cutout_V") },
            { CutoutType.A, Resources.Load<GameObject>("Aggressors/Cutout_A") },
            { CutoutType.S, Resources.Load<GameObject>("Aggressors/Cutout_S") },
        };

        GamePlayControls.Powerups = new List<CutoutType> { CutoutType._Flash, CutoutType._Frost, CutoutType._Nuke, CutoutType._Baby, CutoutType._Random, CutoutType._Reverse, CutoutType._Points };
        GamePlayControls.PowerupLookupDictionary = new Dictionary<CutoutType, Sprite> {
            { CutoutType.Null, Resources.Load<Sprite>("Powerups/lock") },
            { CutoutType._Frost, Resources.Load<Sprite>("Powerups/frost") },
            { CutoutType._Flash, Resources.Load<Sprite>("Powerups/flash") },
            { CutoutType._Nuke, Resources.Load<Sprite>("Powerups/nuke") },
            { CutoutType._Baby, Resources.Load<Sprite>("Powerups/baby") },
            { CutoutType._Random, Resources.Load<Sprite>("Powerups/random") },
            { CutoutType._Reverse, Resources.Load<Sprite>("Powerups/reverse") },
            { CutoutType._Points, Resources.Load<Sprite>("Powerups/points") },
        };
        GamePlayControls.PowerupLookupTime = new Dictionary<CutoutType, float> {
            { CutoutType._Flash, 10},
            { CutoutType._Frost, 6},
            { CutoutType._Nuke, 0},
            { CutoutType._Baby, 7},
            { CutoutType._Random, 0},
            { CutoutType._Reverse, 5},
            { CutoutType._Points, 0},
        };
        GamePlayControls.IndexPowerupNull = 7;

        //Create Group Array
        WallComponenets = new VolumetricLineBehavior[] { LineTopLeft, LineTopRight, LineBottomLeft, LineBottomRight };
        PulseComponenets = new ParticleSystem[] { PulseTopLeft, PulseTopRight, PulseBottomLeft, PulseBottomRight };

        //Initalise Vars
        secondsIntoCycle = Random.Range(0, SecondsPerCycle);
        RemainingPowerup = 0;
        CurrentPowerup = CutoutType.Null;

        //optimization
        GamePlayControls.self.RoundEndGroup.gameObject.SetActive(false);
        R_PulseParticlesToUpdate = new ParticleSystem.Particle[PulseTopLeft.main.maxParticles];

        StartCoroutine(InitAggressor());
    }

    void Update() {
        //Update Seconds In Cycle
        secondsIntoCycle = (secondsIntoCycle + Time.deltaTime) % SecondsPerCycle;

        //Update Vertex Colors
        Color newVerexColor = Color.HSVToRGB(secondsIntoCycle / SecondsPerCycle, .64f, 1);
        SetVertexColours(newVerexColor);

        //Update Powerup
        if (Game.GameActive) {
            RemainingPowerup -= Time.deltaTime;
            if (RemainingPowerup <= 0 && CurrentPowerup != CutoutType.Null) {
                GamePlayControls.self.ClosePowerup();

                //End Powerup Changes 
                if (CurrentPowerup == CutoutType._Frost) {
                    Aggressor.velocity *= 1.8f;
                }
                if (CurrentPowerup == CutoutType._Baby) {
                    StartCoroutine(NextAggressor(Aggressor.transform.position.z, false));
                    Destroy(Aggressor.gameObject);
                }
                if (CurrentPowerup == CutoutType._Reverse) {
                    Aggressor.velocity *= -1;
                }

                CurrentPowerup = CutoutType.Null;

            } else {
                GamePlayControls.self.PowerupBar.fillAmount = RemainingPowerup / MaxPowerupTime;
            }
        }
    }

    private void OnApplicationFocus(bool focus) {
        if(!focus&& FB.IsLoggedIn) {
            FBShare.PostScore(Mathf.Max(Game.HighScore, Game.Score));
        }
    }

    void SetVertexColours(Color color) {
        for (int i = 0; i < WallComponenets.Length; i++) {
            WallComponenets[i].LineColor = color;
        }
        if (Aggressor != null) {
            Renderer rend = Aggressor.GetComponent<Renderer>();
            rend.material.SetColor(Game._V_WIRE_COLOR, color);
        }
        for(int i = 0; i < PulseComponenets.Length; i++) {
            int numParticlesAlive = PulseComponenets[i].GetParticles(R_PulseParticlesToUpdate);
            for (int j = 0; j < numParticlesAlive; j++) {
                R_PulseParticlesToUpdate[j].startColor = color;
            }
            PulseComponenets[i].SetParticles(R_PulseParticlesToUpdate, numParticlesAlive);

            var main = PulseComponenets[i].main;
            main.startColor = color;
        }
        GamePlayControls.self.UpdateButtonColors(color);
    }

    public void ButtonSelectionInput(CutoutType type, GamePlaySelectionButton caller) {
        if (Aggressor == null || Aggressor.transform.position.z >= 112 || !Game.GameActive)
            return;

        if((int)type <= GamePlayControls.IndexPowerupNull && type != CutoutType.Null) {
            caller.ChangeCutoutType(CutoutType.Null, 0.2f);
            if (type == CutoutType._Random) {
                type = GamePlayControls.Powerups[Random.Range(0, GamePlayControls.Powerups.Count)];
                CurrentPowerup = type;
                RemainingPowerup = GamePlayControls.PowerupLookupTime[CurrentPowerup];
                MaxPowerupTime = RemainingPowerup;
            }
            CurrentPowerup = type;
            RemainingPowerup = GamePlayControls.PowerupLookupTime[CurrentPowerup];
            MaxPowerupTime = RemainingPowerup;

            //Initalise UI
            if (MaxPowerupTime != 0) {
                GamePlayControls.self.OpenPowerup();
                GamePlayControls.self.PowerupSymbol.sprite = GamePlayControls.PowerupLookupDictionary[type];
                GamePlayControls.self.PowerupBar.fillAmount = 1;
            }

            //Immediate Changes
            if(type == CutoutType._Frost) {
                Aggressor.velocity /= 1.8f;
            }
            if(type == CutoutType._Nuke) {
                Aggressor.transform.position = new Vector3(Aggressor.transform.position.x, Aggressor.transform.position.y, 120);
            }
            if(type == CutoutType._Baby) {
                StartCoroutine(NextAggressor(Aggressor.transform.position.z));
                Destroy(Aggressor.gameObject);
            }
            if(type == CutoutType._Points) {
                Score += 5;
            }
            if(type == CutoutType._Reverse) {
                Aggressor.velocity *= -1;
            }
            return;
        }

        if (Aggressor.type == type) {
            Score++;
            Aggressor.AnimateDestruction();                 //Animation set to occur outside object

            float lastZ = Aggressor.transform.position.z;
            StartCoroutine(NextAggressor(lastZ));
        } else {
            LooseRound();
        }
    }

    public void LooseRound() {
        GamePlayControls.self.CloseDiamonds();
        GamePlayControls.self.OpenPowerup();
        Game.GameActive = false;

        CanvasGroup roundEndGroup = GamePlayControls.self.RoundEndGroup;
        roundEndGroup.gameObject.SetActive(true);
        roundEndGroup.interactable = true;
        LeanTween.alphaCanvas(roundEndGroup, 1, 0.5f);

        CurrentPowerup = CutoutType.Null;
        RemainingPowerup = 0;
        GamePlayControls.self.PowerupGroup.SetActive(false);

        Game.Score = Mathf.Max(Game.Score, Score);

        FBAppEvents.GameComplete(Score);
    }

    public void TryAgain() {
        GamePlayControls gpc = GamePlayControls.self;
        CanvasGroup roundEndGroup = gpc.RoundEndGroup;
        roundEndGroup.gameObject.SetActive(false);
        roundEndGroup.interactable = false;
        roundEndGroup.alpha = 0;
        Score = 0;
    
        Destroy(Aggressor.gameObject);

        LeanTween.cancelAll(false);
        Game.GameActive = true;
        gpc.OpenDiamonds();

        StartCoroutine(InitAggressor());
    }

    IEnumerator InitAggressor() {
        yield return 0; //Skip frame

        CutoutType type = Cutout.CutoutTypes[Random.Range(0, Cutout.CutoutTypes.Count)];
        Aggressor = Instantiate(Cutout.CutoutToGameobject[type]).GetComponent<Cutout>(); //-1 to ignore null cutout
        Aggressor.transform.SetParent(this.transform, true);
        Aggressor.transform.position = new Vector3(0, 0, 120);
        Aggressor.velocity = SpawnNum * 0.6f + 14;
        Aggressor.type = type;
        Renderer rend = Aggressor.GetComponent<Renderer>();
        rend.material.SetColor(Game._V_WIRE_COLOR, Color.HSVToRGB(secondsIntoCycle / SecondsPerCycle, .64f, 1));
        Aggressor.gameObject.SetActive(false);

        yield return 0;

        Aggressor.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        GamePlayControls.self.SwitchButtonSymbols(type);
    }

    IEnumerator NextAggressor(float lastZ, bool doPushback = true) {
        yield return 0; //Skip frame

        CutoutType type = Cutout.CutoutTypes[Random.Range(0, Cutout.CutoutTypes.Count)];
        Aggressor = Instantiate(Cutout.CutoutToGameobject[type]).GetComponent<Cutout>(); //-1 to ignore null cutout
        Aggressor.transform.SetParent(this.transform, true);
        Aggressor.velocity = SpawnNum * 0.6f + 14;
        if (CurrentPowerup == CutoutType._Frost)
            Aggressor.velocity /= 1.8f;
        if (CurrentPowerup == CutoutType._Reverse)
            Aggressor.velocity *= -1;
        float pushback = (Mathf.Sqrt(3 / (SpawnNum + 3)) + 0.4f) * Mathf.Abs(Aggressor.velocity);
        Aggressor.transform.position = new Vector3(0, 0, lastZ + (doPushback ? pushback : 0));
        Aggressor.type = type;
        Renderer rend = Aggressor.GetComponent<Renderer>();
        rend.material.SetColor(Game._V_WIRE_COLOR, Color.HSVToRGB(secondsIntoCycle / SecondsPerCycle, .64f, 1));

        GamePlayControls.self.SwitchButtonSymbols(type, CurrentPowerup == CutoutType._Flash ? 0.06f : 0.2f); 
    }
}
