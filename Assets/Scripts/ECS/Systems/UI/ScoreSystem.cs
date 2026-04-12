using System.Collections.Generic;

public class ScoreSystem : SystemBase
{
    public ScoreSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var scoreEvents = GetEntitiesWith<ScoreEventComponent>();
        if (scoreEvents.Count == 0) return;

        int totalAddedScore = 0;
        for (int i = scoreEvents.Count - 1; i >= 0; i--)
        {
            var entity = scoreEvents[i];
            totalAddedScore += entity.GetComponent<ScoreEventComponent>().Amount;
            entity.RemoveComponent<ScoreEventComponent>(); 
        }

        if (totalAddedScore > 0)
        {
            ECSManager.Instance.Score += totalAddedScore;
            
            // 经验与升级判定
            var players = GetEntitiesWith<PlayerTag, ExperienceComponent>();
            foreach(var p in players)
            {
                var exp = p.GetComponent<ExperienceComponent>();
                exp.CurrentXP += totalAddedScore;
                
                if (exp.CurrentXP >= exp.MaxXP && !p.HasComponent<LevelUpEventComponent>())
                {
                    exp.CurrentXP -= exp.MaxXP;
                    exp.MaxXP *= 1.2f; // 每级所需经验增加 20%
                    exp.Level++;
                    p.AddComponent(new LevelUpEventComponent());
                }
            }
        }
    }
}