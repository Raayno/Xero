using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackDataSO", menuName = "Combat/PlayerAttackDataSO")]
public class PlayerAttackDataSO : AttackDataSO
{
    [Header("Player Attack Info")]
    [SerializeField] private string attackName;

    [TextArea]
    [SerializeField] private string attackDescription;

    public string AttackName => attackName;
    public string AttackDescription => attackDescription;
}