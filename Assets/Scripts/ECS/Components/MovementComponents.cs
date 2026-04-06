public class PositionComponent : Component 
{
    public float X, Y, Z;
    public PositionComponent(float x, float y, float z) { X = x; Y = y; Z = z; }
}

public class VelocityComponent : Component 
{
    public float VX; // 必须大写，与系统逻辑一致
    public float VY;
    public VelocityComponent(float vx, float vy) { VX = vx; VY = vy; }
}

public class TraceComponent : Component 
{
    public float PreviousX, PreviousY; // 用于高速物体防穿透检测
    public TraceComponent(float x, float y) { PreviousX = x; PreviousY = y; }
}

// 存储移动意图，不代表最终速度
public class MoveInputComponent : Component 
{
    public float X;
    public float Y;
    public MoveInputComponent(float x, float y) { X = x; Y = y; }
}
