// 路径: Assets/Scripts/ECS/Core/ECSDefinitions.cs
using System.Collections.Generic;
using UnityEngine;

public abstract class Component { }

// 更新类型枚举
public enum BulletType { Normal, Slow, ChainLightning, AOE }
public enum EnemyType { Normal, Fast, Tank, Charger, Ranged, Boss }
public enum PlayerClass { Standard, Melee, Agile } // 将 Heavy 改为 Melee

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
public class MeleeCombatComponent : Component 
{
    public float AttackRadius = 3f;      // 基础攻击半径
    public float AttackAngle = 90f;      // 基础攻击角度 (四分之一圆)
    public float LifeStealRatio = 0.05f; // 基础 5% 吸血
    public float HealthRegen = 2f;       // 每秒回血量
    public float Defense = 0f;           // 固定防御力
    public float ThornDamage = 0f;       // 反伤数值
    public bool HasDoubleHit = false;    // 是否解锁二重连击
}

// 【新增】挥砍意图（瞬时事件）
public class MeleeSwingIntentComponent : Component 
{
    public float RadiusMultiplier = 1.0f; 
    public float AngleOverride = -1f; // -1 表示使用组件默认角度
}
// ==========================================
// 用于控制二重连击延迟触发的计时器组件
// ==========================================
public class MeleeDoubleHitPendingComponent : Component
{
    public float Timer;

    public MeleeDoubleHitPendingComponent(float delay)
    {
        Timer = delay;
    }
}
