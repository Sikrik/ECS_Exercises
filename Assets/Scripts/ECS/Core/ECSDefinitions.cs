using UnityEngine;

public abstract class Component { }

public enum BulletType { Normal, Slow, ChainLightning, AOE }
public enum EnemyType { Normal, Fast, Tank }

// 玩家血量变化事件
public struct PlayerHealthChangedEvent 
{
    public float CurrentHealth;
    public float MaxHealth;
}

// 玩家得分变化事件
public struct ScoreChangedEvent 
{
    public int NewScore;
}

// 游戏结束事件
public struct GameOverEvent { }