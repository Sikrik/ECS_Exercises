public class VelocityComponent : Component 
{
    public float VX; // 必须大写，与系统逻辑一致
    public float VY;
    public VelocityComponent(float vx, float vy) { VX = vx; VY = vy; }
}
public class SpeedComponent : Component 
{
    public float BaseSpeed;    // 基础速度（来自配置）
    public float CurrentSpeed; // 当前速度（计算减速后的实时数值）

    public SpeedComponent(float speed) 
    { 
        BaseSpeed = speed; 
        CurrentSpeed = speed; 
    }
}
public class TraceComponent : Component 
{
    public float PreviousX, PreviousY; // 用于高速物体防穿透检测
    public TraceComponent(float x, float y) { PreviousX = x; PreviousY = y; }
}