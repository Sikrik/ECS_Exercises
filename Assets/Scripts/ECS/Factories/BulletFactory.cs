// 路径: Assets/Scripts/ECS/Factories/BulletFactory.cs
using UnityEngine;

public static class BulletFactory 
{
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction, FactionType sourceFaction = FactionType.Player, WeaponModifierComponent modifiers = null)
    {
        var ecs = ECSManager.Instance;
        
        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        
        // ==========================================
        // 基础默认数值（代替废弃的 Bullet_Config）
        // ==========================================
        float baseSpeed = 12f;
        float baseDamage = 15f;
        float baseHitRadius = 0.2f;
        float baseLifeTime = 3f;

        // --- 1. 基础组件装配 ---
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction));
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * baseSpeed, direction.y * baseSpeed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new DamageComponent(baseDamage));
        bullet.AddComponent(new LifetimeComponent { Duration = baseLifeTime });
        bullet.AddComponent(new TraceComponent(position.x, position.y));
        
        // --- 2. 补充通用物理组件 ---
        bullet.AddComponent(new CollisionComponent(baseHitRadius));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());

        bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: true));

        // --- 3. 动态结算升级系统修饰器 (提取等级数值) ---
        if (modifiers != null)
        {
            // 如果获得了减速附魔，等级越高可以提供更长的持续时间
            int slowLevel = modifiers.GetLevel("AddSlow");
            if (slowLevel > 0 && !bullet.HasComponent<SlowEffectComponent>())
            {
                bullet.AddComponent(new SlowEffectComponent(0.5f, 2f + slowLevel));
            }
                
            // 如果获得了闪电附魔，基础弹射 3 次，每级加 1 次
            int chainLevel = modifiers.GetLevel("AddChain");
            if (chainLevel > 0 && !bullet.HasComponent<ChainComponent>())
            {
                bullet.AddComponent(new ChainComponent(3 + chainLevel, 5f));
            }
                
            // 如果获得了爆炸附魔，基础半径 2，每级加 1
            int aoeLevel = modifiers.GetLevel("AddAOE");
            if (aoeLevel > 0 && !bullet.HasComponent<AOEComponent>())
            {
                bullet.AddComponent(new AOEComponent(2f + aoeLevel));
            }
        }

        return bullet;
    }
}