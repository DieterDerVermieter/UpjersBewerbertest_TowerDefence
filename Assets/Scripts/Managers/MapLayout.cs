using UnityEngine;

public class MapLayout : Singleton<MapLayout>
{
    [System.Serializable]
    private class Waypoint
    {
        public Vector3 Position = Vector3.zero;
        public Vector3 Direction = Vector3.right;
        public float Distance = 0;
    }


    [Header("Path")]
    [SerializeField] private Transform m_groundWaypointsContainer;
    [SerializeField] private Transform m_airWaypointsContainer;

    [Header("Grid")]
    [SerializeField] private Vector2Int m_gridAreaSize;
    [SerializeField] private Vector3 m_gridAreaOffset;

    [SerializeField] private Collider2D m_pathCollider;


    // Generated waypoints
    [HideInInspector][SerializeField] private Waypoint[] m_waypointsGround;
    [HideInInspector][SerializeField] private Waypoint[] m_waypointsAir;

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
    /// <param name="isAirPosition">Calculate ground or air position</param>
    /// <returns>If the distance is still on the path</returns>
    public bool GetPositionOnPath(float distance, bool isAirPosition, out Vector3 position)
    {
        if (isAirPosition)
            return GetPositionOnPath(ref m_waypointsAir, distance, out position);

        return GetPositionOnPath(ref m_waypointsGround, distance, out position);
    }


    /// <summary>
    /// Calculates the direction on the path given a distance.
    /// </summary>
    /// <param name="distance">The distance along the path</param>
    /// <param name="direction">The direction along the path</param>
    /// <param name="isAirDirection">Calculate ground or air direction</param>
    /// <returns>If the distance is still on the path</returns>
    public bool GetDirectionOnPath(float distance, bool isAirDirection, out Vector3 direction)
    {
        if (isAirDirection)
            return GetDirectionOnPath(ref m_waypointsAir, distance, out direction);

        return GetDirectionOnPath(ref m_waypointsGround, distance, out direction);
    }


    private bool GetPositionOnPath(ref Waypoint[] waypoints, float distance, out Vector3 position)
    {
        if(GetWaypointOnPath(ref waypoints, ref distance, out var waypoint))
        {
            position = waypoint.Position + waypoint.Direction * distance;
            return true;
        }

        position = waypoint.Position;
        return false;
    }

    private bool GetDirectionOnPath(ref Waypoint[] waypoints, float distance, out Vector3 direction)
    {
        var isOnPath = GetWaypointOnPath(ref waypoints, ref distance, out var waypoint);
        direction = waypoint.Direction;
        return isOnPath;
    }

    private bool GetWaypointOnPath(ref Waypoint[] waypoints, ref float distance, out Waypoint waypoint)
    {
        // Find the waypoint that covers the distance
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoint = waypoints[i];

            if (waypoint.Distance > distance)
                return true;

            distance -= waypoint.Distance;
        }

        // Distance is too large
        waypoint = waypoints[waypoints.Length - 1];
        return false;
    }


#if UNITY_EDITOR
    [MyBox.ButtonMethod]
    private void GeneratePath()
    {
        // Generate ground waypoints
        float groundDistance = GenerateWaypoints(ref m_waypointsGround, m_groundWaypointsContainer);
        Debug.Log($"Generated ground Path containing {m_waypointsGround.Length} Waypoints and a total distance of {groundDistance}.");

        // Generate air waypoints
        float airDistance = GenerateWaypoints(ref m_waypointsAir, m_airWaypointsContainer);
        Debug.Log($"Generated air Path containing {m_waypointsAir.Length} Waypoints and a total distance of {airDistance}.");
    }


    private float GenerateWaypoints(ref Waypoint[] waypoints, Transform waypointContainer)
    {
        waypoints = new Waypoint[waypointContainer.childCount];
        float totalDistance = 0;

        // Generate the ground path based on the waypoints in the specified container
        for (int i = 0; i < waypointContainer.childCount; i++)
        {
            var position = waypointContainer.GetChild(i).position;

            // Last waypoint has no nextWaypoint
            if (i >= waypointContainer.childCount - 1)
            {
                waypoints[i] = new Waypoint
                {
                    Position = position,
                };

                continue;
            }

            // The next waypoint we are heading towards
            var nextPosition = waypointContainer.GetChild(i + 1).position;

            waypoints[i] = new Waypoint
            {
                Position = position,
                Direction = (nextPosition - position).normalized,
                Distance = (nextPosition - position).magnitude
            };

            totalDistance += waypoints[i].Distance;
        }

        return totalDistance;
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
        // Draw ground path
        if (m_waypointsGround != null)
        {
            var pathColor = Color.yellow;

            Gizmos.color = pathColor;
            for (int i = 0; i < m_waypointsGround.Length - 1; i++)
            {
                Gizmos.DrawLine(m_waypointsGround[i].Position, m_waypointsGround[i + 1].Position);
            }
        }

        // Draw air path
        if (m_waypointsAir != null)
        {
            var pathColor = Color.red;

            Gizmos.color = pathColor;
            for (int i = 0; i < m_waypointsAir.Length - 1; i++)
            {
                Gizmos.DrawLine(m_waypointsAir[i].Position, m_waypointsAir[i + 1].Position);
            }
        }

        // Draw grid
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
