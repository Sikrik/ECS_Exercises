using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理碰撞事件组件，用于在实体间传递碰撞信息
/// </summary>
public class CollisionEventComponent : Component 
{
    /// <summary>
    /// 碰撞源实体（发起碰撞的一方）
    /// </summary>
    public Entity Source;

    /// <summary>
    /// 碰撞目标实体（被碰撞的一方）
    /// </summary>
    public Entity Target;

    /// <summary>
    /// 碰撞法线向量，用于计算反弹方向和击退方向
    /// </summary>
    public Vector2 Normal;
    
    /// <summary>
    /// 无参构造函数，专用于对象池回收和复用
    /// </summary>
    public CollisionEventComponent() { }

    /// <summary>
    /// 初始化碰撞事件组件
    /// </summary>
    /// <param name="src">碰撞源实体</param>
    /// <param name="target">碰撞目标实体</param>
    /// <param name="normal">碰撞法线向量</param>
    public CollisionEventComponent(Entity src, Entity target, Vector2 normal) 
    {
        Source = src; 
        Target = target; 
        Normal = normal;
    }
}

/// <summary>
/// 加分事件组件，用于触发分数增加逻辑
/// </summary>
public class ScoreEventComponent : Component
{
    /// <summary>
    /// 增加的分数值
    /// </summary>
    public int Amount;

    /// <summary>
    /// 初始化加分事件组件
    /// </summary>
    /// <param name="amount">增加的分数值</param>
    public ScoreEventComponent(int amount) => Amount = amount;
}

/// <summary>
/// 伤害承受事件组件，用于在实体受到伤害时传递伤害信息
/// </summary>
public class DamageTakenEventComponent : Component
{
    /// <summary>
    /// 受到的伤害数值
    /// </summary>
    public float DamageAmount;
    
    /// <summary>
    /// 无参构造函数，专用于对象池回收和复用
    /// </summary>
    public DamageTakenEventComponent() { }
    
    /// <summary>
    /// 初始化伤害承受事件组件
    /// </summary>
    /// <param name="amt">伤害数值</param>
    public DamageTakenEventComponent(float amt) => DamageAmount = amt;
}
