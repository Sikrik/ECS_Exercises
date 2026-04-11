/// <summary>
/// 冲锋AI组件：记录触发冲锋的距离等参数
/// </summary>
public class ChargerAIComponent : Component
{
    public float TriggerDistance; // 距离玩家多近时开始冲锋

    public ChargerAIComponent(float triggerDistance = 6f)
    {
        TriggerDistance = triggerDistance;
    }
}