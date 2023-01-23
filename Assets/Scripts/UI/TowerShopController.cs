using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TowerShopController : MonoBehaviour, IPointerExitHandler
{
    [SerializeField] private Transform m_shopItemContainer;
    [SerializeField] private TowerShopItemController m_shopItemPrefab;


    public void AddTowerToShop(TowerData towerData)
    {
        // Spawn and setup a new shop item
        var shopItem = Instantiate(m_shopItemPrefab, m_shopItemContainer);
        shopItem.Setup(towerData);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        // Exit, if there is no object being dragged and there is no object selected
        if (eventData.pointerDrag == null || eventData.selectedObject == null)
            return;

        // Exit, if the selected object wasn't a towerShopItem
        if (!eventData.selectedObject.TryGetComponent<TowerShopItemController>(out var shopItem))
            return;

        // If we are here, we dragged a shopItem out of the shop area
        // Then we want to start building the tower
        TowerManager.Instance.StartBuildingTower(shopItem.GetData());

        // We also need to pass the pointerDrag to the towerManager, so it gets pointerDrag callbacks
        eventData.pointerDrag = TowerManager.Instance.gameObject;

        // Reset selected object, so we can't retrigger this during the same drag
        eventData.selectedObject = null;
    }
}
