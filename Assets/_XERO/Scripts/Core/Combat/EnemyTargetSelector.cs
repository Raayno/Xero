using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetSelector : MonoBehaviour
{
    [SerializeField] private CombatTargetProvider combatTargetProvider;

    public List<Participant> SelectTargets(
        EnemyParticipant enemyCombatTarget,
        AttackDataSO attackData)
    {
        List<Participant> selectedTargets = new();

        if (enemyCombatTarget == null)
        {
            Debug.LogError("[EnemyTargetSelector] Enemy combat target is null.");
            return selectedTargets;
        }

        if (attackData == null)
        {
            Debug.LogError("[EnemyTargetSelector] Attack data is null.");
            return selectedTargets;
        }

        if (combatTargetProvider == null)
        {
            Debug.LogError("[EnemyTargetSelector] CombatTargetProvider is not assigned.");
            return selectedTargets;
        }

        switch (attackData.TargetType)
        {
            case CombatActionTargetType.SingleEnemy:
                {
                    List<Participant> validTargets =
                        combatTargetProvider.GetAliveEnemiesOfAttacker(enemyCombatTarget);

                    AddRandomTarget(selectedTargets, validTargets);
                    break;
                }

            case CombatActionTargetType.SingleAlly:
                {
                    List<Participant> validTargets =
                        combatTargetProvider.GetAliveAlliesOfAttacker(enemyCombatTarget);

                    AddRandomTarget(selectedTargets, validTargets);
                    break;
                }

            case CombatActionTargetType.Self:
            case CombatActionTargetType.AllEnemies:
            case CombatActionTargetType.AllAllies:
                {
                    selectedTargets = combatTargetProvider.GetAutoTargets(enemyCombatTarget, attackData);
                    break;
                }

            default:
                {
                    Debug.LogWarning(
                        $"[EnemyTargetSelector] Unsupported target type: {attackData.TargetType}");

                    break;
                }
        }

        if (selectedTargets.Count == 0)
        {
            Debug.LogWarning(
                $"[EnemyTargetSelector] No valid targets found for {enemyCombatTarget.CombatantName} using {attackData.name}.");
        }

        return selectedTargets;
    }

    private void AddRandomTarget(
        List<Participant> selectedTargets,
        List<Participant> validTargets)
    {
        if (selectedTargets == null)
        {
            return;
        }

        if (validTargets == null || validTargets.Count == 0)
        {
            return;
        }

        Participant randomTarget = validTargets[Random.Range(0, validTargets.Count)];

        if (randomTarget == null)
        {
            return;
        }

        selectedTargets.Add(randomTarget);
    }
}