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
        // 1. 回收列表... (保持不变)
        foreach (var list in Grid.Values)
        {
            list.Clear();
            _internalListPool.Push(list);
        }
        Grid.Clear();

        // 2. 【核心修改】扫描所有拥有 CollisionComponent 的实体！不再局限于 EnemyTag
        var colliders = GetEntitiesWith<CollisionComponent, PositionComponent>();
        foreach (var e in colliders)
        {
            if (!e.IsAlive) continue; // 只有活着的实体才进网格
        
            var pos = e.GetComponent<PositionComponent>();
            Vector2Int key = GetKey(pos.X, pos.Y);
        
            if (!Grid.ContainsKey(key)) 
            {
                Grid[key] = _internalListPool.Count > 0 ? _internalListPool.Pop() : new List<Entity>();
            }
            Grid[key].Add(e);
        }
    }

// 3. 【修改方法名】因为现在网格里什么都有了，改名叫 GetNearbyEntities
    public List<Entity> GetNearbyEntities(float x, float y, int radius = 1)
    {
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

    /// <summary>
    /// 坐标转网格 Key
    /// </summary>
    public Vector2Int GetKey(float x, float y) 
    {
        return new Vector2Int(Mathf.FloorToInt(x / CellSize), Mathf.FloorToInt(y / CellSize));
    }
}