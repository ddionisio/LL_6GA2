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
    [SerializeField]
    GridEntityData _doodadEntityData = null; //use for filtering out entities when grouping

    [Header("Signal Invoke")]
    public SignalGridEntityData signalInvokeMapUpdate;

    public GridController controller { get { return _controller; } }

    public Transform root { get { return _root; } }

    public M8.CacheList<GridEntity> entities { get; private set; }

    /// <summary>
    /// Called when an entity is added/removed/updated
    /// </summary>
    public event System.Action<GridEntityData> mapUpdateCallback;

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
        for(int i = 0; i < entities.Count; i++) {
            var ent = entities[i];
            if(ent.IsContained(cell))
                return ent;
        }

        return null;
    }

    /// <summary>
    /// Check if an area is empty, set ignoreEnt to treat that entity as empty (use during moving/expanding)
    /// </summary>
    public bool IsPlaceable(GridCell index, GridCell size, GridEntity ignoreEnt) {
        //check if contained
        if(controller.IsContained(index, size)) {
            //ensure nothing is occupying the spaces
            for(int i = 0; i < entities.Count; i++) {
                var ent = entities[i];
                if(ent != ignoreEnt) {
                    if(GridCell.IsIntersectFloor(index, size, ent.cellIndex, ent.cellSize))
                        return false;
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
        entities.Clear();
    }

    public void AddEntity(GridEntity ent) {
        //check if it already exists
        if(!entities.Exists(ent)) {
            entities.Add(ent);

            mapUpdateCallback?.Invoke(ent.data);

            if(signalInvokeMapUpdate)
                signalInvokeMapUpdate.Invoke(ent.data);
        }
    }

    public void RemoveEntity(GridEntity ent) {
        if(entities.Remove(ent)) {
            mapUpdateCallback?.Invoke(ent.data);

            if(signalInvokeMapUpdate)
                signalInvokeMapUpdate.Invoke(ent.data);
        }
    }

    /// <summary>
    /// This will group up entities with the same data if they are neighboring each other
    /// </summary>
    public List<List<GridEntity>> GenerateEntityGroups() {
        var entGroups = new List<List<GridEntity>>();

        var ents = new M8.CacheList<GridEntity>(entities);
        while(ents.Count > 0) {
            var entList = new List<GridEntity>();

            var ent = ents.RemoveLast();

            if(ent.data == _doodadEntityData) //exclude doodads
                continue;

            entList.Add(ent);

            //check neighbors
            GroupAddEntities(ent, ents, entList);

            entGroups.Add(entList);
        }

        return entGroups;
    }

    /// <summary>
    /// Returns how much was added
    /// </summary>
    private int GroupAddEntities(GridEntity srcEnt, M8.CacheList<GridEntity> ents, List<GridEntity> group) {
        var addedCount = 0;

        var _cellInd = new GridCell();
        var _cellSize = new GridCell();

        int curInd = ents.Count - 1;
        while(curInd >= 0) {
            var checkEnt = ents[curInd];
            if(checkEnt.data != srcEnt.data) {
                curInd--;
                continue;
            }

            //check with top/bottom extended
            _cellInd = srcEnt.cellIndex;
            _cellInd.row -= 1;
            _cellSize = srcEnt.cellSize;
            _cellSize.row += 2;

            if(GridCell.IsIntersectFloor(_cellInd, _cellSize, checkEnt.cellIndex, checkEnt.cellSize)) {
                ents.RemoveAt(curInd);
                group.Add(checkEnt);
                addedCount++;

                //check this entity with the rest
                var subAddCount = GroupAddEntities(checkEnt, ents, group);
                if(subAddCount > 0) {
                    addedCount += subAddCount;
                    curInd = ents.Count - 1;
                }
                else
                    curInd--;

                continue;
            }

            //check with left/right extended
            _cellInd = srcEnt.cellIndex;
            _cellInd.col -= 1;
            _cellSize = srcEnt.cellSize;
            _cellSize.col += 2;
            if(GridCell.IsIntersectFloor(_cellInd, _cellSize, checkEnt.cellIndex, checkEnt.cellSize)) {
                ents.RemoveAt(curInd);
                group.Add(checkEnt);
                addedCount++;

                //check this entity with the rest
                var subAddCount = GroupAddEntities(checkEnt, ents, group);
                if(subAddCount > 0) {
                    addedCount += subAddCount;
                    curInd = ents.Count - 1;
                }
                else
                    curInd--;

                continue;
            }

            curInd--;
        }

        return addedCount;
    }

    void Awake() {
        if(!_controller)
            _controller = GetComponent<GridController>();

        //init containers
        var cellSize = _controller.cellSize;

        entities = new M8.CacheList<GridEntity>(cellSize.row * cellSize.col);
    }
}
