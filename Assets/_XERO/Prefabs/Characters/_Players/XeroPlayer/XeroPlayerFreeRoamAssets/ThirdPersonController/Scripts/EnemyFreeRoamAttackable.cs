using UnityEngine;
using MoreMountains.Feedbacks;

public class EnemyFreeRoamAttackable : MonoBehaviour, IFreeRoamAttackable
{
    [SerializeField] private MMF_Player loadCombatSceneFeedback;

    public bool OnAttack()
    {
        SpecialCombatDataCarrier.BattleEntryType = BattleEntryType.PlayerAttack; // TODO: add animation and reset to EnemyAttack when the attack is finished
        if (loadCombatSceneFeedback != null)
            loadCombatSceneFeedback.PlayFeedbacks();
        return true; // Return true to indicate that this attackable should block other things in range from being attacked by this attack
    }

    void OnValidate()
    {
        if (loadCombatSceneFeedback == null)
        {
            loadCombatSceneFeedback = GetComponent<MMF_Player>();
        }
    }
}
