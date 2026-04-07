public class HealthComponent : Component 
{
    public float CurrentHealth;
    public float MaxHealth;
    public HealthComponent(float maxHealth) { MaxHealth = maxHealth; CurrentHealth = maxHealth; }
}