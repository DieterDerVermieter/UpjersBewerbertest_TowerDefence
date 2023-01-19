using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLayout : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform m_waypointContainer;


    public int WaypointCount => m_waypointContainer.childCount;

    public Vector3 WaypointPosition(int index)
    {
        if (index < 0 || index >= WaypointCount)
            return Vector3.zero;

        return m_waypointContainer.GetChild(index).position;
    }


    private void OnDrawGizmos()
    {
        var pathColor = Color.yellow;

        Gizmos.color = pathColor;
        for (int i = 0; i < WaypointCount - 1; i++)
        {
            Gizmos.DrawLine(WaypointPosition(i), WaypointPosition(i + 1));
        }
    }
}
