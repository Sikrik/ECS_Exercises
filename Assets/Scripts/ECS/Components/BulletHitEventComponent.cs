using UnityEngine;

/// <summary>
/// 命中事件组件：当物理系统检测到碰撞时，给子弹挂载此组件。
/// 它不具备持久性，处理完逻辑后随子弹销毁。
/// </summary>
public class BulletHitEventComponent : Component 
{
    public Entity Target; // 命中的目标敌人
    public BulletHitEventComponent(Entity target) => Target = target;
}