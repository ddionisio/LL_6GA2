using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityDeckWidget : MonoBehaviour {
    [Header("Template")]
    public GridEntityCardWidget cardTemplate;

    [Header("Display")]
    public Transform container;
    public GridEntityCardWidget cardDrag; //use for dragging

    public M8.CacheList<GridEntityCardWidget> cards { get; private set; }

    void Awake() {
        cardTemplate.gameObject.SetActive(false);
        cardDrag.gameObject.SetActive(false);

        //setup cards
        var itemGrp = GridEditController.instance.entityDataGroup;
        var items = itemGrp.items;

        cards = new M8.CacheList<GridEntityCardWidget>(items.Length);

        for(int i = 0; i < items.Length; i++) {
            var itm = items[i];

            var cardWidget = Instantiate(cardTemplate, container);
                        
            cardWidget.Setup(itm.data, cardDrag);
            cardWidget.SetCount(itm.count);

            cardWidget.gameObject.SetActive(true);

            cards.Add(cardWidget);
        }
    }
}
