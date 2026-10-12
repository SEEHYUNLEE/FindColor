using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum EAudioMixerType
{
    Master,
    BGM,
    SFX
}

public enum BGMType
{
    MainMenu,
    Main,
    NormalStage,
    BossStage
}

public enum SFXType
{
    Click,
    Keyboard,
    Warp,
    PlayerAttack,
    PlayerHit,
    SlimeAttack,
    SlimeHit,
    SlimeDie,
    Pickup,
    Upgrade
    
}

[System.Serializable]
public class BGMData
{
    public BGMType type;
    public AudioClip clip;
}

[System.Serializable]
public class SFXData
{
    public SFXType type;
    public AudioClip clip;
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField] private BGMData[] bgmDatas;

    [Header("SFX")]
    [SerializeField] private SFXData[] sfxDatas;

    [Header("Default Volume")]
    [Range(0.0001f, 1f)]
    [SerializeField] private float defaultMasterVolume = 1f;

    [Range(0.0001f, 1f)]
    [SerializeField] private float defaultBGMVolume = 1f;

    [Range(0.0001f, 1f)]
    [SerializeField] private float defaultSFXVolume = 1f;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private bool bgmWasPlayingBeforeDialogue;

    private void Awake()
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

        LoadVolume();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayBGMByCurrentScene();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        PlayBGMByCurrentScene();
    }

    // =========================================================
    // BGM
    // =========================================================

    private void PlayBGMByCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "MainMenu":
                PlayBGM(BGMType.MainMenu);
                break;

            case "Main":
                PlayBGM(BGMType.Main);
                break;

            case "NormalStage":
                PlayBGM(BGMType.NormalStage);
                break;

            case "BossCutscene":
                PlayBGM(BGMType.BossStage);
                break;
        }
    }

    public void PlayBGM(BGMType type)
    {
        AudioClip clip = GetBGMClip(type);

        if (clip == null)
        {
            return;
        }

        if (bgmSource.clip == clip &&
            bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    private AudioClip GetBGMClip(BGMType type)
    {
        foreach (BGMData data in bgmDatas)
        {
            if (data.type == type)
            {
                return data.clip;
            }
        }

        return null;
    }

    // =========================================================
    // SFX
    // =========================================================

    public void PlaySFX(SFXType type)
    {
        AudioClip clip = GetSFXClip(type);

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    private AudioClip GetSFXClip(SFXType type)
    {
        foreach (SFXData data in sfxDatas)
        {
            if (data.type == type)
            {
                return data.clip;
            }
        }

        return null;
    }
    public void StopSFX()
    {
        sfxSource.Stop();
    }

    // =========================================================
    // Volume
    // =========================================================

    public void SetAudioVolume(EAudioMixerType type, float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);

        float decibel = Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(type.ToString(), decibel);

        PlayerPrefs.SetFloat(GetPlayerPrefsKey(type), volume);

        PlayerPrefs.Save();
    }

    public float GetAudioVolume(EAudioMixerType type)
    {
        string key = GetPlayerPrefsKey(type);

        float defaultValue = 1f;

        switch (type)
        {
            case EAudioMixerType.Master:
                defaultValue = defaultMasterVolume;
                break;

            case EAudioMixerType.BGM:
                defaultValue = defaultBGMVolume;
                break;

            case EAudioMixerType.SFX:
                defaultValue = defaultSFXVolume;
                break;
        }

        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    private string GetPlayerPrefsKey(EAudioMixerType type)
    {
        switch (type)
        {
            case EAudioMixerType.Master:
                return MASTER_VOLUME_KEY;

            case EAudioMixerType.BGM:
                return BGM_VOLUME_KEY;

            case EAudioMixerType.SFX:
                return SFX_VOLUME_KEY;
        }
        return "";
    }

    private void LoadVolume()
    {
        SetMixerVolumeWithoutSave(EAudioMixerType.Master, GetAudioVolume(EAudioMixerType.Master));

        SetMixerVolumeWithoutSave(EAudioMixerType.BGM, GetAudioVolume(EAudioMixerType.BGM));

        SetMixerVolumeWithoutSave(EAudioMixerType.SFX, GetAudioVolume(EAudioMixerType.SFX));
    }

    private void SetMixerVolumeWithoutSave(EAudioMixerType type, float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);

        float decibel = Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(type.ToString(), decibel);
    }

    public void PauseBGMForDialogue()
    {
        bgmWasPlayingBeforeDialogue = bgmSource.isPlaying;

        if (bgmWasPlayingBeforeDialogue)
            bgmSource.Pause();
    }

    public void ResumeBGMAfterDialogue()
    {
        if (bgmWasPlayingBeforeDialogue)
            bgmSource.UnPause();

        bgmWasPlayingBeforeDialogue = false;
    }

}