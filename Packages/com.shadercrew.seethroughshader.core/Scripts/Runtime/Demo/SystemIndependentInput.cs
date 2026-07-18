using UnityEngine;

#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    //using UnityEngine.InputSystem.LowLevel;
#endif
namespace ShaderCrew.SeeThroughShader
{
    public static class SystemIndependentInput
    {
        private const string noInputSystem = "The project is set to use the New Input System, but the Input System package is not installed." +
        " Please install the Input System package or switch to the Old Input Manager in Player Settings." +
        "\r\nNo input system is currently available.";

        public static bool GetKeyDown(KeyCode key)
        {

#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
            if (Keyboard.current != null)
            {
                //Key newKey;
                //if (TryConvertKeyCode(key, out newKey))
                //    return Keyboard.current[newKey].wasPressedThisFrame;


                string target = key.ToString().ToLower(); 
                foreach (var keyNew in Keyboard.current.allKeys)
                {
                    if (keyNew != null && keyNew.displayName != null && keyNew.displayName.ToLower() == target)
                        return keyNew.wasPressedThisFrame;
                }

            }

            // Mouse?
            if (TryConvertMouseButton(key, out var btn))
                return btn.wasPressedThisFrame;

            return false;

#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);

#else
            Debug.LogWarning(noInputSystem);
            return false;
#endif
        }


        public static bool GetKey(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
            // Keyboard?
            if (Keyboard.current != null)
            {
                //Key newKey;
                //if (TryConvertKeyCode(key, out newKey))
                //    return Keyboard.current[newKey].isPressed;

                string target = key.ToString().ToLower();
                foreach (var keyNew in Keyboard.current.allKeys)
                {
                    if (keyNew.displayName.ToLower() == target)
                        return keyNew.isPressed;
                }
            }

            // Mouse?
            if (TryConvertMouseButton(key, out var btn))
                return btn.isPressed;

            return false;

#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            return false;
#endif
        }



        public static bool GetMouseButton(int button)
        {
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
            var mouse = Mouse.current;
            if (mouse == null) return false;

            switch (button)
            {
                case 0: return mouse.leftButton.isPressed;
                case 1: return mouse.rightButton.isPressed;
                case 2: return mouse.middleButton.isPressed;
                default: return false;
            }

#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(button);
#else
            return false;
#endif
        }

        public static bool GetMouseButtonDown(int button)
        {
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
            var mouse = Mouse.current;
            if (mouse == null) return false;

            switch (button)
            {
                case 0: return mouse.leftButton.wasPressedThisFrame;
                case 1: return mouse.rightButton.wasPressedThisFrame;
                case 2: return mouse.middleButton.wasPressedThisFrame;
                default: return false;
            }

#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(button);
#else
            return false;
#endif
        }

        public static Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null) return Vector2.zero;
            return mouse.delta.ReadValue();

#elif ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }



        public static Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
            return Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#else
            return null;
#endif
        }

#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE_INSTALLED
        //doesnt work with internation keyboard layouts
        //private static bool TryConvertKeyCode(KeyCode oldKey, out Key newKey)
        //{
        //    switch (oldKey)
        //    {
        //        case KeyCode.D: newKey = Key.D; return true;
        //        case KeyCode.A: newKey = Key.A; return true;
        //        case KeyCode.W: newKey = Key.W; return true;
        //        case KeyCode.S: newKey = Key.S; return true;
        //        case KeyCode.I: newKey = Key.I; return true;

        //        case KeyCode.X: newKey = Key.X; return true;
        //        case KeyCode.Y: newKey = Key.Y; return true;


        //        case KeyCode.Q: newKey = Key.Q; return true;
        //        case KeyCode.E: newKey = Key.E; return true;

        //        case KeyCode.Space: newKey = Key.Space; return true;
        //        case KeyCode.Escape: newKey = Key.Escape; return true;
        //        case KeyCode.UpArrow: newKey = Key.UpArrow; return true;
        //        case KeyCode.DownArrow: newKey = Key.DownArrow; return true;
        //        case KeyCode.LeftArrow: newKey = Key.LeftArrow; return true;
        //        case KeyCode.RightArrow: newKey = Key.RightArrow; return true;

        //        default:
        //            newKey = Key.None;
        //            return false;
        //    }
        //}

        private static bool TryConvertMouseButton(KeyCode oldKey, out ButtonControl button)
        {
            var m = Mouse.current;
            button = null;
            if (m == null) return false;

            switch (oldKey)
            {
                case KeyCode.Mouse0: button = m.leftButton; return true;
                case KeyCode.Mouse1: button = m.rightButton; return true;
                case KeyCode.Mouse2: button = m.middleButton; return true;
                // optional:
                case KeyCode.Mouse3: button = m.backButton; return true;
                case KeyCode.Mouse4: button = m.forwardButton; return true;
            }

            return false;
        }

#endif
    }
}