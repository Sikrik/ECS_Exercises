// 先放枚举，在类的外面
public enum BulletType
{
    Normal,     // 普通子弹
    Slow,       // 减速子弹
    ChainLightning, // 连锁闪电
    AOE         // 范围伤害
}
/// <summary>
/// 子弹组件，用于标记子弹实体，并存储子弹的相关属性
/// 仅用于存储数据，不包含业务逻辑
/// </summary>
public class BulletComponent : Component 
{
    /// <summary>子弹类型，决定命中后的效果</summary>
    public BulletType Type;
    /// <summary>子弹的伤害值</summary>
    public float Damage;
    /// <summary>子弹的剩余存活时间</summary>
    public float LifeTime;
}