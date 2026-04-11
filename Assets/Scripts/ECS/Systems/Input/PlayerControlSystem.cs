using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家控制系统
/// 注意：在引入统一的惯性系统后，玩家的速度计算已彻底交由 MovementSystem 接管。
/// 此系统目前作为一个空壳保留，可用于未来扩展纯玩家专属的非移动类输入判定逻辑。
/// </summary>
public class PlayerControlSystem : SystemBase
{
    public PlayerControlSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选：玩家标记 + 移动意图 + 速度/速率组件
        var entities = GetEntitiesWith<PlayerTag, MoveInputComponent, VelocityComponent, SpeedComponent>();

        foreach (var entity in entities)
        {
            // =========================================================================
            // 【核心重构注记】
            // 1. 玩家的状态锁（击退、硬直）已在 MovementSystem 中统一仲裁。
            // 2. 速度计算（Lerp 惯性与 MaxHealth 挂钩）也已移至 MovementSystem 统一处理。
            // 因此，这里不再直接对 VelocityComponent 赋值，防止破坏全局惯性插值系统。
            // =========================================================================
            
            // 未来如果你有诸如“按下特定键扣除体力”等仅在移动时触发的纯玩家逻辑，可以写在这里
        }

        // 归还查询列表
        ReturnListToPool(entities);
    }
}