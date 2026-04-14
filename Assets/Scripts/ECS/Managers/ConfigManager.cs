// 路径: Assets/Scripts/ECS/Core/Managers/ConfigManager.cs
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }
    
    public GameConfig Config { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 唯一指责：加载并持有配置表
            Config = ConfigLoader.Load(); 
            Debug.Log("[ConfigManager] 游戏配置表加载完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}