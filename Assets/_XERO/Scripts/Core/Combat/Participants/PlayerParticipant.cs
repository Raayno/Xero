using UnityEngine;
using UnityEngine.Timeline;
using System.Collections;

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
        if (canParry) StartCoroutine(ExecuteParrySequence());
    }

    private IEnumerator ExecuteParrySequence()
    {
        if (playableDirector == null)
        {
            Debug.LogError("[PlayerParticipant] Parry Director is not assigned.");
            yield break;
        }

        isTrueParry = false;
        TimelineSignalBridge.SubscribeToNotifications(true, HandleParrySignal);

        playableDirector.playableAsset = parryTimelineAsset;
        playableDirector.Play();

        canParry = false;
        yield return new WaitForSeconds((float)playableDirector.duration);
        canParry = true;

        TimelineSignalBridge.SubscribeToNotifications(false, HandleParrySignal);
    }

    private void HandleParrySignal(SignalAsset signal)
    {
        if (signal == parrySignalAsset)
        {
            isTrueParry = !isTrueParry;
            Debug.Log($"<color=green>[PlayerParticipant]</color> True Parry window of {combatantName} is now {(isTrueParry ? "open" : "closed")}.");
        }
    }
}