using UnityEngine;

public class AttackTransform : MonoBehaviour
{
    public Vector3 Position => transform.position;
    [Header("Doesn't take Hit Transform into account!!!")]
    [SerializeField] private Mesh debugMesh;
    [SerializeField] private Color color = new(0.65f, 0.20f, 0.20f);
    
    private void OnDrawGizmos()
    {
        if (transform == null || debugMesh == null) return;

        Gizmos.color = color;
        Gizmos.DrawWireMesh(debugMesh, transform.position + Vector3.up, transform.rotation);
    }
}
