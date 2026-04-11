using UnityEngine;

/// <summary>
/// 防止随身 UI 因为父物体转身而发生镜像反转
/// </summary>
public class HUDStabilizer : MonoBehaviour
{
    private Vector3 _initialScale;

    void Start()
    {
        // 记录 Canvas 初始大小（例如 0.01, 0.01, 0.01）
        _initialScale = transform.localScale; 
    }

    void LateUpdate()
    {
        // 强制解除旋转
        transform.rotation = Quaternion.identity;
        
        // 抵消父物体的负数缩放（转身）
        if (transform.parent != null)
        {
            float signX = Mathf.Sign(transform.parent.localScale.x);
            transform.localScale = new Vector3(_initialScale.x * signX, _initialScale.y, _initialScale.z);
        }
    }
}