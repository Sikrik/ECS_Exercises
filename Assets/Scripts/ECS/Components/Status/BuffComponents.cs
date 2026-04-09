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

/// <summary>
/// 击退效果组件：使实体沿指定方向产生强制位移，模拟物理冲击效果。
/// 工作原理：系统在每帧根据 DirX/DirY 方向和 Speed 速度更新实体位置，
/// 同时递减 Timer 直到归零后移除组件。
/// 应用场景：爆炸冲击波、近战重击、风力推动等。
/// </summary>
public class KnockbackComponent : Component 
{
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
    
    /// <summary>
    /// 击退剩余时间（秒）
    /// 每帧递减 Time.deltaTime，归零时移除此组件。
    /// </summary>
    public float Timer;
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