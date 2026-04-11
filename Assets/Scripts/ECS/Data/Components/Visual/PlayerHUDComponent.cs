using UnityEngine;

/// <summary>
/// 玩家专属 HUD 数据组件（已移除通用方向指示器数据）
/// </summary>
public class PlayerHUDComponent : Component 
{
    public UnityEngine.UI.Image HealthRing;
    public UnityEngine.UI.Image FlashIcon;

    public PlayerHUDComponent(UnityEngine.UI.Image health, UnityEngine.UI.Image flash) 
    {
        HealthRing = health;
        FlashIcon = flash;
    }
}