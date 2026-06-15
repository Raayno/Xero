using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager : MMPersistentSingleton<MusicManager>
{

    [System.Serializable]
    public class MusicTrack
    {
        public string id;
        public AudioClip clip;

        [Range(0f, 2f)]
        public float volume = 1f;

        [Range(0.1f, 3f)]
        public float pitch = 1f;

        public bool loop = true;
    }

    [Header("Music Library")]
    [SerializeField] private MusicTrack[] musicTracks;

    [Header("Default Music")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private string startMusicID;

    [Header("Transition Settings")]
    [SerializeField] private float defaultFadeOutDuration = 1f;
    [SerializeField] private float defaultFadeInDuration = 1f;
    [SerializeField] private bool ignoreSameMusicRequest = true;

    [Header("Persistence")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    [Header("Events")]
    public UnityEvent<string> OnMusicStarted;
    public UnityEvent<string> OnMusicStopped;
    public UnityEvent<string, string> OnMusicChanged;

    private const int BaseMusicID = 900000;

    private string currentMusicID;
    private string previousMusicID;

    private int currentSoundID = -1;
    private int nextSoundID = -1;

    private Coroutine transitionCoroutine;


    private void Start()
    {
        if (playOnStart && !string.IsNullOrWhiteSpace(startMusicID))
        {
            PlayMusic(startMusicID, 0f, defaultFadeInDuration);
        }
    }

    public void PlayMusic(string musicID)
    {
        PlayMusic(musicID, defaultFadeOutDuration, defaultFadeInDuration);
    }

    public void PlayMusic(string musicID, float fadeOutDuration, float fadeInDuration)
    {
        if (string.IsNullOrWhiteSpace(musicID))
        {
            Debug.LogWarning($"{nameof(MusicManager)}: Music ID is null or empty.");
            return;
        }

        MusicTrack targetTrack = GetMusicTrack(musicID);

        if (targetTrack == null)
        {
            Debug.LogWarning($"{nameof(MusicManager)}: No music track found with ID '{musicID}'.");
            return;
        }

        if (targetTrack.clip == null)
        {
            Debug.LogWarning($"{nameof(MusicManager)}: Music track '{musicID}' has no AudioClip assigned.");
            return;
        }

        if (ignoreSameMusicRequest && currentMusicID == musicID)
        {
            Log($"Music '{musicID}' is already playing. Request ignored.");
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionToMusicRoutine(targetTrack, fadeOutDuration, fadeInDuration));
    }

    public void StopMusic()
    {
        StopMusic(defaultFadeOutDuration);
    }

    public void StopMusic(float fadeOutDuration)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (currentSoundID == -1)
        {
            return;
        }

        StartCoroutine(StopMusicRoutine(fadeOutDuration));
    }

    public void PauseMusic()
    {
        if (currentSoundID == -1)
        {
            return;
        }

        MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Pause, currentSoundID);
    }

    public void ResumeMusic()
    {
        if (currentSoundID == -1)
        {
            return;
        }

        MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Resume, currentSoundID);
    }

    public void RestartCurrentMusic()
    {
        if (string.IsNullOrWhiteSpace(currentMusicID))
        {
            return;
        }

        string musicToRestart = currentMusicID;
        currentMusicID = null;

        PlayMusic(musicToRestart, defaultFadeOutDuration, defaultFadeInDuration);
    }

    private IEnumerator TransitionToMusicRoutine(MusicTrack targetTrack, float fadeOutDuration, float fadeInDuration)
    {
        previousMusicID = currentMusicID;

        int oldSoundID = currentSoundID;
        nextSoundID = GenerateSoundID(targetTrack.id);

        if (oldSoundID != -1)
        {
            FadeSound(oldSoundID, fadeOutDuration, 0f);
        }

        PlayTrackThroughSoundManager(targetTrack, nextSoundID, 0f);

        yield return null;

        FadeSound(nextSoundID, fadeInDuration, targetTrack.volume);

        if (oldSoundID != -1)
        {
            yield return MMCoroutine.WaitFor(fadeOutDuration);
            FreeSound(oldSoundID);
        }

        currentMusicID = targetTrack.id;
        currentSoundID = nextSoundID;
        nextSoundID = -1;

        OnMusicStarted?.Invoke(currentMusicID);
        OnMusicChanged?.Invoke(previousMusicID, currentMusicID);

        Log($"Changed music from '{previousMusicID}' to '{currentMusicID}'.");

        transitionCoroutine = null;
    }

    private IEnumerator StopMusicRoutine(float fadeOutDuration)
    {
        int oldSoundID = currentSoundID;
        string stoppedMusicID = currentMusicID;

        FadeSound(oldSoundID, fadeOutDuration, 0f);

        yield return MMCoroutine.WaitFor(fadeOutDuration);

        FreeSound(oldSoundID);

        currentSoundID = -1;
        currentMusicID = null;
        previousMusicID = stoppedMusicID;

        OnMusicStopped?.Invoke(stoppedMusicID);

        Log($"Stopped music '{stoppedMusicID}'.");
    }

    private void PlayTrackThroughSoundManager(MusicTrack musicTrack, int soundID, float startingVolume)
    {
        MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;

        options.ID = soundID;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
        options.Loop = musicTrack.loop;
        options.Persistent = dontDestroyOnLoad;
        options.Volume = startingVolume;
        options.Pitch = musicTrack.pitch;

        MMSoundManagerSoundPlayEvent.Trigger(musicTrack.clip, options);
    }

    private void FadeSound(int soundID, float duration, float targetVolume)
    {
        duration = Mathf.Max(0f, duration);

        MMSoundManagerSoundFadeEvent.Trigger(
            MMSoundManagerSoundFadeEvent.Modes.PlayFade,
            soundID,
            duration,
            targetVolume,
            new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic)
        );
    }

    private void FreeSound(int soundID)
    {
        MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, soundID);
    }

    private MusicTrack GetMusicTrack(string musicID)
    {
        if (musicTracks == null)
        {
            return null;
        }

        for (int i = 0; i < musicTracks.Length; i++)
        {
            if (musicTracks[i] == null)
            {
                continue;
            }

            if (musicTracks[i].id == musicID)
            {
                return musicTracks[i];
            }
        }

        return null;
    }

    private int GenerateSoundID(string musicID)
    {
        unchecked
        {
            int hash = musicID.GetHashCode();
            return BaseMusicID + Mathf.Abs(hash % 99999);
        }
    }

    private void Log(string message)
    {
        if (!debugLogs)
        {
            return;
        }

        Debug.Log($"[{nameof(MusicManager)}] {message}");
    }
}