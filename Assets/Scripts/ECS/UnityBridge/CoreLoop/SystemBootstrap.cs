using System.Collections.Generic;

public static class SystemBootstrap
{
    /// <summary>
    /// 创建并返回游戏默认的系统流水线
    /// </summary>
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        // 1. 初始化网格系统 (由于其他系统依赖它，所以单独传出)
        grid = new GridSystem(2.0f, entities);
        systems.Add(grid);

        // --- 2. 感知与意图层 ---
        systems.Add(new InputCaptureSystem(entities));    // 捕捉玩家输入
        systems.Add(new StatusGatherSystem(entities));    // 状态汇总管线 (判断本帧是否能移动、计算速度倍率)
        systems.Add(new EnemyTrackingSystem(entities));   // AI 寻路决策 (依赖汇总管线的结果)

        // --- 3. 状态控制层 ---
        systems.Add(new PlayerControlSystem(entities));   // 意图转化为具体移动速度

        // --- 4. 生产与物理层 ---
        systems.Add(new EnemySpawnSystem(entities));      // 刷怪逻辑
        systems.Add(new PlayerShootingSystem(entities, grid)); // 玩家射击逻辑
        systems.Add(new PhysicsBakingSystem(entities));   // 预制体物理、视觉缓存烘焙
        systems.Add(new MovementSystem(entities));        // 坐标更新
        systems.Add(new ViewSyncSystem(entities));        // ECS 数据同步到 Unity 表现层

        // --- 5. 碰撞响应流水线 (强依赖执行顺序) ---
        systems.Add(new PhysicsDetectionSystem(entities));// 1. 物理重叠检测，产生碰撞事件
        
        systems.Add(new DamageSystem(entities));          // 2. 造成直接伤害，并抛出受伤瞬时事件
        
        // 👇 核心微调：伤害刚刚造成，立刻进行受伤反应（挂载硬直/无敌组件，并通知 UI）
        systems.Add(new EnemyHitReactionSystem(entities));// 2.1 怪物受伤反应 
        systems.Add(new PlayerHitReactionSystem(entities));// 2.2 玩家受伤反应 
        
        systems.Add(new KnockbackSystem(entities));       // 3. 消费碰撞事件，施加击退力，并扣减击退时间
        systems.Add(new BulletEffectSystem(entities));    // 4. 消费碰撞事件，触发闪电链、爆炸等

        // --- 6. 状态维持与表现 ---
        systems.Add(new SlowEffectSystem(entities));      // 减速时间扣减与冰蓝色渲染
        systems.Add(new HealthSystem(entities));          // 死亡判定，抛出计分事件
        
        // 👇 紧跟在死亡判定之后，立刻进行分数统计
        systems.Add(new ScoreSystem(entities)); 
        
        systems.Add(new HitRecoverySystem(entities));     // 硬直时间扣减与红白闪烁表现
        systems.Add(new LifetimeSystem(entities));        // 飞行物生存时间扣减
        systems.Add(new LightningRenderSystem(entities)); // 绘制闪电链特效
        systems.Add(new VFXSystem(entities));             // 跟随特效挂载表现
        systems.Add(new InvincibleVisualSystem(entities));// 无敌时间扣减与半透明闪烁表现

        // --- 7. 收尾清理层 ---
        systems.Add(new EventCleanupSystem(entities));    // 帧末：清理瞬时事件 (如碰撞事件、受伤事件)

        // 注意：如果在你的工程中有回收实体的专属系统，它必须作为全场的最后一个压轴出场
        // systems.Add(new EntityCleanupSystem(entities)); 

        return systems;
    }
}