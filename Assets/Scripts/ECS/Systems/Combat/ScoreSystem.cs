using System.Collections.Generic;

/// <summary>
/// 计分系统：收集所有的加分事件并统一处理
/// </summary>
public class ScoreSystem : SystemBase
{
    public ScoreSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取世界上所有的加分事件
        var scoreEvents = GetEntitiesWith<ScoreEventComponent>();

        for (int i = scoreEvents.Count - 1; i >= 0; i--)
        {
            var eventEntity = scoreEvents[i];
            var scoreEvt = eventEntity.GetComponent<ScoreEventComponent>();

            // 统一处理分数
            ECSManager.Instance.Score += scoreEvt.Amount;

            // 事件处理完毕，立刻销毁这个临时实体（阅后即焚）
            ECSManager.Instance.DestroyEntity(eventEntity);
        }
    }
}