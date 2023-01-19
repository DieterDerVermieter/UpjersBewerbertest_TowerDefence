using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [System.Serializable]
    public class BatchData
    {
        public EnemyData Enemy;
        public int Count = 1;
        public float Delay = 0.0f;
    }

    public class ActiveBatch
    {
        public BatchData Batch;

        public int CountLeft;
        public float Timer;


        public ActiveBatch(BatchData data)
        {
            Batch = data;

            CountLeft = data.Count;
            Timer = 0.0f;
        }
    }


    [System.Serializable]
    public class DelayedBatch
    {
        public BatchData Batch;
        public float Delay = 0.0f;
    }

    [System.Serializable]
    public class MultiBatchData
    {
        public List<DelayedBatch> Batches = new List<DelayedBatch>();
    }

    public class ActiveMultiBatch
    {
        public Queue<DelayedBatch> Batches = new Queue<DelayedBatch>();
        public float Timer = 0.0f;


        public ActiveMultiBatch(MultiBatchData data)
        {
            foreach (var batch in data.Batches)
            {
                Batches.Enqueue(batch);
            }
        }
    }


    [SerializeField] private MapLayout m_mapLayout;

    [Header("Testing")]
    [SerializeField] private MultiBatchData m_testMultiBatch;


    private List<ActiveBatch> m_activeBatches = new List<ActiveBatch>();
    private Queue<ActiveMultiBatch> m_multiBatchQueue = new Queue<ActiveMultiBatch>();


    private void Start()
    {
        SpawnMultiBatch(m_testMultiBatch);
    }


    public void SpawnMultiBatch(MultiBatchData data)
    {
        m_multiBatchQueue.Enqueue(new ActiveMultiBatch(data));
    }


    public void SpawnBatch(BatchData data)
    {
        m_activeBatches.Add(new ActiveBatch(data));
    }


    public void SpawnEnemy(EnemyData data)
    {
        var enemy = Instantiate(data.Prefab);

        enemy.transform.position = m_mapLayout.WaypointPosition(0);

        enemy.Setup(m_mapLayout);
    }


    private void Update()
    {
        if(m_multiBatchQueue.TryPeek(out var currentMultiBatch))
        {
            currentMultiBatch.Timer -= Time.deltaTime;

            if(currentMultiBatch.Timer <= 0)
            {
                SpawnBatch(currentMultiBatch.Batches.Dequeue().Batch);

                if(currentMultiBatch.Batches.TryPeek(out var nextBatch))
                {
                    currentMultiBatch.Timer += nextBatch.Delay;
                }
                else
                {
                    m_multiBatchQueue.Dequeue();
                }
            }
        }

        for (int i = 0; i < m_activeBatches.Count;)
        {
            var activeBatch = m_activeBatches[i];

            activeBatch.Timer -= Time.deltaTime;

            if(activeBatch.Timer <= 0)
            {
                SpawnEnemy(activeBatch.Batch.Enemy);

                activeBatch.CountLeft--;
                activeBatch.Timer += activeBatch.Batch.Delay;
            }

            if (activeBatch.CountLeft <= 0)
            {
                m_activeBatches.RemoveAt(i);
                continue;
            }

            i++;
        }
    }
}
