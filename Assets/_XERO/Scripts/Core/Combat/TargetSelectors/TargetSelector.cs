using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

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

    public IEnumerator SelectTargetsAsync(Participant self, Action<List<Participant>> onCompleted, bool doNotAssignThisValueIsForDifferentiation = false)
    {
        combatController = CombatController.Instance;
        yield return SelectTargetsAsync(self, onCompleted);
    }

    protected virtual IEnumerator SelectTargetsAsync(Participant self, Action<List<Participant>> onCompleted)
    {
        onCompleted?.Invoke(SelectTargets(self));
        yield break;
    }

    protected virtual List<Participant> SelectTargets(Participant self) => SelectTargets();

    protected virtual List<Participant> SelectTargets()
    {
        Debug.LogError("SelectTargets() called on a TargetSelector that is missing an implementation");
        return new();
    }
}