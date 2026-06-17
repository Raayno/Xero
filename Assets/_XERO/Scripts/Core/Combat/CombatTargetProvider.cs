using System.Collections.Generic;
using UnityEngine;

public class CombatTargetProvider : MonoBehaviour
{
    [SerializeField] private CombatTimelineController combatTimelineController;

    public List<CombatTarget> GetAutoTargets(
        CombatTarget attacker,
        AttackDataSO attackData)
    {
        List<CombatTarget> targets = new List<CombatTarget>();

        if (attacker == null)
        {
            Debug.LogError("[CombatTargetProvider] Attacker is null.");
            return targets;
        }

        if (attackData == null)
        {
            Debug.LogError("[CombatTargetProvider] Attack data is null.");
            return targets;
        }

        if (combatTimelineController == null)
        {
            Debug.LogError("[CombatTargetProvider] CombatTimelineController is not assigned.");
            return targets;
        }

        switch (attackData.TargetType)
        {
            case CombatActionTargetType.Self:
                AddTargetIfValid(targets, attacker);
                break;

            case CombatActionTargetType.AllEnemies:
                AddAllEnemiesOfAttacker(targets, attacker);
                break;

            case CombatActionTargetType.AllAllies:
                AddAllAlliesOfAttacker(targets, attacker);
                break;

            case CombatActionTargetType.SingleEnemy:
            case CombatActionTargetType.SingleAlly:
                Debug.Log(
                    $"[CombatTargetProvider] {attackData.TargetType} requires manual target selection.");

                break;

            default:
                Debug.LogWarning(
                    $"[CombatTargetProvider] Unsupported target type: {attackData.TargetType}");

                break;
        }

        return targets;
    }

    public List<CombatTarget> GetAliveEnemiesOfAttacker(CombatTarget attacker)
    {
        List<CombatTarget> targets = new List<CombatTarget>();

        if (attacker == null || combatTimelineController == null)
        {
            return targets;
        }

        AddAllEnemiesOfAttacker(targets, attacker);

        return targets;
    }

    public List<CombatTarget> GetAliveAlliesOfAttacker(CombatTarget attacker)
    {
        List<CombatTarget> targets = new List<CombatTarget>();

        if (attacker == null || combatTimelineController == null)
        {
            return targets;
        }

        AddAllAlliesOfAttacker(targets, attacker);

        return targets;
    }

    public bool IsValidManualTarget(
        CombatTarget attacker,
        CombatTarget receiver,
        CombatActionTargetType targetType)
    {
        if (attacker == null || receiver == null)
        {
            return false;
        }

        if (attacker.IsDefeated || receiver.IsDefeated)
        {
            return false;
        }

        switch (targetType)
        {
            case CombatActionTargetType.SingleEnemy:
                return IsEnemyOfAttacker(attacker, receiver);

            case CombatActionTargetType.SingleAlly:
                return IsAllyOfAttacker(attacker, receiver);

            case CombatActionTargetType.Self:
                return attacker == receiver;

            default:
                return false;
        }
    }

    private void AddAllEnemiesOfAttacker(List<CombatTarget> targets, CombatTarget attacker)
    {
        if (attacker is PlayerCombatTarget)
        {
            foreach (EnemyCombatTarget enemy in combatTimelineController.GetEnemies())
            {
                AddTargetIfValid(targets, enemy);
            }

            return;
        }

        if (attacker is EnemyCombatTarget)
        {
            foreach (PlayerCombatTarget player in combatTimelineController.GetPlayers())
            {
                AddTargetIfValid(targets, player);
            }
        }
    }

    private void AddAllAlliesOfAttacker(List<CombatTarget> targets, CombatTarget attacker)
    {
        if (attacker is PlayerCombatTarget)
        {
            foreach (PlayerCombatTarget player in combatTimelineController.GetPlayers())
            {
                AddTargetIfValid(targets, player);
            }

            return;
        }

        if (attacker is EnemyCombatTarget)
        {
            foreach (EnemyCombatTarget enemy in combatTimelineController.GetEnemies())
            {
                AddTargetIfValid(targets, enemy);
            }
        }
    }

    private bool IsEnemyOfAttacker(CombatTarget attacker, CombatTarget receiver)
    {
        if (attacker is PlayerCombatTarget && receiver is EnemyCombatTarget)
        {
            return true;
        }

        if (attacker is EnemyCombatTarget && receiver is PlayerCombatTarget)
        {
            return true;
        }

        return false;
    }

    private bool IsAllyOfAttacker(CombatTarget attacker, CombatTarget receiver)
    {
        if (attacker is PlayerCombatTarget && receiver is PlayerCombatTarget)
        {
            return true;
        }

        if (attacker is EnemyCombatTarget && receiver is EnemyCombatTarget)
        {
            return true;
        }

        return false;
    }

    private void AddTargetIfValid(List<CombatTarget> targets, CombatTarget target)
    {
        if (target == null)
        {
            return;
        }

        if (target.IsDefeated)
        {
            return;
        }

        if (targets.Contains(target))
        {
            return;
        }

        targets.Add(target);
    }
}
