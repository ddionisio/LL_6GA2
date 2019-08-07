using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity within GridEntityContainer. Ensure that this entity is inside the GridController hierarchy
/// </summary>
public class GridEntity : MonoBehaviour {
    public GridEntityContainer container {
        get {
            if(!mContainer)
                mContainer = GetComponentInParent<GridEntityContainer>();
            return mContainer;
        }
    }

    public GridCell cell {
        get { return mCell; }
        set {
            if(mCell != value) {
                //do we need to move?
                if(mCell.row != value.row || mCell.col != value.col) {
                    container.RemoveEntity(this, false);

                    mCell = value;

                    container.AddEntity(this);
                }
                else
                    mCell = value;
            }
        }
    }

    private GridEntityContainer mContainer;

    private GridCell mCell; //cell position bottom-left
    private GridCell mCellSize;

    public void SetCell(GridCell index, GridCell size) {

    }

    public void Release() {

    }
    
}
