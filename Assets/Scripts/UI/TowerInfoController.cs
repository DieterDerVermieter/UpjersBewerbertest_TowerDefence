using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class TowerInfoController : MonoBehaviour
{
    [SerializeField] private TMP_Text m_nameText;
    [SerializeField] private TMP_Dropdown m_targetModeDropdown;


    private BuildManager m_buildManager;
    private TowerController m_targetBuilding;


    public void Setup(BuildManager buildManager)
    {
        m_buildManager = buildManager;

        m_targetModeDropdown.ClearOptions();
        m_targetModeDropdown.AddOptions(System.Enum.GetNames(typeof(TowerController.TargetMode)).ToList());
    }


    public void SetTargetBuilding(TowerController building)
    {
        m_targetBuilding = building;

        m_nameText.text = m_targetBuilding.Data.DisplayName;
    }


    public void OnSellButton()
    {
        m_buildManager.SellSelectedBuilding();
    }


    public void OnTargetModeDropdown(int value)
    {
        m_targetBuilding.SetTargetMode((TowerController.TargetMode)value);
    }
}
