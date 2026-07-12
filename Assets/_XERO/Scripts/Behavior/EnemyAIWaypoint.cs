using UnityEngine;

public class EnemyAIWaypoint : MonoBehaviour
{
    public float Radius = 2f;
    [SerializeField] private Mesh debugMesh;
    const float height = 0.5f;

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    public void DrawGizmos()
    {
        // Draw a wire circle to visualize the waypoint radius
        Gizmos.color = Color.skyBlue;

        if (debugMesh == null)
        {
            Debug.LogWarning("Cylinder mesh is null. Creating a new cylinder mesh for visualization. Please assign through Project -> Inspector.");
            debugMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(GameObject.CreatePrimitive(PrimitiveType.Cylinder));
        }
        Gizmos.DrawWireMesh(debugMesh, transform.position + Vector3.up * height / 2, Quaternion.identity, new Vector3(Radius * 2, height, Radius * 2));
    }
}
