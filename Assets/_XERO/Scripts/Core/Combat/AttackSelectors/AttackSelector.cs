using System.Collections.Generic;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[EnsureAssetInstance]
public abstract class AttackSelector: ScriptableObject
{
    public virtual UniTask<AttackDataSO> SelectAttackAsync(List<AttackDataSO> attacks, CancellationToken cancellationToken = default)
    {
        if (attacks == null || attacks.Count == 0)
        {
            Debug.LogError("[AttackSelector] No attacks provided for selection.");
            return UniTask.FromResult<AttackDataSO>(null);
        }
        
        return UniTask.FromResult(SelectAttack(attacks));
    }

    protected virtual AttackDataSO SelectAttack(List<AttackDataSO> attacks)
    {
        Debug.LogError("SelectAttack() called on an AttackSelector that is missing an implementation");
        return null;
    }
}