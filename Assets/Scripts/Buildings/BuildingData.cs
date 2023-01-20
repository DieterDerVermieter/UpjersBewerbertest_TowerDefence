using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BuildingData : ScriptableObject
{
    public string DisplayName = "Cool Building";
    public Sprite Icon;

    public BuildingController Prefab;

    public int Price = 100;
    public int Size = 2;
}
