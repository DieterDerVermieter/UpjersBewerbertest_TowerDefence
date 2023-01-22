using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Camera m_mainCamera;
    [SerializeField] private string m_buildingsResourcePath;

    [SerializeField] private MapLayout m_map;
    [SerializeField] private TowerMenuController m_buildingInfoController;

    [SerializeField] private Transform m_shopItemContainer;
    [SerializeField] private TowerShopItemController m_shopItemPrefab;

    [SerializeField] private Color m_positionFreeColor;
    [SerializeField] private Color m_positionBlockedColor;


    private TowerData m_draggedBuildingData;
    private TowerController m_draggedBuilding;

    private TowerController m_selectedBuilding;


    private void Start()
    {
        var buildings = Resources.LoadAll<TowerData>(m_buildingsResourcePath);

        foreach (var buildingData in buildings)
        {
            var shopItem = Instantiate(m_shopItemPrefab, m_shopItemContainer);
            shopItem.Setup(buildingData);
        }

        m_buildingInfoController.Setup(this);
        m_buildingInfoController.gameObject.SetActive(false);
    }


    public void StartBuildBuilding(PointerEventData eventData, TowerData buildingData)
    {
        SelectBuilding(null);

        m_draggedBuildingData = buildingData;
        m_draggedBuilding = Instantiate(buildingData.Prefab, transform);

        m_draggedBuilding.Setup(this, m_draggedBuildingData);
        m_draggedBuilding.SetPreviewState(true);

        eventData.pointerDrag = gameObject;

        Debug.Log($"Start build building {buildingData.DisplayName}");
    }


    public void SelectBuilding(TowerController building)
    {
        if(m_selectedBuilding != null)
        {
            m_selectedBuilding.SetSelectionState(false);
            m_buildingInfoController.gameObject.SetActive(false);
        }

        m_selectedBuilding = building;

        if (m_selectedBuilding != null)
        {
            m_selectedBuilding.SetSelectionState(true);

            m_buildingInfoController.SetTargetBuilding(building);
            m_buildingInfoController.gameObject.SetActive(true);

            Debug.Log($"Select building {m_selectedBuilding.Data.DisplayName}");
        }
    }


    public void SellSelectedBuilding()
    {
        if (m_selectedBuilding == null)
            return;

        var sellReward = (int)(m_selectedBuilding.Data.Price * 0.5f);
        GameManager.Instance.RewardCash(sellReward);

        var gridPosition = m_map.WorldToGridPosition(m_selectedBuilding.transform.position);
        gridPosition -= Vector2Int.one * m_selectedBuilding.GridSize / 2;

        m_map.SetGridArea(gridPosition.x, gridPosition.y, m_selectedBuilding.GridSize, 0);

        Destroy(m_selectedBuilding.gameObject);
        SelectBuilding(null);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (m_draggedBuilding == null)
            eventData.pointerDrag = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        float evenSizeOffset = m_draggedBuilding.GridSize % 2 == 0 ? 1 : 0;
        worldPosition += new Vector3(1, 1) * MapLayout.CELL_SIZE * 0.5f * evenSizeOffset;

        var gridPosition = m_map.WorldToGridPosition(worldPosition);

        worldPosition = m_map.GridToWorldPosition(gridPosition.x, gridPosition.y);
        worldPosition -= new Vector3(1, 1) * MapLayout.CELL_SIZE * 0.5f * evenSizeOffset;

        m_draggedBuilding.transform.position = worldPosition;

        gridPosition -= Vector2Int.one * m_draggedBuilding.GridSize / 2;

        var isAreaBlocked = m_map.IsGridAreaBlocked(gridPosition.x, gridPosition.y, m_draggedBuilding.GridSize);
        m_draggedBuilding.SetSizeIndicatorColor(isAreaBlocked ? m_positionBlockedColor : m_positionFreeColor);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        float evenSizeOffset = m_draggedBuilding.GridSize % 2 == 0 ? 1 : 0;
        worldPosition += new Vector3(1, 1) * MapLayout.CELL_SIZE * 0.5f * evenSizeOffset;

        var gridPosition = m_map.WorldToGridPosition(worldPosition);
        gridPosition -= Vector2Int.one * m_draggedBuilding.GridSize / 2;

        if (!m_map.IsGridAreaBlocked(gridPosition.x, gridPosition.y, m_draggedBuilding.GridSize)
            && GameManager.Instance.UseCash(m_draggedBuildingData.Price))
        {
            m_map.SetGridArea(gridPosition.x, gridPosition.y, m_draggedBuilding.GridSize, 1);
            m_draggedBuilding.SetPreviewState(false);
        }
        else
        {
            Destroy(m_draggedBuilding.gameObject);
        }

        m_draggedBuilding = null;
        m_draggedBuildingData = null;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        SelectBuilding(null);
    }
}
