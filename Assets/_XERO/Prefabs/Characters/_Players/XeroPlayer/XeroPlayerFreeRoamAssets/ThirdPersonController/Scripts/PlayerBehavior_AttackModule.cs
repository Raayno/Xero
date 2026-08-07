using UnityEngine;
using Vastav.Utils.Input;
using System.Linq;

public class PlayerBehavior_AttackModule : PlayerBehavior_Module
{
    [SerializeField] private float reach = 3f;
    [SerializeField, Range(0f, 360f)] private float angleDeg = 60f;
    [SerializeField] private float sourceWidth = 0f;

    public Vector3 ReachAngleAndSourceWidth => new(reach, angleDeg, sourceWidth);

    protected override void EnableModule()
    {
        InputSystem_PlayerActionsSO.OnAttackEvent += HandleAttackInput;
    }

    protected override void DisableModule()
    {
        InputSystem_PlayerActionsSO.OnAttackEvent -= HandleAttackInput;
    }

    private void HandleAttackInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("[PlayerBehavior_AttackModule] Attack input received.");

        Attack();
    }

    private readonly Collider[] hits = new Collider[20]; // Preallocate an array to avoid garbage collection
    private void Attack()
    {
        // SphereCast to detect IFreeRoamAttackable objects within the attack range
        Vector3 pos = refs.playerTransform.position;
        Vector3 forward = refs.playerTransform.forward;

        Physics.OverlapSphereNonAlloc(pos, reach, hits);

        var sortedAttackables = hits
            .Where(hit => hit != null
                && hit.TryGetComponent<IFreeRoamAttackable>(out _) 
                && IsWithinTheAngle(hit.transform.position)
                && IsInFrontOfPlayer(hit.transform.position))
            .OrderByDescending(hit => PlayerIntentionValue(hit.transform.position)) // Sort by player intention value from highest to lowest
            .Select(hit => hit.GetComponent<IFreeRoamAttackable>())
            .ToArray();

        foreach (var attackable in sortedAttackables)
        {
            bool shouldBlockOthers = attackable.OnAttack();
            if (shouldBlockOthers)
            {
                Debug.Log("[PlayerBehavior_AttackModule] Attack hit: " + attackable.GetType().Name + ". Blocking other targets.");
                break; // Stop processing further targets if this one blocks others
            }
            else
            {
                Debug.Log("[PlayerBehavior_AttackModule] Attack hit: " + attackable.GetType().Name + ". Not blocking other targets.");
            }
        }
    }

    /// <summary>
    /// Calculates the position and half-angle of the attack slice based on the player's position, forward direction, reach, angle, and source width.
    /// </summary>
    private bool IsWithinTheAngle(Vector3 hitPoint)
    {
        Vector3 pos = refs.playerTransform.position;
        Vector3 forward = refs.playerTransform.forward;

        float backwardOffset = reach * sourceWidth/2 * Mathf.Cos(Mathf.Deg2Rad * angleDeg/2) / (reach * Mathf.Sin(Mathf.Deg2Rad * angleDeg/2) - sourceWidth/2);
        Vector3 backwardOffsetPoint = pos - backwardOffset * forward;

        float halfAngleDeg = Mathf.Atan(backwardOffset != 0 ? (sourceWidth/2 / backwardOffset) : float.MaxValue) * Mathf.Rad2Deg;

        return Vector3.Angle(forward, hitPoint - backwardOffsetPoint) <= halfAngleDeg;
    }

    private bool IsInFrontOfPlayer(Vector3 hitPoint)
    {
        Vector3 pos = refs.playerTransform.position;
        pos.y = 0; // Ignore vertical difference for front/back check
        Vector3 forward = refs.playerTransform.forward;

        return Vector3.Dot(forward, hitPoint - pos) > 0;
    }

    /// <summary>
    /// Uses closeness to the source and the angle to the center of the attack slice to determine how much the player intended to hit this target.
    /// Sometimes 1 is unachievablem because of the cone being cut (sourceWidth > 0), but the closer to 1, the more the player intended to hit this target.
    /// </summary>
    /// <returns>Value between 0 and 1</returns>
    private float PlayerIntentionValue(Vector3 hitPoint)
    {
        Vector3 pos = refs.playerTransform.position;
        Vector3 forward = refs.playerTransform.forward;

        float backwardOffset = reach * sourceWidth/2 * Mathf.Cos(Mathf.Deg2Rad * angleDeg/2) / (reach * Mathf.Sin(Mathf.Deg2Rad * angleDeg/2) - sourceWidth/2);
        Vector3 backwardOffsetPoint = pos - backwardOffset * forward;

        float halfAngleDeg = Mathf.Atan(backwardOffset != 0 ? (sourceWidth/2 / backwardOffset) : float.MaxValue) * Mathf.Rad2Deg;

        return 1 - (Vector3.Distance(backwardOffsetPoint, hitPoint) / reach + Vector3.Angle(forward, hitPoint - backwardOffsetPoint) / halfAngleDeg) / 2;
    }
}
