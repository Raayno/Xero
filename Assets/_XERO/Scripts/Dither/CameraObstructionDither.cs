using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionDither : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;

    [Header("Detection")]
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float sphereRadius = 0.35f;

    [Header("Shader Values")]
    [SerializeField] private float obstructedValue = 0f;
    [SerializeField] private float unobstructedValue = 1f;

    private readonly HashSet<DitherObject> currentObjects = new();
    private readonly HashSet<DitherObject> previousObjects = new();

    private void LateUpdate()
    {
        if (targetCamera == null || player == null)
            return;

        previousObjects.Clear();

        foreach (var obj in currentObjects)
            previousObjects.Add(obj);

        currentObjects.Clear();

        Vector3 origin = targetCamera.transform.position;
        Vector3 direction = player.position - origin;
        float distance = direction.magnitude;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            sphereRadius,
            direction.normalized,
            distance,
            obstructionMask,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            DitherObject dither = hit.collider.GetComponentInParent<DitherObject>();

            if (dither == null)
                continue;

            if (currentObjects.Add(dither))
            {
                dither.SetValue(obstructedValue);
            }

            previousObjects.Remove(dither);
        }

        foreach (DitherObject obj in previousObjects)
        {
            obj.SetValue(unobstructedValue);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (targetCamera == null || player == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 origin = targetCamera.transform.position;
        Vector3 direction = (player.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.position);

        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.DrawWireSphere(origin + direction * distance, sphereRadius);
        Gizmos.DrawLine(origin, player.position);
    }
#endif
}