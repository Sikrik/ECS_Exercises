using System.Collections.Generic;
using UnityEngine;

public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        Grid = new GridSystem(2f, entities);

        // ==========================================
        // 1. 初始化组 (数据捕捉)
        // ==========================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    
        initGroup.AddSystem(new StatusGatherSystem(entities));    
        _systemGroups.Add(initGroup);

        // ==========================================
        // 2. 模拟组 (逻辑结算)
        // ==========================================
        var simGroup = new SimulationSystemGroup(entities);
        simGroup.AddSystem(new PhysicsBakingSystem(entities));    
        simGroup.AddSystem(new PlayerControlSystem(entities));    
        simGroup.AddSystem(new WeaponCooldownSystem(entities));   
        simGroup.AddSystem(new PlayerAimingSystem(entities));     
        simGroup.AddSystem(new WeaponFiringSystem(entities, Grid)); 
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    
        simGroup.AddSystem(new ChargerAISystem(entities));        
        simGroup.AddSystem(new DashPrepSystem(entities));         
        simGroup.AddSystem(new DashCooldownSystem(entities)); 
        simGroup.AddSystem(new DashActivationSystem(entities));             
        simGroup.AddSystem(new DashStateSystem(entities));         
        simGroup.AddSystem(Grid);                                 
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); 
        simGroup.AddSystem(new BulletDestroySystem(entities));
        simGroup.AddSystem(new SlowBulletReactionSystem(entities));
        simGroup.AddSystem(new AOEBulletReactionSystem(entities));
        simGroup.AddSystem(new ChainLightningReactionSystem(entities));
        simGroup.AddSystem(new DamageSystem(entities));           
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); 
        simGroup.AddSystem(new PlayerHitReactionSystem(entities)); 
        simGroup.AddSystem(new HitRecoverySystem(entities));      
        simGroup.AddSystem(new HealthSystem(entities));           
        simGroup.AddSystem(new BountySystem(entities));           
        simGroup.AddSystem(new PlayerDeathSystem(entities));      
        simGroup.AddSystem(new DeathCleanupSystem(entities));     
        simGroup.AddSystem(new ScoreSystem(entities));            
        simGroup.AddSystem(new SlowEffectSystem(entities));       
        simGroup.AddSystem(new ExplosionSystem(entities)); 
        simGroup.AddSystem(new LifetimeSystem(entities));         
        simGroup.AddSystem(new EventCleanupSystem(entities));     
        simGroup.AddSystem(new EntityCleanupSystem(entities));    
        simGroup.AddSystem(new RangedAISystem(entities)); // 👇 新增：注册远程怪 AI 系统
        _systemGroups.Add(simGroup);

        // ==========================================
        // 3. 表现组 (渲染同步)
        // ==========================================
        var presGroup = new PresentationSystemGroup(entities);
        presGroup.AddSystem(new VFXInstantiationSystem(entities)); 
        presGroup.AddSystem(new VisualBakingSystem(entities));    
        
        presGroup.AddSystem(new CameraCullingSystem(entities));   
        presGroup.AddSystem(new CameraFollowSystem(entities));
        presGroup.AddSystem(new GhostTrailSystem(entities));      
        presGroup.AddSystem(new ViewSyncSystem(entities));        
        
        // 👇 新增通用方向指示器系统 (必须在 ViewSync 位置同步之后执行)
        presGroup.AddSystem(new DirectionIndicatorSystem(entities)); 

        // --- 渲染与颜色覆写 ---
        presGroup.AddSystem(new RenderSyncSystem(entities));        
        presGroup.AddSystem(new HitFeedbackVisualSystem(entities)); 
        presGroup.AddSystem(new InvincibleVisualSystem(entities));  
        
        presGroup.AddSystem(new VFXSystem(entities));             
        presGroup.AddSystem(new VFXCleanupSystem(entities));
        presGroup.AddSystem(new LightningRenderSystem(entities)); 
        presGroup.AddSystem(new UISyncSystem(entities));          
        _systemGroups.Add(presGroup);
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }
}