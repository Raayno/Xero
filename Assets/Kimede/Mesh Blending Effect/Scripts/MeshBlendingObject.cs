using UnityEngine;

namespace Kimede
{
    /// <summary>
    /// Attach this component to objects to customize their Mesh Blending Radius.
    /// The custom radius is encoded in the ObjectID texture's alpha channel.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class MeshBlendingObject : MonoBehaviour
    {
        [Header("Blending Radius")]
        [Range(0.01f, 2f)]
        [Tooltip("Custom blending radius for this object (0.01 - 2.0)")]
        public float blendingRadius = 0.1f;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo = false;

        /// <summary>
        /// Get the actual blending radius value (for external access)
        /// </summary>
        public float GetBlendingRadius()
        {
            return blendingRadius;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Visualize blending radius in Scene view
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);

            // Draw approximate blend radius sphere
            Vector3 position = transform.position;
            float visualRadius = blendingRadius * 10f; // Approximate visualization

            Gizmos.DrawWireSphere(position, visualRadius);
        }
#endif
    }
}
