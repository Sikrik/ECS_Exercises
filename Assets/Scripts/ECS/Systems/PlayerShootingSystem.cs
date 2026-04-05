using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    public static BulletType CurrentBulletType = BulletType.Normal;
    
    public PlayerShootingSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        _shootTimer += deltaTime;
        if (_shootTimer >= config.ShootInterval) {
            _shootTimer = 0;
            Shoot(config);
        }
    }

    private void Shoot(GameConfig config)
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearest(pPos.X, pPos.Y);
        if (target == null) return;

        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;

        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * config.BulletSpeed, dir.y * config.BulletSpeed, 0));
        bullet.AddComponent(new LifetimeComponent { RemainingTime = config.BulletLifeTime });
        bullet.AddComponent(new CollisionComponent(config.BulletCollisionRadius));
        
        // --- 核心修复：记录 Prefab 来源 ---
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));

        // 挂载特定伤害组件
        bullet.AddComponent(new DamageComponent(config.BulletDamage));
        if (CurrentBulletType == BulletType.Slow)
            bullet.AddComponent(new SlowEffectComponent(config.SlowRatio, config.SlowDuration));
    }

    private Entity FindNearest(float x, float y) {
        // 简化的查找逻辑...
        return null; 
    }
}