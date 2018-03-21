using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour {
    public Transform Displacer;
    public Image Border;
    public RawImage Profile;
    public Text Score;
    public Image Left;
    public Image Right;
    public Image Bottom;

    public string id;
    Texture picture;

    public void initalise(int score, string _id) {
        Profile.texture = Resources.Load<Texture>("Profile5");
        Score.text = score.ToString();
        id = _id;
        StartCoroutine(tryUpdateImage());
    }
    public void initalise(int score, Texture image) {
        Profile.texture = image;
        Score.text = score.ToString();
    }
    IEnumerator tryUpdateImage() {
        int num = 1;
        while (true) {
            yield return new WaitForSeconds(num * num * 0.05f);
            num++;

            if (Game.FriendImages.TryGetValue(id, out picture)) {
                Profile.texture = picture;
                break;
            }
        }
    }
}
