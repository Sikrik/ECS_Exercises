// GameConfig.cs 优化版本
// 优化内容：
// 1. 调大子弹击退默认参数，让弹开效果更明显
using UnityEngine;
[System.Serializable]
public class GameConfig
{
    // ===================== 玩家配置 =====================
    public float PlayerMoveSpeed = 5f;
    public int PlayerMaxHealth = 100;
    public float PlayerCollisionRadius = 0.5f;
    
    // ===================== 普通敌人基础配置 =====================
    public float EnemyMoveSpeed = 2f;
    public int EnemyMaxHealth = 50;
    public int EnemyDamage = 10;
    public float EnemyAttackCooldown = 1f;
    public float EnemyCollisionRadius = 0.5f;
    
    // ===================== 敌人生成配置 =====================
    public float InitialSpawnInterval = 2f;
    public float MinSpawnInterval = 0.5f;
    public float SpawnIntervalDecrease = 0.01f;
    public int EnemyKillScore = 10;
    
    // ===================== 对象池配置 =====================
    public int EnemyPoolInitialSize = 10;
    public int EnemyPoolMaxSize = 50;
    public int BulletPoolInitialSize = 20;
    public int BulletPoolMaxSize = 100;
    
    // ===================== 普通子弹配置 =====================
    public float BulletSpeed = 8f;
    public int BulletDamage = 20;
    public float BulletLifeTime = 3f;
    public float BulletCollisionRadius = 0.2f;
    public float ShootInterval = 0.5f;
    
    // ===================== 减速子弹配置 =====================
    public float SlowBulletSlowRatio = 0.5f;
    public float SlowBulletDuration = 2f;
    public float SlowBulletShootInterval = 0.6f;
    public int SlowBulletDamage = 15;
    
    // ===================== 连锁闪电子弹配置 =====================
    public float ChainLightningShootInterval = 0.8f;
    public int ChainLightningDamage = 25;
    public int ChainLightningMaxTargets = 5;
    public float ChainLightningChainRange = 5.0f;
    
    // ===================== 范围伤害子弹配置 =====================
    public float AOEBulletShootInterval = 1f;
    public int AOEBulletDamage = 30;
    public float AOEBulletRadius = 1.5f;
    
    // ===================== 快速敌人配置 =====================
    public float FastEnemyMaxHealth = 30;
    public float FastEnemySpeed = 4f;
    public float FastEnemyCollisionRadius = 0.3f;
    
    // ===================== 坦克敌人配置 =====================
    public float TankEnemyMaxHealth = 200;
    public float TankEnemySpeed = 0.8f;
    public float TankEnemyCollisionRadius = 0.8f;
    
    // ===================== 拾取道具配置 =====================
    public float PickupDropChance = 0.1f;
    public float HealPickupPercent = 0.3f;
    public float SpeedBoostDuration = 5f;
    public float SpeedBoostPercent = 0.5f;
    
    // ===================== 敌人碰撞击退与恢复配置 =====================
    public float EnemyKnockbackSpeed = 3f;
    public float EnemyKnockbackDuration = 0.1f;
    public float EnemyHitRecoveryDuration = 0.4f;
    
    // ===================== 不同敌人类型的独立击退配置 =====================
    // 普通敌人击退配置
    public float NormalEnemyKnockbackSpeed = 3f;
    public float NormalEnemyKnockbackDuration = 0.1f;
    public float NormalEnemyHitRecoveryDuration = 0.4f;
    
    // 快速敌人击退配置
    public float FastEnemyKnockbackSpeed = 4f;
    public float FastEnemyKnockbackDuration = 0.15f;
    public float FastEnemyHitRecoveryDuration = 0.3f;
    
    // 坦克敌人击退配置
    public float TankEnemyKnockbackSpeed = 1.5f;
    public float TankEnemyKnockbackDuration = 0.08f;
    public float TankEnemyHitRecoveryDuration = 0.6f;
    
    // 新增：子弹击退配置（优化：调大参数，让弹开效果更明显）
    public float BulletKnockbackSpeed = 5.0f;
    public float BulletKnockbackDuration = 0.2f;
    public float BulletHitRecoveryDuration = 0.2f;
    
    // 新增：玩家无敌帧配置
    public float PlayerInvincibleDuration = 0.5f;
    
    // ===================== 构造函数：初始化所有默认值 =====================
    public GameConfig()
    {
        // 玩家配置默认值
        PlayerMoveSpeed = 5f;
        PlayerMaxHealth = 100;
        PlayerCollisionRadius = 0.5f;
        
        // 普通敌人默认值
        EnemyMoveSpeed = 2f;
        EnemyMaxHealth = 50;
        EnemyDamage = 10;
        EnemyAttackCooldown = 1f;
        EnemyCollisionRadius = 0.5f;
        
        // 生成配置默认值
        InitialSpawnInterval = 2f;
        MinSpawnInterval = 0.5f;
        SpawnIntervalDecrease = 0.01f;
        EnemyKillScore = 10;
        
        // 对象池默认值
        EnemyPoolInitialSize = 10;
        EnemyPoolMaxSize = 50;
        BulletPoolInitialSize = 20;
        BulletPoolMaxSize = 100;
        
        // 普通子弹默认值
        BulletSpeed = 8f;
        BulletDamage = 20;
        BulletLifeTime = 3f;
        BulletCollisionRadius = 0.2f;
        ShootInterval = 0.5f;
        
        // 减速子弹默认值
        SlowBulletSlowRatio = 0.5f;
        SlowBulletDuration = 2f;
        SlowBulletShootInterval = 0.6f;
        SlowBulletDamage = 15;
        
        // 连锁闪电默认值
        ChainLightningShootInterval = 0.8f;
        ChainLightningDamage = 25;
        ChainLightningMaxTargets = 5;
        ChainLightningChainRange = 2.5f;
        
        // AOE 默认值
        AOEBulletShootInterval = 1f;
        AOEBulletDamage = 30;
        AOEBulletRadius = 1.5f;
        
        // 新敌人默认值
        FastEnemyMaxHealth = 30;
        FastEnemySpeed = 4f;
        FastEnemyCollisionRadius = 0.3f;
        TankEnemyMaxHealth = 200;
        TankEnemySpeed = 0.8f;
        TankEnemyCollisionRadius = 0.8f;
        
        // 拾取道具默认值
        PickupDropChance = 0.1f;
        HealPickupPercent = 0.3f;
        SpeedBoostDuration = 5f;
        SpeedBoostPercent = 0.5f;
        
        // 敌人碰撞击退与恢复默认值
        EnemyKnockbackSpeed = 3f;
        EnemyKnockbackDuration = 0.1f;
        EnemyHitRecoveryDuration = 0.4f;
        
        // 不同敌人类型的击退默认值
        NormalEnemyKnockbackSpeed = 3f;
        NormalEnemyKnockbackDuration = 0.1f;
        NormalEnemyHitRecoveryDuration = 0.4f;
        FastEnemyKnockbackSpeed = 4f;
        FastEnemyKnockbackDuration = 0.15f;
        FastEnemyHitRecoveryDuration = 0.3f;
        TankEnemyKnockbackSpeed = 1.5f;
        TankEnemyKnockbackDuration = 0.08f;
        TankEnemyHitRecoveryDuration = 0.6f;
        
        // 子弹击退默认值（优化：调大参数，让弹开效果更明显）
        BulletKnockbackSpeed = 5.0f;
        BulletKnockbackDuration = 0.2f;
        BulletHitRecoveryDuration = 0.2f;
        
        // 玩家无敌帧默认值
        PlayerInvincibleDuration = 0.5f;
    }
}