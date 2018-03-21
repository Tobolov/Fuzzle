using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Facebook.Unity;
public class GamePlayControls : MonoBehaviour {

    [Header("Button Groups")]
    public GameObject GroupAll;
    public GameObject GroupTop;
    public GameObject GroupBottom;
    public GameObject GroupLeft;
    public GameObject GroupRight;
    public GameObject PowerupGroup;

    [Header("Other Button References")]
    public Text ScoreTextField;
    public GameObject MenuButton;
    public CanvasGroup RoundEndGroup;
    public Image PowerupSymbol;
    public Image PowerupBar;
    public Image PowerupTop;
    public Image PowerupRight;
    public CanvasGroup Menu;
    public Button Continue;
    public Button Quit;

    [Header("External References")]
    public Transform Lines;

    [HideInInspector]
    private Image[] GamePlayButtonImages;
    private GamePlaySelectionButton[] GamePlayButtons;
    public static GamePlayControls self;
    public static Dictionary<CutoutType, Sprite> PowerupLookupDictionary;
    public static List<CutoutType> Powerups;
    public static Dictionary<CutoutType, float> PowerupLookupTime;
    public static int IndexPowerupNull;

    [HideInInspector]
    int[,] ButtonGroupIndexs = new int[,] {
        {0, 4 },
        {5, 9 },
        {10, 13 },
        {14, 17 }
    };

    void Start () {
        self = this;

        //Group all buttons in Array
        var temp = new List<Image>();
        foreach (Image i in GroupAll.GetComponentsInChildren<Image>())
            if (i != null && i.tag != "IgnoreUIScan")
                temp.Add(i);
        GamePlayButtonImages = temp.ToArray();
        GamePlayButtons = new GamePlaySelectionButton[GamePlayButtonImages.Length];

        //Get GamePlaySelecitonButton script from all Buttons
        for(int i = 0; i < GamePlayButtonImages.Length; i++) {
            GamePlayButtons[i] = GamePlayButtonImages[i].gameObject.GetComponent<GamePlaySelectionButton>();
        }

        //Set Button groups out of vison
        GroupTop.transform.localPosition = new Vector2(0, 450 + 200);
        GroupBottom.transform.localPosition = new Vector2(0, -450 - 200);
        GroupLeft.transform.localPosition = new Vector2(-863 - 200, 0);
        GroupRight.transform.localPosition = new Vector2(863 + 200, 0);
        PowerupGroup.SetActive(false);
        PowerupGroup.transform.position = new Vector3(0, -97, 0);

        //Set Lines and UI out of vision
        Lines.localPosition = new Vector3(0, 0, 300);
        ScoreTextField.transform.localPosition = new Vector3(-787.4f, 560, 0);
        MenuButton.transform.localPosition = new Vector3(856.1f, 580, 0);


        //Button functions
        MenuButton.GetComponent<Button>().onClick.AddListener(() => {
            Game.GameActive = false;
            Menu.gameObject.SetActive(true);
            LeanTween.value(0, 1, 0.4f).setOnUpdate((f) => {
                Menu.alpha = f;
            });
        });
        Continue.onClick.AddListener(() => {
            LeanTween.value(1, 0, 0.4f).setOnUpdate((f) => {
                Menu.alpha = f;
            }).setOnComplete(() => {
                Game.GameActive = true;
                Menu.gameObject.SetActive(false);
            });
        });
        Quit.onClick.AddListener(() => {
            if (FB.IsLoggedIn) {
                FBShare.PostScore(Mathf.Max(Game.HighScore, Game.Score));
            }
            Game.LoadByIndex(0);
        });

        OpenWallsAndUI();
        LeanTween.delayedCall(1f, OpenDiamonds);
    }
	
	void Update () {
        
	}


    public void SwitchButtonSymbols(CutoutType next, float delay = 0.4f) {
        List<CutoutType> NextCutoutList = new List<CutoutType>() { next };

        if (GamePlay.self.CurrentPowerup == CutoutType._Baby) {
            //Add Symbols
            Cutout.CutoutTypes.Shuffle();
            bool HitDouble = false;
            for (int i = 0; i < (HitDouble ? 4 : 3); i++) { //add 3 more symbols 
                if (Cutout.CutoutTypes[i] == next) {
                    HitDouble = true;
                } else {
                    NextCutoutList.Add(Cutout.CutoutTypes[i]);
                }
            }

            //Apply Symbols
            NextCutoutList.Shuffle();
            for(int i = 0; i < 4; i++) {
                for(int j = ButtonGroupIndexs[i,0]; j <= ButtonGroupIndexs[i, 1]; j++) {
                    GamePlayButtons[j].ChangeCutoutType(NextCutoutList[i], delay);
                }
            }

        } else {
            //Add Symbols
            Cutout.CutoutTypes.Shuffle();
            bool HitDouble = false;
            for (int i = 0; i < (HitDouble ? 6 : 5); i++) { //add 5 more symbols 
                if (Cutout.CutoutTypes[i] == next) {
                    HitDouble = true;
                } else {
                    NextCutoutList.Add(Cutout.CutoutTypes[i]);
                }
            }

            //Add Powerup?!
            if (Game.gameMode == GameMode.Powerups && GamePlay.self.RemainingPowerup <= -4 && Random.Range(0, 0) == 0) { //4 seconds(VALUE IN SECONDS) from last powerup and 1/7 chance
                NextCutoutList.Add(Powerups[Random.Range(0, Powerups.Count)]);
            }

            //Fill Remaining Space
            for (int i = 0; NextCutoutList.Count < 18; i++) {
                NextCutoutList.Add(CutoutType.Null);
            }

            //Apply Symbols
            NextCutoutList.Shuffle();
            for (int i = 0; i < NextCutoutList.Count(); i++) {
                GamePlayButtons[i].ChangeCutoutType(NextCutoutList[i], delay);
            }
        }
    }

    public void UpdateButtonColors(Color c) {
        for(int i = 0; i < GamePlayButtonImages.Length; i++) {
            GamePlayButtonImages[i].color = c;
        }
        PowerupBar.color = c;
        PowerupSymbol.color = c;
        PowerupTop.color = c;
        PowerupRight.color = c;
    }

    public void OpenWallsAndUI() {
        LeanTween.moveLocalZ(Lines.gameObject, 0, 1f);
        LeanTween.moveLocalY(ScoreTextField.gameObject, 440f, 0.7f).setEaseInOutCubic().setDelay(0.4f);
        LeanTween.moveLocalY(MenuButton.gameObject, 450f, 0.7f).setEaseInOutCubic().setDelay(0.4f);
    }

    public void OpenDiamonds() { 
        GroupBottom.SetActive(true);
        GroupBottom.GetComponent<HorizontalLayoutGroup>().spacing = -1100;
        LeanTween.value(GroupBottom, -1100, 1, 1).setDelay(0.7f).setEaseOutBounce().setOnUpdate((float f) => {
            if (GroupBottom == null)
                return;
            GroupBottom.GetComponent<HorizontalLayoutGroup>().spacing = f;
        });
        LeanTween.value(GroupBottom, GroupBottom.transform.localPosition, new Vector3(0, -450), 1).setEaseInCubic().setOnUpdate((Vector3 v) => {
            if (GroupBottom == null)
                return;
            GroupBottom.transform.localPosition = v;
        });
        GroupTop.SetActive(true);
        GroupTop.GetComponent<HorizontalLayoutGroup>().spacing = -1100;
        LeanTween.value(GroupTop, -1100, 1, 1).setDelay(0.7f).setEaseOutBounce().setOnUpdate((float f) => {
            if (GroupTop == null)
                return;
            GroupTop.GetComponent<HorizontalLayoutGroup>().spacing = f;
        });
        LeanTween.value(GroupTop, GroupTop.transform.localPosition, new Vector3(0, 450), 1).setEaseInCubic().setOnUpdate((Vector3 v) => {
            if (GroupTop == null)
                return;
            GroupTop.transform.localPosition = v;
        });
        GroupLeft.SetActive(true);
        GroupRight.SetActive(true);
        GroupLeft.GetComponent<VerticalLayoutGroup>().spacing = -800;
        GroupRight.GetComponent<VerticalLayoutGroup>().spacing = -800;
        LeanTween.value(-1100, 1, 1).setDelay(0.7f).setEaseOutBounce().setOnUpdate((float f) => {
            if (GroupLeft == null)
                return;
            GroupLeft.GetComponent<VerticalLayoutGroup>().spacing = f;
            GroupRight.GetComponent<VerticalLayoutGroup>().spacing = f;
        });
        LeanTween.value(GroupLeft, GroupLeft.transform.localPosition, new Vector3(-863, 0), 1).setEaseInCubic().setOnUpdate((Vector3 v) => {
            if (GroupLeft == null)
                return;
            GroupLeft.transform.localPosition = v;
        });
        LeanTween.value(GroupRight, GroupRight.transform.localPosition, new Vector3(863, 0), 1).setEaseInCubic().setOnUpdate((Vector3 v) => {
            if (GroupRight == null)
                return;
            GroupRight.transform.localPosition = v;
        });
    }
    public void CloseDiamonds() {
        LeanTween.value(1, -1100, 1).setEaseOutBounce().setOnUpdate((float f) => {
            if (GroupBottom == null)
                return;
            GroupBottom.GetComponent<HorizontalLayoutGroup>().spacing = f;
            GroupTop.GetComponent<HorizontalLayoutGroup>().spacing = f;
        });
        LeanTween.value(1, -800, 1).setEaseOutBounce().setOnUpdate((float f) => {
            if (GroupLeft == null)
                return;
            GroupLeft.GetComponent<VerticalLayoutGroup>().spacing = f;
            GroupRight.GetComponent<VerticalLayoutGroup>().spacing = f;
        });

        LeanTween.value(GroupTop, GroupTop.transform.localPosition, new Vector3(0, 450 + 200), 1).setEaseInCubic().setDelay(0.7f).setOnUpdate((Vector3 v) => {
            if (GroupTop == null)
                return;
            GroupTop.transform.localPosition = v;
        });
        LeanTween.value(GroupBottom, GroupBottom.transform.localPosition, new Vector3(0, -450 - 200), 1).setEaseInCubic().setDelay(0.7f).setOnUpdate((Vector3 v) => {
            if (GroupBottom == null)
                return;
            GroupBottom.transform.localPosition = v;
        });
        LeanTween.value(GroupLeft, GroupLeft.transform.localPosition, new Vector3(-863 - 200, 0), 1).setEaseInCubic().setDelay(0.7f).setOnUpdate((Vector3 v) => {
            if (GroupLeft == null)
                return;
            GroupLeft.transform.localPosition = v;
        });
        LeanTween.value(GroupRight, GroupRight.transform.localPosition, new Vector3(863 + 200, 0), 1).setEaseInCubic().setDelay(0.7f).setOnUpdate((Vector3 v) => {
            if (GroupRight == null)
                return;
            GroupRight.transform.localPosition = v;
        }).setOnComplete(() => {
            if (GroupBottom == null)
                return;
            GroupBottom.SetActive(false);
            GroupTop.SetActive(false);
            GroupLeft.SetActive(false);
            GroupRight.SetActive(false);
        });
    }
    public void OpenPowerup() {
        PowerupGroup.SetActive(true);
        LeanTween.value(PowerupGroup, PowerupGroup.transform.position.y, 0, 0.12f).setEaseOutCubic().setOnUpdate((v) => {
            PowerupGroup.transform.localPosition = new Vector3(0, v, 0);
        });
        }
    public void ClosePowerup() {
        LeanTween.value(PowerupGroup, PowerupGroup.transform.position.y, -97f, 0.12f).setEaseInCubic().setOnUpdate((v) => {
            PowerupGroup.transform.localPosition = new Vector3(0, v, 0);
        }).setOnComplete(() => {
            PowerupGroup.SetActive(false);
        });
    }

    public void OpenStartMenu() {
        
    }
}
