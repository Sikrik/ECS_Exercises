// 监听每次伤害事件，呼叫 UI 弹出伤害数字

using System.Numerics;

public class DamageTextSystem : SystemBase
{
    public DamageTextSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var damages = GetEntitiesWith<DamageEventComponent, PositionComponent>();
        foreach (var e in damages)
        {
            var dmg = e.GetComponent<DamageEventComponent>();
            var pos = e.GetComponent<PositionComponent>();

            // 呼叫 UIManager 或 飘字对象池 生成飘字
            // 如果 isCritical 为 true，让字体变大并变成红色/黄色
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowDamageText(new Vector2(pos.X, pos.Y), dmg.DamageAmount, dmg.IsCritical);
            }
        }
    }
}