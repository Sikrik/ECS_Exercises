/// <summary>
/// 事件组件：标记一颗子弹命中了某个目标，仅在命中瞬间存在。
/// </summary>
public class BulletHitEventComponent : Component 
{
    public Entity Target;
    public BulletHitEventComponent(Entity target) => Target = target;
}