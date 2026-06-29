using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "AttackDataSO", menuName = "Combat/AttackDataSO/AttackDataSO")]
public class AttackDataSO : ScriptableObject
{
    [Header("Damage")]
    [SerializeField] private DamageDataSO damageData;

    [Header("Timeline")]
    [Required][SerializeField] private TimelineAsset timelineAsset;

    [Header("Targeting")]
    [Required] public TargetSelector TargetSelector;

    public DamageDataSO DamageData => damageData;
    public TimelineAsset TimelineAsset => timelineAsset;
}
