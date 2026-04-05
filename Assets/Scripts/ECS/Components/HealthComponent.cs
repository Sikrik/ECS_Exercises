/// <summary>
/// 血量组件，存储实体的血量相关属性
/// 用于表示实体的存活状态
/// </summary>
public class HealthComponent : Component
{
    /// <summary>
    /// 实体的当前血量
    /// 当该值小于等于0时，实体将会被销毁
    /// </summary>
    public float CurrentHealth;
    
    /// <summary>
    /// 实体的最大血量，初始时当前血量等于最大血量
    /// </summary>
    public float MaxHealth;
    
    // 新增：无敌计时器，受伤后生效
    public float InvincibleTimer;
    
    /// <summary>
    /// 初始化血量组件实例
    /// </summary>
    /// <param name="maxHealth">实体的最大血量</param>
    public HealthComponent(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }
}