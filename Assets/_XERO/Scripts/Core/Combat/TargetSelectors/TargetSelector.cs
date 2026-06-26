using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TargetSelector: MonoBehaviour
{
    [SerializeField] protected CombatController combatController;

    /// <summary>
    /// Selects targets based on the provided selectors. If no selectors are provided, it will use the default SelectTargets() method. If a self participant is provided, it will use the SelectTargets(Participant self) method.
    /// </summary>
    /// <param name="selectors">Let's you construct a custom TargetSelector. For example select all allies and a random enemy</param>
    /// <returns></returns>
    public List<Participant> SelectTargets(Participant self = null, List<TargetSelector> selectors = null)
    {
        if (selectors != null)
        {
            var targets = new List<Participant>();
            foreach (var selector in selectors)
            {
                targets.AddRange(selector.SelectTargets().Except(targets));
            }
            return targets;
        }

        if (self == null) return SelectTargets();
        return SelectTargets(self);
    }

    protected virtual List<Participant> SelectTargets(Participant self) => SelectTargets();

    protected virtual List<Participant> SelectTargets()
    {
        Debug.LogError("SelectTargets() called on a TargetSelector that is missing an implementation");
        return new();
    }
}