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
    private int _lastWave = -1;        // 缓存上一帧的波次

    public UISyncSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        if (UIManager.Instance == null) return;

        // ==========================================
        // 1. 处理玩家血量刷新事件 (全局屏幕UI)
        // ==========================================
        var healthEvents = GetEntitiesWith<PlayerTag, HealthComponent, UIHealthUpdateEvent>();
        foreach (var entity in healthEvents)
        {
            var health = entity.GetComponent<HealthComponent>();
            UIManager.Instance.UpdateHealth(health.CurrentHealth, health.MaxHealth);
            entity.RemoveComponent<UIHealthUpdateEvent>();
        }

        // ==========================================
        // 2. 处理游戏失败/死亡事件
        // ==========================================
        var gameOverEvents = GetEntitiesWith<GameOverEventComponent>();
        foreach (var entity in gameOverEvents)
        {
            UIManager.Instance.ShowGameOver(ECSManager.Instance.Score);
            entity.AddComponent(new PendingDestroyComponent());
        }

        // ==========================================
        // 3. 处理游戏胜利事件
        // ==========================================
        var victoryEvents = GetEntitiesWith<GameVictoryEventComponent>();
        foreach (var entity in victoryEvents)
        {
            UIManager.Instance.ShowVictory(ECSManager.Instance.Score);
            Time.timeScale = 0; // 胜利后暂停游戏时间
            entity.AddComponent(new PendingDestroyComponent());
        }

        // ==========================================
        // 4. 同步得分变化 (脏标记)
        // ==========================================
        if (ECSManager.Instance.Score != _lastScore)
        {
            UIManager.Instance.UpdateScore(ECSManager.Instance.Score);
            _lastScore = ECSManager.Instance.Score;
        }

        // ==========================================
        // 5. 同步在场敌人计数 (脏标记)
        // ==========================================
        var enemies = GetEntitiesWith<EnemyTag>();
        int currentEnemyCount = enemies.Count;

        if (currentEnemyCount != _lastEnemyCount)
        {
            UIManager.Instance.UpdateEnemyCount(currentEnemyCount);
            _lastEnemyCount = currentEnemyCount;
        }

        // ==========================================
        // 6. 同步波次显示 (脏标记)
        // ==========================================
        if (ECSManager.Instance.CurrentWave != _lastWave)
        {
            UIManager.Instance.UpdateWave(ECSManager.Instance.CurrentWave, ECSManager.Instance.MaxWave);
            _lastWave = ECSManager.Instance.CurrentWave;
        }

        // ==========================================
        // 7. 同步玩家随身 HUD (血条、冲刺 CD)
        // ==========================================
        var hudEntities = GetEntitiesWith<PlayerHUDComponent, HealthComponent, DashAbilityComponent>();
        foreach (var entity in hudEntities)
        {
            var hud = entity.GetComponent<PlayerHUDComponent>();
            var hp = entity.GetComponent<HealthComponent>();
            var dash = entity.GetComponent<DashAbilityComponent>();

            // 7.1 同步 360 度圆环血量
            if (hud.HealthRing != null)
            {
                hud.HealthRing.fillAmount = hp.MaxHealth > 0 ? hp.CurrentHealth / hp.MaxHealth : 0;
            }

            // 7.2 同步冲刺 CD
            if (hud.FlashIcon != null)
            {
                if (dash.CurrentCD > 0)
                {
                    hud.FlashIcon.fillAmount = 1f - (dash.CurrentCD / dash.Cooldown);
                }
                else
                {
                    hud.FlashIcon.fillAmount = 1f; 
                }
            }
        }

        // ==========================================
        // 8. 同步准星 (Crosshair) 状态与位置
        // ==========================================
        // 检测当前是否处于鼠标手动瞄准模式
        bool isManualAim = PlayerAimingSystem.CurrentAimStrategy is ManualAimStrategy;
        
        UIManager.Instance.SetCrosshairActive(isManualAim);

        if (isManualAim)
        {
            // 准星跟随鼠标屏幕坐标
            UIManager.Instance.UpdateCrosshairPosition(Input.mousePosition);
            // 隐藏系统默认鼠标指针
            Cursor.visible = false;
        }
        else
        {
            // 自动瞄准模式下恢复系统鼠标指针
            Cursor.visible = true;
        }

        // ==========================================
        // 9. 处理经验升级事件 (弹出三选一面板)
        // ==========================================
        var levelUpEvents = GetEntitiesWith<PlayerTag, WeaponModifierComponent, LevelUpEventComponent>();
        for (int i = levelUpEvents.Count - 1; i >= 0; i--)
        {
            var entity = levelUpEvents[i];
            
            // 呼叫 UI 表现层处理面板展示
            if (UpgradeUIManager.Instance != null)
            {
                UpgradeUIManager.Instance.ShowUpgradePanel(entity);
            }
            
            // 处理完后立刻撕下单帧标签，防止下一帧重复触发
            entity.RemoveComponent<LevelUpEventComponent>();
        }
    }
}