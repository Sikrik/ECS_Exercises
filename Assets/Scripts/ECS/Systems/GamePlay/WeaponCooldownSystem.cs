using System.Collections.Generic;

public class WeaponCooldownSystem : SystemBase
{
    public WeaponCooldownSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var armedEntities = GetEntitiesWith<WeaponComponent>();

        for (int i = armedEntities.Count - 1; i >= 0; i--)
        {
            var weapon = armedEntities[i].GetComponent<WeaponComponent>();
            if (weapon.CurrentCooldown > 0)
            {
                weapon.CurrentCooldown -= deltaTime;
            }
        }

    }
}