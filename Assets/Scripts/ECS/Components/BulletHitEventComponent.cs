public class BulletHitEventComponent : Component 
{
    public Entity Target; // 命中的目标敌人
    public BulletHitEventComponent(Entity target) => Target = target;
}