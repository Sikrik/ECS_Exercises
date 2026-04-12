using UnityEngine;

public abstract class Component { }

public enum BulletType { Normal, Slow, ChainLightning, AOE }
// 路径: Assets/Scripts/ECS/Core/ECSDefinitions.cs
public enum EnemyType { Normal, Fast, Tank, Charger, Ranged } // 新增 Ranged
public enum PlayerClass { Standard, Heavy, Agile }


// --- 新增的单帧事件与视觉意图组件 ---

// 变色意图组件：逻辑层告诉表现层“我想变色”
public class ColorTintComponent : Component 
{
    public Color TargetColor;
    public ColorTintComponent(Color color) => TargetColor = color;
}

// UI 血量刷新事件（单帧组件）
public class UIHealthUpdateEvent : Component { }

// 游戏结束事件（单帧组件）
public class GameOverEventComponent : Component { }

// 在 ECSDefinitions.cs 或 StateComponents.cs 中添加
public class OffScreenTag : Component { }
/// <summary>
/// 表现层特效清理标记（单帧意图）
/// 逻辑层加上这个标签，表现层看到后就会去销毁实体身上的 GameObject 特效
/// </summary>
public class PendingVFXDestroyTag : Component { }

/// <summary>
/// 爆炸意图组件（逻辑层）：
/// 这是一个瞬时意图组件，标记一个位置需要发生爆炸计算。
/// </summary>
public class ExplosionIntentComponent : Component
{
    public float Radius; // 爆炸半径
    public float Damage; // 爆炸伤害

    public ExplosionIntentComponent(float radius, float damage)
    {
        Radius = radius;
        Damage = damage;
    }
}
// 游戏胜利事件（单帧组件）
public class GameVictoryEventComponent : Component { }


/// <summary>
/// 经验值组件
/// </summary>
public class ExperienceComponent : Component
{
    public float CurrentXP;
    public float MaxXP;
    public int Level;
    
    public ExperienceComponent(float maxXP = 50f) 
    { 
        MaxXP = maxXP; 
        CurrentXP = 0; 
        Level = 1; 
    }
}

/// <summary>
/// 升级事件（单帧意图，用于呼出 UI）
/// </summary>
public class LevelUpEventComponent : Component { }

/// <summary>
/// 武器修饰器组件：记录所有通过升级获得的永久 Buff
/// </summary>
public class WeaponModifierComponent : Component
{
    public int ExtraProjectiles = 0;      // 额外弹道数 (分裂/多重射击)
    public bool HasSlow = false;          // 是否附加减速
    public bool HasChainLightning = false;// 是否附加闪电链
    public bool HasAOE = false;           // 是否附加范围爆炸
    public float FireRateMultiplier = 1f; // 射速倍率
}

/// <summary>
/// 定义升级池中的选项枚举
/// </summary>
public enum UpgradeType
{
    MultiShot,      // 额外发射子弹
    AddSlow,        // 赋予减速效果
    AddChain,       // 赋予闪电链效果
    AddAOE,         // 赋予爆炸效果
    FireRateUp      // 提升射速
}