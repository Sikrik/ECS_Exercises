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
            var config = ECSManager.Instance.Config;
            
            // 经验与升级判定
            var players = GetEntitiesWith<PlayerTag, ExperienceComponent>();
            foreach(var p in players)
            {
                var exp = p.GetComponent<ExperienceComponent>();
                
                // 【核心修改】：加经验时，乘以局外天赋带来的经验倍率！
                exp.CurrentXP += totalAddedScore * exp.ExpMultiplier;
                
                // 尝试从字典获取当前等级升下一级所需的经验值
                if (config.LevelExpRecipes.TryGetValue(exp.Level, out int requiredExp))
                {
                    exp.MaxXP = requiredExp; // 同步给 UI 显示
                    
                    if (exp.CurrentXP >= requiredExp && !p.HasComponent<LevelUpEventComponent>())
                    {
                        exp.CurrentXP -= requiredExp;
                        exp.Level++;
                        p.AddComponent(new LevelUpEventComponent());
                    }
                }
            }
        }
    }
}