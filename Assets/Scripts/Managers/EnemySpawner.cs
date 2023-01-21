using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Singleton<EnemySpawner>
{
    public class ActiveBatch
    {
        public WaveData.Batch Batch;

        public int CountLeft;
        public float Timer;


        public ActiveBatch(WaveData.Batch data)
        {
            Batch = data;

            CountLeft = data.Count;
            Timer = 0.0f;
        }
    }


    [SerializeField] private MapLayout m_mapLayout;


    private List<ActiveBatch> m_activeBatches = new List<ActiveBatch>();

    private Queue<WaveData> m_waveQueue = new Queue<WaveData>();
    private float m_waveTimer;
    private int m_waveIndex;

    private int m_spawnIndex;


    public bool IsSpawning { get; private set; }


    public void SpawnWave(WaveData data)
    {
        m_waveQueue.Enqueue(data);

        IsSpawning = true;
    }


    public void SpawnBatch(WaveData.Batch data)
    {
        m_activeBatches.Add(new ActiveBatch(data));

        IsSpawning = true;
    }


    public void SpawnEnemy(EnemyData data)
    {
        SpawnEnemy(data, m_mapLayout.GetWaypoint(0), 1, m_spawnIndex);
        m_spawnIndex++;
    }

    public void SpawnEnemy(EnemyData data, Vector3 position, int nextWaypoint, int identifier)
    {
        var enemy = Instantiate(data.Prefab, transform);

        enemy.transform.position = position;
        enemy.Setup(data, m_mapLayout, nextWaypoint, identifier);
    }


    private void Update()
    {
        if (!IsSpawning)
            return;

        if(m_waveQueue.TryPeek(out var currentWave))
        {
            m_waveTimer -= Time.deltaTime;

            if(m_waveTimer <= 0)
            {
                SpawnBatch(currentWave.Batches[m_waveIndex].Batch);
                m_waveIndex++;

                if (m_waveIndex < currentWave.Batches.Count)
                {
                    m_waveTimer += currentWave.Batches[m_waveIndex].Delay;
                }
                else
                {
                    m_waveQueue.Dequeue();

                    m_waveTimer = 0;
                    m_waveIndex = 0;
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

        if (m_waveQueue.Count <= 0 && m_activeBatches.Count <= 0)
            IsSpawning = false;
    }
}
