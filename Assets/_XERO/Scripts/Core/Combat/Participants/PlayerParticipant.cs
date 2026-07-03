using UnityEngine;
using UnityEngine.Timeline;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerParticipant : Participant
{
    [Header("Player-specific")]
    [SerializeField] private TimelineAsset parryTimelineAsset;
    [SerializeField] private SignalAsset parrySignalAsset;
    private bool canParry = true;
    private bool isTrueParry = false;
    public bool IsTrueParry => isTrueParry;

    public void OnParry()
    {
        if (canParry) ExecuteParrySequenceAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask ExecuteParrySequenceAsync(CancellationToken cancellationToken)
    {
        isTrueParry = false;
        canParry = false;

        try
        {
            TimelineSignalBridge.SubscribeToSignal(true, parrySignalAsset, OnParrySignal);

            TimelineManager.PlayTimeline(parryTimelineAsset, Animator);

            await UniTask.Delay(System.TimeSpan.FromSeconds(parryTimelineAsset.duration), cancellationToken: cancellationToken);
        }
        finally
        {
            canParry = true;
            TimelineSignalBridge.SubscribeToSignal(false, parrySignalAsset, OnParrySignal);
            isTrueParry = false;
        }
    }

    private void OnParrySignal()
    {
        isTrueParry = !isTrueParry;
        if (enableDebug) Debug.Log($"<color=green>[PlayerParticipant]</color> True Parry window of {combatantName} is now {(isTrueParry ? "open" : "closed")}.");
    }
}