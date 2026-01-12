using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ================= MOVEMENT =================
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 14f;

    // ================= CAMERA =================
    [Header("Camera")]
    public CamraFollow cameraFollow;

    // ================= COMBAT =================
    [Header("Combat")]
    public EnemyParryController currentEnemy; // assigned by enemy

    [Tooltip("ALLOW movement even in combat (for testing)")]
    public bool allowMovementInCombat = false;

    bool inCombat;
    Vector3 combatLockPosition;

    // ================= TURN =================
    [Header("Turn Slowdown")]
    [Range(0.2f, 1f)]
    public float turnSpeedMultiplier = 0.6f;
    public float maxTurnAngle = 120f;

    [Header("Facing Lock")]
    public float facingThreshold = 20f;

    [Header("Rotation")]
    public float lookSpeed = 10f;

    // ================= INTERNAL =================
    Vector2 move;
    Vector3 currentVelocity;
    float currentTurnAmount;

    // ================= INPUT =================
    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    // ================= UNITY =================
    void Update()
    {
        HandleCombatState();
        HandleMovementFacing();
        HandleMovementTranslation();
    }

    // ================= COMBAT STATE =================
    void HandleCombatState()
    {
        // ENTER COMBAT
        if (!inCombat && currentEnemy != null)
        {
            inCombat = true;
            combatLockPosition = transform.position;
        }

        // EXIT COMBAT
        if (inCombat && currentEnemy == null)
        {
            inCombat = false;
        }
    }

    // ================= FACING =================
    void HandleMovementFacing()
    {
        // Still allow rotation even when movement locked
        Vector3 desiredFacing = Vector3.zero;

        if (Mathf.Abs(move.x) > 0.01f || Mathf.Abs(move.y) > 0.01f)
        {
            desiredFacing = new Vector3(move.x, 0f, move.y);
            if (desiredFacing.sqrMagnitude > 1f)
                desiredFacing.Normalize();
        }

        if (desiredFacing == Vector3.zero)
        {
            currentTurnAmount = 0f;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(desiredFacing);
        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        currentTurnAmount = Mathf.Clamp01(angle / maxTurnAngle);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            lookSpeed * Time.deltaTime
        );
    }

    // ================= MOVEMENT =================
    void HandleMovementTranslation()
    {
        // 🔒 LOCK MOVEMENT DURING COMBAT
        if (inCombat && !allowMovementInCombat)
        {
            transform.position = combatLockPosition;
            currentVelocity = Vector3.zero;
            return;
        }

        Vector3 desiredFacing = Vector3.zero;

        if (Mathf.Abs(move.x) > 0.01f || Mathf.Abs(move.y) > 0.01f)
            desiredFacing = new Vector3(move.x, 0f, move.y).normalized;

        if (desiredFacing == Vector3.zero)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime
            );
            return;
        }

        float angle = Vector3.Angle(transform.forward, desiredFacing);

        float speedMultiplier =
            angle <= facingThreshold ? 1f : turnSpeedMultiplier;

        float turnSlowdown =
            Mathf.Lerp(1f, turnSpeedMultiplier, currentTurnAmount);

        Vector3 desiredVelocity =
            transform.forward * maxSpeed * speedMultiplier * turnSlowdown;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            desiredVelocity,
            acceleration * Time.deltaTime
        );

        transform.Translate(currentVelocity * Time.deltaTime, Space.World);
    }
}
