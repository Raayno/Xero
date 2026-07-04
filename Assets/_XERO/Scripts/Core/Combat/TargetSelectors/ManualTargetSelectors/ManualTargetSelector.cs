using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[IgnoreAssetInstanceEnsurement]
[CreateAssetMenu(fileName = "ManualTargetSelector", menuName = "Combat/TargetSelectors/ManualTargetSelector")]
public class ManualTargetSelector : TargetSelector
{
    [SerializeField] protected TargetSelector selectionPoolSelector;
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

    protected override async UniTask<List<Participant>> SelectTargetsAsync(Participant self, CancellationToken cancellationToken)
    {
        try
        {
            if (!UpdateSelectionPoolMask())
            {
                return new();
            }

            selectedParticipant = null;
            selectionWasCanceled = false;
            HandlePointInputEventsSubscription(true);
            ParticipantPointInput.Instance.StartSelection();
            
            Debug.Log("<color=yellow>[ManualTargetSelector]</color> Awaiting player input for target selection...");
            await UniTask.WaitUntil(() => selectionWasCanceled || selectedParticipant != null, cancellationToken: cancellationToken);

            if (selectionWasCanceled || ParticipantPointInput.Instance.PointedParticipant == null)
            {
                return new();
            }

            Debug.Log($"<color=yellow>[ManualTargetSelector]</color> Player selected target: {ParticipantPointInput.Instance.PointedParticipant.CombatantName}");
            return new() { ParticipantPointInput.Instance.PointedParticipant };
        }
        finally
        {
            HandlePointInputEventsSubscription(false);
            ClearCurrentSelectionPoolMask();
            if (ParticipantPointInput.Instance != null) ParticipantPointInput.Instance.StopSelection();
        }
    }

    protected virtual void HandlePointInputEventsSubscription(bool subscribe)
    {
        if (subscribe)
        {
            ParticipantPointInput.Instance.OnParticipantSelected += ParticipantSelected;
            ParticipantPointInput.Instance.OnSelectionCancelled += SelectionCanceled;
        }
        else
        {
            ParticipantPointInput.Instance.OnParticipantSelected -= ParticipantSelected;
            ParticipantPointInput.Instance.OnSelectionCancelled -= SelectionCanceled;
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
            selectionPoolMask = ParticipantPointInput.Instance.pointableLayerMask;
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
            ParticipantPointInput.Instance.CancelSelection();
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

    protected virtual TargetSelector GetSelectionPoolSelector()
    {
        if (selectionPoolSelector is ManualTargetSelector) throw new System.InvalidOperationException("[ManualTargetSelector] Selection pool selector cannot be another ManualTargetSelector to avoid infinite recursion.");
        return selectionPoolSelector;
    }
}
