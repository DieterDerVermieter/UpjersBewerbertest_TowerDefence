using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyController : GenericController<EnemyData>
{
    private CircleCollider2D m_circleCollider;

    private bool m_isActive;
    private int m_identifier;

    private int m_nextWaypointIndex;

    private float m_currentHealth;


    /// <summary>
    /// All active enemies in the scene.
    /// </summary>
    public static List<EnemyController> ActiveEnemies { get; private set; } = new List<EnemyController>();

    /// <summary>
    /// Identifer of this enemy. Gets passed to the child of this enemy.
    /// </summary>
    public int Identifier => m_identifier;

    /// <summary>
    /// The world position attacks should be aimed at
    /// </summary>
    public Vector3 HitCenter => transform.position + (Vector3)m_circleCollider.offset;


    private void Awake()
    {
        m_circleCollider = GetComponent<CircleCollider2D>();
    }


    /// <summary>
    /// Setup this enemy.
    /// </summary>
    /// <param name="nextWaypoint">Waypoint the enemy should head towards.</param>
    /// <param name="identifier">Identifier of this enemy</param>
    public void Setup(int nextWaypoint, int identifier)
    {
        m_nextWaypointIndex = nextWaypoint;
        m_identifier = identifier;

        // Activate enemy
        ActiveEnemies.Add(this);
        m_isActive = true;
    }


    private void Start()
    {
        // If we can fly, skip all waypoints and head towards last one
        if (Data.CanFly)
            m_nextWaypointIndex = MapLayout.Instance.GetWaypointCount() - 1;

        // Fill up our health
        m_currentHealth = Data.MaxHealth;
    }


    private void FixedUpdate()
    {
        // Skip logic if enemy isn't active
        if (!m_isActive)
            return;

        // Distance we want to travel this tick
        var distanceLeft = Data.MovementSpeed * Time.fixedDeltaTime;
        bool m_reachedExit = false;

        // Move while we have distance left
        while(distanceLeft > 0)
        {
            // Did we reach the exit?
            if(m_nextWaypointIndex >= MapLayout.Instance.GetWaypointCount())
            {
                m_reachedExit = true;
                break;
            }

            // Calculate position of the next waypoint
            var targetPosition = MapLayout.Instance.GetWaypoint(m_nextWaypointIndex);
            var vectorToTarget = targetPosition - transform.position;

            var distance = distanceLeft;

            // Calulate distance to next waypoint
            var distanceToTarget = vectorToTarget.magnitude;
            if(distanceToTarget < distance)
            {
                distance = distanceToTarget;
                m_nextWaypointIndex++;
            }

            // Move
            transform.position += vectorToTarget.normalized * distance;
            distanceLeft -= distance;
        }

        // Leak, if we reached the exit
        if (m_reachedExit)
        {
            Leak();
        }
    }


    /// <summary>
    /// Deals damage to the enemy. Can cause death, if enemy health reches zero.
    /// </summary>
    /// <param name="amount">The amount of damage to deal to the enemy</param>
    public void TakeDamage(float amount)
    {
        if (amount < 0)
            return;

        m_currentHealth -= amount;

        // Did we die?
        if (m_currentHealth <= 0)
        {
            Die();
            return;
        }

        // Spawn a hit effect, if we didn't die
        Instantiate(Data.HitEffectPrefab, transform.position, Quaternion.identity, transform.parent);
    }


    /// <summary>
    /// Deactivates this enemy and destroys it.
    /// Rewards cash to the player.
    /// Spawns a child if it has one.
    /// </summary>
    public void Die()
    {
        // Deactivate enemy
        ActiveEnemies.Remove(this);
        m_isActive = false;

        // Reward cash to the player
        GameManager.Instance.RewardCash(Data.CashReward);

        // Spawn a child enemy with our position and identifier
        if (Data.Child != null)
            EnemySpawner.Instance.SpawnEnemy(Data.Child, transform.position, m_nextWaypointIndex, m_identifier);

        // Spawn a death effect
        Instantiate(Data.DeathEffectPrefab, transform.position, Quaternion.identity, transform.parent);

        Destroy(gameObject);
    }


    /// <summary>
    /// Deactivates this enemy and destroys it.
    /// Deals leak damage to the player.
    /// </summary>
    public void Leak()
    {
        // Deactivate enemy
        ActiveEnemies.Remove(this);
        m_isActive = false;

        // Deal leak damage to the player
        GameManager.Instance.LeakLives(Data.LeakDamage);

        Destroy(gameObject);
    }


    /// <summary>
    /// Calculates the progress of the enemy on the path.
    /// Used for tower targeting.
    /// </summary>
    /// <returns>The current progress of the enemy</returns>
    public float Progress()
    {
        // Use last waypoint for ground units and first for flying units
        var waypointA = Data.CanFly ? MapLayout.Instance.GetWaypoint(0) : MapLayout.Instance.GetWaypoint(m_nextWaypointIndex - 1);
        var waypointB = MapLayout.Instance.GetWaypoint(m_nextWaypointIndex);

        return m_nextWaypointIndex + transform.position.InverseLerp(waypointA, waypointB);
    }

    /// <summary>
    /// Calculates the enemies strength.
    /// Used for tower targeting.
    /// </summary>
    /// <returns>The strength of the enemy</returns>
    public float Strength()
    {
        return Data.LeakDamage;
    }
}
