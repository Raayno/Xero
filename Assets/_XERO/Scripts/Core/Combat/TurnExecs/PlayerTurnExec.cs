using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerTurnExec : TurnExec
{
    [SerializeField] protected SignalAsset hitTargetsSignal;
    protected List<Damageable> targetDamageables;
    protected DamageDataSO damageData;

    protected override void PrepareForExecution(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        targetDamageables = new List<Damageable>();
        foreach (var target in targets)
        {
            if (target.TryGetComponent<CombatDamageable>(out var damageable))
            {
                targetDamageables.Add(damageable);
            }
        }
        damageData = attack.DamageData;

        Debug.Log($"<color=purple>[PlayerTurnExec]</color>. Executing player turn for {executor.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
    }

    protected override void SubscribeToAttackSequenceEvents(UnityEngine.Playables.PlayableDirector director, bool isSubscribe)
    {
        TimelineSignalBridge.SubscribeToNotifications(isSubscribe, hitTargetsSignal, OnHitTargets);
    }

    protected void OnHitTargets()
    {
        foreach (var target in targetDamageables)
        {
            target.TakeDamage(damageData);
        }
    }
}