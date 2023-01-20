using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Camera m_mainCamera;
    [SerializeField] private string m_buildingsResourcePath;

    [SerializeField] private MapLayout m_map;

    [SerializeField] private Transform m_shopItemContainer;
    [SerializeField] private BuildingShopItemController m_shopItemPrefab;

    [SerializeField] private GameObject m_buildingPreviewPrefab;


    private BuildingData m_draggedBuildingData;
    private GameObject m_draggedBuildingPreview;


    private void Start()
    {
        var buildings = Resources.LoadAll<BuildingData>(m_buildingsResourcePath);

        foreach (var buildingData in buildings)
        {
            var shopItem = Instantiate(m_shopItemPrefab, m_shopItemContainer);
            shopItem.Setup(buildingData);
        }
    }


    public void StartBuilding(PointerEventData eventData, BuildingData buildingData)
    {
        m_draggedBuildingData = buildingData;
        m_draggedBuildingPreview = Instantiate(m_buildingPreviewPrefab, transform);

        eventData.pointerDrag = gameObject;

        Debug.Log($"Start building {buildingData.DisplayName}");
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        float evenSizeOffset = m_draggedBuildingData.Size % 2 == 0 ? 1 : 0;
        worldPosition += new Vector3(1, 1) * m_map.GetGridCellSize() * 0.5f * evenSizeOffset;

        var gridPosition = m_map.WorldToGridPosition(worldPosition);

        worldPosition = m_map.GridToWorldPosition(gridPosition.x, gridPosition.y);
        worldPosition -= new Vector3(1, 1) * m_map.GetGridCellSize() * 0.5f * evenSizeOffset;

        m_draggedBuildingPreview.transform.position = worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        float evenSizeOffset = m_draggedBuildingData.Size % 2 == 0 ? 1 : 0;
        worldPosition += new Vector3(1, 1) * m_map.GetGridCellSize() * 0.5f * evenSizeOffset;

        var gridPosition = m_map.WorldToGridPosition(worldPosition);
        gridPosition -= Vector2Int.one * m_draggedBuildingData.Size / 2;    

        if (!m_map.IsGridAreaBlocked(gridPosition.x, gridPosition.y, m_draggedBuildingData.Size))
        {
            m_map.SetGridArea(gridPosition.x, gridPosition.y, m_draggedBuildingData.Size, 1);

            var building = Instantiate(m_draggedBuildingData.Prefab, transform);
            building.transform.position = m_draggedBuildingPreview.transform.position;
        }

        Destroy(m_draggedBuildingPreview);

        m_draggedBuildingPreview = null;
        m_draggedBuildingData = null;
    }
}
