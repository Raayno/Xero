using UnityEngine;

namespace Vastav.Utils.Input
{
    public class InputManager : MoreMountains.Tools.MMSingleton<InputManager>
    {
        [SerializeField] private InputSystem_PlayerActionsSO inputSystem_PlayerActionsSO;
        [SerializeField] private InputSystem_UIActionsSO inputSystem_UIActionsSO;

        public InputSystem_Actions inputActions { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (this == Instance)
            {
                DontDestroyOnLoad(gameObject);
            }
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            EnableUIActions();
            EnablePlayerActions();
        }

        private void OnDisable()
        {
            DisableUIActions();
            DisablePlayerActions();
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
