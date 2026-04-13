// 路径: Assets/Scripts/ECS/Systems/GamePlay/WeaponFiringSystem.cs
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

            // ==========================================
            // 【核心修复 2】：在这里拦截近战！
            // 这样既保留了 UI 的丝滑转向，又绝对不会射出子弹！
            // ==========================================
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
                
                int multiShotLevel = modifiers != null ? modifiers.GetLevel("MultiShot") : 0;
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

                float fireRateLevel = modifiers != null ? modifiers.GetLevel("FireRateUp") : 0f;
                float cooldownReduction = Mathf.Min(0.8f, fireRateLevel * 0.1f); 
                weapon.CurrentCooldown = weapon.FireRate * (1f - cooldownReduction);

                Entity audioEvent = ECSManager.Instance.CreateEntity();
                audioEvent.AddComponent(new AudioPlayEventComponent("Shoot"));
            }

            entity.RemoveComponent<FireIntentComponent>();
        }
    }
}