using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerShopItemController : MonoBehaviour
{
    [SerializeField] private Image m_iconImage;
    [SerializeField] private TMP_Text m_priceText;


    private TowerData m_data;


    public void Setup(TowerData data)
    {
        m_data = data;
    }


    public TowerData GetData() => m_data;


    private void Start()
    {
        m_iconImage.sprite = m_data.Icon;
        m_priceText.text = m_data.Price.ToString("#,#$");
    }
}
