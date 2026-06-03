using UnityEngine;

namespace Vastav.Utils.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputSystem_PlayerActionsSO inputSystem_PlayerActionsSO;
        [SerializeField] private InputSystem_UIActionsSO inputSystem_UIActionsSO;

        public InputSystem_Actions inputActions { get; private set; }

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
            EnableUIActions();
            EnablePlayerActions();
        }

        public void EnablePlayerActions()
        {
            inputActions.Player.SetCallbacks(inputSystem_PlayerActionsSO);
            inputActions.Player.Enable();
        }

        public void DisablePlayerActions()
        {
            inputActions.Player.RemoveCallbacks(inputSystem_PlayerActionsSO);
            inputActions.Player.Disable();
        }

        public void EnableUIActions()
        {
            inputActions.UI.SetCallbacks(inputSystem_UIActionsSO);
            inputActions.UI.Enable();
        }

        public void DisableUIActions()
        {
            inputActions.UI.RemoveCallbacks(inputSystem_UIActionsSO);
            inputActions.UI.Disable();
        }
    }
}
