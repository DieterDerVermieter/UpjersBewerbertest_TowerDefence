using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class Batch
    {
        public EnemyData Enemy;
        public int Count = 1;
        public float Delay = 0.0f;
    }

    [System.Serializable]
    public class DelayedBatch
    {
        public Batch Batch;
        public float Delay = 0.0f;
    }


    public List<DelayedBatch> Batches;
}
