using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

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

    protected override async UniTask<List<Participant>> SelectTargetsAsync(Participant self, CancellationToken cancellationToken)
    {
        if (!EnsurePointInput())
        {
            return new();
        }

        try
        {
            if (!UpdateSelectionPoolMask())
            {
                return new();
            }

            selectedParticipant = null;
            selectionWasCanceled = false;
            HandlePointInputEventsSubscription(true);
            pointInput.StartSelection();
            
            Debug.Log("<color=yellow>[ManualTargetSelector]</color> Awaiting player input for target selection...");
            await UniTask.WaitUntil(() => selectionWasCanceled || selectedParticipant != null, cancellationToken: cancellationToken);

            if (selectionWasCanceled || pointInput.PointedParticipant == null)
            {
                return new();
            }

            Debug.Log($"<color=yellow>[ManualTargetSelector]</color> Player selected target: {pointInput.PointedParticipant.CombatantName}");
            return new() { pointInput.PointedParticipant };
        }
        finally
        {
            HandlePointInputEventsSubscription(false);
            ClearCurrentSelectionPoolMask();
            if (pointInput != null) pointInput.StopSelection();
        }
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

    private bool EnsurePointInput()
    {
        if (pointInput == null)
        {
            pointInput = combatController.ParticipantPointInput;
            if (pointInput == null)
            {
                pointInput = combatController.GetComponentInChildren<ParticipantPointInput>();
                if (pointInput == null)
                {
                    pointInput = FindFirstObjectByType<ParticipantPointInput>();
                    if (pointInput == null)
                    {
                        Debug.LogError("[ManualTargetSelector] No ParticipantPointInput found in the scene. Please ensure one exists.");
                        return false;
                    }
                    else Debug.LogWarning("[ManualTargetSelector] No ParticipantPointInput found in CombatController's children. Using the first one found in the scene, however it is recommended to fix the hierarchy structure in the scene.");
                }
            }
        }

        return true;
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
