using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 通用GameObject对象池，解决频繁创建销毁的GC卡顿问题
/// </summary>
public class ObjectPool
{
    // 预制体：池内所有对象的模板
    private readonly GameObject _prefab;
    // 空闲对象队列：存储回收的对象
    private readonly Queue<GameObject> _pool;
    // 池的最大容量：避免池无限增长，占用过多内存
    private readonly int _maxSize;
    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="prefab">要复用的预制体</param>
    /// <param name="initialSize">预热数量：游戏启动时提前创建的对象数</param>
    /// <param name="maxSize">池的最大容量：超过这个数量的对象会被真的销毁</param>
    public ObjectPool(GameObject prefab, int initialSize, int maxSize)
    {
        _prefab = prefab;
        _maxSize = maxSize;
        _pool = new Queue<GameObject>();
        // 预热：提前创建一批对象，避免游戏刚开始时频繁创建
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(_prefab);
            obj.SetActive(false); // 预热对象默认禁用，不占用性能
            _pool.Enqueue(obj);
        }
    }
    /// <summary>
    /// 从池里获取一个对象
    /// </summary>
    /// <returns>可用的GameObject</returns>
    public GameObject Get()
    {
        GameObject obj;
        if (_pool.Count > 0)
        {
            // 池里有空闲对象，直接拿出来用
            obj = _pool.Dequeue();
            obj.SetActive(true); // 激活对象
        }
        else
        {
            // 池里没有空闲的，才创建新的
            obj = Object.Instantiate(_prefab);
        }
        return obj;
    }
    /// <summary>
    /// 把对象回收到池里
    /// </summary>
    /// <param name="obj">要回收的GameObject</param>
    public void Release(GameObject obj)
    {
        if (_pool.Count < _maxSize)
        {
            // 池没满，回收对象，禁用后存到队列
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
        else
        {
            // 池满了，真的销毁这个对象，避免内存泄漏
            Object.Destroy(obj);
        }
    }
    /// <summary>
    /// 清空整个池，重启游戏时调用
    /// </summary>
    public void Clear()
    {
        // 销毁池里所有的对象
        foreach (var obj in _pool)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
        _pool.Clear();
    }
}