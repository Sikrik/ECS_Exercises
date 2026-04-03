/// <summary>
/// 碰撞组件，存储实体的碰撞检测相关属性
/// 用于圆形碰撞检测的半径参数
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