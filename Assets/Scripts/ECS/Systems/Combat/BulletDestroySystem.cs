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

            // 1. 过滤：确保发起者是子弹，且目标依然存活
            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<BulletTag>()) continue;
            if (target == null || !target.IsAlive) continue;

            // 2. 【核心修复】：同阵营过滤。子弹不应该因为碰到发射者自己或同类友军而被拦截销毁
            var bFac = bullet.GetComponent<FactionComponent>();
            var tFac = target.GetComponent<FactionComponent>();
            if (bFac != null && tFac != null && bFac.Value == tFac.Value) 
            {
                continue;
            }

            // 3. 【核心修复】：不再写死 EnemyTag，只要是肉体（玩家或敌人），子弹打中后就触发穿透/销毁判定
            if (!target.HasComponent<EnemyTag>() && !target.HasComponent<PlayerTag>()) 
            {
                continue;
            }

            // 处理穿透逻辑
            var pierce = bullet.GetComponent<PierceComponent>();
            if (pierce != null)
            {
                // 注意：防重检测在 PhysicsDetectionSystem 已经拦截过了，这里必然是合法命中的新目标
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