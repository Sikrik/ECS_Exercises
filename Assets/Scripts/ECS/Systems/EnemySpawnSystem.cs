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
        if (_timer >= config.InitialSpawnInterval) { _timer = 0; SpawnEnemy(config); }
    }

    private void SpawnEnemy(GameConfig config)
    {
        var pool = PoolManager.Instance;
        var ecs = ECSManager.Instance;
        
        EnemyType type = (EnemyType)Random.Range(0, 3);
        GameObject prefab = pool.GetEnemyPrefab(type);
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(-12, 12), Random.Range(-7, 7), 0);
        GameObject go = pool.Spawn(prefab, spawnPos, Quaternion.identity);
        
        Entity enemy = ecs.CreateEntity();
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new ViewComponent(go, prefab));
        enemy.AddComponent(new NeedsBakingTag()); 
        enemy.AddComponent(new BouncyTag());

        // 核心：设置怪物要撞谁 (Player层)
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player")));
        
        // 核心：给怪物增加伤害组件，否则通用 DamageSystem 不会处理它
        enemy.AddComponent(new DamageComponent(config.EnemyDamage));

        float health = type == EnemyType.Fast ? config.FastEnemyMaxHealth : (type == EnemyType.Tank ? config.TankEnemyMaxHealth : config.EnemyMaxHealth);
        float speed = type == EnemyType.Fast ? config.FastEnemySpeed : (type == EnemyType.Tank ? config.TankEnemySpeed : config.EnemyMoveSpeed);

        enemy.AddComponent(new HealthComponent(health));
        enemy.AddComponent(new EnemyStatsComponent { Type = type, MoveSpeed = speed, Damage = config.EnemyDamage });
    }
}