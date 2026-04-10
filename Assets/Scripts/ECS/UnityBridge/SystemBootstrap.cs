using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 系统引导器：负责初始化所有系统并严格定义它们的执行顺序（Pipeline）
/// 架构准则：严禁随意调整顺序，每一层都依赖前一层产生的数据。
/// </summary>
public static class SystemBootstrap
{
    public static List<SystemBase> CreateDefaultSystems(List<Entity> entities, out GridSystem grid)
    {
        List<SystemBase> systems = new List<SystemBase>();

        // 空间系统初始化（格栅大小可根据地图规模调整）
        grid = new GridSystem(2.0f, entities);

        // ========================================================
        // 阶段 1：采集层 (Inputs & Intentions)
        // 职责：收集玩家输入、运行AI算法，输出“移动/射击意图”，但不执行任何位移。
        // ========================================================
        systems.Add(new InputCaptureSystem(entities));    // 捕获键盘、鼠标(ShootInput)
        systems.Add(new StatusGatherSystem(entities));    // 汇总减速、硬直等状态，作为逻辑前提
        systems.Add(new EnemyTrackingSystem(entities));  // AI计算追逐玩家的方向，输出为MoveInput

        // ========================================================
        // 阶段 2：生产层 (Spawning & Logistics)
        // 职责：处理实体的创建。
        // ========================================================
        systems.Add(new EnemySpawnSystem(entities));      // 怪物生成
        systems.Add(new PlayerShootingSystem(entities, grid)); // 读取ShootInput，在玩家位置创建子弹

        // ========================================================
        // 阶段 3：模拟层 (Simulation & Motion)
        // 职责：唯一的马达层，执行最终逻辑坐标的改变。
        // ========================================================
        systems.Add(new PhysicsBakingSystem(entities));   // 为底层物理准备数据
        systems.Add(new MovementSystem(entities));        // 唯一修改Position的系统。仲裁AI、玩家和击退力

        // 【关键时序】：GridSystem 必须在 Movement 之后，PhysicsDetection 之前更新！
        // 确保它录入的是本帧移动后的最新位置，消除“过时数据”导致的索敌/碰撞失败。
        systems.Add(grid); 

        // ========================================================
        // 阶段 4：结算层 (Combat & Physics Reactions)
        // 职责：检测碰撞、分配伤害、触发反馈。
        // ========================================================
        systems.Add(new PhysicsDetectionSystem(entities)); // 基于最新网格检测碰撞，发出事件
        systems.Add(new KnockbackSystem(entities));        // 计算肉体撞墙的反弹速度（叠加给下一帧Movement）

        // 【修复VFX消失】：BulletEffectSystem 必须在 DamageSystem 销毁子弹之前执行
        systems.Add(new BulletEffectSystem(entities));     // 子弹爆开瞬间生成特效/AOE范围

        systems.Add(new DamageSystem(entities));           // 结算伤害，并给命中的子弹打上“待销毁”标签
        systems.Add(new EnemyHitReactionSystem(entities)); // 死亡判定分流：若怪物已死，不给硬直
        systems.Add(new PlayerHitReactionSystem(entities)); // 玩家受击无敌帧处理

        // ========================================================
        // 阶段 5：视觉表现层 (Visuals & Rendering)
        // 职责：将逻辑数据转化为画面表现。
        // ========================================================
        systems.Add(new ViewSyncSystem(entities));        // 将逻辑 Position 同步给 Unity Transform
        systems.Add(new VFXSystem(entities));             // 同步跟随特效（如减速冰冻）的坐标
        systems.Add(new LightningRenderSystem(entities)); // 绘制闪电链轨迹
        systems.Add(new InvincibleVisualSystem(entities)); // 玩家无敌时的半透明闪烁效果

        // ========================================================
        // 阶段 6：生命周期层 (Post-Processing & Cleanup)
        // 职责：计分、延时销毁、回收内存。
        // ========================================================
        systems.Add(new HealthSystem(entities));          // 判断 HP<=0，发出死亡事件
        systems.Add(new ScoreSystem(entities));           // 捕捉死亡事件，增加UI分数
        
        systems.Add(new HitRecoverySystem(entities));     // 硬直计时器
        systems.Add(new SlowEffectSystem(entities));      // 减速效果计时器
        systems.Add(new LifetimeSystem(entities));        // 子弹/特效寿命计时器

        // 【安全清理】：必须最后执行，确保本帧所有系统都读过事件和销毁标签了
        systems.Add(new EventCleanupSystem(entities));    // 清理一帧寿命的 Event 实体
        systems.Add(new EntityCleanupSystem(entities));   // 彻底从内存中移除标记了 PendingDestroy 的实体

        return systems;
    }
}