using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridEntityDeckWidget : MonoBehaviour {
    [Header("Template")]
    public GridEntityCardWidget cardTemplate;

    [Header("Display")]
    public Text countText;
    public GameObject countInvalidGO; //specific to the count display

    public GameObject invalidGO;

    public Transform container;

    [Header("Signal Listen")]
    public SignalGridEntity signalListenEntitySizeChanged;
    public SignalGridEntityData signalListenMapChanged;

    public M8.CacheList<GridEntityCardWidget> cards { get; private set; }

    void OnDestroy() {
        signalListenEntitySizeChanged.callback -= OnEntitySizeChanged;
        signalListenMapChanged.callback -= OnMapChanged;

        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditModeChanged;
    }

    void Awake() {
        cardTemplate.gameObject.SetActive(false);

        //setup cards
        var itemGrp = GridEditController.instance.levelData;
        var items = itemGrp.items;

        cards = new M8.CacheList<GridEntityCardWidget>(items.Length);

        for(int i = 0; i < items.Length; i++) {
            var itm = items[i];

            var cardWidget = Instantiate(cardTemplate, container);
                        
            cardWidget.Setup(itm);

            cardWidget.gameObject.SetActive(true);

            cards.Add(cardWidget);
        }

        RefreshCount();

        signalListenEntitySizeChanged.callback += OnEntitySizeChanged;
        signalListenMapChanged.callback += OnMapChanged;

        GridEditController.instance.editChangedCallback += OnEditModeChanged;
    }

    void OnEntitySizeChanged(GridEntity ent) {
        RefreshCount();
    }

    void OnMapChanged(GridEntityData entDat) {
        RefreshCount();
    }

    void OnEditModeChanged() {
        RefreshCount();
    }

    private void RefreshCount() {
        var availableCount = GridEditController.instance.GetAvailableCount();

        /*var editCtrl = GridEditController.instance;
        var measureType = editCtrl.levelData.measureType;
        var side = editCtrl.levelData.sideMeasure;
        var volume = side * side * side;

        countText.text = UnitMeasure.GetVolumeText(measureType, volume * availableCount);*/

        countText.text = availableCount.ToString();

        if(countInvalidGO) countInvalidGO.SetActive(availableCount < 0);
        if(invalidGO) invalidGO.SetActive(availableCount <= 0 || GridEditController.instance.editMode == GridEditController.EditMode.Expand);
    }
}
