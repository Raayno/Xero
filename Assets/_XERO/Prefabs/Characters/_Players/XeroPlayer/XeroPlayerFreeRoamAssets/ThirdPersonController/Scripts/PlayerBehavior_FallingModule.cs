using UnityEngine;
using UnityEngine.Rendering;

public class PlayerBehavior_FallingModule : PlayerBehavior_Module
{
    [SerializeField] private PlayerBehavior_MovementModule movementModule;

    [Header("Landing Impact")]
    [Tooltip("X = fall distance for minimum impact, Y = fall distance for maximum impact.")]
    [SerializeField] private Vector2 fallDistanceImpactRange = new(1f, 10f);

    [Tooltip("X = minimum feedback impact, Y = maximum feedback impact.")]
    [SerializeField] private Vector2 feedbackImpactRange = new(0.25f, 1f);

    [SerializeField] private bool dieOnFallDistanceExceed = false;
    [ShowIf("dieOnFallDistanceExceed")]
    [SerializeField] private float fallDistanceToDie = 15f;
    
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float Gravity = -15.0f;

    private float fallStartHeight;
    private float verticalVelocity;
    private readonly float terminalVelocity = 53.0f;

    protected override void EnableModule()
    {
        refs.animationManager.SetFreeFall(true);

        fallStartHeight = refs.playerTransform.position.y;
        verticalVelocity = refs.characterController.velocity.y;
    }

    protected override void UpdateModule()
    {
        if (refs.playerBehavior.IsGrounded)
        {
            Land();
        }

        if (dieOnFallDistanceExceed && !refs.playerBehavior.IsGrounded)
        {
            float currentFallDistance = Mathf.Max(
                0f,
                fallStartHeight - refs.playerTransform.position.y
            );

            if (currentFallDistance >= fallDistanceToDie)
            {
                if (enableDebug)
                {
                    Debug.Log($"<color=red>[Landing Impact]</color> Fall distance {currentFallDistance} exceeded threshold {fallDistanceToDie}. Triggering death.");
                }

                RespawnAtLastGroundedPosition();
            }
        }
    }

    private void JumpAndGravity()
    {
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Mathf.Clamp(Gravity * Time.deltaTime, -terminalVelocity, terminalVelocity);
        }

        refs.characterController.Move(new Vector3(
            refs.characterController.velocity.x,
            verticalVelocity,
            refs.characterController.velocity.z
        ));
    }

    private void RespawnAtLastGroundedPosition()
    {
        Vector3 respawnPosition = refs.playerBehavior.LastGroundedPosition ?? Vector3.up * 1000f;

        if (enableDebug)
        {
            if (refs.playerBehavior.LastGroundedPosition.HasValue)
            {
                Debug.Log($"<color=yellow>[Landing Impact]</color> Respawning player at last grounded position: {respawnPosition}");
            }
            else
            {
                Debug.LogWarning("<color=yellow>[Landing Impact]</color> Last grounded position is unknown. Respawning player at default position.");
            }
        }

        refs.characterController.enabled = false; // Disable to avoid collision issues
        refs.playerTransform.position = respawnPosition;
        refs.characterController.enabled = true;  // Re-enable after repositioning
    }

    private void Land()
    {
        float fallDistance = Mathf.Max(
            0f,
            fallStartHeight - refs.playerTransform.position.y
        );

        float feedbackImpact = CalculateLandingImpact(fallDistance);
        refs.feedbacks.PlayFeedback(FeedbackType.FreeRoamPlayerLandAfterFall, refs.playerTransform.position, feedbackImpact);

        if (enableDebug)
        {
            Debug.Log($"<color=green>[Landing Impact]</color> Fall distance: {fallDistance}. Applying feedback impact: {feedbackImpact}");
        }

        TransitionToModule(movementModule);
    }
    
    private float CalculateLandingImpact(float fallDistance)
    {
        float minimumDistance = Mathf.Min(fallDistanceImpactRange.x, fallDistanceImpactRange.y);
        float maximumDistance = Mathf.Max(fallDistanceImpactRange.x, fallDistanceImpactRange.y);

        float minimumImpact = Mathf.Min(feedbackImpactRange.x, feedbackImpactRange.y);
        float maximumImpact = Mathf.Max(feedbackImpactRange.x, feedbackImpactRange.y);

        if (Mathf.Approximately(minimumDistance, maximumDistance))
            return maximumImpact;

        float normalizedImpact = Mathf.InverseLerp(
            minimumDistance,
            maximumDistance,
            fallDistance
        );

        return Mathf.Lerp(
            minimumImpact,
            maximumImpact,
            normalizedImpact
        );
    }

    protected override void DisableModule()
    {
        refs.animationManager.SetFreeFall(false);
    }
}
