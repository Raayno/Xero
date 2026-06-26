using System.Collections.Generic;
using UnityEngine;

public class CombatTargetProvider : MonoBehaviour
{
    [SerializeField] private CombatController controller;

    public List<Participant> GetAutoTargets(
        Participant attacker,
        AttackDataSO attackData)
    {
        List<Participant> targets = new();

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

        if (controller == null)
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

    public List<Participant> GetAliveEnemiesOfAttacker(Participant attacker)
    {
        List<Participant> targets = new List<Participant>();

        if (attacker == null || controller == null)
        {
            return targets;
        }

        AddAllEnemiesOfAttacker(targets, attacker);

        return targets;
    }

    public List<Participant> GetAliveAlliesOfAttacker(Participant attacker)
    {
        List<Participant> targets = new List<Participant>();

        if (attacker == null || controller == null)
        {
            return targets;
        }

        AddAllAlliesOfAttacker(targets, attacker);

        return targets;
    }

    public bool IsValidManualTarget(
        Participant attacker,
        Participant receiver,
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

    private void AddAllEnemiesOfAttacker(List<Participant> targets, Participant attacker)
    {
        if (attacker is PlayerParticipant)
        {
            foreach (EnemyParticipant enemy in controller.GetEnemies())
            {
                AddTargetIfValid(targets, enemy);
            }

            return;
        }

        if (attacker is EnemyParticipant)
        {
            foreach (PlayerParticipant player in controller.GetPlayers())
            {
                AddTargetIfValid(targets, player);
            }
        }
    }

    private void AddAllAlliesOfAttacker(List<Participant> targets, Participant attacker)
    {
        if (attacker is PlayerParticipant)
        {
            foreach (PlayerParticipant player in controller.GetPlayers())
            {
                AddTargetIfValid(targets, player);
            }

            return;
        }

        if (attacker is EnemyParticipant)
        {
            foreach (EnemyParticipant enemy in controller.GetEnemies())
            {
                AddTargetIfValid(targets, enemy);
            }
        }
    }

    private bool IsEnemyOfAttacker(Participant attacker, Participant receiver)
    {
        if (attacker is PlayerParticipant && receiver is EnemyParticipant)
        {
            return true;
        }

        if (attacker is EnemyParticipant && receiver is PlayerParticipant)
        {
            return true;
        }

        return false;
    }

    private bool IsAllyOfAttacker(Participant attacker, Participant receiver)
    {
        if (attacker is PlayerParticipant && receiver is PlayerParticipant)
        {
            return true;
        }

        if (attacker is EnemyParticipant && receiver is EnemyParticipant)
        {
            return true;
        }

        return false;
    }

    private void AddTargetIfValid(List<Participant> targets, Participant target)
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
