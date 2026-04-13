using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TalentUIManager : MonoBehaviour
{
    [Header("Global UI")]
    public TextMeshProUGUI TotalGoldText;

    [Header("Talent: Health Up")]
    public TextMeshProUGUI HealthLevelText;
    public TextMeshProUGUI HealthCostText;
    public Button HealthUpgradeBtn;
    private int _healthBaseCost = 100; // 基础升级消耗

    [Header("Talent: Exp Up")]
    public TextMeshProUGUI ExpLevelText;
    public TextMeshProUGUI ExpCostText;
    public Button ExpUpgradeBtn;
    private int _expBaseCost = 150;

    void OnEnable()
    {
        UpdateAllUI();
    }

    // 刷新整个天赋界面的显示
    private void UpdateAllUI()
    {
        if (GameDataManager.Instance == null) return;

        int currentGold = GameDataManager.Instance.SaveData.TotalGold;
        TotalGoldText.text = $"当前金币: {currentGold}";

        // --- 刷新血量天赋 ---
        int healthLevel = GameDataManager.Instance.GetTalentLevel("HealthUp");
        int currentHealthCost = _healthBaseCost + (healthLevel * 50); // 每升一级变贵 50
        
        HealthLevelText.text = $"生命值LV.{healthLevel}";
        HealthCostText.text = $"消耗: {currentHealthCost}";
        HealthUpgradeBtn.interactable = currentGold >= currentHealthCost; // 金币不够则按钮置灰

        // --- 刷新经验天赋 ---
        int expLevel = GameDataManager.Instance.GetTalentLevel("ExpUp");
        int currentExpCost = _expBaseCost + (expLevel * 80);
        
        ExpLevelText.text = $"经验值LV.{expLevel}";
        ExpCostText.text = $"消耗: {currentExpCost}";
        ExpUpgradeBtn.interactable = currentGold >= currentExpCost;
    }

    // ==========================================
    // 按钮点击事件绑定
    // ==========================================

    public void OnUpgradeHealthClick()
    {
        int level = GameDataManager.Instance.GetTalentLevel("HealthUp");
        int cost = _healthBaseCost + (level * 50);
        
        if (GameDataManager.Instance.TryUpgradeTalent("HealthUp", cost))
        {
            UpdateAllUI(); // 升级成功，刷新界面
        }
    }

    public void OnUpgradeExpClick()
    {
        int level = GameDataManager.Instance.GetTalentLevel("ExpUp");
        int cost = _expBaseCost + (level * 80);
        
        if (GameDataManager.Instance.TryUpgradeTalent("ExpUp", cost))
        {
            UpdateAllUI();
        }
    }
}