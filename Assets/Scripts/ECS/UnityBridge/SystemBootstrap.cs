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
        // 2. 模拟组 (纯逻辑计算 - 包含物理与位置同步)
        // ==========================================
        var simGroup = new SimulationSystemGroup(entities);

        simGroup.AddSystem(new PhysicsBakingSystem(entities));
        simGroup.AddSystem(new WeaponCooldownSystem(entities));
        simGroup.AddSystem(new DashCooldownSystem(entities));

        simGroup.AddSystem(new PlayerAimingSystem(entities));
        simGroup.AddSystem(new EnemySpawnSystem(entities));
        simGroup.AddSystem(new EnemyTrackingSystem(entities));
        simGroup.AddSystem(new SwarmSeparationSystem(entities));
        simGroup.AddSystem(new ChargerAISystem(entities));
        simGroup.AddSystem(new RangedAISystem(entities));
        simGroup.AddSystem(new MeleeTargetingSystem(entities));

        simGroup.AddSystem(new DashPrepSystem(entities));
        simGroup.AddSystem(new ShootPrepSystem(entities));
        simGroup.AddSystem(new DashActivationSystem(entities));
        simGroup.AddSystem(new MeleeDashReactionSystem(entities));
        simGroup.AddSystem(new MeleeExecutionSystem(entities));
        simGroup.AddSystem(new WeaponFiringSystem(entities));
        simGroup.AddSystem(new DashStateSystem(entities));

        simGroup.AddSystem(Grid);
        simGroup.AddSystem(new KnockbackSystem(entities));
        simGroup.AddSystem(new MovementSystem(entities));
        simGroup.AddSystem(new ViewSyncSystem(entities));

        // --- 1. 物理碰撞 ---
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        simGroup.AddSystem(new ImpactResolutionSystem(entities));
        
        // --- 2. 核心反应 (必须在子弹销毁前读取数据!) ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities));     // 读取子弹基础伤害
        simGroup.AddSystem(new PlayerHitReactionSystem(entities));
        simGroup.AddSystem(new BulletDOTReactionSystem(entities));    // 挂载五行 DOT
        simGroup.AddSystem(new SlowBulletReactionSystem(entities));
        simGroup.AddSystem(new AOEBulletReactionSystem(entities));
        simGroup.AddSystem(new ChainLightningReactionSystem(entities));
        simGroup.AddSystem(new ExplosionSystem(entities));
        
        simGroup.AddSystem(new DOTSystem(entities));                  // 每秒生成 DOT 伤害事件
        
        // --- 3. 伤害表现 (必须在 DamageSystem 消耗事件前执行!) ---
        simGroup.AddSystem(new DamageTextSystem(entities));           // <--- 移到这里！抢在扣血前读数字

        // --- 4. 扣血结算 ---
        simGroup.AddSystem(new DamageSystem(entities));               // 扣除血量，并删除 DamageEvent
        
        // --- 5. 状态与死亡判定 ---
        simGroup.AddSystem(new HitRecoverySystem(entities));
        simGroup.AddSystem(new HealthSystem(entities));
        simGroup.AddSystem(new BountySystem(entities));
        simGroup.AddSystem(new PlayerDeathSystem(entities));
        simGroup.AddSystem(new DeathCleanupSystem(entities));
        simGroup.AddSystem(new ScoreSystem(entities));
        simGroup.AddSystem(new SlowEffectSystem(entities));

        // --- 6. 销毁子弹 (等所有反应都白嫖完子弹数据后，再销毁它) ---
        simGroup.AddSystem(new BulletDestroySystem(entities));        // <--- 移到这里！

        // --- 内存清理 ---
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

        presGroup.AddSystem(new CameraFollowSystem(entities));
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