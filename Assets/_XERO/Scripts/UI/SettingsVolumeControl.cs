using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class SettingsVolumeControl : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Volume Range")]
    [Tooltip("Slider range will be 0 to this value. Default is 1. Set to 2 if you want louder-than-normal volume.")]
    [SerializeField] private float maxVolume = 1f;

    [Header("Saved Values - Replace Later With Actual Save Data")]
    [SerializeField] private float savedMasterVolume = 1f;
    [SerializeField] private float savedMusicVolume = 1f;
    [SerializeField] private float savedSfxVolume = 1f;

    [Header("Sound Manager")]
    [Tooltip("Optional. If empty, the script will use MMSoundManager.Instance.")]
    [SerializeField] private MMSoundManager soundManager;

    private bool isInitializing;

    private void Awake()
    {
        TryFindSoundManager();
        SetupSliderRanges();
    }

    private void OnEnable()
    {
        TryFindSoundManager();

        SetupSliderRanges();
        RegisterSliderEvents();
        ResetSlidersToSavedValues();
        ApplyAllVolumes();
    }

    private void OnDisable()
    {
        UnregisterSliderEvents();
    }

    private void SetupSliderRanges()
    {
        SetupSliderRange(masterSlider);
        SetupSliderRange(musicSlider);
        SetupSliderRange(sfxSlider);
    }

    private void SetupSliderRange(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = maxVolume;
        slider.wholeNumbers = false;
    }

    private void RegisterSliderEvents()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }

    private void UnregisterSliderEvents()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
        }
    }

    private void ResetSlidersToSavedValues()
    {
        isInitializing = true;

        SetSliderValueWithoutNotify(masterSlider, savedMasterVolume);
        SetSliderValueWithoutNotify(musicSlider, savedMusicVolume);
        SetSliderValueWithoutNotify(sfxSlider, savedSfxVolume);

        isInitializing = false;
    }

    private void SetSliderValueWithoutNotify(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(Mathf.Clamp(value, 0f, maxVolume));
    }

    private void ApplyAllVolumes()
    {
        SetMasterVolume(masterSlider != null ? masterSlider.value : savedMasterVolume);
        SetMusicVolume(musicSlider != null ? musicSlider.value : savedMusicVolume);
        SetSfxVolume(sfxSlider != null ? sfxSlider.value : savedSfxVolume);
    }

    public void SetMasterVolume(float volume)
    {
        if (isInitializing)
        {
            return;
        }

        if (!TryFindSoundManager())
        {
            return;
        }

        volume = Mathf.Clamp(volume, 0f, maxVolume);
        soundManager.SetVolumeMaster(volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (isInitializing)
        {
            return;
        }

        if (!TryFindSoundManager())
        {
            return;
        }

        volume = Mathf.Clamp(volume, 0f, maxVolume);
        soundManager.SetVolumeMusic(volume);
    }

    public void SetSfxVolume(float volume)
    {
        if (isInitializing)
        {
            return;
        }

        if (!TryFindSoundManager())
        {
            return;
        }

        volume = Mathf.Clamp(volume, 0f, maxVolume);
        soundManager.SetVolumeSfx(volume);
    }

    private bool TryFindSoundManager()
    {
        if (soundManager != null)
        {
            return true;
        }

        if (MMSoundManager.HasInstance)
        {
            soundManager = MMSoundManager.Instance;
            return true;
        }

        Debug.LogWarning($"{nameof(SettingsVolumeControl)}: No MMSoundManager found in the scene.");
        return false;
    }
}