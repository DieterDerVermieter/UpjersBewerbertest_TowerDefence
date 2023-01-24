using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileData : GenericData<ProjectileController>
{
    [Header("Stats")]
    public float MovementSpeed = 10.0f;
    public float MaxLifetime = 1.0f;

    public float HitRadius = 0.1f;
    public int HitDamage = 1;

    public int MaxPiercing = 1;
}
