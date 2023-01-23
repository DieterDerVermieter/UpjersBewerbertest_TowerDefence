using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLayout : Singleton<MapLayout>
{
    [Header("Path")]
    [SerializeField] private Transform m_waypointContainer;

    [Header("Grid")]
    [SerializeField] private Vector2Int m_gridAreaSize;
    [SerializeField] private Vector3 m_gridAreaOffset;

    [SerializeField] private Collider2D m_pathCollider;


    // Generated waypoints
    [HideInInspector][SerializeField] private Vector3[] m_waypoints;

    // Generated grid
    [HideInInspector][SerializeField] private bool[] m_grid;
    [HideInInspector][SerializeField] private Vector2Int m_gridSize;
    [HideInInspector][SerializeField] private Vector3 m_gridOffset;


    // Size of the grid cells
    public const int CELLS_PER_TILE = 2;
    public const float CELL_SIZE = 1.0f / CELLS_PER_TILE;


    #region Path
    public int GetWaypointCount() => m_waypoints.Length;

    public Vector3 GetWaypoint(int index) => m_waypoints[index];


#if UNITY_EDITOR
    [MyBox.ButtonMethod]
    private void GeneratePath()
    {
        m_waypoints = new Vector3[m_waypointContainer.childCount];

        // Generate the path based on the waypoints in the specified container
        for (int i = 0; i < m_waypointContainer.childCount; i++)
        {
            m_waypoints[i] = m_waypointContainer.GetChild(i).position;
        }

        Debug.Log($"Generated Path containing {GetWaypointCount()} Waypoints.");
    }
#endif
    #endregion


    #region Grid
    private bool GetGridValue(Vector2Int gridPosition)
    {
        // Default all positions outside the grid to occupied
        if (gridPosition.x < 0 || gridPosition.x >= m_gridSize.x || gridPosition.y < 0 || gridPosition.y >= m_gridSize.y)
            return true;

        return m_grid[gridPosition.x + gridPosition.y * m_gridSize.x];
    }


    private void SetGridValue(Vector2Int gridPosition, bool isOccupied)
    {
        // Constraint position to grid
        gridPosition = Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(m_gridSize, gridPosition));

        m_grid[gridPosition.x + gridPosition.y * m_gridSize.x] = isOccupied;
    }



    /// <summary>
    /// Is the provided area on the grid unoccupied?
    /// </summary>
    /// <param name="gridPosition">The corner position of the area</param>
    /// <param name="areaSize">The size of the square area</param>
    /// <returns>The occupancy state of the area</returns>
    public bool IsGridAreaFree(Vector2Int gridPosition, int areaSize)
    {
        for (int x = 0; x < areaSize; x++)
        {
            for (int y = 0; y < areaSize; y++)
            {
                var position = gridPosition + new Vector2Int(x, y);
                if (GetGridValue(position))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Sets a provided area of the grid.
    /// </summary>
    /// <param name="gridPosition">The corner position of the area</param>
    /// <param name="areaSize">The size of the square area</param>
    /// <param name="isOccupied">The occupancy state of the area</param>
    public void SetGridArea(Vector2Int gridPosition, int areaSize, bool isOccupied)
    {
        for (int x = 0; x < areaSize; x++)
        {
            for (int y = 0; y < areaSize; y++)
            {
                var position = gridPosition + new Vector2Int(x, y);
                SetGridValue(position, isOccupied);
            }
        }
    }


    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f) * CELL_SIZE + m_gridOffset;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return Vector2Int.FloorToInt((worldPosition - m_gridOffset) * CELLS_PER_TILE);
    }


#if UNITY_EDITOR
    [MyBox.ButtonMethod]
    private void GenerateGrid()
    {
        // Save the new size and offset values
        m_gridSize = m_gridAreaSize * CELLS_PER_TILE;
        m_gridOffset = m_gridAreaOffset;

        // Generate a new grid array
        m_grid = new bool[m_gridSize.x * m_gridSize.y];

        // Go through all grid positions and check the path collider for overlaps
        for (int x = 0; x < m_gridSize.x; x++)
        {
            for (int y = 0; y < m_gridSize.y; y++)
            {
                var gridPosition = new Vector2Int(x, y);
                var worldPosition = GridToWorldPosition(gridPosition);

                // If the path collider overlaps this grid position, mark it as occupied
                if (m_pathCollider.OverlapPoint(worldPosition))
                    SetGridValue(gridPosition, true);
            }
        }

        Debug.Log($"Generated Grid containing {m_grid.Length} Cells.");
    }
#endif
    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (m_waypoints != null)
        {
            var pathColor = Color.yellow;

            Gizmos.color = pathColor;
            for (int i = 0; i < GetWaypointCount() - 1; i++)
            {
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(i + 1));
            }
        }

        if (m_grid != null)
        {
            for (int x = 0; x < m_gridSize.x; x++)
            {
                for (int y = 0; y < m_gridSize.y; y++)
                {
                    var gridPosition = new Vector2Int(x, y);
                    var gridValue = GetGridValue(gridPosition);

                    var gridColor = Color.blue;
                    if (gridValue)
                        gridColor = Color.red;

                    var worldPosition = GridToWorldPosition(gridPosition);

                    // gridColor.a = 1.0f;
                    // Gizmos.color = gridColor;
                    // Gizmos.DrawWireCube(cellPosition, Vector3.one / m_gridDivisions);

                    gridColor.a = 0.3f;
                    Gizmos.color = gridColor;
                    Gizmos.DrawCube(worldPosition, 0.7f * Vector3.one * CELL_SIZE);
                }
            }
        }
    }
#endif
}
