using System.Collections.Generic;

public static class SystemBootstrap
{
    /// <summary>
    /// 创建并返回游戏默认的系统流水线
    /// </summary>
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        // 1. 初始化网格系统 (由于其他系统依赖它，所以单独传出)
        grid = new GridSystem(2.0f, entities);
        systems.Add(grid);

        // --- 2. 感知与意图层 ---
        systems.Add(new InputCaptureSystem(entities));    // 捕捉输入
        systems.Add(new EnemyTrackingSystem(entities));   // 怪物 AI 决策

        // --- 3. 状态控制层 ---
        systems.Add(new PlayerControlSystem(entities));   // 意图转速度
        systems.Add(new StateTimerSystem(entities));      // 各种倒计时

        // --- 4. 生产与物理层 ---
        systems.Add(new EnemySpawnSystem(entities));
        systems.Add(new PlayerShootingSystem(entities, grid));
        systems.Add(new PhysicsBakingSystem(entities)); 
        systems.Add(new MovementSystem(entities));
        systems.Add(new ViewSyncSystem(entities));

        // --- 5. 碰撞响应流水线 ---
        systems.Add(new PhysicsDetectionSystem(entities));
        systems.Add(new DamageSystem(entities));
        systems.Add(new KnockbackSystem(entities));
        systems.Add(new BulletEffectSystem(entities));

        // --- 6. 状态维持与表现 ---
        systems.Add(new SlowEffectSystem(entities));
        systems.Add(new HealthSystem(entities));
        systems.Add(new LifetimeSystem(entities));
        systems.Add(new LightningRenderSystem(entities));
        systems.Add(new VFXSystem(entities));
        systems.Add(new InvincibleVisualSystem(entities));

        // --- 7. 清理 ---
        systems.Add(new EventCleanupSystem(entities));

        return systems;
    }
}