// 路径: Assets/Scripts/ECS/Systems/GamePlay/EnemyTrackingSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        var pVel = player.GetComponent<VelocityComponent>(); // 获取玩家速度用于预判

        foreach (var enemy in enemies)
        {
            // 1. 硬控状态下停止思考
            if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>())
                continue;

            // 降频优化：屏幕外降低寻路计算频率
            if (enemy.HasComponent<OffScreenTag>() && Time.frameCount % 15 != 0) 
                continue; 

            var ePos = enemy.GetComponent<PositionComponent>();
            Vector2 currentPos = new Vector2(ePos.X, ePos.Y);
            
            // ==========================================
            // AI 策略 1：确定目标点 (基础追踪 vs 预判追踪)
            // ==========================================
            Vector2 targetPos = new Vector2(pPos.X, pPos.Y);
            
            if (enemy.HasComponent<PredictiveAIComponent>() && pVel != null)
            {
                var predict = enemy.GetComponent<PredictiveAIComponent>();
                // 预判玩家未来的位置：目标位置 = 玩家当前位置 + 玩家速度 * 预判时间
                targetPos.x += pVel.VX * predict.PredictTime;
                targetPos.y += pVel.VY * predict.PredictTime;
            }

            Vector2 toTarget = targetPos - currentPos;
            float distToTarget = toTarget.magnitude;
            Vector2 desiredDir = Vector2.zero;

            // ==========================================
            // AI 策略 2：移动逻辑 (引入多重随机扰动与顿挫)
            // ==========================================
            float time = Time.time;
            int hash = enemy.GetHashCode();

            // 1. 多重柏林噪声：基础平滑侧移 + 高频微小抖动
            float baseNoise = Mathf.PerlinNoise(hash * 0.1f, time * 0.5f) * 2f - 1f;
            float jitterNoise = Mathf.PerlinNoise(hash * 0.8f, time * 2.5f) * 2f - 1f;
            float noise = Mathf.Clamp(baseNoise + jitterNoise * 0.4f, -1f, 1f);

            // 2. 随机顿挫（Hesitation）：模拟怪物偶尔“走神”或“停顿观察”
            bool isHesitating = Mathf.PerlinNoise(hash * 0.05f, time * 0.2f) > 0.88f; 

            Vector2 sideStepDir = Vector2.zero;
            
            if (distToTarget > 0.001f)
            {
                // 计算垂直于玩家方向的侧向向量
                sideStepDir = new Vector2(-toTarget.y, toTarget.x).normalized * noise;
            }

            if (isHesitating)
            {
                // 顿挫状态：原地徘徊或直接缓慢侧移，增加攻击节奏的不可预测性
                desiredDir = sideStepDir * 0.2f; 
            }
            else
            {
                if (enemy.HasComponent<RangedAIComponent>())
                {
                    var ranged = enemy.GetComponent<RangedAIComponent>();
                    
                    // 动态容差：让不同个体的“撤退/前进”阈值产生差异，避免群体怪物动作过于一致或反复横跳
                    float personalTolerance = ranged.Tolerance + (Mathf.Sin(time + hash) * 0.5f);

                    // 距离太远，靠近
                    if (distToTarget > ranged.PreferredDistance + personalTolerance) 
                    {
                        desiredDir = (toTarget.normalized + sideStepDir * 0.3f).normalized;
                    }
                    // 距离太近，后退（风筝），并加入较强的侧向平移，避免死板倒退
                    else if (distToTarget < ranged.PreferredDistance - personalTolerance) 
                    {
                        desiredDir = (-toTarget.normalized + sideStepDir * 0.8f).normalized;
                    }
                    // 距离合适，徘徊状态（小幅度左右横移寻找输出位置）
                    else 
                    {
                        desiredDir = sideStepDir * 0.5f; 
                    }

                    // 只要在射程内，就产生开火意图（具体开火频率由 WeaponFiringSystem 的 CD 控制）
                    if (distToTarget <= ranged.PreferredDistance + ranged.Tolerance + 1f)
                    {
                        if (!enemy.HasComponent<FireIntentComponent>())
                        {
                            // 射击方向永远是真实的玩家方向，不受预判和移动扰动的影响
                            Vector2 trueToPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);
                            if (trueToPlayer.magnitude > 0.001f)
                            {
                                enemy.AddComponent(new FireIntentComponent(trueToPlayer.normalized)); 
                            }
                        }
                    }
                }
                else
                {
                    // 普通近战怪物：直接冲锋，但也混入较强的曲线扰动，让走位更灵动
                    if (distToTarget > 0.001f) 
                    {
                        desiredDir = (toTarget.normalized + sideStepDir * 0.4f).normalized;
                    }
                }
            }

            // ==========================================
            // AI 策略 3：虫群分离 (Swarm Separation) - 形成包围网
            // ==========================================
            if (enemy.HasComponent<SwarmSeparationComponent>())
            {
                var separation = enemy.GetComponent<SwarmSeparationComponent>();
                Vector2 avoidanceDir = Vector2.zero;
                
                // 借用 GridSystem 高效获取周围的怪物
                var nearby = ECSManager.Instance.Grid.GetNearbyEnemies(ePos.X, ePos.Y, 1);
                foreach (var other in nearby)
                {
                    if (other == enemy || !other.HasComponent<EnemyTag>()) continue;
                    
                    var oPos = other.GetComponent<PositionComponent>();
                    Vector2 diff = currentPos - new Vector2(oPos.X, oPos.Y);
                    float sqrDist = diff.sqrMagnitude;
                    
                    // 如果两个怪物太近（比如相距 1.5 米以内）
                    if (sqrDist < 2.25f && sqrDist > 0.001f)
                    {
                        // 距离越近，排斥力越大
                        avoidanceDir += diff.normalized / sqrDist; 
                    }
                }
                
                // 将分离向量与目标移动向量融合
                if (avoidanceDir != Vector2.zero)
                {
                    if (desiredDir == Vector2.zero) 
                        desiredDir = avoidanceDir.normalized;
                    else 
                        desiredDir = (desiredDir + avoidanceDir * separation.SeparationWeight).normalized;
                }
            }

            // ==========================================
            // 写入意图组件（交由 MovementSystem 处理惯性和实际位移）
            // ==========================================
            if (!enemy.HasComponent<MoveInputComponent>())
            {
                enemy.AddComponent(new MoveInputComponent(desiredDir.x, desiredDir.y));
            }
            else
            {
                var input = enemy.GetComponent<MoveInputComponent>();
                input.X = desiredDir.x;
                input.Y = desiredDir.y;
            }
        }
        
        ReturnListToPool(enemies);
    }
}