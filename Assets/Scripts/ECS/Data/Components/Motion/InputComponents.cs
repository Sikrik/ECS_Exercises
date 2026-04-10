/// <summary>
/// 移动输入组件，存储玩家的移动意图方向，不直接代表最终速度
/// </summary>
public class MoveInputComponent : Component 
{
    /// <summary>
    /// X轴方向的移动输入值，范围：-1到1
    /// </summary>
    public float X;

    /// <summary>
    /// Y轴方向的移动输入值，范围：-1到1
    /// </summary>
    public float Y;

    /// <summary>
    /// 初始化移动输入组件
    /// </summary>
    /// <param name="x">X轴移动输入值</param>
    /// <param name="y">Y轴移动输入值</param>
    public MoveInputComponent(float x, float y) { X = x; Y = y; }
}

/// <summary>
/// 射击输入组件，存储玩家的射击意图和鼠标目标位置的世界坐标
/// </summary>
public class ShootInputComponent : Component
{
    /// <summary>
    /// 是否正在按下射击键（通常为鼠标左键）
    /// </summary>
    public bool IsShooting;

    /// <summary>
    /// 鼠标指针所在的世界坐标X值，单位：米
    /// </summary>
    public float TargetX;

    /// <summary>
    /// 鼠标指针所在的世界坐标Y值，单位：米
    /// </summary>
    public float TargetY;
}