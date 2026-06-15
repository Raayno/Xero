using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackDataSO", menuName = "Combat/PlayerAttackDataSO")]
public class PlayerAttackDataSO : AttackDataSO
{
    [Space]
    public string attackName;
    public string attackDescription;
}
