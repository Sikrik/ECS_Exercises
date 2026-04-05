public class HealthComponent : Component 
{
    public float CurrentHealth;
    public float MaxHealth;
    public HealthComponent(float maxHealth) { MaxHealth = maxHealth; CurrentHealth = maxHealth; }
}

public class DamageComponent : Component { public float Value; public DamageComponent(float v) => Value = v; }