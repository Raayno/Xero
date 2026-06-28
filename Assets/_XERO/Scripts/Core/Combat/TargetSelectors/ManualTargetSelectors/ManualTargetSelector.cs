using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[IgnoreAssetInstanceEnsurement]
[CreateAssetMenu(fileName = "ManualTargetSelector", menuName = "Combat/TargetSelectors/ManualTargetSelector")]
public class ManualTargetSelector : TargetSelector
{
    [SerializeField] protected TargetSelector selectionPoolSelector;
    protected ParticipantPointInput pointInput;
    protected LayerMask selectionPoolMask;
    protected int selectionPoolLayer = -1;
    protected bool selectionWasCanceled;
    protected Participant selectedParticipant;
    [SerializeField] protected bool enableDebug = false;
    static private readonly List<Participant> currentSelectionPool = new();

    protected override List<Participant> SelectTargets()
    {
        Debug.LogError("[ManualTargetSelector] Manual target selection must be awaited asynchronously.");
        return new();
    }
    protected override List<Participant> SelectTargets(Participant self)
    {
        return SelectTargets();
    }

    protected override IEnumerator SelectTargetsAsync(Participant self, Action<List<Participant>> onCompleted)
    {
        yield return GetPointInput(onCompleted);

        if (!UpdateSelectionPoolMask())
        {
            HandlePointInputEventsSubscription(false);
            onCompleted?.Invoke(new());
            yield break;
        }

        selectedParticipant = null;
        selectionWasCanceled = false;
        HandlePointInputEventsSubscription(true);
        pointInput.StartSelection();
        
        Debug.Log("<color=yellow>[ManualTargetSelector]</color> Awaiting player input for target selection...");
        yield return new WaitUntil(() => selectionWasCanceled || selectedParticipant != null);

        HandlePointInputEventsSubscription(false);

        if (selectionWasCanceled || pointInput.PointedParticipant == null)
        {
            onCompleted?.Invoke(new());
            yield break;
        }

        Debug.Log($"<color=yellow>[ManualTargetSelector]</color> Player selected target: {pointInput.PointedParticipant.CombatantName}");

        onCompleted?.Invoke(new() { pointInput.PointedParticipant });
    }

    protected virtual void HandlePointInputEventsSubscription(bool subscribe)
    {
        if (pointInput == null)
        {
            Debug.LogError("[ManualTargetSelector] PointInput is not assigned.");
            return;
        }

        if (subscribe)
        {
            pointInput.OnParticipantSelected += ParticipantSelected;
            pointInput.OnSelectionCancelled += SelectionCanceled;
        }
        else
        {
            pointInput.OnParticipantSelected -= ParticipantSelected;
            pointInput.OnSelectionCancelled -= SelectionCanceled;
        }
    }

    private IEnumerator GetPointInput(Action<List<Participant>> onCompleted)
    {
        if (pointInput == null)
        {
            pointInput = combatController.GetComponentInChildren<ParticipantPointInput>();
            if (pointInput == null)
            {
                pointInput = FindFirstObjectByType<ParticipantPointInput>();
                if (pointInput == null)
                {
                    Debug.LogError("[ManualTargetSelector] No ParticipantPointInput found in the scene. Please ensure one exists.");
                    onCompleted?.Invoke(new());
                    yield break;
                }
                else Debug.LogWarning("[ManualTargetSelector] No ParticipantPointInput found in CombatController's children. Using the first one found in the scene, however it is recommended to fix the hierarchy structure in the scene.");
            }
        }
    }

    protected void ParticipantSelected(Participant selected)
    {
        if (selected == null)
        {
            Debug.LogError("<color=yellow>[ManualTargetSelector]</color> ParticipantSelected was called with a null participant.");
            return;
        }
        
        selectedParticipant = selected;
    }

    protected void SelectionCanceled()
    {
        Debug.Log("<color=yellow>[ManualTargetSelector]</color> Target selection was canceled by the player.");
        selectionWasCanceled = true;
    }

    protected bool UpdateSelectionPoolMask()
    {
        if (selectionPoolMask == default)
        {
            selectionPoolMask = pointInput.pointableLayerMask;
            selectionPoolLayer = LayerMaskToLayerIndex(selectionPoolMask);
        }

        if (selectionPoolLayer < 0)
        {
            Debug.LogError("[ManualTargetSelector] PointableParticipant must resolve to exactly one Unity layer.");
            return false;
        }

        ClearCurrentSelectionPoolMask();


        List<Participant> selectionPool = GetSelectionPoolSelector().SelectTargets();

        if (selectionPool == null || selectionPool.Count == 0)
        {
            Debug.LogWarning("[ManualTargetSelector] Selection pool is empty. No targets available for selection.");
            pointInput.CancelSelection();
            return false;
        }

        foreach (var participant in selectionPool)
        {
            participant.gameObject.layer = selectionPoolLayer;
            currentSelectionPool.Add(participant);
        }
        return true;
    }

    protected void ClearCurrentSelectionPoolMask()
    {
        int defaultLayer = LayerMask.NameToLayer("Default");

        foreach (var participant in currentSelectionPool)
        {
            if (participant != null)
            {
                participant.gameObject.layer = defaultLayer;
            }
        }

        currentSelectionPool.Clear();
    }

    private static int LayerMaskToLayerIndex(LayerMask layerMask)
    {
        int maskValue = layerMask.value;

        if (maskValue == 0 || (maskValue & (maskValue - 1)) != 0)
        {
            return -1;
        }

        int layerIndex = 0;
        while ((maskValue >>= 1) != 0)
        {
            ++layerIndex;
        }

        return layerIndex;
    }

    protected virtual TargetSelector GetSelectionPoolSelector() => selectionPoolSelector;
}
