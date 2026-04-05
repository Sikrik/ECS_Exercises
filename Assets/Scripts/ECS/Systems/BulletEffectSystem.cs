using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选产生了命中事件的子弹
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();
            var target = hitEvent.Target;

            // 1. 处理单体伤害 (如果有 DamageComponent)
            if (bullet.HasComponent<DamageComponent>() && target.IsAlive)
            {
                var dmg = bullet.GetComponent<DamageComponent>().Value;
                var health = target.GetComponent<HealthComponent>();
                if (health != null) health.CurrentHealth -= dmg;
            }

            // 2. 处理爆炸效果 (如果有 AOEComponent)
            if (bullet.HasComponent<AOEComponent>())
            {
                var aoe = bullet.GetComponent<AOEComponent>();
                ProcessAOE(pos.X, pos.Y, aoe);
            }

            // 3. 处理闪电链弹射 (如果有 ChainComponent)
            if (bullet.HasComponent<ChainComponent>() && target.IsAlive)
            {
                var chain = bullet.GetComponent<ChainComponent>();
                ProcessChain(target, pos, chain);
            }

            // 处理完毕，销毁子弹实体
            ecs.DestroyEntity(bullet);
        }
    }

    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(x, y);
        float rSq = aoe.Radius * aoe.Radius;
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
            if (d2 <= rSq) 
            {
                var health = e.GetComponent<HealthComponent>();
                if (health != null) health.CurrentHealth -= aoe.Damage;
            }
        }
    }

    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config)
    {
        var ecs = ECSManager.Instance;
        List<Entity> hitHistory = new List<Entity> { startTarget };
        Entity current = startTarget;
        Vector3 lastVfxPos = new Vector3(hitPos.X, hitPos.Y, 0);

        for (int i = 0; i < config.MaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.Range * config.Range;
            var curPos = current.GetComponent<PositionComponent>();

            // 在网格中寻找下一个最近的敌人
            var nearby = ecs.Grid.GetNearbyEnemies(curPos.X, curPos.Y);
            foreach (var e in nearby)
            {
                if (!e.IsAlive || !e.HasComponent<EnemyTag>() || hitHistory.Contains(e)) continue;
                var ePos = e.GetComponent<PositionComponent>();
                float d2 = (ePos.X - curPos.X) * (ePos.X - curPos.X) + (ePos.Y - curPos.Y) * (ePos.Y - curPos.Y);
                if (d2 < minDistSq) { minDistSq = d2; next = e; }
            }

            if (next != null)
            {
                hitHistory.Add(next);
                next.GetComponent<HealthComponent>().CurrentHealth -= config.Damage;
                
                var nPos = next.GetComponent<PositionComponent>();
                Vector3 nextPosV3 = new Vector3(nPos.X, nPos.Y, 0);

                // 生成专门的视觉 VFX 实体
                Entity vfx = ecs.CreateEntity();
                vfx.AddComponent(new LightningVFXComponent(lastVfxPos, nextPosV3)); //
                vfx.AddComponent(new ViewComponent(PoolManager.Instance.Spawn(PoolManager.Instance.LightningChainVFX, Vector3.zero, Quaternion.identity)));

                current = next;
                lastVfxPos = nextPosV3;
            }
            else break;
        }
    }
}