using UnityEngine;
/// <summary>
/// 视图组件，存储实体对应的Unity GameObject视图对象
/// 用于将ECS的数据同步到Unity的场景视图中
/// 这是连接ECS纯数据层和Unity渲染层的桥梁，几乎所有需要显示的实体都必须有此组件
/// </summary>
public class ViewComponent:Component
{
    /// <summary>
    /// 实体对应的Unity场景对象，用于显示实体的视觉表现
    /// </summary>
    public GameObject GameObject;
    
    /// <summary>
    /// 初始化视图组件实例
    /// </summary>
    /// <param name="go">实体对应的GameObject实例</param>
    public ViewComponent(GameObject go)
    {
        GameObject = go;
    }
}
/// <summary>
/// 位置组件，存储实体的世界坐标位置
/// 这是游戏实体最基本的属性之一，几乎所有移动、碰撞、渲染系统都依赖位置信息
/// 没有位置的实体无法在游戏世界中存在和交互
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
/// <summary>
/// 速度组件，存储实体的移动速度
/// 用于MovementSystem计算实体的位置更新
/// 对于动态实体（敌人、子弹等）至关重要，但静态实体可能不需要此组件
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
/// <summary>
/// 血量组件，存储实体的血量相关属性
/// 用于表示实体的存活状态
/// 对于所有可被摧毁的实体（敌人、玩家等）是必需的，决定了实体的生命周期
/// </summary>
public class HealthComponent : Component
{
    /// <summary>
    /// 实体的当前血量
    /// 当该值小于等于0时，实体将会被销毁
    /// </summary>
    public float CurrentHealth;
    
    /// <summary>
    /// 实体的最大血量，初始时当前血量等于最大血量
    /// </summary>
    public float MaxHealth;
    
    // 新增：无敌计时器，受伤后生效
    public float InvincibleTimer;
    
    /// <summary>
    /// 初始化血量组件实例
    /// </summary>
    /// <param name="maxHealth">实体的最大血量</param>
    public HealthComponent(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }
}
/// <summary>
/// 碰撞组件，存储实体的碰撞检测相关属性
/// 用于圆形碰撞检测的半径参数
/// 仅在需要进行碰撞检测的实体上使用，不是所有实体都需要（如纯装饰性物体）
/// </summary>
public class CollisionComponent : Component
{
    /// <summary>
    /// 实体的碰撞半径，用于圆形碰撞检测
    /// 当两个实体的中心距离小于两者半径之和时，判定为碰撞
    /// </summary>
    public float Radius;
    
    /// <summary>
    /// 初始化碰撞组件实例
    /// </summary>
    /// <param name="radius">碰撞半径</param>
    public CollisionComponent(float radius)
    {
        Radius = radius;
    }
}
