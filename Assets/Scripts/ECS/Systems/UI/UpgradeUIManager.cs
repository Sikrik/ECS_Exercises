// 路径: Assets/Scripts/ECS/Systems/UI/UpgradeUIManager.cs
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIManager : MonoBehaviour
{
    public static UpgradeUIManager Instance;

    public GameObject UpgradePanel;
    public Button[] ChoiceButtons; // 拖入3个按钮
    
    private Entity _targetPlayer;

    void Awake() => Instance = this;

    public void ShowUpgradePanel(Entity playerEntity)
    {
        _targetPlayer = playerEntity;
        var modifiers = _targetPlayer.GetComponent<WeaponModifierComponent>();
        var config = ECSManager.Instance.Config;

        // 1. 动态生成有效卡池（过滤掉已满级的选项）
        List<string> validPool = new List<string>();
        foreach (var kvp in config.UpgradeRecipes)
        {
            string upgradeId = kvp.Key;
            int maxLevel = kvp.Value.MaxLevel;
            int currentLevel = modifiers.GetLevel(upgradeId);

            // 只有当前等级小于最大等级时，才加入随机卡池
            if (currentLevel < maxLevel)
            {
                validPool.Add(upgradeId);
            }
        }

        // 2. 防御判定：如果没有任何可升级项（全部满级），直接关闭面板恢复游戏
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
                ChoiceButtons[i].gameObject.SetActive(false); // 选项不足3个时隐藏多余按钮
                continue;
            }
            
            ChoiceButtons[i].gameObject.SetActive(true);
            
            int randomIndex = UnityEngine.Random.Range(0, validPool.Count);
            string selectedId = validPool[randomIndex];
            validPool.RemoveAt(randomIndex); // 确保本次抽取的3个选项互不重复

            // 读取 CSV 中的描述文本
            ChoiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = config.UpgradeRecipes[selectedId].Description;
            
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
            
            // 提升该技能的等级
            if (!modifiers.UpgradeLevels.ContainsKey(upgradeId))
            {
                modifiers.UpgradeLevels[upgradeId] = 0;
            }
            modifiers.UpgradeLevels[upgradeId]++;
        }

        UpgradePanel.SetActive(false);
        Time.timeScale = 1; // 恢复游戏
    }
}