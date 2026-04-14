using DOTS.Compenonts;

namespace DOTS.Systems
{
    using Unity.Entities;
    using Unity.Transforms;
    using Unity.Mathematics;

    public partial struct PlayerMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // DOTS 获取 DeltaTime 的方式
            float deltaTime = SystemAPI.Time.DeltaTime;

            // 遍历查询：
            // LocalTransform 需要修改坐标 -> RefRW
            // PlayerInput 和 MoveSpeed 只需要读取 -> RefRO (Read Only，只读可以提升多线程性能)
            foreach (var (transform, input, speed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerInput>, RefRO<MoveSpeed>>().WithAll<PlayerTag>())
            {
                // float2 转 float3 (如果是 3D 游戏，这里的 y 就是 z)
                float3 moveDirection = new float3(input.ValueRO.Value.x, input.ValueRO.Value.y, 0);

                // 修改 LocalTransform 的坐标
                transform.ValueRW.Position += moveDirection * speed.ValueRO.Value * deltaTime;
            }
        }
    }
}