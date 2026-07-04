using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Timeline;
using Cysharp.Threading.Tasks;
using System.Threading;

[EnsureAssetInstance]
public abstract class TurnExec: ScriptableObject
{
    [SerializeField] private AttackSelector attackSelector;
    [SerializeField] private List<AttackDataSO> availableAttacks;
    [Tooltip("Signals that this TurnExec will listen for during the attack sequence.")]
    [SerializeField] private List<SignalAsset> signalsToListenFor;
    [SerializeField] protected bool enableDebug = false;

    public async UniTask ExecuteTurn(Participant participant, CancellationToken cancellationToken)
    {
        if (attackSelector == null)
        {
            Debug.LogError("<color=purple>[TurnExec]</color> AttackSelector is not assigned.");
            return;
        }

        // Select an attack
        AttackDataSO attack = await attackSelector.SelectAttackAsync(availableAttacks, cancellationToken);

        if (attack == null)
        {
            Debug.LogError("<color=purple>[TurnExec]</color> AttackSelector returned a null attack.");
            return;
        }
        Debug.Log($"<color=purple>[TurnExec]</color> {participant.name} selected attack: {attack.name}");

        try
        {
            if (attack.TargetSelector == null)
            {
                Debug.LogError($"<color=purple>[TurnExec]</color> Attack '{attack.name}' has no target selector assigned.");
                return;
            }

            // Select targets
            List<Participant> targets = await attack.TargetSelector.SelectTargetsAsync(participant, cancellationToken);

            if (targets == null || targets.Count == 0)
            {
                return;
            }

            Debug.Log($"<color=purple>[TurnExec]</color> {participant.name} selected {targets.Count} target(s) for attack: {attack.name}. That is: {string.Join(", ", targets.ConvertAll(t => t.name))}");

            PrepareForExecution(participant, attack, targets, cancellationToken);

            if (participant.ParticipantMovable != null && attack.IsMoveToTarget)
                await participant.ParticipantMovable.MoveToTargetAsync(targets, cancellationToken);
            
            SubscribeToAttackSequenceEventsBase(true);
            TimelineManager.PlayTimeline(attack.TimelineAsset, participant.Animator, () => OnAttackSequenceFinishedBase());

            // Wait for the attack sequence to complete
            await WaitForAttackSequenceCompletionAsync(cancellationToken);
        }
        finally
        {
            SubscribeToAttackSequenceEventsBase(false);

            if (participant.ParticipantMovable != null && attack.IsMoveToTarget)
                await participant.ParticipantMovable.ReturnToOriginalPositionAsync(cancellationToken);
        }
    }

    protected virtual void PrepareForExecution(Participant participant, AttackDataSO attack, List<Participant> targets, CancellationToken cancellationToken)
    {
        PrepareForExecution(participant, attack, targets);
    }

    protected virtual void PrepareForExecution(Participant participant, AttackDataSO attack, List<Participant> targets)
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> No specific preparation for execution in this TurnExec");
    }

    private void SubscribeToAttackSequenceEventsBase(bool isSubscribe)
    {
        SubscribeToAttackSequenceEvents(isSubscribe);
    }

    protected virtual void SubscribeToAttackSequenceEvents(bool isSubscribe)
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> No specific subscription to attack sequence events in this TurnExec");
    }
    
    private async UniTask WaitForAttackSequenceCompletionAsync(CancellationToken cancellationToken)
    {
        isSequenceCompleted = false;
        await UniTask.WaitUntil(() => isSequenceCompleted, cancellationToken: cancellationToken);
    }
    
    bool isSequenceCompleted = false;
    private void OnAttackSequenceFinishedBase()
    {
        isSequenceCompleted = true;
        OnAttackSequenceFinished();
    }

    protected virtual void OnAttackSequenceFinished()
    {
        if (enableDebug) Debug.Log($"<color=purple>[TurnExec]</color> Attack sequence finished.");
    }
}