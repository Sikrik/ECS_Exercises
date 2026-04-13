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

