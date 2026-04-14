using DOTS.Compenonts;

namespace DOTS.Systems
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

// 确保在主逻辑更新前获取输入
    [UpdateInGroup(typeof(InitializationSystemGroup))] 
    public partial struct PlayerInputSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // 1. 获取传统的 Unity 输入 (这部分只能在主线程运行)
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            float2 inputDir = math.normalizesafe(new float2(x, y));

            // 2. 遍历所有带有 PlayerTag 和 PlayerInput 的实体
            // RefRW 表示我们需要对数据进行 读写 (Read/Write)
            foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<PlayerTag>())
            {
                // 将读取到的输入赋值给组件
                playerInput.ValueRW.Value = inputDir;
            }
        }
    }
}