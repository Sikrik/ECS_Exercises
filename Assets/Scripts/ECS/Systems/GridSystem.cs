using System.Collections.Generic;
using UnityEngine;

public class GridSystem : SystemBase
{
    public float CellSize;
    public Dictionary<Vector2Int, List<Entity>> Grid = new Dictionary<Vector2Int, List<Entity>>();

    public GridSystem(float cellSize, List<Entity> entities) : base(entities)
    {
        CellSize = cellSize; // 修正：直接赋值给公有的 CellSize 字段
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

    public List<Entity> GetNearbyEnemies(float x, float y)
    {
        List<Entity> nearby = new List<Entity>();
        Vector2Int center = GetKey(x, y);
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (Grid.TryGetValue(center + new Vector2Int(i, j), out var list)) nearby.AddRange(list);
            }
        }
        return nearby;
    }
}