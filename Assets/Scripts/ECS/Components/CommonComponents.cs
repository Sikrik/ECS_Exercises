using UnityEngine;

/// <summary>
/// 视图组件：仅存储视觉表现引用
/// </summary>
public class ViewComponent : Component
{
    public GameObject GameObject;
    public GameObject Prefab; // 新增：记录这个物体是从哪个预制体生成的
    
    public ViewComponent(GameObject go, GameObject prefab) 
    { 
        GameObject = go; 
        Prefab = prefab; 
    }
}

/// <summary>
/// 基础颜色组件：存储实体最原始、自然的状态颜色
/// </summary>
public class BaseColorComponent : Component 
{
    public Color Value;
    public BaseColorComponent(Color c) => Value = c;
}

/// <summary>
/// 基础位置组件：仅存储当前坐标
/// </summary>
public class PositionComponent : Component
{
    public float X, Y, Z;
    public PositionComponent(float x, float y, float z) { X = x; Y = y; Z = z; }
}

/// <summary>
/// 轨迹追踪组件：仅给高速物体（如子弹）使用，记录上一帧位置防止穿透
/// </summary>
public class TraceComponent : Component
{
    public float PreviousX, PreviousY;
    public TraceComponent(float x, float y) { PreviousX = x; PreviousY = y; }
}

/// <summary>
/// 速度组件：存储当前运动矢量
/// </summary>
// Assets/Scripts/ECS/Components/ActorComponents.cs
public class VelocityComponent : Component
{
    public float VX; // 必须是大写且 public
    public float VY;
    public VelocityComponent(float vx, float vy) { VX = vx; VY = vy; }
}

/// <summary>
/// 血量组件：纯粹的数据存储，不再包含无敌逻辑
/// </summary>
public class HealthComponent : Component
{
    public float CurrentHealth;
    public float MaxHealth;

    public HealthComponent(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }
}

/// <summary>
/// 碰撞定义组件
/// </summary>
public class CollisionComponent : Component
{
    public float Radius;
    public CollisionComponent(float radius) => Radius = radius;
}

// 1. 物理碰撞体组件：持有对 Unity Collider2D 的引用
public class PhysicsColliderComponent : Component
{
    public Collider2D Collider;
    public PhysicsColliderComponent(Collider2D collider) => Collider = collider;
}
/// <summary>
/// 弹性组件：标记该实体碰撞后会触发弹开（击退）
/// </summary>
public class BouncyTag : Component { }
// 2. 烘焙标记：告诉系统这个实体刚出生，需要把 GameObject 里的 Collider 拿过来
public class NeedsBakingTag : Component { }
