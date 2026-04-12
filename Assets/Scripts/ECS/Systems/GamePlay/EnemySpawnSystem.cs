using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnSystem : SystemBase
{
    private enum SpawnState { InitWave, Spawning, WaitingForClear, DelayingNextWave, Finished }
    
    private SpawnState _state = SpawnState.InitWave;
    private int _currentWaveListIndex = 0;
    private float _timer = 0;

    // 混合兵种卡池
    private List<string> _spawnPool = new List<string>();

    private const float SPAWN_MIN_RADIUS = 12f;
    private const float SPAWN_MAX_RADIUS = 15f;

    public EnemySpawnSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        
        // 防御：没有波次配置或已经完成通关，则不执行
        if (config.Waves == null || config.Waves.Count == 0 || _state == SpawnState.Finished) 
            return;

        var currentWave = config.Waves[_currentWaveListIndex];

        switch (_state)
        {
            case SpawnState.InitWave:
                // 1. 初始化本波卡的卡池
                _spawnPool.Clear();
                foreach (var kvp in currentWave.SpawnDict)
                {
                    for (int i = 0; i < kvp.Value; i++) 
                    {
                        _spawnPool.Add(kvp.Key);
                    }
                }
                
                // 2. 洗牌算法打乱卡池，保证混合兵种随机刷新
                for (int i = 0; i < _spawnPool.Count; i++)
                {
                    int rnd = UnityEngine.Random.Range(i, _spawnPool.Count);
                    string temp = _spawnPool[i];
                    _spawnPool[i] = _spawnPool[rnd];
                    _spawnPool[rnd] = temp;
                }

                _state = SpawnState.Spawning;
                _timer = currentWave.SpawnInterval; // 立即开始刷第一只
                break;

            case SpawnState.Spawning:
                _timer += deltaTime;
                if (_timer >= currentWave.SpawnInterval)
                {
                    _timer = 0;
                    
                    // 从卡池末尾抽一只怪生成
                    string enemyToSpawn = _spawnPool[_spawnPool.Count - 1];
                    _spawnPool.RemoveAt(_spawnPool.Count - 1);
                    
                    SpawnEnemy(enemyToSpawn);

                    // 卡池抽空了，进入清怪等待阶段
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

                    // 判断是否打完了最后一波
                    if (_currentWaveListIndex >= config.Waves.Count)
                    {
                        _state = SpawnState.Finished;
                        TriggerVictory();
                    }
                    else
                    {
                        _state = SpawnState.InitWave; // 循环回到初始化新卡池
                    }
                }
                break;
        }

        // 无论何种状态，始终更新数据层，供 UI 读取
        ECSManager.Instance.CurrentWave = Mathf.Min(_currentWaveListIndex + 1, config.Waves.Count);
        ECSManager.Instance.MaxWave = config.Waves.Count;
    }

    private void SpawnEnemy(string enemyId)
    {
        if (Enum.TryParse<EnemyType>(enemyId, out EnemyType type))
        {
            Vector3 spawnPos = GetOffScreenSpawnPosition();
            EnemyFactory.Create(type, spawnPos);
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

        // 从屏幕外随机极坐标产生
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
        eventEntity.AddComponent(new GameVictoryEventComponent()); // 抛出事件给 UI 拦截
    }
}