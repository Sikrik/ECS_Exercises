using System.Collections.Generic;

/// <summary>
/// 计分系统：收集所有的加分事件并统一处理
/// </summary>
public class ScoreSystem : SystemBase
{
    public ScoreSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var scoreEvents = GetEntitiesWith<ScoreEventComponent>();
        bool scoreChanged = false;

        for (int i = scoreEvents.Count - 1; i >= 0; i--)
        {
            var scoreEvt = scoreEvents[i].GetComponent<ScoreEventComponent>();
            ECSManager.Instance.Score += scoreEvt.Amount;
            scoreChanged = true;
            ECSManager.Instance.DestroyEntity(scoreEvents[i]);
        }

        // 👇 只有分数真的增加时，才向全世界广播：分数变啦！
        if (scoreChanged)
        {
            EventManager.Broadcast(new ScoreChangedEvent { NewScore = ECSManager.Instance.Score });
        }
    }
}