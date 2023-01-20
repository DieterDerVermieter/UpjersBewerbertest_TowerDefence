using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    [SerializeField] private Transform m_headContainer;
    [SerializeField] private Transform m_bulletSpawnPoint;

    [SerializeField] private BulletController m_bulletPrefab;

    [SerializeField] private float m_attackRadius = 5.0f;
    [SerializeField] private float m_attackSpeed = 0.5f;


    private float m_attackTimer;

    private static Collider2D[] m_overlappingColliders = new Collider2D[100];


    private void Update()
    {
        if(m_attackTimer > 0)
        {
            m_attackTimer -= Time.deltaTime;
        }
        else
        {
            EnemyController targetEnemy = null;
            float targetValue = 0;

            // filter enemies
            foreach (var enemy in EnemyController.ActiveEnemies)
            {
                var distanceSqrt = (enemy.HitCenter - transform.position).sqrMagnitude;

                var distanceThreshold = m_attackRadius + enemy.HitRadius;
                distanceThreshold *= distanceThreshold;

                if (distanceSqrt > distanceThreshold)
                    continue;

                float value = 0;

                // closest
                value = distanceSqrt;

                if(targetEnemy == null || value < targetValue)
                {
                    targetValue = value;
                    targetEnemy = enemy;
                }
            }

            if(targetEnemy != null)
            {
                m_attackTimer = 1 / m_attackSpeed;
                Attack(targetEnemy);
            }
        }
    }


    private void Attack(EnemyController target)
    {
        var direction = (target.transform.position - transform.position).normalized;

        m_headContainer.up = direction;

        var bullet = Instantiate(m_bulletPrefab, transform);
        bullet.transform.position = m_bulletSpawnPoint.position;
        bullet.Setup(direction);
    }


    private void OnDrawGizmosSelected()
    {
        var attackRadiusColor = Color.blue;

        Gizmos.color = attackRadiusColor;
        Gizmos.DrawWireSphere(transform.position, m_attackRadius);
    }
}
