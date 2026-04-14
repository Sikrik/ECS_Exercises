using System.Collections.Generic;
using UnityEngine;

public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        // 1. 初始化网格
        Grid = new GridSystem(2f, entities);

        // 2. 装配各个大组
        SetupInitializationGroup(entities);
        SetupSimulationGroup(entities);
        SetupPresentationGroup(entities);
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }

    // =========================================================
    // 阶段 1：输入与状态初始化
    // =========================================================
    private void SetupInitializationGroup(List<Entity> entities)
    {
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));
        initGroup.AddSystem(new StatusGatherSystem(entities));
        _systemGroups.Add(initGroup);
    }

    // =========================================================
    // 阶段 2：核心逻辑模拟 (按严格管线拆分)
    // =========================================================
    private void SetupSimulationGroup(List<Entity> entities)
    {
        var simGroup = new SimulationSystemGroup(entities);

        // [管线 2.1] 冷却与 AI 决策
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

        // [管线 2.2] 动作执行与位移
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
        simGroup.AddSystem(new ViewSyncSystem(entities)); // 确保位移后同步包围盒，再做物理检测

        // [管线 2.3] ★ 物理碰撞与附魔反应 (极其关键) ★
        // 这一步产生 CollisionEvent，各类系统必须在这里白嫖子弹数据！
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); 
        simGroup.AddSystem(new ImpactResolutionSystem(entities));
        simGroup.AddSystem(new EnemyHitReactionSystem(entities));     // 读基础伤害
        simGroup.AddSystem(new PlayerHitReactionSystem(entities));
        simGroup.AddSystem(new BulletDOTReactionSystem(entities));    // 挂载燃烧/剧毒
        simGroup.AddSystem(new SlowBulletReactionSystem(entities));   // 挂载减速
        simGroup.AddSystem(new AOEBulletReactionSystem(entities));    // 触发范围爆炸
        simGroup.AddSystem(new ChainLightningReactionSystem(entities));
        simGroup.AddSystem(new ExplosionSystem(entities));
        simGroup.AddSystem(new DOTSystem(entities));                  // 将 DOT 转化为当帧的伤害事件

        // [管线 2.4] ★ 伤害结算与死亡判定 ★
        simGroup.AddSystem(new DamageTextSystem(entities));           // 抢在扣血前，读取伤害数字并飘字！
        simGroup.AddSystem(new DamageSystem(entities));               // 扣血，并【销毁】伤害事件组件
        simGroup.AddSystem(new HitRecoverySystem(entities));
        simGroup.AddSystem(new HealthSystem(entities));               // 判定血量，若<=0则标记死亡

        // [管线 2.5] 内存清理与实体销毁
        simGroup.AddSystem(new BountySystem(entities));
        simGroup.AddSystem(new PlayerDeathSystem(entities));
        simGroup.AddSystem(new ScoreSystem(entities));
        simGroup.AddSystem(new SlowEffectSystem(entities));
        simGroup.AddSystem(new BulletDestroySystem(entities));        // 所有反应都完事了，放心销毁碰撞过的子弹
        simGroup.AddSystem(new DeathCleanupSystem(entities));         // 销毁死去的怪物
        simGroup.AddSystem(new LifetimeSystem(entities));
        
        // 单帧事件组件的清理必须放在 Simulation 的最末尾！
        simGroup.AddSystem(new GenericEventCleanupSystem<CollisionEventComponent>(entities));
        simGroup.AddSystem(new GenericEventCleanupSystem<DamageTakenEventComponent>(entities));
        simGroup.AddSystem(new GenericEventCleanupSystem<DashStartedEventComponent>(entities));
        simGroup.AddSystem(new EntityCleanupSystem(entities));

        _systemGroups.Add(simGroup);
    }

    // =========================================================
    // 阶段 3：视觉渲染与 UI
    // =========================================================
    private void SetupPresentationGroup(List<Entity> entities)
    {
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
}