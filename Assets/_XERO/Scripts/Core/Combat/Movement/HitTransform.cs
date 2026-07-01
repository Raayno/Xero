using UnityEngine;
using UnityEditor;

public class HitTransform : MonoBehaviour
{
    [SerializeField] private Mesh debugMesh;
    [SerializeField] private Color color = new(0.1f, 0.3f, 0.8f);
    
    private void OnDrawGizmos()
    {
        if (transform == null || debugMesh == null) return;

        Gizmos.color = color;
        Gizmos.DrawWireMesh(debugMesh, transform.position + Vector3.up, transform.rotation);
    }
}
