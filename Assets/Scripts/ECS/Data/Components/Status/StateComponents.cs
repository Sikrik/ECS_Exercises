/// <summary>
/// 生命周期组件：管理实体的存活时长，用于自动清理过期实体。
/// 适用场景：子弹飞行、特效播放、临时增益效果等具有明确生存时间的游戏对象。
/// </summary>
public class LifetimeComponent : Component 
{
    /// <summary>
    /// 生存持续时间（秒）
    /// 当实体存在时间超过此值时，将被标记为待销毁。
    /// </summary>
    public float Duration;
}

/// <summary>
/// 待销毁标记组件：标识实体需要在当前帧结束时被统一回收。
/// 设计目的：避免在系统处理过程中直接销毁实体导致的迭代器失效问题，
/// 采用延迟销毁策略确保数据一致性和系统稳定性。
/// 使用流程：系统标记此组件 → 帧末销毁系统统一清理 → 移除实体。
/// </summary>
public class PendingDestroyComponent : Component { }

