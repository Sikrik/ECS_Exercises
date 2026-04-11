using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动器 (高内聚管线最终重构版)
/// 职责：严格按照 逻辑(数据计算) -> 生命周期(回收) -> 表现(渲染同步) 的顺序执行。
/// 彻底保证 Simulation 组不再碰触 Unity 的 Transform、Camera 或 Instantiate。
/// </summary>
public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    
    // 全局共享的网格空间划分系统
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        Grid = new GridSystem(2f, entities);

        // ========================================================
        // 1. 初始化组 (Initialization) - 准备本帧基础数据和输入意图
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    
        initGroup.AddSystem(new StatusGatherSystem(entities));    
        _systemGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation) - 【纯净数据流区】无表现层代码
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        simGroup.AddSystem(new PhysicsBakingSystem(entities));    
        
        simGroup.AddSystem(new PlayerControlSystem(entities));    
        simGroup.AddSystem(new DashSystem(entities));             
        
        // --- 射击系统拆分 ---
        simGroup.AddSystem(new WeaponCooldownSystem(entities));   
        simGroup.AddSystem(new PlayerAimingSystem(entities));     
        simGroup.AddSystem(new WeaponFiringSystem(entities, Grid)); 
        
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    
        
        // --- 空间与物理 ---
        simGroup.AddSystem(Grid);                                 
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        
        // --- 战斗结算 ---
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); 
        simGroup.AddSystem(new DamageSystem(entities));           
        simGroup.AddSystem(new BulletEffectSystem(entities));     
        
        // --- 状态与反应 ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); 
        simGroup.AddSystem(new PlayerHitReactionSystem(entities)); 
        simGroup.AddSystem(new HitRecoverySystem(entities));      
        
        simGroup.AddSystem(new HealthSystem(entities));           
        simGroup.AddSystem(new BountySystem(entities));           
        simGroup.AddSystem(new PlayerDeathSystem(entities));      
        simGroup.AddSystem(new DeathCleanupSystem(entities));     
        
        simGroup.AddSystem(new ScoreSystem(entities));            
        simGroup.AddSystem(new SlowEffectSystem(entities));       
        
        // --- 生命周期清理 (帧末执行) ---
        simGroup.AddSystem(new LifetimeSystem(entities));         
        simGroup.AddSystem(new EventCleanupSystem(entities));     
        simGroup.AddSystem(new EntityCleanupSystem(entities));    
        
        _systemGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation) - 【视觉渲染与画面表现区】
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        presGroup.AddSystem(new VisualBakingSystem(entities));    
        presGroup.AddSystem(new CameraCullingSystem(entities));   
        presGroup.AddSystem(new CameraFollowSystem(entities));

  
        presGroup.AddSystem(new GhostTrailSystem(entities));      
        presGroup.AddSystem(new ViewSyncSystem(entities));        
        presGroup.AddSystem(new RenderSyncSystem(entities));      
        presGroup.AddSystem(new VFXSystem(entities));             
        presGroup.AddSystem(new VFXInstantiationSystem(entities));// 接管所有特效的生成
        presGroup.AddSystem(new InvincibleVisualSystem(entities)); 
        presGroup.AddSystem(new LightningRenderSystem(entities)); 
        presGroup.AddSystem(new UISyncSystem(entities));          
        
        _systemGroups.Add(presGroup);

        Debug.Log("<color=cyan>[SystemBootstrap]</color> ECS 启动完毕。高内聚分离管线已装载。");
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }
}