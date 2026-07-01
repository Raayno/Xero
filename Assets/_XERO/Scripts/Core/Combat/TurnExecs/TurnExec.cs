using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[EnsureAssetInstance]
public abstract class TurnExec: ScriptableObject
{
    protected static TimelineSignalBridge signalBridge;
    [SerializeField] private AttackSelector attackSelector;
    [SerializeField] private List<AttackDataSO> availableAttacks;
    [Tooltip("Signals that this TurnExec will listen for during the attack sequence.")]
    [SerializeField] private List<SignalAsset> signalsToListenFor;
    [SerializeField] protected bool enableDebug = false;

    public IEnumerator ExecuteTurn(Participant executor)
    {
        if (attackSelector == null)
        {
            Debug.LogError("<color=purple>[TurnExec]</color> AttackSelector is not assigned.");
            yield break;
        }

        // Select an attack
        AttackDataSO attack = null;
        yield return attackSelector.SelectAttackAsync(availableAttacks, selectedAttack => attack = selectedAttack);

        if (attack == null)
        {
            Debug.LogError("<color=purple>[TurnExec]</color> AttackSelector returned a null attack.");
            yield break;
        }
        Debug.Log($"<color=purple>[TurnExec]</color> {executor.name} selected attack: {attack.name}");

        if (attack.TargetSelector == null)
        {
            Debug.LogError($"<color=purple>[TurnExec]</color> Attack '{attack.name}' has no target selector assigned.");
            yield break;
        }

        // Select targets
        List<Participant> targets = null;
        yield return attack.TargetSelector.SelectTargetsAsync(executor, selectedTargets => targets = selectedTargets);

        if (targets == null || targets.Count == 0)
        {
            yield break;
        }

        Debug.Log($"<color=purple>[TurnExec]</color> {executor.name} selected {targets.Count} target(s) for attack: {attack.name}. That is: {string.Join(", ", targets.ConvertAll(t => t.name))}");

        PlayableDirector director = executor.playableDirector;
        director.playableAsset = attack.TimelineAsset;

        PrepareForExecution(executor, attack, targets);
        
        SubscribeToAttackSequenceEventsBase(director, true);
        director.Play();

        // Wait for the attack sequence to complete
        yield return WaitForAttackSequenceCompletion();

        SubscribeToAttackSequenceEventsBase(director, false);
    }

    protected virtual void PrepareForExecution(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> No specific preparation for execution in this TurnExec");
    }

    private void SubscribeToAttackSequenceEventsBase(PlayableDirector director, bool isSubscribe)
    {
        if (!TimelineSignalBridge.GetSignalBridge(signalBridge, out signalBridge)) throw new System.Exception("<color=purple>[TurnExec]</color> TimelineSignalBridge not found in CombatController's children.");

        if (isSubscribe)
        {
            director.stopped += OnAttackSequenceFinishedBase;
        }
        else
        {
            director.stopped -= OnAttackSequenceFinishedBase;
        }
        SubscribeToAttackSequenceEvents(director, isSubscribe);
    }

    protected virtual void SubscribeToAttackSequenceEvents(PlayableDirector director, bool isSubscribe)
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> No specific subscription to attack sequence events in this TurnExec");
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
    private void OnAttackSequenceFinishedBase(PlayableDirector director)
    {
        isSequenceCompleted = true;
        OnAttackSequenceFinished(director);
    }

    protected virtual void OnAttackSequenceFinished(PlayableDirector director)
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> Attack sequence finished for {director.name}.");
    }
}