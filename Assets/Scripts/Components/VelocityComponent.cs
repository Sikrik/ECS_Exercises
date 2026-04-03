/// <summary>
/// 速度组件，存储实体的移动速度
/// 用于MovementSystem计算实体的位置更新
/// </summary>
public class VelocityComponent:Component
{
    /// <summary>
    /// X轴方向的移动速度
    /// </summary>
    public float SpeedX;
    
    /// <summary>
    /// Y轴方向的移动速度
    /// </summary>
    public float SpeedY;
    
    /// <summary>
    /// Z轴方向的移动速度
    /// </summary>
    public float SpeedZ;
    
    // 兼容属性：支持X/Y/Z别名访问，适配协同文档的新代码
    public float X {
        get { return SpeedX; }
        set { SpeedX = value; }
    }
    public float Y {
        get { return SpeedY; }
        set { SpeedY = value; }
    }
    public float Z {
        get { return SpeedZ; }
        set { SpeedZ = value; }
    }
    
    /// <summary>
    /// 初始化速度组件实例
    /// </summary>
    /// <param name="x">X轴初始速度</param>
    /// <param name="y">Y轴初始速度</param>
    /// <param name="z">Z轴初始速度</param>
    public VelocityComponent(float x, float y, float z)
    {
        SpeedX = x;
        SpeedY = y;
        SpeedZ = z;
    }
}