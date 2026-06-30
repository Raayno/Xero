using UnityEngine;
using UnityEditor;

public class AttackTransform : MonoBehaviour
{
    [Header("Doesn't take Hit Transform into account!!!")]
    [SerializeField] private Mesh debugMesh;
    
    private void OnDrawGizmos()
    {
        if (transform == null) return;

        if (debugMesh == null) 
        {
            GetMesh();
        }

        Gizmos.color = new(0.65f, 0.20f, 0.20f);
        Gizmos.DrawWireMesh(debugMesh, transform.position + Vector3.up, transform.rotation);
    }

    private void GetMesh()
    {
        debugMesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath("0000000000000000e000000000000000"));
    }
}
