namespace DOTS.Compenonts
{
    // 路径: Assets/Scripts/DOTS/Components/PlayerComponents.cs
    using Unity.Entities;
    using Unity.Mathematics;

// 1. 玩家标签 (空结构体，不占内存)
    public struct PlayerTag : IComponentData { }
    
// 2. 玩家输入 (用于存储 WASD 转化来的方向)
    public struct PlayerInput : IComponentData
    {
        public float2 Value; // float2 完美契合 2D 移动 (x, y)
    }
}