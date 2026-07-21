using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

[IgnoreAssetInstanceEnsurement]
[CreateAssetMenu(fileName = "ManualTargetSelector", menuName = "Combat/TargetSelectors/ManualTargetSelector")]
public class ManualTargetSelector : TargetSelector
{
    [SerializeField] protected TargetSelector selectionPoolSelector;
    [Tooltip("The maximum number of targets the player can select. Set to 0 for no limit.")]
    [SerializeField] protected int maxSelectableTargets = 1;
    [SerializeField] protected bool enableDebug = false;

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
        List<Participant> selectionPool = GetSelectionPoolSelector().SelectTargets();

        if (selectionPool == null || selectionPool.Count == 0)
        {
            Debug.LogWarning("[ManualTargetSelector] Selection pool is empty. No targets available for selection.");
            return null;
        }
        
        List<Participant> selectedParticipants = await ParticipantSelectionInput.Instance.AwaitPlayerSelectionAsync(maxSelectableTargets, selectionPool, cancellationToken);

        if (selectedParticipants == null || selectedParticipants.Count == 0)
        {
            Debug.LogWarning("[ManualTargetSelector] No participants were selected by the player.");
            return null;
        }

        if (enableDebug)
        {
            string selectedNames = string.Join(", ", selectedParticipants.Select(p => p.CombatantName));
            Debug.Log($"<color=yellow>[ManualTargetSelector]</color> Player selected {selectedParticipants.Count} target(s): {selectedNames}");
        }

        return selectedParticipants;
    }

    protected virtual TargetSelector GetSelectionPoolSelector()
    {
        if (selectionPoolSelector is ManualTargetSelector) throw new System.InvalidOperationException("[ManualTargetSelector] Selection pool selector cannot be another ManualTargetSelector to avoid infinite recursion.");
        return selectionPoolSelector;
    }
}
