using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Entity Data")]
public class GridEntityData : ScriptableObject {
    public const string poolGroup = "entity";
    public const int poolCapacity = 16;

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
    string _shaderPulseScaleVar = "_PulseScale";
    [SerializeField]
    string _shaderPulseColorVar = "_PulseColor";
    [SerializeField]
    string _shaderColorVar = "_Color";    

    public int shaderPulseScaleId { get; private set; }
    public int shaderPulseColorId { get; private set; }
    public int shaderColorId { get; private set; }

    public Color pulseColor { get; private set; }

    private M8.PoolController mPool;
    private M8.GenericParams mSpawnParms;

    public GridEntity Spawn(GridCell cell, GridCell cellSize) {
        GridEntity ret = null;

        //ensure we can spawn on given cell info
        var container = GridEditController.instance.entityContainer;
        if(container.IsPlaceable(cell, cellSize, null)) {
            if(!mPool) {
                mPool = M8.PoolController.CreatePool(poolGroup);
                mPool.AddType(template, poolCapacity, poolCapacity);
            }

            if(mSpawnParms == null)
                mSpawnParms = new M8.GenericParams();

            mSpawnParms[GridEntity.parmData] = this;
            mSpawnParms[GridEntity.parmCellIndex] = cell;
            mSpawnParms[GridEntity.parmCellSize] = cellSize;

            ret = mPool.Spawn<GridEntity>(template.name, name, container.root, mSpawnParms);
        }

        return ret;
    }

    public void Init() {
        shaderPulseScaleId = Shader.PropertyToID(_shaderPulseScaleVar);
        shaderPulseColorId = Shader.PropertyToID(_shaderPulseColorVar);
        shaderColorId = Shader.PropertyToID(_shaderColorVar);

        if(material)
            pulseColor = material.GetColor(shaderPulseColorId);
    }

    void Awake() {
        Init();
    }
}
