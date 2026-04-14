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

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float MasterBGMVolume = 0.5f;
    [Range(0f, 1f)] public float MasterSFXVolume = 0.5f;

    [Header("BGM Settings")]
    public AudioSource BGMSource;
    public AudioClip MainBGM;

    [Header("SFX Settings")]
    public List<SoundDictionary> SoundEffects = new List<SoundDictionary>();
    
    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private Dictionary<string, SoundDictionary> _sfxDict = new Dictionary<string, SoundDictionary>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        foreach (var sfx in SoundEffects)
        {
            if (!_sfxDict.ContainsKey(sfx.Name))
                _sfxDict.Add(sfx.Name, sfx);
        }

        for (int i = 0; i < 10; i++)
        {
            CreateNewSFXSource();
        }
    }

    void Start()
    {
        // 游戏启动（主界面）立即播放 BGM
        if (BGMSource != null && MainBGM != null)
        {
            BGMSource.clip = MainBGM;
            BGMSource.loop = true;
            BGMSource.volume = MasterBGMVolume;
            BGMSource.Play();
        }
    }

    public void SetBGMVolume(float volume)
    {
        MasterBGMVolume = Mathf.Clamp01(volume);
        if (BGMSource != null)
        {
            BGMSource.volume = MasterBGMVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        MasterSFXVolume = Mathf.Clamp01(volume);
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
        return CreateNewSFXSource();
    }

    public void PlaySFX(string clipName)
    {
        if (_sfxDict.TryGetValue(clipName, out var sfx))
        {
            AudioSource source = GetAvailableSource();
            source.spatialBlend = 0f; 
            source.pitch = Random.Range(0.9f, 1.1f); 
            source.PlayOneShot(sfx.Clip, sfx.Volume * MasterSFXVolume);
        }
    }

    public void PlaySFXAtPosition(string clipName, Vector3 position)
    {
        if (_sfxDict.TryGetValue(clipName, out var sfx))
        {
            AudioSource source = GetAvailableSource();
            source.transform.position = position;
            source.spatialBlend = 1f; 
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(sfx.Clip, sfx.Volume * MasterSFXVolume);
        }
    }
}