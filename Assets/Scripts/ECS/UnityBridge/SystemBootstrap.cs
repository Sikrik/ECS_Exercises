// 路径: Assets/Scripts/ECS/UnityBridge/SystemBootstrap.cs
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
        // 2. 模拟组 (逻辑结算 - 顺序极其关键！)
        // ==========================================
        var simGroup = new SimulationSystemGroup(entities);
        simGroup.AddSystem(new PhysicsBakingSystem(entities));    

        // --- 技能与冷却 ---
        simGroup.AddSystem(new WeaponCooldownSystem(entities));   
        simGroup.AddSystem(new DashCooldownSystem(entities)); 

        // --- 意图生成层 (Player & AI) ---
        simGroup.AddSystem(new PlayerAimingSystem(entities));     
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));  
        simGroup.AddSystem(new SwarmSeparationSystem(entities)); // 👈【新增】：紧跟AI之后，修正拥挤意图
        simGroup.AddSystem(new ChargerAISystem(entities));        
        simGroup.AddSystem(new RangedAISystem(entities));   
        simGroup.AddSystem(new MeleeTargetingSystem(entities));  // 近战决策

        // --- 状态与前摇处理 ---
        simGroup.AddSystem(new DashPrepSystem(entities));         
        simGroup.AddSystem(new ShootPrepSystem(entities));     

        // --- 核心技能触发与执行 ---
        simGroup.AddSystem(new DashActivationSystem(entities));             
        simGroup.AddSystem(new MeleeDashReactionSystem(entities)); // 👈【新增】：监听冲刺事件触发旋风斩
        simGroup.AddSystem(new MeleeExecutionSystem(entities));    // 执行挥砍意图，抛出伤害
        simGroup.AddSystem(new WeaponFiringSystem(entities));      // 执行射击意图
        
        simGroup.AddSystem(new DashStateSystem(entities));         

        // --- 物理与运动管线 ---
        simGroup.AddSystem(Grid);                                 
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         // 唯一仲裁者，执行最终位移
        simGroup.AddSystem(new ViewSyncSystem(entities));         
        
        // --- 碰撞与伤害管线 ---
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); 
        simGroup.AddSystem(new BulletDestroySystem(entities));
        simGroup.AddSystem(new SlowBulletReactionSystem(entities));
        simGroup.AddSystem(new AOEBulletReactionSystem(entities));
        simGroup.AddSystem(new ChainLightningReactionSystem(entities));
        simGroup.AddSystem(new ExplosionSystem(entities));       // 爆炸抛出伤害事件
        simGroup.AddSystem(new DamageSystem(entities));          // 唯一生命值修改者！
        
        // --- 受击反应与死亡结算 ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); 
        simGroup.AddSystem(new PlayerHitReactionSystem(entities)); 
        simGroup.AddSystem(new HitRecoverySystem(entities));      
        simGroup.AddSystem(new HealthSystem(entities));           
        simGroup.AddSystem(new BountySystem(entities));           
        simGroup.AddSystem(new PlayerDeathSystem(entities));      
        simGroup.AddSystem(new DeathCleanupSystem(entities));     
        simGroup.AddSystem(new ScoreSystem(entities));            
        simGroup.AddSystem(new SlowEffectSystem(entities));       
        
        // --- 生命周期与内存清理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         
        simGroup.AddSystem(new GenericEventCleanupSystem<CollisionEventComponent>(entities));
        simGroup.AddSystem(new GenericEventCleanupSystem<DamageTakenEventComponent>(entities));
        simGroup.AddSystem(new GenericEventCleanupSystem<DashStartedEventComponent>(entities));
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
        
        // --- 音频与UI ---
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