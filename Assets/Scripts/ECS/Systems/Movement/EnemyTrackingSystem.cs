using System.Collections.Generic;
using UnityEngine;

public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 获取玩家位置作为目标
        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count == 0) return;
    
        var player = players[0]; // 获取第一个玩家实体
        if (player == null) return;
        var pPos = player.GetComponent<PositionComponent>();

        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, SpeedComponent>();
        
        foreach (var enemy in enemies)
        {
            // 【状态互斥核心】：如果正在被弹开滑动，或者正在硬直中，跳过寻路逻辑
            if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>())
            {
                continue; 
            }

            var ePos = enemy.GetComponent<PositionComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();
            var speed = enemy.GetComponent<SpeedComponent>();

            // 正常的朝向玩家寻路逻辑
            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > 0.1f)
            {
                // 使用 CurrentSpeed（这可能被 SlowEffectSystem 等修改过）
                vel.VX = (dx / dist) * speed.CurrentSpeed;
                vel.VY = (dy / dist) * speed.CurrentSpeed;
            }
            else
            {
                vel.VX = 0;
                vel.VY = 0;
            }
        }
    }
}