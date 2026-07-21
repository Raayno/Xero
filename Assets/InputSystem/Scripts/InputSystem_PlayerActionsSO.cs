using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Vastav.Utils.Input
{
    [CreateAssetMenu(fileName = "InputSystem_PlayerActionsSO", menuName = "InputSystem/PlayerActionSO")]
    public class InputSystem_PlayerActionsSO : InputActionSO, InputSystem_Actions.IPlayerActions
    {
        [SerializeField] private bool isParrySeparateFromInteract = false;
        public static event Action<InputAction.CallbackContext> OnAttackEvent;
        public static event Action<InputAction.CallbackContext> OnCrouchEvent;
        public static event Action<InputAction.CallbackContext> OnParryEvent;
        public static event Action<InputAction.CallbackContext> OnInteractEvent;
        public static event Action<InputAction.CallbackContext> OnJumpEvent;
        public static event Action<InputAction.CallbackContext> OnLookEvent;
        public static event Action<InputAction.CallbackContext> OnMoveEvent;
        public static event Action<InputAction.CallbackContext> OnNextEvent;
        public static event Action<InputAction.CallbackContext> OnPreviousEvent;
        public static event Action<InputAction.CallbackContext> OnSprintEvent;

        public void OnAttack(InputAction.CallbackContext context)
        {
            OnAttackEvent?.Invoke(context);
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            OnCrouchEvent?.Invoke(context);
        }

        /// <summary>
        /// This method handles the Interact action. If a Parry event is subscribed, it will invoke the Parry event instead of the Interact event.
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!isParrySeparateFromInteract && OnParryEvent != null)
            {
                OnParryEvent.Invoke(context);
                return; // If parry event is handled, do not invoke interact event
            }

            OnInteractEvent?.Invoke(context);
        }

        /// <summary>
        /// In case we want to handle parry separately, we use this method
        /// </summary>
        public void OnParry(InputAction.CallbackContext context)
        {
            if (isParrySeparateFromInteract)
            {
                OnParryEvent?.Invoke(context);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            OnJumpEvent?.Invoke(context);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            OnLookEvent?.Invoke(context);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveEvent?.Invoke(context);
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            OnNextEvent?.Invoke(context);
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            OnPreviousEvent?.Invoke(context);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            OnSprintEvent?.Invoke(context);
        }
    }
}