using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 纯粹的伤害结算系统 (高内聚改造版)
/// 职责：只处理数值扣减，绝不插手任何物理击退逻辑或子弹销毁。
/// </summary>
public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;
            var source = evt.Source;

            if (target == null || !target.IsAlive || source == null || !source.IsAlive) continue;
            
            // 全局无敌帧保护
            if (target.HasComponent<InvincibleComponent>()) continue; 

            // 阵营免伤保护
            var sourceFac = source.GetComponent<FactionComponent>();
            var targetFac = target.GetComponent<FactionComponent>();
            if (sourceFac != null && targetFac != null && sourceFac.Value == targetFac.Value)
            {
                continue;
            }

            // 纯粹的扣血逻辑
            if (target.HasComponent<HealthComponent>() && source.HasComponent<DamageComponent>())
            {
                var hp = target.GetComponent<HealthComponent>();
                var dmg = source.GetComponent<DamageComponent>();
    
                // 1. 扣除血量
                hp.CurrentHealth -= dmg.Value;
                Debug.Log($"{target} 扣除血量：{dmg.Value}");

                // 2. 读取攻击源的物理反馈配置，判断是否该造成硬直，以及覆盖的硬直时间
                bool causeRecovery = false;
                float durationOverride = 0f;
                var feedback = source.GetComponent<ImpactFeedbackComponent>();
                if (feedback != null) 
                {
                    causeRecovery = feedback.CauseHitRecovery;
                    durationOverride = feedback.HitRecoveryDurationOverride;
                }

                // 3. 防覆盖与内存泄漏处理（合并状态与时间）
                var existingEvt = target.GetComponent<DamageTakenEventComponent>();
                if (existingEvt != null)
                {
                    existingEvt.DamageAmount += dmg.Value;
                    existingEvt.CauseHitRecovery = existingEvt.CauseHitRecovery || causeRecovery;
                    // 取两者的最大硬直时间叠加
                    existingEvt.RecoveryDurationOverride = Mathf.Max(existingEvt.RecoveryDurationOverride, durationOverride);
                }
                else
                {
                    target.AddComponent(EventPool.GetDamageEvent(dmg.Value, causeRecovery, durationOverride));
                }
            }
        }
    }
}