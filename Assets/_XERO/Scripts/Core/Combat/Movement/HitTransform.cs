using UnityEngine;

public class HitTransform : MonoBehaviour
{
    private Vector3 position;
    public Vector3 Position { 
        get 
        {
            if (position == default || position == null) position = transform.position;
            return position;
        }
    }

    [SerializeField] private Mesh debugMesh;
    [SerializeField] private Color color = new(0.1f, 0.3f, 0.8f);
    
    private void OnDrawGizmos()
    {
        if (transform == null || debugMesh == null) return;

        Gizmos.color = color;
        Gizmos.DrawWireMesh(debugMesh, transform.position + Vector3.up, transform.rotation);
    }
}
