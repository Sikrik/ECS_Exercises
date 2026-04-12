using System.Collections.Generic;
using UnityEngine;

public class WeaponFiringSystem : SystemBase
{
    public WeaponFiringSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var firingEntities = GetEntitiesWith<WeaponComponent, FireIntentComponent, PositionComponent>();

        for (int i = firingEntities.Count - 1; i >= 0; i--)
        {
            var entity = firingEntities[i];
            var weapon = entity.GetComponent<WeaponComponent>();
            var intent = entity.GetComponent<FireIntentComponent>();
            var pos = entity.GetComponent<PositionComponent>();
            
            // 读取武器修饰器（如果没有则视为默认状态）
            var modifiers = entity.HasComponent<WeaponModifierComponent>() 
                ? entity.GetComponent<WeaponModifierComponent>() 
                : null;

            var factionComp = entity.GetComponent<FactionComponent>();
            FactionType faction = factionComp != null ? factionComp.Value : FactionType.Player;

            if (weapon.CurrentCooldown <= 0f)
            {
                Vector3 spawnPos = new Vector3(pos.X, pos.Y, 0);
                
                // 计算多重射击数量与扇形散布
                int projectileCount = 1 + (modifiers != null ? modifiers.ExtraProjectiles : 0);
                float spreadAngle = 15f; // 每颗子弹之间的夹角
                float startAngle = -spreadAngle * (projectileCount - 1) / 2f;
                Vector2 baseDir = intent.AimDirection;

                for (int j = 0; j < projectileCount; j++)
                {
                    float currentAngle = startAngle + j * spreadAngle;
                    Vector2 finalDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;
                    
                    // 将修饰器传入工厂
                    BulletFactory.Create(weapon.CurrentBulletType, spawnPos, finalDir, faction, modifiers);
                }

                // 结算射速提升修饰
                float rateMult = modifiers != null ? modifiers.FireRateMultiplier : 1f;
                weapon.CurrentCooldown = weapon.FireRate * rateMult;
            }

            entity.RemoveComponent<FireIntentComponent>();
        }
    }
}