using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : Singleton<TowerManager>, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Camera m_mainCamera;

    [SerializeField] private float m_sellbackMultiplier = 0.5f;

    [Header("Tower Shop")]
    [SerializeField] private TowerShopController m_towerShopController;
    [SerializeField] private TowerData[] m_towers;

    [Header("Tower Info")]
    [SerializeField] private TowerInfoController m_towerInfoController;

    [Header("Tower Preview")]
    [SerializeField] private Color m_positionFreeColor;
    [SerializeField] private Color m_positionOccupiedColor;


    // Currently dragged or selected tower
    private TowerController m_draggedTower;
    private TowerController m_selectedTower;


    private void Start()
    {
        // Add towers to shop
        foreach (var towerData in m_towers)
        {
            m_towerShopController.AddTowerToShop(towerData);
        }

        // Hide the tower info panel
        m_towerInfoController.gameObject.SetActive(false);
    }


    /// <summary>
    /// Starts the building process of a new tower.
    /// </summary>
    /// <param name="towerData">The tower to build</param>
    public void StartBuildingTower(TowerData towerData)
    {
        // Deselect current selection
        SelectTower(null);

        // Spawn and setup the tower preview
        m_draggedTower = Instantiate(towerData.ControllerPrefab, transform);
        m_draggedTower.SetPreviewState(true);

        Debug.Log($"Start build tower {towerData.DisplayName}");
    }


    /// <summary>
    /// Select a new tower or deselect the selected one.
    /// </summary>
    /// <param name="tower">The target tower</param>
    public void SelectTower(TowerController tower)
    {
        // Deselect current selection, if it isn't null
        if(m_selectedTower != null)
        {
            m_selectedTower.SetSelectionState(false);
            m_towerInfoController.gameObject.SetActive(false);

            Debug.Log($"Deselect tower {m_selectedTower.Data.DisplayName}");
        }

        // Exit early, if we clicked the same tower
        if(m_selectedTower == tower)
        {
            m_selectedTower = null;
            return;
        }

        // Select the new tower, if it isn't null
        if (tower != null)
        {
            tower.SetSelectionState(true);

            m_towerInfoController.SetTargetBuilding(tower);
            m_towerInfoController.gameObject.SetActive(true);

            Debug.Log($"Select tower {tower.Data.DisplayName}");
        }

        m_selectedTower = tower;
    }


    /// <summary>
    /// Sells the currently selected tower.
    /// This destroyes the tower and rewards the player cash, based on the towers price.
    /// </summary>
    public void SellSelectedBuilding()
    {
        // Can't sell nothing
        if (m_selectedTower == null)
            return;

        // Calculate the sell value and reward it to the player
        var sellReward = (int)(m_selectedTower.Data.Price * m_sellbackMultiplier);
        GameManager.Instance.RewardCash(sellReward);

        // Calculate the grid position of the tower area
        var gridAreaPosition = MapLayout.Instance.WorldToGridPosition(m_selectedTower.transform.position);
        gridAreaPosition -= Vector2Int.one * m_selectedTower.Data.GridSize / 2;

        // Mark the area as unoccupied
        MapLayout.Instance.SetGridArea(gridAreaPosition, m_selectedTower.Data.GridSize, false);

        // Destroy and deselect the tower
        Destroy(m_selectedTower.gameObject);
        SelectTower(null);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        // If we start a drag inside the world area and aren't dragging any tower, stop dragging
        if (m_draggedTower == null)
            eventData.pointerDrag = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Calculate the world position of the pointer
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        // We need to add and offset the world position for even sized objects
        var evenSizeOffset = new Vector3(1, 1) * (m_draggedTower.Data.GridSize % 2 == 0 ? MapLayout.CELL_SIZE * 0.5f : 0);
        worldPosition += evenSizeOffset;

        // Calculate the grid position
        var gridPosition = MapLayout.Instance.WorldToGridPosition(worldPosition);

        // Recalculate the world position, essentially clamping it to the grid
        // Also substract the offset again
        worldPosition = MapLayout.Instance.GridToWorldPosition(gridPosition);
        worldPosition -= evenSizeOffset;

        // Finally position dragged our tower
        m_draggedTower.transform.position = worldPosition;

        // Calculate the grid position of the area the tower occupies
        var gridAreaPosition = gridPosition - Vector2Int.one * m_draggedTower.Data.GridSize / 2;

        // Change the towers indicator color based on the occupancy state of it's area
        var isAreaFree = MapLayout.Instance.IsGridAreaFree(gridAreaPosition, m_draggedTower.Data.GridSize);
        m_draggedTower.SetSizeIndicatorColor(isAreaFree ? m_positionFreeColor : m_positionOccupiedColor);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Calculate the world position of the pointer
        var worldPosition = m_mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
        worldPosition.z = 0;

        // We need to add and offset the world position for even sized objects
        var evenSizeOffset = new Vector3(1, 1) * (m_draggedTower.Data.GridSize % 2 == 0 ? MapLayout.CELL_SIZE * 0.5f : 0);
        worldPosition += evenSizeOffset;

        // Calculate the grid position
        var gridPosition = MapLayout.Instance.WorldToGridPosition(worldPosition);

        // Calculate the grid position of the area the tower occupies
        var gridAreaPosition = gridPosition - Vector2Int.one * m_draggedTower.Data.GridSize / 2;

        // If the tower area is unoccupied and we have enough cash, place the tower
        // Else, destroy it
        if (MapLayout.Instance.IsGridAreaFree(gridAreaPosition, m_draggedTower.Data.GridSize)
            && GameManager.Instance.UseCash(m_draggedTower.Data.Price))
        {
            // Mark Grid area as occupied
            MapLayout.Instance.SetGridArea(gridAreaPosition, m_draggedTower.Data.GridSize, true);

            // Remove preview flag from tower
            m_draggedTower.SetPreviewState(false);
        }
        else
        {
            Destroy(m_draggedTower.gameObject);
        }

        // Reset dragged tower, so we now, when we are dragging something
        m_draggedTower = null;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // Deselect any tower, if we click on the floor
        SelectTower(null);
    }
}
