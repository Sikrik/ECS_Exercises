using System;

/// <summary>
/// 敌人配置模板：用于定义一种敌人的静态数据和特性开关
/// </summary>
[Serializable]
public class EnemyTemplate
{
    public float Health;       // 基础血量
    public float Speed;        // 移动速度
    public int Damage;         // 碰撞伤害
    
    // 特性开关（决定挂载哪些组件）
    public bool IsBouncy;      // 是否具有弹性（决定是否挂载 BouncyTag）
    
    // 构造函数方便代码内快速创建，也可以配合 CSV 加载
    public EnemyTemplate(float hp, float spd, int dmg, bool bouncy)
    {
        Health = hp;
        Speed = spd;
        Damage = dmg;
        IsBouncy = bouncy;
    }
}