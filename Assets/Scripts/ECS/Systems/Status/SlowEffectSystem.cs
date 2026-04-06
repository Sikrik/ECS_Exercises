using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 减速效果系统：负责维持减速状态、更新计时器以及处理冰冻视觉效果
/// </summary>
public class SlowEffectSystem : SystemBase
{
    // 定义冰蓝色常量
    private readonly Color IceBlue = new Color(0.4f, 0.7f, 1.0f, 1.0f);

    public SlowEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出所有被减速且拥有视图的实体
        var entities = GetEntitiesWith<SlowEffectComponent, ViewComponent>();

        foreach (var entity in entities)
        {
            var slow = entity.GetComponent<SlowEffectComponent>();
            var view = entity.GetComponent<ViewComponent>();

            // 安全获取 SpriteRenderer
            SpriteRenderer sr = null;
            if (view.GameObject != null)
            {
                view.GameObject.TryGetComponent(out sr);
            }

            if (sr != null)
            {
                // --- 核心修复：记录基础颜色 (懒加载) ---
                // 如果实体还没有基础颜色记录，说明当前 sr.color 是它最原始的状态
                if (!entity.HasComponent<BaseColorComponent>())
                {
                    entity.AddComponent(new BaseColorComponent(sr.color));
                }
                
                // 只要减速未结束，维持冰蓝色
                sr.color = IceBlue;
            }

            // 更新计时器
            slow.RemainingDuration -= deltaTime;

            // 效果结束后的清理工作
            if (slow.RemainingDuration <= 0)
            {
                // --- 核心修复：从 BaseColorComponent 恢复原始颜色 ---
                if (sr != null && entity.HasComponent<BaseColorComponent>())
                {
                    sr.color = entity.GetComponent<BaseColorComponent>().Value;
                }

                // 清理关联的冰冻特效对象
                if (entity.HasComponent<AttachedVFXComponent>())
                {
                    var vfxComp = entity.GetComponent<AttachedVFXComponent>();
                    if (vfxComp.EffectObject != null)
                    {
                        // 使用对象池回收特效，而不是直接销毁
                        PoolManager.Instance.Despawn(PoolManager.Instance.SlowVFXPrefab, vfxComp.EffectObject);
                    }
                    entity.RemoveComponent<AttachedVFXComponent>();
                }

                // 移除减速组件
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}