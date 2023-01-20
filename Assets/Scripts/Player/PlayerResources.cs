using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private TMP_Text m_lifesText;
    [SerializeField] private TMP_Text m_cashText;

    [Header("Stats")]
    [SerializeField] private int m_startingLifes = 100;
    [SerializeField] private int m_startingCash = 500;


    [HideInInspector] public int CurrentLifes;
    [HideInInspector] public int CurrentCash;


    private void Start()
    {
        CurrentLifes = m_startingLifes;
        CurrentCash = m_startingCash;
    }


    private void Update()
    {
        m_lifesText.text = CurrentLifes.ToString();
        m_cashText.text = CurrentCash.ToString("###,###");
    }
}
