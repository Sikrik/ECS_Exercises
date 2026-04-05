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
public class VelocityComponent : Component
{
    public float SpeedX, SpeedY, SpeedZ;
    public VelocityComponent(float x, float y, float z) { SpeedX = x; SpeedY = y; SpeedZ = z; }

    // 快捷访问接口
    public float X { get { return SpeedX; } set { SpeedX = value; } }
    public float Y { get { return SpeedY; } set { SpeedY = value; } }
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