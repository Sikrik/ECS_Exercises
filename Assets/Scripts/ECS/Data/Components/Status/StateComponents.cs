// 路径: Assets/Scripts/ECS/Data/Components/Status/StateComponents.cs
using UnityEngine;
using System.Collections.Generic;

// --- 基础生命周期与标记 ---
public class LifetimeComponent : Component { public float Duration; }
public class PendingDestroyComponent : Component { }

// --- 意图组件 (用于 UI/预警) ---
public class AimLineIntentComponent : Component
{
    public Vector2 Direction;
    public float Length;
    public float Width;
    public AimLineIntentComponent(Vector2 dir, float len, float width) { Direction = dir; Length = len; Width = width; }
}

public class DashPreviewIntentComponent : Component
{
    public Vector2 Direction;
    public float Length;
    public float Width;
    public DashPreviewIntentComponent(Vector2 dir, float len, float width) { Direction = dir; Length = len; Width = width; }
}

// --- 👇 新增：战斗机制组件 ---

// 暴击标记 (用于子弹)
public class CriticalBulletComponent : Component { }

// DOT 负载 (用于子弹命中前携带数据)
public class BulletDOTPayloadComponent : Component 
{
    public float DPS;
    public float Duration;
    public string VfxName;
    public BulletDOTPayloadComponent(float dps, float duration, string vfx) 
    { DPS = dps; Duration = duration; VfxName = vfx; }
}

// DOT 效果 (用于实体正在受到的伤害)
public class DOTEffectComponent : Component 
{
    public class DOTState 
    {
        public float DamagePerSecond;
        public float Duration;
        public float TickTimer;
        public string VfxName;
    }
    
    // 用字典同时存储多种 DOT（Key 为 VfxName 如 "BurnVFX", "PoisonVFX"）
    public Dictionary<string, DOTState> ActiveDOTs = new Dictionary<string, DOTState>();
}

// 特效清理标记
public class PendingVFXDestroyTag : Component { }