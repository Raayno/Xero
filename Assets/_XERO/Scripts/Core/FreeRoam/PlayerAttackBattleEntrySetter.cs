using UnityEngine;
using MoreMountains.Tools;

[RequireComponent(typeof(MMF_ColliderActions))]
public class PlayerAttackBattleEntrySetter : MonoBehaviour
{
    [SerializeField] private MMF_ColliderActions colliderActions;
    [SerializeField] private LayerMask playerAttackLayer = -1;
    public void SetBattleEntryTypeToPlayerAttack()
    {
        if (!playerAttackLayer.MMContains(colliderActions.LayerOfOtherObject)) return;
        SpecialCombatDataCarrier.BattleEntryType = BattleEntryType.PlayerAttack;
    }

    private void OnValidate()
    {
        if (colliderActions == null)
        {
            colliderActions = GetComponent<MMF_ColliderActions>();
        }
        if (playerAttackLayer == -1)
        {
            playerAttackLayer = LayerMask.GetMask("Player Attack");
        }
    }
}
