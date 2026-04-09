using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家射击系统：负责根据当前选择的子弹类型进行开火逻辑处理
/// 重构点：统一使用 DamageComponent 存储破坏力，特殊效果组件仅存储范围参数
/// </summary>
public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    public static BulletType CurrentBulletType = BulletType.Normal;
    
    public PlayerShootingSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        
        // 1. 获取当前子弹类型的配置数据
        string bulletId = CurrentBulletType.ToString();
        if (!config.BulletRecipes.TryGetValue(bulletId, out var bulletData)) 
        {
            Debug.LogError($"未找到子弹配置: {bulletId}");
            return;
        }

        _shootTimer += deltaTime;
        
        // 2. 检查射击间隔
        if (_shootTimer >= bulletData.ShootInterval)
        {
            if (Shoot(bulletData)) 
            {
                _shootTimer = 0;
            }
        }
    }

    // 在 PlayerShootingSystem.cs 中修改 Shoot 方法
    private bool Shoot(BulletData recipe)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return false;

        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearestInGrid(pPos.X, pPos.Y, 3); 
        if (target == null) return false;

        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;

        // --- 使用工厂一键创建 ---
        BulletFactory.Create(CurrentBulletType, new Vector3(pPos.X, pPos.Y, 0), dir);

        return true;
    }

    private Entity FindNearestInGrid(float x, float y, int radius)
    {
        if (_grid == null) return null;
        var enemies = _grid.GetNearbyEnemies(x, y, radius);
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