using UnityEngine;
using System;

public class ParticipantPointInput : MonoBehaviour
{
    public Participant PointedParticipant { get; private set; }

    [Header("Selection")]
    [SerializeField] protected Camera selectionCamera;
    public LayerMask pointableLayerMask;
    [SerializeField] protected float maxRayDistance = 500f;

    private bool isDetectInput = false;

    public event Action<Participant> OnParticipantSelected;
    public event Action OnSelectionCancelled;

    private void Reset()
    {
        selectionCamera = selectionCamera != null ? selectionCamera : Camera.main;

        pointableLayerMask = pointableLayerMask != 0 ? pointableLayerMask : LayerMask.GetMask("Pointable Participant Layer");
    }

    private void Awake()
    {
        if (selectionCamera == null)
        {
            selectionCamera = Camera.main;
        }
    }

    private Coroutine selectionCoroutine = null;
    private void Update()
    {
        if (!isDetectInput)
        {
            if (selectionCoroutine != null)
            {
                StopCoroutine(selectionCoroutine);
                selectionCoroutine = null;
            }
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectTargetFromMouse();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelSelection();
        }
    }
    
    /// <summary>
    /// Except for stopping the selection, invoke the cancellation event to notify any listeners that the selection was cancelled
    /// </summary>
    public void CancelSelection()
    {
        if (!isDetectInput)
        {
            return;
        }

        StopSelection();

        Debug.Log("<color=#FFAA55>[ParticipantPointInput]</color> Selection cancelled.");

        OnSelectionCancelled?.Invoke();
    }

    private void TrySelectTargetFromMouse()
    {
        if (selectionCamera == null)
        {
            Debug.LogError("[ParticipantPointInput] No selection camera assigned.");
            return;
        }

        Ray ray = selectionCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, pointableLayerMask))
        {
            return;
        }

        Participant selectedTarget = hit.collider.GetComponentInParent<Participant>();

        if (selectedTarget == null)
        {
            return;
        }

        SelectTarget(selectedTarget);
    }

    private void SelectTarget(Participant selectedTarget)
    {
        StopSelection();

        Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Selected target: {selectedTarget.CombatantName}");

        PointedParticipant = selectedTarget;
        OnParticipantSelected?.Invoke(selectedTarget);
    }

    public void StartSelection()
    {
        isDetectInput = true;
    }

    private void StopSelection()
    {
        isDetectInput = false;
    }
}
