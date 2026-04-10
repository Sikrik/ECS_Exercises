using System.Collections.Generic;

/// <summary>
/// 系统引导器：负责初始化所有系统并严格定义它们的执行顺序（Pipeline）
/// </summary>
public static class SystemBootstrap
{
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> rootGroups = new List<SystemBase>();
        
        // GridSystem 需要跨系统共享引用
        grid = new GridSystem(2.0f, entities);

        // ========================================================
        // 1. 初始化组 (Initialization)
        // 职责：捕获玩家输入、运行 AI 寻路、重置上一帧的状态
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    // 捕获玩家键盘鼠标
        initGroup.AddSystem(new EnemyTrackingSystem(entities));   // AI 产生移动意图
        initGroup.AddSystem(new StatusGatherSystem(entities));    // 汇总减速/硬直状态，计算最终速度
        initGroup.AddSystem(new PlayerControlSystem(entities));   // 将玩家意图转化为速度
        rootGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation)
        // 职责：处理生成、移动、物理碰撞、伤害结算、Buff倒计时
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        // --- 生成与逻辑 ---
        simGroup.AddSystem(new EnemySpawnSystem(entities));      
        simGroup.AddSystem(new PlayerShootingSystem(entities, grid)); 
        simGroup.AddSystem(new PhysicsBakingSystem(entities));   
        
        // --- 运动与物理 (严格管线顺序：位移 -> 检测 -> 挤压 -> 空间划分) ---
        simGroup.AddSystem(new MovementSystem(entities));        
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); // 【核心修复】：物理检测必须在所有读取碰撞的系统之前！
        simGroup.AddSystem(new KnockbackSystem(entities));        // 虫群挤压（依赖刚才生成的碰撞事件）
        simGroup.AddSystem(grid);                                 // 更新空间网格（供后续 AOE 查找）
        
        // --- 战斗结算 (读取物理管线生成的碰撞事件实体) ---
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); // 仲裁是否被击退/硬直
        simGroup.AddSystem(new BulletEffectSystem(entities));     // 触发范围/闪电/减速意图
        simGroup.AddSystem(new DamageSystem(entities));           // 纯扣血
        simGroup.AddSystem(new EnemyHitReactionSystem(entities)); // 怪物硬直触发
        
        // --- 纯逻辑倒计时 ---
        simGroup.AddSystem(new PlayerHitReactionSystem(entities));// 玩家受击发 UI 事件
        simGroup.AddSystem(new SlowEffectSystem(entities));       // 减速倒计时与变色意图
        simGroup.AddSystem(new HitRecoverySystem(entities));      // 硬直倒计时
        simGroup.AddSystem(new LifetimeSystem(entities));         // 寿命倒计时
        
        // --- 状态结算 ---
        simGroup.AddSystem(new HealthSystem(entities));           // 【核心修复】：先让 HealthSystem 判定死亡并打上得分标签
        simGroup.AddSystem(new ScoreSystem(entities));            // 紧接着 ScoreSystem 就能顺利捕获并加分
        
        rootGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation)
        // 职责：绝对纯净的视图层，只读数据，同步 GameObject 和 UI
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        presGroup.AddSystem(new ViewSyncSystem(entities));        // 同步位置 Transform
        presGroup.AddSystem(new VFXSystem(entities));             // 跟随特效同步
        presGroup.AddSystem(new LightningRenderSystem(entities)); // 绘制闪电链
        
        presGroup.AddSystem(new InvincibleVisualSystem(entities));// 玩家无敌闪烁
        presGroup.AddSystem(new RenderSyncSystem(entities));      // 统一处理变色意图 (冰冻变蓝等)
        presGroup.AddSystem(new UISyncSystem(entities));          // 统一处理 UI 面板刷新
        rootGroups.Add(presGroup);

        // ========================================================
        // 4. 清理组 (Cleanup)
        // 职责：回收实体、清空单帧事件，确保下一帧内存干净
        // ========================================================
        var cleanupGroup = new CleanupSystemGroup(entities);
        cleanupGroup.AddSystem(new EventCleanupSystem(entities)); // 撕掉碰撞、受击、UI刷新等单帧标签
        cleanupGroup.AddSystem(new EntityCleanupSystem(entities));// 将死亡物体放回对象池
        rootGroups.Add(cleanupGroup);

        return rootGroups;
    }
}