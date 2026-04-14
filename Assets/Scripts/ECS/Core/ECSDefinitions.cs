// 路径: Assets/Scripts/ECS/Core/ECSDefinitions.cs
using System.Collections.Generic;
using UnityEngine;

public abstract class Component { }

// 更新类型枚举
public enum BulletType { Normal, Slow, ChainLightning, AOE }
public enum EnemyType { Normal, Fast, Tank, Charger, Ranged, Boss }
public enum PlayerClass { Standard, Melee, Agile } 

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
    public float ExpMultiplier; 
    
    public ExperienceComponent(float maxXP = 50f, float multiplier = 1.0f) 
    { 
        MaxXP = maxXP; 
        CurrentXP = 0; 
        Level = 1; 
        ExpMultiplier = multiplier; 
    }
}

public class LevelUpEventComponent : Component { }
public class MeleeCombatComponent : Component 
{
    public float AttackRadius = 3f;      
    public float AttackAngle = 90f;      
    public float LifeStealRatio = 0.05f; 
    public float HealthRegen = 2f;       
    public float Defense = 0f;           
    public float ThornDamage = 0f;       
    public bool HasDoubleHit = false;    
}

public class MeleeSwingIntentComponent : Component 
{
    public float RadiusMultiplier = 1.0f; 
    public float AngleOverride = -1f; 
}

public class MeleeDoubleHitPendingComponent : Component
{
    public float Timer;

    public MeleeDoubleHitPendingComponent(float delay)
    {
        Timer = delay;
    }
}