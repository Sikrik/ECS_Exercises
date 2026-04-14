namespace DOTS.Compenonts
{
    using Unity.Entities;

    public struct Lifetime : IComponentData
    {
        public float Value; // 剩余存活时间
    }
}