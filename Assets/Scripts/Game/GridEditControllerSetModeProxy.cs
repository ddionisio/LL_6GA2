using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEditControllerSetModeProxy : MonoBehaviour {
    public GridEditController.EditMode toMode;
    public bool clearSelection; //clear selection before changing mode

    public void Invoke() {
        if(clearSelection)
            GridEditController.instance.selected = null;

        GridEditController.instance.editMode = toMode;
    }
}
