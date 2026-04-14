// 路径: Assets/Scripts/ECS/Systems/GamePlay/EnemySpawnSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnSystem : SystemBase
{
    private enum SpawnState { InitWave, Spawning, WaitingForClear, DelayingNextWave, Finished }
    
    private SpawnState _state = SpawnState.InitWave;
    private int _currentWaveListIndex = 0;
    private float _timer = 0;

    // 【修改】混合兵种卡池，现在存储的是带等级的 SpawnInfo
    private List<EnemySpawnInfo> _spawnPool = new List<EnemySpawnInfo>();

    private const float SPAWN_MIN_RADIUS = 12f;
    private const float SPAWN_MAX_RADIUS = 15f;

    public EnemySpawnSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = BattleManager.Instance.Config;
        
        if (config.Waves == null || config.Waves.Count == 0 || _state == SpawnState.Finished) 
            return;

        var currentWave = config.Waves[_currentWaveListIndex];

        switch (_state)
        {
            case SpawnState.InitWave:
                _spawnPool.Clear();
                // 【修改】装填卡池逻辑
                foreach (var spawnInfo in currentWave.SpawnList)
                {
                    for (int i = 0; i < spawnInfo.Count; i++) 
                    {
                        _spawnPool.Add(spawnInfo);
                    }
                }
                
                // 洗牌算法打乱卡池
                for (int i = 0; i < _spawnPool.Count; i++)
                {
                    int rnd = UnityEngine.Random.Range(i, _spawnPool.Count);
                    var temp = _spawnPool[i];
                    _spawnPool[i] = _spawnPool[rnd];
                    _spawnPool[rnd] = temp;
                }

                _state = SpawnState.Spawning;
                _timer = currentWave.SpawnInterval; 
                break;

            case SpawnState.Spawning:
                _timer += deltaTime;
                if (_timer >= currentWave.SpawnInterval)
                {
                    _timer = 0;
                    
                    // 从卡池抽一只带等级的怪生成
                    var enemyToSpawn = _spawnPool[_spawnPool.Count - 1];
                    _spawnPool.RemoveAt(_spawnPool.Count - 1);
                    
                    SpawnEnemy(enemyToSpawn.Id, enemyToSpawn.Level);

                    if (_spawnPool.Count == 0)
                    {
                        _state = SpawnState.WaitingForClear;
                    }
                }
                break;

            case SpawnState.WaitingForClear:
                var enemies = GetEntitiesWith<EnemyTag>();
                int aliveCount = 0;
                foreach (var e in enemies)
                {
                    if (e.IsAlive && !e.HasComponent<DeadTag>()) aliveCount++;
                }

                if (aliveCount == 0)
                {
                    _state = SpawnState.DelayingNextWave;
                    _timer = 0;
                }
                break;

            case SpawnState.DelayingNextWave:
                _timer += deltaTime;
                if (_timer >= currentWave.NextWaveDelay)
                {
                    _timer = 0;
                    _currentWaveListIndex++;

                    if (_currentWaveListIndex >= config.Waves.Count)
                    {
                        _state = SpawnState.Finished;
                        TriggerVictory();
                    }
                    else
                    {
                        _state = SpawnState.InitWave; 
                    }
                }
                break;
        }

        BattleManager.Instance.CurrentWave = Mathf.Min(_currentWaveListIndex + 1, config.Waves.Count);
        BattleManager.Instance.MaxWave = config.Waves.Count;
    }

    // 【修改】传入 Level 级别参数
    private void SpawnEnemy(string enemyId, int level)
    {
        if (Enum.TryParse<EnemyType>(enemyId, out EnemyType type))
        {
            Vector3 spawnPos = GetOffScreenSpawnPosition();
            EnemyFactory.Create(type, level, spawnPos); // 传递等级
        }
        else
        {
            Debug.LogError($"[EnemySpawnSystem] 波次配置中出现了无效的敌人类型: {enemyId}");
        }
    }

    private Vector3 GetOffScreenSpawnPosition()
    {
        Vector2 centerPos = Vector2.zero;
        var player = ECSManager.Instance.PlayerEntity;
        
        if (player != null && player.IsAlive && player.HasComponent<PositionComponent>())
        {
            var pComp = player.GetComponent<PositionComponent>();
            centerPos = new Vector2(pComp.X, pComp.Y);
        }

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float radius = UnityEngine.Random.Range(SPAWN_MIN_RADIUS, SPAWN_MAX_RADIUS);
        float x = centerPos.x + Mathf.Cos(angle) * radius;
        float y = centerPos.y + Mathf.Sin(angle) * radius;

        return new Vector3(x, y, 0f);
    }

    private void TriggerVictory()
    {
        Debug.Log("<color=green>所有波次清空，游戏胜利！</color>");
        var eventEntity = ECSManager.Instance.CreateEntity();
        eventEntity.AddComponent(new GameVictoryEventComponent()); 
    }
}