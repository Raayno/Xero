using UnityEngine;

[EnsureAssetInstance]
public abstract class PlayerBehavior_Module : ScriptableObject
{
    public bool IsBlockForcedTransition { get; private set; } = false;
    public bool RevertToDefaultModuleOnUnableToTransition { get; private set; } = true;

    protected PlayerBehavior_References refs;

    [SerializeField] protected bool enableDebug = false;
    [SerializeField] protected bool enableContinuousDebug = false;

    public void Enable(PlayerBehavior_References references)
    {
        refs = references;
        EnableModule();
        WakeUp();
    }

    protected virtual void EnableModule()
    {
        if (enableDebug) Debug.Log("[PlayerBehavior_Module] Enable() not implemented in " + GetType().Name);
    }

    public void UpdatePublic()
    {
        
        UpdateModule();
    }

    protected virtual void UpdateModule()
    {
        if (enableContinuousDebug) Debug.Log("[PlayerBehavior_Module] Update() not implemented in " + GetType().Name);
    }

    public void Disable()
    {

        PutToSleep();
        DisableModule();
    }

    protected virtual void DisableModule()
    {
        if (enableDebug) Debug.Log("[PlayerBehavior_Module] Disable() not implemented in " + GetType().Name);
    }

    public void WakeUp()
    {
        WakeUpModule();
    }

    /// <summary>
    /// Waking up and putting to sleep are used for enabling/disabling update-like behavior. For instance subscriptions to input events or coroutines.
    /// </summary>
    protected virtual void WakeUpModule()
    {
        if (enableDebug) Debug.Log("[PlayerBehavior_Module] WakeUp() not implemented in " + GetType().Name);
    }

    public void PutToSleep()
    {
        PutToSleepModule();
    }

    /// <summary>
    /// Waking up and putting to sleep are used for enabling/disabling update-like behavior. For instance subscriptions to input events or coroutines.
    /// </summary>
    protected virtual void PutToSleepModule()
    {
        if (enableDebug) Debug.Log("[PlayerBehavior_Module] PutToSleep() not implemented in " + GetType().Name);
    }

    protected void TransitionToModule(PlayerBehavior_Module newModule)
    {
        if (refs.playerBehavior == null)
        {
            Debug.LogError("[PlayerBehavior_Module] refs.playerBehavior is null. Cannot transition to module: " + newModule.GetType().Name);
            return;
        }

        refs.playerBehavior.TryTransition(this, newModule);
    }

    public virtual void OnSignalReceived(UnityEngine.Timeline.SignalAsset signal)
    {
        if (enableDebug) Debug.Log("[PlayerBehavior_Module] OnSignalReceived() not implemented in " + GetType().Name + " for signal: " + signal.name);
    }

    public virtual void OnDrawGizmos()
    {
        if (enableContinuousDebug) Debug.Log("[PlayerBehavior_Module] OnDrawGizmos() not implemented in " + GetType().Name);
    }
}
