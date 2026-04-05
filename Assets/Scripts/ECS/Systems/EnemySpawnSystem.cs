using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnSystem : SystemBase
{
    private float _timer;
    public EnemySpawnSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        _timer += deltaTime;

        if (_timer >= config.InitialSpawnInterval)
        {
            _timer = 0;
            SpawnEnemy(config);
        }
    }

    private void SpawnEnemy(GameConfig config)
    {
        var pool = PoolManager.Instance;
        var ecs = ECSManager.Instance;
        
        // 1. 随机敌人类型并从对象池获取
        EnemyType type = (EnemyType)Random.Range(0, 3);
        GameObject prefab = pool.GetEnemyPrefab(type);
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(-12, 12), Random.Range(-7, 7), 0);
        GameObject go = pool.Spawn(prefab, spawnPos, Quaternion.identity);
        
        // 2. 创建实体
        Entity enemy = ecs.CreateEntity();
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new ViewComponent(go, prefab));

        // --- 仿 DOTS 核心：添加功能标签 ---
        // 标记 NeedsBakingTag：让 PhysicsBakingSystem 自动处理 Collider 引用和映射
        enemy.AddComponent(new NeedsBakingTag()); 
        // 标记 BouncyTag：让 CollisionSystem 对其执行法线反弹
        enemy.AddComponent(new BouncyTag());

        // 3. 设置属性数据
        float health = type switch {
            EnemyType.Fast => config.FastEnemyMaxHealth,
            EnemyType.Tank => config.TankEnemyMaxHealth,
            _ => config.EnemyMaxHealth
        };

        float speed = type switch {
            EnemyType.Fast => config.FastEnemySpeed,
            EnemyType.Tank => config.TankEnemySpeed,
            _ => config.EnemyMoveSpeed
        };

        enemy.AddComponent(new HealthComponent(health));
        enemy.AddComponent(new EnemyStatsComponent { 
            Type = type, 
            MoveSpeed = speed, 
            Damage = config.EnemyDamage,
            AttackCooldown = config.EnemyAttackCooldown
        });
    }
}