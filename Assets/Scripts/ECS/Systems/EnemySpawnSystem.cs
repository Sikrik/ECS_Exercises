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
        if (_timer >= config.InitialSpawnInterval) {
            _timer = 0;
            SpawnEnemy(config);
        }
    }

    private void SpawnEnemy(GameConfig config)
    {
        var pool = PoolManager.Instance;
        var ecs = ECSManager.Instance;
        
        // 随机选择敌人类型
        EnemyType type = (EnemyType)Random.Range(0, 3);
        GameObject prefab = pool.GetEnemyPrefab(type);
        
        Vector3 spawnPos = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0);
        GameObject go = pool.Spawn(prefab, spawnPos, Quaternion.identity);
        
        Entity enemy = ecs.CreateEntity();
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0, 0));
        enemy.AddComponent(new CollisionComponent(config.EnemyCollisionRadius));
        enemy.AddComponent(new HealthComponent(config.EnemyMaxHealth));
        enemy.AddComponent(new BouncyTag());
        
        // --- 核心修复：记录 Prefab 来源 ---
        enemy.AddComponent(new ViewComponent(go, prefab));
        
        enemy.AddComponent(new EnemyStatsComponent { 
            Type = type, 
            MoveSpeed = config.EnemyMoveSpeed, 
            Damage = config.EnemyDamage 
        });
    }
}