using System.Collections.Generic;

public abstract class SystemGroup : SystemBase
{
    protected List<SystemBase> Systems = new List<SystemBase>();

    public SystemGroup(List<Entity> entities) : base(entities) { }

    public void AddSystem(SystemBase system)
    {
        Systems.Add(system);
    }

    public override void Update(float deltaTime)
    {
        // 按添加顺序，依次更新组内的子系统
        for (int i = 0; i < Systems.Count; i++)
        {
            Systems[i].Update(deltaTime);
        }
    }
}

// 定义具体的生命周期组
public class InitializationSystemGroup : SystemGroup { public InitializationSystemGroup(List<Entity> e) : base(e) {} }
public class SimulationSystemGroup : SystemGroup { public SimulationSystemGroup(List<Entity> e) : base(e) {} }
public class PresentationSystemGroup : SystemGroup { public PresentationSystemGroup(List<Entity> e) : base(e) {} }
public class CleanupSystemGroup : SystemGroup { public CleanupSystemGroup(List<Entity> e) : base(e) {} }