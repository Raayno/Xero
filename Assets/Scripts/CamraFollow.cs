using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamraFollow : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public Vector3 offset;
    public float smoothTime = 0.25f;

    Vector3 velocity;

    [Header("Zoom (Orthographic Camera)")]
    public float normalZoom = 5f;     // Default camera size
    public float zoomInSize = 3.8f;    // Zoom during enemy wind-up
    public float zoomSpeed = 6f;       // How fast zoom blends

    float targetZoom;
    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetZoom = normalZoom;
    }

    void Update()
    {
        if (target == null)
            return;

        // Smooth follow
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // Smooth zoom
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomSpeed
        );
    }

    // 🔍 Called when enemy wind-up starts
    public void ZoomIn()
    {
        targetZoom = zoomInSize;
    }

    // 🔍 Called when parry window ends
    public void ZoomOut()
    {
        targetZoom = normalZoom;
    }
}
