using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动器 (高内聚管线最终重构版)
/// 职责：初始化所有系统组，并严格按照 逻辑(数据计算) -> 生命周期(回收) -> 表现(渲染同步) 的顺序执行。
/// </summary>
public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    
    // 全局共享的网格空间划分系统
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        // 预先创建网格系统，因为它会被查询和物理系统引用
        Grid = new GridSystem(2f, entities);

        // ========================================================
        // 1. 初始化组 (Initialization) - 准备本帧基础数据
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    // 捕获玩家物理层输入
        initGroup.AddSystem(new StatusGatherSystem(entities));    // 仲裁实时速度（处理减速/硬直对速度的影响）
        _systemGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation) - 处理核心物理、技能与战斗逻辑
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        // --- 第一步：物理烘焙 ---
        simGroup.AddSystem(new PhysicsBakingSystem(entities));    // 确保新生成的实体拥有合法的碰撞数据
        
        // --- 第二步：意图与技能产生 (通用武器管线改造区) ---
        simGroup.AddSystem(new PlayerControlSystem(entities));    // 产生移动意图
        simGroup.AddSystem(new DashSystem(entities));             // 产生冲刺状态
        
        simGroup.AddSystem(new WeaponCooldownSystem(entities));   // [高内聚] 独立维护所有武器的冷却倒计时
        simGroup.AddSystem(new PlayerAimingSystem(entities));     // [高内聚] 玩家输入转换为单帧 FireIntentComponent
        // simGroup.AddSystem(new EnemyAimingSystem(entities));   // (未来扩展) AI 逻辑也可抛出 FireIntentComponent
        simGroup.AddSystem(new WeaponFiringSystem(entities, Grid)); // [高内聚] 统一消费 FireIntentComponent 并生成子弹
        
        simGroup.AddSystem(new EnemySpawnSystem(entities));       // 抛出敌人生成意图并执行
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    // AI 寻路，产生敌人移动意图
        
        // --- 第三步：物理位移与空间检测 ---
        simGroup.AddSystem(Grid);                                 // 更新实体的空间网格索引
        simGroup.AddSystem(new KnockbackSystem(entities));        // 处理受击退滑行位移
        simGroup.AddSystem(new MovementSystem(entities));         // 执行最终坐标位移计算
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); // 碰撞检测，产生 CollisionEvent
        
        // --- 第四步：冲突仲裁与伤害扣减 ---
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); // 处理物理防穿模与反弹
        simGroup.AddSystem(new DamageSystem(entities));           // [高内聚] 仅处理扣血逻辑，绝对不碰对象销毁
        simGroup.AddSystem(new BulletEffectSystem(entities));     // 处理子弹特有效果并挂载待销毁标记
        
        // --- 第五步：状态反馈与死亡结算 (死亡结算管线改造区) ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); // 受击硬直触发
        simGroup.AddSystem(new PlayerHitReactionSystem(entities)); // 玩家无敌帧触发
        simGroup.AddSystem(new HitRecoverySystem(entities));      // 更新硬直计时器
        
        simGroup.AddSystem(new HealthSystem(entities));           // [高内聚] 1. 发现空血，贴上 DeadTag
        simGroup.AddSystem(new BountySystem(entities));           // [高内聚] 2. 消费 DeadTag，产生 ScoreEvent
        simGroup.AddSystem(new PlayerDeathSystem(entities));      // [高内聚] 3. 拦截特定 DeadTag，抛出全局游戏结束
        simGroup.AddSystem(new DeathCleanupSystem(entities));     // [高内聚] 4. 正式将 DeadTag 转化为 PendingDestroy
        
        simGroup.AddSystem(new ScoreSystem(entities));            // 结算得分事件
        simGroup.AddSystem(new SlowEffectSystem(entities));       // 更新减速 Buff
        
        // --- 第六步：生命周期清理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         // 处理定时寿命衰减
        simGroup.AddSystem(new EventCleanupSystem(entities));     // 回收单帧事件（如碰撞事件、伤害事件）
        simGroup.AddSystem(new EntityCleanupSystem(entities));    // 【死神系统】统一回收所有带有 PendingDestroy 的实体
        
        _systemGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation) - 纯视觉同步，不影响任何核心数据
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        
        presGroup.AddSystem(new CameraCullingSystem(entities));   // 判定视口剔除
        presGroup.AddSystem(new GhostTrailSystem(entities));      // 冲刺残影渲染
        presGroup.AddSystem(new ViewSyncSystem(entities));        // 同步 ECS 坐标到 Unity Transform
        presGroup.AddSystem(new RenderSyncSystem(entities));      // 基础颜色同步
        presGroup.AddSystem(new VFXSystem(entities));             // 拖尾特效同步
        presGroup.AddSystem(new InvincibleVisualSystem(entities)); // 无敌闪烁表现
        presGroup.AddSystem(new LightningRenderSystem(entities)); // 闪电链特效表现
        presGroup.AddSystem(new UISyncSystem(entities));          // 血条与分数的 UI 同步
        
        _systemGroups.Add(presGroup);

        Debug.Log("<color=cyan>[SystemBootstrap]</color> ECS 启动完毕。高内聚死亡管线与通用武器管线已装载。");
    }

    public void Update(float deltaTime)
    {
        // 严格遵循：初始化 -> 逻辑模拟 -> 视觉渲染 的更新流
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }
}