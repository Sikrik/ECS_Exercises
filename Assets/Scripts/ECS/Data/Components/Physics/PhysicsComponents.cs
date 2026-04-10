using UnityEngine;

/// <summary>
/// 碰撞体组件，存储实体的逻辑碰撞半径信息
/// </summary>
public class CollisionComponent : Component 
{
    /// <summary>
    /// 逻辑碰撞半径，单位：米
    /// </summary>
    public float Radius;

    /// <summary>
    /// 初始化碰撞体组件
    /// </summary>
    /// <param name="radius">碰撞半径</param>
    public CollisionComponent(float radius) => Radius = radius;
}

/// <summary>
/// 物理碰撞器组件，引用Unity原生的Collider2D组件以实现精确碰撞检测
/// </summary>
public class PhysicsColliderComponent : Component 
{
    /// <summary>
    /// Unity原生2D碰撞器组件引用
    /// </summary>
    public Collider2D Collider;

    /// <summary>
    /// 初始化物理碰撞器组件
    /// </summary>
    /// <param name="collider">Unity的Collider2D组件实例</param>
    public PhysicsColliderComponent(Collider2D collider) => Collider = collider;
}

/// <summary>
/// 碰撞过滤组件，用于定义实体可以与之交互的层级掩码
/// </summary>
public class CollisionFilterComponent : Component 
{
    /// <summary>
    /// 目标层级掩码，用于碰撞检测时的层级过滤
    /// </summary>
    public int LayerMask;

    /// <summary>
    /// 初始化碰撞过滤组件
    /// </summary>
    /// <param name="mask">层级掩码值</param>
    public CollisionFilterComponent(int mask) => LayerMask = mask;
}

/// <summary>
/// 质量组件，存储实体的物理质量信息，用于动量计算和碰撞反弹
/// </summary>
public class MassComponent : Component 
{
    /// <summary>
    /// 实体质量值，单位：千克
    /// </summary>
    public float Value;

    /// <summary>
    /// 初始化质量组件
    /// </summary>
    /// <param name="v">质量值</param>
    public MassComponent(float v) => Value = v;
}