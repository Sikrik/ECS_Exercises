using UnityEngine;

public static class EnemyBuilderExtensions
{
    public static Entity AsEnemy(this Entity entity)
    {
        entity.AddComponent(new EnemyTag());
        return entity;
    }

    public static Entity WithBaseView(this Entity entity, GameObject go, GameObject prefab, Vector3 pos)
    {
        entity.AddComponent(new ViewComponent(go, prefab));
        entity.AddComponent(new PositionComponent(pos.x, pos.y, 0));
        entity.AddComponent(new VelocityComponent(0, 0));
        entity.AddComponent(new NeedsBakingTag());
        entity.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player")));
        return entity;
    }

    public static Entity WithBouncy(this Entity entity)
    {
        entity.AddComponent(new BouncyTag());
        return entity;
    }
}