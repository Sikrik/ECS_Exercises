// 路径: Assets/Scripts/ECS/TalentUIManager.cs
using UnityEngine;
using TMPro;
using System.Linq;

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

        var talentRecipes = ConfigManager.Instance.Config.TalentRecipes.Values.ToList();

        // 【优化】使用 UI 复用机制替代无脑的 Destroy()
        int dataCount = talentRecipes.Count;
        
        for (int i = 0; i < dataCount; i++)
        {
            TalentData talentData = talentRecipes[i];
            TalentItemUI itemUI;

            // 1. 获取或实例化 UI 预制体
            if (i < TalentListRoot.childCount)
            {
                itemUI = TalentListRoot.GetChild(i).GetComponent<TalentItemUI>();
            }
            else
            {
                GameObject itemObj = Instantiate(TalentItemPrefab, TalentListRoot);
                itemUI = itemObj.GetComponent<TalentItemUI>();
            }
            
            itemUI.gameObject.SetActive(true);

            // 2. 绑定数据
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

        // 3. 隐藏多余的旧 UI 对象
        for (int i = dataCount; i < TalentListRoot.childCount; i++)
        {
            TalentListRoot.GetChild(i).gameObject.SetActive(false);
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