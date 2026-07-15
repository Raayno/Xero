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

    public static void PlayTimeline(TimelineAsset timelineAsset, Animator animator, out PlayableDirector director, Action onTimelineEnd = null)
    {
        director = GetOrCreateDirector(animator.transform);

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

        TrackTimelineDuration(director, onTimelineEnd).Forget();
    }

    public static void PlayTimeline(TimelineAsset timelineAsset, Animator animator, Action onTimelineEnd = null)
    {
        PlayTimeline(timelineAsset, animator, out _, onTimelineEnd);
    }

    /// <param name="director">Get this by adding "out var director" to your PlayTimeline()</param>
    public static void StopTimeline(PlayableDirector director) => ReturnToPool(director);

    public static void StopTimeline(Animator animator)
    {
        var director = animator.GetComponentInChildren<PlayableDirector>();
        if (director != null)
        {
            ReturnToPool(director);
        }
    }

    private static PlayableDirector GetOrCreateDirector(Transform parent)
    {
        if (directorPool.Count > 0)
        {
            var director = directorPool.Dequeue();
            activeDirectors.Add(director);
            director.transform.SetParent(parent);
            director.enabled = true;
            return director;
        }

        GameObject go = new("Pooled_PlayableDirector", typeof(PlayableDirector));
        go.transform.SetParent(parent);
        
        var newDirector = go.GetComponent<PlayableDirector>();
        // Wyłączamy automatyczne odtwarzanie przy starcie
        newDirector.playOnAwake = false; 
        
        activeDirectors.Add(newDirector);
        return newDirector;
    }

    private static async UniTaskVoid TrackTimelineDuration(PlayableDirector director, Action onTimelineEnd)
    {
        while (director.state == PlayState.Playing)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        
        onTimelineEnd?.Invoke();
        ReturnToPool(director);
    }

    private static void ReturnToPool(PlayableDirector director)
    {
        if (director == null)
        {
            Debug.LogError("<color=#55AAFF>[CombatTimelineManager]</color> Cannot return timeline to pool. PlayableDirector is null.");
            return;
        }

        if (!activeDirectors.Contains(director))
        {
            Debug.Log("<color=#55AAFF>[CombatTimelineManager]</color> The provided PlayableDirector is not currently active or it was already returned.");
            return;
        }

        director.Stop();
        director.playableAsset = null;
        director.enabled = false;

        director.transform.SetParent(Instance.transform); // Reset parent to TimelineManager for organization

        activeDirectors.Remove(director);
        directorPool.Enqueue(director);
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
}
