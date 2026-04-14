// 路径: Assets/Scripts/ECS/Systems/Combat/DamageSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var victims = GetEntitiesWith<HealthComponent, DamageEventComponent>();

        for (int i = victims.Count - 1; i >= 0; i--)
        {
            var victim = victims[i];
            var hp = victim.GetComponent<HealthComponent>();
            var damageEvt = victim.GetComponent<DamageEventComponent>();
            
            float actualDamage = damageEvt.DamageAmount;

            // 1. 防御减伤结算
            if (victim.HasComponent<MeleeCombatComponent>())
            {
                float def = victim.GetComponent<MeleeCombatComponent>().Defense;
                actualDamage = Mathf.Max(1f, actualDamage - def); 
            }

            hp.CurrentHealth -= actualDamage;

            // 2. 反伤与吸血结算
            Entity attacker = damageEvt.Source; 
            bool causeRecovery = false;
            float durationOverride = 0f;

            if (attacker != null && attacker.IsAlive)
            {
                // 获取攻击者的硬直配置（用于向下游传递）
                if (attacker.HasComponent<ImpactFeedbackComponent>())
                {
                    var feedback = attacker.GetComponent<ImpactFeedbackComponent>();
                    causeRecovery = feedback.CauseHitRecovery;
                    durationOverride = feedback.HitRecoveryDurationOverride;
                }

                if (victim.HasComponent<MeleeCombatComponent>())
                {
                    var melee = victim.GetComponent<MeleeCombatComponent>();
                    if (melee.ThornDamage > 0)
                    {
                        var attackerHp = attacker.GetComponent<HealthComponent>();
                        if (attackerHp != null)
                        {
                            attackerHp.CurrentHealth -= melee.ThornDamage;
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

                if (attacker.HasComponent<MeleeCombatComponent>())
                {
                    var attackerMelee = attacker.GetComponent<MeleeCombatComponent>();
                    var attackerHp = attacker.GetComponent<HealthComponent>();
                    if (attackerHp != null && attackerMelee.LifeStealRatio > 0 && actualDamage > 0)
                    {
                        float healAmount = actualDamage * attackerMelee.LifeStealRatio;
                        attackerHp.CurrentHealth = Mathf.Min(attackerHp.MaxHealth, attackerHp.CurrentHealth + healAmount);
                    }
                }
            }

            // ==========================================
            // 【核心修复】：向下游抛出受击反应事件
            // 确保玩家能获得无敌帧，怪物能触发受击硬直！
            // ==========================================
            victim.RemoveComponent<DamageEventComponent>();

            if (!victim.HasComponent<DamageTakenEventComponent>())
            {
                // 从事件池取出一个反应事件并挂载
                victim.AddComponent(EventPool.GetDamageEvent(actualDamage, causeRecovery, durationOverride));
            }
            else
            {
                var existing = victim.GetComponent<DamageTakenEventComponent>();
                existing.DamageAmount += actualDamage;
                existing.CauseHitRecovery = existing.CauseHitRecovery || causeRecovery;
                existing.RecoveryDurationOverride = Mathf.Max(existing.RecoveryDurationOverride, durationOverride);
            }
            
            // 死亡判定
            if (hp.CurrentHealth <= 0 && !victim.HasComponent<DeadTag>())
            {
                victim.AddComponent(new DeadTag());
            }
        }
    }
}