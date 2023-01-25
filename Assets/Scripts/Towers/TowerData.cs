using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerData : GenericData<TowerController>
{
    [Header("General Settings")]
    public string DisplayName = "Cool Building";
    public Sprite Icon;

    [Header("Shop")]
    public int Price = 100;

    [Header("Grid")]
    public int GridSize = 2;

    [Header("Attack")]
    public bool CanHitFlyingEnemies = true;

    public float AttackRadius = 2.0f;
    public TowerController.TargetMode TargetMode;

    [Min(0.1f)] public float AttackSpeed = 1.0f;
    [Range(0, 180)] public float AttackSpread = 0.0f;

    [Min(1)] public int ProjectileCount = 1;
    [Range(0, 180)] public float ProjectileSpread = 90.0f;

    public ProjectileController ProjectilePrefab;
}
