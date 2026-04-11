using System.Collections.Generic;

public class DashPrepSystem : SystemBase
{
    public DashPrepSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var preps = GetEntitiesWith<DashPrepStateComponent>();
        
        for (int i = preps.Count - 1; i >= 0; i--)
        {
            var e = preps[i];

            // ==========================================
            // 【新增】打断逻辑：如果死亡、受到硬直或被击退，中断蓄力
            // ==========================================
            if (e.HasComponent<DeadTag>() || e.HasComponent<HitRecoveryComponent>() || e.HasComponent<KnockbackComponent>())
            {
                e.RemoveComponent<DashPrepStateComponent>();
                e.RemoveComponent<DashPreviewIntentComponent>();
                // 被打断了，直接跳过本帧，且不再赋予 DashInputComponent
                continue;
            }

            var prep = e.GetComponent<DashPrepStateComponent>();
            prep.Timer -= deltaTime;
            
            // 蓄力期间强行停止移动
            if (e.HasComponent<VelocityComponent>())
            {
                var vel = e.GetComponent<VelocityComponent>();
                vel.VX = 0; vel.VY = 0;
            }

            if (prep.Timer <= 0)
            {
                if (e.HasComponent<MoveInputComponent>())
                {
                    var move = e.GetComponent<MoveInputComponent>();
                    move.X = prep.TargetDir.x;
                    move.Y = prep.TargetDir.y;
                }

                // 蓄力结束，移除蓄力和预览，下达正式冲刺指令
                e.RemoveComponent<DashPrepStateComponent>();
                e.RemoveComponent<DashPreviewIntentComponent>();
                e.AddComponent(new DashInputComponent()); 
            }
        }
    }
}