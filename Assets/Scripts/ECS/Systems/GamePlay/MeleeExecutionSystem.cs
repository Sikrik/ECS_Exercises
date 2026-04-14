using System.Collections.Generic;
using UnityEngine;

public class MeleeExecutionSystem : SystemBase
{
    public MeleeExecutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var attackers = GetEntitiesWith<MeleeSwingIntentComponent, MeleeCombatComponent, PositionComponent>();

        for (int i = attackers.Count - 1; i >= 0; i--)
        {
            var p = attackers[i];
            var melee = p.GetComponent<MeleeCombatComponent>();
            var pPos = p.GetComponent<PositionComponent>();
            var intent = p.GetComponent<MeleeSwingIntentComponent>();
            var modifiers = p.HasComponent<WeaponModifierComponent>() ? p.GetComponent<WeaponModifierComponent>() : null;
            
            int executeLvl = modifiers != null ? modifiers.GetLevel("Melee_Execute") : 0;
            int waveLvl = modifiers != null ? modifiers.GetLevel("Melee_Wave") : 0;

            // 获取面向方向 (如果没移动，默认朝右)
            Vector2 aimDir = Vector2.right;
            if (p.HasComponent<MoveInputComponent>())
            {
                var input = p.GetComponent<MoveInputComponent>();
                if (Mathf.Abs(input.X) > 0.01f || Mathf.Abs(input.Y) > 0.01f) 
                    aimDir = new Vector2(input.X, input.Y).normalized;
            }

            // 读取冲刺意图覆盖的半径和角度 (360度大回旋斩)
            float attackAngle = intent != null && intent.AngleOverride > 0 ? intent.AngleOverride : melee.AttackAngle;
            float actualRadius = intent != null ? melee.AttackRadius * intent.RadiusMultiplier : melee.AttackRadius;

            // 1. 本体范围挥砍判定
            var targets = ECSManager.Instance.Grid.GetNearbyEntities(pPos.X, pPos.Y, Mathf.CeilToInt(actualRadius));
            foreach (var e in targets)
            {
                if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
                
                // ==========================================
                // 【核心修复 3】：精确的扇形与距离物理判定
                // ==========================================
                var tPos = e.GetComponent<PositionComponent>();
                Vector2 toTarget = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y);
                
                // 距离判断
                if (toTarget.sqrMagnitude > actualRadius * actualRadius) continue;
                
                // 角度判定 (如果是360度旋风斩则忽略)
                if (attackAngle < 360f)
                {
                    float angleToTarget = Vector2.Angle(aimDir, toTarget);
                    if (angleToTarget > attackAngle / 2f) continue; 
                }

                float dmg = 35f * (modifiers != null ? modifiers.GlobalDamageMultiplier : 1f);
                bool isCrit = false;

                if (executeLvl > 0 && e.HasComponent<HealthComponent>())
                {
                    var hp = e.GetComponent<HealthComponent>();
                    if (hp.CurrentHealth / hp.MaxHealth <= 0.3f) 
                    { 
                        dmg += 50f * executeLvl; 
                        isCrit = true; 
                    }
                }

                // ==========================================
                // 【核心修复 4】：伤害累加，防止吞没同帧子弹伤害
                // ==========================================
                var existingDmg = e.GetComponent<DamageEventComponent>();
                if (existingDmg != null)
                {
                    existingDmg.DamageAmount += dmg;
                }
                else
                {
                    e.AddComponent(new DamageEventComponent { DamageAmount = dmg, Source = p, IsCritical = isCrit });
                }
            }

            // 2. 触发剑气纵横
            if (waveLvl > 0)
            {
                Entity waveBullet = BulletFactory.Create(BulletType.Normal, new Vector3(pPos.X, pPos.Y, 0), aimDir, FactionType.Player, modifiers);
                
                if (waveBullet != null)
                {
                    waveBullet.AddComponent(new PierceComponent(1 + waveLvl * 2)); 
                    waveBullet.AddComponent(new ColorTintComponent(Color.cyan)); 
                }
            }

            p.RemoveComponent<MeleeSwingIntentComponent>();
        }
    }
}