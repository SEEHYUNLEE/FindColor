using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (AudioManager.Instance == null)
            return;

        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(AudioManager.Instance.GetAudioVolume(EAudioMixerType.Master));

            masterSlider.onValueChanged.AddListener(value =>AudioManager.Instance.SetAudioVolume(EAudioMixerType.Master, value));
        }

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.GetAudioVolume(EAudioMixerType.BGM));

            bgmSlider.onValueChanged.AddListener(value => AudioManager.Instance.SetAudioVolume(EAudioMixerType.BGM, value));
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetAudioVolume(EAudioMixerType.SFX));

            sfxSlider.onValueChanged.AddListener(value => AudioManager.Instance.SetAudioVolume(EAudioMixerType.SFX,value));
        }
    }
}