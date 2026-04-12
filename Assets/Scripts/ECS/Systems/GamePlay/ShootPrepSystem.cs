// 路径: Assets/Scripts/ECS/Systems/GamePlay/ShootPrepSystem.cs
using System.Collections.Generic;

public class ShootPrepSystem : SystemBase
{
    public ShootPrepSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var preps = GetEntitiesWith<ShootPrepStateComponent>();
        
        for (int i = preps.Count - 1; i >= 0; i--)
        {
            var e = preps[i];

            // 打断逻辑：如果死亡、受到硬直或被击退，中断射击蓄力
            if (e.HasComponent<DeadTag>() || e.HasComponent<HitRecoveryComponent>() || e.HasComponent<KnockbackComponent>())
            {
                e.RemoveComponent<ShootPrepStateComponent>();
                e.RemoveComponent<DashPreviewIntentComponent>();
                continue;
            }

            var prep = e.GetComponent<ShootPrepStateComponent>();
            prep.Timer -= deltaTime;
            
            // 👇 【核心修复】：蓄力期间强行清空移动意图和物理速度，焊死在原地
            if (e.HasComponent<MoveInputComponent>())
            {
                var input = e.GetComponent<MoveInputComponent>();
                input.X = 0; input.Y = 0;
            }
            
            if (e.HasComponent<VelocityComponent>())
            {
                var vel = e.GetComponent<VelocityComponent>();
                vel.VX = 0; vel.VY = 0;
            }

            if (prep.Timer <= 0)
            {
                // 蓄力结束，移除蓄力和红外线预览
                e.RemoveComponent<ShootPrepStateComponent>();
                e.RemoveComponent<DashPreviewIntentComponent>();
                
                // 下发标准的开火意图
                e.AddComponent(new FireIntentComponent(prep.TargetDir)); 
            }
        }
    }
}