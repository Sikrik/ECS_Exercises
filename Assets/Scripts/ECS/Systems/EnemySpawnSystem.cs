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
        
        // 1. 随机选择敌人类型
        EnemyType type = (EnemyType)Random.Range(0, 3);
        GameObject prefab = pool.GetEnemyPrefab(type);
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(-12, 12), Random.Range(-7, 7), 0);
        GameObject go = pool.Spawn(prefab, spawnPos, Quaternion.identity);
        
        // 2. 创建实体并挂载基础组件
        Entity enemy = ecs.CreateEntity();
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new ViewComponent(go, prefab));
        enemy.AddComponent(new NeedsBakingTag()); // 等待烘焙系统处理物理
        enemy.AddComponent(new DamageComponent(config.EnemyDamage));

        // --- BUG 修复：坦克分类讨论 ---
        // 只有非坦克单位才添加 BouncyTag，使其能被弹开
        if (type != EnemyType.Tank)
        {
            enemy.AddComponent(new BouncyTag());
        }

        // --- BUG 修复：补全碰撞与伤害逻辑 ---
        // 设置怪物要撞谁（Player层）
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player")));
        // 必须挂载伤害组件，否则玩家不会掉血
        enemy.AddComponent(new DamageComponent(config.EnemyDamage));

        // 3. 根据类型设置具体数值
        float health = type == EnemyType.Fast ? config.FastEnemyMaxHealth : 
                      (type == EnemyType.Tank ? config.TankEnemyMaxHealth : config.EnemyMaxHealth);
        float speed = type == EnemyType.Fast ? config.FastEnemySpeed : 
                     (type == EnemyType.Tank ? config.TankEnemySpeed : config.EnemyMoveSpeed);

        enemy.AddComponent(new HealthComponent(health));
        enemy.AddComponent(new EnemyStatsComponent { 
            Type = type, 
            MoveSpeed = speed, 
            Damage = config.EnemyDamage 
        });
    }
}