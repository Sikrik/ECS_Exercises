using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冲刺残影特效系统
/// 职责：监听 GhostTrailComponent，并在实体坐标生成淡出的 Sprite 克隆体
/// </summary>
public class GhostTrailSystem : SystemBase
{
    private class ActiveGhost
    {
        public GameObject Go;
        public SpriteRenderer Sr;
        public float Alpha;
    }
    
    // 内部对象池，防止频繁实例化销毁
    private Queue<GameObject> _ghostPool = new Queue<GameObject>();
    private List<ActiveGhost> _activeGhosts = new List<ActiveGhost>();
    
    // 用于收纳残影的统一父节点容器
    private Transform _ghostRootContainer;

    public GhostTrailSystem(List<Entity> entities) : base(entities) 
    { 
        // 在系统初始化时创建一个空物体充当文件夹
        GameObject rootGo = new GameObject("[Pool] DashGhosts");
        _ghostRootContainer = rootGo.transform;
    }

    public override void Update(float deltaTime)
    {
        // 1. 扫描需要生成残影的实体
        var generators = GetEntitiesWith<GhostTrailComponent, ViewComponent>();
        foreach (var e in generators)
        {
            var config = e.GetComponent<GhostTrailComponent>();
            var view = e.GetComponent<ViewComponent>();
            
            if (view.SpriteRenderer == null) continue;

            config.Timer += deltaTime;
            if (config.Timer >= config.SpawnInterval)
            {
                config.Timer = 0;
                SpawnGhost(view.GameObject.transform, view.SpriteRenderer);
            }
        }

        // 2. 更新已生成的残影淡出逻辑
        for (int i = _activeGhosts.Count - 1; i >= 0; i--) 
        {
            var ghost = _activeGhosts[i];
            ghost.Alpha -= 3.5f * deltaTime; // 消失速度
            
            if (ghost.Alpha <= 0) 
            {
                ghost.Go.SetActive(false);
                _ghostPool.Enqueue(ghost.Go);
                _activeGhosts.RemoveAt(i);
            } 
            else 
            {
                Color c = ghost.Sr.color;
                c.a = ghost.Alpha;
                ghost.Sr.color = c;
            }
        }
    }

    private void SpawnGhost(Transform sourceTf, SpriteRenderer sourceSr)
    {
        GameObject ghostGo;
        SpriteRenderer ghostSr;

        if (_ghostPool.Count > 0)
        {
            ghostGo = _ghostPool.Dequeue();
            ghostGo.SetActive(true);
            ghostSr = ghostGo.GetComponent<SpriteRenderer>();
        }
        else
        {
            ghostGo = new GameObject("DashGhost_Item");
            ghostSr = ghostGo.AddComponent<SpriteRenderer>();
            
            // 将新生成的残影直接放进专属文件夹里
            if (_ghostRootContainer != null)
            {
                ghostGo.transform.SetParent(_ghostRootContainer);
            }
        }

        // 同步视觉状态
        ghostGo.transform.SetPositionAndRotation(sourceTf.position, sourceTf.rotation);
        ghostGo.transform.localScale = sourceTf.lossyScale;

        ghostSr.sprite = sourceSr.sprite;
        ghostSr.flipX = sourceSr.flipX;
        ghostSr.flipY = sourceSr.flipY;
        
        // 设置初始颜色（带透明度）
        float initialAlpha = 0.6f;
        ghostSr.color = new Color(sourceSr.color.r, sourceSr.color.g, sourceSr.color.b, initialAlpha);
        
        // 渲染层级永远比本体低一级
        ghostSr.sortingLayerID = sourceSr.sortingLayerID;
        ghostSr.sortingOrder = sourceSr.sortingOrder - 1;

        _activeGhosts.Add(new ActiveGhost { Go = ghostGo, Sr = ghostSr, Alpha = initialAlpha });
    }
}