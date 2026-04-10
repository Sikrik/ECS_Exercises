using System.Collections.Generic;

/// <summary>
/// UI 数据同步系统 (位于 PresentationSystemGroup)
/// 职责：扫描单帧 UI 事件标签，并调用 UIManager 更新实际的画面。
/// </summary>
public class UISyncSystem : SystemBase
{
    private int _lastScore = -1; // 【新增】缓存上一帧的分数

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
        // 3. 【核心修改】：通过脏标记处理得分变化
        // ==========================================
        if (ECSManager.Instance.Score != _lastScore)
        {
            UIManager.Instance.UpdateScore(ECSManager.Instance.Score);
            _lastScore = ECSManager.Instance.Score;
        }
    }
}