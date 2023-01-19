using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float m_movementSpeed = 1.0f;
    [SerializeField] private bool m_canFly = false;


    private MapLayout m_mapLayout;

    private int m_nextWaypointIndex = 1;


    public void Setup(MapLayout mapLayout)
    {
        m_mapLayout = mapLayout;
    }


    private void Start()
    {
        if (m_canFly)
            m_nextWaypointIndex = m_mapLayout.WaypointCount - 1;
    }


    private void FixedUpdate()
    {
        var distanceLeft = m_movementSpeed * Time.fixedDeltaTime;
        bool m_reachedExit = false;

        while(distanceLeft > 0)
        {
            if(m_nextWaypointIndex >= m_mapLayout.WaypointCount)
            {
                m_reachedExit = true;
                break;
            }

            var targetPosition = m_mapLayout.WaypointPosition(m_nextWaypointIndex);
            var vectorToTarget = targetPosition - transform.position;

            var distance = distanceLeft;

            var distanceToTarget = vectorToTarget.magnitude;
            if(distanceToTarget < distance)
            {
                distance = distanceToTarget;
                m_nextWaypointIndex++;
            }

            transform.position += vectorToTarget.normalized * distance;
            distanceLeft -= distance;
        }

        if (m_reachedExit)
        {
            Destroy(gameObject);
        }
    }
}
