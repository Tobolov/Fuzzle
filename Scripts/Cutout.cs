using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutout : MonoBehaviour {

    [Header("Cutout Properties")]
    public CutoutType type;

    [HideInInspector]
    public float velocity = 10;
    public static List<CutoutType> CutoutTypes; //Loaded in GamePlay
    public static Dictionary<CutoutType, GameObject> CutoutToGameobject; //Loaded in GamePlay

	void Update () {
        if (!Game.GameActive)
            return;

        //Move towards camera
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - velocity * Time.deltaTime);
        if(transform.position.z <= -14) {
            GamePlay.self.LooseRound();
        }
	}

    public void AnimateDestruction() {
        Destroy(this.gameObject);
    }
}


public enum CutoutType {
    Null,
    _Flash,
    _Frost,
    _Nuke,
    _Baby,
    _Random,
    _Reverse,
    _Points,
    L,
    H,
    T,
    E,
    F,
    Exclaim,
    P,
    N,
    V,
    A,
    S
}
