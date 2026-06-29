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
            OnSignalReceived?.Invoke(emitter.asset);
        }
    }
}