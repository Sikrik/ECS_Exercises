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
        // PlayerFactory.cs
        player.AddComponent(new SpeedComponent(config.PlayerMoveSpeed)); // 玩家也有了自己的速度组件
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new HealthComponent(config.PlayerMaxHealth));
        player.AddComponent(new ViewComponent(go, prefab));
        // 赋予玩家质量 (100)
        player.AddComponent(new MassComponent(100f)); 
        // 补上弹性标签，让玩家也能被物理弹开
        player.AddComponent(new BouncyTag());
        // 关键标记：触发后续的物理烘焙与视图注册
        player.AddComponent(new NeedsBakingTag());
        
        // 设置玩家物理层级过滤：只撞敌人
        player.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));

        return player;
    }
}