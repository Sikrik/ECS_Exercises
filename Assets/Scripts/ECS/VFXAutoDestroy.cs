using UnityEngine;

/// <summary>
/// 特效自动销毁脚本：挂载在拥有 ParticleSystem 的 Prefab 上
/// 确保粒子播放完毕后自动回收资源
/// </summary>
public class VFXAutoDestroy : MonoBehaviour
{
    private ParticleSystem _particle;

    void Start()
    {
        // 获取当前物体上的粒子系统组件
        _particle = GetComponent<ParticleSystem>();

        if (_particle != null)
        {
            // 计算销毁延迟：粒子系统的主时长 + 0.1秒缓冲
            // 这样可以确保粒子完全消失后再销毁 GameObject
            float destroyDelay = _particle.main.duration + 0.1f;
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            // 如果没找到粒子系统，默认1秒后销毁，避免永久留在场景中
            Destroy(gameObject, 1.0f);
        }
    }
}