using System.Collections.Generic;
using UnityEngine;
using System;

public class ProjectileController : GenericController<ProjectileData>
{
    /// <summary>
    /// Comparer for <see cref="RaycastHit2D"/> that compares the distances.
    /// </summary>
    private class RaycastHitDistanceComparer : IComparer<RaycastHit2D>
    {
        public int Compare(RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance);
    }


    [SerializeField] private LayerMask m_enemyMask;


    // Should the logic be active
    private bool m_isActive;

    // Movement direction
    private Vector3 m_direction;

    // Tracking of lifetime and pierce
    private float m_currentLifetime;
    private int m_currentPierce;

    // Tracking of enemies hit
    private List<int> m_hitList = new List<int>();


    // Static array for raycasts
    private static RaycastHit2D[] s_raycastResults = new RaycastHit2D[100];

    // Comparer to sort raycastResults by distance
    private static RaycastHitDistanceComparer s_raycastResultComparer = new RaycastHitDistanceComparer();


    /// <summary>
    /// Setup this Projectile.
    /// </summary>
    /// <param name="direction">Movement direction</param>
    public void Setup(Vector3 direction)
    {
        m_isActive = true;
        m_direction = direction;
    }


    private void Update()
    {
        // Only do logic, if projectile is active
        if (!m_isActive)
            return;

        // Check, if the projectile is too old
        m_currentLifetime += Time.deltaTime;
        if (m_currentLifetime >= Data.MaxLifetime)
            Die();
    }


    private void FixedUpdate()
    {
        // Only do logic, if projectile is active
        if (!m_isActive)
            return;

        // Distance the projectile wants to travel this tick
        var distance = Data.MovementSpeed * Time.fixedDeltaTime;
        var hitPierceCap = false;

        // Raycast and sort results
        var resultCount = Physics2D.CircleCastNonAlloc(transform.position, Data.HitRadius, m_direction, s_raycastResults, distance, m_enemyMask);
        Array.Sort(s_raycastResults, 0, resultCount, s_raycastResultComparer);

        // Check all raycast results for enemies
        for (int i = 0; i < resultCount; i++)
        {
            var result = s_raycastResults[i];

            // Skip result, if it isn't an Enemy
            if (!result.collider.TryGetComponent<EnemyController>(out var enemy))
                continue;

            // Skip Enemy, if we hit it before
            if (m_hitList.Contains(enemy.Identifier))
                continue;

            // Hit Enemy
            enemy.TakeDamage(Data.HitDamage);

            // Track Enemy and increase Pierce
            m_hitList.Add(enemy.Identifier);
            m_currentPierce++;

            // Exit early, if we hit the maximum pierce
            if (m_currentPierce >= Data.MaxPiercing)
            {
                distance = result.distance;
                hitPierceCap = true;
                break;
            }
        }

        // Move
        transform.position += m_direction * distance;

        // Die, if we hit our pierce cap
        if (hitPierceCap)
            Die();
    }


    /// <summary>
    /// Destroys the GameObject.
    /// </summary>
    private void Die()
    {
        Destroy(gameObject);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var hitRadiusColor = Color.green;

        Gizmos.color = hitRadiusColor;
        Gizmos.DrawWireSphere(transform.position, Data.HitRadius);
    }
#endif
}
