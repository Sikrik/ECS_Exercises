using System.Collections.Generic;

public class ScoreSystem : SystemBase
{
    public ScoreSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var scoreEvents = GetEntitiesWith<ScoreEventComponent>();
        if (scoreEvents.Count == 0) 
        {
            ReturnListToPool(scoreEvents);
            return;
        }

        int totalAddedScore = 0;
        bool scoreChanged = false;

        for (int i = scoreEvents.Count - 1; i >= 0; i--)
        {
            var entity = scoreEvents[i];
            var evt = entity.GetComponent<ScoreEventComponent>();

            totalAddedScore += evt.Amount;
            scoreChanged = true;

            // 处理完立即移除组件，防止同一帧内其他系统干扰或下一帧重复计算
            entity.RemoveComponent<ScoreEventComponent>(); 
        }

        if (scoreChanged)
        {
            ECSManager.Instance.Score += totalAddedScore;
            EventManager.Broadcast(new ScoreChangedEvent { NewScore = ECSManager.Instance.Score });
        }

        ReturnListToPool(scoreEvents);
    }
}