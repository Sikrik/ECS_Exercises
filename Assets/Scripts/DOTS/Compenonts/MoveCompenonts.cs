using Unity.Entities;
using Unity.Mathematics; // 使用 Unity.Mathematics 替代 Mathf 和 Vector3

public struct Position : IComponentData {
    public float3 Value;
}

public struct Velocity : IComponentData {
    public float2 Value;
}