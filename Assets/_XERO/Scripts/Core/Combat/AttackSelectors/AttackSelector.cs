using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

[EnsureAssetInstance]
public abstract class AttackSelector: ScriptableObject
{
    public virtual IEnumerator SelectAttackAsync(List<AttackDataSO> attacks, Action<AttackDataSO> onCompleted)
    {
        if (attacks == null || attacks.Count == 0)
        {
            Debug.LogError("[AttackSelector] No attacks provided for selection.");
            onCompleted?.Invoke(null);
            yield break;
        }
        
        onCompleted?.Invoke(SelectAttack(attacks));
        yield break;
    }

    protected virtual AttackDataSO SelectAttack(List<AttackDataSO> attacks)
    {
        Debug.LogError("SelectAttack() called on an AttackSelector that is missing an implementation");
        return null;
    }
}