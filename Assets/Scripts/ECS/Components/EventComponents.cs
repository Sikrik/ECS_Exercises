
/// <summary>
/// 子弹命中事件组件
/// 当子弹碰撞系统检测到接触时，会给子弹挂载此组件。
/// BulletEffectSystem 会根据此组件的信息分发伤害。
/// </summary>
public class BulletHitEventComponent : Component 
{
    public Entity Target; // 命中的目标实体

    public BulletHitEventComponent(Entity target)
    {
        Target = target;
    }
}

/// <summary>
/// 经验值收集事件 (扩展用)
/// </summary>
public class ExpCollectEventComponent : Component
{
    public int Amount;
    public ExpCollectEventComponent(int amount) => Amount = amount;
}