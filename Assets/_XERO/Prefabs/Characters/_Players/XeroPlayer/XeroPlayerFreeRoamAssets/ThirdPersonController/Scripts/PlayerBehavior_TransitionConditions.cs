using UnityEngine;
// using Alchemy.Inspector;

public partial class PlayerBehavior : MonoBehaviour
{
    private bool CanTransition(PlayerBehavior_Module newModule)
    {
        if (newModule == null) return true;

        if (!availableModules.ContainsKey(newModule))
        {
            Debug.Log("[PlayerBehavior] Transition blocked, module not available: " + newModule.GetType().Name + ". Please make sure to add the module to the availableModules dictionary in the PlayerBehavior inspector.");
            return false;
        }

        var conditions = availableModules[newModule];
        foreach (var condition in conditions)
        {
            switch (condition)
            {
                case TransitionConditionType.None:
                    break;
                case TransitionConditionType.IsGrounded:
                    if (!IsGrounded)
                    {
                        Debug.Log("[PlayerBehavior] Transition blocked, condition not met: IsGrounded");
                        return false;
                    }
                    break;
                case TransitionConditionType.IsNotGrounded:
                    if (IsGrounded)
                    {
                        Debug.Log("[PlayerBehavior] Transition blocked, condition not met: IsNotGrounded");
                        return false;
                    }
                    break;
                default:
                    Debug.Log("[PlayerBehavior] Transition blocked, unknown condition: " + condition);
                    return false;
            }
        }

        return true;
    }

    [System.Serializable]
    private enum TransitionConditionType
    {
        None,
        IsGrounded,
        IsNotGrounded,
    }

    #region IsGrounded
    [Gaskellgames.ReadOnly, SerializeField] private bool isGrounded;
    private int lastIsGroundedCheckFrame = -1;

    // [Group("1) IsGrounded")]
    [Tooltip("Useful for rough ground")]
    [SerializeField] private float GroundedOffset = -0.14f;
    
    // [Group("1) IsGrounded")]

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float GroundedRadius = 0.28f;
    
    // [Group("1) IsGrounded")]

    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask GroundLayers;

    // [Group("1) IsGrounded")]
    [SerializeField] private bool isGroundedGizmos = true;

    public bool IsGrounded
    {
        get
        {
            if (WasNotSetThisFrame(lastIsGroundedCheckFrame)) IsGroundedCheck();
            return isGrounded;
        }
    }

    public Vector3? LastGroundedPosition { get; private set; }

    private void IsGroundedCheck()
    {
        Vector3 spherePosition = new(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );

        isGrounded = Physics.CheckSphere(
            spherePosition,
            GroundedRadius,
            GroundLayers,
            QueryTriggerInteraction.Ignore
        );

        lastIsGroundedCheckFrame = Time.frameCount;

        if (isGrounded)
        {
            LastGroundedPosition = refs.playerTransform.position;
        }
        // TODO: This is a temporary solution, until animation manager is fully integrated with player behavior. Once it is, we can remove this and just have the animation manager listen to the IsGrounded property.
        if (refs?.animationManager) refs?.animationManager.SetGrounded(isGrounded);
    }

    private void IsGroundedGizmos()
    {
        Color transparentGreen = new (0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new (1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = IsGrounded ? transparentGreen : transparentRed;

        Gizmos.DrawSphere(
            new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            ),
            GroundedRadius
        );
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (isGroundedGizmos) IsGroundedGizmos();
    }

    private bool WasNotSetThisFrame(int lastCheckFrame)
    {
        return lastCheckFrame != Time.frameCount;
    }
}
