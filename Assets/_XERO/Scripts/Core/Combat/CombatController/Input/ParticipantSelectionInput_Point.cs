using UnityEngine;
using Vastav.Utils.Input;
using UnityEngine.InputSystem;
using Alchemy.Inspector;
using UnityEngine.InputSystem.Interactions;

public partial class ParticipantSelectionInput : MoreMountains.Tools.MMSingleton<ParticipantSelectionInput>
{
    
    [FoldoutGroup("Pointing")]
    [SerializeField] private Camera selectionCamera;
    [FoldoutGroup("Pointing")]
    public LayerMask pointableLayerMask;
    [FoldoutGroup("Pointing")]
    [SerializeField] private float maxRayDistance = 500f;
    [FoldoutGroup("Pointing/Point Input Detection")]
    [SerializeField] private float minShowPointTravelDistance = 0.1f;
    [FoldoutGroup("Pointing/Point Input Detection")]
    [ReadOnly, SerializeField] private Vector2? lastPointPosition;

    private int selectionPoolLayer = -1;
    private int[] cashedSelectionPoolLayers;

    private void InitializePoint()
    {
        Cursor.lockState = CursorLockMode.None;
        
        UpdateSelectionPoolMask();

        InputSystem_UIActionsSO.OnPointEvent += OnPointInput;
        InputSystem_UIActionsSO.OnClickEvent += OnClickInput;
        InputSystem_UIActionsSO.OnRightClickEvent += OnRightClickInput;
    }

    private void DisablePoint()
    {
        InputSystem_UIActionsSO.OnPointEvent -= OnPointInput;
        InputSystem_UIActionsSO.OnClickEvent -= OnClickInput;
        InputSystem_UIActionsSO.OnRightClickEvent -= OnRightClickInput;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ClearSelectionPoolMask();
    }

    private void OnPointInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 pointPosition = context.ReadValue<Vector2>();

        if (Cursor.visible == false)
        {
            if (!lastPointPosition.HasValue
                || lastPointPosition.HasValue && Vector2.Distance(lastPointPosition.Value, pointPosition) > minShowPointTravelDistance)
            {
                Cursor.visible = true;
            }
        }
        
        Ray ray = selectionCamera.ScreenPointToRay(pointPosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo, maxRayDistance, pointableLayerMask))
        {
            if (highlightedParticipant != null)
            {
                UnhighlightTarget(highlightedParticipant);
            }
            return;
        }

        if (hitInfo.collider.TryGetComponent<Participant>(out var pointedParticipant))
        {
            // Highlight the pointed participant if it's not already highlighted
            if (highlightedParticipant != pointedParticipant)
            {
                if (highlightedParticipant != null)
                {
                    UnhighlightTarget(highlightedParticipant);
                }

                HighlightTarget(pointedParticipant);
            }
        }
        else
        {
            Debug.LogWarning($"<color=#FFAA55>[ParticipantPointInput]</color> Pointed object {hitInfo.collider.name} does not have a Participant component.");
        }
    }


    private bool UpdateSelectionPoolMask()
    {
        if (selectionPoolLayer < 0)
        {
            selectionPoolLayer = LayerMaskToLayerIndex(pointableLayerMask);
        }

        if (selectionPoolLayer < 0)
        {
            Debug.LogError("[ManualTargetSelector] PointableParticipant must resolve to exactly one Unity layer.");
            return false;
        }

        ClearSelectionPoolMask();

        if (selectionPool == null || selectionPool.Length == 0)
        {
            Debug.LogWarning("[ManualTargetSelector] Selection pool is empty. No targets available for selection.");
            return false;
        }

        cashedSelectionPoolLayers = new int[selectionPool.Length];
        for (int i = 0; i < selectionPool.Length; i++)
        {
            Participant participant = selectionPool[i];

            cashedSelectionPoolLayers[i] = participant.gameObject.layer;
            participant.gameObject.layer = selectionPoolLayer;
        }
        return true;
    }

    private void ClearSelectionPoolMask()
    {
        if (selectionPool == null || selectionPool.Length == 0 || cashedSelectionPoolLayers == null || cashedSelectionPoolLayers.Length != selectionPool.Length)
        {
            return;
        }

        for (int i = 0; i < selectionPool.Length; i++)
        {
            Participant participant = selectionPool[i];
            if (participant != null)
            {
                participant.gameObject.layer = cashedSelectionPoolLayers[i];
            }
        }
    }

    private static int LayerMaskToLayerIndex(LayerMask layerMask)
    {
        int maskValue = layerMask.value;

        if (maskValue == 0 || (maskValue & (maskValue - 1)) != 0)
        {
            Debug.LogError("[ManualTargetSelector] LayerMask must resolve to exactly one Unity layer.");
            return -1;
        }

        int layerIndex = 0;
        while ((maskValue >>= 1) != 0)
        {
            ++layerIndex;
        }

        return layerIndex;
    }


    private void OnClickInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (context.interaction is not PressInteraction press || press.behavior != PressBehavior.ReleaseOnly) return;

        OnSubmitInput(context);
    }

    private void OnRightClickInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (context.interaction is not PressInteraction press || press.behavior != PressBehavior.PressOnly) return;

        OnCancelInput(context);
    }
}