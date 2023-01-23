using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class TowerInfoController : MonoBehaviour
{
    [SerializeField] private TMP_Text m_nameText;


    private TowerController m_targetBuilding;


    public void SetTargetBuilding(TowerController building)
    {
        m_targetBuilding = building;

        m_nameText.text = m_targetBuilding.Data.DisplayName;
    }


    public void OnSellButton()
    {
        TowerManager.Instance.SellSelectedBuilding();
    }
}
