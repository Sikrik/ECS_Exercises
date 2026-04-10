using System.Collections.Generic;

public static class SystemBootstrap
{
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        grid = new GridSystem(2.0f, entities);
        
        // ==========================================
        // 阶段 1：数据采集与意图生成 (Inputs)
        // ==========================================
        systems.Add(new InputCaptureSystem(entities));
        systems.Add(new StatusGatherSystem(entities)); 
        systems.Add(new EnemyTrackingSystem(entities)); // 玩家和AI在此刻均输出 MoveInput 
        
        // 【注意】：因为 MovementSystem 统一接管了 MoveInput，
        // 这里我们可以自豪地删掉 PlayerControlSystem 了！架构解耦成功！

        // ==========================================
        // 阶段 2：生成与开火 (Spawning)
        // ==========================================
        systems.Add(new EnemySpawnSystem(entities));
        systems.Add(new PlayerShootingSystem(entities, grid));

        // ==========================================
        // 阶段 3：坐标位移与空间刷新 (Movement & Spatial)
        // ==========================================
        systems.Add(new PhysicsBakingSystem(entities));
        systems.Add(new MovementSystem(entities)); // 执行所有的意图和物理滑动，坐标在此处改变
        
        // 【核心修复】：将 GridSystem 移到 Movement 之后！
        // 这样它录入的就是绝对新鲜的本帧坐标，彻底解决过时脏数据导致索敌/碰撞失败的问题
        systems.Add(grid); 

        // ==========================================
        // 阶段 4：精准的碰撞与战斗结算 (Combat & Reactions)
        // ==========================================
        systems.Add(new PhysicsDetectionSystem(entities)); // 读取最新网格，进行精准碰撞
        systems.Add(new KnockbackSystem(entities));        // 瞬间施加反弹力与状态标签
        systems.Add(new BulletEffectSystem(entities));     // 子弹爆炸 VFX
        systems.Add(new DamageSystem(entities));           // 扣血
        systems.Add(new EnemyHitReactionSystem(entities)); // 死亡判定与硬直分流
        systems.Add(new PlayerHitReactionSystem(entities)); 

        // ==========================================
        // 阶段 5：画面同步 (Visuals)
        // ==========================================
        systems.Add(new ViewSyncSystem(entities));
        systems.Add(new VFXSystem(entities));               
        systems.Add(new LightningRenderSystem(entities));   
        systems.Add(new InvincibleVisualSystem(entities));  
        
        // ==========================================
        // 阶段 6：死亡结算与清理 (Cleanup)
        // ==========================================
        systems.Add(new HealthSystem(entities));            // 如果死了，发出死亡事件
        systems.Add(new ScoreSystem(entities));             // 接收事件，增加分数
        systems.Add(new HitRecoverySystem(entities)); 
        systems.Add(new SlowEffectSystem(entities));
        systems.Add(new LifetimeSystem(entities));
        systems.Add(new EventCleanupSystem(entities));      // 在管线最末端统一清理本帧事件
        systems.Add(new EntityCleanupSystem(entities));

        return systems;
    }
}