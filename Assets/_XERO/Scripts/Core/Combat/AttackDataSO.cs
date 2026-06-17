using UnityEngine;
using UnityEngine.Timeline;

public abstract class AttackDataSO : ScriptableObject
{
    [Header("Damage")]
    [SerializeField] private DamageDataSO damageData;

    [Header("Timeline")]
    [SerializeField] private TimelineAsset timelineAsset;

    public DamageDataSO DamageData => damageData;
    public TimelineAsset TimelineAsset => timelineAsset;
}