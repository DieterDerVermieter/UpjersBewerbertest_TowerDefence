using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : GenericController<TowerData>, IPointerClickHandler
{
    public enum TargetMode
    {
        First,
        Last,
        Close,
        Strong
    }

    [Header("Indicators")]
    [SerializeField] private SpriteRenderer m_sizeIndicator;
    [SerializeField] private Transform m_rangeIndicator;

    [Header("Active")]
    [SerializeField] private Transform m_headContainer;
    [SerializeField] private Transform m_bulletSpawnPoint;

    [SerializeField] private float m_attackRadius = 5.0f;
    [SerializeField] private int m_gridSize = 2;

    [SerializeField] private TargetMode m_targetMode;


    private BuildManager m_buildManager;

    private TowerData m_data;

    private bool m_isPreview;
    private bool m_isSelected;

    private float m_attackTimer;

    private static Collider2D[] m_overlappingColliders = new Collider2D[100];


    public TowerData MyData => m_data;

    public int GridSize => m_gridSize;


    private void OnValidate()
    {
        if(m_sizeIndicator != null)
        {
            m_sizeIndicator.transform.localScale = Vector3.one * m_gridSize * MapLayout.CELL_SIZE;
        }

        if (m_rangeIndicator != null)
        {
            m_rangeIndicator.transform.localScale = Vector3.one * m_attackRadius * 2;
        }
    }


    public void Setup(BuildManager buildManager, TowerData data)
    {
        m_buildManager = buildManager;
        m_data = data;
    }


    public void SetTargetMode(TargetMode mode)
    {
        m_targetMode = mode;
    }


    public void SetPreviewState(bool isPreview)
    {
        m_isPreview = isPreview;

        m_sizeIndicator.gameObject.SetActive(isPreview);
        m_rangeIndicator.gameObject.SetActive(isPreview);
    }

    public void SetSizeIndicatorColor(Color color)
    {
        m_sizeIndicator.color = color;
    }


    public void SetSelectionState(bool isSelected)
    {
        m_isSelected = isSelected;

        m_sizeIndicator.gameObject.SetActive(isSelected);
        m_rangeIndicator.gameObject.SetActive(isSelected);
    }


    private void Update()
    {
        if (m_isPreview)
            return;

        if(m_attackTimer > 0)
        {
            m_attackTimer -= Time.deltaTime;
        }
        else
        {
            EnemyController targetEnemy = null;
            float targetValue = 0;
            float targetProgress = 0;

            // filter enemies
            foreach (var enemy in EnemyController.ActiveEnemies)
            {
                var distanceSqrt = (enemy.HitCenter - transform.position).sqrMagnitude;

                var distanceThreshold = m_attackRadius + enemy.HitRadius;
                distanceThreshold *= distanceThreshold;

                if (distanceSqrt > distanceThreshold)
                    continue;

                var progress = enemy.Progress();

                float value = m_targetMode switch
                {
                    TargetMode.First => enemy.Progress(),
                    TargetMode.Last => -enemy.Progress(),
                    TargetMode.Close => -distanceSqrt,
                    TargetMode.Strong => enemy.Strength(),
                    _ => 0,
                };

                if(targetEnemy == null || value > targetValue || (value == targetValue && progress > targetProgress))
                {
                    targetEnemy = enemy;

                    targetProgress = progress;
                    targetValue = value;
                }
            }

            if(targetEnemy != null)
            {
                m_attackTimer = 1 / m_data.AttackSpeed;
                Attack(targetEnemy);
            }
        }
    }


    private void Attack(EnemyController target)
    {
        var direction = (target.transform.position - transform.position).normalized;
        var directionOffset = m_data.AttackSpread * Random.Range(-1.0f, 1.0f);

        m_headContainer.up = direction;

        direction = direction.RotateRad(directionOffset);

        var bullet = Instantiate(m_data.ProjectilePrefab, transform);
        bullet.transform.position = m_bulletSpawnPoint.position;
        bullet.Setup(direction);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        m_buildManager.SelectBuilding(this);

        Debug.Log($"{name}: OnPointerClick()");
    }
}
