using UnityEngine;
using TMPro;

public class TalentUIManager : MonoBehaviour
{
    [Header("Global UI")]
    public TextMeshProUGUI TotalGoldText;

    [Header("Dynamic Talent Setup")]
    public Transform TalentListRoot;      // 列表的父节点 (通常是 ScrollView 的 Content)
    public GameObject TalentItemPrefab;   // 单个天赋条目的预制体

    void OnEnable()
    {
        UpdateAllUI();
    }

    // 刷新整个天赋界面的显示
    public void UpdateAllUI()
    {
        if (GameDataManager.Instance == null || ECSManager.Instance == null) return;

        // 1. 更新全局金币
        int currentGold = GameDataManager.Instance.SaveData.TotalGold;
        if (TotalGoldText != null)
        {
            TotalGoldText.text = $"当前金币: {currentGold}";
        }

        // 2. 清理旧的天赋条目
        foreach (Transform child in TalentListRoot)
        {
            Destroy(child.gameObject);
        }

        // 3. 动态生成天赋条目
        var talentRecipes = ECSManager.Instance.Config.TalentRecipes;

        foreach (var talentKvp in talentRecipes)
        {
            TalentData talentData = talentKvp.Value;
            int currentLevel = GameDataManager.Instance.GetTalentLevel(talentData.Id);
            
            // 计算当前升级所需的金币消耗
            int currentCost = talentData.CostBase + (currentLevel * talentData.CostIncrement);

            // 实例化预制体
            GameObject itemObj = Instantiate(TalentItemPrefab, TalentListRoot);
            
            // 假设预制体上挂载了处理单一 UI 元素的脚本：TalentItemUI
            // 此处通过 SendMessage 传递或者如果你实现了脚本可以直接 GetComponent调用
            var itemUI = itemObj.GetComponent<TalentItemUI>();
            if (itemUI != null)
            {
                // 初始化该条目的 UI，并绑定点击回调闭包
                itemUI.Setup(
                    talentData, 
                    currentLevel, 
                    currentCost, 
                    currentGold, 
                    () => OnUpgradeClick(talentData.Id, currentCost)
                );
            }
        }
    }

    // 处理任何天赋的升级点击逻辑
    private void OnUpgradeClick(string talentId, int cost)
    {
        if (GameDataManager.Instance.TryUpgradeTalent(talentId, cost))
        {
            // 升级成功后立刻刷新整个列表UI
            UpdateAllUI(); 
        }
    }
}