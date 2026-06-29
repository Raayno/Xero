using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PlayerTurnExec : TurnExec
{
    protected static TimelineSignalBridge signalBridge;
    [SerializeField] private HashSet<SignalAsset> signalsToListenFor;
    protected List<Damageable> targetDamageables;
    protected DamageDataSO damageData;

    protected override IEnumerator ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        PlayableDirector director = executor.AttackSequenceDirector;
        director.playableAsset = attack.TimelineAsset;

        targetDamageables = new List<Damageable>();
        foreach (var target in targets)
        {
            if (target.TryGetComponent<CombatDamageable>(out var damageable))
            {
                targetDamageables.Add(damageable);
            }
        }

        Debug.Log($"Executing player turn for {executor.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
        
        SubscribeToAttackSequenceEvents(director, true);
        director.Play();

        // Wait for the attack sequence to complete
        yield return WaitForAttackSequenceCompletion();

        SubscribeToAttackSequenceEvents(director, false);
    }

    protected void SubscribeToAttackSequenceEvents(PlayableDirector director, bool subscribe)
    {
        if (!GetSignalBridge()) throw new System.Exception("<color=purple>[PlayerTurnExec]</color> TimelineSignalBridge not found in CombatController's children.");
        if (subscribe)
        {
            director.stopped += OnAttackSequenceFinished;
            signalBridge.OnSignalReceived += HandleSignalReceived;
        }
        else
        {
            director.stopped -= OnAttackSequenceFinished;
            signalBridge.OnSignalReceived -= HandleSignalReceived;
        }
    }

    protected void HandleSignalReceived(SignalAsset signal)
    {
        if (!signalsToListenFor.Contains(signal)) return;
        switch (signal.name)
        {
            case "HitTargets":
                OnTargetsHit();
                break;
            default:
                Debug.LogError($"<color=purple>[PlayerTurnExec]</color> Unhandled signal received: {signal.name}");
                break;
        }
    }

    protected bool GetSignalBridge()
    {
        if (signalBridge == null)
        {
            signalBridge = CombatController.Instance.GetComponentInChildren<TimelineSignalBridge>();
            if (signalBridge == null)
            {
                Debug.LogError("<color=purple>[PlayerTurnExec]</color> TimelineSignalBridge not found in CombatController's children.");
                return false;
            }
        }
        return true;
    }

    protected void OnTargetsHit()
    {
        foreach (var target in targetDamageables)
        {
            target.TakeDamage(damageData);
        }
    }
    
    private IEnumerator WaitForAttackSequenceCompletion()
    {
        isSequenceCompleted = false;
        while (!isSequenceCompleted)
        {
            yield return null;
        }
    }

    bool isSequenceCompleted = false;
    private void OnAttackSequenceFinished(PlayableDirector director)
    {
        isSequenceCompleted = true;
    }
}