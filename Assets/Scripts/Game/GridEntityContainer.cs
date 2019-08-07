using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use for mapping entities (GridEntity) within the grid via GridController. Mapping is stricly only on row/col
/// </summary>
public class GridEntityContainer : MonoBehaviour {
    [Header("Data")]
    [SerializeField]
    GridController _controller;

    public GridController controller { get { return _controller; } }

    private int[,] mEntityMap; //indices from mEntities, -1 = empty, [row, col]

    private M8.CacheList<GridEntity> mEntities;

    public void ClearEntities() {
        //clear map
        for(int r = 0; r < mEntityMap.GetLength(0); r++) {
            for(int c = 0; c < mEntityMap.GetLength(1); c++) {
                mEntityMap[r, c] = -1;
            }
        }

        //clear entities
        for(int i = 0; i < mEntities.Count; i++) {
            if(mEntities[i])
                mEntities[i].Release();
        }

        mEntities.Clear();
    }

    public void AddEntity(GridEntity ent) {

    }

    public void RemoveEntity(GridEntity ent, bool release) {
        if(mEntities.Remove(ent)) {
            //clear mapping


            if(release)
                ent.Release();
        }
    }

    public void ClearAt(GridCell cell) {
        ClearAt(cell.row, cell.col);
    }

    public void ClearAt(int row, int col) {

    }

    void Awake() {
        if(!_controller)
            _controller = GetComponent<GridController>();

        //init containers
        var cellSize = _controller.cellSize;

        mEntityMap = new int[cellSize.row, cellSize.col];

        mEntities = new M8.CacheList<GridEntity>(cellSize.row * cellSize.col);

        ClearEntities();
    }
}
