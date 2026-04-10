using System.Collections.Generic;

/// <summary>
/// 列表池：负责 System 查询时临时列表的复用
/// </summary>
public static class ListPool
{
    // 预分配外层栈大小，避免游戏启动时的扩容开销
    private static Stack<List<Entity>> _pool = new Stack<List<Entity>>(50);
    
    // 【核心优化】：设定安全容量阈值，防止偶尔产生的超大列表被回收后一直常驻内存导致泄漏
    private const int MAX_CAPACITY = 1024;

    public static List<Entity> Get() => _pool.Count > 0 ? _pool.Pop() : new List<Entity>();

    public static void Return(List<Entity> list)
    {
        list.Clear();
        
        // 如果底层数组容量过大，强行缩容，释放多余的内存给 GC
        if (list.Capacity > MAX_CAPACITY)
        {
            list.Capacity = MAX_CAPACITY;
        }
        
        _pool.Push(list);
    }
}