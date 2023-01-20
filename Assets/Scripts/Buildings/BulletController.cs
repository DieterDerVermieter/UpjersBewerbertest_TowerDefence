using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float m_movementSpeed = 5.0f;

    [SerializeField] private float m_hitRadius = 0.1f;
    [SerializeField] private float m_hitDamage = 10.0f;

    [SerializeField] private int m_maxPierce = 1;

    [SerializeField] private float m_maxLifetime = 5.0f;


    private Vector3 m_direction;

    private int m_currentPierce;
    private float m_currentLifetime;

    private HashSet<EnemyController> m_hitSet = new HashSet<EnemyController>();

    private static RaycastHit2D[] m_raycastResults = new RaycastHit2D[100];


    public void Setup(Vector3 direction)
    {
        m_direction = direction;
    }


    private void Update()
    {
        m_currentLifetime += Time.deltaTime;
        if(m_currentLifetime >= m_maxLifetime)
        {
            Destroy(gameObject);
        }
    }


    private void FixedUpdate()
    {
        var distance = m_movementSpeed * Time.fixedDeltaTime;
        EnemyController hitTarget = null;

        for (int i = 0; i < Physics2D.CircleCastNonAlloc(transform.position, m_hitRadius, m_direction, m_raycastResults); i++)
        {
            var result = m_raycastResults[i];
            if(result.collider.TryGetComponent<EnemyController>(out var enemy))
            {
                if (m_hitSet.Contains(enemy))
                    continue;

                if (result.distance < distance)
                {
                    hitTarget = enemy;
                    distance = result.distance;
                }
            }
        }

        transform.position += m_direction * distance;

        if(hitTarget != null)
        {
            hitTarget.TakeDamage(m_hitDamage);

            m_hitSet.Add(hitTarget);
            m_currentPierce++;

            if(m_currentPierce >= m_maxPierce)
                Destroy(gameObject);
        }
    }


    private void OnDrawGizmosSelected()
    {
        var hitRadiusColor = Color.green;

        Gizmos.color = hitRadiusColor;
        Gizmos.DrawWireSphere(transform.position, m_hitRadius);
    }
}
