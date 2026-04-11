using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成系统，负责根据游戏时长动态调整生成频率和数量，实现难度递增
/// </summary>
public class EnemySpawnSystem : SystemBase
{
    /// <summary>
    /// 生成计时器，累计距离上次生成的时间间隔
    /// </summary>
    private float _timer;

    /// <summary>
    /// 游戏总时长计数器，用于推算动态难度系数
    /// </summary>
    private float _totalTime;

    /// <summary>
    /// 常量定义：生成间隔随时间递减的速率（每秒减少的秒数）
    /// </summary>
    private const float SPAWN_INTERVAL_DECAY_RATE = 0.02f;

    /// <summary>
    /// 常量定义：最小生成间隔保底值（秒）
    /// </summary>
    private const float MIN_SPAWN_INTERVAL = 0.2f;

    /// <summary>
    /// 常量定义：波次叠加的时间周期（秒），每经过该时间增加一次生成数量
    /// </summary>
    private const float WAVE_INCREMENT_PERIOD = 30f;

    /// <summary>
    /// 常量定义：敌人生成的最小半径距离（单位：米）
    /// </summary>
    private const float SPAWN_MIN_RADIUS = 12f;

    /// <summary>
    /// 常量定义：敌人生成的最大半径距离（单位：米）
    /// </summary>
    private const float SPAWN_MAX_RADIUS = 15f;

    public EnemySpawnSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        _timer += deltaTime;
        _totalTime += deltaTime;

        // 计算当前动态生成间隔，随着游戏进行逐渐加快刷怪频率
        float currentInterval = CalculateSpawnInterval(config.InitialSpawnInterval);

        if (_timer >= currentInterval)
        {
            _timer = 0;
            
            // 根据存活时间计算本批次生成数量，实现波次叠加效果
            int spawnCount = CalculateSpawnBatchCount();
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy();
            }
        }
    }

    /// <summary>
    /// 计算当前动态生成间隔，基于初始间隔和时间衰减率
    /// </summary>
    private float CalculateSpawnInterval(float initialInterval)
    {
        return Mathf.Max(MIN_SPAWN_INTERVAL, initialInterval - (_totalTime * SPAWN_INTERVAL_DECAY_RATE));
    }

    /// <summary>
    /// 计算当前批次应生成的敌人数量，基于存活时间的波次叠加逻辑
    /// </summary>
    private int CalculateSpawnBatchCount()
    {
        return 1 + Mathf.FloorToInt(_totalTime / WAVE_INCREMENT_PERIOD);
    }

    /// <summary>
    /// 执行单个敌人的生成逻辑
    /// </summary>
    private void SpawnEnemy()
    {
        // 👇 【核心优化】：使用 System.Enum.GetValues 获取枚举类型的总数量
        // 这样一来，你在 EnemyType 枚举里加多少种怪物，这里都会自动适配，不用再手动改数字了！
        int enemyTypeCount = System.Enum.GetValues(typeof(EnemyType)).Length;
        EnemyType type = (EnemyType)Random.Range(0, enemyTypeCount);
        
        // 获取屏幕外的安全生成坐标，防止怪物贴脸生成
        Vector3 spawnPos = GetOffScreenSpawnPosition();
        
        // 使用工厂模式创建敌人实体
        EnemyFactory.Create(type, spawnPos);
    }

    /// <summary>
    /// 获取屏幕外的安全生成坐标，以玩家为中心在指定圆环范围内随机生成
    /// </summary>
    private Vector3 GetOffScreenSpawnPosition()
    {
        Vector2 centerPos = Vector2.zero;
        var player = ECSManager.Instance.PlayerEntity;
        
        // 获取当前玩家的逻辑坐标作为生成圆心
        if (player != null && player.IsAlive && player.HasComponent<PositionComponent>())
        {
            var pComp = player.GetComponent<PositionComponent>();
            centerPos = new Vector2(pComp.X, pComp.Y);
        }

        // 在玩家周围的圆环范围内随机生成（确保从屏幕边缘走进来）
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(SPAWN_MIN_RADIUS, SPAWN_MAX_RADIUS);

        // 极坐标转换为笛卡尔坐标
        float x = centerPos.x + Mathf.Cos(angle) * radius;
        float y = centerPos.y + Mathf.Sin(angle) * radius;

        return new Vector3(x, y, 0f);
    }
}