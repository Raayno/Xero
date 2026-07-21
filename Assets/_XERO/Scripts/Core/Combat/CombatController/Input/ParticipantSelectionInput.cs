using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Vastav.Utils.Input;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public partial class ParticipantSelectionInput : MoreMountains.Tools.MMSingleton<ParticipantSelectionInput>
{
    private Participant highlightedParticipant;
    private HashSet<Participant> selectedParticipants;

    private Participant[] selectionPool;

    private bool selectionSubmitted;
    private bool selectionCancelled;

    public async UniTask<List<Participant>> AwaitPlayerSelectionAsync(int maxNumberOfParticipants, List<Participant> selectionPool, CancellationToken cancellationToken)
    {
        try
        {
            this.selectionPool = selectionPool.ToArray();
            Initialize();

            await UniTask.WaitUntil(() => selectionCancelled || selectionSubmitted || selectedParticipants.Count >= maxNumberOfParticipants || selectedParticipants.Count == selectionPool.Count, cancellationToken: cancellationToken);

            if (selectionCancelled)
            {
                Debug.Log("<color=#FFAA55>[ParticipantSelectionInput]</color> Player selection was cancelled.");
                return null;
            }

            int limit = Math.Min(selectionPool.Count, maxNumberOfParticipants);
            Debug.Log($"<color=#55FF88>[ParticipantSelectionInput]</color> Player selection completed. Returning {Math.Min(limit, selectedParticipants.Count)} selected participants.");
            return selectedParticipants.ToList().GetRange(0, Math.Min(limit, selectedParticipants.Count));
        }
        finally
        {
            StopSelection();
        }
    }

    private void Initialize()
    {
        selectionSubmitted = false;
        selectionCancelled = false;
        selectedParticipants = new();

        InputManager.Instance.EnableUIActions();
        InputSystem_UIActionsSO.OnSubmitEvent += OnSubmitInput;
        InputSystem_UIActionsSO.OnCancelEvent += OnCancelInput;

        InitializePoint();
    }

    protected override void Awake()
    {
        base.Awake();
        if (selectionCamera == null)
        {
            selectionCamera = Camera.main;
        }
    }

    private void HighlightTarget(Participant target)
    {
        if (target == null)
        {
            Debug.LogWarning("[ParticipantPointInput] Cannot highlight a null target.");
            return;
        }

        highlightedParticipant = target;
        //target.OnSelectionHighlight();

        Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Highlighting target: {target.CombatantName}");
    }

    private void UnhighlightTarget(Participant target)
    {
        if (target == null)
        {
            Debug.LogWarning("[ParticipantPointInput] Cannot unhighlight a null target.");
            return;
        }

        if (selectedParticipants.Contains(target))
        {
            Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Target {target.CombatantName} is selected. Not unhighlighting.");
            return;
        }

        if (highlightedParticipant == target)
        {
            highlightedParticipant = null;
            //target.OnSelectionUnhighlight();
        }

        Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Unhighlighting target: {target.CombatantName}");
    }
    
    /// <summary>
    /// If a participant is highlighted, add it to the selected participants list, if already selected, finish the selection process and return the selected participants.
    /// </summary>
    public void OnSubmitInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (highlightedParticipant == null)
        {
            Debug.LogWarning("<color=#FFAA55>[ParticipantPointInput]</color> No participant is currently highlighted. Cannot submit selection.");
            return;
        }

        // Submit if already selected
        if (selectedParticipants.Contains(highlightedParticipant))
        {
            Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Participant {highlightedParticipant.CombatantName} is already selected. Finalizing selection.");
            selectionSubmitted = true;
            return;
        }

        // Otherwise, add to selected participants
        Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Adding {highlightedParticipant.CombatantName} to selected participants.");
        selectedParticipants.Add(highlightedParticipant);
    }
    
    /// <summary>
    /// Unselect the currently highlighted participant or if none highlighted, cancel the selection process.
    /// </summary>
    public void OnCancelInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        StopSelection();

        Debug.Log("<color=#FFAA55>[ParticipantPointInput]</color> Selection cancelled.");

        if (highlightedParticipant != null && selectedParticipants.Contains(highlightedParticipant))
        {
            selectedParticipants.Remove(highlightedParticipant);
            UnhighlightTarget(highlightedParticipant);

            Debug.Log($"<color=#55FF88>[ParticipantPointInput]</color> Participant {highlightedParticipant.CombatantName} unselected.");
        }

        Debug.Log("<color=#FFAA55>[ParticipantPointInput]</color> Cancelling selection process.");
        selectionCancelled = true;
    }

    public void StopSelection()
    {
        InputSystem_UIActionsSO.OnSubmitEvent -= OnSubmitInput;
        InputSystem_UIActionsSO.OnCancelEvent -= OnCancelInput;
        DisablePoint();

    }

    private void OnDisable()
    {
        StopSelection();
    }

    private void Reset()
    {
        selectionCamera = selectionCamera != null ? selectionCamera : Camera.main;

        pointableLayerMask = pointableLayerMask != 0 ? pointableLayerMask : LayerMask.GetMask("Pointable Participant Layer");
    }
}
