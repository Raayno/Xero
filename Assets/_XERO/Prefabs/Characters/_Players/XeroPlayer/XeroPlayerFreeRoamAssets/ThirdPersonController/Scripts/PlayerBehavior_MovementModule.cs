using UnityEngine;
using Vastav.Utils.Input;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using Gaskellgames;
using System.Threading;

public class PlayerBehavior_MovementModule : PlayerBehavior_Module
{
    [SerializeField] private PlayerBehavior_FallingModule fallingModule;

    [Tooltip("Move speed of the character in m/s")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    [SerializeField] private float sprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [SerializeField] private float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float speedChangeRate = 10.0f;
    
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    [SerializeField] private float fallTimeout = 0.15f;

    [SerializeField] private float downforce = -2f;

    // [Group("Input")]
    [SerializeField] private bool sprintIsToggle = true;
    [SerializeField] private float unresponsiveMovementDectionDelay = 0.5f;

    private bool isSprint = false;
    private Vector2 moveInput = Vector2.zero;
    private float speed;        
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float fallTimeoutDelta = 0.1f;
    private CancellationTokenSource cancellationTokenSource;

#region Enable
    protected override void EnableModule()
    {
        SubscribeToInputEvents();
        
        // Reset variables
        moveInput = refs.playerInput.actions["Move"].ReadValue<Vector2>();
        fallTimeoutDelta = fallTimeout;

        cancellationTokenSource = new CancellationTokenSource();
    }

#endregion
#region Update
    protected override void UpdateModule()
    {
        if (ShouldStartFalling()) return;

        Move();
        DetectUnresponsiveInput();
    }

    private bool ShouldStartFalling()
    {
        if (!refs.playerBehavior.IsGrounded)
        {
            fallTimeoutDelta -= Time.deltaTime;
            if (fallTimeoutDelta <= 0.0f)
            {
                if (enableDebug) Debug.Log($"<color=cyan>[PlayerBehavior_MovementModule]</color> Transitioning to FallingModule due to fall timeout.");

                TransitionToModule(fallingModule);
                return true;
            }
            return false;
        }
        fallTimeoutDelta = fallTimeout;
        return false;
    }

    private void Move()
    {
        float targetSpeed = isSprint ? sprintSpeed : moveSpeed;

        if (moveInput == Vector2.zero)
            targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(
            refs.characterController.velocity.x,
            0.0f,
            refs.characterController.velocity.z
        ).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            speed = Mathf.Lerp(
                currentHorizontalSpeed,
                targetSpeed * inputMagnitude,
                Time.deltaTime * speedChangeRate
            );

            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        CalculateInputDirection(out Vector3 worldMoveDirection);

        if (moveInput != Vector2.zero)
        {
            float rotation = Mathf.SmoothDampAngle(
                refs.playerTransform.eulerAngles.y,
                targetRotation,
                ref rotationVelocity,
                rotationSmoothTime
            );
            refs.playerTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 finalMovement = worldMoveDirection * (speed * Time.deltaTime);
        finalMovement.y = GetVerticalDisplacement();

        refs.characterController.Move(finalMovement);

        refs.animationManager.SetMovementBlend(animationBlend, inputMagnitude);
    }

    private float GetVerticalDisplacement()
    {
        float baseVerticalVelocity = downforce;
    
        return baseVerticalVelocity * Time.deltaTime;
    }

    private void CalculateInputDirection(out Vector3 worldMoveDirection)
    {
        if (moveInput == Vector2.zero)
        {
            worldMoveDirection = Vector3.zero;
            return;
        }

        Vector3 cameraForward = refs.mainCamera.transform.forward;
        Vector3 cameraRight = refs.mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        worldMoveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        targetRotation = Mathf.Atan2(worldMoveDirection.x, worldMoveDirection.z) * Mathf.Rad2Deg;
    }
#endregion
#region Disable
    protected override void DisableModule()
    {
        SubscribeToInputEvents(false);
        cancellationTokenSource?.CancelAndDispose();
        cancellationTokenSource = null;
    }
#endregion
#region Input
    private void SubscribeToInputEvents(bool subscribe = true)
    {
        if (subscribe)
        {
            InputSystem_PlayerActionsSO.OnSprintEvent += OnSprintInput;
            InputSystem_PlayerActionsSO.OnMoveEvent += OnMoveInput;
        }
        else
        {
            InputSystem_PlayerActionsSO.OnSprintEvent -= OnSprintInput;
            InputSystem_PlayerActionsSO.OnMoveEvent -= OnMoveInput;
        }
    }

    private void OnSprintInput(InputAction.CallbackContext context)
    {
        if (sprintIsToggle)
        {
            if (!context.performed) return;

            // Toggle sprint
            isSprint = !isSprint;
        }
        else
        {
            // Hold walk (sprint as default)
            if (context.started)
            {
                isSprint = false;
            }
            else if (context.canceled)
            {
                isSprint = true;
            }
        }
    }

    private void OnMoveInput(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    
    private Vector3? positionBeforeUnresponsiveInput = null;
    private void DetectUnresponsiveInput(bool isAfterDelay = false)
    {
        if (moveInput != Vector2.zero && refs.characterController.velocity.magnitude < 0.1f)
        {
            if (isAfterDelay)
            {
                Debug.LogWarning($"<color=cyan>[PlayerBehavior_MovementModule]</color> Detected unresponsive input. Nudge applied to break out of freeze.");
                // Nudge the player more strongly to break out of the freeze. This is a last resort if the player is stuck and not moving.
                Vector3 nudgeDirection = refs.playerTransform.forward;
                refs.characterController.Move(nudgeDirection * 0.1f);
            }
            else
            {
                positionBeforeUnresponsiveInput ??= refs.playerBehavior.PreviousLastGroundedPosition;;
                DetectUnresponsiveInputAfterDelay(unresponsiveMovementDectionDelay).Forget();
            }
        }
    }

    private async UniTask DetectUnresponsiveInputAfterDelay(float delay)
    {
        await UniTask.Delay((int)(delay * 1000), cancellationToken: cancellationTokenSource.Token);
        DetectUnresponsiveInput(true);
    }
#endregion
}
