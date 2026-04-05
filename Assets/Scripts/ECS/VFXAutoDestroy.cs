using UnityEngine;
public class VFXAutoDestroy : MonoBehaviour
{
    private ParticleSystem _particle;
    void Start()
    {
        _particle = GetComponent<ParticleSystem>();
        // 等待粒子播放完成，自动销毁这个特效对象
        Destroy(gameObject, _particle.main.duration + 0.1f);
    }
}