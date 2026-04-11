using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 全局配置加载器：负责将所有 CSV 数据反序列化为内存中的 GameConfig 对象
/// </summary>
public static class ConfigLoader
{
    // 主入口
    public static GameConfig Load()
    {
        GameConfig config = new GameConfig();

        // 1. 加载基础全局配置
        TextAsset baseCsv = Resources.Load<TextAsset>("game_config");
        if (baseCsv != null) ParseBaseSettings(config, baseCsv);
        else Debug.LogError("初始化失败：未找到 Resources/game_config.csv");

        // 2. 加载敌人装配配方
        TextAsset enemyCsv = Resources.Load<TextAsset>("Enemy_config");
        if (enemyCsv != null) ParseEnemyRecipes(config, enemyCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Enemy_config.csv");

        // 3. 加载子弹数值配方
        TextAsset bulletCsv = Resources.Load<TextAsset>("Bullet_Config"); // 注意大小写匹配实际文件名
        if (bulletCsv != null) ParseBulletRecipes(config, bulletCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Bullet_Config.csv");

        return config;
    }

    private static void ParseBaseSettings(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        // 从 i=1 开始，跳过表头 (Key, Value, Description)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 2) continue;

            string key = cols[0].Trim();
            string valueStr = cols[1].Trim();

            // 使用硬编码映射，比反射性能好且利于代码追踪
            switch (key)
            {
                case "PlayerMaxHealth": config.PlayerMaxHealth = ParseFloat(valueStr); break;
                case "PlayerMoveSpeed": config.PlayerMoveSpeed = ParseFloat(valueStr); break;
                case "PlayerInvincibleDuration": config.PlayerInvincibleDuration = ParseFloat(valueStr); break;
                case "CollisionPushDistance": config.CollisionPushDistance = ParseFloat(valueStr); break;
                case "CollisionBounceForce": config.CollisionBounceForce = ParseFloat(valueStr); break;
                case "InitialSpawnInterval": config.InitialSpawnInterval = ParseFloat(valueStr); break;
            }
        }
    }

    private static void ParseEnemyRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 7) continue; // ID,Health,Speed,Damage,HitRecovery,EnemyDeathScore,Traits

            EnemyData data = new EnemyData
            {
                Id = cols[0].Trim(),
                Health = ParseFloat(cols[1]),
                Speed = ParseFloat(cols[2]),
                Damage = ParseInt(cols[3]),
                HitRecoveryDuration = ParseFloat(cols[4]),
                EnemyDeathScore = ParseInt(cols[5]),
                Traits = string.IsNullOrWhiteSpace(cols[6]) ? new string[0] : cols[6].Trim().Split('|'),
                // --- 新增：解析第8列数据 ---
                BounceForce = cols.Length > 7 ? ParseFloat(cols[7]) : 5.0f
            };
            config.EnemyRecipes[data.Id] = data;
        }
    }

    private static void ParseBulletRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 10) continue; // 10列数据

            BulletData data = new BulletData
            {
                Id = cols[0].Trim(),
                Speed = ParseFloat(cols[1]),
                Damage = ParseFloat(cols[2]),
                LifeTime = ParseFloat(cols[3]),
                ShootInterval = ParseFloat(cols[4]),
                SlowRatio = ParseFloat(cols[5]),
                SlowDuration = ParseFloat(cols[6]),
                ChainTargets = ParseInt(cols[7]),
                ChainRange = ParseFloat(cols[8]),
                AOERadius = ParseFloat(cols[9])
            };
            config.BulletRecipes[data.Id] = data;
        }
    }

    // ==========================================
    // 数据解析助手 (避免因语言环境差异或填表失误导致的闪退)
    // ==========================================
    
    private static float ParseFloat(string s)
    {
        if (float.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            return result;
        Debug.LogWarning($"[ConfigLoader] 无法将 '{s}' 解析为浮点数，已默认为 0");
        return 0f;
    }

    private static int ParseInt(string s)
    {
        if (int.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            return result;
        Debug.LogWarning($"[ConfigLoader] 无法将 '{s}' 解析为整数，已默认为 0");
        return 0;
    }
}