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
            var prep = e.GetComponent<ShootPrepStateComponent>();
            
            prep.Timer -= deltaTime;
            
            // 蓄力期间强行停止物理移动 (对照 DashPrepSystem 的做法)
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
                
                // 【对接现有系统】：下发标准的开火意图，WeaponFiringSystem 会在这一帧接管它！
                e.AddComponent(new FireIntentComponent(prep.TargetDir)); 
            }
        }
        ReturnListToPool(preps);
    }
}