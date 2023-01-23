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

    [Header("Preview")]
    [SerializeField] private SpriteRenderer m_sizeIndicator;
    [SerializeField] private Transform m_rangeIndicator;

    [Header("Attack")]
    [SerializeField] private Transform m_headContainer;
    [SerializeField] private Transform m_projectileSpawnPoint;


    // Flags for preview and selection
    private bool m_isPreview;
    private bool m_isSelected;

    // Current attack cooldown
    private float m_attackTimer;


    private void Start()
    {
        // Setup our preview indicators
        m_sizeIndicator.transform.localScale = Vector3.one * Data.GridSize * MapLayout.CELL_SIZE;
        m_rangeIndicator.transform.localScale = Vector3.one * Data.AttackRadius * 2;
    }


    /// <summary>
    /// Sets the current previewState and enables/disables the preview indicators accordingly.
    /// </summary>
    /// <param name="isPreview">The next preview state</param>
    public void SetPreviewState(bool isPreview)
    {
        m_isPreview = isPreview;

        m_sizeIndicator.gameObject.SetActive(isPreview);
        m_rangeIndicator.gameObject.SetActive(isPreview);
    }

    /// <summary>
    /// Sets the current selectionState and enables/disables the indicators accordingly.
    /// </summary>
    /// <param name="isSelected">The next selection state</param>
    public void SetSelectionState(bool isSelected)
    {
        m_isSelected = isSelected;

        m_sizeIndicator.gameObject.SetActive(isSelected);
        m_rangeIndicator.gameObject.SetActive(isSelected);
    }


    public void SetSizeIndicatorColor(Color color)
    {
        m_sizeIndicator.color = color;
    }


    private void Update()
    {
        // Don't do logic, if we are in preview state
        if (m_isPreview)
            return;

        // Can we try to attack?
        if(m_attackTimer > 0)
        {
            // No, countdown cooldown
            m_attackTimer -= Time.deltaTime;
        }
        else
        {
            // Yes, find enemy that best fits our targetMode and attack it
            if(TryFindTargetEnemy(out var enemy))
            {
                ShootAtEnemy(enemy);
                m_attackTimer = 1 / Data.AttackSpeed;
            }
        }
    }


    private bool TryFindTargetEnemy(out EnemyController targetEnemy)
    {
        // Calculate the maximum shooting distance
        var distanceThreshold = Data.AttackRadius * Data.AttackRadius;

        targetEnemy = null;
        float targetValue = 0;
        float targetProgress = 0;

        foreach (var enemy in EnemyController.ActiveEnemies)
        {
            // Is Enemy in range?
            var distanceSqrt = (enemy.HitCenter - transform.position).sqrMagnitude;
            if (distanceSqrt > distanceThreshold)
                continue;

            float progress = enemy.Progress();

            // Calculate a value based on our targetMode
            float value = Data.TargetMode switch
            {
                // Furthest on the path => biggest progrssion
                TargetMode.First => progress,

                // Last on the path => smallest progrssion
                TargetMode.Last => -progress,

                // Closest to the tower => smallest distance 
                TargetMode.Close => -distanceSqrt,

                // Strongest and most dangerous enemy for the player => biggest strength
                TargetMode.Strong => enemy.Strength(),

                _ => 0,
            };

            // If we have no target yet
            // or this enemy has a better filter value
            // or for the same filter values it is further on the path
            if (targetEnemy == null || value > targetValue || (value == targetValue && progress > targetProgress))
            {
                targetEnemy = enemy;

                targetProgress = progress;
                targetValue = value;
            }
        }

        return targetEnemy != null;
    }


    private void ShootAtEnemy(EnemyController enemy)
    {
        // Calculate direction to enemy
        var direction = (enemy.HitCenter - transform.position).normalized;

        // Rotate tower head to aim at that direction
        m_headContainer.up = direction;

        // Offset the direction based on our attackSpread
        // Do that after rotating, or else the tower jitters like the projectiles
        var directionOffset = Data.AttackSpread * Random.Range(-1.0f, 1.0f);
        direction = direction.RotateRad(directionOffset);

        // Spawn and setup the projectile
        var projectile = Instantiate(Data.ProjectilePrefab, transform);
        projectile.transform.position = m_projectileSpawnPoint.position;
        projectile.Setup(direction);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // This building got clicked
        // Tell the buildManager to select this building
        TowerManager.Instance.SelectTower(this);

        // Debug.Log($"{name}: OnPointerClick()");
    }
}
