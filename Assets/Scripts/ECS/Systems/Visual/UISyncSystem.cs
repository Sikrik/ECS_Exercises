using System.Collections.Generic;

/// <summary>
/// UI 数据同步系统 (位于 PresentationSystemGroup)
/// 职责：扫描单帧 UI 事件标签，并调用 UIManager 更新实际的画面。
/// 优化：采用脏标记机制，只有数据变化时才触发 UI 更新，维持 0 GC。
/// </summary>
public class UISyncSystem : SystemBase
{
    private int _lastScore = -1;       // 缓存上一帧的分数
    private int _lastEnemyCount = -1;  // 缓存上一帧的敌人计数

    public UISyncSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        if (UIManager.Instance == null) return;

        // ==========================================
        // 1. 处理玩家血量刷新事件 (基于单帧组件标签)
        // ==========================================
        var healthEvents = GetEntitiesWith<PlayerTag, HealthComponent, UIHealthUpdateEvent>();
        foreach (var entity in healthEvents)
        {
            var health = entity.GetComponent<HealthComponent>();
            
            // 调用 UIManager 提供的纯渲染接口
            UIManager.Instance.UpdateHealth(health.CurrentHealth, health.MaxHealth);
            
            // 消费完毕后立即移除单帧标签，防止下一帧重复执行
            entity.RemoveComponent<UIHealthUpdateEvent>();
        }

        // ==========================================
        // 2. 处理游戏结束事件
        // ==========================================
        var gameOverEvents = GetEntitiesWith<GameOverEventComponent>();
        foreach (var entity in gameOverEvents)
        {
            // 弹出结算面板并显示最终分数
            UIManager.Instance.ShowGameOver(ECSManager.Instance.Score);
            
            // 标记该事件实体在帧末销毁
            entity.AddComponent(new PendingDestroyComponent());
        }

        // ==========================================
        // 3. 同步得分变化 (基于脏标记判断)
        // ==========================================
        if (ECSManager.Instance.Score != _lastScore)
        {
            UIManager.Instance.UpdateScore(ECSManager.Instance.Score);
            _lastScore = ECSManager.Instance.Score;
        }

        // ==========================================
        // 4. 同步在场敌人计数 (新增功能)
        // ==========================================
        // 利用缓存查询直接获取所有带 EnemyTag 的实体
        var enemies = GetEntitiesWith<EnemyTag>();
        int currentEnemyCount = enemies.Count;

        if (currentEnemyCount != _lastEnemyCount)
        {
            // 需要在 UIManager 中预先实现 UpdateEnemyCount 方法
            UIManager.Instance.UpdateEnemyCount(currentEnemyCount);
            _lastEnemyCount = currentEnemyCount;
        }

        // 养成好习惯：显式调用 ReturnListToPool 虽然在你的基类中是空实现，
        // 但符合你代码规范中的“逻辑闭环”。
        ReturnListToPool(enemies);
        ReturnListToPool(healthEvents);
        ReturnListToPool(gameOverEvents);
    }
}