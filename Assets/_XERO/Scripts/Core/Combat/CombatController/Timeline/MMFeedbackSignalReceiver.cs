using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Timeline;

public class MMFeedbackSignalReceiver : MonoBehaviour
{
    [SerializeField] private MMFeedbacks feedbacks;
    [SerializeField] private SignalAsset[] signalAssets;
    [SerializeField] private bool enableDebug = false;

    private void OnSignalReceived()
    {
        if (feedbacks == null)
        {
            Debug.LogError("<color=black>[MMFeedbackSignalReceiver]</color> MMFeedbacks reference is not assigned.");
            return;
        }

        if (enableDebug) Debug.Log($"<color=black>[MMFeedbackSignalReceiver]</color> Signal received. Playing feedbacks.");
        feedbacks.PlayFeedbacks();
    }

    private void OnEnable()
    {
        Reset();

        if (signalAssets != null && signalAssets.Length != 0)
        {
            // Subscribe to the signals
            foreach (var signal in signalAssets)
            {
                if (signal != null)
                {
                    TimelineSignalBridge.SubscribeToSignal(true, signal, OnSignalReceived);
                }
            }
        }
        else
        {
            Debug.LogWarning("<color=black>[MMFeedbackSignalReceiver]</color> No SignalAssets assigned. Please assign at least one SignalAsset to receive signals.");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the signals
        if (signalAssets != null && signalAssets.Length != 0)
        {
            foreach (var signal in signalAssets)
            {
                if (signal != null)
                {
                    TimelineSignalBridge.SubscribeToSignal(false, signal, OnSignalReceived);
                }
            }
        }
    }

    private void Reset()
    {
        if (feedbacks == null)
        {
            if (TryGetComponent(out feedbacks))
            {
                if (enableDebug) Debug.Log($"<color=black>[MMFeedbackSignalReceiver]</color> MMFeedbacks reference found on the same GameObject.");
            }
            else
            {
                Debug.LogWarning("<color=black>[MMFeedbackSignalReceiver]</color> MMFeedbacks reference is not assigned. Attempting to get it from the same GameObject.");
            }
        }
    }
}
