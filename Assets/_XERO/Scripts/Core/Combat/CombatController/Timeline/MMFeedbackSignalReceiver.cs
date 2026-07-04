using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Timeline;

public class MMFeedbackSignalReceiver : MonoBehaviour
{
    [SerializeField] private MMFeedbacks feedbacks;
    [SerializeField] private SignalAsset[] signalAssets;
    [SerializeField] private bool enableDebug = false;

    private void OnEnable()
    {
        if (feedbacks == null)
        {
            if (TryGetComponent(out feedbacks))
            {
                if (enableDebug) Debug.Log($"<color=black>[MMFeedbackSignalReceiver]</color> MMFeedbacks reference found on the same GameObject.");
            }
            else
            {
                Debug.LogError("<color=black>[MMFeedbackSignalReceiver]</color> MMFeedbacks reference is not assigned. Attempting to get it from the same GameObject.");
            }
        }

        if (signalAssets == null || signalAssets.Length == 0)
        {
            Debug.LogWarning("<color=black>[MMFeedbackSignalReceiver]</color> No SignalAssets assigned. Please assign at least one SignalAsset to receive signals.");
            return;
        }

        foreach (var signal in signalAssets)
        {
            if (signal != null)
            {
                TimelineSignalBridge.SubscribeToSignal(true, signal, OnSignalReceived);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var signal in signalAssets)
        {
            if (signal != null)
            {
                TimelineSignalBridge.SubscribeToSignal(false, signal, OnSignalReceived);
            }
        }
    }

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
}