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

            // 双重保险：就算近战不小心拿到了开火意图，也直接掐灭，绝不射出子弹！
            if (entity.HasComponent<MeleeCombatComponent>())
            {
                entity.RemoveComponent<FireIntentComponent>();
                continue;
            }

            var weapon = entity.GetComponent<WeaponComponent>();
            var intent = entity.GetComponent<FireIntentComponent>();
            var pos = entity.GetComponent<PositionComponent>();
            
            var modifiers = entity.HasComponent<WeaponModifierComponent>() 
                ? entity.GetComponent<WeaponModifierComponent>() 
                : null;

            var factionComp = entity.GetComponent<FactionComponent>();
            FactionType faction = factionComp != null ? factionComp.Value : FactionType.Player;

            if (weapon.CurrentCooldown <= 0f)
            {
                Vector3 spawnPos = new Vector3(pos.X, pos.Y, 0);
                
                // 【修复】正确读取散弹数量
                int multiShotLevel = modifiers != null ? modifiers.GetLevel("AddProjectile") : 0;
                int projectileCount = 1 + multiShotLevel;
                
                float spreadAngle = 15f; 
                float startAngle = -spreadAngle * (projectileCount - 1) / 2f;
                Vector2 baseDir = intent.AimDirection;

                for (int j = 0; j < projectileCount; j++)
                {
                    float currentAngle = startAngle + j * spreadAngle;
                    Vector2 finalDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;
                    
                    BulletFactory.Create(weapon.CurrentBulletType, spawnPos, finalDir, faction, modifiers);
                }

                // 【修复】正确读取攻速提升
                float fireRateLevel = modifiers != null ? modifiers.GetLevel("IncreaseFireRate") : 0f;
                float cooldownReduction = Mathf.Min(0.8f, fireRateLevel * 0.1f); 
                weapon.CurrentCooldown = weapon.FireRate * (1f - cooldownReduction);

                Entity audioEvent = ECSManager.Instance.CreateEntity();
                audioEvent.AddComponent(new AudioPlayEventComponent("Shoot"));
            }

            // 射击完成，正常移除意图
            entity.RemoveComponent<FireIntentComponent>();
        }
    }
}