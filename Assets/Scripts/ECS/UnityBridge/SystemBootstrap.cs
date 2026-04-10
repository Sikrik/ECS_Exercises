using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动器
/// 职责：初始化所有系统组，并严格按照 逻辑 -> 表现 的顺序进行每帧更新
/// </summary>
public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();

    public SystemBootstrap(List<Entity> entities)
    {
        // ========================================================
        // 1. 初始化组 (Initialization) - 负责每一帧的最早处理
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    // 捕获原始输入 (包括 Dash 按键)
        initGroup.AddSystem(new StatusGatherSystem(entities));    // 状态数据收集
        _systemGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation) - 核心逻辑运算
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        // --- 输入处理与意图识别 ---
        simGroup.AddSystem(new PlayerControlSystem(entities));    // 将移动输入转为速度意图
        
        // 【新增】冲刺逻辑：在普通移动之前处理，以便冲刺可以覆盖普通移动状态
        simGroup.AddSystem(new DashSystem(entities));             
        
        // --- 战斗与状态逻辑 ---
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    // AI 追踪
        simGroup.AddSystem(new PlayerShootingSystem(entities));   // 自动瞄准与射击
        simGroup.AddSystem(new DamageSystem(entities));           // 伤害计算
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); // 碰撞效果解析
        simGroup.AddSystem(new HitRecoverySystem(entities));      // 受击硬直处理
        simGroup.AddSystem(new HealthSystem(entities));           // 血量扣除与死亡判定
        simGroup.AddSystem(new SlowEffectSystem(entities));       // 减速 Buff 处理
        
        // --- 物理与空间逻辑 ---
        simGroup.AddSystem(new KnockbackSystem(entities));        // 击退位移
        simGroup.AddSystem(new MovementSystem(entities));         // 最终位移应用
        simGroup.AddSystem(new GridSystem(entities));             // 空间网格更新
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); // 物理碰撞检测
        
        // --- 生命周期管理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         // 倒计时销毁
        simGroup.AddSystem(new EventCleanupSystem(entities));     // 清理单帧事件组件
        simGroup.AddSystem(new EntityCleanupSystem(entities));    // 彻底销毁标记为死亡的实体
        
        _systemGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation) - 视觉渲染、UI、VFX
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        
        // 【新增】残影系统：纯表现系统，根据逻辑状态生成视觉特效
        presGroup.AddSystem(new GhostTrailSystem(entities));      
        
        presGroup.AddSystem(new ViewSyncSystem(entities));        // 将实体坐标同步到 GameObject Transform
        presGroup.AddSystem(new VFXSystem(entities));             // 受击闪烁、爆炸特效
        presGroup.AddSystem(new InvincibleVisualSystem(entities));// 无敌状态的半透明/闪烁效果
        presGroup.AddSystem(new BulletEffectSystem(entities));    // 子弹尾迹
        presGroup.AddSystem(new LightningRenderSystem(entities)); // 闪电链渲染
        presGroup.AddSystem(new UISyncSystem(entities));          // 血条、分数、游戏结束界面更新
        presGroup.AddSystem(new ScoreSystem(entities));           // 飘字分数显示
        
        _systemGroups.Add(presGroup);

        Debug.Log("<color=green>[SystemBootstrap]</color> 所有 ECS 系统已完成初始化并注册。");
    }

    /// <summary>
    /// 由 ECSManager 在每一帧调用
    /// </summary>
    public void Update(float deltaTime)
    {
        // 严格按照组的顺序更新
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }
}