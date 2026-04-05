// 基础伤害组件
public class DamageComponent : Component {
    public float Value;
    public DamageComponent(float value) => Value = value;
}

// 爆炸(AOE)组件
public class AOEComponent : Component {
    public float Radius;
    public float Damage;
    public AOEComponent(float r, float d) { Radius = r; Damage = d; }
}

// 闪电链组件
public class ChainComponent : Component {
    public int MaxTargets;
    public float Range;
    public float Damage;
    public ChainComponent(int count, float r, float d) { MaxTargets = count; Range = r; Damage = d; }
}