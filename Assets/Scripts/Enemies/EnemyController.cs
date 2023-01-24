using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyController : GenericController<EnemyData>
{
    [SerializeField] private Transform m_visualsContainer;


    private CircleCollider2D m_circleCollider;

    private bool m_isActive;

    private int m_currentHealth;


    /// <summary>
    /// All active enemies in the scene.
    /// </summary>
    public static List<EnemyController> ActiveEnemies { get; private set; } = new List<EnemyController>();

    /// <summary>
    /// The distance along the path the enemy has traveled.
    /// </summary>
    public float DistanceAlongPath { get; private set; }

    /// <summary>
    /// Identifer of this enemy.
    /// </summary>
    public int Identifier { get; private set; }

    /// <summary>
    /// Identifer of the parent of this enemy.
    /// Gets passed to the child of this enemy. Negative values mean no parent.
    /// </summary>
    public int ParentIdentifier { get; private set; }

    /// <summary>
    /// The world position attacks should be aimed at
    /// </summary>
    public Vector3 HitCenter => transform.position + (Vector3)m_circleCollider.offset;

    /// <summary>
    /// The radius of the hitBox
    /// </summary>
    public float HitRadius => m_circleCollider.radius;


    private void Awake()
    {
        m_circleCollider = GetComponent<CircleCollider2D>();
    }


    /// <summary>
    /// Setup this enemy.
    /// </summary>
    /// <param name="distanceAlongPath">Distance along the path</param>
    /// <param name="identifier">Identifier of this enemy</param>
    /// <param name="parentIdentifier">Identifier of the parent of this enemy</param>
    /// <param name="overflowDamage">The enemy starts with reduced health</param>
    public void Setup(float distanceAlongPath, int identifier, int parentIdentifier, int overflowDamage)
    {
        DistanceAlongPath = distanceAlongPath;
        Identifier = identifier;
        ParentIdentifier = parentIdentifier;

        // Setup initial position
        MapLayout.Instance.GetPositionOnPath(distanceAlongPath, Data.CanFly, out var position);
        transform.position = position;

        // Setup health
        m_currentHealth = Data.MaxHealth - overflowDamage;

        // Activate enemy
        ActiveEnemies.Add(this);
        m_isActive = true;
    }


    private void OnDestroy()
    {
        // If the object gets destroyed through some outside force, remove it from the list
        if(m_isActive)
        {
            ActiveEnemies.Remove(this);
        }
    }


    private void FixedUpdate()
    {
        // Skip logic if enemy isn't active
        if (!m_isActive)
            return;

        // Increase distance along the path
        DistanceAlongPath += Data.MovementSpeed * Time.fixedDeltaTime;

        // Have we reached the exit?
        if(!MapLayout.Instance.GetPositionOnPath(DistanceAlongPath, Data.CanFly, out var position))
        {
            Leak();
        }

        // Move along the path
        transform.position = position;

        // Rotate towards direction on path
        if (Data.RotateVisuals)
        {
            MapLayout.Instance.GetDirectionOnPath(DistanceAlongPath, Data.CanFly, out var direction);
            m_visualsContainer.right = direction;
        }
    }


    /// <summary>
    /// Deals damage to the enemy. Can cause death, if enemy health reches zero.
    /// </summary>
    /// <param name="amount">The amount of damage to deal to the enemy</param>
    public void TakeDamage(int amount)
    {
        if (amount < 0)
            return;

        m_currentHealth -= amount;

        // Did we die?
        if (m_currentHealth <= 0)
        {
            int overflowDamage = 0;
            if (Data.OverflowDamage)
                overflowDamage = -m_currentHealth;

            Die(overflowDamage);
        }
    }


    /// <summary>
    /// Deactivates this enemy and destroys it.
    /// Rewards cash to the player.
    /// Spawns a child if it has one.
    /// </summary>
    public void Die(int overflowDamage)
    {
        // Deactivate enemy
        ActiveEnemies.Remove(this);
        m_isActive = false;

        // Reward cash to the player
        GameManager.Instance.RewardCash(Data.CashReward);

        // Spawn children
        EnemySpawner.Instance.SpawnChildren(Data, DistanceAlongPath, Identifier, overflowDamage);

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
}
