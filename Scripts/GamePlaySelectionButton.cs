using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamePlaySelectionButton : MonoBehaviour {

    [Header("Button References")]
    public Text TextField;
    public Image LockImage;

    [HideInInspector]
    private CutoutType type = CutoutType.Null;
    private static Dictionary<CutoutType, string> CutoutSymbolLookup = new Dictionary<CutoutType, string>() {
        {CutoutType.Null, "." },
        {CutoutType.L, "L" },
        {CutoutType.H, "H" },
        {CutoutType.T, "T" },
        {CutoutType.E, "E" },
        {CutoutType.F, "F" },
        {CutoutType.Exclaim, "!" },
        {CutoutType._Flash, "." },
        {CutoutType._Frost, "." },
        {CutoutType._Nuke, "." },
        {CutoutType._Baby, "." },
        {CutoutType._Random, "." },
        {CutoutType._Reverse, "." },
        {CutoutType._Points, "." },
        {CutoutType.P, "P" },
        {CutoutType.N, "N" },
        {CutoutType.V, "V" },
        {CutoutType.A, "A" },
        {CutoutType.S, "S" },
    };

    void Start() {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPointerClickDelegate((PointerEventData)data); });
        trigger.triggers.Add(entry);

        LockImage.transform.localScale = new Vector3(0, 0, 0);
        LockImage.transform.rotation = Quaternion.Euler(0, 0, 0);
        TextField.transform.localScale = new Vector3(0, 0, 0);
        TextField.transform.rotation = Quaternion.Euler(0, 0, 0);

    }
    public void OnPointerClickDelegate(PointerEventData data) {
        GamePlay.self.ButtonSelectionInput(type, this);
    }   

    public void ChangeCutoutType(CutoutType newType, float delay) {
        if ((int)type <= GamePlayControls.IndexPowerupNull) {
            LeanTween.scale(LockImage.gameObject, new Vector3(0, 0, 0), delay);
            LeanTween.rotate(LockImage.gameObject, new Vector3(0, 0, 720), delay).setOnStart(() => {
                TextField.transform.rotation = Quaternion.Euler(0, 0, 0);
            }).setOnComplete(() => {
                TextField.text = CutoutSymbolLookup[newType];
                type = newType;
                LockImage.gameObject.SetActive(false);
            });
        } else {
            LeanTween.scale(TextField.gameObject, new Vector3(0, 0, 0), delay);
            LeanTween.rotate(TextField.gameObject, new Vector3(0, 0, 720), delay).setOnStart(() => {
                TextField.transform.rotation = Quaternion.Euler(0, 0, 0);
            }).setOnComplete(() => {
                TextField.text = CutoutSymbolLookup[newType];
                type = newType;
                TextField.gameObject.SetActive(false);
            });
        }
        
        if((int)newType <= GamePlayControls.IndexPowerupNull) {
            LockImage.sprite = GamePlayControls.PowerupLookupDictionary[newType];
            LeanTween.scale(LockImage.gameObject, new Vector3(1, 1, 1), delay).setDelay(delay).setOnStart(() => {
                LockImage.gameObject.SetActive(true);
            });
            LeanTween.rotate(LockImage.gameObject, new Vector3(0, 0, 720), delay).setDelay(delay).setOnStart(() => {
                TextField.transform.rotation = Quaternion.Euler(0, 0, 0);
            });
        } else {
            LeanTween.scale(TextField.gameObject, new Vector3(1, 1, 1), delay).setDelay(delay).setOnStart(() => {
                TextField.gameObject.SetActive(true);
            });
            LeanTween.rotate(TextField.gameObject, new Vector3(0, 0, 720), delay).setDelay(delay).setOnStart(() => {
                TextField.transform.rotation = Quaternion.Euler(0, 0, 0);
            });
        }
    } 
}
