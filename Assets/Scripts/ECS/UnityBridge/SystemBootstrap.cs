using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动器
/// 职责：初始化所有系统组，并严格按照 逻辑 -> 表现 的顺序进行每帧更新
/// </summary>
public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        Grid = new GridSystem(2f, entities);

        // ========================================================
        // 1. 初始化组 (Initialization)
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    
        initGroup.AddSystem(new StatusGatherSystem(entities));    
        _systemGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation) 
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        simGroup.AddSystem(new PhysicsBakingSystem(entities));
        
        simGroup.AddSystem(new PlayerControlSystem(entities));    
        simGroup.AddSystem(new DashSystem(entities));             
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    
        simGroup.AddSystem(new PlayerShootingSystem(entities, Grid)); 
        
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         
        simGroup.AddSystem(Grid);             
        
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        simGroup.AddSystem(new DamageSystem(entities));           
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); 
        simGroup.AddSystem(new BulletEffectSystem(entities));     
        
        // --- 第五步：状态反馈与状态机 ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities));
        simGroup.AddSystem(new PlayerHitReactionSystem(entities));
        simGroup.AddSystem(new HitRecoverySystem(entities));      
        simGroup.AddSystem(new HealthSystem(entities));  
        
        // 【核心修复】：将 ScoreSystem 移到这里！
        // 紧跟在 HealthSystem 判定死亡之后立即结算分数，赶在实体被销毁之前！
        simGroup.AddSystem(new ScoreSystem(entities));    
                 
        simGroup.AddSystem(new SlowEffectSystem(entities));       
        
        // --- 第六步：生命周期管理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         
        simGroup.AddSystem(new EventCleanupSystem(entities));     
        simGroup.AddSystem(new EntityCleanupSystem(entities));    // 这里的“死神”会销毁实体
        
        _systemGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation)
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        
        presGroup.AddSystem(new CameraCullingSystem(entities));   
        presGroup.AddSystem(new GhostTrailSystem(entities));      
        presGroup.AddSystem(new ViewSyncSystem(entities));        
        presGroup.AddSystem(new RenderSyncSystem(entities));      
        presGroup.AddSystem(new VFXSystem(entities));             
        presGroup.AddSystem(new InvincibleVisualSystem(entities));
        presGroup.AddSystem(new LightningRenderSystem(entities)); 
        presGroup.AddSystem(new UISyncSystem(entities));          
        // 【注意】：删除了原本在这里的 presGroup.AddSystem(new ScoreSystem(entities)); 
        
        _systemGroups.Add(presGroup);

        Debug.Log("<color=green>[SystemBootstrap]</color> 所有 ECS 系统已完成初始化并注册。");
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }
}