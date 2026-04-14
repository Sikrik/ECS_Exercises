using DOTS.Compenonts;

namespace DOTS.Authorings
{
    using Unity.Entities;
    using UnityEngine;

    public class PlayerAuthoring : MonoBehaviour
    {
        // 在 Inspector 面板中暴露给策划调配的参数
        public float MoveSpeed = 5f;

        // Baker 类负责将 MonoBehaviour 转换成 Entity
        class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                // TransformUsageFlags.Dynamic 表示这个实体是会移动的
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // 给这个实体挂载我们刚才定义的组件
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, new MoveSpeed { Value = authoring.MoveSpeed });
                AddComponent<PlayerInput>(entity); // 初始输入默认为 (0,0)
            }
        }
    }
}