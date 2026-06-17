using UnityEngine;
using UnityEngine.Timeline;

public abstract class AttackDataSO : ScriptableObject
{
    [Header("Damage")]
    [SerializeField] private DamageDataSO damageData;

    [Header("Timeline")]
    [SerializeField] private TimelineAsset timelineAsset;

    [Header("Targeting")]
    [SerializeField] private CombatActionTargetType targetType = CombatActionTargetType.SingleEnemy;

    public DamageDataSO DamageData => damageData;
    public TimelineAsset TimelineAsset => timelineAsset;
    public CombatActionTargetType TargetType => targetType;
}
