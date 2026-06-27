using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Timeline;

public abstract class AttackDataSO : ScriptableObject
{
    [Header("Damage")]
    [SerializeField] private DamageDataSO damageData;

    [Header("Timeline")]
    [Required][SerializeField] private TimelineAsset timelineAsset;

    [Header("Targeting")]
    public SerializableType TargetSelectorType;

    public DamageDataSO DamageData => damageData;
    public TimelineAsset TimelineAsset => timelineAsset;
}
