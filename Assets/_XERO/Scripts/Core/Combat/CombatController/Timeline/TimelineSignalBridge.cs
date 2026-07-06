using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using System.Collections.Generic;
using MoreMountains.Tools;

public class TimelineSignalBridge : MMSingleton<TimelineSignalBridge>, INotificationReceiver
{
    [SerializeField] private bool enableDebug = false;
    public Action<SignalAsset> OnSignalReceived;

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SignalEmitter emitter && emitter.asset != null)
        {
            if (enableDebug) Debug.Log($"<color=#FF69B4>[TimelineSignalBridge]</color> Signal received: {emitter.asset.name}");
            OnSignalReceived?.Invoke(emitter.asset);
        }
    }
    
    public static void SubscribeToSignal(bool isSubscribe, Action<SignalAsset> callback)
    {
        if (isSubscribe)
        {
            Instance.OnSignalReceived += callback;
        }
        else
        {
            Instance.OnSignalReceived -= callback;
        }
        
        if (Instance.enableDebug) Debug.Log($"<color=#FF69B4>[TimelineSignalBridge]</color> {(isSubscribe ? "Subscribed to" : "Unsubscribed from")} notifications with callback: {callback.Method.Name}");
    }

    private struct SubscriptionKey : IEquatable<SubscriptionKey>
    { 
        public SignalAsset signalAsset; 
        public Action callback; 

        public readonly bool Equals(SubscriptionKey other) => signalAsset == other.signalAsset && callback == other.callback;
        public override readonly bool Equals(object obj) => obj is SubscriptionKey other && Equals(other);
        public override readonly int GetHashCode() => HashCode.Combine(signalAsset, callback);
    }
    private struct SubscriptionValue { public Action<SignalAsset> signalHandler; }
    private static readonly Dictionary<SubscriptionKey, SubscriptionValue> signalHandlers = new();
    public static void SubscribeToSignal(bool isSubscribe, SignalAsset signalAssetToSubscribeTo, Action callback)
    {
        if (callback == null || signalAssetToSubscribeTo == null) return;

        SubscriptionKey key = new() { signalAsset = signalAssetToSubscribeTo, callback = callback };
        if (isSubscribe)
        {
            if (signalHandlers.ContainsKey(key))
            {
                Debug.LogWarning("<color=#FF69B4>[TimelineSignalBridge]</color> Already subscribed to this signal with the provided callback.");
                return;
            }
            void signalHandler(SignalAsset signal) => CompareAndNotify(signal, key);
            signalHandlers[key] = new SubscriptionValue { signalHandler = signalHandler };
            Instance.OnSignalReceived += signalHandler;
        }
        else
        {
            if (!signalHandlers.TryGetValue(key, out var v)) return;
            Instance.OnSignalReceived -= v.signalHandler;
            signalHandlers.Remove(key);
        }

        if (Instance.enableDebug) Debug.Log($"<color=#FF69B4>[TimelineSignalBridge]</color> {(isSubscribe ? "Subscribed to" : "Unsubscribed from")} notifications for signal: {signalAssetToSubscribeTo.name} with callback: {callback.Method.Name}");
    }

    private static void CompareAndNotify(SignalAsset signalAsset, SubscriptionKey context)
    {
        if (signalAsset == null) return;
        if (context.signalAsset != signalAsset) return;

        context.callback?.Invoke();
    }

    public static void UnsubscribeAll()
    {
        foreach (var kvp in signalHandlers)
        {
            Instance.OnSignalReceived -= kvp.Value.signalHandler;
        }
        signalHandlers.Clear();

        if (Instance.enableDebug) Debug.Log("<color=#FF69B4>[TimelineSignalBridge]</color> Unsubscribed from all notifications.");
    }
}
