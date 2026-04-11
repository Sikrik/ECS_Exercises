using UnityEngine;

/// <summary>
/// 武器属性组件：纯粹描述实体的武装状态，不包含任何逻辑
/// </summary>
public class WeaponComponent : Component
{
    public BulletType CurrentBulletType; // 子弹类型
    public float FireRate;               // 射击间隔（秒）
    public float CurrentCooldown;        // 当前冷却倒计时

    public WeaponComponent(BulletType bulletType, float fireRate)
    {
        CurrentBulletType = bulletType;
        FireRate = fireRate;
        CurrentCooldown = 0f;
    }
}

/// <summary>
/// 开火意图组件：这是一个【单帧状态】，表示实体在这一帧想要朝哪个方向开火
/// </summary>
public class FireIntentComponent : Component
{
    public Vector2 AimDirection;

    public FireIntentComponent(Vector2 aimDirection)
    {
        AimDirection = aimDirection;
    }
}