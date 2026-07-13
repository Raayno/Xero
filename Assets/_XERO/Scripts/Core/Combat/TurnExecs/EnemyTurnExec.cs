using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyTurnExec : TurnExec
{
    [SerializeField] protected UnityEngine.Timeline.SignalAsset parryAttackWindowOpenCloseSignal;
    protected Participant executingParticipant;
    protected List<Participant> targets;
    protected DamageDataSO damageData;
    protected bool isParryWindowOpen = false;
    private readonly HashSet<int> parriedTargetIds = new();
    private CancellationToken executionCancellationToken;

    protected override void PrepareForExecution(Participant participant, AttackDataSO attack, List<Participant> targets, CancellationToken cancellationToken)
    {
        executionCancellationToken = cancellationToken;
        executingParticipant = participant;
        this.targets = targets;
        isParryWindowOpen = false; // Reset parry window state at the start of the turn
        damageData = attack.DamageData;

        // Enable parry input
        ParryInput.Instance.IsEnabled = true;
        // Subscribe to parry signal
        ParryInput.Instance.OnParry += OnParry;

        Debug.Log($"<color=purple>[EnemyTurnExec]</color> Executing enemy turn for {participant.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
    }

    protected override void SubscribeToAttackSequenceEvents(bool isSubscribe)
    {
        TimelineSignalBridge.SubscribeToSignal(isSubscribe, parryAttackWindowOpenCloseSignal, OnParryWindowOpenClose);
    }

    protected void OnParryWindowOpenClose()
    {
        // Open if closed, close if open
        isParryWindowOpen = !isParryWindowOpen;

        if (enableDebug) Debug.Log($"Parry window is now {(isParryWindowOpen ? "open" : "closed")}.");

        if (isParryWindowOpen)
        {
            ParryScanTaskAsync(executionCancellationToken).Forget();
        }
        else
        {
            HitTargets(); // Execute hit targets when parry window closes
        }
    }

    private async UniTask ParryScanTaskAsync(CancellationToken cancellationToken)
    {
        parriedTargetIds.Clear();
        
        while (isParryWindowOpen && !cancellationToken.IsCancellationRequested)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (parriedTargetIds.Contains(i)) continue; // Skip already parried targets TODO: Can be optimised if HashSet were sorted
                var target = targets[i];
                if (target is PlayerParticipant player && player.IsTrueParry)
                {
                    target.Feedbacks.PlayFeedback(FeedbackType.PlayerOnParry, target.transform.position);
                    parriedTargetIds.Add(i); // Mark this target as parried
                }
            }
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }

    protected virtual void OnParry()
    {
        foreach (var target in targets)
        {
            if (target is PlayerParticipant player)
            {
                player.OnParry();
            }
        }
    }


    protected virtual void HitTargets()
    {
        // move to SPOT 2 if it shouldn't play feedbacks when parried, but for now, we want to play feedbacks even if parried
        executingParticipant.Feedbacks.PlayFeedback(FeedbackType.EnemyOnAttack, executingParticipant.transform.position);

        if (enableDebug) Debug.Log($"<color=purple>[EnemyTurnExec]</color> HitTargets called");
        for (int i = 0; i < targets.Count; i++)
        {
            if (parriedTargetIds.Contains(i)) continue; // Skip already parried targets

            Participant target = targets[i];
            if (target is PlayerParticipant player && player.IsTrueParry)
            {
                target.Feedbacks.PlayFeedback(FeedbackType.PlayerOnParry, targets[i].transform.position);
                Debug.Log($"<color=purple>[EnemyTurnExec]</color> <b>{target.name} successfully parried the attack!</b> Damage is not applied.");
                continue; // Skip damage application for successful parry
            }
            if (target.damageable == null)
            {
                Debug.LogWarning($"<color=purple>[EnemyTurnExec]</color> Target {target.name} does not have a CombatDamageable component. Skipping damage application.");
                continue;
            }

            // SPOT 2
            target.damageable.TakeDamage(damageData);
        }
    }

    protected override void OnAttackSequenceFinished()
    {
        // Unsubscribe from parry signal
        ParryInput.Instance.OnParry -= OnParry;
        ParryInput.Instance.IsEnabled = false;
        isParryWindowOpen = false;
    }
}