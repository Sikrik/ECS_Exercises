using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundDictionary
{
    public string Name;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM Settings")]
    public AudioSource BGMSource;
    public AudioClip MainBGM;

    [Header("SFX Settings")]
    public List<SoundDictionary> SoundEffects = new List<SoundDictionary>();
    
    // 用于播放音效的 AudioSource 池
    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private Dictionary<string, SoundDictionary> _sfxDict = new Dictionary<string, SoundDictionary>();

    void Awake()
    {
        Instance = this;
        
        // 预热字典
        foreach (var sfx in SoundEffects)
        {
            if (!_sfxDict.ContainsKey(sfx.Name))
                _sfxDict.Add(sfx.Name, sfx);
        }

        // 初始化 SFX 池 (初始给 10 个通道，足够应付同帧多音效)
        for (int i = 0; i < 10; i++)
        {
            CreateNewSFXSource();
        }
    }

    void Start()
    {
        // 启动 BGM
        if (BGMSource != null && MainBGM != null)
        {
            BGMSource.clip = MainBGM;
            BGMSource.loop = true;
            BGMSource.Play();
        }
    }

    private AudioSource CreateNewSFXSource()
    {
        GameObject go = new GameObject("SFX_Source");
        go.transform.SetParent(this.transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        _sfxPool.Add(source);
        return source;
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in _sfxPool)
        {
            if (!source.isPlaying) return source;
        }
        // 如果全在播放，动态扩容
        return CreateNewSFXSource();
    }

    // 播放全局 2D 音效 (如开枪、UI)
    public void PlaySFX(string clipName)
    {
        if (_sfxDict.TryGetValue(clipName, out var sfx))
        {
            AudioSource source = GetAvailableSource();
            source.spatialBlend = 0f; // 2D 声音
            source.pitch = Random.Range(0.9f, 1.1f); // 加入轻微音调随机，增加打击丰富度
            source.PlayOneShot(sfx.Clip, sfx.Volume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] 找不到音效: {clipName}");
        }
    }

    // 播放 3D 空间音效 (如远处怪物的爆炸)
    public void PlaySFXAtPosition(string clipName, Vector3 position)
    {
        if (_sfxDict.TryGetValue(clipName, out var sfx))
        {
            AudioSource source = GetAvailableSource();
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D 声音
            source.minDistance = 5f;
            source.maxDistance = 20f;
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(sfx.Clip, sfx.Volume);
        }
    }
}