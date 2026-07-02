using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cysharp.Threading.Tasks;
using System.Threading;

[EnsureAssetInstance]
public abstract class TurnExec: ScriptableObject
{
    protected static TimelineSignalBridge signalBridge;
    [SerializeField] private AttackSelector attackSelector;
    [SerializeField] private List<AttackDataSO> availableAttacks;
    [Tooltip("Signals that this TurnExec will listen for during the attack sequence.")]
    [SerializeField] private List<SignalAsset> signalsToListenFor;
    [SerializeField] protected bool enableDebug = false;

    public async UniTask ExecuteTurn(Participant executor, CancellationToken cancellationToken)
    {
        if (attackSelector == null)
        {
            Debug.LogError("<color=purple>[TurnExec]</color> AttackSelector is not assigned.");
            return;
        }

        PlayableDirector director = executor.playableDirector;
        bool shouldReturnToOriginalPosition = false;
        try
        {
            // Select an attack
            AttackDataSO attack = await attackSelector.SelectAttackAsync(availableAttacks, cancellationToken);

            if (attack == null)
            {
                Debug.LogError("<color=purple>[TurnExec]</color> AttackSelector returned a null attack.");
                return;
            }
            Debug.Log($"<color=purple>[TurnExec]</color> {executor.name} selected attack: {attack.name}");

            if (attack.TargetSelector == null)
            {
                Debug.LogError($"<color=purple>[TurnExec]</color> Attack '{attack.name}' has no target selector assigned.");
                return;
            }

            // Select targets
            List<Participant> targets = await attack.TargetSelector.SelectTargetsAsync(executor, cancellationToken);

            if (targets == null || targets.Count == 0)
            {
                return;
            }

            Debug.Log($"<color=purple>[TurnExec]</color> {executor.name} selected {targets.Count} target(s) for attack: {attack.name}. That is: {string.Join(", ", targets.ConvertAll(t => t.name))}");

            director.playableAsset = attack.TimelineAsset;

            PrepareForExecution(executor, attack, targets, cancellationToken);

            await executor.ParticipantMovable.MoveToTargetAsync(targets, cancellationToken);
            shouldReturnToOriginalPosition = true;
            
            SubscribeToAttackSequenceEventsBase(director, true, cancellationToken);
            director.playableAsset = attack.TimelineAsset;
            isSequenceCompleted = false;
            director.Play();

            // Wait for the attack sequence to complete
            await WaitForAttackSequenceCompletionAsync(cancellationToken);
        }
        finally
        {
            SubscribeToAttackSequenceEventsBase(director, false, cancellationToken);

            if (shouldReturnToOriginalPosition && executor.ParticipantMovable != null)
            {
                await executor.ParticipantMovable.ReturnToOriginalPositionAsync(CancellationToken.None);
            }
        }
    }

    protected virtual void PrepareForExecution(Participant executor, AttackDataSO attack, List<Participant> targets, CancellationToken cancellationToken)
    {
        PrepareForExecution(executor, attack, targets);
    }

    protected virtual void PrepareForExecution(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> No specific preparation for execution in this TurnExec");
    }

    private void SubscribeToAttackSequenceEventsBase(PlayableDirector director, bool isSubscribe, CancellationToken cancellationToken)
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
    
    private async UniTask WaitForAttackSequenceCompletionAsync(CancellationToken cancellationToken)
    {
        isSequenceCompleted = false;
        await UniTask.WaitUntil(() => isSequenceCompleted, cancellationToken: cancellationToken);
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