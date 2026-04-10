using System.Collections.Generic;

public static class SystemBootstrap
{
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        grid = new GridSystem(2.0f, entities);
        systems.Add(grid);
        systems.Add(new InputCaptureSystem(entities));
        
        // 1. 状态汇总
        systems.Add(new StatusGatherSystem(entities)); 

        // 2. 物理与反馈 (产生 Knockback/HitRecovery 标记)
        systems.Add(new PhysicsDetectionSystem(entities)); 
        systems.Add(new KnockbackSystem(entities)); 

        // 3. AI 控制 (由于在 Knockback 之后，它能看到标记并停止更新速度)
        systems.Add(new EnemyTrackingSystem(entities));   
        systems.Add(new PlayerControlSystem(entities));

        // 4. 战斗
        systems.Add(new EnemySpawnSystem(entities));
        systems.Add(new PlayerShootingSystem(entities, grid));
        systems.Add(new DamageSystem(entities));
        systems.Add(new BulletEffectSystem(entities));

        // 5. 坐标执行
        systems.Add(new PhysicsBakingSystem(entities));
        systems.Add(new MovementSystem(entities)); 
        systems.Add(new ViewSyncSystem(entities));
        
        // 6. 计时管理
        systems.Add(new HealthSystem(entities));
        systems.Add(new HitRecoverySystem(entities)); 
        systems.Add(new SlowEffectSystem(entities));
        systems.Add(new LifetimeSystem(entities));

        // 7. 清理
        systems.Add(new EventCleanupSystem(entities));
        systems.Add(new EntityCleanupSystem(entities));

        return systems;
    }
}