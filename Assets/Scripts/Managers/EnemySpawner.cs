using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemySpawner : Singleton<EnemySpawner>
{
    #region Types
    /// <summary>
    /// A Batch is a group of a single enemy type with an even delay between them.
    /// </summary>
    [Serializable]
    public class Batch
    {
        public EnemyData Enemy;
        public int Count = 1;
        public float Delay = 0.0f;
    }

    /// <summary>
    /// A Batch that is currently being spawned.
    /// Contains fields to track the spawn progress.
    /// </summary>
    private class ActiveBatch
    {
        public Batch Batch;

        public int CountLeft;
        public float Timer;


        public ActiveBatch(Batch batch)
        {
            Batch = batch;

            CountLeft = batch.Count;
            Timer = 0.0f;
        }
    }
    #endregion


    // Batches that are currently being spawned
    private List<ActiveBatch> m_activeBatches = new List<ActiveBatch>();

    // Queued waves and fields to track wave spanwing progress
    private Queue<WaveData> m_waveQueue = new Queue<WaveData>();
    private float m_waveTimer;
    private int m_waveIndex;

    // This gets used as the enemies identifier
    private int m_spawnIndex;


    /// <summary>
    /// Are there active waves or batches being spawned?
    /// </summary>
    public bool IsSpawning { get; private set; }

    /// <summary>
    /// The spawn progress of the active wave.
    /// </summary>
    public float SpawnProgress { get; private set; }


    /// <summary>
    /// Eneque a new wave to be spawned. Only spawns on wave at a time.
    /// </summary>
    /// <param name="waveData">The wave to be spawned</param>
    public void SpawnWave(WaveData waveData)
    {
        m_waveQueue.Enqueue(waveData);

        IsSpawning = true;
    }


    /// <summary>
    /// Add a batch to the active batches list. Multiple Batches can be spawned at the same time.
    /// </summary>
    /// <param name="batch">The batch to be spawned</param>
    public void SpawnBatch(Batch batch)
    {
        m_activeBatches.Add(new ActiveBatch(batch));

        IsSpawning = true;
    }


    /// <summary>
    /// Spawne a single enemy with a unique identifier.
    /// </summary>
    /// <param name="enemyData">Enemy to be spawned</param>
    public void SpawnEnemy(EnemyData enemyData)
    {
        SpawnEnemy(enemyData, MapLayout.Instance.GetWaypoint(0), 1, m_spawnIndex);
        m_spawnIndex++;
    }

    /// <summary>
    /// Spawne a single enemy.
    /// </summary>
    /// <param name="data">Enemy to be spawned</param>
    /// <param name="position">World position to spawn the enemy at</param>
    /// <param name="nextWaypoint">The next waypoint the enemy should head towards</param>
    /// <param name="identifier">The identifier of the enemy</param>
    public void SpawnEnemy(EnemyData data, Vector3 position, int nextWaypoint, int identifier)
    {
        var enemy = Instantiate(data.ControllerPrefab, transform);

        enemy.transform.position = position;
        enemy.Setup(nextWaypoint, identifier);
    }


    private void Update()
    {
        if (!IsSpawning)
            return;

        ProcessWaves();
        ProcessBatches();

        // If there aren't any waves or batches left, stop spawning for now
        if (m_waveQueue.Count <= 0 && m_activeBatches.Count <= 0)
            IsSpawning = false;
    }


    private void ProcessWaves()
    {
        // Exit, if there is no wave to process
        if (!m_waveQueue.TryPeek(out var currentWave))
            return;

        // Should we spawn the next batch?
        m_waveTimer -= Time.deltaTime;
        if (m_waveTimer > 0)
            return;

        // Spawn the next batch
        SpawnBatch(currentWave.Batches[m_waveIndex].Batch);
        m_waveIndex++;

        // If it was the last batch, dequeue wave
        if (m_waveIndex >= currentWave.Batches.Count)
        {
            m_waveQueue.Dequeue();

            m_waveTimer = 0;
            m_waveIndex = 0;

            return;
        }

        // Increase our wave timer by the delay of the next batch
        m_waveTimer += currentWave.Batches[m_waveIndex].Delay;
    }


    private void ProcessBatches()
    {
        // Use a for loop, to modify list while iterating
        for (int i = 0; i < m_activeBatches.Count; i++)
        {
            var activeBatch = m_activeBatches[i];

            // Should we spawn the next enemy
            activeBatch.Timer -= Time.deltaTime;
            if (activeBatch.Timer > 0)
                continue;

            // Spawn the next enemy
            SpawnEnemy(activeBatch.Batch.Enemy);

            // Adjust our counter and timer
            activeBatch.CountLeft--;
            activeBatch.Timer += activeBatch.Batch.Delay;

            // Remove batch, if we spawned all enemies
            if (activeBatch.CountLeft <= 0)
            {
                m_activeBatches.RemoveAt(i);
                i--;
            }
        }
    }
}
