using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
[TrackColor(0.2f, 0.7f, 1f)]
[TrackClipType(typeof(CombatTargetMoveClip))]
[TrackBindingType(typeof(CombatTarget))]
public class CombatTargetMoveTrack : TrackAsset
{
    public override Playable CreateTrackMixer(
        PlayableGraph graph,
        GameObject go,
        int inputCount)
    {
        return ScriptPlayable<CombatTargetMoveMixer>.Create(graph, inputCount);
    }
}