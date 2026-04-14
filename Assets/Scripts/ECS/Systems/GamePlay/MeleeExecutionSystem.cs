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
            
            // 提取攻击参数
            float radius = melee.AttackRadius * intent.RadiusMultiplier;
            float angle = intent.AngleOverride > 0 ? intent.AngleOverride : melee.AttackAngle;
            
            int executeLvl = modifiers != null ? modifiers.GetLevel("Melee_Execute") : 0;
            int waveLvl = modifiers != null ? modifiers.GetLevel("Melee_Wave") : 0;

            // 获取玩家当前朝向 (根据最后一次输入)
            Vector2 forwardDir = Vector2.right;
            if (p.HasComponent<MoveInputComponent>())
            {
                var input = p.GetComponent<MoveInputComponent>();
                Vector2 aimDir = new Vector2(input.X, input.Y);
                if (aimDir.sqrMagnitude > 0.001f) forwardDir = aimDir.normalized;
            }

            // ==========================================
            // 1. 触发特效表现
            // ==========================================
            Entity vfxEvent = ECSManager.Instance.CreateEntity();
            vfxEvent.AddComponent(new VFXSpawnEventComponent {
                VFXType = "MeleeSlash",
                Position = new Vector3(pPos.X, pPos.Y, 0),
                EndPosition = new Vector3(pPos.X + forwardDir.x * radius, pPos.Y + forwardDir.y * radius, 0),
                NumericParam = angle // 将扇形角度传递给特效网格生成器
            });

            // ==========================================
            // 2. 扇形范围挥砍判定与结算
            // ==========================================
            var targets = ECSManager.Instance.Grid.GetNearbyEntities(pPos.X, pPos.Y, Mathf.CeilToInt(radius));
            foreach (var e in targets)
            {
                if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
                
                var ePos = e.GetComponent<PositionComponent>();
                Vector2 toEnemy = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y);
                
                // 【核心修复】：角度过滤，排除背后的敌人（除非是360度大风车）
                if (angle < 360f && Vector2.Angle(forwardDir, toEnemy) > angle / 2f) 
                    continue;

                // 伤害计算
                float damageBonus = modifiers != null ? (1f + modifiers.GetLevel("Melee_IncreaseDamage") * 0.2f) : 1f;
                float dmg = 35f * damageBonus * (modifiers != null ? modifiers.GlobalDamageMultiplier : 1f);
                bool isCrit = false;

                // 斩杀逻辑
                if (executeLvl > 0 && e.HasComponent<HealthComponent>())
                {
                    var hp = e.GetComponent<HealthComponent>();
                    if (hp.CurrentHealth / hp.MaxHealth <= 0.3f) 
                    { 
                        dmg += 50f * executeLvl; 
                        isCrit = true; 
                    }
                }
                
                // 击退逻辑增强
                if (modifiers != null && modifiers.GetLevel("Melee_Knockback") > 0)
                {
                    // 给玩家自身贴上反馈组件，DamageSystem 结算时会提取并施加给受害者
                    p.AddComponent(new ImpactFeedbackComponent(true, true));
                    p.AddComponent(new BounceForceComponent(8f + modifiers.GetLevel("Melee_Knockback") * 2f));
                }

                e.AddComponent(new DamageEventComponent { DamageAmount = dmg, Source = p, IsCritical = isCrit });
            }

            // ==========================================
            // 3. 剑气纵横 (发射穿透子弹)
            // ==========================================
            if (waveLvl > 0)
            {
                Entity waveBullet = BulletFactory.Create(BulletType.Normal, new Vector3(pPos.X, pPos.Y, 0), forwardDir, FactionType.Player, modifiers);
                if (waveBullet != null)
                {
                    waveBullet.AddComponent(new PierceComponent(1 + waveLvl * 2)); 
                    waveBullet.AddComponent(new ColorTintComponent(Color.cyan)); 
                }
            }

            // ==========================================
            // 4. 状态清理
            // ==========================================
            p.RemoveComponent<MeleeSwingIntentComponent>();
            if (p.HasComponent<ImpactFeedbackComponent>()) p.RemoveComponent<ImpactFeedbackComponent>();
            if (p.HasComponent<BounceForceComponent>()) p.RemoveComponent<BounceForceComponent>();
        }
    }
}