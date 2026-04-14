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
    
    private Queue<GameObject> _ghostPool = new Queue<GameObject>();
    private List<ActiveGhost> _activeGhosts = new List<ActiveGhost>();
    private Transform _ghostRootContainer;

    public GhostTrailSystem(List<Entity> entities) : base(entities) 
    { 
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

        // 【动态配置】：读取全局残影消散速度
        float fadeSpeed = BattleManager.Instance.Config.GhostFadeSpeed;

        // 2. 更新已生成的残影淡出逻辑
        for (int i = _activeGhosts.Count - 1; i >= 0; i--) 
        {
            var ghost = _activeGhosts[i];
            ghost.Alpha -= fadeSpeed * deltaTime; // 动态控制消失速度
            
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
        
        // 【动态配置】：读取全局残影初始透明度
        float initialAlpha = BattleManager.Instance.Config.GhostInitialAlpha;
        ghostSr.color = new Color(sourceSr.color.r, sourceSr.color.g, sourceSr.color.b, initialAlpha);
        
        ghostSr.sortingLayerID = sourceSr.sortingLayerID;
        ghostSr.sortingOrder = sourceSr.sortingOrder - 1;

        _activeGhosts.Add(new ActiveGhost { Go = ghostGo, Sr = ghostSr, Alpha = initialAlpha });
    }
}