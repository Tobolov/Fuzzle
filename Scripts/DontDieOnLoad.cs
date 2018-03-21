using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDieOnLoad : MonoBehaviour {
    private static DontDieOnLoad self;

    void Awake() {
        if(self != null && this != self) {
            Destroy(this.gameObject);
            return;
        }
        else {
            self = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
