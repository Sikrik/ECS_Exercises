using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsUIManager : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider BGMSlider;
    public Slider SFXSlider;

    void OnEnable()
    {
        if (AudioManager.Instance != null)
        {
            // 初始化 BGM 滑动条
            if (BGMSlider != null)
            {
                BGMSlider.value = AudioManager.Instance.MasterBGMVolume;
                BGMSlider.onValueChanged.RemoveAllListeners();
                BGMSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            // 初始化 SFX 滑动条
            if (SFXSlider != null)
            {
                SFXSlider.value = AudioManager.Instance.MasterSFXVolume;
                SFXSlider.onValueChanged.RemoveAllListeners();
                // 实时更新数值但不播放声音
                SFXSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

                // 添加 EventTrigger 实现“松开鼠标播放音效”
                AddPointerUpEvent(SFXSlider, "Shoot"); 
            }
        }
    }

    // 实时调整背景音乐音量
    public void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    // 实时更新全局音效倍率数值
    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    // 给滑动条动态添加 PointerUp 事件
    private void AddPointerUpEvent(Slider slider, string testSoundName)
    {
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slider.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { 
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(testSoundName); 
        });
        trigger.triggers.Add(entry);
    }
}