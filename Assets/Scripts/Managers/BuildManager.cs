using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : Singleton<BuildManager>, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Camera m_mainCamera;

    [SerializeField] private List<TowerData> m_towers;

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
        foreach (var towerData in m_towers)
        {
            var shopItem = Instantiate(m_shopItemPrefab, m_shopItemContainer);
            shopItem.Setup(towerData);
        }

        m_buildingInfoController.Setup(this);
        m_buildingInfoController.gameObject.SetActive(false);
    }


    public void StartBuildBuilding(PointerEventData eventData, TowerData buildingData)
    {
        SelectBuilding(null);

        m_draggedBuildingData = buildingData;
        m_draggedBuilding = Instantiate(buildingData.ControllerPrefab, transform);

        m_draggedBuilding.Setup(this, m_draggedBuildingData);
        m_draggedBuilding.SetPreviewState(true);

        eventData.pointerDrag = gameObject;

        Debug.Log($"Start build tower {buildingData.DisplayName}");
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

            Debug.Log($"Select building {m_selectedBuilding.MyData.DisplayName}");
        }
    }


    public void SellSelectedBuilding()
    {
        if (m_selectedBuilding == null)
            return;

        var sellReward = (int)(m_selectedBuilding.MyData.Price * 0.5f);
        GameManager.Instance.RewardCash(sellReward);

        var gridPosition = MapLayout.Instance.WorldToGridPosition(m_selectedBuilding.transform.position);
        gridPosition -= Vector2Int.one * m_selectedBuilding.GridSize / 2;

        MapLayout.Instance.SetGridArea(gridPosition.x, gridPosition.y, m_selectedBuilding.GridSize, 0);

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

        var gridPosition = MapLayout.Instance.WorldToGridPosition(worldPosition);

        worldPosition = MapLayout.Instance.GridToWorldPosition(gridPosition.x, gridPosition.y);
        worldPosition -= new Vector3(1, 1) * MapLayout.CELL_SIZE * 0.5f * evenSizeOffset;

        m_draggedBuilding.transform.position = worldPosition;

        gridPosition -= Vector2Int.one * m_draggedBuilding.GridSize / 2;

        var isAreaBlocked = MapLayout.Instance.IsGridAreaBlocked(gridPosition.x, gridPosition.y, m_draggedBuilding.GridSize);
        m_draggedBuilding.SetSizeIndicatorColor(isAreaBlocked ? m_positionBlockedColor : m_positionFreeColor);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        float evenSizeOffset = m_draggedBuilding.GridSize % 2 == 0 ? 1 : 0;
        worldPosition += new Vector3(1, 1) * MapLayout.CELL_SIZE * 0.5f * evenSizeOffset;

        var gridPosition = MapLayout.Instance.WorldToGridPosition(worldPosition);
        gridPosition -= Vector2Int.one * m_draggedBuilding.GridSize / 2;

        if (!MapLayout.Instance.IsGridAreaBlocked(gridPosition.x, gridPosition.y, m_draggedBuilding.GridSize)
            && GameManager.Instance.UseCash(m_draggedBuildingData.Price))
        {
            MapLayout.Instance.SetGridArea(gridPosition.x, gridPosition.y, m_draggedBuilding.GridSize, 1);
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
