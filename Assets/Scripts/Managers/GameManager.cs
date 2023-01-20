using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Building,
        Defending
    }


    [Header("References")]
    [SerializeField] private Button m_startWaveButton;
    [SerializeField] private TMP_Text m_startWaveButtonText;

    [SerializeField] private Button m_fastForwardButton;
    [SerializeField] private TMP_Text m_fastForwardButtonText;

    [SerializeField] private TMP_Text m_lifesText;
    [SerializeField] private TMP_Text m_cashText;

    [Header("Waves")]
    [SerializeField] private List<WaveData> m_waves;

    [Header("Resources")]
    [SerializeField] private int m_startingLifes = 100;
    [SerializeField] private int m_startingCash = 500;

    [SerializeField] private int m_endOfRoundCash = 100;


    private float m_gameSpeedMultiplier = 1;


    public GameState CurrentState { get; private set; }

    public int CurrentWave { get; private set; }

    public int CurrentLifes { get; private set; }
    public int CurrentCash { get; private set; }


    private void OnEnable()
    {
        m_startWaveButton.onClick.AddListener(StartWaveButtonOnClick);
        m_fastForwardButton.onClick.AddListener(FastForwardButtonOnClick);
    }

    private void OnDisable()
    {
        m_startWaveButton.onClick.RemoveListener(StartWaveButtonOnClick);
        m_fastForwardButton.onClick.RemoveListener(FastForwardButtonOnClick);
    }


    private void Start()
    {
        SetGameState(GameState.Building);

        CurrentLifes = m_startingLifes;
        CurrentCash = m_startingCash;

        m_startWaveButtonText.text = $"Start Wave {CurrentWave + 1}";
        m_fastForwardButtonText.text = $"{m_gameSpeedMultiplier}x";
    }


    private void Update()
    {
        if (CurrentState == GameState.Defending)
        {
            Time.timeScale = m_gameSpeedMultiplier;

            if (!EnemySpawner.Instance.IsSpawning && EnemyController.ActiveEnemies.Count <= 0)
            {
                m_startWaveButtonText.text = $"Start Wave {CurrentWave + 1}";

                Time.timeScale = 1;

                RewardCash(m_endOfRoundCash);
                SetGameState(GameState.Building);
            }
        }

        m_startWaveButton.interactable = CurrentState == GameState.Building;

        m_lifesText.text = CurrentLifes.ToString();
        m_cashText.text = CurrentCash.ToString("#,#$");
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

    private void FastForwardButtonOnClick()
    {
        if(m_gameSpeedMultiplier < 8)
        {
            m_gameSpeedMultiplier *= 2;
        }
        else
        {
            m_gameSpeedMultiplier = 1;
        }

        m_fastForwardButtonText.text = $"{m_gameSpeedMultiplier}x";
    }


    private void StartNextWave()
    {
        SetGameState(GameState.Defending);

        EnemySpawner.Instance.SpawnWave(m_waves[CurrentWave]);
        CurrentWave = (CurrentWave + 1) % m_waves.Count;
    }


    public void RewardCash(int amount)
    {
        if (amount < 0)
            return;

        CurrentCash += amount;
    }

    public void LeakLifes(int amount)
    {
        if (amount < 0)
            return;

        CurrentLifes -= amount;
    }
}
