using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using StarterAssets;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(PlayableDirector))]
public class ParryThirdPersonControllerExtension : MonoBehaviour
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
        if (ParryInput.Instance != null)
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
        // Check if the player is grounded
        if (!thirdPersonController.Grounded)
        {
            Debug.Log("Player is not grounded.");
            return true;
        }
        
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
        thirdPersonController.BlockMovement = true;

        // Start the parry timeline
        playableDirector.playableAsset = parryTimelineAsset;
        playableDirector.Play();

        // Start the cooldown for the parry ability
        if (parryIconCooldownController != null)
        {
            parryIconCooldownController.StartCooldown((float)parryTimelineAsset.duration);
        }
    }

    private void OnParryEnd()
    {
        // Re-enable movement animations
        playerAnimationManager.enabled = true;
        thirdPersonController.BlockMovement = false;
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

        if (playerAnimationManager == null && thirdPersonController != null)
        {
            playerAnimationManager = thirdPersonController.AnimationManager;
        }

        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }
}
