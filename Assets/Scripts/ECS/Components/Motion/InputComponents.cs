// 存储移动意图，不代表最终速度
public class MoveInputComponent : Component 
{
    public float X;
    public float Y;
    public MoveInputComponent(float x, float y) { X = x; Y = y; }
}

// 【新增】：存储玩家射击意图与鼠标世界坐标
public class ShootInputComponent : Component
{
    public bool IsShooting; // 是否正在按下左键
    public float TargetX;   // 鼠标所在的世界坐标 X
    public float TargetY;   // 鼠标所在的世界坐标 Y
}