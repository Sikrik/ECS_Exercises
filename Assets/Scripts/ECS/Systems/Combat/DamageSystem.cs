using System.Collections.Generic;

public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 处理直接碰撞伤害 (CollisionEvent)
        var attackers = GetEntitiesWith<CollisionEventComponent, DamageComponent>();
        foreach (var attacker in attackers)
        {
            var evt = attacker.GetComponent<CollisionEventComponent>();
            var dmg = attacker.GetComponent<DamageComponent>();
            
            if (evt.Target != null && IsEnemyFaction(attacker, evt.Target))
            {
                ApplyDamageIntent(evt.Target, dmg.Value);
            }
        }
        ReturnListToPool(attackers);

        // 2. 统一结算所有伤害意图 (DamageTakenEvent)
        // 无论是碰撞、AOE还是连锁闪电产生的伤害，最终都在这里处理
        var victims = GetEntitiesWith<HealthComponent, DamageTakenEventComponent>();
        foreach (var victim in victims)
        {
            var health = victim.GetComponent<HealthComponent>();
            var damageEvt = victim.GetComponent<DamageTakenEventComponent>();
            
            // 只有非无敌状态才扣血
            if (!victim.HasComponent<InvincibleComponent>())
            {
                health.CurrentHealth -= damageEvt.DamageAmount;
                // 此时不移除 DamageTakenEventComponent，留给后面的 HitReactionSystem 判断是否需要播放受击表现
            }
        }
        ReturnListToPool(victims);
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