using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 系统引导器：负责初始化所有系统并严格定义它们的执行顺序（Pipeline）
/// </summary>
public static class SystemBootstrap
{
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        grid = new GridSystem(2.0f, entities);

        // ========================================================
        // 阶段 1：采集层 (Inputs & Intentions)
        // ========================================================
        systems.Add(new InputCaptureSystem(entities));    
        systems.Add(new StatusGatherSystem(entities));    
        systems.Add(new EnemyTrackingSystem(entities));  

        // ========================================================
        // 阶段 2：生产层 (Spawning & Logistics)
        // ========================================================
        systems.Add(new EnemySpawnSystem(entities));      
        systems.Add(new PlayerShootingSystem(entities, grid)); 

        // ========================================================
        // 阶段 3：模拟层 (Simulation & Motion)
        // ========================================================
        systems.Add(new PhysicsBakingSystem(entities));   
        systems.Add(new MovementSystem(entities));        
        systems.Add(grid); // 必须在 Movement 之后，Physics 之前

        // ========================================================
        // 阶段 4：结算层 (Combat & Physics Reactions) - 【核心重构区】
        // ========================================================
        systems.Add(new PhysicsDetectionSystem(entities)); // 1. 产出物理碰撞事件
        
        // 👇 【全新枢纽】翻译碰撞意图，决定谁该被击退，谁该罚站硬直
        systems.Add(new ImpactResolutionSystem(entities)); 
        
        // 👇 退化为纯物理马达，只负责击退的滑行刹车，以及怪物肉体互挤的虫群流动
        systems.Add(new KnockbackSystem(entities));        

        systems.Add(new BulletEffectSystem(entities));     // 子弹特效/AOE展开
        
        // 👇 退化为纯数值计算，只管扣血和销毁子弹，不管任何物理表现
        systems.Add(new DamageSystem(entities));           
        
        // 注：EnemyHitReactionSystem 已被 ImpactResolutionSystem 完美替代，彻底删除！
        
        systems.Add(new PlayerHitReactionSystem(entities)); // 玩家受击无敌帧

        // ========================================================
        // 阶段 5：视觉表现层 (Visuals & Rendering)
        // ========================================================
        systems.Add(new ViewSyncSystem(entities));        
        systems.Add(new VFXSystem(entities));             
        systems.Add(new LightningRenderSystem(entities)); 
        systems.Add(new InvincibleVisualSystem(entities)); 

        // ========================================================
        // 阶段 6：生命周期层 (Post-Processing & Cleanup)
        // ========================================================
        systems.Add(new HealthSystem(entities));          
        systems.Add(new ScoreSystem(entities));           
        
        systems.Add(new HitRecoverySystem(entities));     
        systems.Add(new SlowEffectSystem(entities));      
        systems.Add(new LifetimeSystem(entities));        

        systems.Add(new EventCleanupSystem(entities));    
        systems.Add(new EntityCleanupSystem(entities));   

        return systems;
    }
}