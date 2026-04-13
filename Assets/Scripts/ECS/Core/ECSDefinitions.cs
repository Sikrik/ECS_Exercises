// 路径: Assets/Scripts/ECS/Core/ECSDefinitions.cs
using System.Collections.Generic;
using UnityEngine;

public abstract class Component { }

public enum BulletType { Normal, Slow, ChainLightning, AOE }
public enum EnemyType { Normal, Fast, Tank, Charger, Ranged } 
public enum PlayerClass { Standard, Heavy, Agile }

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
public class WeaponModifierComponent : Component
{
    // Key: Upgrade_Config 中的 ID, Value: 当前已升级的等级
    public Dictionary<string, int> UpgradeLevels = new Dictionary<string, int>();

    // 辅助获取方法：如果没学过该技能则返回 0
    public int GetLevel(string upgradeId)
    {
        return UpgradeLevels.TryGetValue(upgradeId, out int level) ? level : 0;
    }
}