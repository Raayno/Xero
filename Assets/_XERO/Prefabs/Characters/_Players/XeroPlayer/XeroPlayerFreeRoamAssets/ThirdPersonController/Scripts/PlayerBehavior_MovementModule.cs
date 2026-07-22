using UnityEngine;
using Vastav.Utils.Input;
using UnityEngine.InputSystem;

public class PlayerBehavior_MovementModule : PlayerBehavior_Module
{
    [SerializeField] private PlayerBehavior_FallingModule fallingModule;

    [Tooltip("Move speed of the character in m/s")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    [SerializeField] private float sprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    [SerializeField] private float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float speedChangeRate = 10.0f;
    
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    [SerializeField] private float fallTimeout = 0.15f;

    // [Group("Input")]
    [SerializeField] private bool sprintIsToggle = true;

    private bool isSprint = false;
    private Vector2 moveInput = Vector2.zero;
    private float speed;        
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float fallTimeoutDelta = 0.1f;

#region Enable
    protected override void EnableModule()
    {
        SubscribeToInputEvents();
    }

#endregion
#region Update
    protected override void UpdateModule()
    {
        if (ShouldStartFalling()) return;

        Gravity();
        Move();
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
        }
        fallTimeoutDelta = fallTimeout;
        return false;
    }

    private void Gravity()
    {
        if (refs.characterController.velocity.y > 0.0f)
        {
            refs.characterController.Move(new Vector3(
                refs.characterController.velocity.x,
                -2f,
                refs.characterController.velocity.z
            ));
        }
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
        finalMovement.y = refs.characterController.velocity.y * Time.deltaTime;

        refs.characterController.Move(finalMovement);

        refs.animationManager.SetMovementBlend(animationBlend, inputMagnitude);
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
            // Hold sprint
            if (context.started)
            {
                isSprint = true;
            }
            else if (context.canceled)
            {
                isSprint = false;
            }
        }
    }

    private void OnMoveInput(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

#endregion
}
