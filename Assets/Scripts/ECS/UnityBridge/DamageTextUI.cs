using UnityEngine;
using TMPro;

public class DamageTextUI : MonoBehaviour
{
    public TextMeshPro TextComponent;
    public float MoveSpeed = 2f;      // 向上飘的速度
    public float FadeSpeed = 1.5f;    // 渐隐的速度
    
    private Color _textColor;

    // 接收系统传来的数据
    public void Initialize(float damageAmount, bool isCritical)
    {
        TextComponent.text = Mathf.CeilToInt(damageAmount).ToString();
        
        if (isCritical)
        {
            // 暴击表现：红色，字体放大
            TextComponent.color = new Color(1f, 0.2f, 0.2f, 1f); 
            TextComponent.fontSize = 8f; 
            // 暴击时弹跳得更高一点
            MoveSpeed = 3f;
        }
        else
        {
            // 普通伤害：白色，常规大小
            TextComponent.color = Color.white;
            TextComponent.fontSize = 5f;
            MoveSpeed = 2f;
        }
        
        _textColor = TextComponent.color;

        // 稍微加一点随机位置偏移，防止同一帧多个伤害字完全重叠
        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f), 0);
    }

    void Update()
    {
        // 1. 向上位移
        transform.position += Vector3.up * MoveSpeed * Time.deltaTime;
        
        // 2. 透明度渐隐
        _textColor.a -= FadeSpeed * Time.deltaTime;
        TextComponent.color = _textColor;

        // 3. 完全透明后自我销毁/回收
        if (_textColor.a <= 0)
        {
            // 如果你的 GameObject_PoolManager 有 Despawn 方法，请替换为它以保证性能
            // GameObject_PoolManager.Instance.Despawn(gameObject); 
            Destroy(gameObject); 
        }
    }
}