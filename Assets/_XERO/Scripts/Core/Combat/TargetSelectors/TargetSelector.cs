using System.Collections.Generic;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[EnsureAssetInstance]
public abstract class TargetSelector: ScriptableObject
{
    protected static CombatController combatController;

    /// <summary>
    /// Selects targets based on the provided selectors. If no selectors are provided, it will use the default SelectTargets() method. If a self participant is provided, it will use the SelectTargets(Participant self) method.
    /// </summary>
    /// <param name="selectors">Let's you construct a custom TargetSelector. For example select all allies and a random enemy</param>
    /// <returns></returns>
    public List<Participant> SelectTargets(Participant self = null, bool doNotAssignThisValueIsForDifferentiation = false)
    {
        if (self == null) return SelectTargets();
        return SelectTargets(self);
    }

    public UniTask<List<Participant>> SelectTargetsAsync(Participant self, CancellationToken cancellationToken = default, bool doNotAssignThisValueIsForDifferentiation = false)
    {
        combatController = CombatController.Instance;
        return SelectTargetsAsync(self, cancellationToken);
    }

    protected virtual UniTask<List<Participant>> SelectTargetsAsync(Participant self, CancellationToken cancellationToken)
    {
        return UniTask.FromResult(SelectTargets(self));
    }

    protected virtual List<Participant> SelectTargets(Participant self) => SelectTargets();

    protected virtual List<Participant> SelectTargets()
    {
        Debug.LogError("SelectTargets() called on a TargetSelector that is missing an implementation");
        return new();
    }
}