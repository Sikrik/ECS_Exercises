// 路径: Assets/Scripts/ECS/Systems/Combat/DamageSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 获取所有正在受到伤害的实体
        var victims = GetEntitiesWith<HealthComponent, DamageEventComponent>();

        for (int i = victims.Count - 1; i >= 0; i--)
        {
            var victim = victims[i];
            var hp = victim.GetComponent<HealthComponent>();
            var damageEvt = victim.GetComponent<DamageEventComponent>();
            
            float actualDamage = damageEvt.DamageAmount;

            // ==========================================
            // 1. 防御减伤结算 (Defense)
            // ==========================================
            if (victim.HasComponent<MeleeCombatComponent>())
            {
                float def = victim.GetComponent<MeleeCombatComponent>().Defense;
                // 减去防御力，但保证至少造成 1 点伤害
                actualDamage = Mathf.Max(1f, actualDamage - def); 
            }

            // 执行扣血
            hp.CurrentHealth -= actualDamage;

            // ==========================================
            // 2. 反伤结算 (Thorns)
            // ==========================================
            Entity attacker = damageEvt.Source; 
            if (victim.HasComponent<MeleeCombatComponent>() && attacker != null && attacker.IsAlive)
            {
                var melee = victim.GetComponent<MeleeCombatComponent>();
                if (melee.ThornDamage > 0)
                {
                    var attackerHp = attacker.GetComponent<HealthComponent>();
                    if (attackerHp != null)
                    {
                        attackerHp.CurrentHealth -= melee.ThornDamage;
                        
                        // 抛出反伤特效事件
                        var pos = victim.GetComponent<PositionComponent>();
                        if (pos != null) {
                            Entity vfx = ECSManager.Instance.CreateEntity();
                            vfx.AddComponent(new VFXSpawnEventComponent { 
                                VFXType = "ThornReflect", 
                                Position = new Vector3(pos.X, pos.Y, 0) 
                            });
                        }
                    }
                }
            }

            // ==========================================
            // 3. 全局吸血 (Global Life Steal)
            // 只要伤害来源拥有 MeleeCombatComponent，任何伤害都能吸血 (包括子弹、AOE等)
            // ==========================================
            if (attacker != null && attacker.IsAlive && attacker.HasComponent<MeleeCombatComponent>())
            {
                var attackerMelee = attacker.GetComponent<MeleeCombatComponent>();
                var attackerHp = attacker.GetComponent<HealthComponent>();
                
                if (attackerHp != null && attackerMelee.LifeStealRatio > 0 && actualDamage > 0)
                {
                    float healAmount = actualDamage * attackerMelee.LifeStealRatio;
                    attackerHp.CurrentHealth = Mathf.Min(attackerHp.MaxHealth, attackerHp.CurrentHealth + healAmount);
                }
            }

            // 结算完毕，移除伤害事件
            victim.RemoveComponent<DamageEventComponent>();
            
            // 死亡判定标签 (交由后续 DeathCleanupSystem 处理)
            if (hp.CurrentHealth <= 0 && !victim.HasComponent<DeadTag>())
            {
                victim.AddComponent(new DeadTag());
            }
        }
    }
}