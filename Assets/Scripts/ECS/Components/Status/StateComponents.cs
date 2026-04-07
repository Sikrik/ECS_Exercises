public class LifetimeComponent : Component 
{
    public float Duration; // 子弹、特效等的生存计时
}



/// <summary>
/// 待销毁标记：被贴上此标签的实体，将在帧末被统一回收。
/// </summary>
public class PendingDestroyComponent : Component { }