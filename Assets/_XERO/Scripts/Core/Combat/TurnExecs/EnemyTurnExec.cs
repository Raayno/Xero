using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyTurnExec : TurnExec
{
    [SerializeField] protected UnityEngine.Timeline.SignalAsset parryAttackWindowOpenCloseSignal;
    protected List<Participant> targets;
    protected DamageDataSO damageData;
    protected bool isParryWindowOpen = false;
    protected static ParryInput parryInput;
    private readonly HashSet<int> parriedTargetIds = new();
    private CancellationToken executionCancellationToken;

    protected override void PrepareForExecution(Participant executor, AttackDataSO attack, List<Participant> targets, CancellationToken cancellationToken)
    {
        executionCancellationToken = cancellationToken;
        this.targets = targets;
        isParryWindowOpen = false; // Reset parry window state at the start of the turn
        damageData = attack.DamageData;

        GetParryInput();
        // Enable parry input
        parryInput.IsEnabled = true;
        // Subscribe to parry signal
        parryInput.OnParry += OnParry;

        Debug.Log($"<color=purple>[EnemyTurnExec]</color> Executing enemy turn for {executor.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
    }

    protected override void SubscribeToAttackSequenceEvents(PlayableDirector director, bool isSubscribe)
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
                if (targets[i] is PlayerParticipant player && player.IsTrueParry)
                {
                    Debug.Log($"<color=purple>[EnemyTurnExec]</color> <b>{targets[i].name} successfully parried the attack!</b> Damage is not applied.");
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
        if (enableDebug) Debug.Log($"<color=purple>[EnemyTurnExec]</color> HitTargets called");
        for (int i = 0; i < targets.Count; i++)
        {
            if (parriedTargetIds.Contains(i)) continue; // Skip already parried targets

            Participant target = targets[i];
            if (target is PlayerParticipant player && player.IsTrueParry)
            {
                Debug.Log($"<color=purple>[EnemyTurnExec]</color> <b>{target.name} successfully parried the attack!</b> Damage is not applied.");
                continue; // Skip damage application for successful parry
            }
            if (target.damageable == null)
            {
                Debug.LogWarning($"<color=purple>[EnemyTurnExec]</color> Target {target.name} does not have a CombatDamageable component. Skipping damage application.");
                continue;
            }
            target.damageable.TakeDamage(damageData);
        }
    }

    protected override void OnAttackSequenceFinished(PlayableDirector director)
    {
        // Unsubscribe from parry signal
        parryInput.OnParry -= OnParry;
        parryInput.IsEnabled = false;
        isParryWindowOpen = false;
    }

    protected void GetParryInput()
    {
        if (parryInput == null)
        {
            parryInput = CombatController.Instance.ParryInput;
            if (parryInput == null)
            {
                Debug.LogError("[EnemyTurnExec] No ParryInput found in the scene. Parry functionality will not work.");
            }
        }
    }
}