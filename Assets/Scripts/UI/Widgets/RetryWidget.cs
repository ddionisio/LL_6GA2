using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryWidget : MonoBehaviour {
    [Header("Data")]
    public GridEntityData doodadData; //use to determine which are doodads to ignore

    [Header("Display")]
    public Button retryButton;

    [Header("Confirm")]
    public string modalConfirm = "confirm";
    [M8.Localize]
    public string modalConfirmTitleRef;
    [M8.Localize]
    public string modalConfirmDescRef;

    private M8.GenericParams mParms;

    void OnDisable() {
        if(GridEditController.isInstantiated) {
            var editCtrl = GridEditController.instance;
            editCtrl.editChangedCallback -= RefreshDisplay;

            if(editCtrl.entityContainer)
                editCtrl.entityContainer.mapUpdateCallback -= OnMapUpdate;
        }
    }

    void OnEnable() {
        var editCtrl = GridEditController.instance;
        editCtrl.editChangedCallback += RefreshDisplay;
        editCtrl.entityContainer.mapUpdateCallback += OnMapUpdate;

        RefreshDisplay();
    }

    void Awake() {
        retryButton.onClick.AddListener(OnClick);

        mParms = new M8.GenericParams();
        mParms[ModalConfirm.parmTitleTextRef] = modalConfirmTitleRef;
        mParms[ModalConfirm.parmDescTextRef] = modalConfirmDescRef;
        mParms[ModalConfirm.parmCallback] = (System.Action<bool>)OnClickConfirm;
    }

    void OnClick() {
        M8.ModalManager.main.Open(modalConfirm, mParms);
    }

    void OnClickConfirm(bool confirm) {
        if(confirm) {
            var editCtrl = GridEditController.instance;

            editCtrl.selected = null;

            var ents = editCtrl.entityContainer.entities;
            if(ents != null) {
                for(int i = ents.Count - 1; i >= 0; i--) {
                    var ent = ents[i];
                    if(ent.poolDataController)
                        ent.poolDataController.Release();
                }
            }

            if(editCtrl.editMode != GridEditController.EditMode.Select)
                editCtrl.editMode = GridEditController.EditMode.Select;
            else
                editCtrl.ChangeInvoke();
        }
    }

    void OnMapUpdate(GridEntityData entDat) {
        RefreshDisplay();
    }

    void RefreshDisplay() {
        //set interaction if there are any placed cubes
        var editCtrl = GridEditController.instance;

        bool isInteractible = false;

        //determine if there are placed entities
        var ents = editCtrl.entityContainer.entities;
        if(ents != null) {
            for(int i = 0; i < ents.Count; i++) {
                var ent = ents[i];
                if(ent.data != doodadData) {
                    isInteractible = true;
                    break;
                }
            }
        }

        retryButton.interactable = isInteractible;
    }
}
