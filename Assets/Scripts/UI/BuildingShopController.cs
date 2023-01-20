using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BuildingShopController : MonoBehaviour, IPointerExitHandler
{
    public UnityEvent<PointerEventData, BuildingData> OnBuildingDragged;


    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.selectedObject == null)
            return;

        if (!eventData.selectedObject.TryGetComponent<BuildingShopItemController>(out var shopItem))
            return;

        eventData.selectedObject = null;
        OnBuildingDragged?.Invoke(eventData, shopItem.GetData());
    }
}
