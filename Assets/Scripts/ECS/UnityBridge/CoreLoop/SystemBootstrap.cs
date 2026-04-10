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

        // 2. 物理与反馈
        systems.Add(new PhysicsDetectionSystem(entities)); 
        systems.Add(new KnockbackSystem(entities)); 

        // 3. AI 控制
        systems.Add(new EnemyTrackingSystem(entities));   
        systems.Add(new PlayerControlSystem(entities));

        // 4. 战斗核心逻辑
        systems.Add(new EnemySpawnSystem(entities));
        systems.Add(new PlayerShootingSystem(entities, grid));
        systems.Add(new DamageSystem(entities));
        systems.Add(new BulletEffectSystem(entities));
        
        // 【补全漏掉的系统】：得分与受击反应
        systems.Add(new EnemyHitReactionSystem(entities));  // 让怪物受到非击退攻击时也能产生硬直
        systems.Add(new PlayerHitReactionSystem(entities)); // 让玩家受击能触发无敌时间
        systems.Add(new ScoreSystem(entities));             // 让 UI 上的分数能正常增加

        // 5. 坐标执行与视觉表现
        systems.Add(new PhysicsBakingSystem(entities));
        systems.Add(new MovementSystem(entities)); 
        systems.Add(new ViewSyncSystem(entities));
        
        // 【核心修复】：补全视觉特效系统
        systems.Add(new VFXSystem(entities));               // 同步减速冰冻等附加特效的坐标
        systems.Add(new LightningRenderSystem(entities));   // 绘制闪电链并处理淡出表现
        systems.Add(new InvincibleVisualSystem(entities));  // 玩家无敌时的半透明闪烁表现
        
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