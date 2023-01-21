using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyController : MonoBehaviour
{
    private CircleCollider2D m_circleCollider;

    private EnemyData m_data;

    private MapLayout m_mapLayout;
    private int m_nextWaypointIndex;

    private int m_identifier;

    private float m_currentHealth;


    public static List<EnemyController> ActiveEnemies { get; private set; } = new List<EnemyController>();

    public int Identifier => m_identifier;

    public float HitRadius => m_circleCollider.radius;
    public Vector3 HitCenter => transform.position + (Vector3)m_circleCollider.offset;


    private void Awake()
    {
        m_circleCollider = GetComponent<CircleCollider2D>();
    }


    private void OnEnable()
    {
        ActiveEnemies.Add(this);
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }


    public void Setup(EnemyData data, MapLayout mapLayout, int nextWaypoint, int identifier)
    {
        m_data = data;

        m_mapLayout = mapLayout;
        m_nextWaypointIndex = nextWaypoint;

        m_identifier = identifier;
    }


    private void Start()
    {
        if (m_data.CanFly)
            m_nextWaypointIndex = m_mapLayout.GetWaypointCount() - 1;

        m_currentHealth = m_data.MaxHealth;
    }


    private void FixedUpdate()
    {
        var distanceLeft = m_data.MovementSpeed * Time.fixedDeltaTime;
        bool m_reachedExit = false;

        while(distanceLeft > 0)
        {
            if(m_nextWaypointIndex >= m_mapLayout.GetWaypointCount())
            {
                m_reachedExit = true;
                break;
            }

            var targetPosition = m_mapLayout.GetWaypoint(m_nextWaypointIndex);
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
            Leak();
        }
    }


    public void TakeDamage(float amount)
    {
        if (amount < 0)
            return;

        m_currentHealth -= amount;

        if (m_currentHealth > 0)
        {
            Instantiate(m_data.HitEffectPrefab, transform.position, Quaternion.identity, transform.parent);
        }
        else
        {
            Die();
        }
    }


    public void Die()
    {
        GameManager.Instance.RewardCash(m_data.CashReward);

        if (m_data.Child != null)
            EnemySpawner.Instance.SpawnEnemy(m_data.Child, transform.position, m_nextWaypointIndex, m_identifier);

        Instantiate(m_data.DeathEffectPrefab, transform.position, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }


    public void Leak()
    {
        GameManager.Instance.LeakLifes(m_data.LeakDamage);

        Destroy(gameObject);
    }


    public float Progress()
    {
        var waypointA = m_mapLayout.GetWaypoint(m_nextWaypointIndex - 1);
        var waypointB = m_mapLayout.GetWaypoint(m_nextWaypointIndex);

        return m_nextWaypointIndex + transform.position.InverseLerp(waypointA, waypointB);
    }

    public float Strength()
    {
        return m_data.LeakDamage;
    }
}
