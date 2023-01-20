using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Building,
        Defending
    }


    [SerializeField] private Button m_startWaveButton;

    [SerializeField] private List<WaveData> m_waves;


    public int CurrentWave { get; private set; }

    public GameState CurrentState { get; private set; }


    private void OnEnable()
    {
        m_startWaveButton.onClick.AddListener(StartWaveButtonOnClick);
    }

    private void OnDisable()
    {
        m_startWaveButton.onClick.RemoveListener(StartWaveButtonOnClick);
    }


    private void Start()
    {
        SetGameState(GameState.Building);
    }


    private void Update()
    {
        m_startWaveButton.interactable = CurrentState == GameState.Building;

        if(CurrentState == GameState.Defending)
        {
            if (!EnemySpawner.Instance.IsSpawning && EnemyController.ActiveEnemies.Count <= 0)
                SetGameState(GameState.Building);
        }
    }


    private void SetGameState(GameState nextState)
    {
        CurrentState = nextState;
    }


    private void StartWaveButtonOnClick()
    {
        if (CurrentState != GameState.Building)
            return;

        StartNextWave();
    }


    private void StartNextWave()
    {
        SetGameState(GameState.Defending);

        EnemySpawner.Instance.SpawnWave(m_waves[CurrentWave]);
        CurrentWave = (CurrentWave + 1) % m_waves.Count;
    }
}
