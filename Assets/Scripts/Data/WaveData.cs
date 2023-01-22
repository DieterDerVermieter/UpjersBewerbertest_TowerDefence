using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class WaveData : ScriptableObject
{
    [Serializable]
    public class DelayedBatch
    {
        public EnemySpawner.Batch Batch;
        public float Delay = 0.0f;
    }

    public List<DelayedBatch> Batches;
}
