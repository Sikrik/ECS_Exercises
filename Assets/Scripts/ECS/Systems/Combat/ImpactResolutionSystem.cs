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
            // 【核心修复】：阵营过滤 (防止子弹出生时秒杀自己)
            // ==========================================
            if (source.HasComponent<FactionComponent>() && target.HasComponent<FactionComponent>())
            {
                // 如果是同阵营（比如 玩家子弹 碰到了 玩家），直接忽略此次碰撞！
                if (source.GetComponent<FactionComponent>().Value == target.GetComponent<FactionComponent>().Value)
                {
                    continue; 
                }
            }

            // ==========================================
            // 产生伤害事件 (兼容近战、远程、敌人触碰)
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