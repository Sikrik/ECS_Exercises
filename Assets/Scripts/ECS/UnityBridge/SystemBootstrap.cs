using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 系统启动器
/// 职责：初始化所有系统组，并严格按照 逻辑 -> 表现 的顺序进行每帧更新
/// </summary>
public class SystemBootstrap
{
    private List<SystemGroup> _systemGroups = new List<SystemGroup>();
    
    // 对外暴露 GridSystem，供 ECSManager 获取
    public GridSystem Grid { get; private set; }

    public SystemBootstrap(List<Entity> entities)
    {
        // 提前实例化 GridSystem，填入 CellSize (这里设定为 2f，可按需调整)
        Grid = new GridSystem(2f, entities);

        // ========================================================
        // 1. 初始化组 (Initialization) - 负责每一帧的最早处理
        // ========================================================
        var initGroup = new InitializationSystemGroup(entities);
        initGroup.AddSystem(new InputCaptureSystem(entities));    
        initGroup.AddSystem(new StatusGatherSystem(entities));    
        _systemGroups.Add(initGroup);

        // ========================================================
        // 2. 模拟组 (Simulation) - 核心逻辑运算 (严格按顺序执行)
        // ========================================================
        var simGroup = new SimulationSystemGroup(entities);
        
        // --- 第一步：物理组件烘焙 (确保新生成的实体立刻有碰撞体) ---
        simGroup.AddSystem(new PhysicsBakingSystem(entities));
        
        // --- 第二步：意图与输入 ---
        simGroup.AddSystem(new PlayerControlSystem(entities));    
        simGroup.AddSystem(new DashSystem(entities));             
        simGroup.AddSystem(new EnemySpawnSystem(entities));       
        simGroup.AddSystem(new EnemyTrackingSystem(entities));    
        simGroup.AddSystem(new PlayerShootingSystem(entities, Grid)); 
        
        // --- 第三步：位移计算与应用 ---
        simGroup.AddSystem(new KnockbackSystem(entities));        
        simGroup.AddSystem(new MovementSystem(entities));         
        simGroup.AddSystem(Grid);             
        
        // --- 第四步：物理检测与立即结算 (核心防丢事件闭环) ---
        simGroup.AddSystem(new PhysicsDetectionSystem(entities)); // 产生碰撞事件
        simGroup.AddSystem(new DamageSystem(entities));           // 结算伤害
        simGroup.AddSystem(new ImpactResolutionSystem(entities)); // 结算物理击退与怪物互推
        simGroup.AddSystem(new BulletEffectSystem(entities));     // 结算子弹命中特效/范围伤害
        
        // --- 第五步：状态反馈与状态机 ---
        simGroup.AddSystem(new EnemyHitReactionSystem(entities));
        simGroup.AddSystem(new PlayerHitReactionSystem(entities));
        simGroup.AddSystem(new HitRecoverySystem(entities));      
        simGroup.AddSystem(new HealthSystem(entities));           
        simGroup.AddSystem(new SlowEffectSystem(entities));       
        
        // --- 第六步：生命周期管理 ---
        simGroup.AddSystem(new LifetimeSystem(entities));         
        simGroup.AddSystem(new EventCleanupSystem(entities));     // 清除这一帧产生的所有事件
        simGroup.AddSystem(new EntityCleanupSystem(entities));    // 销毁死亡实体
        
        _systemGroups.Add(simGroup);

        // ========================================================
        // 3. 表现组 (Presentation) - 视觉渲染、UI、VFX
        // ========================================================
        var presGroup = new PresentationSystemGroup(entities);
        
        presGroup.AddSystem(new GhostTrailSystem(entities));      
        presGroup.AddSystem(new ViewSyncSystem(entities));        
        presGroup.AddSystem(new RenderSyncSystem(entities));      // 变色同步 (受击闪烁/冰冻)
        presGroup.AddSystem(new VFXSystem(entities));             
        presGroup.AddSystem(new InvincibleVisualSystem(entities));
        presGroup.AddSystem(new LightningRenderSystem(entities)); 
        presGroup.AddSystem(new UISyncSystem(entities));          
        presGroup.AddSystem(new ScoreSystem(entities));           
        
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