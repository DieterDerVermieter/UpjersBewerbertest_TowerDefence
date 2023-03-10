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
    public bool RotateVisuals = false;
    public bool CanFly = false;

    [Header("Health")]
    public int MaxHealth = 1;

    [Header("Children")]
    public bool OverflowDamage = true;
    public EnemyData[] Children;


    [MyBox.ButtonMethod]
    public void CalculateLeakDamage()
    {
        LeakDamage = MaxHealth;
        foreach (var childData in Children)
        {
            childData.CalculateLeakDamage();
            LeakDamage += childData.LeakDamage;
        }
    }
}
