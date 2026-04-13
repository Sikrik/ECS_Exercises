using UnityEngine;
using TMPro;

/// <summary>
/// 局外天赋界面管理器
/// 职责：根据 Talent_Config.csv 动态生成天赋升级格子，并刷新金币显示。
/// </summary>
public class TalentUIManager : MonoBehaviour
{
    [Header("Global UI")]
    public TextMeshProUGUI TotalGoldText; // 关联显示金币的文本

    [Header("Dynamic Talent Setup")]
    public Transform TalentListRoot;      // 关联 ScrollView 的 Content 节点
    public GameObject TalentItemPrefab;   // 关联你制作的天赋格子预制体

    void OnEnable()
    {
        UpdateAllUI(); // 界面打开时刷新
    }

    // 刷新整个天赋界面的显示
    public void UpdateAllUI()
    {
        if (GameDataManager.Instance == null || ECSManager.Instance == null) 
        {
            Debug.LogError("[TalentUI] 错误：GameDataManager 或 ECSManager 实例为空！");
            return;
        }

        // 1. 更新全局金币显示
        int currentGold = GameDataManager.Instance.SaveData.TotalGold;
        if (TotalGoldText != null)
        {
            TotalGoldText.text = $"当前金币: {currentGold}";
        }

        // 2. 清理旧的天赋条目（防止重复生成）
        foreach (Transform child in TalentListRoot)
        {
            Destroy(child.gameObject);
        }

        // 3. 动态生成天赋条目
        var talentRecipes = ECSManager.Instance.Config.TalentRecipes;

        // 🔍 调试：检查配置表是否成功加载
        Debug.Log($"[TalentUI] 准备生成天赋，当前配置表中的条目数量: {talentRecipes.Count}");

        foreach (var talentKvp in talentRecipes)
        {
            TalentData talentData = talentKvp.Value;
            
            // 实例化预制体到 Content 下
            GameObject itemObj = Instantiate(TalentItemPrefab, TalentListRoot);
            
            // 获取格子脚本并初始化
            var itemUI = itemObj.GetComponent<TalentItemUI>();
            if (itemUI != null)
            {
                int currentLevel = GameDataManager.Instance.GetTalentLevel(talentData.Id);
                int currentCost = talentData.CostBase + (currentLevel * talentData.CostIncrement);

                // 初始化 UI 表现，并绑定点击升级的回调
                itemUI.Setup(
                    talentData, 
                    currentLevel, 
                    currentCost, 
                    currentGold, 
                    () => OnUpgradeClick(talentData.Id, currentCost)
                );
            }
            else
            {
                Debug.LogWarning($"[TalentUI] 预制体 {TalentItemPrefab.name} 上缺少 TalentItemUI 脚本！");
            }
        }
    }

    // 处理升级点击
    private void OnUpgradeClick(string talentId, int cost)
    {
        if (GameDataManager.Instance.TryUpgradeTalent(talentId, cost))
        {
            UpdateAllUI(); // 升级成功后立刻刷新界面
        }
    }
}