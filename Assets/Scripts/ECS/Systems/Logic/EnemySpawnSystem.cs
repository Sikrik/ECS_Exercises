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

    // 在 EnemySpawnSystem 的 SpawnEnemy 方法中
    private void SpawnEnemy(GameConfig config)
    {
        EnemyType type = (EnemyType)Random.Range(0, 3);
        Vector3 spawnPos = new Vector3(Random.Range(-12, 12), Random.Range(-7, 7), 0);
    
        // 使用工厂一键创建
        EnemyFactory.Create(type, spawnPos);
    }
}