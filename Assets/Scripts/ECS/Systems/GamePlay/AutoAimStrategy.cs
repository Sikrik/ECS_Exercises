using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 1. 策略模式接口定义
// ==========================================
public interface IAimStrategy
{
    /// <summary>
    /// 获取开火方向。如果返回 null，代表当前不满足开火条件。
    /// </summary>
    Vector2? GetAimDirection(Entity player, ShootInputComponent input, GridSystem grid);
}

// 策略A：自动索敌 (只要附近有敌人，无视玩家是否按鼠标，自动开火)
public class AutoAimStrategy : IAimStrategy
{
    public Vector2? GetAimDirection(Entity player, ShootInputComponent input, GridSystem grid)
    {
        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearestInGrid(pPos.X, pPos.Y, 3, grid); // 半径为3
        
        // 没找到敌人，不开火
        if (target == null) return null; 

        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y);
        return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.up;
    }

    private Entity FindNearestInGrid(float x, float y, int radius, GridSystem grid)
    {
        if (grid == null) return null;
        var enemies = grid.GetNearbyEntities(x, y, radius);
        Entity nearest = null;
        float minDistSq = float.MaxValue;
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
            var ePos = e.GetComponent<PositionComponent>();
            float d2 = (ePos.X - x) * (ePos.X - x) + (ePos.Y - y) * (ePos.Y - y);
            if (d2 < minDistSq) { minDistSq = d2; nearest = e; }
        }
        return nearest;
    }
}

// 策略B：鼠标手动瞄准 (只有当玩家按住左键时，才朝鼠标方向开火)
public class ManualAimStrategy : IAimStrategy
{
    public Vector2? GetAimDirection(Entity player, ShootInputComponent input, GridSystem grid)
    {
        // 没按左键，不开火
        if (input == null || !input.IsShooting) return null; 

        var pPos = player.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(input.TargetX - pPos.X, input.TargetY - pPos.Y);
        
        return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.up;
    }
}