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
        player.AddComponent(new PlayerTag());
        player.AddComponent(new PositionComponent(0, 0, 0));
        player.AddComponent(new SpeedComponent(config.PlayerMoveSpeed)); 
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new HealthComponent(config.PlayerMaxHealth));
        player.AddComponent(new ViewComponent(go, prefab));
        
        // 赋予玩家质量与弹性
        player.AddComponent(new MassComponent(100f)); 
        player.AddComponent(new BouncyTag());
        
        // 【核心修复 1】：补充碰撞反馈组件，让玩家在主动撞击时能产生物理排斥力挤开怪物
        player.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
        
        // 关键标记：触发后续的物理烘焙与视图注册
        player.AddComponent(new NeedsBakingTag());
        
        // 【核心修复 3】：取消玩家主动检测怪物的权限，防止同一帧内触发双重排斥导致怪物抖动/乱飞。
        // （掩码设为 0。如果你的游戏后续有拾取物，可以改为 LayerMask.GetMask("Item")）
        player.AddComponent(new CollisionFilterComponent(0));

        // 进入游戏第一帧强制刷新一次血条
        player.AddComponent(new UIHealthUpdateEvent()); 

        return player;
    }
}