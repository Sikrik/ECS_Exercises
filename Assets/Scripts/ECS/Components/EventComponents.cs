/// <summary>
/// 子弹命中事件组件，用于在ECS系统中传递命中信息
/// 这是战斗系统的核心事件机制，连接子弹系统和伤害处理系统
/// 当子弹与目标发生碰撞时创建此事件，触发后续的伤害计算、特效播放、状态应用等逻辑
/// </summary>
public class BulletHitEventComponent : Component {
    /// <summary>被命中的目标实体，伤害和效果将应用到此实体上</summary>
    public Entity Target;
    public BulletHitEventComponent(Entity target) => Target = target;
}

/// <summary>
/// 升级事件组件，用于触发玩家或敌人的升级逻辑
/// 当前为空标记组件，预留用于未来的等级系统或技能升级功能
/// 可以扩展为包含升级类型、新属性值等数据的完整事件系统
/// </summary>
public class LevelUpEventComponent : Component { }