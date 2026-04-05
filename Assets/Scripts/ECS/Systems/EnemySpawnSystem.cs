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
        
        // 1. 获取预制体资源
        EnemyType type = (EnemyType)Random.Range(0, 3);
        GameObject prefab = pool.GetEnemyPrefab(type);
        if (prefab == null) return;

        // --- 核心修复：获取预制体的原始颜色 ---
        Color prefabColor = Color.white; // 兜底颜色
        if (prefab.TryGetComponent<SpriteRenderer>(out var prefabSr))
        {
            prefabColor = prefabSr.color; // 读取预制体在资源文件夹里的颜色
        }
        else if (prefab.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer childSr)
        {
            prefabColor = childSr.color; // 如果预制体在子物体上挂的 Renderer
        }

        // 2. 从对象池生成/取出物体
        Vector3 spawnPos = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0);
        GameObject go = pool.Spawn(prefab, spawnPos, Quaternion.identity);
        
        // --- 重置实例颜色：将从池子拿出的物体恢复为该预制体的本色 ---
        if (go.TryGetComponent<SpriteRenderer>(out var instanceSr))
        {
            instanceSr.color = prefabColor;
        }
        else if (go.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer instanceChildSr)
        {
            instanceChildSr.color = prefabColor;
        }
        
        // 3. 创建实体
        Entity enemy = ecs.CreateEntity();
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0, 0));
        enemy.AddComponent(new CollisionComponent(config.EnemyCollisionRadius));
        enemy.AddComponent(new BouncyTag());
        enemy.AddComponent(new ViewComponent(go, prefab));
        
        // --- 记录此实体的“本色”档案，供 SlowEffectSystem 恢复使用 ---
        enemy.AddComponent(new BaseColorComponent(prefabColor));
        
        // 4. 初始化属性
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