using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TowerData : ScriptableObject
{
    public string DisplayName = "Cool Building";
    public Sprite Icon;

    public TowerController Prefab;

    public int Price = 100;

    [Header("Attack")]
    public float AttackSpeed = 1.0f;
    public float AttackSpread = 0.0f;

    public ProjectileController ProjectilePrefab;
}
