using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EnemyTurnExec : TurnExec
{
    protected List<Participant> targets;
    protected DamageDataSO damageData;
    protected bool isParryWindowOpen = false;
    protected static ParryInput parryInput;

    protected override void PrepareForExecution(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        this.targets = targets;
        isParryWindowOpen = false; // Reset parry window state at the start of the turn
        damageData = attack.DamageData;

        GetParryInput();
        // Enable parry input
        parryInput.IsEnabled = true;

        Debug.Log($"Executing enemy turn for {executor.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
    }

    protected override void HandleSignalReceived(SignalAsset signal)
    {
        if (signal.name == "ParryAttackWindowOpenClose") OnParryWindowOpenClose();
    }

    protected void OnParryWindowOpenClose()
    {
        // Open if closed, close if open
        isParryWindowOpen = !isParryWindowOpen;

        if (enableDebug) Debug.Log($"Parry window is now {(isParryWindowOpen ? "open" : "closed")}.");

        if (isParryWindowOpen)
        {
            // Subscribe to parry signal
            parryInput.OnParry += OnParry;
        }
        else
        {
            HitTargets(); // Execute hit targets when parry window closes
            // Unsubscribe from parry signal
            parryInput.OnParry -= OnParry;
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
        foreach (var target in targets)
        {
            if (target.damageable == null)
            {
                Debug.LogWarning($"<color=purple>[EnemyTurnExec]</color> Target {target.name} does not have a CombatDamageable component. Skipping damage application.");
                continue;
            }
            if (target is PlayerParticipant player && player.IsTrueParry)
            {
                Debug.Log($"<color=purple>[EnemyTurnExec]</color> <b>{target.name} successfully parried the attack!</b> Damage is not applied.");
                continue; // Skip damage application for successful parry
            }
            target.damageable.TakeDamage(damageData);
        }
    }

    protected override void OnAttackSequenceFinished(PlayableDirector director)
    {
        parryInput.IsEnabled = false;
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