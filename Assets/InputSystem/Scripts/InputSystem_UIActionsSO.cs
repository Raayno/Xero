using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Vastav.Utils.Input
{
    [CreateAssetMenu(fileName = "InputSystem_UIActionsSO", menuName = "InputSystem/UIActionsSO")]
    public class InputSystem_UIActionsSO : InputActionSO, InputSystem_Actions.IUIActions
    {
        public static event Action<InputAction.CallbackContext> OnCancelEvent;
        public static event Action<InputAction.CallbackContext> OnClickEvent;
        public static event Action<InputAction.CallbackContext> OnMiddleClickEvent;
        public static event Action<InputAction.CallbackContext> OnNavigateEvent;
        public static event Action<InputAction.CallbackContext> OnPointEvent;
        public static event Action<InputAction.CallbackContext> OnRightClickEvent;
        public static event Action<InputAction.CallbackContext> OnScrollWheelEvent;
        public static event Action<InputAction.CallbackContext> OnSubmitEvent;
        public static event Action<InputAction.CallbackContext> OnTrackedDeviceOrientationEvent;
        public static event Action<InputAction.CallbackContext> OnTrackedDevicePositionEvent;

        public void OnCancel(InputAction.CallbackContext context)
        {
            OnCancelEvent?.Invoke(context);
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            OnClickEvent?.Invoke(context);
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            OnMiddleClickEvent?.Invoke(context);
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            OnNavigateEvent?.Invoke(context);
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            OnPointEvent?.Invoke(context);
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            OnRightClickEvent?.Invoke(context);
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            OnScrollWheelEvent?.Invoke(context);
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            OnSubmitEvent?.Invoke(context);
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
            OnTrackedDeviceOrientationEvent?.Invoke(context);
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
            OnTrackedDevicePositionEvent?.Invoke(context);
        }
    }
}