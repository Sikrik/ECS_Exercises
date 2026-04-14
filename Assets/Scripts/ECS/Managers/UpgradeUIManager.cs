// 路径: Assets/Scripts/ECS/Systems/UI/UpgradeUIManager.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIManager : MonoBehaviour
{
    public static UpgradeUIManager Instance;

    public GameObject UpgradePanel;
    public Button[] ChoiceButtons; 
    
    private Entity _targetPlayer;

    void Awake() => Instance = this;

    public void ShowUpgradePanel(Entity playerEntity)
    {
        _targetPlayer = playerEntity;
        var modifiers = _targetPlayer.GetComponent<WeaponModifierComponent>();
        var config = BattleManager.Instance.Config;

        // 👇【核心修改】：判定玩家是近战还是远程，选择对应的升级配方表
        bool isMelee = _targetPlayer.HasComponent<MeleeCombatComponent>();
        var activeRecipes = isMelee ? config.MeleeUpgradeRecipes : config.RangedUpgradeRecipes;

        // 1. 动态生成有效卡池
        List<string> validPool = new List<string>();
        foreach (var kvp in activeRecipes)
        {
            string upgradeId = kvp.Key;
            int maxLevel = kvp.Value.MaxLevel;
            string prerequisite = kvp.Value.Prerequisite;
            int currentLevel = modifiers.GetLevel(upgradeId);

            // 【过滤条件 1】如果有前置条件且玩家还没学会前置（等级为0），则跳过
            if (!string.IsNullOrEmpty(prerequisite) && modifiers.GetLevel(prerequisite) == 0)
            {
                continue;
            }

            // 【过滤条件 2】如果该项还没升满，则加入随机池
            if (currentLevel < maxLevel)
            {
                validPool.Add(upgradeId);
            }
        }

        // 2. 防御判定：如果没有任何可升级项，直接关闭面板恢复游戏
        if (validPool.Count == 0)
        {
            Time.timeScale = 1;
            return;
        }

        UpgradePanel.SetActive(true);
        Time.timeScale = 0; // 暂停游戏

        // 3. 随机抽取选项并绑定 UI
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (validPool.Count == 0) 
            {
                ChoiceButtons[i].gameObject.SetActive(false); 
                continue;
            }
            
            ChoiceButtons[i].gameObject.SetActive(true);
            
            int randomIndex = UnityEngine.Random.Range(0, validPool.Count);
            string selectedId = validPool[randomIndex];
            validPool.RemoveAt(randomIndex); // 确保本次抽取的选项互不重复

            // 👇【核心修改】：从 activeRecipes 中读取文本
            ChoiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = activeRecipes[selectedId].Description;
            
            // 绑定点击事件
            ChoiceButtons[i].onClick.RemoveAllListeners();
            ChoiceButtons[i].onClick.AddListener(() => OnUpgradeChosen(selectedId));
        }
    }

    private void OnUpgradeChosen(string upgradeId)
    {
        if (_targetPlayer != null && _targetPlayer.HasComponent<WeaponModifierComponent>())
        {
            var modifiers = _targetPlayer.GetComponent<WeaponModifierComponent>();
            modifiers.AddModifier(upgradeId, 1);
        }

        UpgradePanel.SetActive(false);
        Time.timeScale = 1; // 恢复游戏
    }
}