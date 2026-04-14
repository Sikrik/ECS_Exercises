// 路径: Assets/Scripts/ECS/TalentUIManager.cs
using UnityEngine;
using TMPro;

public class TalentUIManager : MonoBehaviour
{
    [Header("Global UI")]
    public TextMeshProUGUI TotalGoldText; 

    [Header("Dynamic Talent Setup")]
    public Transform TalentListRoot;      
    public GameObject TalentItemPrefab;   

    void OnEnable()
    {
        UpdateAllUI(); 
    }

    public void UpdateAllUI()
    {
        // 👇 【核心修改1】：移除对 ECSManager 的强依赖
        if (GameDataManager.Instance == null) 
        {
            Debug.LogError("[TalentUI] 错误：GameDataManager 实例为空！请先从主菜单启动。");
            return;
        }

        int currentGold = GameDataManager.Instance.SaveData.TotalGold;
        if (TotalGoldText != null)
        {
            TotalGoldText.text = $"当前金币: {currentGold}";
        }

        foreach (Transform child in TalentListRoot)
        {
            Destroy(child.gameObject);
        }

        // 👇 【核心修改2】：从 GameDataManager 中读取天赋配表
        var talentRecipes = GameDataManager.Instance.Config.TalentRecipes;

        Debug.Log($"[TalentUI] 准备生成天赋，当前配置表中的条目数量: {talentRecipes.Count}");

        foreach (var talentKvp in talentRecipes)
        {
            TalentData talentData = talentKvp.Value;
            GameObject itemObj = Instantiate(TalentItemPrefab, TalentListRoot);
            var itemUI = itemObj.GetComponent<TalentItemUI>();
            if (itemUI != null)
            {
                int currentLevel = GameDataManager.Instance.GetTalentLevel(talentData.Id);
                int currentCost = talentData.CostBase + (currentLevel * talentData.CostIncrement);

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

    private void OnUpgradeClick(string talentId, int cost)
    {
        if (GameDataManager.Instance.TryUpgradeTalent(talentId, cost))
        {
            UpdateAllUI(); 
        }
    }
}