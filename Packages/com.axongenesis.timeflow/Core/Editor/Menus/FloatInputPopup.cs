// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AxonGenesis
{

    public class FloatInputPopup : EditorWindow
    {
        private const int _width = 120;
        private const int _height = 90;
        private float _value = 0f; // The float value to be entered
        private string _label = ""; // The label of the input field
        private System.Action<float> onValueSubmitted; // Callback when the value is submitted

        /// <summary>
        /// Opens the popup near the mouse position.
        /// </summary>
        public static void ShowPopup(string label, float value, System.Action<float> onValueSubmitted)
        {
            // Create the popup window
            FloatInputPopup popup = ScriptableObject.CreateInstance<FloatInputPopup>();
            popup._value = value;
            popup._label = label;
            popup.onValueSubmitted = onValueSubmitted;

            // Get the mouse position
            Vector2 mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            // Set the position and size of the popup
            popup.position = new Rect(mousePosition.x, mousePosition.y, _width, _height);
            popup.maxSize = new Vector2(_width, _height);

            // Show the popup as a utility window
            popup.ShowUtility();
        }

        private void OnGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.Label(_label, EditorStyles.boldLabel);

            AxonGUI.BeginHorizontal();
            _value = AxonGUI.FieldFloat(null, _value, GUILayout.Width(80));
            AxonGUI.EndHorizontal();

            AxonGUI.Space(10);

            AxonGUI.BeginHorizontal();
            if (AxonGUI.ButtonInline("Cancel")) {
                Close(); // Close the popup window
            }
            if (AxonGUI.ButtonInline("Apply")) {
                onValueSubmitted?.Invoke(_value);
                Close(); // Close the popup window
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();
        }
    }

}

#endif