using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 数据同步系统 (位于 PresentationSystemGroup)
/// 职责：扫描单帧 UI 事件标签，并调用 UIManager 更新实际的画面。
/// 优化：采用脏标记机制，只有数据变化时才触发 UI 更新，维持 0 GC。
/// </summary>
public class UISyncSystem : SystemBase
{
    private int _lastScore = -1;       // 缓存上一帧的分数
    private int _lastEnemyCount = -1;  // 缓存上一帧的敌人计数

    public UISyncSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        if (UIManager.Instance == null) return;

        // ==========================================
        // 1. 处理玩家血量刷新事件 (基于单帧组件标签，用于全局屏幕UI)
        // ==========================================
        var healthEvents = GetEntitiesWith<PlayerTag, HealthComponent, UIHealthUpdateEvent>();
        foreach (var entity in healthEvents)
        {
            var health = entity.GetComponent<HealthComponent>();
            
            // 调用 UIManager 提供的纯渲染接口
            UIManager.Instance.UpdateHealth(health.CurrentHealth, health.MaxHealth);
            
            // 消费完毕后立即移除单帧标签，防止下一帧重复执行
            entity.RemoveComponent<UIHealthUpdateEvent>();
        }

        // ==========================================
        // 2. 处理游戏结束事件
        // ==========================================
        var gameOverEvents = GetEntitiesWith<GameOverEventComponent>();
        foreach (var entity in gameOverEvents)
        {
            // 弹出结算面板并显示最终分数
            UIManager.Instance.ShowGameOver(ECSManager.Instance.Score);
            
            // 标记该事件实体在帧末销毁
            entity.AddComponent(new PendingDestroyComponent());
        }

        // ==========================================
        // 3. 同步得分变化 (基于脏标记判断)
        // ==========================================
        if (ECSManager.Instance.Score != _lastScore)
        {
            UIManager.Instance.UpdateScore(ECSManager.Instance.Score);
            _lastScore = ECSManager.Instance.Score;
        }

        // ==========================================
        // 4. 同步在场敌人计数
        // ==========================================
        var enemies = GetEntitiesWith<EnemyTag>();
        int currentEnemyCount = enemies.Count;

        if (currentEnemyCount != _lastEnemyCount)
        {
            UIManager.Instance.UpdateEnemyCount(currentEnemyCount);
            _lastEnemyCount = currentEnemyCount;
        }

        // ==========================================
        // 5. 同步玩家随身 HUD (血条、冲刺 CD、方向指示箭头)
        // ==========================================
        // 【注意】这里在查询参数中新增了 VelocityComponent 来获取实时速度
        var hudEntities = GetEntitiesWith<PlayerHUDComponent, HealthComponent, DashAbilityComponent, VelocityComponent>();
        foreach (var entity in hudEntities)
        {
            var hud = entity.GetComponent<PlayerHUDComponent>();
            var hp = entity.GetComponent<HealthComponent>();
            var dash = entity.GetComponent<DashAbilityComponent>();
            var vel = entity.GetComponent<VelocityComponent>();

            // 5.1 同步 360 度圆环血量
            if (hud.HealthRing != null)
            {
                hud.HealthRing.fillAmount = hp.MaxHealth > 0 ? hp.CurrentHealth / hp.MaxHealth : 0;
            }

            // 5.2 同步冲刺 CD
            if (hud.FlashIcon != null)
            {
                if (dash.CurrentCD > 0)
                {
                    // 正在冷却中：用 1 减去剩余比例
                    // 效果：刚冲刺完变空(0)，然后随着冷却慢慢涨满到(1)
                    hud.FlashIcon.fillAmount = 1f - (dash.CurrentCD / dash.Cooldown);
                }
                else
                {
                    // 冷却完毕：设为 1f，让闪电完全亮起常驻！
                    hud.FlashIcon.fillAmount = 1f; 
                }
            }

            
            // 5.3 【新增】同步前进方向箭头 (冰块级丝滑延迟)
            if (hud.ArrowPivot != null && vel != null)
            {
                // 只有当玩家在移动（存在物理速度）时才计算目标方向
                if (vel.VX != 0 || vel.VY != 0)
                {
                    // 1. 计算出理论上的“目标角度”
                    float targetAngle = Mathf.Atan2(vel.VY, vel.VX) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                    
                    // 2. 使用 Slerp 进行平滑过渡
                    // 【核心修改】：把原来的 15f 改成 4f！
                    // 数值越小（比如 2f、3f），箭头转得越慢，那种“冰面打滑、悠悠转过去”的阻尼感就越强。
                    hud.ArrowPivot.localRotation = Quaternion.Slerp(hud.ArrowPivot.localRotation, targetRotation, deltaTime * 4f);
                }
            }
        }

        // 养成好习惯：显式调用 ReturnListToPool 维持闭环
        ReturnListToPool(enemies);
        ReturnListToPool(healthEvents);
        ReturnListToPool(gameOverEvents);
        ReturnListToPool(hudEntities);
    }
}