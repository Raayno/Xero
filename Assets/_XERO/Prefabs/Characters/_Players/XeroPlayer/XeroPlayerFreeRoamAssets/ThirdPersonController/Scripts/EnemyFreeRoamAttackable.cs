using UnityEngine;
using MoreMountains.Feedbacks;

public class EnemyFreeRoamAttackable : MonoBehaviour, IFreeRoamAttackable
{
    public MMF_Player LoadCombatSceneFeedback;

    public bool OnAttack()
    {
        SpecialCombatDataCarrier.BattleEntryType = BattleEntryType.PlayerAttack; // TODO: add animation and reset to EnemyAttack when the attack is finished
        if (LoadCombatSceneFeedback != null)
            LoadCombatSceneFeedback.PlayFeedbacks();
        return true; // Return true to indicate that this attackable should block other things in range from being attacked by this attack
    }

    void OnValidate()
    {
        if (LoadCombatSceneFeedback == null)
        {
            LoadCombatSceneFeedback = GetComponent<MMF_Player>();
        }
    }
}
