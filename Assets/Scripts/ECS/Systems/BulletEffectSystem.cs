using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 获取所有产生了碰撞事件的子弹
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();

            // 1. 处理单体伤害 (只要子弹有 DamageComponent)
            if (bullet.HasComponent<DamageComponent>() && hitEvent.Target.IsAlive)
            {
                var dmg = bullet.GetComponent<DamageComponent>().Value;
                var health = hitEvent.Target.GetComponent<HealthComponent>();
                if (health != null) health.CurrentHealth -= dmg;
            }

            // 2. 处理爆炸效果 (只要子弹有 AOEComponent)
            if (bullet.HasComponent<AOEComponent>())
            {
                var aoe = bullet.GetComponent<AOEComponent>();
                ProcessAOE(pos.X, pos.Y, aoe);
            }

            // 3. 处理闪电链跳跃 (只要子弹有 ChainComponent)
            if (bullet.HasComponent<ChainComponent>() && hitEvent.Target.IsAlive)
            {
                var chain = bullet.GetComponent<ChainComponent>();
                ProcessChain(hitEvent.Target, pos, chain);
            }

            // 处理完毕，销毁子弹
            ecs.DestroyEntity(bullet);
        }
    }

    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        // 同样可以利用网格优化 AOE 性能
        var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(x, y);
        float rSq = aoe.Radius * aoe.Radius;
        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
            if (d2 <= rSq) e.GetComponent<HealthComponent>().CurrentHealth -= aoe.Damage;
        }
    }

    private void ProcessChain(Entity target, PositionComponent hitPos, ChainComponent config)
    {
        var ecs = ECSManager.Instance;
        List<Entity> hitHistory = new List<Entity> { target };
        Entity current = target;
        Vector3 lastVfxPos = new Vector3(hitPos.X, hitPos.Y, 0);

        for (int i = 0; i < config.MaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.Range * config.Range;
            var curPos = current.GetComponent<PositionComponent>();

            // 在网格中寻找下一个弹射目标
            var nearby = ecs.Grid.GetNearbyEnemies(curPos.X, curPos.Y);
            foreach (var e in nearby)
            {
                if (!e.IsAlive || hitHistory.Contains(e)) continue;
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

                // 生成 VFX 实体
                Entity vfx = ecs.CreateEntity();
                vfx.AddComponent(new LightningVFXComponent(lastVfxPos, nextPosV3));
                vfx.AddComponent(new ViewComponent(PoolManager.Instance.Spawn(PoolManager.Instance.LightningChainVFX, Vector3.zero, Quaternion.identity)));

                current = next;
                lastVfxPos = nextPosV3;
            }
            else break;
        }
    }
}