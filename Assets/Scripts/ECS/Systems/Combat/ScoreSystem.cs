using System.Collections.Generic;

/// <summary>
/// 计分系统：收集所有的加分事件并统一处理
/// 【优化版】：合并遍历、防止重复加分、安全的内存回收
/// </summary>
public class ScoreSystem : SystemBase
{
    public ScoreSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var scoreEvents = GetEntitiesWith<ScoreEventComponent>();
        
        bool scoreChanged = false;
        int totalAddedScore = 0; // 缓存本帧总共加了多少分

        // 1. 倒序遍历，安全处理本帧所有的加分事件
        for (int i = scoreEvents.Count - 1; i >= 0; i--)
        {
            var entity = scoreEvents[i];
            var evt = entity.GetComponent<ScoreEventComponent>();

            // 累加本帧的分数
            totalAddedScore += evt.Amount;
            scoreChanged = true;

            // 👇 双保险：加完分立刻撕掉计分标签，哪怕收尸慢了一帧也不会重复加分！
            entity.RemoveComponent<ScoreEventComponent>(); 
            
            // 注：不需要在这里加 PendingDestroyComponent，因为怪物在 HealthSystem 已经被判死刑了
        }

        // 2. 如果分数真的增加了，统一更新数据并向全宇宙广播
        if (scoreChanged)
        {
            // 更新全局分数 (统一使用 ECSManager 或 GameManager，这里以你代码中的 ECSManager 为准)
            ECSManager.Instance.Score += totalAddedScore;

            // 广播分数改变事件，通知 UI 刷新
            EventManager.Broadcast(new ScoreChangedEvent { NewScore = ECSManager.Instance.Score });
        }

        // 👇 3. 全场最后一步：必须在方法的最后一行归还 List，防止内存泄漏！
        ReturnListToPool(scoreEvents);
    }
}