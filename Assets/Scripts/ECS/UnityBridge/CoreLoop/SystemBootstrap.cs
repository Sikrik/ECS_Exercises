using System.Collections.Generic;

/// <summary>
/// 系统引导程序：定义 ECS 框架中所有逻辑系统的执行顺序。
/// 这里的顺序直接决定了逻辑优先级，是消除视觉抖动和逻辑 Bug 的核心。
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
        // 1. 基础架构与空间管理 (必须最先运行)
        // ==========================================
        grid = new GridSystem(2.0f, entities); // 初始化/刷新空间网格
        systems.Add(grid);

        // ==========================================
        // 2. 输入捕捉与状态汇总 (决策基础)
        // ==========================================
        systems.Add(new InputCaptureSystem(entities));    // 捕捉玩家按键意图
        
        // 状态汇总管线：判断是否硬直、减速，后续系统将直接信任此结果
        systems.Add(new StatusGatherSystem(entities));    
        
        // AI 寻路决策：依赖 StatusGatherSystem 的移动许可标记
        systems.Add(new EnemyTrackingSystem(entities));   

        // ==========================================
        // 3. 意图转化与生产层
        // ==========================================
        systems.Add(new PlayerControlSystem(entities));   // 玩家输入转化为速度
        systems.Add(new EnemySpawnSystem(entities));      // 刷怪逻辑
        systems.Add(new PlayerShootingSystem(entities, grid)); // 射击逻辑

        // ==========================================
        // 4. 物理模拟与视觉对齐 (核心优化：逻辑与视觉紧贴)
        // ==========================================
        // 在移动前烘焙新实体的物理引用和渲染器缓存，消除每帧 GetComponent
        systems.Add(new PhysicsBakingSystem(entities));   

        // 逻辑坐标更新
        systems.Add(new MovementSystem(entities));        

        // 【冗余修复】立即同步逻辑坐标到 Unity Transform，彻底解决画面撕裂
        systems.Add(new ViewSyncSystem(entities));        
        
        // 物理检测：基于更新后的位置产生 CollisionEventComponent
        systems.Add(new PhysicsDetectionSystem(entities)); 

        // ==========================================
        // 5. 战斗响应流水线 (职责解耦版)
        // ==========================================
        
        // 第一步：DamageSystem 处理所有伤害意图（包含碰撞和后续的 AOE/连锁）
        systems.Add(new DamageSystem(entities));          
        
        // 第二步：受击反应。仅对通过 DamageSystem 判定后存活且受过伤的实体触发表现
        systems.Add(new EnemyHitReactionSystem(entities));
        systems.Add(new PlayerHitReactionSystem(entities));
        
        // 第三步：特殊效果处理。产生位移修正或扩散新的伤害意图
        systems.Add(new KnockbackSystem(entities));       
        
        // 子弹特效系统：不再直接扣血，仅产生 AOE/连锁的 DamageTakenEventComponent
        systems.Add(new BulletEffectSystem(entities));    

        // ==========================================
        // 6. 状态维持与表现辅助
        // ==========================================
        systems.Add(new HealthSystem(entities));          // 死亡判定
        systems.Add(new ScoreSystem(entities));           // 分数统计
        
        systems.Add(new SlowEffectSystem(entities));      // 减速时间扣减
        systems.Add(new HitRecoverySystem(entities));     // 硬直时间扣减与受击闪烁
        systems.Add(new InvincibleVisualSystem(entities));// 无敌时间扣减与半透明闪烁
        
        systems.Add(new LifetimeSystem(entities));        // 寿命管理
        systems.Add(new LightningRenderSystem(entities)); // 绘制闪电视觉
        systems.Add(new VFXSystem(entities));             // 附加特效位置跟随

        // ==========================================
        // 7. 帧末清理层 (核心 0 GC 保障)
        // ==========================================
        // 统一清理并回收所有瞬时事件组件到对象池
        systems.Add(new EventCleanupSystem(entities));    
        // 彻底移除标记销毁的实体并回收逻辑对象
        systems.Add(new EntityCleanupSystem(entities));   

        return systems;
    }
}