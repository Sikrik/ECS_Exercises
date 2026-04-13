// 供你挂载在 TalentItemPrefab 上的辅助脚本
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TalentItemUI : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI CostText;
    public Button UpgradeButton;

    public void Setup(TalentData data, int currentLevel, int currentCost, int currentGold, System.Action onClickAction)
    {
        NameText.text = data.Name;
        DescriptionText.text = data.Description;
        LevelText.text = $"LV.{currentLevel} / {data.MaxLevel}";
        
        if (currentLevel >= data.MaxLevel)
        {
            CostText.text = "已满级";
            UpgradeButton.interactable = false;
        }
        else
        {
            CostText.text = $"消耗: {currentCost}";
            UpgradeButton.interactable = currentGold >= currentCost;
        }

        UpgradeButton.onClick.RemoveAllListeners();
        UpgradeButton.onClick.AddListener(() => onClickAction?.Invoke());
    }
}