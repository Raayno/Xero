using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using StarterAssets;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(PlayableDirector))]
public class ParryThirdPersonControllerExtension : SignalReceiver
{
    [Header("Parry Settings")]
    [SerializeField] private TimelineAsset parryTimelineAsset;
    [Header("References")]
    [SerializeField] private IconCooldownController parryIconCooldownController;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private PlayerAnimationManager playerAnimationManager;
    [SerializeField] private PlayableDirector playableDirector;

    void OnEnable()
    {
        if (parryTimelineAsset == null)
        {
            Debug.LogError("Parry Timeline Asset is not assigned.");
            enabled = false; // Disable this component to prevent further errors
            return;
        }

        ParryInput.Instance.OnParry += HandleParryInput;
        playableDirector.stopped += OnPlayableDirectorStopped;


        if (parryIconCooldownController != null)
        {
            parryIconCooldownController.gameObject.SetActive(true);
            parryIconCooldownController.SkipCooldown();
        }
    }

    void OnDisable()
    {
        if (ParryInput.HasInstance)
        {
            ParryInput.Instance.OnParry -= HandleParryInput;
        }

        if (playableDirector != null)
        {
            playableDirector.stopped -= OnPlayableDirectorStopped;
        }

        if (parryIconCooldownController != null)
        {
            parryIconCooldownController.gameObject.SetActive(false);
        }

        OnParryEnd(); // Ensure that we reset the state when this component is disabled
    }

    private void HandleParryInput()
    {
        if (CannotParry())
        {
            Debug.Log("Cannot parry at this time.");
            return;
        }

        Parry();
    }

    private bool CannotParry()
    {
        if (playableDirector.state == PlayState.Playing)
        {
            Debug.Log("Parry is already in progress.");
            return true;
        }

        return false; // Player can parry
    }

    private void Parry()
    {
        // disable movement animations
        playerAnimationManager.SetMovementBlend(0f, 0f); // Reset movement blend to idle
        playerAnimationManager.enabled = false;

        // Start the parry timeline
        playableDirector.playableAsset = parryTimelineAsset;
        playableDirector.Play();

        // Start the cooldown for the parry ability
        if (parryIconCooldownController != null)
        {
            parryIconCooldownController.StartCooldown((float)parryTimelineAsset.duration);
        }
    }

    private BattleEntryType cashedBattleEntryType = BattleEntryType.EnemyAttack;
    public void OnParrySignal()
    {
        if (SpecialCombatDataCarrier.BattleEntryType != BattleEntryType.PlayerParry)
        {
            cashedBattleEntryType = SpecialCombatDataCarrier.BattleEntryType; // Cache the current state before parry
            SpecialCombatDataCarrier.BattleEntryType = BattleEntryType.PlayerParry; // Set to PlayerParry during the parry window (to carry that data into combat if it were entered during the parry window)
        }
        else
        {
            SpecialCombatDataCarrier.BattleEntryType = cashedBattleEntryType; // Reset to the previous state after parry
        }
    }

    private void OnParryEnd()
    {
        // Re-enable movement animations
        playerAnimationManager.enabled = true;

        if (SpecialCombatDataCarrier.BattleEntryType == BattleEntryType.PlayerParry)
        {
            SpecialCombatDataCarrier.BattleEntryType = BattleEntryType.EnemyAttack; // Reset to default state if parry ends mid-parry-window
        }
    }
    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (director == playableDirector)
        {
            OnParryEnd();
        }
    }
    
    void Start()
    {
        OnValidate(); // Ensure references are set up correctly
    }

    private void OnValidate()
    {
        if (thirdPersonController == null)
        {
            thirdPersonController = GetComponent<ThirdPersonController>();
        }

        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }
}
