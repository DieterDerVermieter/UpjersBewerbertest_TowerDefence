using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Button m_startWaveButton;

    [SerializeField] private TMP_Text m_fastForwardButtonText;

    [SerializeField] private TMP_Text m_waveText;

    [SerializeField] private TMP_Text m_livesText;
    [SerializeField] private TMP_Text m_cashText;


    private void Update()
    {
        m_startWaveButton.interactable = GameManager.Instance.CurrentState == GameManager.GameState.Building;

        m_fastForwardButtonText.text = $"{GameManager.Instance.CurrentGameSpeed}x";

        m_waveText.text = $"{GameManager.Instance.CurrentWave + 1}/{GameManager.Instance.MaxWaves}";

        m_livesText.text = GameManager.Instance.CurrentLives.ToString();
        m_cashText.text = $"{GameManager.Instance.CurrentCash:N0}$";
    }


    public void OnStartWaveButton()
    {
        GameManager.Instance.StartNextWave();
    }

    public void OnFastForwardButton()
    {
        GameManager.Instance.ChangeGameSpeed();
    }
}
