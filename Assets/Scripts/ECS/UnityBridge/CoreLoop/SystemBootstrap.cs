using System.Collections.Generic;

public static class SystemBootstrap
{
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        // 1. 基础与环境
        grid = new GridSystem(2.0f, entities);
        systems.Add(grid);
        systems.Add(new InputCaptureSystem(entities));
        
        // 状态汇总（初步确定本帧基准速度）
        systems.Add(new StatusGatherSystem(entities)); 

        // 2. 物理与事件流（产生状态标签的关键）
        systems.Add(new PhysicsDetectionSystem(entities)); 
        systems.Add(new KnockbackSystem(entities)); // 这里决定了谁进 Knockback，谁进 HitRecovery

        // 3. 决策流（尊重状态标签）
        // 因为已经在上面 KnockbackSystem 里挂了组件，所以这里的 Tracking 会自动跳过受击者
        systems.Add(new EnemyTrackingSystem(entities));   
        systems.Add(new PlayerControlSystem(entities));

        // 4. 战斗流
        systems.Add(new EnemySpawnSystem(entities));
        systems.Add(new PlayerShootingSystem(entities, grid));
        systems.Add(new DamageSystem(entities));
        
        // 5. 最终物理执行
        systems.Add(new PhysicsBakingSystem(entities));
        systems.Add(new MovementSystem(entities)); // 最终应用速度到坐标
        systems.Add(new ViewSyncSystem(entities)); // 同步到 Transform
        
        // 6. 状态维护与计时
        systems.Add(new HealthSystem(entities));
        systems.Add(new HitRecoverySystem(entities)); // 处理硬直计时器的倒计时
        systems.Add(new SlowEffectSystem(entities));
        systems.Add(new LifetimeSystem(entities));
        systems.Add(new VFXSystem(entities));

        // 7. 清理
        systems.Add(new EventCleanupSystem(entities));
        systems.Add(new EntityCleanupSystem(entities));

        return systems;
    }
}