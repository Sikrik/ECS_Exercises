using System.Collections.Generic;
using UnityEngine;

public class GridSystem : SystemBase
{
    public float CellSize;
    public Dictionary<Vector2Int, List<Entity>> Grid = new Dictionary<Vector2Int, List<Entity>>();
    
    // 👇 新增：内部 List 对象池，用于复用网格分配的列表
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    public GridSystem(float cellSize, List<Entity> entities) : base(entities)
    {
        CellSize = cellSize;
    }

    public override void Update(float deltaTime)
    {
        // 1. 回收上一帧产生的所有 List 到池子里
        foreach (var list in Grid.Values)
        {
            list.Clear();
            _listPool.Push(list);
        }
        Grid.Clear();

        // 2. 重新填充本帧的网格
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            
            var pos = e.GetComponent<PositionComponent>();
            var key = GetKey(pos.X, pos.Y);
            
            if (!Grid.ContainsKey(key)) 
            {
                // 从池子里拿，只有当池子空了才 fallback 到 new
                Grid[key] = _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
            }
            Grid[key].Add(e);
        }
    }

    public Vector2Int GetKey(float x, float y) => new Vector2Int(Mathf.FloorToInt(x / CellSize), Mathf.FloorToInt(y / CellSize));

    /// <summary>
    /// 获取指定坐标附近的敌人
    /// </summary>
    /// <param name="radius">搜索范围深度。1表示3x3格子，2表示5x5格子，以此类推</param>
    public List<Entity> GetNearbyEnemies(float x, float y, int radius = 1)
    {
        List<Entity> nearby = new List<Entity>();
        Vector2Int center = GetKey(x, y);
        
        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                if (Grid.TryGetValue(center + new Vector2Int(i, j), out var list)) 
                    nearby.AddRange(list);
            }
        }
        return nearby;
    }
}