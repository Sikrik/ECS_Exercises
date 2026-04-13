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
            
            var modifiers = entity.HasComponent<WeaponModifierComponent>() 
                ? entity.GetComponent<WeaponModifierComponent>() 
                : null;

            var factionComp = entity.GetComponent<FactionComponent>();
            FactionType faction = factionComp != null ? factionComp.Value : FactionType.Player;

            if (weapon.CurrentCooldown <= 0f)
            {
                Vector3 spawnPos = new Vector3(pos.X, pos.Y, 0);
                
                // 【重构】从动态字典中提取多重射击技能等级
                int multiShotLevel = modifiers != null ? modifiers.GetLevel("MultiShot") : 0;
                int projectileCount = 1 + multiShotLevel;
                
                float spreadAngle = 15f; // 每颗子弹之间的夹角
                float startAngle = -spreadAngle * (projectileCount - 1) / 2f;
                Vector2 baseDir = intent.AimDirection;

                for (int j = 0; j < projectileCount; j++)
                {
                    float currentAngle = startAngle + j * spreadAngle;
                    Vector2 finalDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;
                    
                    BulletFactory.Create(weapon.CurrentBulletType, spawnPos, finalDir, faction, modifiers);
                }

                // 【重构】从动态字典中提取射速提升等级 (例如：每升一级缩短 10% 冷却)
                float fireRateLevel = modifiers != null ? modifiers.GetLevel("FireRateUp") : 0f;
                float cooldownReduction = Mathf.Min(0.8f, fireRateLevel * 0.1f); // 最多减免 80%
                
                weapon.CurrentCooldown = weapon.FireRate * (1f - cooldownReduction);

                // ==========================================
                // 【新增】发送开火音效事件
                // ==========================================
                Entity audioEvent = ECSManager.Instance.CreateEntity();
                audioEvent.AddComponent(new AudioPlayEventComponent("Shoot"));
                audioEvent.AddComponent(new PendingDestroyComponent()); // 确保生命周期安全
            }

            entity.RemoveComponent<FireIntentComponent>();
        }
    }
}