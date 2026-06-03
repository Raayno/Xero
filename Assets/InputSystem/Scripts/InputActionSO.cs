using System;
using UnityEngine;

namespace Vastav.Utils.Input
{
    public class InputActionSO : ScriptableObject
    {
        public static InputActionSO GetInputActionSO(Type type)
        {
            return Resources.Load<InputActionSO>($"InputSystem/InputActions/{type.Name}");
        }
    }
}
