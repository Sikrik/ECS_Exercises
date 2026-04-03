/// <summary>
/// 位置组件，存储实体的世界坐标位置
/// </summary>
public class PositionComponent:Component
{
    /// <summary>
    /// X轴坐标
    /// </summary>
    public float X;
    
    /// <summary>
    /// Y轴坐标
    /// </summary>
    public float Y;
    
    /// <summary>
    /// Z轴坐标
    /// </summary>
    public float Z;
    
    /// <summary>
    /// 上一帧的X轴坐标，用于解决高速物体的碰撞穿透问题
    /// </summary>
    public float PreviousX;
    
    /// <summary>
    /// 上一帧的Y轴坐标，用于解决高速物体的碰撞穿透问题
    /// </summary>
    public float PreviousY;
    
    /// <summary>
    /// 初始化位置组件实例
    /// </summary>
    /// <param name="x">X轴初始坐标</param>
    /// <param name="y">Y轴初始坐标</param>
    /// <param name="z">Z轴初始坐标</param>
    public PositionComponent(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
        // 初始时，上一帧位置等于当前位置
        PreviousX = x;
        PreviousY = y;
    }
}