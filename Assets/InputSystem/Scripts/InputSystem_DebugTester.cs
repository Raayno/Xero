using UnityEngine;
using UnityEngine.InputSystem;

namespace Vastav.Utils.Input.Test
{
    public class InputSystem_DebugTester : MonoBehaviour
    {
        private const string PlayerPrefix = "<color=#4DA6FF>[Player]</color>";
        private const string UIPrefix = "<color=#7CFF7C>[UI]</color>";

        private void OnEnable()
        {
            // Player Actions
            InputSystem_PlayerActionsSO.OnAttackEvent += HandlePlayerAttack;
            InputSystem_PlayerActionsSO.OnCrouchEvent += HandlePlayerCrouch;
            InputSystem_PlayerActionsSO.OnInteractEvent += HandlePlayerInteract;
            InputSystem_PlayerActionsSO.OnJumpEvent += HandlePlayerJump;
            InputSystem_PlayerActionsSO.OnLookEvent += HandlePlayerLook;
            InputSystem_PlayerActionsSO.OnMoveEvent += HandlePlayerMove;
            InputSystem_PlayerActionsSO.OnNextEvent += HandlePlayerNext;
            InputSystem_PlayerActionsSO.OnPreviousEvent += HandlePlayerPrevious;
            InputSystem_PlayerActionsSO.OnSprintEvent += HandlePlayerSprint;

            // UI Actions
            InputSystem_UIActionsSO.OnCancelEvent += HandleUICancel;
            InputSystem_UIActionsSO.OnClickEvent += HandleUIClick;
            InputSystem_UIActionsSO.OnMiddleClickEvent += HandleUIMiddleClick;
            InputSystem_UIActionsSO.OnNavigateEvent += HandleUINavigate;
            InputSystem_UIActionsSO.OnPointEvent += HandleUIPoint;
            InputSystem_UIActionsSO.OnRightClickEvent += HandleUIRightClick;
            InputSystem_UIActionsSO.OnScrollWheelEvent += HandleUIScrollWheel;
            InputSystem_UIActionsSO.OnSubmitEvent += HandleUISubmit;
            InputSystem_UIActionsSO.OnTrackedDeviceOrientationEvent += HandleUITrackedDeviceOrientation;
            InputSystem_UIActionsSO.OnTrackedDevicePositionEvent += HandleUITrackedDevicePosition;
        }

        private void OnDisable()
        {
            // Player Actions
            InputSystem_PlayerActionsSO.OnAttackEvent -= HandlePlayerAttack;
            InputSystem_PlayerActionsSO.OnCrouchEvent -= HandlePlayerCrouch;
            InputSystem_PlayerActionsSO.OnInteractEvent -= HandlePlayerInteract;
            InputSystem_PlayerActionsSO.OnJumpEvent -= HandlePlayerJump;
            InputSystem_PlayerActionsSO.OnLookEvent -= HandlePlayerLook;
            InputSystem_PlayerActionsSO.OnMoveEvent -= HandlePlayerMove;
            InputSystem_PlayerActionsSO.OnNextEvent -= HandlePlayerNext;
            InputSystem_PlayerActionsSO.OnPreviousEvent -= HandlePlayerPrevious;
            InputSystem_PlayerActionsSO.OnSprintEvent -= HandlePlayerSprint;

            // UI Actions
            InputSystem_UIActionsSO.OnCancelEvent -= HandleUICancel;
            InputSystem_UIActionsSO.OnClickEvent -= HandleUIClick;
            InputSystem_UIActionsSO.OnMiddleClickEvent -= HandleUIMiddleClick;
            InputSystem_UIActionsSO.OnNavigateEvent -= HandleUINavigate;
            InputSystem_UIActionsSO.OnPointEvent -= HandleUIPoint;
            InputSystem_UIActionsSO.OnRightClickEvent -= HandleUIRightClick;
            InputSystem_UIActionsSO.OnScrollWheelEvent -= HandleUIScrollWheel;
            InputSystem_UIActionsSO.OnSubmitEvent -= HandleUISubmit;
            InputSystem_UIActionsSO.OnTrackedDeviceOrientationEvent -= HandleUITrackedDeviceOrientation;
            InputSystem_UIActionsSO.OnTrackedDevicePositionEvent -= HandleUITrackedDevicePosition;
        }

        // Player Handlers

        private void HandlePlayerAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Attack");
        }

        private void HandlePlayerCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Crouch");

            if (context.canceled)
                Debug.Log($"{PlayerPrefix} Crouch Cancelled");
        }

        private void HandlePlayerInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Interact");
        }

        private void HandlePlayerJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Jump");
        }

        private void HandlePlayerLook(InputAction.CallbackContext context)
        {
            Vector2 lookInput = context.ReadValue<Vector2>();

            if (lookInput != Vector2.zero)
                Debug.Log($"{PlayerPrefix} Look: {lookInput}");
        }

        private void HandlePlayerMove(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();

            if (moveInput != Vector2.zero)
                Debug.Log($"{PlayerPrefix} Move: {moveInput}");

            if (context.canceled)
                Debug.Log($"{PlayerPrefix} Move Stopped");
        }

        private void HandlePlayerNext(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Next");
        }

        private void HandlePlayerPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Previous");
        }

        private void HandlePlayerSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{PlayerPrefix} Sprint Started");

            if (context.canceled)
                Debug.Log($"{PlayerPrefix} Sprint Stopped");
        }

        // UI Handlers

        private void HandleUICancel(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{UIPrefix} Cancel");
        }

        private void HandleUIClick(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{UIPrefix} Click");
        }

        private void HandleUIMiddleClick(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{UIPrefix} Middle Click");
        }

        private void HandleUINavigate(InputAction.CallbackContext context)
        {
            Vector2 navigateInput = context.ReadValue<Vector2>();

            if (navigateInput != Vector2.zero)
                Debug.Log($"{UIPrefix} Navigate: {navigateInput}");

            if (context.canceled)
                Debug.Log($"{UIPrefix} Navigate Stopped");
        }

        private void HandleUIPoint(InputAction.CallbackContext context)
        {
            Vector2 pointPosition = context.ReadValue<Vector2>();

            if (pointPosition != Vector2.zero)
                Debug.Log($"{UIPrefix} Point: {pointPosition}");
        }

        private void HandleUIRightClick(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{UIPrefix} Right Click");
        }

        private void HandleUIScrollWheel(InputAction.CallbackContext context)
        {
            Vector2 scrollInput = context.ReadValue<Vector2>();

            if (scrollInput != Vector2.zero)
                Debug.Log($"{UIPrefix} Scroll Wheel: {scrollInput}");
        }

        private void HandleUISubmit(InputAction.CallbackContext context)
        {
            if (context.performed)
                Debug.Log($"{UIPrefix} Submit");
        }

        private void HandleUITrackedDeviceOrientation(InputAction.CallbackContext context)
        {
            Quaternion orientation = context.ReadValue<Quaternion>();

            if (orientation != Quaternion.identity)
                Debug.Log($"{UIPrefix} Tracked Device Orientation: {orientation.eulerAngles}");
        }

        private void HandleUITrackedDevicePosition(InputAction.CallbackContext context)
        {
            Vector3 position = context.ReadValue<Vector3>();

            if (position != Vector3.zero)
                Debug.Log($"{UIPrefix} Tracked Device Position: {position}");
        }
    }
}