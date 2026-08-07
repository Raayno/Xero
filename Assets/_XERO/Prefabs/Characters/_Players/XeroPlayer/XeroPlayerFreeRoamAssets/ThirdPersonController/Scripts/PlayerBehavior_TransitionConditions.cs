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

    [Tooltip("Useful for rough ground")]
    [SerializeField] private float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask GroundLayers;

    public bool IsGrounded
    {
        get
        {
            if (WasNotSetThisFrame(lastIsGroundedCheckFrame)) IsGroundedCheck();
            return isGrounded;
        }
    }

    public Vector3? LastGroundedPosition { get; private set; }
    public Vector3? PreviousLastGroundedPosition { get; private set; }

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
            PreviousLastGroundedPosition = LastGroundedPosition;
            LastGroundedPosition = refs.playerTransform.position;
        }
    }
    #endregion

    private bool WasNotSetThisFrame(int lastCheckFrame)
    {
        return lastCheckFrame != Time.frameCount;
    }
}
