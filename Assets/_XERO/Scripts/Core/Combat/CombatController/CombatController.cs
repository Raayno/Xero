using UnityEngine;

public partial class CombatController : MoreMountains.Tools.MMSingleton<CombatController>
{
    [Header("Turn Management")]
    [SerializeField] private TurnSelector turnSelector;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    [Gaskellgames.Button]
    public void ResetCombat()
    {
        CleanseCombat();
        Start();
    }

    private void Start()
    {
        InitializeCombat();

        RunCombatLoop();
    }

    private void CleanseCombat()
    {
        StopCombatLoop();

        // Reset the turn selector
        if (turnSelector != null) turnSelector.ResetTimeline();

        // Clear all subscriptions to timeline signals (game-wide)
        TimelineSignalBridge.UnsubscribeAll();

        RemoveAndDestroyAllParticipants();
    }

    void OnDestroy()
    {
        CleanseCombat();
    }
    
    void OnValidate()
    {
        OnValidateInitialization();
    }
}
