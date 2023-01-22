using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyData : GenericData<EnemyController>
{
    [Header("General Settings")]
    public string DisplayName = "Cool Enemy";

    [Header("Player")]
    public int LeakDamage = 1;
    public int CashReward = 1;

    [Header("Movement")]
    public float MovementSpeed = 1.0f;
    public bool CanFly = false;

    [Header("Health")]
    public float MaxHealth = 100.0f;
    public GameObject HitEffectPrefab;
    public GameObject DeathEffectPrefab;

    [Header("Child")]
    public EnemyData Child;
}
