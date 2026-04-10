using System.Collections.Generic;
using UnityEngine;

public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 兼容写法：获取第一个玩家
        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count == 0) return;
        var pPos = players[0].GetComponent<PositionComponent>();

        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, SpeedComponent>();
        
        foreach (var enemy in enemies)
        {
            // 状态保护：如果正在滑动或硬直，跳过寻路覆盖速度
            if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>())
            {
                continue; 
            }

            var ePos = enemy.GetComponent<PositionComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();
            var speed = enemy.GetComponent<SpeedComponent>();

            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > 0.1f)
            {
                vel.VX = (dx / dist) * speed.CurrentSpeed;
                vel.VY = (dy / dist) * speed.CurrentSpeed;
            }
            else
            {
                vel.VX = 0; vel.VY = 0;
            }
        }
    }
}