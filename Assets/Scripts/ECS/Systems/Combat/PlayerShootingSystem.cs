// ==========================================
// 2. 重构后的玩家射击系统
// ==========================================

using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    
    public static BulletType CurrentBulletType = BulletType.Normal;
    // 【核心新增】：当前正在使用的瞄准策略（默认自动索敌）
    public static IAimStrategy CurrentAimStrategy = new AutoAimStrategy(); 
    
    public PlayerShootingSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        
        string bulletId = CurrentBulletType.ToString();
        if (!config.BulletRecipes.TryGetValue(bulletId, out var bulletData)) return;

        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        _shootTimer += deltaTime;
        
        // 检查冷却时间
        if (_shootTimer >= bulletData.ShootInterval)
        {
            var shootInput = player.GetComponent<ShootInputComponent>();
            var pPos = player.GetComponent<PositionComponent>();

            // 【策略模式调用】：问当前的策略，这帧要不要开火？往哪开？
            Vector2? aimDir = CurrentAimStrategy.GetAimDirection(player, shootInput, _grid);

            // 如果策略返回了方向，说明满足了开火条件
            if (aimDir.HasValue)
            {
                BulletFactory.Create(CurrentBulletType, new Vector3(pPos.X, pPos.Y, 0), aimDir.Value);
                _shootTimer = 0; // 重置冷却
            }
        }
    }
}