using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ManualTargetSelector : TargetSelector
{
    [SerializeField] protected ParticipantPointInput pointInput;
    protected LayerMask selectionPoolMask;
    protected int selectionPoolLayer = -1;
    protected bool selectionWasCanceled;
    [SerializeField] protected bool enableDebug = true;
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
        if (pointInput == null)
        {
            pointInput = combatController.GetComponentInChildren<ParticipantPointInput>();
        }

        selectionWasCanceled = false;
        pointInput.OnSelectionCancelled += SelectionCanceled;

        if (!UpdateSelectionPoolMask())
        {
            pointInput.OnSelectionCancelled -= SelectionCanceled;
            onCompleted?.Invoke(new());
            yield break;
        }

        pointInput.StartSelection();

        float startTime = Time.time;
        bool alreadyWarned = false;
        while (pointInput.PointedParticipant == null && !selectionWasCanceled)
        {
            if (enableDebug && !alreadyWarned && Time.time - startTime > 5f)
            {
                Debug.LogWarning("Waiting for player to select a target for more than 5 seconds. Make sure the player can select a target and that the selection pool is not empty.");
                alreadyWarned = true;
            }

            yield return null;
        }

        pointInput.OnSelectionCancelled -= SelectionCanceled;

        if (selectionWasCanceled || pointInput.PointedParticipant == null)
        {
            onCompleted?.Invoke(new());
            yield break;
        }

        onCompleted?.Invoke(new() { pointInput.PointedParticipant });
    }

    protected void OnDisable()
    {
        pointInput.OnSelectionCancelled -= SelectionCanceled;
    }

    protected void SelectionCanceled()
    {
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

    protected abstract TargetSelector GetSelectionPoolSelector();
}
