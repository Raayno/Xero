// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;

namespace AxonGenesis
{
    public partial class AxonGUI
    {
        #region ICONS

        public static bool ButtonRefresh(string tooltip)
        {
            Texture2D icon = AxonUI.Icons.RefreshOff;
            Texture2D iconOn = AxonUI.Icons.RefreshOn;
            return AxonGUI.ButtonTexture(icon, iconOn, tooltip, new RectOffset(0, 0, 2, 0));
        }

        public static bool ButtonRemove(string tooltip)
        {
            return AxonGUI.ButtonTexture(AxonUI.Icons.Remove, tooltip);
        }

        #endregion

        #region BUTTONS

        public static bool Button(string label, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            bool pressed = false;

            pressed = GUILayout.Button(GetLabelWithTooltip(label), EditorStyles.toolbarButton, AddOptions(options));

            return pressed;
        }

        public static bool Button(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            bool pressed = false;

            pressed = GUILayout.Button(GetLabelWithTooltip(label), style, AddOptions(options));

            return pressed;
        }

        public static bool ButtonInline(string label, params GUILayoutOption[] options)
        {
            bool pressed = false;
            ClearIndent();

            options = AddOptions(options, false, GUILayout.Width(CalculateWidth(label) + 10));
            pressed = GUILayout.Button(GetLabelWithTooltip(label), EditorStyles.toolbarButton, options);

            RestoreIndent();
            return pressed;
        }

        public static bool ButtonInline(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            bool pressed = false;
            ClearIndent();

            options = AddOptions(options, false, GUILayout.Width(CalculateWidth(label, style) + 10));
            pressed = GUILayout.Button(GetLabelWithTooltip(label), style, options);

            RestoreIndent();
            return pressed;
        }

        public static int ButtonRow(List<string> labels, int index, params GUILayoutOption[] options)
        {
            BeginHorizontal(AxonUI.HeaderStyleDarkBig);
            int i = 0;
            foreach (string label in labels) {
                GUI.color = i == index ? SelectedColor : DeselectedColor;
                if (AxonGUI.ButtonInline(label)) {
                    index = i;
                    break;
                }
                i++;
            }
            GUI.color = AxonColor.Default;
            EndHorizontal(false);

            return index;
        }

        public static int ButtonRowInline(List<string> labels, int index, params GUILayoutOption[] options)
        {
            int i = 0;
            foreach (string label in labels) {
                GUI.color = i == index ? SelectedColor : DeselectedColor;
                if (AxonGUI.ButtonInline(label)) {
                    index = i;
                    break;
                }
                i++;
            }
            GUI.color = AxonColor.Default;
            return index;
        }

        public static int ButtonRowFoldout(List<string> labels, int index, ref bool unfolded, params GUILayoutOption[] options)
        {
            BeginHorizontal(AxonUI.HeaderStyleDarkBig);
            bool fold = FoldoutInline(unfolded);
            if (fold != unfolded) {
                unfolded = fold;
            }
            if (unfolded && index < 0) index = 0;
            LabelInline(" ", "");
            if (labels != null) {
                int i = 0;
                foreach (string label in labels) {
                    GUI.color = i == index ? SelectedColor : DeselectedColor;
                    if (AxonGUI.ButtonInline(label)) {
                        index = i;
                        unfolded = true;
                        break;
                    }
                    i++;
                }
            }
            GUI.color = AxonColor.Default;
            EndHorizontal(false);

            return index;
        }

        public static bool ButtonTexture(Texture2D texture, string tooltip) { return ButtonTexture(texture, null, tooltip, new RectOffset(1, 1, 0, 0)); }

        public static bool ButtonTexture(Texture2D texture, string tooltip, Vector2 size) { return ButtonTexture(texture, null, tooltip, new RectOffset(1, 1, 0, 0), true, size); }

        public static bool ButtonTexture(Texture2D texture, string tooltip, RectOffset margin) { return ButtonTexture(texture, null, tooltip, margin); }

        public static bool ButtonTexture(Texture2D texture, string tooltip, RectOffset margin, Vector2 size) { return ButtonTexture(texture, null, tooltip, margin, true, size); }

        public static bool ButtonTexture(Texture2D texture, string tooltip, bool inline) { return ButtonTexture(texture, null, tooltip, new RectOffset(1, 1, 0, 0), inline); }

        public static bool ButtonTexture(Texture2D texture, Texture2D textureOn, string tooltip) { return ButtonTexture(texture, textureOn, tooltip, new RectOffset(0, 0, 0, 0)); }

        public static bool ButtonTexture(Texture2D texture, Texture2D textureOn, string tooltip, Vector2 size) { return ButtonTexture(texture, textureOn, tooltip, new RectOffset(0, 0, 0, 0), true, size); }

        public static bool ButtonTexture(GUIStyle texture, string tooltip) { return ButtonTexture(texture.normal.background, tooltip, new RectOffset(0, 0, 0, 0)); }

        public static bool ButtonTexture(GUIStyle texture, string tooltip, Vector2 size) { return ButtonTexture(texture.normal.background, null, tooltip, new RectOffset(0, 0, 0, 0), true, size); }

        public static bool ButtonTexture(GUIStyle texture, string tooltip, RectOffset margin) { return ButtonTexture(texture.normal.background, tooltip, margin); }

        public static bool ButtonTexture(GUIStyle texture, string tooltip, RectOffset margin, Vector2 size) { return ButtonTexture(texture.normal.background, tooltip, margin, size); }

        public static bool ButtonTexture(GUIStyle texture, GUIStyle textureOn, string tooltip) { return ButtonTexture(texture.normal.background, textureOn.normal.background, tooltip, new RectOffset(0, 0, 0, 0)); }

        public static bool ButtonTexture(Texture2D texture, Texture2D textureOn, string tooltip, RectOffset margin) { return ButtonTexture(texture, textureOn, tooltip, margin, true); }

        public static bool ButtonTexture(Texture2D texture, Texture2D textureOn, string tooltip, RectOffset margin, bool inline)
        {
            if (texture == null) {
                //Debug.LogWarning("AxonGUI: Missing texture resource for button:"+tooltip);
                return false;
            }
            else {
                Vector2 size = new Vector2(texture.width, texture.height);
                return ButtonTexture(texture, textureOn, tooltip, margin, inline, size, false);
            }
        }

        public static bool ButtonTexture(Texture2D texture, Texture2D textureOn, string tooltip, RectOffset margin, bool inline, Vector2 size, bool stretch = false)
        {
            bool pressed = false;

            if (texture == null) {
                //Debug.LogWarning("AxonGUI: Missing texture resource for button:"+tooltip);
            }
            else {
                int indent = EditorGUI.indentLevel;
                if (inline) EditorGUI.indentLevel = 0;
                else if (!IsRowHorizontal) RowCount++;

                GUIStyle style = AxonUI.TextureButtonStyle;
                style.normal.background = texture;
                style.active.background = textureOn;
                style.alignment = TextAnchor.MiddleCenter;
                style.stretchWidth = stretch;
                style.stretchHeight = stretch;

                Rect rect = EditorGUILayout.GetControlRect(false, size.y, style, GUILayout.Width(size.x), GUILayout.Height(size.y));
                rect.x += (EditorGUI.indentLevel * 14) + margin.left;
                rect.y += margin.top;
                rect.width = size.x;
                rect.height = size.y;

                GUIContent content = new GUIContent("", tooltip);
                if (GUI.Button(rect, content, style)) {
                    pressed = true;
                }
                if (inline) EditorGUI.indentLevel = indent;
            }
            return pressed;
        }

        public static bool ButtonTexture(Rect rect, Texture2D texture, Texture2D textureOn, string tooltip, RectOffset margin, bool inline)
        {
            bool pressed = false;

            if (texture == null) {
                //Debug.LogWarning("AxonGUI: Missing texture resource for button:"+tooltip);
            }
            else {
                int indent = EditorGUI.indentLevel;
                if (inline) EditorGUI.indentLevel = 0;
                else if (!IsRowHorizontal) RowCount++;

                GUIStyle style = AxonUI.TextureButtonStyle;
                style.normal.background = texture;
                style.active.background = textureOn;
                style.margin = margin;

                GUIContent content = new GUIContent("", tooltip);
                if (GUI.Button(rect, content, style)) {
                    pressed = true;
                }
                if (inline) EditorGUI.indentLevel = indent;
            }
            return pressed;
        }

        public static bool ButtonTexture(Rect rect, Texture2D texture, string tooltip, RectOffset margin, bool inline)
        {
            bool pressed = false;

            if (texture == null) {
                //Debug.LogWarning("AxonGUI: Missing texture resource for button:"+tooltip);
            }
            else {
                int indent = EditorGUI.indentLevel;
                if (inline) EditorGUI.indentLevel = 0;
                else if (!IsRowHorizontal) RowCount++;

                GUIStyle style = AxonUI.TextureButtonStyle;
                style.normal.background = texture;
                style.active.background = texture;
                style.margin = margin;

                GUIContent content = new GUIContent("", tooltip);
                if (GUI.Button(rect, content, style)) {
                    pressed = true;
                }
                if (inline) EditorGUI.indentLevel = indent;
            }
            return pressed;
        }

        public static bool ButtonIcon(Texture2D texture, RectOffset margin, int size = 16, string tooltip = "")
        {
            return AxonGUI.ButtonTexture(texture, texture, tooltip, margin, true, new Vector2(size, size), false);
        }

        public static bool ButtonIcon(Texture2D texture, int size = 16, string tooltip = "")
        {
            return AxonGUI.ButtonTexture(texture, texture, tooltip, new RectOffset(0, 0, 0, 0), true, new Vector2(size, size), false);
        }

        public static bool ButtonIconUrl(Texture2D icon, string name, string url)
        {
            bool pressed = false;
            if (!string.IsNullOrEmpty(url) && ButtonTexture(icon, name)) {
                Debug.Log("Opening url:" + url);//--KEEP
                Application.OpenURL(url);
                pressed = true;
            }
            return pressed;
        }

        public static bool ButtonDocs(string name, string url)
        {
            bool pressed = false;
            if (!string.IsNullOrEmpty(url) && ButtonTexture(AxonUI.Icons.Docs, name)) {
                Debug.Log("Opening documentation url:" + url);//--KEEP
                Application.OpenURL(url);
                pressed = true;
            }
            return pressed;
        }

        public static bool ButtonLock(bool value, string tooltip)
        {
            return AxonGUI.ButtonTexture(value ? AxonUI.LockOnStyle : AxonUI.LockOffStyle, tooltip, new RectOffset(2, 4, 2, 4));
        }

        #endregion    }
    }
}
#endif
