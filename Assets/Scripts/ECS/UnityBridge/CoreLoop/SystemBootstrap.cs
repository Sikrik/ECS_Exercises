using System.Collections.Generic;

/// <summary>
/// 系统引导程序：定义 ECS 框架中所有逻辑系统的实例化顺序。
/// 这里的顺序直接决定了逻辑优先级。
/// </summary>
public static class SystemBootstrap
{
    /// <summary>
    /// 创建并返回游戏默认的系统流水线
    /// </summary>
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        // ==========================================
        // 1. 基础架构层 (必须最先运行)
        // ==========================================
        grid = new GridSystem(2.0f, entities); // 初始化空间网格
        systems.Add(grid);

        // ==========================================
        // 2. 感知与状态汇总层 (决策基础)
        // ==========================================
        systems.Add(new InputCaptureSystem(entities));    // 捕捉玩家按键意图
        
        // 【核心优化】先汇总状态（判断是否硬直、减速），后续所有移动系统将直接信任此结果
        systems.Add(new StatusGatherSystem(entities));    // 状态汇总管线
        
        // 寻路系统现在已简化，完全依赖 StatusSummaryComponent.CanMove
        systems.Add(new EnemyTrackingSystem(entities));   // AI 寻路决策

        // ==========================================
        // 3. 意图转化与生产层
        // ==========================================
        systems.Add(new PlayerControlSystem(entities));   // 玩家移动意图转化为速度
        systems.Add(new EnemySpawnSystem(entities));      // 刷怪逻辑
        systems.Add(new PlayerShootingSystem(entities, grid)); // 玩家射击逻辑

        // ==========================================
        // 4. 物理烘焙与模拟层
        // ==========================================
        // 在移动前烘焙新实体的物理数据，确保新生成的子弹/怪物在第一帧就能参与碰撞
        systems.Add(new PhysicsBakingSystem(entities));   // 物理、视觉缓存烘焙
        systems.Add(new MovementSystem(entities));        // 坐标更新与相机跟随
        
        // 物理检测产生 CollisionEventComponent
        systems.Add(new PhysicsDetectionSystem(entities)); 

        // ==========================================
        // 5. 战斗响应流水线 (职责统一版)
        // ==========================================
        
        // 第一步：DamageSystem 处理所有 DamageTakenEventComponent（包括直接碰撞和 AOE/连锁产生的受伤事件）
        systems.Add(new DamageSystem(entities));          // 统一造成伤害并判定扣血
        
        // 第二步：受击反应系统。DamageSystem 运行后，若实体存活且受过伤，则触发反应（如无敌、UI刷新）
        systems.Add(new EnemyHitReactionSystem(entities));// 怪物受击表现
        systems.Add(new PlayerHitReactionSystem(entities));// 玩家受击表现（包含无敌挂载）
        
        // 第三步：特殊效果处理。处理击退、AOE 扩散、连锁传递
        systems.Add(new KnockbackSystem(entities));       // 消费碰撞事件施加推力
        
        // 子弹特效系统现在不直接扣血，而是为 AOE 范围内的目标添加 DamageTakenEventComponent
        // 交给下一帧或后续流水线处理，实现职责解耦
        systems.Add(new BulletEffectSystem(entities));    

        // ==========================================
        // 6. 状态维持、表现与视觉同步
        // ==========================================
        systems.Add(new HealthSystem(entities));          // 死亡判定
        systems.Add(new ScoreSystem(entities));           // 分数统计
        
        systems.Add(new SlowEffectSystem(entities));      // 减速时间扣减与颜色恢复
        systems.Add(new HitRecoverySystem(entities));     // 硬直时间扣减与受击闪烁
        systems.Add(new InvincibleVisualSystem(entities));// 无敌时间扣减与半透明闪烁
        
        systems.Add(new LifetimeSystem(entities));        // 限时实体（子弹/特效）寿命管理
        systems.Add(new LightningRenderSystem(entities)); // 绘制闪电视觉
        systems.Add(new VFXSystem(entities));             // 特效位移跟随
        
        // 将逻辑坐标同步到 Unity Transform
        systems.Add(new ViewSyncSystem(entities));        // 数据同步表现层

        // ==========================================
        // 7. 帧末清理层 (核心 0 GC 保障)
        // ==========================================
        systems.Add(new EventCleanupSystem(entities));    // 清理并回收所有瞬时事件组件
        systems.Add(new EntityCleanupSystem(entities));   // 彻底移除并回收标记销毁的实体

        return systems;
    }
}