using UnityEngine;

// ===================== 玩家相关 (Player) =====================

/// <summary>
/// 玩家标记组件：仅用于标记实体是玩家
/// </summary>
public class PlayerTag : Component { }

/// <summary>
/// 无敌状态组件：仅在玩家/敌人处于无敌帧时存在
/// 挂载此组件即表示当前无敌，不再需要在其他地方判断 Timer > 0
/// </summary>
public class InvincibleComponent : Component 
{
    public float RemainingTime; 
    public Color OriginalColor = Color.clear; // 新增：保存初始颜色，默认设为透明以标记未初始化
}

// ===================== 子弹相关 (Bullet) =====================

public enum BulletType
{
    Normal,         // 普通子弹
    Slow,           // 减速子弹
    ChainLightning, // 连锁闪电
    AOE             // 范围伤害
}

/// <summary>
/// 子弹标记组件：仅用于标记实体是子弹
/// </summary>
public class BulletTag : Component { }

/// <summary>
/// 子弹属性组件：存储子弹的静态战斗数据
/// </summary>
public class BulletStatsComponent : Component 
{
    public BulletType Type;
    public float Damage;
}

/// <summary>
/// 通用生命周期组件：用于任何需要限时销毁的实体（子弹、特效、掉落物等）
/// 这样 BulletLifeTimeSystem 就可以变成通用的 LifetimeSystem
/// </summary>
public class LifetimeComponent : Component 
{
    public float RemainingTime;
}


// ===================== 敌人相关 (Enemy) =====================

public enum EnemyType
{
    Normal, // 普通敌人
    Fast,   // 快速敌人
    Tank    // 坦克敌人
}

/// <summary>
/// 敌人标记组件：仅用于标记实体是敌人
/// </summary>
public class EnemyTag : Component { }

/// <summary>
/// 敌人基础属性：存储不会轻易改变的数值
/// </summary>
public class EnemyStatsComponent : Component
{
    public EnemyType Type;
    public float MoveSpeed;
    public int Damage;
    public float AttackCooldown;
}

/// <summary>
/// AI 运行时状态：存储动态计时器
/// </summary>
public class AIStateComponent : Component
{
    public float CurrentCooldown;
}

/// <summary>
/// 弹性组件：标记该实体碰撞后会触发弹开（击退）
/// </summary>
public class BouncyTag : Component { }

/// <summary>
/// 击退状态组件：仅在实体被弹开时存在
/// </summary>
public class KnockbackComponent : Component
{
    public float DirX;
    public float DirY;
    public float Speed;
    public float Timer;
}

/// <summary>
/// 受击硬直组件：仅在实体无法行动时存在
/// </summary>
public class HitRecoveryComponent : Component
{
    public float Timer;
}