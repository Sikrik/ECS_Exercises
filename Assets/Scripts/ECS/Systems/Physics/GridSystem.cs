using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格空间划分系统
/// 职责：将场景中的实体按坐标分配到网格中，通过局部查询极大提升物理检测和索敌性能。
/// 优化：对接 ECSManager 列表池实现 0 GC，支持动态网格清理。
/// </summary>
public class GridSystem : SystemBase
{
    public float CellSize;
    
    // 网格容器：Key 为坐标索引，Value 为该格子内的实体列表
    public Dictionary<Vector2Int, List<Entity>> Grid = new Dictionary<Vector2Int, List<Entity>>();
    
    // 内部列表池：专门用于复用网格内部存储用的 List，避免每帧 Clear 字典导致的开销
    private Stack<List<Entity>> _internalListPool = new Stack<List<Entity>>();

    public GridSystem(float cellSize, List<Entity> entities) : base(entities)
    {
        CellSize = cellSize;
    }

    /// <summary>
    /// 每帧重建网格：在逻辑组（Simulation Group）最开始执行
    /// </summary>
    public override void Update(float deltaTime)
    {
        // 1. 回收上一帧网格内部使用的所有 List 到内部池
        foreach (var list in Grid.Values)
        {
            list.Clear();
            _internalListPool.Push(list);
        }
        Grid.Clear();

        // 2. 扫描所有敌人并放入网格（目前主要用于玩家索敌和怪物互挤）
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        foreach (var e in enemies)
        {
            // 只有活着的实体才进网格
            if (!e.IsAlive) continue;
            
            var pos = e.GetComponent<PositionComponent>();
            Vector2Int key = GetKey(pos.X, pos.Y);
            
            if (!Grid.ContainsKey(key)) 
            {
                // 从内部池拿 List，如果池空了则新建
                Grid[key] = _internalListPool.Count > 0 ? _internalListPool.Pop() : new List<Entity>();
            }
            Grid[key].Add(e);
        }
        
        // 注意：这里不需要手动回收 enemies 列表，SystemBase 已通过 ECSManager 跨系统处理
    }

    /// <summary>
    /// 坐标转网格 Key
    /// </summary>
    public Vector2Int GetKey(float x, float y) 
    {
        return new Vector2Int(Mathf.FloorToInt(x / CellSize), Mathf.FloorToInt(y / CellSize));
    }

    /// <summary>
    /// 获取指定坐标附近的实体列表
    /// 优化：从 ECSManager.GetListFromPool 借用临时列表，实现查询 0 GC
    /// </summary>
    /// <param name="x">中心点 X</param>
    /// <param name="y">中心点 Y</param>
    /// <param name="radius">搜索深度：1表示周围 3x3 个格，2表示 5x5 个格</param>
    /// <returns>返回一个临时的实体列表，该列表会在帧末自动回收</returns>
    public List<Entity> GetNearbyEnemies(float x, float y, int radius = 1)
    {
        // 从全局池中借用列表，不要 new
        List<Entity> nearby = ECSManager.Instance.GetListFromPool(); 
        Vector2Int center = GetKey(x, y);
        
        for (int i = -radius; i <= radius; i++) 
        {
            for (int j = -radius; j <= radius; j++) 
            {
                Vector2Int targetKey = center + new Vector2Int(i, j);
                if (Grid.TryGetValue(targetKey, out var list)) 
                {
                    nearby.AddRange(list);
                }
            }
        }
        return nearby;
    }
}