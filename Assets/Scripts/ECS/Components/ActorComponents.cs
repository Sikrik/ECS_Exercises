// Assets/Scripts/ECS/Components/ActorComponents.cs

public class EnemyStatsComponent : Component 
{
    public EnemyType Type;
    public EnemyData Config; 

    public int EnemyDeathScore => Config.EnemyDeathScore;
}