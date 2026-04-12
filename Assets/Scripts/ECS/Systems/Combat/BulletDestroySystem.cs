using System.Collections.Generic;

/// <summary>
/// 子弹生命周期管理系统（原子化）
/// 职责：监听碰撞事件，处理穿透次数扣减，并在穿透耗尽后下达销毁判决
/// </summary>
public class BulletDestroySystem : SystemBase
{
    public BulletDestroySystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source;
            var target = evt.Target;

            // 过滤：只处理子弹打中敌人的事件
            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<BulletTag>()) continue;
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            // 处理穿透逻辑
            var pierce = bullet.GetComponent<PierceComponent>();
            if (pierce != null)
            {
                // 如果已经打过这个敌人，跳过防重复计算
                if (pierce.HitHistory.Contains(target)) continue;
                
                pierce.HitHistory.Add(target);
                pierce.CurrentPierces--;

                // 穿透次数耗尽，判处死刑
                if (pierce.CurrentPierces <= 0 && !bullet.HasComponent<PendingDestroyComponent>())
                {
                    bullet.AddComponent(new PendingDestroyComponent());
                }
            }
            else
            {
                // 普通子弹：触碰即销毁
                if (!bullet.HasComponent<PendingDestroyComponent>())
                {
                    bullet.AddComponent(new PendingDestroyComponent());
                }
            }
        }

    }
}