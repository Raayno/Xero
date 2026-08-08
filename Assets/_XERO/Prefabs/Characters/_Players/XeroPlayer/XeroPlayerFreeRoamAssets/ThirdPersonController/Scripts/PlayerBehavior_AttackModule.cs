using UnityEngine;
using Vastav.Utils.Input;
using System.Linq;

public class PlayerBehavior_AttackModule : PlayerBehavior_Module
{
    [SerializeField] private float reach = 3f;
    [SerializeField, Range(0f, 360f)] private float angleDeg = 60f;
    [SerializeField] private float sourceWidth = 0f;

    public Vector3 ReachAngleAndSourceWidth
    {
        get
        {
            ValidateReachAngleAndSourceWidth();
            return new(reach, angleDeg, sourceWidth);
        }
    }

    protected override void EnableModule()
    {
        ValidateReachAngleAndSourceWidth();
        InputSystem_PlayerActionsSO.OnAttackEvent += HandleAttackInput;
    }

    protected override void DisableModule()
    {
        InputSystem_PlayerActionsSO.OnAttackEvent -= HandleAttackInput;
    }

    private void ValidateReachAngleAndSourceWidth()
    {
        // Ensure reach is positive
        reach = Mathf.Max(0f, reach);

        // Ensure angle is within [0, 360]
        angleDeg = Mathf.Clamp(angleDeg, 0f, 180f);

        // Ensure sourceWidth is non-negative and does not exceed reach
        sourceWidth = Mathf.Clamp(sourceWidth, 0f, 2 * reach * Mathf.Sin(Mathf.Deg2Rad * angleDeg / 2) - 0.01f);
    }

    private void HandleAttackInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("[PlayerBehavior_AttackModule] Attack input received.");

        Attack();
    }

    private readonly Collider[] hits = new Collider[100]; // Preallocate an array to avoid garbage collection
    private void Attack()
    {
        // SphereCast to detect IFreeRoamAttackable objects within the attack range
        Vector3 pos = refs.playerTransform.position;
        pos.y = 0; // Ignore vertical difference for attack detection
        Vector3 forward = refs.playerTransform.forward;

        Vector3 backwardOffsetPoint = pos - CalculateBackwardOffset(out float halfAngleFromOriginDeg) * forward;

        int hitCount = Physics.OverlapSphereNonAlloc(pos, reach, hits);

        var sortedAttackables = hits
            .Take(hitCount) // Only consider the valid hits returned by OverlapSphereNonAlloc
            .Where(hit => hit != null)
            .Select(hit => {
                Vector3 toTarget = hit.bounds.center - pos;

                float projectionDistance = Mathf.Clamp(Vector3.Dot(toTarget, forward), 0f, reach);
                Vector3 closestPointOnAttackLine = pos + forward * projectionDistance;

                Vector3 closestToSymmetryAxis = hit.ClosestPoint(closestPointOnAttackLine);
                closestToSymmetryAxis.y = 0; // Ignore vertical difference for angle calculations
                Vector3 closestToPlayer = hit.ClosestPoint(pos);
                closestToPlayer.y = 0; // Ignore vertical difference for angle calculations
                Vector3 center = hit.bounds.center;
                center.y = 0; // Ignore vertical difference for angle calculations
                var ret = new { 
                    ClosestToSymmetryAxis = closestToSymmetryAxis,
                    ClosestToPlayer = closestToPlayer,
                    Center = center,
                    Attackable = hit.GetComponent<IFreeRoamAttackable>()
                };
                return ret;
            })
            .Where(hit => hit.Attackable != null
                && (IsWithinTheAngle(hit.ClosestToSymmetryAxis) && IsInFrontOfPlayer(hit.ClosestToSymmetryAxis)
                || (IsWithinTheAngle(hit.ClosestToPlayer) && IsInFrontOfPlayer(hit.ClosestToPlayer))
                || (IsWithinTheAngle(hit.Center) && IsInFrontOfPlayer(hit.Center))))
            .OrderByDescending(hit => PlayerIntentionValue(hit.Center)) // Sort by player intention value from highest to lowest
            .Select(hit => hit.Attackable)
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
                if (enableDebug && attackable is MonoBehaviour mb)
                {
                    Debug.Log("[PlayerBehavior_AttackModule] That is " + mb.name + " at " + (mb.transform.parent != null ? mb.transform.parent.name : "root") + " with a player intention value of " + PlayerIntentionValue(mb.GetComponent<Collider>().ClosestPoint(pos)));
                }
            }
        }

        /// <summary>
        /// Calculates the position and half-angle of the attack slice based on the player's position, forward direction, reach, angle, and source width.
        /// </summary>
        bool IsWithinTheAngle(Vector3 hitPoint) => Vector3.Angle(forward, hitPoint - backwardOffsetPoint) <= halfAngleFromOriginDeg;

        bool IsInFrontOfPlayer(Vector3 hitPoint) => Vector3.Dot(forward, hitPoint - pos) > 0;

        /// <summary>
        /// Uses closeness to the source and the angle to the center of the attack slice to determine how much the player intended to hit this target.
        /// Sometimes 1 is unachievablem because of the cone being cut (sourceWidth > 0), but the closer to 1, the more the player intended to hit this target.
        /// </summary>
        /// <returns>Value between 0 and 1</returns>
        float PlayerIntentionValue(Vector3 hitPoint) => 1 - 0.5f * (Vector3.Distance(backwardOffsetPoint, hitPoint) / reach  +  Vector3.Angle(forward, hitPoint - backwardOffsetPoint) / halfAngleFromOriginDeg);
    }

    private float CalculateBackwardOffset(out float halfAngleFromOriginDeg)
    {
        if (sourceWidth <= 0.001f)
        {
            halfAngleFromOriginDeg = angleDeg / 2;
            return 0f;
        }

        float halfAngleFromSourceRad = Mathf.Deg2Rad * angleDeg / 2;
        
        float denominator = (reach * Mathf.Sin(halfAngleFromSourceRad)) - (sourceWidth / 2);
        if (denominator <= 0.001f) 
        {
            halfAngleFromOriginDeg = angleDeg / 2;
            return 0f;
        }

        float backwardOffset = reach * (sourceWidth / 2) * Mathf.Cos(halfAngleFromSourceRad) / denominator;
        if (backwardOffset <= 0.001f)
        {
            halfAngleFromOriginDeg = angleDeg / 2;
            return 0f;
        }
        
        halfAngleFromOriginDeg = Mathf.Atan(sourceWidth/2 / backwardOffset) * Mathf.Rad2Deg;
        return backwardOffset;
    }
}
