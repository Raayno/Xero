using UnityEditor;
using UnityEngine;

public class ManualEnemyTargetSelector : ManualTargetSelector
{
    [SerializeField] private AllEnemiesTargetSelector selectionPoolSelector;

    protected override TargetSelector GetSelectionPoolSelector() => selectionPoolSelector;

    protected void Reset()
    {
        selectionPoolSelector = selectionPoolSelector != null ? selectionPoolSelector : AssetDatabase.LoadAssetByGUID<AllEnemiesTargetSelector>(AssetDatabase.FindAssetGUIDs($"t:AllEnemiesTargetSelector .asset")[0]);
    }
}