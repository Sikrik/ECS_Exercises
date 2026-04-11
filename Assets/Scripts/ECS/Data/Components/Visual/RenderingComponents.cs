using UnityEngine;

public class ViewComponent : Component 
{
    public GameObject GameObject;
    public GameObject Prefab; 
    public SpriteRenderer SpriteRenderer; // 新增缓存
    public ViewComponent(GameObject go, GameObject prefab) { GameObject = go; Prefab = prefab; }
}

public class BaseColorComponent : Component 
{
    public Color Color; // 存储物体的原始颜色，用于受击/冰冻效果恢复
    public BaseColorComponent(Color c) => Color = c;
}
// 在 RenderingComponents.cs 文件的末尾添加：
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