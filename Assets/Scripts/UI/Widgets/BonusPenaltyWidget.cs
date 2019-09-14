using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusPenaltyWidget : MonoBehaviour {

    public Text text;

    void OnEnable() {
        text.text = (-GameData.instance.bonusPenalty).ToString();
    }
}
