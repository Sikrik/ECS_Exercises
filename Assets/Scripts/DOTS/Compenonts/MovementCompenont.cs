namespace DOTS.Compenonts
{
    using Unity.Entities;
    using Unity.Mathematics;

// 移动速度（基础属性）
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

// 当前速度向量（用于每一帧改变 LocalTransform 的 Position）
    public struct Velocity : IComponentData
    {
        public float3 Value; // 推荐统一使用 float3，如果是纯2D游戏，让 z 始终为 0 即可
    }
}