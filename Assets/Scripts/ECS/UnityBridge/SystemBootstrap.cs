using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动器 (高内聚管线重构版)
/// 职责：初始化所有系统组，并严格按照 逻辑 -> 表现 的顺序进行每帧更新。
/// </summary>
public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        // 预先创建网格系统，因为它会被多个系统引用
        Grid = new GridSystem(2f, entities);

        // ========================================================
        // 1. 初始化组 (Initialization) - 准备本帧基础数据
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    // 1. 捕获玩家输入
        initGroup.AddSystem(new StatusGatherSystem(entities));    // 2. 仲裁实时速度（处理减速/硬直对速度的影响）
        _systemGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation) - 处理核心物理与战斗逻辑
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        // --- 第一步：物理烘焙 ---
        simGroup.AddSystem(new PhysicsBakingSystem(entities));    // 确保新生成的实体在本帧有碰撞体
        
        // --- 第二步：意图与生成 ---
        simGroup.AddSystem(new PlayerControlSystem(entities));    // 处理玩家移动意图
        simGroup.AddSystem(new DashSystem(entities));             // 处理冲刺逻辑（可能覆盖移动意图）
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    // AI 寻路意图
        simGroup.AddSystem(new PlayerShootingSystem(entities, Grid)); // 射击并生成子弹
        
        // --- 第三步：物理位移与检测 ---
        simGroup.AddSystem(Grid);                                 // 更新空间网格索引
        simGroup.AddSystem(new KnockbackSystem(entities));        // 处理击退滑行
        simGroup.AddSystem(new MovementSystem(entities));         // 执行最终坐标位移
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); // 碰撞检测，产生 CollisionEvent
        
        // --- 第四步：冲突仲裁与伤害处理 ---
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); // 处理物理反弹与挤压
        simGroup.AddSystem(new DamageSystem(entities));           // 处理扣血逻辑 (已改造为纯数值系统)
        simGroup.AddSystem(new BulletEffectSystem(entities));     // 处理子弹特有效果及销毁
        
        // --- 第五步：状态反馈与生命判定 (【高内聚改造区】) ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); // 受击硬直触发
        simGroup.AddSystem(new PlayerHitReactionSystem(entities)); // 玩家无敌帧触发
        simGroup.AddSystem(new HitRecoverySystem(entities));      // 更新硬直计时
        
        // 死亡结算流水线（替代了原本大包大揽的 HealthSystem）
        simGroup.AddSystem(new HealthSystem(entities));           // 1. 判定血量，产生 DeadTag
        simGroup.AddSystem(new BountySystem(entities));           // 2. 消费 DeadTag，产生 ScoreEvent
        simGroup.AddSystem(new PlayerDeathSystem(entities));      // 3. 消费 DeadTag，产生 GameOverEvent
        simGroup.AddSystem(new DeathCleanupSystem(entities));     // 4. 将 DeadTag 转化为 PendingDestroyComponent
        
        simGroup.AddSystem(new ScoreSystem(entities));            // 结算得分 (处理 ScoreEvent)
        simGroup.AddSystem(new SlowEffectSystem(entities));       // 更新减速状态计时
        
        // --- 第六步：生命周期清理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         // 处理定时销毁（如子弹寿命）
        simGroup.AddSystem(new EventCleanupSystem(entities));     // 回收单帧事件组件到对象池
        simGroup.AddSystem(new EntityCleanupSystem(entities));    // 【死神系统】：最后统一销毁/回收实体
        
        _systemGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation) - 纯视觉同步，不影响逻辑数据
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        
        presGroup.AddSystem(new CameraCullingSystem(entities));   // 判定剔除标签
        presGroup.AddSystem(new GhostTrailSystem(entities));      // 生成残影
        presGroup.AddSystem(new ViewSyncSystem(entities));        // 同步逻辑坐标到 Transform
        presGroup.AddSystem(new RenderSyncSystem(entities));      // 同步颜色与视觉状态
        presGroup.AddSystem(new VFXSystem(entities));             // 同步跟随特效位置
        presGroup.AddSystem(new InvincibleVisualSystem(entities)); // 处理无敌闪烁表现
        presGroup.AddSystem(new LightningRenderSystem(entities)); // 绘制闪电链线段
        presGroup.AddSystem(new UISyncSystem(entities));          // 同步血条、分数、数量 UI
        
        _systemGroups.Add(presGroup);

        Debug.Log("<color=green>[SystemBootstrap]</color> ECS 管道流顺序已重构，死亡结算管线高内聚改造完成。");
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _systemGroups.Count; i++)
        {
            _systemGroups[i].Update(deltaTime);
        }
    }
}