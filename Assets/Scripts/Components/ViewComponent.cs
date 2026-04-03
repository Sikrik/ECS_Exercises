using UnityEngine;
/// <summary>
/// 视图组件，存储实体对应的Unity GameObject视图对象
/// 用于将ECS的数据同步到Unity的场景视图中
/// </summary>
public class ViewComponent:Component
{
    /// <summary>
    /// 实体对应的Unity场景对象，用于显示实体的视觉表现
    /// </summary>
    public GameObject GameObject;
    
    /// <summary>
    /// 初始化视图组件实例
    /// </summary>
    /// <param name="go">实体对应的GameObject实例</param>
    public ViewComponent(GameObject go)
    {
        GameObject = go;
    }
}