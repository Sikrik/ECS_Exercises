using System.Collections.Generic;

/// <summary>
/// UI 数据同步系统 (位于 PresentationSystemGroup)
/// 职责：扫描单帧 UI 事件标签，并调用 UIManager 更新实际的画面。
/// </summary>
public class UISyncSystem : SystemBase
{
    public UISyncSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        if (UIManager.Instance == null) return;

        // ==========================================
        // 1. 处理玩家血量刷新事件
        // ==========================================
        var healthEvents = GetEntitiesWith<PlayerTag, HealthComponent, UIHealthUpdateEvent>();
        foreach (var entity in healthEvents)
        {
            var health = entity.GetComponent<HealthComponent>();
            
            // 调用 UI 视图更新
            UIManager.Instance.UpdateHealth(health.CurrentHealth, health.MaxHealth);
            
            // 单帧标签，消费完毕后立即撕掉，下一帧就不会重复执行了
            entity.RemoveComponent<UIHealthUpdateEvent>();
        }

        // ==========================================
        // 2. 处理游戏结束事件
        // ==========================================
        var gameOverEvents = GetEntitiesWith<GameOverEventComponent>();
        foreach (var entity in gameOverEvents)
        {
            // 调用 UI 视图弹出面板
            UIManager.Instance.ShowGameOver(ECSManager.Instance.Score);
            
            // 这是一个专门用来当事件的实体，处理完直接打上销毁标签
            entity.AddComponent(new PendingDestroyComponent());
        }

        // ==========================================
        // 3. 处理得分变化事件 (同理)
        // ==========================================
        var scoreEvents = GetEntitiesWith<ScoreEventComponent>();
        if (scoreEvents.Count > 0)
        {
            // ScoreSystem 处理完数值累加后，这里可以直接读取最新分数刷新 UI
            // 注意：此时事件组件尚未被 EventCleanupSystem 清理，正是 UI 读取的最佳时机
            UIManager.Instance.UpdateScore(ECSManager.Instance.Score);
        }
    }
}