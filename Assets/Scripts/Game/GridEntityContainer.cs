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

    void Awake() {
        if(!_controller)
            _controller = GetComponent<GridController>();

        //init containers
        var cellSize = _controller.cellSize;

        entities = new M8.CacheList<GridEntity>(cellSize.row * cellSize.col);
    }
}
