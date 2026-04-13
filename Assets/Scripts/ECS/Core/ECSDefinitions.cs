// 路径: Assets/Scripts/ECS/Core/ECSDefinitions.cs
using System.Collections.Generic;
using UnityEngine;

public abstract class Component { }

// 更新类型枚举
public enum BulletType { Normal, Slow, ChainLightning, AOE }
public enum EnemyType { Normal, Fast, Tank, Charger, Ranged, Boss } // 新增 Boss
public enum PlayerClass { Standard, Heavy, Agile, Melee } // 新增 Melee

public class ColorTintComponent : Component 
{
    public Color TargetColor;
    public ColorTintComponent(Color color) => TargetColor = color;
}

public class UIHealthUpdateEvent : Component { }
public class GameOverEventComponent : Component { }
public class OffScreenTag : Component { }
public class PendingVFXDestroyTag : Component { }

public class ExplosionIntentComponent : Component
{
    public float Radius; 
    public float Damage; 

    public ExplosionIntentComponent(float radius, float damage)
    {
        Radius = radius;
        Damage = damage;
    }
}

public class GameVictoryEventComponent : Component { }

public class ExperienceComponent : Component
{
    public float CurrentXP;
    public float MaxXP;
    public int Level;
    public float ExpMultiplier; // 【新增】经验获取倍率
    
    public ExperienceComponent(float maxXP = 50f, float multiplier = 1.0f) 
    { 
        MaxXP = maxXP; 
        CurrentXP = 0; 
        Level = 1; 
        ExpMultiplier = multiplier; // 默认是 1.0 倍
    }
}

public class LevelUpEventComponent : Component { }

// ==========================================
// 【重构】武器修饰器组件：使用字典追踪每个 Buff 的等级
// ==========================================
/// <summary>
/// 武器修饰组件
/// 作用：挂载在玩家实体上，用于存储局外天赋带来的全局加成，以及局内吃升级卡牌带来的各种武器附魔等级。
/// BulletFactory 在生成子弹时会读取此组件来决定子弹的最终形态与伤害。
/// </summary>
public class WeaponModifierComponent 
{
    // ==========================================
    // 局外天赋加成 (Metagame Talents)
    // ==========================================
    
    /// <summary>
    /// 全局伤害倍率 (由局外天赋系统计算并注入)
    /// </summary>
    public float GlobalDamageMultiplier = 1.0f;


    // ==========================================
    // 局内升级加成 (In-Game Upgrades)
    // ==========================================
    
    /// <summary>
    /// 存储局内获取的各项武器修饰/附魔等级
    /// Key: 升级项的ID (如 "AttackUp", "AddSlow", "AddChain", "AddAOE", "AddStun" 等)
    /// Value: 当前该修饰项的等级
    /// </summary>
    public Dictionary<string, int> Modifiers;

    public WeaponModifierComponent()
    {
        Modifiers = new Dictionary<string, int>();
        GlobalDamageMultiplier = 1.0f;
    }

    /// <summary>
    /// 获取指定修饰项的等级
    /// </summary>
    /// <param name="modifierId">修饰项ID</param>
    /// <returns>当前等级，若未拥有该修饰项则返回0</returns>
    public int GetLevel(string modifierId)
    {
        if (Modifiers.TryGetValue(modifierId, out int level))
        {
            return level;
        }
        return 0; // 没有该项升级时默认返回0
    }

    /// <summary>
    /// 增加指定修饰项的等级（当玩家在局内三选一界面选择了某项升级时调用）
    /// </summary>
    /// <param name="modifierId">修饰项ID</param>
    /// <param name="levelToAdd">提升的等级数，默认为1</param>
    public void AddModifier(string modifierId, int levelToAdd = 1)
    {
        if (Modifiers.ContainsKey(modifierId))
        {
            Modifiers[modifierId] += levelToAdd;
        }
        else
        {
            Modifiers[modifierId] = levelToAdd;
        }
    }

    /// <summary>
    /// 重置所有局内修饰项（用于玩家死亡或重新开始游戏时清理局内进度）
    /// </summary>
    public void ClearInGameModifiers()
    {
        Modifiers.Clear();
    }
}