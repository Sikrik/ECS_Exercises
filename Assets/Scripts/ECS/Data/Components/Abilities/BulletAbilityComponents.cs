using System.Collections.Generic;

/// <summary>
/// 范围伤害组件，用于标识子弹具有AOE（Area of Effect）效果
/// </summary>
public class AOEComponent : Component 
{
    /// <summary>
    /// AOE效果的半径范围，单位：米
    /// </summary>
    public float Radius;

    /// <summary>
    /// 初始化AOE组件
    /// </summary>
    /// <param name="r">AOE效果的作用半径</param>
    public AOEComponent(float r)
    {
        Radius = r;
    }
}

/// <summary>
/// 连锁伤害组件，用于标识子弹具有连锁打击效果（如闪电链）
/// </summary>
public class ChainComponent : Component 
{
    /// <summary>
    /// 连锁伤害的最大目标数量
    /// </summary>
    public int MaxTargets;
    
    /// <summary>
    /// 连锁搜索下一个目标的最大距离，单位：米
    /// </summary>
    public float Range;

    /// <summary>
    /// 初始化连锁伤害组件
    /// </summary>
    /// <param name="m">连锁伤害的最大目标数量</param>
    /// <param name="r">连锁搜索下一个目标的最大距离</param>
    public ChainComponent(int m, float r) { MaxTargets = m; Range = r;  }
}
public class PierceComponent : Component 
{
    public int MaxPierces;      // 最大穿透次数
    public int CurrentPierces;  // 当前穿透次数
    // 记录已经打过的敌人，防止同一帧或连续帧对同一敌人扣血
    public HashSet<Entity> HitHistory = new HashSet<Entity>(); 
    
    public PierceComponent(int max) { MaxPierces = max; CurrentPierces = max; }
}