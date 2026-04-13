using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器属性组件：纯粹描述实体的武装状态，不包含任何逻辑
/// </summary>
public class WeaponComponent : Component
{
    public BulletType CurrentBulletType; // 子弹类型
    public float FireRate;               // 射击间隔（秒）
    public float CurrentCooldown;        // 当前冷却倒计时

    public WeaponComponent(BulletType bulletType, float fireRate)
    {
        CurrentBulletType = bulletType;
        FireRate = fireRate;
        CurrentCooldown = 0f;
    }
}

/// <summary>
/// 开火意图组件：这是一个【单帧状态】，表示实体在这一帧想要朝哪个方向开火
/// </summary>
public class FireIntentComponent : Component
{
    public Vector2 AimDirection;

    public FireIntentComponent(Vector2 aimDirection)
    {
        AimDirection = aimDirection;
    }
}

/// <summary>
/// 武器修饰组件
/// 作用：挂载在玩家实体上，用于存储局外天赋带来的全局加成，以及局内吃升级卡牌带来的各种武器附魔等级。
/// BulletFactory 在生成子弹时会读取此组件来决定子弹的最终形态与伤害。
/// </summary>
public class WeaponModifierComponent : Component
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