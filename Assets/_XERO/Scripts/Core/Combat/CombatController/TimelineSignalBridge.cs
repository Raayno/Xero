using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineSignalBridge : MonoBehaviour, INotificationReceiver
{
    public System.Action<SignalAsset> OnSignalReceived;

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SignalEmitter emitter && emitter.asset != null)
        {
            Debug.Log($"<color=#FF69B4>[TimelineSignalBridge]</color> Signal received: {emitter.asset.name}");
            OnSignalReceived?.Invoke(emitter.asset);
        }
    }

    private static bool GetSignalBridge(out TimelineSignalBridge signalBridge)
    {
        signalBridge = CombatController.Instance.GetComponentInChildren<TimelineSignalBridge>();
        if (signalBridge == null)
        {
            Debug.LogError("<color=pink>[TimelineSignalBridge]</color> TimelineSignalBridge not found in CombatController's children.");
            return false;
        }
        return true;
    }

    public static bool GetSignalBridge(TimelineSignalBridge signalBridge, out TimelineSignalBridge foundSignalBridge)
    {
        foundSignalBridge = signalBridge;
        if (foundSignalBridge != null) return true;
        return GetSignalBridge(out foundSignalBridge);
    }

    public static void SubscribeToNotifications(bool isSubscribe, System.Action<SignalAsset> callback)
    {
        if (!GetSignalBridge(out TimelineSignalBridge signalBridge)) return;

        if (isSubscribe)
        {
            signalBridge.OnSignalReceived += callback;
        }
        else
        {
            signalBridge.OnSignalReceived -= callback;
        }
    }
}