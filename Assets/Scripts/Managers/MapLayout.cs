using UnityEngine;

public class MapLayout : Singleton<MapLayout>
{
    [System.Serializable]
    private class Waypoint
    {
        public Vector3 Position;
        public Vector3 Direction;
        public float Distance;
    }


    [Header("Path")]
    [SerializeField] private Transform m_waypointContainer;

    [Header("Grid")]
    [SerializeField] private Vector2Int m_gridAreaSize;
    [SerializeField] private Vector3 m_gridAreaOffset;

    [SerializeField] private Collider2D m_pathCollider;


    // Generated waypoints
    [HideInInspector][SerializeField] private Waypoint[] m_waypoints;

    // Generated grid
    [HideInInspector][SerializeField] private bool[] m_grid;
    [HideInInspector][SerializeField] private Vector2Int m_gridSize;
    [HideInInspector][SerializeField] private Vector3 m_gridOffset;


    // Size of the grid cells
    public const int CELLS_PER_TILE = 2;
    public const float CELL_SIZE = 1.0f / CELLS_PER_TILE;


    #region Path
    /// <summary>
    /// Calculates the position on the path given a distance.
    /// </summary>
    /// <param name="distance">The distance along the path</param>
    /// <param name="position">The position on the path</param>
    /// <returns>If the position is still on the path</returns>
    public bool GetPositionOnPath(float distance, out Vector3 position)
    {
        Waypoint waypoint = null;
        int waypointIndex = 0;

        // Loop trough all waypoints but the last one
        while(waypointIndex < m_waypoints.Length - 1)
        {
            waypoint = m_waypoints[waypointIndex];
            if (waypoint.Distance > distance)
            {
                // Found a waypoint that reaches further than we want to go
                position = waypoint.Position + waypoint.Direction * distance;
                return true;
            }
            else
            {
                distance -= waypoint.Distance;
                waypointIndex++;
            }
        }

        // If we are further then the second last one, return the position of the last one
        // Also, we are no longer on the track, so return false
        position = m_waypoints[waypointIndex].Position;
        return false;
    }


#if UNITY_EDITOR
    [MyBox.ButtonMethod]
    private void GeneratePath()
    {
        m_waypoints = new Waypoint[m_waypointContainer.childCount];
        float totalDistance = 0;

        // Generate the path based on the waypoints in the specified container
        for (int i = 0; i < m_waypointContainer.childCount; i++)
        {
            var position = m_waypointContainer.GetChild(i).position;

            // Last waypoint has no nextWaypoint
            if (i >= m_waypointContainer.childCount - 1)
            {
                m_waypoints[i] = new Waypoint
                {
                    Position = position,
                };

                continue;
            }

            var nextPosition = m_waypointContainer.GetChild(i + 1).position;

            m_waypoints[i] = new Waypoint
            {
                Position = position,
                Direction = (nextPosition - position).normalized,
                Distance = (nextPosition - position).magnitude
            };

            totalDistance += m_waypoints[i].Distance;
        }

        Debug.Log($"Generated Path containing {m_waypoints.Length} Waypoints and a total distance of {totalDistance}.");
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
            for (int i = 0; i < m_waypoints.Length - 1; i++)
            {
                Gizmos.DrawLine(m_waypoints[i].Position, m_waypoints[i + 1].Position);
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
