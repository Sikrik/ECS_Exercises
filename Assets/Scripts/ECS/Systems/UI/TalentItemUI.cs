using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 单个天赋格子的 UI 控制脚本
/// </summary>
public class TalentItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI CostText;
    public Button UpgradeButton;

    /// <summary>
    /// 设置格子的数据并显示
    /// </summary>
    public void Setup(TalentData data, int currentLevel, int currentCost, int currentGold, System.Action onClickAction)
    {
        if (NameText) NameText.text = data.Name;
        if (DescriptionText) DescriptionText.text = data.Description;
        if (LevelText) LevelText.text = $"等级: {currentLevel} / {data.MaxLevel}";
        
        // 满级判定
        if (currentLevel >= data.MaxLevel)
        {
            if (CostText) CostText.text = "已满级";
            if (UpgradeButton) UpgradeButton.interactable = false;
        }
        else
        {
            if (CostText) CostText.text = $"消耗: {currentCost}";
            // 金币不足时按钮变灰
            if (UpgradeButton) UpgradeButton.interactable = currentGold >= currentCost;
        }

        // 绑定按钮事件
        if (UpgradeButton)
        {
            UpgradeButton.onClick.RemoveAllListeners();
            UpgradeButton.onClick.AddListener(() => onClickAction?.Invoke());
        }
    }
}