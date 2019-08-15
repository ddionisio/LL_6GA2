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

    public M8.CacheList<GridEntity> entities { get { return mEntities; } }

    private GridEntity[,] mEntityMap; //[row, col]

    private M8.CacheList<GridEntity> mEntities;

    public GridEntity GetEntity(GridCell cell) {
        return GetEntity(cell.row, cell.col);
    }

    public GridEntity GetEntity(int row, int col) {
        if(row >= 0 && row < mEntityMap.GetLength(0) && col >= 0 && col < mEntityMap.GetLength(1))
            return mEntityMap[row, col];

        return null;
    }

    public bool IsPlaceable(GridCell index, GridCell size) {

        return false;
    }

    public bool IsPlaceable(GridEntity ent) {

        return false;
    }

    public void ClearEntities() {
        //clear map
        for(int r = 0; r < mEntityMap.GetLength(0); r++) {
            for(int c = 0; c < mEntityMap.GetLength(1); c++) {
                mEntityMap[r, c] = null;
            }
        }

        mEntities.Clear();
    }

    public void AddEntity(GridEntity ent) {
        //check if it already exists
        if(mEntities.Exists(ent)) {
            //remove current mapping
            ClearEntityMap(ent);
        }
        else
            mEntities.Add(ent);

        //apply mapping
        ApplyEntityMap(ent);
    }

    public void RemoveEntity(GridEntity ent) {
        if(mEntities.Remove(ent)) {
            //clear mapping
            ClearEntityMap(ent);
        }
    }

    private void ClearEntityMap(GridEntity ent) {
        var cellInd = ent.cellIndex;
        var cellSize = ent.cellSize;

        for(int r = 0; r < cellSize.row; r++) {
            var rInd = r + cellInd.row;
            if(rInd < 0 || rInd >= mEntityMap.GetLength(0))
                continue;

            for(int c = 0; c < cellSize.col; c++) {
                var cInd = c + cellInd.col;
                if(cInd < 0 || cInd >= mEntityMap.GetLength(1))
                    continue;

                if(mEntityMap[rInd, cInd] == ent)
                    mEntityMap[rInd, cInd] = null;
            }
        }
    }

    private void ApplyEntityMap(GridEntity ent) {
        var cellInd = ent.cellIndex;
        var cellSize = ent.cellSize;

        for(int r = 0; r < cellSize.row; r++) {
            var rInd = r + cellInd.row;
            if(rInd < 0 || rInd >= mEntityMap.GetLength(0))
                continue;

            for(int c = 0; c < cellSize.col; c++) {
                var cInd = c + cellInd.col;
                if(cInd < 0 || cInd >= mEntityMap.GetLength(1))
                    continue;

                mEntityMap[rInd, cInd] = ent;
            }
        }
    }

    void Awake() {
        if(!_controller)
            _controller = GetComponent<GridController>();

        //init containers
        var cellSize = _controller.cellSize;

        mEntityMap = new GridEntity[cellSize.row, cellSize.col];

        mEntities = new M8.CacheList<GridEntity>(cellSize.row * cellSize.col);
    }
}
