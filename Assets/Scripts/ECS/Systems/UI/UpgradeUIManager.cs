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

    private readonly Dictionary<UpgradeType, string> _upgradeDescriptions = new Dictionary<UpgradeType, string>
    {
        { UpgradeType.MultiShot, "多重射击: 额外发射1发子弹" },
        { UpgradeType.AddSlow, "冰霜附魔: 子弹附带减速效果" },
        { UpgradeType.AddChain, "闪电附魔: 子弹附带闪电链" },
        { UpgradeType.AddAOE, "爆裂附魔: 子弹附带范围爆炸" },
        { UpgradeType.FireRateUp, "快速装填: 攻击速度提升20%" }
    };

    void Awake() => Instance = this;

    public void ShowUpgradePanel(Entity playerEntity)
    {
        _targetPlayer = playerEntity;
        UpgradePanel.SetActive(true);
        Time.timeScale = 0; // 暂停游戏

        // 随机抽取3个不重复的升级
        var allTypes = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));
        List<UpgradeType> pool = new List<UpgradeType>(allTypes);
        
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (pool.Count == 0) break;
            
            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            UpgradeType selectedUpgrade = pool[randomIndex];
            pool.RemoveAt(randomIndex);

            // 刷新按钮UI
            ChoiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = _upgradeDescriptions[selectedUpgrade];
            
            // 绑定点击事件
            ChoiceButtons[i].onClick.RemoveAllListeners();
            ChoiceButtons[i].onClick.AddListener(() => OnUpgradeChosen(selectedUpgrade));
        }
    }

    private void OnUpgradeChosen(UpgradeType upgradeType)
    {
        if (_targetPlayer != null && _targetPlayer.HasComponent<WeaponModifierComponent>())
        {
            var modifiers = _targetPlayer.GetComponent<WeaponModifierComponent>();

            // 应用升级效果
            switch (upgradeType)
            {
                case UpgradeType.MultiShot: modifiers.ExtraProjectiles++; break;
                case UpgradeType.AddSlow: modifiers.HasSlow = true; break;
                case UpgradeType.AddChain: modifiers.HasChainLightning = true; break;
                case UpgradeType.AddAOE: modifiers.HasAOE = true; break;
                case UpgradeType.FireRateUp: modifiers.FireRateMultiplier *= 0.8f; break; // 冷却时间缩短
            }
        }

        UpgradePanel.SetActive(false);
        Time.timeScale = 1; // 恢复游戏
    }
}