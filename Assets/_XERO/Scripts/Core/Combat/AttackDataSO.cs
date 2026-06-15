using UnityEngine;
using UnityEngine.Timeline;

public abstract class AttackDataSO : ScriptableObject
{
    [Space]
    [InlineScriptableObject]
    public DamageDataSO damageAmount;

    [Space]
    public TimelineAsset timelineAsset;
}


