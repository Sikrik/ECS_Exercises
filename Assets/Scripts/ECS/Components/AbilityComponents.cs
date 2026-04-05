public class AOEComponent : Component 
{
    public float Radius;
    public float Damage;
    public AOEComponent(float r, float d) { Radius = r; Damage = d; }
}

public class ChainComponent : Component 
{
    public int MaxTargets;
    public float Range;
    public float Damage;
    public ChainComponent(int m, float r, float d) { MaxTargets = m; Range = r; Damage = d; }
}