using System.Collections.Generic;

public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;
            var source = evt.Source;

            if (target == null || !target.IsAlive) continue;

            // 【核心修复】：不要让怪物在硬直（HitRecovery）期间免伤！
            // 如果你的代码里有 if (target.HasComponent<HitRecoveryComponent>()) continue; 请果断删掉它！
            
            // 只有玩家才享受受击无敌帧保护
            if (target.HasComponent<PlayerTag>() && target.HasComponent<InvincibleComponent>())
            {
                continue; 
            }

            // 正常的扣血逻辑
            if (target.HasComponent<HealthComponent>() && source.HasComponent<DamageComponent>())
            {
                var hp = target.GetComponent<HealthComponent>();
                var dmg = source.GetComponent<DamageComponent>();
                
                hp.CurrentHealth -= dmg.Value;

                // 子弹命中后给自己打上销毁标签（因为已经在 Bootstrap 排在特效后面了，不影响特效生成）
                if (source.HasComponent<BulletTag>() && !source.HasComponent<PendingDestroyComponent>())
                {
                    source.AddComponent(new PendingDestroyComponent());
                }
            }
        }
        ReturnListToPool(hitEvents);
    }

    private bool IsEnemyFaction(Entity a, Entity b)
    {
        bool aIsPlayerSide = a.HasComponent<PlayerTag>() || a.HasComponent<BulletTag>();
        bool bIsEnemySide = b.HasComponent<EnemyTag>();
        bool aIsEnemySide = a.HasComponent<EnemyTag>();
        bool bIsPlayerSide = b.HasComponent<PlayerTag>();
        return (aIsPlayerSide && bIsEnemySide) || (aIsEnemySide && bIsPlayerSide);
    }

    private void ApplyDamageIntent(Entity target, float damage)
    {
        var existing = target.GetComponent<DamageTakenEventComponent>();
        if (existing != null) existing.DamageAmount += damage;
        else target.AddComponent(EventPool.GetDamageEvent(damage));
    }
}