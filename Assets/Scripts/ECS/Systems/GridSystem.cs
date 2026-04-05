using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 空间网格系统：负责维护实体的空间索引，优化近邻查询性能
/// 重构要点：使用 EnemyTag 筛选实体，保持空间管理逻辑的纯粹性
/// </summary>
public class GridSystem : SystemBase
{
    public float CellSize;
    // 网格容器：Key 为网格坐标，Value 为该格内的实体列表
    public Dictionary<Vector2Int, List<Entity>> Grid = new Dictionary<Vector2Int, List<Entity>>();

    private float _cellSize;
    public GridSystem(float cellSize, List<Entity> entities) : base(entities)
    {
        _cellSize = cellSize;
    }

    public GridSystem(float entities) : base(BASE)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 每帧清空并重新填充网格，确保空间索引是最新的
    /// </summary>
    public override void Update(float deltaTime)
    {
        Grid.Clear();
        
        // 1. 核心重构点：筛选所有 拥有 EnemyTag 和 PositionComponent 的实体
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        
        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            
            var pos = e.GetComponent<PositionComponent>();
            var key = GetKey(pos.X, pos.Y);
            
            if (!Grid.ContainsKey(key))
            {
                // 这里建议使用对象池获取 List，进一步优化 GC（如果你有 ListPool 的话）
                Grid[key] = new List<Entity>();
            }
            Grid[key].Add(e);
        }
    }

    /// <summary>
    /// 根据世界坐标计算网格坐标
    /// </summary>
    public Vector2Int GetKey(float x, float y) => new Vector2Int(Mathf.FloorToInt(x / CellSize), Mathf.FloorToInt(y / CellSize));

    /// <summary>
    /// 获取指定坐标周围 3x3 范围内的所有敌人
    /// </summary>
    public List<Entity> GetNearbyEnemies(float x, float y)
    {
        List<Entity> nearby = new List<Entity>();
        Vector2Int center = GetKey(x, y);

        // 遍历九宫格
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Grid.TryGetValue(center + new Vector2Int(i, j), out var list))
                {
                    nearby.AddRange(list);
                }
            }
        }
        return nearby;
    }
}