// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace AxonGenesis
{
    public class InputUtil
    {
#if ENABLE_INPUT_SYSTEM
        private static Dictionary<KeyCode, Key> lookup;
#else
        private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
#endif

        public static bool IsAnyKey()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.anyKey.isPressed;
#else
            return Input.anyKey;
#endif
        }

        public static bool IsAnyKeyReleased()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && !Keyboard.current.anyKey.isPressed || Keyboard.current.anyKey.wasReleasedThisFrame;
#else
            return !Input.anyKey;
#endif
        }

        public static KeyCode DetectKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (lookup == null) BuildLookup();
            foreach (KeyValuePair<KeyCode, Key> key in lookup) {
                try {
                    if (Keyboard.current[key.Value].isPressed) {
                        return lookup.FirstOrDefault(x => x.Value == key.Value).Key;
                    }
                }
                catch {
                    return KeyCode.None;
                }
            }
            return KeyCode.None;
#else
            for (int i = 0; i < keyCodes.Length; i++) {
                if (Input.GetKey(keyCodes[i])) {
                    return keyCodes[i];
                }
            }
            return KeyCode.None;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        public static void BuildLookup()
        {
            if (Keyboard.current == null) return;
            lookup = new Dictionary<KeyCode, Key>();
            foreach (KeyControl control in Keyboard.current.allKeys) {
                Key key = control.keyCode;
                if (Enum.TryParse<KeyCode>(key.ToString().Replace("Numpad", "Keypad"), true, out var value)) {
                    lookup[value] = key;
                }
            }
            lookup[KeyCode.Return] = Key.Enter;
        }
#endif

        public static bool GetKeyDown(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (lookup == null) BuildLookup();
            bool pressed = false;
            if (lookup.ContainsKey(code)) {
                pressed = Keyboard.current[lookup[code]].wasPressedThisFrame;
            }
            return pressed;
#else
            return Input.GetKeyDown(code);
#endif
        }

        public static bool GetKeyUp(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (lookup == null) BuildLookup();
            if (lookup.ContainsKey(code)) {
                return Keyboard.current[lookup[code]].wasReleasedThisFrame;
            }
            return false;
#else
            return Input.GetKeyUp(code);
#endif
        }
    }
}
