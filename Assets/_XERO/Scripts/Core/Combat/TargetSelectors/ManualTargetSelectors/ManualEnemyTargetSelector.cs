[UnityEngine.RequireComponent(typeof(AllEnemiesTargetSelector))]
public class ManualEnemyTargetSelector : ManualTargetSelector
{
    private TargetSelector enemyTargetSelector;

    protected override TargetSelector GetSelectionPoolSelector() => enemyTargetSelector;

    protected override void Reset()
    {
        base.Reset();        
        enemyTargetSelector = enemyTargetSelector != null ? enemyTargetSelector : GetComponent<AllEnemiesTargetSelector>();
    }
}