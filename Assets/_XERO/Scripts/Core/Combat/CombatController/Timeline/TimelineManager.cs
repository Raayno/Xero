using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineManager : MMSingleton<TimelineManager>
{
    [SerializeField] private bool enableDebug = false;

    private static readonly Queue<PlayableDirector> directorPool = new();
    private static readonly List<PlayableDirector> activeDirectors = new();

    public static void PlayTimeline(TimelineAsset timelineAsset, Animator animator, Action onTimelineEnd = null)
    {
        PlayableDirector director = GetOrCreateDirector();

        director.playableAsset = timelineAsset;
        
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is AnimationTrack)
            {
                director.SetGenericBinding(track, animator);
            }
            if (track is SignalTrack)
            {
                director.SetGenericBinding(track, TimelineSignalBridge.Instance.GetComponent<SignalReceiver>());
            }
        }

        director.time = 0;
        director.Play();

        TrackTimelineDuration(director, timelineAsset.duration, onTimelineEnd).Forget();
    }

    private static PlayableDirector GetOrCreateDirector()
    {
        if (directorPool.Count > 0)
        {
            var director = directorPool.Dequeue();
            activeDirectors.Add(director);
            director.enabled = true;
            return director;
        }

        GameObject go = new("Pooled_PlayableDirector", typeof(PlayableDirector));
        go.transform.SetParent(GetTimelineManager(out TimelineManager timelineManager) ? timelineManager.transform : null);
        
        var newDirector = go.GetComponent<PlayableDirector>();
        // Wyłączamy automatyczne odtwarzanie przy starcie
        newDirector.playOnAwake = false; 
        
        activeDirectors.Add(newDirector);
        return newDirector;
    }

    private static async UniTaskVoid TrackTimelineDuration(PlayableDirector director, double duration, Action onTimelineEnd)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        
        onTimelineEnd?.Invoke();

        if (director != null)
        {
            director.Stop();
            director.playableAsset = null;
            director.enabled = false;
            
            activeDirectors.Remove(director);
            directorPool.Enqueue(director);
        }
    }

    private void Start()
    {
        foreach (var dir in transform.GetComponentsInChildren<PlayableDirector>())
        {
            directorPool.Enqueue(dir);
            if (dir.enabled) dir.enabled = false;

            if (enableDebug) Debug.Log($"<color=#55AAFF>[CombatTimelineManager]</color> Added PlayableDirector to pool: {dir.name}");
        }
    }

    private void OnDestroy()
    {
        foreach (var d in activeDirectors) if (d != null) Destroy(d.gameObject);
        foreach (var d in directorPool) if (d != null) Destroy(d.gameObject);
    }

    private static bool GetTimelineManager(out TimelineManager timelineManager)
    {
        timelineManager = Instance;
        if (timelineManager == null)
        {
            Debug.LogError("<color=white>[CombatTimelineManager]</color> CombatTimelineManager Instance not found.");
            return false;
        }
        return true;
    }

    public static bool GetTimelineManager(TimelineManager timelineManager, out TimelineManager foundTimelineManager)
    {
        foundTimelineManager = timelineManager;
        if (foundTimelineManager != null) return true;
        return GetTimelineManager(out foundTimelineManager);
    }
}