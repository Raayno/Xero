using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "AttackDataSO", menuName = "Combat/AttackDataSO/AttackDataSO")]
public class AttackDataSO : ScriptableObject
{
    [Header("Damage")]
    public DamageDataSO DamageData;

    [Header("Timeline")]
    public bool IsMoveToTarget = true;
    public TimelineAsset TimelineAsset;

    [Header("Targeting")]
    [Required] public TargetSelector TargetSelector;
}
