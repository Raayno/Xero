using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class CombatTargetMoveClip : PlayableAsset, ITimelineClipAsset
{
    [SerializeField] private CombatTargetMoveBehaviour template = new CombatTargetMoveBehaviour();

    public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<CombatTargetMoveBehaviour>.Create(graph, template);
    }
}