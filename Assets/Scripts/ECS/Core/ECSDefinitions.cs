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

// --- 新增的单帧事件与视觉意图组件 ---

// 变色意图组件：逻辑层告诉表现层“我想变色”
public class ColorTintComponent : Component 
{
    public Color TargetColor;
    public ColorTintComponent(Color color) => TargetColor = color;
}

// UI 血量刷新事件（单帧组件）
public class UIHealthUpdateEvent : Component { }

// 游戏结束事件（单帧组件）
public class GameOverEventComponent : Component { }

// 在 ECSDefinitions.cs 或 StateComponents.cs 中添加
public class OffScreenTag : Component { }
/// <summary>
/// 表现层特效清理标记（单帧意图）
/// 逻辑层加上这个标签，表现层看到后就会去销毁实体身上的 GameObject 特效
/// </summary>
public class PendingVFXDestroyTag : Component { }