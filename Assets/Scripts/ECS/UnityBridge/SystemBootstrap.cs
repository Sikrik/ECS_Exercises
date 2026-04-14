// 路径: Assets/Scripts/ECS/UnityBridge/SystemBootstrap.cs
using System.Collections.Generic;
using UnityEngine;

public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        // 初始化网格系统 (2米一个格子)
        Grid = new GridSystem(2f, entities);

        // ==========================================
        // 1. 初始化组 (数据捕捉与状态收集)
        // ==========================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    
        initGroup.AddSystem(new StatusGatherSystem(entities));    
        _systemGroups.Add(initGroup);

        // ==========================================
        // 2. 模拟组 (纯逻辑计算 - 不涉及任何 Transform 操作)
        // ==========================================
        var simGroup = new SimulationSystemGroup(entities);
        
        // --- 注意：去掉了 PhysicsBakingSystem，现在直接在工厂里分配碰撞半径 ---

        // --- 技能与冷却 ---
        simGroup.AddSystem(new WeaponCooldownSystem(entities));   
        simGroup.AddSystem(new DashCooldownSystem(entities)); 

        // --- 意图生成层 (AI决策) ---
        simGroup.AddSystem(new PlayerAimingSystem(entities));     
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));  
        simGroup.AddSystem(new SwarmSeparationSystem(entities)); 
        simGroup.AddSystem(new ChargerAISystem(entities));        
        simGroup.AddSystem(new RangedAISystem(entities));   
        simGroup.AddSystem(new MeleeTargetingSystem(entities));  

        // --- 状态与动作前摇 ---
        simGroup.AddSystem(new DashPrepSystem(entities));         
        simGroup.AddSystem(new ShootPrepSystem(entities));     

        // --- 核心动作触发 ---
        simGroup.AddSystem(new DashActivationSystem(entities));             
        simGroup.AddSystem(new MeleeDashReactionSystem(entities)); 
        simGroup.AddSystem(new MeleeExecutionSystem(entities));    
        simGroup.AddSystem(new WeaponFiringSystem(entities));      
        simGroup.AddSystem(new DashStateSystem(entities));         

        // --- 【核心顺序优化】位移管线 ---
        // 先更新网格，再处理位移
        simGroup.AddSystem(Grid);                                 
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         // 唯一仲裁者，算出实体这一帧最终的逻辑 X, Y

        // --- 碰撞与结算管线 (基于 Movement 算出的最新位置) ---
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); // 纯数学碰撞检测
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); 
        simGroup.AddSystem(new BulletDestroySystem(entities));
        simGroup.AddSystem(new SlowBulletReactionSystem(entities));
        simGroup.AddSystem(new AOEBulletReactionSystem(entities));
        simGroup.AddSystem(new ChainLightningReactionSystem(entities));
        simGroup.AddSystem(new ExplosionSystem(entities));       
        simGroup.AddSystem(new DamageSystem(entities));          
        
        // --- 死亡与生命值 ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); 
        simGroup.AddSystem(new PlayerHitReactionSystem(entities)); 
        simGroup.AddSystem(new HitRecoverySystem(entities));      
        simGroup.AddSystem(new HealthSystem(entities));           
        simGroup.AddSystem(new BountySystem(entities));           
        simGroup.AddSystem(new PlayerDeathSystem(entities));      
        simGroup.AddSystem(new DeathCleanupSystem(entities));     
        simGroup.AddSystem(new ScoreSystem(entities));            
        simGroup.AddSystem(new SlowEffectSystem(entities));       
        
        // --- 内存清理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         
        simGroup.AddSystem(new GenericEventCleanupSystem<CollisionEventComponent>(entities));
        simGroup.AddSystem(new GenericEventCleanupSystem<DamageTakenEventComponent>(entities));
        simGroup.AddSystem(new GenericEventCleanupSystem<DashStartedEventComponent>(entities));
        simGroup.AddSystem(new EntityCleanupSystem(entities));    
        _systemGroups.Add(simGroup);

        // ==========================================
        // 3. 表现组 (渲染同步 - 解决抖动的关键点！)
        // ==========================================
        var presGroup = new PresentationSystemGroup(entities);
        
        // --- A. 首先处理相机的逻辑位移 ---
        // 确保它在 ViewSync 之前，这样相机追踪的是这一帧最新的逻辑坐标
        presGroup.AddSystem(new CameraFollowSystem(entities));    // 必须用 SmoothDamp + deltaTime
        
        // --- B. 同步视觉对象 ---
        // 放在相机之后，保证物体和相机在同一帧内完成 Transform 更新，彻底消除 1 帧延迟
        presGroup.AddSystem(new ViewSyncSystem(entities));        // 唯一同步 Transform 的地方
        
        // --- C. 其他表现层 ---
        presGroup.AddSystem(new VFXInstantiationSystem(entities)); 
        presGroup.AddSystem(new VisualBakingSystem(entities));    
        presGroup.AddSystem(new CameraCullingSystem(entities));   
        presGroup.AddSystem(new GhostTrailSystem(entities));      
        presGroup.AddSystem(new DirectionIndicatorSystem(entities)); 
        presGroup.AddSystem(new RenderSyncSystem(entities));        
        presGroup.AddSystem(new HitFeedbackVisualSystem(entities)); 
        presGroup.AddSystem(new InvincibleVisualSystem(entities));  
        presGroup.AddSystem(new VFXSystem(entities));             
        presGroup.AddSystem(new VFXCleanupSystem(entities));
        presGroup.AddSystem(new LightningRenderSystem(entities)); 
        presGroup.AddSystem(new AttackPreviewRenderSystem(entities));
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