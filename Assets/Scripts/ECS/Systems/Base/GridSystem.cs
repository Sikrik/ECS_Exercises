using System.Collections.Generic;
using UnityEngine;

public class GridSystem : SystemBase
{
    public float CellSize;
    public Dictionary<Vector2Int, List<Entity>> Grid = new Dictionary<Vector2Int, List<Entity>>();

    public GridSystem(float cellSize, List<Entity> entities) : base(entities)
    {
        CellSize = cellSize;
    }

    public override void Update(float deltaTime)
    {
        Grid.Clear();
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            var pos = e.GetComponent<PositionComponent>();
            var key = GetKey(pos.X, pos.Y);
            if (!Grid.ContainsKey(key)) Grid[key] = new List<Entity>();
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
        
        // 优化：根据传入的 radius 动态扩大搜索范围
        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                if (Grid.TryGetValue(center + new Vector2Int(i, j), out var list)) 
                    nearby.AddRange(list);
            }
        }
        return nearby;
    }
}