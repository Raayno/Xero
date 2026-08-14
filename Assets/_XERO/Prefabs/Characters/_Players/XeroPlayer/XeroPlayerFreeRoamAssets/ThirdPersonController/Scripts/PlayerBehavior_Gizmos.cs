using UnityEngine;

public partial class PlayerBehavior : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] private bool isGroundedGizmos = true;
    [SerializeField] private bool attackGizmos = true;
    [ShowIf("attackGizmos")] [SerializeField] private PlayerBehavior_AttackModule attackModuleForGizmos;
    [ShowIf("attackGizmos")] [SerializeField] private int approximationDensity = 10;

    private void OnDrawGizmosSelected()
    {
        IsGroundedGizmos();
        AttackGizmos();
    }

    private void IsGroundedGizmos()
    {
        if (!isGroundedGizmos) return;

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

    private void AttackGizmos()
    {
        if (!attackGizmos) return;

        if (attackModuleForGizmos == null)
        {
            Debug.LogWarning("[PlayerBehavior] Please assign an AttackModule to 'attackModuleForGizmos' in the inspector to visualize attack range.");
            return;
        }
        DrawFieldOfView(attackModuleForGizmos.ReachAngleAndSourceWidth, Color.red, approximationDensity);
        
        void DrawFieldOfView(Vector3 reachAngleAndSourceWidth,  Color color, int approximationDensity)
        {
            float radius = reachAngleAndSourceWidth.x;
            float angle = reachAngleAndSourceWidth.y;
            float sourceWidth = reachAngleAndSourceWidth.z;

            Gizmos.color = color;

            Vector3 pos = transform.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 leftBoundary = Quaternion.Euler(0, -angle/2, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, angle/2, 0) * forward;

            Gizmos.DrawLine(pos + right * sourceWidth / 2, pos - right * sourceWidth / 2);
            Gizmos.DrawLine(pos - right * sourceWidth / 2, pos + leftBoundary * radius);
            Gizmos.DrawLine(pos + right * sourceWidth / 2, pos + rightBoundary * radius);

            Vector3 previousPoint = pos + leftBoundary * radius;
            for (int i = 0; i < approximationDensity; i++)
            {
                float t = (float)(i + 1) / approximationDensity;
                float currentAngle = -angle / 2 + t * angle;
                Vector3 currentDirection = Quaternion.Euler(0, currentAngle, 0) * forward;
                Vector3 currentPoint = pos + currentDirection * radius;

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}
