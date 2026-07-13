using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerTurnExec : TurnExec
{
    [SerializeField] protected SignalAsset hitTargetsSignal;
    protected List<CombatDamageable> targetDamageables;
    protected DamageDataSO damageData;
    protected Participant executingParticipant;

    protected override void PrepareForExecution(Participant participant, AttackDataSO attack, List<Participant> targets)
    {
        executingParticipant = participant;

        targetDamageables = new List<CombatDamageable>();
        foreach (var target in targets)
        {
            if (target.TryGetComponent<CombatDamageable>(out var damageable))
            {
                targetDamageables.Add(damageable);
            }
        }
        damageData = attack.DamageData;

        Debug.Log($"<color=purple>[PlayerTurnExec]</color>. Executing player turn for {participant.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
    }

    protected override void SubscribeToAttackSequenceEvents(bool isSubscribe)
    {
        TimelineSignalBridge.SubscribeToSignal(isSubscribe, hitTargetsSignal, OnHitTargets);
    }

    protected void OnHitTargets()
    {
        executingParticipant.Feedbacks.PlayFeedback(FeedbackType.PlayerOnAttack, executingParticipant.transform.position, 1f, playGlobal: true);

        foreach (var target in targetDamageables)
        {
            target.TakeDamage(damageData);
        }
    }
}