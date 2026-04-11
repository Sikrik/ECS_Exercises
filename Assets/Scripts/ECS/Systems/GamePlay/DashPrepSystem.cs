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
            var prep = e.GetComponent<DashPrepStateComponent>();
            
            prep.Timer -= deltaTime;
            
            // 蓄力期间强行停止移动（在 MovementSystem 中也会通过判断此组件停下）
            if (e.HasComponent<VelocityComponent>())
            {
                var vel = e.GetComponent<VelocityComponent>();
                vel.VX = 0; vel.VY = 0;
            }

            if (prep.Timer <= 0)
            {
                // 【修复2】：在正式触发冲刺前，把蓄力时锁定的方向塞回 MoveInputComponent
                // 防止 DashActivationSystem 读到输入为 (0,0) 后默认向右滑步
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