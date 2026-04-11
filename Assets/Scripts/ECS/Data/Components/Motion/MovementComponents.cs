/// <summary>
/// 速度组件，存储实体在X和Y轴上的瞬时速度分量
/// </summary>
public class VelocityComponent : Component 
{
    /// <summary>
    /// X轴方向的速度分量，单位：米/秒
    /// </summary>
    public float VX;

    /// <summary>
    /// Y轴方向的速度分量，单位：米/秒
    /// </summary>
    public float VY;

    /// <summary>
    /// 初始化速度组件
    /// </summary>
    /// <param name="vx">X轴速度分量</param>
    /// <param name="vy">Y轴速度分量</param>
    public VelocityComponent(float vx, float vy) { VX = vx; VY = vy; }
}

/// <summary>
/// 速度值组件，存储实体的基础速度和当前实时速度
/// </summary>
public class SpeedComponent : Component 
{
    /// <summary>
    /// 基础速度值，来源于配置文件，不受临时效果影响
    /// </summary>
    public float BaseSpeed;

    /// <summary>
    /// 当前实时速度，经过减速、加速等效果计算后的实际速度值
    /// </summary>
    public float CurrentSpeed;

    /// <summary>
    /// 初始化速度值组件，基础速度和当前速度均设为指定值
    /// </summary>
    /// <param name="speed">初始速度值</param>
    public SpeedComponent(float speed) 
    { 
        BaseSpeed = speed; 
        CurrentSpeed = speed; 
    }
}

/// <summary>
/// 轨迹追踪组件，记录实体上一帧的位置信息，用于高速物体防穿透检测
/// </summary>
public class TraceComponent : Component 
{
    /// <summary>
    /// 上一帧的X坐标位置
    /// </summary>
    public float PreviousX;

    /// <summary>
    /// 上一帧的Y坐标位置
    /// </summary>
    public float PreviousY;

    /// <summary>
    /// 初始化轨迹追踪组件
    /// </summary>
    /// <param name="x">初始X坐标</param>
    /// <param name="y">初始Y坐标</param>
    public TraceComponent(float x, float y) { PreviousX = x; PreviousY = y; }
}

/// <summary>
/// 位置组件，存储实体在三维空间中的坐标位置
/// </summary>
public class PositionComponent : Component 
{
    /// <summary>
    /// X轴坐标位置，单位：米
    /// </summary>
    public float X;

    /// <summary>
    /// Y轴坐标位置，单位：米
    /// </summary>
    public float Y;

    /// <summary>
    /// Z轴坐标位置，单位：米（2D游戏中通常用于渲染层级）
    /// </summary>
    public float Z;

    /// <summary>
    /// 初始化位置组件
    /// </summary>
    /// <param name="x">X轴坐标</param>
    /// <param name="y">Y轴坐标</param>
    /// <param name="z">Z轴坐标</param>
    public PositionComponent(float x, float y, float z) { X = x; Y = y; Z = z; }
}

/// <summary>
/// 冲刺能力组件：存储实体的冲刺配置和CD状态（玩家和敌人均可挂载）
/// </summary>
public class DashAbilityComponent : Component
{
    public float DashSpeed;     // 冲刺时的固定速度
    public float Duration;      // 冲刺持续时间
    public float Cooldown;      // 技能冷却时间
    public float CurrentCD;     // 当前剩余冷却时间

    public DashAbilityComponent(float speed, float duration, float cd)
    {
        DashSpeed = speed; Duration = duration; Cooldown = cd; CurrentCD = 0;
    }
}

/// <summary>
/// 冲刺状态组件：实体当前正在冲刺中
/// </summary>
public class DashStateComponent : Component
{
    public float Timer; // 剩余冲刺时间
    public float DirX;  // 冲刺方向 X
    public float DirY;  // 冲刺方向 Y
}

/// <summary>
/// 冲刺意图事件：单帧组件，代表当前帧按下了冲刺键或AI决定冲刺
/// </summary>
public class DashInputComponent : Component { }

