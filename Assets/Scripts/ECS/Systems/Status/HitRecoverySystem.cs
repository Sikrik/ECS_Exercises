using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 受击硬直系统：负责扣减硬直时间，并处理受击期间的视觉反馈（打击感）
/// </summary>
public class HitRecoverySystem : SystemBase
{
    public HitRecoverySystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取所有处于硬直状态的实体
        var entities = GetEntitiesWith<HitRecoveryComponent, ViewComponent, BaseColorComponent>();

        // 倒序遍历，安全移除组件
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var recovery = entity.GetComponent<HitRecoveryComponent>();
            var view = entity.GetComponent<ViewComponent>();
            var baseColor = entity.GetComponent<BaseColorComponent>();

            // 1. 扣减硬直时间
            recovery.Timer -= deltaTime;

            if (recovery.Timer > 0)
            {
                // 2. 状态维持与表现：高频白红闪烁（Game Feel 增强）
                if (view.SpriteRenderer != null)
                {
                    // 使用 PingPong 制作高频的受击闪烁效果，让打击感更脆
                    float lerp = Mathf.PingPong(Time.time * 50f, 1f);
                    view.SpriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.5f), lerp);
                }
            }
            else
            {
                // 3. 硬直结束：恢复原状
                if (view.SpriteRenderer != null)
                {
                    view.SpriteRenderer.color = baseColor.Color; // 恢复基础颜色
                }
                
                // 移除硬直组件。下一帧 EnemyTrackingSystem 发现它没有这个组件了，就会自动重新开始寻路！
                entity.RemoveComponent<HitRecoveryComponent>();
            }
        }
    }
}