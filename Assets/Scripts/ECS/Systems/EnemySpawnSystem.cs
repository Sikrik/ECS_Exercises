using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成系统：负责根据游戏进度动态在视野外生成不同类型的敌人
/// 优化点：适配 PoolManager 智能接口，移除冗余的对象池引用
/// </summary>
public class EnemySpawnSystem : SystemBase
{
    private float _spawnTimer;

    public EnemySpawnSystem(List<Entity> entities) : base(entities)
    {
        _spawnTimer = 0;
    }

    public override void Update(float deltaTime)
    {
        if (Time.timeScale <= 0) return;

        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        
        _spawnTimer += deltaTime;

        // 难度曲线：随分数增加缩短生成间隔
        float spawnInterval = Mathf.Max(
            config.MinSpawnInterval,
            config.InitialSpawnInterval - (ecs.Score * config.SpawnIntervalDecrease)
        );

        if (_spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            _spawnTimer = 0;
        }
    }

    private void SpawnEnemy()
    {
        var ecs = ECSManager.Instance;
        var config = ecs.Config;

        // 1. 计算生成坐标（相机视野外随机圆环）
        Vector2 spawnPos = CalculateSpawnPosition();

        // 2. 根据难度（分数）随机敌人类型
        EnemyType type = SelectEnemyTypeByScore(ecs.Score);

        // 3. 获取属性配置
        float health = config.EnemyMaxHealth;
        float speed = config.EnemyMoveSpeed;
        float radius = config.EnemyCollisionRadius;

        switch (type)
        {
            case EnemyType.Fast:
                health = config.FastEnemyMaxHealth;
                speed = config.FastEnemySpeed;
                radius = config.FastEnemyCollisionRadius;
                break;
            case EnemyType.Tank:
                health = config.TankEnemyMaxHealth;
                speed = config.TankEnemySpeed;
                radius = config.TankEnemyCollisionRadius;
                break;
        }

        // 4. 核心架构适配：通过 PoolManager 智能生成对象
        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type);
        GameObject enemyGo = PoolManager.Instance.Spawn(prefab, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity);

        if (enemyGo == null) return;

        // 5. 自动同步视觉半径（如果 Sprite 尺寸与配置不符）
        if (enemyGo.TryGetComponent<SpriteRenderer>(out var sr))
        {
            radius = Mathf.Min(sr.bounds.size.x, sr.bounds.size.y) * 0.5f;
        }

        // 6. 创建 ECS 实体并组装组件
        Entity enemy = ecs.CreateEntity();
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0, 0));
        enemy.AddComponent(new HealthComponent(health));
        enemy.AddComponent(new CollisionComponent(radius));
        enemy.AddComponent(new ViewComponent(enemyGo));
        if (type == EnemyType.Normal || type == EnemyType.Fast)
        {
            enemy.AddComponent(new BouncyComponent());
        }
        // 敌人业务组件
        enemy.AddComponent(new EnemyComponent()
        {
            Type = type,
            Damage = config.EnemyDamage,
            AttackCooldown = config.EnemyAttackCooldown,
            CurrentCooldown = 0,
            MoveSpeed = speed // 确保速度被记录在组件中
        });
    }

    private Vector2 CalculateSpawnPosition()
    {
        Camera cam = Camera.main;
        float height = cam.orthographicSize;
        float width = height * cam.aspect;
        
        float spawnRadius = Mathf.Max(width, height) + 2.0f;
        float angle = Random.Range(0, Mathf.PI * 2);
        
        return new Vector2(Mathf.Cos(angle) * spawnRadius, Mathf.Sin(angle) * spawnRadius);
    }

    private EnemyType SelectEnemyTypeByScore(int score)
    {
        float rand = Random.value;
        if (score > 100 && rand < 0.2f) return EnemyType.Tank;
        if (score > 50 && rand < 0.3f) return EnemyType.Fast;
        return EnemyType.Normal;
    }
}