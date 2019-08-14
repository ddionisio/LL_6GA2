using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Entity Data")]
public class GridEntityData : ScriptableObject {
    [Header("Info")]
    public Sprite icon;
    [M8.Localize]
    public string nameTextRef;
    [M8.Localize]
    public string descTextRef;

    [Header("Display")]
    public Material material; //material used during placement mode
    public Color color; //color used during placement

    [Header("Template")]
    public GameObject template;

    [SerializeField]
    string _shaderScalePulseVar = "_PulseScale";
    [SerializeField]
    string _shaderColorVar = "_Color";

    public int shaderScalePulseId { get; private set; }
    public int shaderColorId { get; private set; }

    public void RefreshShaderPropertyIds() {
        shaderScalePulseId = Shader.PropertyToID(_shaderScalePulseVar);
        shaderColorId = Shader.PropertyToID(_shaderColorVar);
    }

    void Awake() {
        RefreshShaderPropertyIds();
    }
}
