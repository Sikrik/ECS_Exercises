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

        simGroup.AddSystem(new WeaponCooldownSystem(entities));   
        simGroup.AddSystem(new PlayerAimingSystem(entities));     
        simGroup.AddSystem(new WeaponFiringSystem(entities)); 
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    
        simGroup.AddSystem(new ChargerAISystem(entities));        
        simGroup.AddSystem(new RangedAISystem(entities));      
        simGroup.AddSystem(new DashPrepSystem(entities));         
        simGroup.AddSystem(new ShootPrepSystem(entities));     
        simGroup.AddSystem(new DashCooldownSystem(entities)); 
        simGroup.AddSystem(new DashActivationSystem(entities));             
        simGroup.AddSystem(new DashStateSystem(entities));         
        simGroup.AddSystem(Grid);                                 
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         
        // 在 SystemBootstrap 构造函数中：
        simGroup.AddSystem(new MeleeCombatSystem(entities)); // 放在 MovementSystem 附近
        simGroup.AddSystem(new ViewSyncSystem(entities));         
        
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
        
        presGroup.AddSystem(new DirectionIndicatorSystem(entities)); 

        // --- 渲染与颜色覆写 ---
        presGroup.AddSystem(new RenderSyncSystem(entities));        
        presGroup.AddSystem(new HitFeedbackVisualSystem(entities)); 
        presGroup.AddSystem(new InvincibleVisualSystem(entities));  
        
        presGroup.AddSystem(new VFXSystem(entities));             
        presGroup.AddSystem(new VFXCleanupSystem(entities));
        presGroup.AddSystem(new LightningRenderSystem(entities)); 
        
        // ==========================================
        // 【新增】：音频播放系统 (处理逻辑层抛出的音效意图)
        // ==========================================
        presGroup.AddSystem(new AudioSystem(entities));           
        
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