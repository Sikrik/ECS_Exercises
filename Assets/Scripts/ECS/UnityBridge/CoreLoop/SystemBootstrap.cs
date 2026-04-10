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
        
        // 【修复 Bug 1】：特效系统必须在伤害系统之前！
        // 让子弹先爆开 AOE 并生成 VFX，然后再由 DamageSystem 结算单体伤害并销毁子弹。
        systems.Add(new BulletEffectSystem(entities));
        systems.Add(new DamageSystem(entities));
        
        systems.Add(new EnemyHitReactionSystem(entities));  
        systems.Add(new PlayerHitReactionSystem(entities)); 

        // 5. 坐标执行与视觉表现
        systems.Add(new PhysicsBakingSystem(entities));
        systems.Add(new MovementSystem(entities)); 
        systems.Add(new ViewSyncSystem(entities));
        systems.Add(new VFXSystem(entities));               
        systems.Add(new LightningRenderSystem(entities));   
        systems.Add(new InvincibleVisualSystem(entities));  
        
        // 6. 计时与生命周期结算
        systems.Add(new HealthSystem(entities));
        
        // 【修复 Bug 2】：记分系统必须放在 HealthSystem（判断死亡并发出事件）之后，清理系统之前！
        // 这样本帧怪物死亡产生的事件，本帧立刻就能变成玩家的分数。
        systems.Add(new ScoreSystem(entities));             

        systems.Add(new HitRecoverySystem(entities)); 
        systems.Add(new SlowEffectSystem(entities));
        systems.Add(new LifetimeSystem(entities));

        // 7. 清理
        systems.Add(new EventCleanupSystem(entities));
        systems.Add(new EntityCleanupSystem(entities));

        return systems;
    }
}