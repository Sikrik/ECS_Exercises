using UnityEngine;

public class CollisionComponent : Component 
{
    public float Radius; // 逻辑碰撞半径
    public CollisionComponent(float radius) => Radius = radius;
}

public class PhysicsColliderComponent : Component 
{
    public Collider2D Collider; // 引用 Unity 的 Collider2D
    public PhysicsColliderComponent(Collider2D collider) => Collider = collider;
}

// PhysicsComponents.cs
public class CollisionFilterComponent : Component 
{
    public int LayerMask; // 目标层级掩码
    public CollisionFilterComponent(int mask) => LayerMask = mask;
}

public class MassComponent : Component 
{
    public float Value;
    public MassComponent(float v) => Value = v;
}