using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动与装配中心
/// 负责初始化所有系统并将其加入执行管线
/// </summary>
public class SystemBootstrap : MonoBehaviour
{
    private SystemGroup _inputGroup;      // 输入层
    private SystemGroup _simulationGroup; // 逻辑层
    private SystemGroup _presentationGroup; // 表现层

    void Awake()
    {
        // 1. 初始化系统组
        _inputGroup = new SystemGroup();
        _simulationGroup = new SystemGroup();
        _presentationGroup = new SystemGroup();

        var entities = ECSManager.Instance.Entities;

        // ==========================================
        // 2. 装配【输入层】
        // ==========================================
        _inputGroup.AddSystem(new InputCaptureSystem(entities));
        _inputGroup.AddSystem(new PlayerAimingSystem(entities));

        // ==========================================
        // 3. 装配【逻辑与战斗层】
        // ==========================================
        
        // --- AI 与 移动 ---
        _simulationGroup.AddSystem(new EnemyTrackingSystem(entities));
        _simulationGroup.AddSystem(new ChargerAISystem(entities));
        _simulationGroup.AddSystem(new RangedAISystem(entities));
        _simulationGroup.AddSystem(new MovementSystem(entities));
        
        // --- 技能预热与触发 ---
        _simulationGroup.AddSystem(new DashPrepSystem(entities));
        _simulationGroup.AddSystem(new DashSystem(entities));
        _simulationGroup.AddSystem(new ShootPrepSystem(entities));
        _simulationGroup.AddSystem(new WeaponFiringSystem(entities));
        _simulationGroup.AddSystem(new WeaponCooldownSystem(entities));

        // --- 物理与碰撞检测 ---
        _simulationGroup.AddSystem(new PhysicsDetectionSystem(entities));
        _simulationGroup.AddSystem(new GridSystem(entities));

        // --- 核心战斗反应 (五行与特技逻辑在此处) ---
        // 注意：ReactionSystem 必须在 DamageSystem 之前执行，用于给敌人挂载组件
        _simulationGroup.AddSystem(new BulletDOTReactionSystem(entities));    // 五行 DOT 挂载（火、木等）
        _simulationGroup.AddSystem(new SlowBulletReactionSystem(entities));    // 冰霜减速挂载
        _simulationGroup.AddSystem(new ChainLightningReactionSystem(entities));// 闪电连锁
        _simulationGroup.AddSystem(new AOEBulletReactionSystem(entities));     // 爆炸范围伤害
        _simulationGroup.AddSystem(new ImpactResolutionSystem(entities));      // 物理反弹结算
        _simulationGroup.AddSystem(new DOTSystem(entities));                   // DOT 每秒跳血逻辑

        // --- 伤害与生命周期结算 ---
        _simulationGroup.AddSystem(new MeleeExecutionSystem(entities));        // 近战斩杀与剑气发射
        _simulationGroup.AddSystem(new DamageSystem(entities));                // 最终扣血计算
        _simulationGroup.AddSystem(new HealthSystem(entities));                // 血量检查与死亡判定
        _simulationGroup.AddSystem(new BountySystem(entities));                // 击杀奖励计算
        
        // --- 清理 ---
        _simulationGroup.AddSystem(new BulletDestroySystem(entities));         // 销毁已碰撞子弹
        _simulationGroup.AddSystem(new DeathCleanupSystem(entities));          // 销毁死亡实体
        _simulationGroup.AddSystem(new LifetimeSystem(entities));              // 销毁过时实体
        _simulationGroup.AddSystem(new GenericEventCleanupSystem(entities));   // 清理单帧事件组件

        // ==========================================
        // 4. 装配【表现与 UI 层】
        // ==========================================
        
        // --- 特效实例化与清理 ---
        _presentationGroup.AddSystem(new VFXInstantiationSystem(entities));    // 生成特效物体
        _presentationGroup.AddSystem(new VFXCleanupSystem(entities));          // 销毁已结束特效
        
        // --- 视觉反馈 ---
        _presentationGroup.AddSystem(new ViewSyncSystem(entities));            // 同步位置到 GameObject
        _presentationGroup.AddSystem(new HitFeedbackVisualSystem(entities));   // 受击闪红
        _presentationGroup.AddSystem(new InvincibleVisualSystem(entities));    // 无敌透明效果
        _presentationGroup.AddSystem(new DirectionIndicatorSystem(entities));  // 方向指示器
        
        // --- UI 同步 ---
        _presentationGroup.AddSystem(new DamageTextSystem(entities));          // 伤害飘字 UI 弹出
        _presentationGroup.AddSystem(new UISyncSystem(entities));              // 同步血条 HUD
        _presentationGroup.AddSystem(new ScoreSystem(entities));               // 分数更新
        
        // --- 音效与摄像机 ---
        _presentationGroup.AddSystem(new AudioSystem(entities));
        _presentationGroup.AddSystem(new CameraFollowSystem(entities));
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        // 严格按照层级顺序更新
        _inputGroup.Update(deltaTime);
        _simulationGroup.Update(deltaTime);
        _presentationGroup.Update(deltaTime);
    }
}