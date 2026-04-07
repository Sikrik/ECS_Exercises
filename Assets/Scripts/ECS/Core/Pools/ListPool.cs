using System.Collections.Generic;

/// <summary>
/// 列表池：负责 System 查询时临时列表的复用
/// </summary>
public static class ListPool
{
    private static Stack<List<Entity>> _pool = new Stack<List<Entity>>();

    public static List<Entity> Get() => _pool.Count > 0 ? _pool.Pop() : new List<Entity>();

    public static void Return(List<Entity> list)
    {
        list.Clear();
        _pool.Push(list);
    }
}