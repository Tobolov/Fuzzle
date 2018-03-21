using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notice : MonoBehaviour {
    
    public Text header;
    public Text body;
    public CanvasGroup group;
    Action callback;

    public static void OpenNotice(GameObject canvas, string header, Action callback, params string[] body) {
        var e = Instantiate(Resources.Load<GameObject>("Notice"));
        e.transform.SetParent(canvas.transform, false);

        Notice n = e.GetComponent<Notice>();
        n.group.alpha = 0;
        n.header.text = header;
        string raw = "";
        for(int i = 0; i < body.Length; i++) {
            raw += body[i] + "\n";
        }
        n.body.text = raw;
        n.callback = callback;

        LeanTween.value(0, 1, 0.7f).setOnUpdate((f) => {
            n.group.alpha = f;
        });
    }
    public static void OpenNotice(GameObject canvas, string header, params string[] body) {
        var e = Instantiate(Resources.Load<GameObject>("Notice"));
        e.transform.SetParent(canvas.transform, false);

        Notice n = e.GetComponent<Notice>();
        n.group.alpha = 0;
        n.header.text = header;
        string raw = "";
        for (int i = 0; i < body.Length; i++) {
            raw += body[i] + "\n";
        }
        n.body.text = raw;

        LeanTween.value(0, 1, 0.7f).setOnUpdate((f) => {
            n.group.alpha = f;
        });
    }

    public void CloseNotice() {
        LeanTween.value(1, 0, 0.3f).setOnUpdate((f) => {
            group.alpha = f;
        }).setOnComplete(() => {
            if (callback != null)
                callback();
            Destroy(this.gameObject);
        });
    }
}
