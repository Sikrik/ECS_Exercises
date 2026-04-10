/// <summary>
/// 无敌状态组件：使实体在指定时间内免疫所有伤害。
/// 适用场景：角色复活保护、技能无敌帧、道具触发短暂无敌等。
/// </summary>
public class InvincibleComponent : Component 
{
    /// <summary>
    /// 无敌持续时间（秒）
    /// 倒计时结束后，系统将自动移除此组件并恢复正常受击状态。
    /// </summary>
    public float Duration;
}

/// <summary>
/// 减速效果组件：降低实体的移动速度，常用于控制类技能或地形效果。
/// 设计说明：SlowRatio 为减速比例（0-1之间），值越大减速效果越强。
/// 典型应用：冰冻技能、黏液陷阱、范围减速光环等。
/// </summary>
public class SlowEffectComponent : Component 
{
    /// <summary>
    /// 减速比例（0-1）
    /// 0 = 无减速，1 = 完全定身，0.5 = 速度减半
    /// </summary>
    public float SlowRatio;
    
    /// <summary>
    /// 减速持续时间（秒）
    /// 支持叠加逻辑：刷新持续时间或取最大值，具体由系统实现决定。
    /// </summary>
    public float Duration;
    
    /// <summary>
    /// 构造函数：快速创建减速效果实例
    /// </summary>
    /// <param name="r">减速比例（0-1）</param>
    /// <param name="d">持续时间（秒）</param>
    public SlowEffectComponent(float r, float d) { SlowRatio = r; Duration = d; }
}

/// 击退效果组件
/// 重构：增加了 HitRecoveryAfterwards 字段，用于在滑行结束后自动衔接硬直
/// </summary>
public class KnockbackComponent : Component 
{
    public float Timer;
    /// <summary>
    /// 击退方向 X 分量（标准化向量）
    /// </summary>
    public float DirX;
    
    /// <summary>
    /// 击退方向 Y 分量（标准化向量）
    /// </summary>
    public float DirY;
    
    /// <summary>
    /// 击退速度（单位：像素/秒 或 米/秒）
    /// 建议配合物理碰撞检测使用，避免穿墙。
    /// </summary>
    public float Speed;
    // 新增：击退滑行结束后，是否需要进入硬直？要多久？
    public float HitRecoveryAfterwards; 
}

/// <summary>
/// 受击硬直组件：使实体进入短暂的受击僵直状态，无法执行其他动作。
/// 设计目的：增强战斗打击感，防止连续攻击导致的逻辑冲突。
/// 典型用途：角色被击中后的短暂停顿、霸体状态的打断判定。
/// </summary>
public class HitRecoveryComponent : Component 
{ 
    /// <summary>
    /// 硬直剩余时间（秒）
    /// 期间实体通常禁止移动、攻击等操作，具体限制由输入系统控制。
    /// </summary>
    public float Timer; 
}