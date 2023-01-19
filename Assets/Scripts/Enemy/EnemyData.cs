using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    public string DisplayName = "Cool Enemy";

    public EnemyController Prefab;
}
