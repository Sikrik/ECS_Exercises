public class AOEComponent : Component 
{
    public float Radius;

    public AOEComponent(float r)
    {
        Radius = r;
    }
}

public class ChainComponent : Component 
{
    public int MaxTargets;
    public float Range;

    public ChainComponent(int m, float r) { MaxTargets = m; Range = r;  }
}