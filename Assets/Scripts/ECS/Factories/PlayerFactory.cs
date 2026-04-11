using UnityEngine;

public static class PlayerFactory
{
    public static Entity Create(GameObject prefab, GameConfig config)
    {
        var ecs = ECSManager.Instance;
        if (prefab == null) return null;

        // 1. 表现层实例化
        GameObject go = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 2. 逻辑层创建并组装
        Entity player = ecs.CreateEntity();
        
        // ==========================================
        // 挂载 PlayerHUD (纯 UI 显示)
        // ==========================================
        var hudView = go.GetComponent<PlayerHUDView>();
        if (hudView != null && hudView.HealthRing != null && hudView.FlashIcon != null)
        {
            // 已移除 ArrowPivot 参数
            player.AddComponent(new PlayerHUDComponent(hudView.HealthRing, hudView.FlashIcon));
        }
        else
        {
            Debug.LogWarning("Player 预制体上缺少 PlayerHUDView 组件或未绑定相应的 Image 引用！");
        }

        // ==========================================
        // 挂载通用方向指示器 (分离后的解耦逻辑)
        // ==========================================
        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            // 玩家转向灵敏度给 6f
            player.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 6f));
        }

        player.AddComponent(new PlayerTag());
        player.AddComponent(new PositionComponent(0, 0, 0));
        player.AddComponent(new SpeedComponent(config.PlayerMoveSpeed)); 
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new HealthComponent(config.PlayerMaxHealth));
        player.AddComponent(new ViewComponent(go, prefab));
        
        player.AddComponent(new MassComponent(100f)); 
        player.AddComponent(new BouncyTag());
        player.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
        player.AddComponent(new FactionComponent(FactionType.Player));
        
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());
        
        player.AddComponent(new CollisionFilterComponent(0));

        float fireRate = 0.2f; 
        player.AddComponent(new WeaponComponent(BulletType.Normal, fireRate));
        
        player.AddComponent(new DashAbilityComponent(18f, 0.2f, 1.5f));
        player.AddComponent(new UIHealthUpdateEvent()); 

        return player;
    }
}