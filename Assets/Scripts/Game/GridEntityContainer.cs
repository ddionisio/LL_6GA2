using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use for mapping entities (GridEntity) within the grid via GridController. Mapping is stricly only on row/col
/// </summary>
public class GridEntityContainer : MonoBehaviour {
    [Header("Data")]
    [SerializeField]
    GridController _controller = null;
    [SerializeField]
    Transform _root = null;

    public GridController controller { get { return _controller; } }

    public Transform root { get { return _root; } }

    public M8.CacheList<GridEntity> entities { get; private set; }

    /// <summary>
    /// Called when an entity is added/removed/updated
    /// </summary>
    public event System.Action<GridEntityData> mapUpdateCallback;

    private GridEntity[,] mEntityMap; //[row, col]

    public int GetVolumeCount(GridEntityData entDat) {
        int count = 0;
        
        for(int i = 0; i < entities.Count; i++) {
            var ent = entities[i];
            if(ent.data == entDat)
                count += ent.cellSize.volume;
        }

        return count;
    }

    public GridEntity GetEntity(GridCell cell) {
        return GetEntity(cell.row, cell.col);
    }

    public GridEntity GetEntity(int row, int col) {
        if(row >= 0 && row < mEntityMap.GetLength(0) && col >= 0 && col < mEntityMap.GetLength(1))
            return mEntityMap[row, col];

        return null;
    }

    /// <summary>
    /// Check if an area is empty, set ignoreEnt to treat that entity as empty (use during moving/expanding)
    /// </summary>
    public bool IsPlaceable(GridCell index, GridCell size, GridEntity ignoreEnt) {
        //check if contained
        if(controller.IsContained(index, size)) {
            //ensure nothing is occupying the spaces
            for(int r = 0; r < size.row; r++) {
                for(int c = 0; c < size.col; c++) {
                    var ent = mEntityMap[r + index.row, c + index.col];
                    if(ent != null) {
                        if(ignoreEnt && ent == ignoreEnt)
                            continue;
                        else
                            return false;
                    }
                }
            }

            return true;
        }

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

        entities.Clear();
    }

    public void AddEntity(GridEntity ent) {
        //check if it already exists
        if(entities.Exists(ent)) {
            //remove current mapping
            ClearEntityMap(ent);
        }
        else
            entities.Add(ent);

        //apply mapping
        ApplyEntityMap(ent);

        mapUpdateCallback?.Invoke(ent.data);
    }

    public void RemoveEntity(GridEntity ent) {
        if(entities.Remove(ent)) {
            //clear mapping
            ClearEntityMap(ent);
        }

        mapUpdateCallback?.Invoke(ent.data);
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

        entities = new M8.CacheList<GridEntity>(cellSize.row * cellSize.col);
    }
}
