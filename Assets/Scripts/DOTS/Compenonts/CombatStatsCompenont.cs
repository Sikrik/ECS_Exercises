namespace DOTS.Compenonts
{
    using Unity.Entities;

// 生命值
    public struct Health : IComponentData
    {
        public float Value;
        public float MaxValue;
    }

// 武器射击间隔
    public struct WeaponCooldown : IComponentData
    {
        public float FireRate;     // 射击间隔（秒）
        public float CurrentTimer; // 当前冷却计时器
    }
}