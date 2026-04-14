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

        // 物理烘焙系统
        simGroup.AddSystem(new PhysicsBakingSystem(entities));

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

        // --- 位移管线 ---
        simGroup.AddSystem(Grid);
        simGroup.AddSystem(new KnockbackSystem(entities));
        simGroup.AddSystem(new MovementSystem(entities));

        // 确保碰撞盒位置与逻辑坐标严格同步
        simGroup.AddSystem(new ViewSyncSystem(entities));

        // --- 碰撞与结算管线 ---
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        simGroup.AddSystem(new ImpactResolutionSystem(entities));
        simGroup.AddSystem(new BulletDestroySystem(entities));
        
        // --- 元素与反应 (五行与特技逻辑在此处) ---
        simGroup.AddSystem(new BulletDOTReactionSystem(entities));    // 新增：五行 DOT 传染
        simGroup.AddSystem(new SlowBulletReactionSystem(entities));
        simGroup.AddSystem(new AOEBulletReactionSystem(entities));
        simGroup.AddSystem(new ChainLightningReactionSystem(entities));
        simGroup.AddSystem(new ExplosionSystem(entities));
        
        // --- 伤害结算 ---
        simGroup.AddSystem(new DOTSystem(entities));                  // 新增：DOT 每秒跳血
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
        // 3. 表现组 (渲染同步)
        // ==========================================
        var presGroup = new PresentationSystemGroup(entities);

        // 相机跟随
        presGroup.AddSystem(new CameraFollowSystem(entities));
        
        // 特效与视觉反馈
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
        
        // 新增：伤害跳字 UI
        presGroup.AddSystem(new DamageTextSystem(entities));

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