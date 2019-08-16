using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityDeckWidget : MonoBehaviour {
    [Header("Template")]
    public GridEntityCardWidget cardTemplate;

    [Header("Display")]
    public Transform container;
    public GridEntityCardWidget cardDrag; //use for dragging

    void OnDisable() {
        
    }

    void OnEnable() {
        
    }

    void Awake() {
        cardDrag.gameObject.SetActive(false);

        //setup cards
    }
}
