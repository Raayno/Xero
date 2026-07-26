// Copyright 2025 Axon Genesis. All rights reserved.  
// AxonGenesis.com  
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY  
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A  
// PARTICULAR PURPOSE.  

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

namespace AxonGenesis
{
    public class AdvancedPresetsDropdown : PopupWindowContent
    {
        #region STATIC  

        public static AdvancedPresetsDropdown Instance = null;

        public static void Menu(Color color, Texture2D icon, string label, int index, AdvancedPresetsMenuItem[] items, Action<int> onSelected, bool? isMinified = null)
        {
            const int iconSize = 16;
            const int padding = 4;
            const int height = 18;

            if (icon == null) icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;

            GUIStyle style = new GUIStyle(AxonUI.HeaderStyleClosed);
            style.fixedHeight = height;

            GUIContent textContent = GUIContent.none;
            Vector2 textSize = Vector2.zero;

            bool minify = isMinified ?? AdvancedPresetsWindow.IsMinified;
            if (!minify && !string.IsNullOrEmpty(label)) {
                textContent = new GUIContent(label);
                textSize = style.CalcSize(textContent);
            }

            // total dimensions: icon + text + padding
            float totalWidth = padding + iconSize + padding + textSize.x + padding;

            GUI.color = color;
            Rect rect = EditorGUILayout.GetControlRect(
                GUILayout.Width(totalWidth),
                GUILayout.Height(height)
            );

            rect.height = height;
            rect.y -= 1;

            if (GUI.Button(rect, GUIContent.none, style)) {
                //Debug.Log($"<color=orange>Advanced Presets:</color> Selected: {label} ({index})");
                AdvancedPresetsDropdown.Invoke(rect, onSelected, index, items);
            }

            GUI.color = Color.white;

            var iconRect = new Rect(
                rect.x + padding,
                rect.y + (height - iconSize) / 2,
                iconSize, iconSize
            );
            if (icon != null)
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            var textRect = new Rect(
                iconRect.xMax + padding,
                iconRect.y - 1,
                textSize.x, height
            );

            GUI.Label(textRect, textContent);
        }

        public static void Invoke(Rect rect, Action<int> callback, int selection, AdvancedPresetsMenuItem[] items)
        {
            PopupWindow.Show(rect, new AdvancedPresetsDropdown(rect, callback, selection, items));
        }

        public static void InvokeInGUI(Rect rect, Action<int> callback, int selection, AdvancedPresetsMenuItem[] items)
        {
            if (Instance == null) Instance = new AdvancedPresetsDropdown(rect, callback, selection, items);
            Instance.DirectGUI(rect);
        }

        #endregion

        public override Vector2 GetWindowSize() => new Vector2(200, height);

        private Rect _Rect;
        private Action<int> _Callback = null;
        private int _Selection = 0;
        private AdvancedPresetsMenuItem[] _Items = null;
        private float height = 18f;

        public AdvancedPresetsDropdown(Rect rect, Action<int> callback, int selection, AdvancedPresetsMenuItem[] items)
        {
            //Debug.Log($"<color=orange>Advanced Presets:</color> Dropdown created. Selection: {selection}, Items: {items?.Length ?? 0}");
            _Rect = rect;
            _Callback = callback;
            _Selection = selection;
            _Items = items;

            height = items.Length * 25;
        }

        ~AdvancedPresetsDropdown()
        {
            _Callback = null;
            _Items = null;
        }

        public override void OnClose()
        {
            //Debug.Log($"<color=orange>Advanced Presets:</color> Dropdown closed. Selection: {_Selection}");
            Instance = null;
            base.OnClose();
        }

        public override void OnGUI(Rect rect)
        {
            if (_Items == null || _Items.Length == 0) {
                if (AxonGUI.Button("Nothing Listed")) {
                    _Callback.Invoke(0);
                }
                return;
            }

            for (int i = 0; i < _Items.Length; i++) {
                AdvancedPresetsMenuItem item = _Items[i];
                GUI.color = item.Color;

                GUIStyle style = new GUIStyle(_Selection == i ? AxonUI.HeaderStyleOpen : AxonUI.HeaderStyleClosed);
                style.fixedHeight = 25;

                AxonGUI.BeginHorizontal(style);
                GUI.color = Color.white;

                // Draw the icon (16x16) to the left of the name  
                if (item.Icon != null) {
                    GUILayout.Label(item.Icon, GUILayout.Width(16), GUILayout.Height(16));
                }

                if (AxonGUI.Button(item.Name, GUI.skin.label)) {
                    //Debug.Log($"<color=orange>Advanced Presets:</color> Selected: {item.Name} ({i})");
                    _Callback.Invoke(i);
                    editorWindow.Close();
                }

                AxonGUI.EndHorizontal();
            }
        }

        public void DirectGUI(Rect rect)
        {
            if (_Items == null || _Items.Length == 0) {
                Rect buttonRect = new Rect(rect.x, rect.y, rect.width, 25);
                if (GUI.Button(buttonRect, "Nothing Listed")) {
                    _Callback.Invoke(0);
                }
                return;
            }

            float yOffset = rect.y;
            for (int i = 0; i < _Items.Length; i++) {
                AdvancedPresetsMenuItem item = _Items[i];
                GUI.color = item.Color;

                Rect itemRect = new Rect(rect.x, yOffset, rect.width, 25);
                GUIStyle style = new GUIStyle(_Selection == i ? AxonUI.HeaderStyleOpen : AxonUI.HeaderStyleClosed);
                style.fixedHeight = 25;

                GUI.Box(itemRect, GUIContent.none, style);
                GUI.color = Color.white;

                // Draw the icon (16x16) to the left of the name  
                if (item.Icon != null) {
                    Rect iconRect = new Rect(itemRect.x + 5, itemRect.y + 4.5f, 16, 16);
                    GUI.DrawTexture(iconRect, item.Icon, ScaleMode.ScaleToFit);
                }

                Rect labelRect = new Rect(itemRect.x + 25, itemRect.y + 4.5f, itemRect.width - 30, 16);
                if (GUI.Button(labelRect, item.Name, GUI.skin.label)) {
                    //Debug.Log($"<color=orange>Advanced Presets:</color> Selected: {item.Name} ({i})");
                    _Callback.Invoke(i);
                    editorWindow.Close();
                }

                yOffset += 25;
            }
        }
    }
}

#endif
