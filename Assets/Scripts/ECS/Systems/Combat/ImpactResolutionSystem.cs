// 路径: Assets/Scripts/ECS/Systems/Combat/ImpactResolutionSystem.cs
using System.Collections.Generic;

public class ImpactResolutionSystem : SystemBase
{
    public ImpactResolutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var source = evt.Source; // 撞击源（比如子弹、或者是碰到玩家的敌人）
            var target = evt.Target; // 被撞者（比如敌人、玩家）

            // 确保实体都还存活
            if (source == null || !source.IsAlive) continue;
            if (target == null || !target.IsAlive) continue;

            // ==========================================
            // 1. 阵营过滤 (防止子弹出生时秒杀自己或友军)
            // ==========================================
            if (source.HasComponent<FactionComponent>() && target.HasComponent<FactionComponent>())
            {
                if (source.GetComponent<FactionComponent>().Value == target.GetComponent<FactionComponent>().Value)
                {
                    continue; 
                }
            }

            // ==========================================
            // 2. 物理防重叠 (挤压分离)
            // 排除子弹，防止子弹把肉体推走；主要用于肉体之间的碰撞分离
            // ==========================================
            if (!source.HasComponent<BulletTag>())
            {
                var sourcePos = source.GetComponent<PositionComponent>();
                var targetPos = target.GetComponent<PositionComponent>();

                if (sourcePos != null && targetPos != null)
                {
                    // 读取配置表中的挤压距离 (game_config.csv 中配置)
                    float pushDist = BattleManager.Instance.Config.CollisionPushDistance; 
                    
                    // 将主动碰撞方(Source)沿着法线的反方向稍微推开，防止完全黏在一起
                    sourcePos.X -= evt.Normal.x * pushDist;
                    sourcePos.Y -= evt.Normal.y * pushDist;
                }
            }

            // ==========================================
            // 3. 弹性击退反馈 (Knockback)
            // 处理肉搏怪物撞击玩家产生的弹力效果
            // ==========================================
            if (source.HasComponent<ImpactFeedbackComponent>())
            {
                var feedback = source.GetComponent<ImpactFeedbackComponent>();
                
                // 如果碰撞源允许造成弹力，且配置了弹力数值
                if (feedback.CauseBounce && source.HasComponent<BounceForceComponent>())
                {
                    float bounceForce = source.GetComponent<BounceForceComponent>().Value;
                    
                    // 确保目标没有在免控/无敌状态，给目标挂载击退组件
                    if (!target.HasComponent<KnockbackComponent>() && !target.HasComponent<InvincibleComponent>())
                    {
                        target.AddComponent(new KnockbackComponent {
                            DirX = evt.Normal.x,
                            DirY = evt.Normal.y,
                            Speed = bounceForce,
                            Timer = 0.15f, // 击退的滑行持续时间
                            HitRecoveryAfterwards = feedback.HitRecoveryDurationOverride
                        });
                    }
                }
            }

            // ==========================================
            // 4. 产生伤害事件 (兼容近战、远程、敌人触碰)
            // ==========================================
            if (source.HasComponent<DamageComponent>() && target.HasComponent<HealthComponent>())
            {
                // 如果目标正在无敌帧（如刚冲刺或刚受击），则免疫伤害
                if (target.HasComponent<InvincibleComponent>()) continue;

                float actualDmg = source.GetComponent<DamageComponent>().Value;

                // 为了防止多重射击（散弹）在同一帧命中同一个敌人时伤害丢失，我们做伤害叠加
                if (!target.HasComponent<DamageEventComponent>())
                {
                    target.AddComponent(new DamageEventComponent { 
                        DamageAmount = actualDmg, 
                        Source = source, // 记录来源，用于反伤或逻辑判定
                        IsCritical = false 
                    });
                }
                else
                {
                    // 同一帧被多颗子弹命中，伤害累加
                    var dmgEvt = target.GetComponent<DamageEventComponent>();
                    dmgEvt.DamageAmount += actualDmg;
                }

                // 如果是子弹造成的碰撞，打上 HitTag 交给 BulletDestroySystem 处理（穿透或销毁）
                if (source.HasComponent<BulletTag>() && !source.HasComponent<HitTag>())
                {
                    source.AddComponent(new HitTag());
                }
            }
        }
    }
}