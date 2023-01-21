using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLayout : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform m_waypointContainer;

    [Header("Grid")]
    [SerializeField] private Vector2Int m_newGridSize;
    [SerializeField] private Vector3 m_newGridOffset;

    [SerializeField] private Collider2D m_pathCollider;


    [HideInInspector][SerializeField] private Vector3[] m_waypoints;

    [HideInInspector][SerializeField] private int[] m_grid;

    [HideInInspector][SerializeField] private Vector2Int m_gridSize;
    [HideInInspector][SerializeField] private Vector3 m_gridOffset;


    public const int CELLS_PER_TILE = 2;
    public const float CELL_SIZE = 1.0f / CELLS_PER_TILE;


    public int GetWaypointCount() => m_waypoints.Length;
    public Vector3 GetWaypoint(int index) => m_waypoints[index];

    public Vector2Int GetGridSize() => m_gridSize;

    public int GetGridCell(int x, int y) => m_grid[x + y * m_gridSize.x];

    public Vector3 GridToWorldPosition(int x, int y) => new Vector3(x + 0.5f, y + 0.5f) * CELL_SIZE + m_gridOffset;
    public Vector2Int WorldToGridPosition(Vector3 worldPosition) => Vector2Int.FloorToInt((worldPosition - m_gridOffset) * CELLS_PER_TILE);

    public void SetGridCell(int x, int y, int value) => m_grid[x + y * m_gridSize.x] = value;

    public bool IsGridAreaBlocked(int x, int y, int size)
    {
        for (int oX = 0; oX < size; oX++)
        {
            for (int oY = 0; oY < size; oY++)
            {
                var cell = GetGridCell(x + oX, y + oY);
                if (cell > 0)
                    return true;
            }
        }

        return false;
    }

    public void SetGridArea(int x, int y, int size, int value)
    {
        for (int oX = 0; oX < size; oX++)
        {
            for (int oY = 0; oY < size; oY++)
            {
                SetGridCell(x + oX, y + oY, value);
            }
        }
    }


    [MyBox.ButtonMethod]
    private void GeneratePath()
    {
        m_waypoints = new Vector3[m_waypointContainer.childCount];

        for (int i = 0; i < m_waypointContainer.childCount; i++)
        {
            m_waypoints[i] = m_waypointContainer.GetChild(i).position;
        }

        Debug.Log($"Generated Path containing {GetWaypointCount()} Waypoints.");
    }


    [MyBox.ButtonMethod]
    private void GenerateGrid()
    {
        m_gridSize = m_newGridSize * CELLS_PER_TILE;

        m_gridOffset = m_newGridOffset;

        m_grid = new int[m_gridSize.x * m_gridSize.y];

        for (int x = 0; x < GetGridSize().x; x++)
        {
            for (int y = 0; y < GetGridSize().y; y++)
            {
                var cellPosition = GridToWorldPosition(x, y);

                if (m_pathCollider.OverlapPoint(cellPosition))
                    SetGridCell(x, y, 1);
            }
        }

        Debug.Log($"Generated Grid containing {m_grid.Length} Cells.");
    }


    private void OnDrawGizmosSelected()
    {
        if(m_waypoints != null)
        {
            var pathColor = Color.yellow;

            Gizmos.color = pathColor;
            for (int i = 0; i < GetWaypointCount() - 1; i++)
            {
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(i + 1));
            }
        }

        if(m_grid != null)
        {
            for (int x = 0; x < GetGridSize().x; x++)
            {
                for (int y = 0; y < GetGridSize().y; y++)
                {
                    var cell = GetGridCell(x, y);

                    var gridColor = Color.blue;
                    if (cell > 0)
                        gridColor = Color.red;

                    var cellPosition = GridToWorldPosition(x, y);

                    // gridColor.a = 1.0f;
                    // Gizmos.color = gridColor;
                    // Gizmos.DrawWireCube(cellPosition, Vector3.one / m_gridDivisions);

                    gridColor.a = 0.3f;
                    Gizmos.color = gridColor;
                    Gizmos.DrawCube(cellPosition, 0.7f * Vector3.one * CELL_SIZE);
                }
            }
        }
    }
}
