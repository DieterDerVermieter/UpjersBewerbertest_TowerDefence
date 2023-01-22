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

    [Header("Attack")]
    public float AttackSpeed = 1.0f;
    public float AttackSpread = 0.0f;

    public ProjectileController ProjectilePrefab;
}
