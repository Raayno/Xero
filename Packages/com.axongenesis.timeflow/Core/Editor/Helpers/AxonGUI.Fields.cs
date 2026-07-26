// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AxonGenesis
{
    public partial class AxonGUI
    {
        private static void OnEndField()
        {
            _UndoEnabled = false;
        }

        #region TOGGLE

        public static bool FieldToggle(Object target, bool value, params GUILayoutOption[] options) { return FieldToggle(target, null, value, options); }

        public static bool FieldToggle(Object target, string label, bool value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            bool v = EditorGUILayout.Toggle(GetLabelWithTooltip(label), value, AddOptions(options, GUILayout.Width(EditorGUIUtility.labelWidth + 20)));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Toggle";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static bool FieldToggleLeft(Object target, string label, bool value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            bool v = EditorGUILayout.ToggleLeft(GetLabelWithTooltip(label), value, AddOptions(options, GUILayout.Width(EditorGUIUtility.labelWidth + 20)));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Toggle";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static bool FieldToggleInline(Object target, bool value, params GUILayoutOption[] options) { return FieldToggleInline(target, null, value, options); }

        public static bool FieldToggleInline(Object target, string label, bool value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = 0;
            float width = 15;

            bool v = EditorGUILayout.Toggle(value, AddOptions(options, false, GUILayout.Width(width)));

            width = CalculateWidth(label) + 5;
            EditorGUILayout.LabelField(GetLabelWithTooltip(label), GUILayout.Width(width));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Toggle";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static bool FieldToggleLock(bool value, string tooltip)
        {
            bool v = FieldToggleLock(value, tooltip, new RectOffset(2, 4, 2, 4));
            OnEndField();
            return v;
        }

        public static bool FieldToggleLock(bool value, string tooltip, Vector2 size) => FieldToggleLock(value, tooltip, new RectOffset(2, 4, 2, 4), new Vector2(16, 16));

        public static bool FieldToggleLock(bool value, string tooltip, RectOffset margin) => FieldToggleLock(value, tooltip, margin, new Vector2(16, 16));

        public static bool FieldToggleLock(bool value, string tooltip, RectOffset margin, Vector2 size)
        {
            if (AxonGUI.ButtonTexture(value ? AxonUI.LockOnStyle : AxonUI.LockOffStyle, tooltip, margin, size)) {
                value = !value;
            }
            OnEndField();
            return value;
        }

        public static bool FieldToggleMinMax(Object target, bool enabled, RectOffset rectOffset)
        {
            Texture2D icon = enabled ? AxonUI.MinMaxFieldToggleOnStyle.normal.background : AxonUI.MinMaxFieldToggleOffStyle.normal.background;
            Texture2D iconOn = !enabled ? AxonUI.MinMaxFieldToggleOffStyle.normal.background : AxonUI.MinMaxFieldToggleOnStyle.normal.background;
            AxonGUI.SetTooltip("Toggle between min/max or +/- randomization input");
            if (AxonGUI.ButtonTexture(icon, iconOn, "Toggle between min/max or +/- randomization input", rectOffset)) {
                _Undo(target, UndoName, $"{enabled}");
                enabled = !enabled;
                FocusControl(null);
            }
            OnEndField();
            return enabled;
        }

        public static bool FieldToggleMinMax(Object target, bool enabled)
        {
            return FieldToggleMinMax(target, enabled, new RectOffset(5, 0, 4, 0));
        }

        public static bool FieldToggleEnabled(Object target, bool enabled, RectOffset rectOffset)
        {
            Texture2D icon = enabled ? AxonUI.BehaviorOnStyle.normal.background : AxonUI.BehaviorOffStyle.normal.background;
            Texture2D iconOn = !enabled ? AxonUI.BehaviorOffStyle.normal.background : AxonUI.BehaviorOnStyle.normal.background;
            if (AxonGUI.ButtonTexture(icon, iconOn, "Toggle to enable or disable this property", rectOffset)) {
                _Undo(target, "Enabled", $"{enabled}");
                enabled = !enabled;
            }
            OnEndField();
            return enabled;
        }

        public static bool FieldToggleEnabled(Object target, bool enabled)
        {
            return FieldToggleEnabled(target, enabled, new RectOffset(1, 0, 4, 0));
        }

        public static bool FieldToggleDebug(bool enabled)
        {
            if (TimeflowPreferences.DebugEnabled) {
                Texture2D icon = enabled ? AxonUI.Icons.DebugOn : AxonUI.Icons.DebugOff;
                Texture2D iconOn = !enabled ? AxonUI.Icons.DebugOn : AxonUI.Icons.DebugOff;
                if (AxonGUI.ButtonTexture(icon, iconOn, "Enables debug logging to the console for this object or channel.")) {
                    if (Event.current != null && Event.current.alt) {
                        AxonTools.DisableDebugAll();
                    }
                    enabled = !enabled;
                }
            }
            OnEndField();
            return enabled && TimeflowPreferences.DebugEnabled;
        }

        public static bool FieldToggleUniform(Object target, bool enabled) { return FieldToggleUniform(target, enabled, new RectOffset(0, 0, 0, 0)); }

        public static bool FieldToggleUniform(Object target, bool enabled, RectOffset margin)
        {
            Texture2D icon = enabled ? AxonUI.UniformValueOnStyle.normal.background : AxonUI.UniformValueOffStyle.normal.background;
            Texture2D iconOn = !enabled ? AxonUI.UniformValueOnStyle.normal.background : AxonUI.UniformValueOffStyle.normal.background;
            if (AxonGUI.ButtonTexture(icon, iconOn, "Uniform Value", margin, true, new Vector2(16, 16))) {
                _Undo(target, "Uniform Value", $"{enabled}");
                enabled = !enabled;
            }
            OnEndField();
            return enabled;
        }

        #endregion

        #region TEXT

        public static string FieldText(Object target, string value, params GUILayoutOption[] options) { return FieldText(target, null, value, options); }

        public static string FieldText(Object target, string label, string value, params GUILayoutOption[] options) { return FieldText(target, label, value, false, options); }

        public static string FieldText(Object target, string label, string value, bool delayed, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            string v;
            if (value == null) value = "";
            if (delayed) {
                v = EditorGUILayout.DelayedTextField(GetLabelWithTooltip(label), value, AddOptions(options));
            }
            else {
                v = EditorGUILayout.TextField(GetLabelWithTooltip(label), value, AddOptions(options));
            }

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Text";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static string FieldTextInline(Object target, string value, params GUILayoutOption[] options) { return FieldTextInline(target, null, value, false, options); }

        public static string FieldTextInline(Object target, string label, string value, params GUILayoutOption[] options) { return FieldTextInline(target, label, value, false, options); }

        public static string FieldTextInline(Object target, string label, string value, bool delayed, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            string v;
            if (delayed) {
                v = EditorGUILayout.DelayedTextField(GetLabelWithTooltip(label), value, AddOptions(options, false));
            }
            else {
                v = EditorGUILayout.TextField(GetLabelWithTooltip(label), value, AddOptions(options, false));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Text";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region TEXTAREA

        public static string FieldTextArea(Object target, string value, params GUILayoutOption[] options) { return FieldTextArea(target, null, value, options); }

        public static string FieldTextArea(Object target, string label, string value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            Label(label, "");
            ResetLabelWidth();

            if (value == null) value = "";
            string v = EditorGUILayout.TextArea(value, AddOptions(options));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Text Field";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static string FieldTextAreaInline(Object target, string value, params GUILayoutOption[] options) { return FieldTextAreaInline(target, null, value, options); }

        public static string FieldTextAreaInline(Object target, string label, string value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            string v = EditorGUILayout.TextArea(value, AddOptions(options, false));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Text Field";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region FLOAT

        public static float FieldFloat(Object target, float value, params GUILayoutOption[] options) { return FieldFloat(target, null, value, false, options); }

        public static float FieldFloat(Object target, string label, float value, params GUILayoutOption[] options) { return FieldFloat(target, label, value, false, options); }

        public static float FieldFloat(Object target, string label, float value, bool delayed, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            float v = value;
            if (delayed) {
                v = EditorGUILayout.DelayedFloatField(GetLabelWithTooltip(label), value, AddOptions(options, GUILayout.Width(LabelWidth + FloatWidth)));
            }
            else {
                v = EditorGUILayout.FloatField(GetLabelWithTooltip(label), value, AddOptions(options, GUILayout.Width(LabelWidth + FloatWidth)));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Text Field";
                _Undo(target, label, $"{v}");
                value = v;
            }
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static float FieldFloatInline(Object target, float value, params GUILayoutOption[] options) { return FieldFloatInline(target, null, value, false, options); }

        public static float FieldFloatInline(Object target, string label, float value, params GUILayoutOption[] options) { return FieldFloatInline(target, label, value, false, options); }

        public static float FieldFloatInline(Object target, string label, float value, bool delayed, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            float v = value;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                if (delayed) {
                    v = EditorGUILayout.DelayedFloatField(GetLabelWithTooltip(label), value, AddOptions(options, false));
                }
                else {
                    v = EditorGUILayout.FloatField(GetLabelWithTooltip(label), value, AddOptions(options, false));
                }
            }
            else {
                EditorGUIUtility.labelWidth = 10; // Add a tiny label for value scrubbing feature
                if (delayed) {
                    v = EditorGUILayout.DelayedFloatField(GetLabelWithTooltip(" "), value, AddOptions(options));
                }
                else {
                    v = EditorGUILayout.FloatField(GetLabelWithTooltip(" "), value, AddOptions(options));
                }
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Text Field";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region FLOAT SLIDER

        public static float FieldSlider(Object target, float value, float min, float max, params GUILayoutOption[] options)
        {
            return FieldSlider(target, null, value, min, max, options);
        }

        public static float FieldSlider(Object target, string label, float value, float min, float max, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            float v = EditorGUILayout.Slider(GetLabelWithTooltip(label), value, min, max, AddOptions(options));
            if (string.IsNullOrEmpty(label)) label = "Slider";
            if (v != value) {
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static float FieldSliderInline(Object target, float value, float min, float max, params GUILayoutOption[] options)
        {
            return FieldSliderInline(target, null, value, min, max, options);
        }

        public static float FieldSliderInline(Object target, string label, float value, float min, float max, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            float v;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                v = EditorGUILayout.Slider(GetLabelWithTooltip(label), value, min, max, AddOptions(options, false));
            }
            else {
                label = "Slider";
                v = EditorGUILayout.Slider(value, min, max, AddOptions(options, false));
            }
            if (value != v) {
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static void FieldSliderMinMax(Object target, string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            float min = minValue;
            float max = maxValue;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.MinMaxSlider(GetLabelWithTooltip(label), ref min, ref max, minLimit, maxLimit, AddOptions(options, false));
            }
            else {
                label = "Slider";
                EditorGUILayout.MinMaxSlider(ref min, ref max, minLimit, maxLimit, AddOptions(options, false));
            }
            if (minValue != min || maxValue != max) {
                _Undo(target, label, $"{min} to {max}");
                minValue = min;
                maxValue = max;
            }

            RestoreLabelWidth();
            OnEndField();
        }

        public static void FieldSliderMinMaxInline(Object target, string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            float min = minValue;
            float max = maxValue;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                EditorGUILayout.MinMaxSlider(GetLabelWithTooltip(label), ref min, ref max, minLimit, maxLimit, AddOptions(options, false));
            }
            else {
                label = "Slider";
                EditorGUILayout.MinMaxSlider(ref min, ref max, minLimit, maxLimit, AddOptions(options, false));
            }
            if (minValue != min || maxValue != max) {
                _Undo(target, label, $"{min} to {max}");
                minValue = min;
                maxValue = max;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
        }

        #endregion

        #region INT

        public static int FieldInt(Object target, int value, params GUILayoutOption[] options) { return FieldInt(target, null, value, false, options); }

        public static int FieldInt(Object target, string label, int value, params GUILayoutOption[] options) { return FieldInt(target, label, value, false, options); }

        public static int FieldInt(Object target, string label, int value, bool delayed, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            int v = value;
            if (delayed) {
                v = EditorGUILayout.DelayedIntField(GetLabelWithTooltip(label), value, AddOptions(options, GUILayout.Width(LabelWidth + FloatWidth)));
            }
            else {
                v = EditorGUILayout.IntField(GetLabelWithTooltip(label), value, AddOptions(options, GUILayout.Width(LabelWidth + FloatWidth)));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Int Value";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldIntInline(Object target, int value, params GUILayoutOption[] options) { return FieldIntInline(target, null, value, false, options); }

        public static int FieldIntInline(Object target, string label, int value, params GUILayoutOption[] options) { return FieldIntInline(target, label, value, false, options); }

        public static int FieldIntInline(Object target, string label, int value, bool delayed, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            int v = value;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                if (delayed) {
                    v = EditorGUILayout.DelayedIntField(GetLabelWithTooltip(label), value, AddOptions(options, false, GUILayout.Width(EditorGUIUtility.labelWidth + FloatWidth)));
                }
                else {
                    v = EditorGUILayout.IntField(GetLabelWithTooltip(label), value, AddOptions(options, false, GUILayout.Width(EditorGUIUtility.labelWidth + FloatWidth)));
                }
            }
            else {
                label = "Int Value";
                if (delayed) {
                    v = EditorGUILayout.DelayedIntField(value, AddOptions(options, false, GUILayout.Width(FloatWidth)));
                }
                else {
                    v = EditorGUILayout.IntField(value, AddOptions(options, false, GUILayout.Width(FloatWidth)));
                }
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Int Value";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region INT SLIDER

        public static int FieldSliderInt(Object target, int value, int min, int max, params GUILayoutOption[] options)
        {
            return FieldSliderInt(target, null, value, min, max, options);
        }

        public static int FieldSliderInt(Object target, string label, int value, int min, int max, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            int v = EditorGUILayout.IntSlider(GetLabelWithTooltip(label), value, min, max, AddOptions(options));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Int Slider";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldSliderIntInline(Object target, int value, int min, int max, params GUILayoutOption[] options)
        {
            return FieldSliderIntInline(target, null, value, min, max, options);
        }

        public static int FieldSliderIntInline(Object target, string label, int value, int min, int max, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            int v = value;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                v = EditorGUILayout.IntSlider(GetLabelWithTooltip(label), value, min, max, AddOptions(options, false));
            }
            else {
                v = EditorGUILayout.IntSlider(value, min, max, AddOptions(options, false));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Int Slider";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region COLOR

        public static Color FieldColor(Object target, Color value, bool hdr, params GUILayoutOption[] options) { return FieldColor(target, "", value, hdr, options); }

        public static Color FieldColor(Object target, string label, Color value, bool hdr, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            Color v = EditorGUILayout.ColorField(new GUIContent(label), value, true, true, hdr, AddOptions(options, false, GUILayout.Width(LabelWidth + 100)));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Color Value";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Color FieldColorInline(Object target, Color value, bool hdr, params GUILayoutOption[] options) { return FieldColorInline(target, "", value, hdr, options); }

        public static Color FieldColorInline(Object target, string label, Color value, bool hdr, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            float w = EditorGUIUtility.labelWidth = CalculateWidth(label);
            Color v = EditorGUILayout.ColorField(new GUIContent(label), value, true, true, hdr, AddOptions(options, false));
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Color Value";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region VECTOR2

        public static Vector2 FieldVector2(Object target, Vector2 value, params GUILayoutOption[] options) { return FieldVector2(target, null, value, false, options); }

        public static Vector2 FieldVector2(Object target, string label, Vector2 value, params GUILayoutOption[] options) { return FieldVector2(target, label, value, false, options); }

        public static Vector2 FieldVector2(Object target, string label, Vector2 value, bool delayed, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            BeginHorizontal();

            EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(LabelWidth));
            ClearIndent();

            Vector2 v = value;
            EditorGUIUtility.labelWidth = TinyLabelWidth;
            if (delayed) {
                v.x = EditorGUILayout.DelayedFloatField("x", value.x);
                v.y = EditorGUILayout.DelayedFloatField("y", value.y);
            }
            else {
                v.x = EditorGUILayout.FloatField("x", value.x);
                v.y = EditorGUILayout.FloatField("y", value.y);
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 2";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();

            EndHorizontal();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Vector2 FieldVector2Inline(Object target, Vector2 value, params GUILayoutOption[] options) { return FieldVector2Inline(target, null, value, options); }

        public static Vector2 FieldVector2Inline(Object target, string label, Vector2 value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(CalculateWidth(label)));
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            Vector2 v = value;
            v.x = EditorGUILayout.FloatField("x", value.x);
            v.y = EditorGUILayout.FloatField("y", value.y);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 2";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region VECTOR3

        public static Vector3 FieldVector3(Object target, Vector3 value, params GUILayoutOption[] options) { return FieldVector3(target, null, value, options); }

        public static Vector3 FieldVector3(Object target, string label, Vector3 value, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            BeginHorizontal();

            EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(LabelWidth));

            EditorGUIUtility.labelWidth = TinyLabelWidth;

            ClearIndent();

            Vector3 v = value;
            v.x = EditorGUILayout.FloatField("x", value.x);
            v.y = EditorGUILayout.FloatField("y", value.y);
            v.z = EditorGUILayout.FloatField("z", value.z);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 3";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();

            EndHorizontal();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Vector3 FieldVector3Inline(Object target, Vector3 value, params GUILayoutOption[] options) { return FieldVector3Inline(target, null, value, options); }

        public static Vector3 FieldVector3Inline(Object target, string label, Vector3 value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(CalculateWidth(label)));
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            Vector3 v = value;
            v.x = EditorGUILayout.FloatField("x", value.x);
            v.y = EditorGUILayout.FloatField("y", value.y);
            v.z = EditorGUILayout.FloatField("z", value.z);
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 3";
                _Undo(target, label, $"{v}");
                value = v;
            }


            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Vector2 FieldVector2Inline(Object target, Rect rect, string label, Vector2 value, params GUILayoutOption[] options)
        {
            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 2f;
            rect.width = width;

            Vector2 v = value;
            v.x = EditorGUI.FloatField(rect, "x", value.x);
            rect.x += rect.width;
            v.y = EditorGUI.FloatField(rect, "y", value.y);
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 2";
                _Undo(target, label, $"{v}");
                value = v;
            }

            OnEndField();
            return value;
        }

        public static Vector3 FieldVector3Inline(Object target, Rect rect, string label, Vector3 value, params GUILayoutOption[] options)
        {
            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 3f;
            rect.width = width;

            Vector3 v = value;
            v.x = EditorGUI.FloatField(rect, "x", value.x);
            rect.x += rect.width;
            v.y = EditorGUI.FloatField(rect, "y", value.y);
            rect.x += rect.width;
            v.z = EditorGUI.FloatField(rect, "z", value.z);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 3";
                _Undo(target, label, $"{v}");
                value = v;
            }

            OnEndField();
            return value;
        }

        #endregion

        #region VECTOR4

        public static Vector4 FieldVector4AsRectInline(Object target, Rect rect, string label, Vector4 value, params GUILayoutOption[] options)
        {
            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 4f;
            rect.width = width;

            Vector4 v = value;
            v.x = EditorGUI.FloatField(rect, "x", value.x);
            rect.x += rect.width;
            v.y = EditorGUI.FloatField(rect, "y", value.y);
            rect.x += rect.width;
            v.z = EditorGUI.FloatField(rect, "w", value.z);
            rect.x += rect.width;
            v.w = EditorGUI.FloatField(rect, "h", value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect";
                _Undo(target, label, $"{v}");
                value = v;
            }
            OnEndField();
            return value;
        }

        public static Vector4 FieldVector4AsRectOffsetInline(Object target, Rect rect, string label, Vector4 value, params GUILayoutOption[] options)
        {
            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 4f;
            rect.width = width;

            Vector4 v = value;
            v.x = EditorGUI.IntField(rect, "l", (int)value.x);
            rect.x += rect.width;
            v.y = EditorGUI.IntField(rect, "r", (int)value.y);
            rect.x += rect.width;
            v.z = EditorGUI.IntField(rect, "t", (int)value.z);
            rect.x += rect.width;
            v.w = EditorGUI.IntField(rect, "b", (int)value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect Offset";
                _Undo(target, label, $"{v}");
                value = v;
            }

            OnEndField();
            return value;
        }

        public static Vector4 FieldVector4Inline(Object target, Rect rect, string label, Vector4 value, params GUILayoutOption[] options)
        {
            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 4f;
            rect.width = width;

            Vector4 v = value;
            v.x = EditorGUI.FloatField(rect, "x", value.x);
            rect.x += rect.width;
            v.y = EditorGUI.FloatField(rect, "y", value.y);
            rect.x += rect.width;
            v.z = EditorGUI.FloatField(rect, "z", value.z);
            rect.x += rect.width;
            v.w = EditorGUI.FloatField(rect, "w", value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 4";
                _Undo(target, label, $"{v}");
                value = v;
            }

            OnEndField();
            return value;
        }

        public static Vector4 FieldVector4(Object target, Vector4 value, params GUILayoutOption[] options) { return FieldVector4(target, null, value, options); }

        public static Vector4 FieldVector4(Object target, string label, Vector4 value, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            BeginHorizontal();

            EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(LabelWidth));

            EditorGUIUtility.labelWidth = TinyLabelWidth;
            ClearIndent();

            Vector4 v = value;
            v.x = EditorGUILayout.FloatField("x", value.x);
            v.y = EditorGUILayout.FloatField("y", value.y);
            v.z = EditorGUILayout.FloatField("z", value.z);
            v.w = EditorGUILayout.FloatField("w", value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 4";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();

            EndHorizontal();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Vector4 FieldVector4AsRect(Object target, string label, Vector4 value, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            BeginHorizontal();

            EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(LabelWidth));

            EditorGUIUtility.labelWidth = TinyLabelWidth;
            ClearIndent();

            Vector4 v = value;
            v.x = EditorGUILayout.FloatField("x", value.x);
            v.y = EditorGUILayout.FloatField("y", value.y);
            v.z = EditorGUILayout.FloatField("w", value.z);
            v.w = EditorGUILayout.FloatField("h", value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();

            EndHorizontal();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Vector4 FieldVector4AsRectOffset(Object target, string label, Vector4 value, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            BeginHorizontal();

            EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(LabelWidth));

            EditorGUIUtility.labelWidth = TinyLabelWidth;
            ClearIndent();

            Vector4 v = value;
            v.x = EditorGUILayout.IntField("l", (int)value.x);
            v.y = EditorGUILayout.IntField("r", (int)value.y);
            v.z = EditorGUILayout.IntField("t", (int)value.z);
            v.w = EditorGUILayout.IntField("b", (int)value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect Offset";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();

            EndHorizontal();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Vector4 FieldVectorType(Object target, string label, Vector4 value, Property.PropertyTypes type, params GUILayoutOption[] options)
        {
            if (type == Property.PropertyTypes.Rect) {
                return FieldVector4AsRect(target, label, value, options);
            }
            else
            if (type == Property.PropertyTypes.RectOffset) {
                return FieldVector4AsRectOffset(target, label, value, options);
            }
            else {
                return FieldVector4(target, label, value, options);
            }
        }

        public static Vector4 PropertySelectValue(Object target, string label, Vector4 value, Property property, params GUILayoutOption[] options)
        {
            Property.PropertyTypes type = property.PropertyType;
            if (type == Property.PropertyTypes.Float || property.Attribute > -1) {
                value.x = FieldFloat(target, label, value.x, options);
                return value;
            }
            else
            if (type == Property.PropertyTypes.Int) {
                value.x = FieldInt(target, label, (int)value.x, options);
                return value;
            }
            else
            if (type == Property.PropertyTypes.Bool) {
                value.x = FieldToggle(target, label, value.x > 0.5f, options) ? value.x : 0f;
                return value;
            }
            else
            if (type == Property.PropertyTypes.Color) {
                value = (Vector4)FieldColor(target, label, (Color)value, true, options);
                return value;
            }
            else
            if (type == Property.PropertyTypes.Rect) {
                return FieldVector4AsRect(target, label, value, options);
            }
            else
            if (type == Property.PropertyTypes.RectOffset) {
                return FieldVector4AsRectOffset(target, label, value, options);
            }
            else {
                return FieldVector4(target, label, value, options);
            }
        }

        public static Vector4 FieldVector4Inline(Object target, Vector4 value, params GUILayoutOption[] options) { return FieldVector4Inline(target, null, value, options); }

        public static Vector4 FieldVector4Inline(Object target, string label, Vector4 value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(CalculateWidth(label)));
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            Vector4 v = value;
            v.x = EditorGUILayout.FloatField("x", value.x);
            v.y = EditorGUILayout.FloatField("y", value.y);
            v.z = EditorGUILayout.FloatField("z", value.z);
            v.w = EditorGUILayout.FloatField("w", value.w);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Vector 4";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region RECT

        public static Rect FieldRect(Object target, string label, Rect value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            Rect v = EditorGUILayout.RectField(GetLabelWithTooltip(label), value, AddOptions(options));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Rect FieldRectInline(Object target, string label, Rect value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            Rect v = EditorGUILayout.RectField(GetLabelWithTooltip(label), value, AddOptions(options, false));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Rect FieldRectInline(Object target, Rect rect, string label, Rect value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(CalculateWidth(label)));
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 4f;
            rect.width = width;

            Rect v = value;
            v.xMin = EditorGUI.FloatField(rect, "x", value.xMin);
            rect.x += rect.width;
            v.yMin = EditorGUI.FloatField(rect, "y", value.yMin);
            rect.x += rect.width;
            v.xMax = EditorGUI.FloatField(rect, "w", value.xMax);
            rect.x += rect.width;
            v.yMax = EditorGUI.FloatField(rect, "h", value.yMax);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static RectOffset FieldRectOffsetInline(Object target, Rect rect, string label, RectOffset value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(CalculateWidth(label)));
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            float width = rect.width;
            if (!string.IsNullOrEmpty(label)) {
                rect.width = CalculateWidth(label);
                EditorGUI.LabelField(rect, GetLabel(label), "");
            }
            EditorGUIUtility.labelWidth = TinyLabelWidth;

            rect.x += rect.width;
            width = (width - rect.width) / 4f;
            rect.width = width;

            RectOffset v = value;
            v.left = EditorGUI.IntField(rect, "l", value.left);
            rect.x += rect.width;
            v.right = EditorGUI.IntField(rect, "r", value.right);
            rect.x += rect.width;
            v.top = EditorGUI.IntField(rect, "t", value.top);
            rect.x += rect.width;
            v.bottom = EditorGUI.IntField(rect, "b", value.bottom);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Rect Offset";
                _Undo(target, label, $"{v}");
                value = v;
            }

            OnEndField();
            return value;
        }

        public static Rect FieldRectInline(Object target, Rect value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            Rect v = EditorGUILayout.RectField(value, AddOptions(options, false));

            if (value != v) {
                _Undo(target, "Rect", $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static EdgeRect FieldEdgeRect(Object target, string label, EdgeRect value)
        {
            ResetLabelWidth();
            BeginHorizontal();
            EditorGUILayout.LabelField(GetLabel(label), "", GUILayout.Width(EditorGUIUtility.labelWidth));

            EditorGUIUtility.labelWidth = TinyLabelWidth;

            ClearIndent();

            EdgeRect v = value;
            v.l = EditorGUILayout.FloatField("L", value.l);
            v.r = EditorGUILayout.FloatField("R", value.r);
            v.t = EditorGUILayout.FloatField("T", value.t);
            v.b = EditorGUILayout.FloatField("B", value.b);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Edge Rect";
                _Undo(target, label, $"{v}");
                value = v;
            }
            RestoreIndent();
            EndHorizontal();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region OBJECT

        public static Object FieldObject(Object target, Object value, Type type, bool inScene, params GUILayoutOption[] options)
        {
            return FieldObject(target, null, value, type, inScene, options);
        }

        public static Object FieldObject(Object target, string label, Object value, Type type, bool inScene, params GUILayoutOption[] options)
        {
            return FieldObject(target, label, value, type, inScene, true, options);
        }

        public static Object FieldObject(Object target, string label, Object value, Type type, bool inScene, bool useMaxWidth, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            Object v = EditorGUILayout.ObjectField(GetLabelWithTooltip(label), value, type, inScene, AddOptions(options, useMaxWidth));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Object";
                _Undo(target, label, $"{(v == null ? "null" : v.name)}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Object FieldObjectInline(Object target, Object value, Type type, bool inScene, params GUILayoutOption[] options)
        {
            return FieldObjectInline(target, null, value, type, inScene, options);
        }

        public static Object FieldObjectInline(Object target, string label, Object value, Type type, bool inScene, params GUILayoutOption[] options)
        {
            return FieldObjectInline(target, label, value, type, inScene, true, options);
        }

        public static Object FieldObjectInline(Object target, string label, Object value, Type type, bool inScene, bool useMaxWidth, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            Object v = EditorGUILayout.ObjectField(GetLabelWithTooltip(label), value, type, inScene, AddOptions(options, useMaxWidth));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Object";
                _Undo(target, label, $"{(v == null ? "null" : v.name)}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static void FieldTimeValue(Object target, TimeValue value, params GUILayoutOption[] options) { FieldTimeValue(target, null, value, false, options); }

        public static void FieldTimeValue(Object target, string label, TimeValue value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();
            FieldTimeValue(target, label, value, false, options);
            RestoreLabelWidth();
            OnEndField();
        }

        public static void FieldTimeValue(Object target, string label, TimeValue value, bool inline, params GUILayoutOption[] options)
        {
            if (!inline) {
                BeginHorizontal();
            }
            if (value.Mode == TimeValue.Modes.Time) {
                if (inline) {
                    value.TimeType = (TimeValue.TimeTypes)AxonGUI.FieldEnumPopupInline(target, label, value.TimeType);
                }
                else {
                    value.TimeType = (TimeValue.TimeTypes)AxonGUI.FieldEnumPopup(target, label, value.TimeType, GUILayout.Width(EditorGUIUtility.labelWidth + 120));
                }
                if (value.TimeType == TimeValue.TimeTypes.Seconds) {
                    value.Time = AxonGUI.FieldFloatInline(target, value.Time);
                }
                else
                if (value.TimeType == TimeValue.TimeTypes.Frames) {
                    value.Frame = AxonGUI.FieldIntInline(target, value.Frame);
                }
                else
                if (value.TimeType == TimeValue.TimeTypes.Beats) {
                    value.Note = (MusicUtil.Notes)AxonGUI.FieldEnumPopupInline(target, value.Note, GUILayout.Width(100));
                    if (value.NoteCount < 0.01f) value.NoteCount = 0.01f;
                    value.NoteCount = AxonGUI.FieldFloatInline(target, "X", value.NoteCount);
                    if (AxonGUI.ButtonInline("x2")) {
                        value.NoteCount *= 2f;
                    }
                    if (AxonGUI.ButtonInline("/2")) {
                        value.NoteCount /= 2f;
                    }
                }
                else
                if (value.TimeType == TimeValue.TimeTypes.Marker) {
                    value.Marker = AxonGUI.FieldMarkerInline(null, value.Marker);
                }
                else
                if (value.TimeType == TimeValue.TimeTypes.ObjectEnd || value.TimeType == TimeValue.TimeTypes.ObjectStart) {
                    value.Object = (TimeflowObject)AxonGUI.FieldObjectInline(target, value.Object, typeof(TimeflowObject), true);
                }
            }
            else
            if (value.Mode == TimeValue.Modes.Duration) {
                if (inline) {
                    value.DurationType = (TimeValue.DurationTypes)AxonGUI.FieldEnumPopupInline(target, label, value.DurationType);
                }
                else {
                    value.DurationType = (TimeValue.DurationTypes)AxonGUI.FieldEnumPopup(target, label, value.DurationType, GUILayout.Width(EditorGUIUtility.labelWidth + 70));
                }
                if (value.DurationType == TimeValue.DurationTypes.Seconds) {
                    value.Time = AxonGUI.FieldFloatInline(target, value.Time);
                }
                else
                if (value.DurationType == TimeValue.DurationTypes.Beats) {
                    if (!value.UseTimeflowBPM) {
                        AxonGUI.SetTooltip("Sets the beats per minute the motion timing is based on.");
                        value.BPM = AxonGUI.FieldFloatInline(target, "BPM", value.BPM);
                    }
                    float sceneBPM = value.Object == null || value.Object.Timeflow == null ? 120 : value.Object.Timeflow.BPM;

                    AxonGUI.SetTooltip("Use the BPM defined in the main Timeflow or container of this object. Most often it is desirable to enable this option to sync with the scene, though can be manually set for polyrythmic effects.");
                    value.UseTimeflowBPM = AxonGUI.FieldToggleInline(target, "Timeflow BPM (" + sceneBPM + ")", value.UseTimeflowBPM);
                    if (!inline) {
                        EndHorizontal();
                        BeginHorizontal();
                        AxonGUI.SetTooltip("Sets a the musical note to base duration on. Note durations are calculated based on the BPM (beats per minute).");
                        MusicUtil.Notes note = (MusicUtil.Notes)AxonGUI.FieldEnumPopup(target, "Note", value.Note);
                        if (!value.Note.Equals(note)) {
                            _Undo(target, "Note", $"{note}");
                            value.Note = note;
                        }
                    }
                    else {
                        AxonGUI.SetTooltip("Sets a the musical note to base duration on. Note durations are calculated based on the BPM (beats per minute).");
                        MusicUtil.Notes note = (MusicUtil.Notes)AxonGUI.FieldEnumPopupInline(target, value.Note);
                        if (!value.Note.Equals(note)) {
                            _Undo(target, "Note", $"{note}");
                            value.Note = note;
                        }
                    }
                    if (value.NoteCount < 0.01f) value.NoteCount = 0.01f;
                    AxonGUI.SetTooltip("The timing interval is based on the note type selected multiplied by this value (the number of notes).");
                    value.NoteCount = AxonGUI.FieldFloatInline(target, "X", value.NoteCount, GUILayout.Width(60));
                    if (AxonGUI.ButtonInline("x2")) {
                        value.NoteCount *= 2f;
                    }
                    if (AxonGUI.ButtonInline("/2")) {
                        value.NoteCount /= 2f;
                    }
                }
                else
                if (value.DurationType == TimeValue.DurationTypes.ObjectDuration) {
                    AxonGUI.SetTooltip("This sets the duration to the Timeflow object track duration. This can be helpful to create a simple to-from animation spanning the length of the track.");
                    value.Object = (TimeflowObject)AxonGUI.FieldObjectInline(target, value.Object, typeof(TimeflowObject), true);
                }
                else
                if (value.DurationType == TimeValue.DurationTypes.Markers) {
                    AxonGUI.SetTooltip("Select markers from the Timeflow view to define the time range.");
                    value.Marker = AxonGUI.FieldMarkerInline(target, "From", value.Marker);
                    value.MarkerEnd = AxonGUI.FieldMarkerInline(target, "To", value.MarkerEnd);
                }
            }
            if (!inline) {
                EndHorizontal();
            }
            if (GUI.changed) {
                value.Calculate();
            }
            OnEndField();
        }

        public static void FieldTimeValueInline(Object target, TimeValue value, params GUILayoutOption[] options) { FieldTimeValueInline(target, null, value, options); }

        public static void FieldTimeValueInline(Object target, string label, TimeValue value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = 0;
            FieldTimeValue(target, label, value, true, options);

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
        }

        #endregion

        #region LAYERS

        public static int FieldLayerMask(Object target, int value, params GUILayoutOption[] options) { return FieldLayerMask(target, null, value, options); }

        public static int FieldLayerMask(Object target, string label, int value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            GetLayerMaskLayers();

            int v = EditorGUILayout.MaskField(GetLabelWithTooltip(label), value, layerNames);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Layer Mask";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldLayerMask(Object target, Rect rect, int value, params GUILayoutOption[] options) { return FieldLayerMask(target, null, value, options); }

        public static int FieldLayerMask(Object target, Rect rect, string label, int value, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;

            GetLayerMaskLayers();

            int v = EditorGUI.MaskField(rect, GetLabelWithTooltip(label), value, layerNames);

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Layer Mask";
                _Undo(target, label, $"{v}");
                value = v;
            }

            OnEndField();
            return value;
        }

        public static string[] GetLayerMaskLayers()
        {
            if (layers == null) {
                layers = new List<string>();
                layerNames = new string[4];
            }
            else {
                layers.Clear();
            }

            int emptyLayers = 0;
            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "") {

                    for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                    layers.Add(layerName);
                }
                else {
                    emptyLayers++;
                }
            }

            if (layerNames.Length != layers.Count) {
                layerNames = new string[layers.Count];
            }
            for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];

            return layerNames;
        }

        public static int FieldLayerMaskInline(Object target, int value, params GUILayoutOption[] options) { return FieldLayerMaskInline(target, null, value, options); }

        public static int FieldLayerMaskInline(Object target, string label, int value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            int v = value;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                v = FieldLayerMask(target, GetLabel(label), value, AddOptions(options, false));
            }
            else {
                v = FieldLayerMask(target, value, AddOptions(options, false));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Layer Mask";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldLayerMaskInline(Object target, Rect rect, int value, params GUILayoutOption[] options) { return FieldLayerMaskInline(target, null, value, options); }

        public static int FieldLayerMaskInline(Object target, Rect rect, string label, int value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            int v = value;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                v = FieldLayerMask(target, rect, GetLabel(label), value, AddOptions(options, false));
            }
            else {
                v = FieldLayerMask(target, rect, value, AddOptions(options, false));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Layer Mask";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldLayerSelect(Object target, int value, params GUILayoutOption[] options) { return FieldLayerSelect(target, null, value, false, options); }

        public static int FieldLayerSelect(Object target, string label, int value, params GUILayoutOption[] options) { return FieldLayerSelect(target, label, value, false, options); }

        public static int FieldLayerSelect(Object target, string label, int value, bool inline, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            if (layers == null) {
                layers = new List<string>();
                layerNames = new string[4];
            }
            else {
                layers.Clear();
            }

            int emptyLayers = 0;
            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "") {
                    for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                    layers.Add(layerName);
                }
                else {
                    emptyLayers++;
                }
            }

            if (layerNames.Length != layers.Count) {
                layerNames = new string[layers.Count];
            }
            for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];

            int v = value;
            if (inline) {
                v = FieldPopupInline(target, GetLabel(label), value, layerNames);
            }
            else {
                v = EditorGUILayout.Popup(GetLabelWithTooltip(label), value, layerNames);
            }

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Layer Select";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldLayerSelectInline(Object target, string label, int value, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            int v = value;
            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
                v = FieldLayerSelect(target, GetLabel(label), value, true, AddOptions(options, false));
            }
            else {
                v = FieldLayerSelect(target, value, AddOptions(options, false));
            }
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Layer Select";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region POPUP

        public static Enum FieldEnumPopup(Object target, Enum value, params GUILayoutOption[] options) { return FieldEnumPopupInline(target, null, value, false, options); }

        public static Enum FieldEnumPopup(Object target, string label, Enum value, params GUILayoutOption[] options) { return FieldEnumPopup(target, label, value, false, options); }

        public static Enum FieldEnumPopup(Object target, string label, Enum value, bool maskSelect, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            Enum v = value;
            if (maskSelect) {
                v = EditorGUILayout.EnumFlagsField(GetLabelWithTooltip(label), value, AddOptions(options));
            }
            else {
                v = EditorGUILayout.EnumPopup(GetLabelWithTooltip(label), value, AddOptions(options));
            }

            if (!value.Equals(v)) {
                if (string.IsNullOrEmpty(label)) label = "Enum";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static Enum FieldEnumPopupInline(Object target, Enum value, params GUILayoutOption[] options) { return FieldEnumPopupInline(target, null, value, false, options); }

        public static Enum FieldEnumPopupInline(Object target, string label, Enum value, params GUILayoutOption[] options) { return FieldEnumPopupInline(target, label, value, false, options); }

        public static Enum FieldEnumPopupInline(Object target, string label, Enum value, bool maskSelect, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            Enum v = value;
            if (maskSelect) {
                v = EditorGUILayout.EnumFlagsField(GetLabelWithTooltip(label), value, AddOptions(options));
            }
            else {
                v = EditorGUILayout.EnumPopup(GetLabelWithTooltip(label), value, AddOptions(options));
            }
            if (!value.Equals(v)) {
                if (string.IsNullOrEmpty(label)) label = "Enum";
                //Debug.Log($"{value} != {v}");
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldPopup(Object target, int value, string[] values, params GUILayoutOption[] options)
        {
            return FieldPopup(target, null, value, values, options);
        }

        public static int FieldPopup(Object target, string label, int value, string[] values, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            int v = EditorGUILayout.Popup(GetLabelWithTooltip(label), value, values, AddOptions(options));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Popup";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldPopup(Object target, string label, int value, string[] values, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            // There is no variation of Popup that takes a style and GUIContent label without the values being GUIContent[]
            int v = EditorGUILayout.Popup(label, value, values, style, AddOptions(options));

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Popup";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static int FieldPopupInline(Object target, int value, string[] values, params GUILayoutOption[] options)
        {
            return FieldPopupInline(target, null, value, values, options);
        }

        public static int FieldPopupInline(Object target, string label, int value, string[] values, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            if (values != null && values.Length > 0) {
                if (value < 0) value = 0;
                else
                if (value >= values.Length) value = values.Length - 1;

                EditorGUIUtility.labelWidth = CalculateWidth(label);
                float width = CalculateWidth(values[value]) + EditorGUIUtility.labelWidth + 20;

                int v = EditorGUILayout.Popup(GetLabelWithTooltip(label), value, values, AddOptions(options, GUILayout.Width(width)));

                if (value != v) {
                    if (string.IsNullOrEmpty(label)) label = "Popup";
                    _Undo(target, label, $"{v}");
                    value = v;
                }

            }
            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static string FieldPopupString(Object target, string value, string[] values, params GUILayoutOption[] options)
        {
            return FieldPopupString(target, null, value, values, options);
        }

        public static string FieldPopupString(Object target, string label, string value, string[] values, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            int i = 0;
            int id = -1;
            foreach (string val in values) {
                if (val == value) {
                    id = i;
                    break;
                }
                i++;
            }
            id = EditorGUILayout.Popup(GetLabelWithTooltip(label), id, values, AddOptions(options));

            string v;
            if (id >= 0) v = values[id];
            else v = "";

            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Popup";
                _Undo(target, label, $"{v}");
                value = v;
            }

            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static string FieldPopupStringInline(Object target, string value, string[] values, params GUILayoutOption[] options)
        {
            return FieldPopupStringInline(target, null, value, values, options);
        }

        public static string FieldPopupStringInline(Object target, string label, string value, string[] values, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            if (values == null || values.Length == 0) {
                EditorGUILayout.LabelField(GetLabel(label), "No values", AddOptions(options));
                return value;
            }
            else {
                int i = 0;
                int id = -1;
                foreach (string val in values) {
                    if (val == value) {
                        id = i;
                        break;
                    }
                    i++;
                }
                id = EditorGUILayout.Popup(GetLabelWithTooltip(label), id, values, AddOptions(options));

                string v;
                if (id >= 0) v = values[id];
                else v = "";

                if (value != v) {
                    if (string.IsNullOrEmpty(label)) label = "Popup";
                    _Undo(target, label, $"{v}");
                    value = v;
                }
            }
            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        public static string FieldPopupString(Object target, Rect rect, string label, string value, string[] values)
        {
            if (values == null || values.Length == 0) return value;
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);

            if (values == null || values.Length == 0) {
                EditorGUILayout.LabelField(GetLabel(label), "No values");
                return value;
            }
            else {
                int i = 0;
                int id = -1;
                GUIContent[] contents = new GUIContent[values.Length];
                foreach (string val in values) {
                    contents[i] = new GUIContent(val);
                    if (val == value) {
                        id = i;
                        //break;
                    }
                    i++;
                }
                id = EditorGUI.Popup(rect, GetLabelWithTooltip(label), id, contents);

                string v;
                if (id >= 0) v = values[id];
                else v = "";

                if (value != v) {
                    if (string.IsNullOrEmpty(label)) label = "Popup";
                    _Undo(target, label, $"{v}");
                    value = v;
                }
            }

            RestoreIndent();
            RestoreLabelWidth();
            OnEndField();
            return value;
        }

        #endregion

        #region RANDOM MIN-MAX

        private const float MinMaxFieldWidth = 55;

        public static void FieldFloatMinMax(Object target, string label, ref float value, ref float plusMinus, ref bool isMinMax, string tooltip1 = null, string tooltip2 = null)
        {
            float baseWidth = AxonGUI.LabelWidth;

            if (string.IsNullOrEmpty(tooltip1)) tooltip1 = isMinMax ? "Min Value" : "Target Value";
            if (string.IsNullOrEmpty(tooltip1)) tooltip2 = isMinMax ? "Max Value" : "Add/Subtract Amount";

            if (isMinMax) {
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                float timeMin = value - plusMinus;
                timeMin = AxonGUI.FieldFloat(target, $"{label}", timeMin, GUILayout.Width(baseWidth));

                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);

                AxonGUI.UndoName = $"Set {label} Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxX = value + plusMinus;
                maxX = AxonGUI.FieldFloatInline(target, maxX, GUILayout.Width(MinMaxFieldWidth));

                value = (timeMin + maxX) / 2f;
                plusMinus = (maxX - timeMin) / 2f;
            }
            else {
                AxonGUI.UndoName = $"Set {label}";
                AxonGUI.SetTooltip(tooltip1);
                value = AxonGUI.FieldFloat(target, label, value, GUILayout.Width(baseWidth));

                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);

                AxonGUI.UndoName = $"Set {label} +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus = AxonGUI.FieldFloatInline(target, plusMinus, GUILayout.Width(MinMaxFieldWidth));
            }
        }

        public static void FieldVector2MinMax(Object target, string label, ref Vector2 value, ref Vector2 plusMinus, ref bool isMinMax, string tooltip1 = null, string tooltip2 = null)
        {
            BeginHorizontal();

            float baseWidth = AxonGUI.LabelWidth;

            if (string.IsNullOrEmpty(tooltip1)) tooltip1 = isMinMax ? "Min Value" : "Target Value";
            if (string.IsNullOrEmpty(tooltip1)) tooltip2 = isMinMax ? "Max Value" : "Add/Subtract Amount";

            if (isMinMax) {
                AxonGUI.Label($"{label}", GUILayout.Width(baseWidth));

                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);

                Vector2 min = new Vector2(value.x - plusMinus.x, value.y - plusMinus.y);
                min.x = AxonGUI.FieldFloatInline(target, $"X", min.x, GUILayout.Width(MinMaxFieldWidth));
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} X Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxX = value.x + plusMinus.x;
                maxX = AxonGUI.FieldFloatInline(target, maxX, GUILayout.Width(MinMaxFieldWidth));
                value.x = (min.x + maxX) / 2f;
                plusMinus.x = (maxX - min.x) / 2f;

                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                min.y = AxonGUI.FieldFloatInline(target, $"  Y", min.y, GUILayout.Width(MinMaxFieldWidth));
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Y Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxY = value.y + plusMinus.y;
                maxY = AxonGUI.FieldFloatInline(target, maxY, GUILayout.Width(MinMaxFieldWidth));
                value.y = (min.y + maxY) / 2f;
                plusMinus.y = (maxY - min.y) / 2f;
            }
            else {
                AxonGUI.Label($"{label}", GUILayout.Width(baseWidth));

                AxonGUI.UndoName = $"Set {label} X";
                AxonGUI.SetTooltip(tooltip1);
                value.x = AxonGUI.FieldFloatInline(target, $"X", value.x, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} X +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.x = AxonGUI.FieldFloatInline(target, plusMinus.x, GUILayout.Width(MinMaxFieldWidth));

                AxonGUI.UndoName = $"Set {label} Y";
                AxonGUI.SetTooltip(tooltip1);
                value.y = AxonGUI.FieldFloatInline(target, $"  Y", value.y, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Y +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.y = AxonGUI.FieldFloatInline(target, plusMinus.y, GUILayout.Width(MinMaxFieldWidth));
            }
            EndHorizontal();
        }

        public static void FieldVector2MinMax(Object target, string label, ref Vector4 value, ref Vector4 plusMinus, ref bool isMinMax, string tooltip1 = null, string tooltip2 = null)
        {
            Vector2 b = value;
            Vector2 p = plusMinus;
            FieldVector2MinMax(target, label, ref b, ref p, ref isMinMax, tooltip1, tooltip2);
            value = new Vector4(b.x, b.y, value.z, value.w);
            plusMinus = new Vector4(p.x, p.y, plusMinus.z, plusMinus.w);
        }

        public static void FieldVector3MinMax(Object target, string label, ref Vector4 value, ref Vector4 plusMinus, ref bool isMinMax, string tooltip1 = null, string tooltip2 = null)
        {
            Vector3 b = value;
            Vector3 p = plusMinus;
            FieldVector3MinMax(target, label, ref b, ref p, ref isMinMax, tooltip1, tooltip2);
            value = new Vector4(b.x, b.y, b.z, value.w);
            plusMinus = new Vector4(p.x, p.y, p.z, plusMinus.w);
        }

        public static void FieldColorMinMax(Object target, string label, ref Color value, ref Color plusMinus, ref bool isMinMax, bool hdr = true, string tooltip1 = null, string tooltip2 = null)
        {
            Vector4 b = value;
            Vector4 p = plusMinus;
            FieldColorMinMax(target, label, ref b, ref p, ref isMinMax, hdr, tooltip1, tooltip2);
            value = new Vector4(b.x, b.y, b.z, b.w);
            plusMinus = new Vector4(p.x, p.y, p.z, p.w);
        }

        public static void FieldColorMinMax(Object target, string label, ref Vector4 value, ref Vector4 plusMinus, ref bool isMinMax, bool hdr = true, string tooltip1 = null, string tooltip2 = null)
        {
            BeginHorizontal();

            float baseWidth = AxonGUI.LabelWidth;

            if (string.IsNullOrEmpty(tooltip1)) tooltip1 = isMinMax ? "Min Value" : "Target Value";
            if (string.IsNullOrEmpty(tooltip1)) tooltip2 = isMinMax ? "Max Value" : "Add/Subtract Amount";

            if (isMinMax) {
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                Color minVal = value - plusMinus;
                minVal = AxonGUI.FieldColor(target, $"{label}", minVal, hdr, GUILayout.Width(baseWidth));

                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);

                AxonGUI.UndoName = $"Set {label} Max";
                AxonGUI.SetTooltip(tooltip2);
                Vector4 maxVal = value + plusMinus;
                maxVal = AxonGUI.FieldColorInline(target, " ", maxVal, hdr, GUILayout.Width(MinMaxFieldWidth + 20));

                value = Vector4.Lerp(minVal, maxVal, 0.5f);
                plusMinus = maxVal - value;
            }
            else {
                AxonGUI.UndoName = $"Set {label}";
                AxonGUI.SetTooltip(tooltip1);
                value = AxonGUI.FieldColor(target, label, value, hdr, GUILayout.Width(baseWidth));

                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);

                AxonGUI.UndoName = $"Set {label} +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus = AxonGUI.FieldColorInline(target, " ", plusMinus, hdr, GUILayout.Width(MinMaxFieldWidth + 20));
            }

            EndHorizontal();
        }

        public static void FieldVector3MinMax(Object target, string label, ref Vector3 value, ref Vector3 plusMinus, ref bool isMinMax, string tooltip1 = null, string tooltip2 = null)
        {
            BeginHorizontal();
            float baseWidth = AxonGUI.LabelWidth;

            if (string.IsNullOrEmpty(tooltip1)) tooltip1 = isMinMax ? "Min Value" : "Target Value";
            if (string.IsNullOrEmpty(tooltip1)) tooltip2 = isMinMax ? "Max Value" : "Add/Subtract Amount";

            if (isMinMax) {
                AxonGUI.Label($"{label}", GUILayout.Width(baseWidth));

                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);

                Vector3 min = new Vector3(value.x - plusMinus.x, value.y - plusMinus.y, value.z - plusMinus.z);

                // X
                min.x = AxonGUI.FieldFloatInline(target, $"X", min.x, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} X Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxX = value.x + plusMinus.x;
                maxX = AxonGUI.FieldFloatInline(target, maxX, GUILayout.Width(MinMaxFieldWidth));
                value.x = (min.x + maxX) / 2f;
                plusMinus.x = (maxX - min.x) / 2f;

                // Y
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                min.y = AxonGUI.FieldFloatInline(target, $"  Y", min.y, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Y Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxY = value.y + plusMinus.y;
                maxY = AxonGUI.FieldFloatInline(target, maxY, GUILayout.Width(MinMaxFieldWidth));
                value.y = (min.y + maxY) / 2f;
                plusMinus.y = (maxY - min.y) / 2f;

                // Z
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                min.z = AxonGUI.FieldFloatInline(target, $"  Z", min.z, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Z Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxZ = value.z + plusMinus.z;
                maxZ = AxonGUI.FieldFloatInline(target, maxZ, GUILayout.Width(MinMaxFieldWidth));
                value.z = (min.z + maxZ) / 2f;
                plusMinus.z = (maxZ - min.z) / 2f;
            }
            else {
                AxonGUI.Label($"{label}", GUILayout.Width(baseWidth));

                // X
                AxonGUI.UndoName = $"Set {label} X";
                AxonGUI.SetTooltip(tooltip1);
                value.x = AxonGUI.FieldFloatInline(target, $"X", value.x, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} X +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.x = AxonGUI.FieldFloatInline(target, plusMinus.x, GUILayout.Width(MinMaxFieldWidth));

                // Y
                AxonGUI.UndoName = $"Set {label} Y";
                AxonGUI.SetTooltip(tooltip1);
                value.y = AxonGUI.FieldFloatInline(target, $"  Y", value.y, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Y +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.y = AxonGUI.FieldFloatInline(target, plusMinus.y, GUILayout.Width(MinMaxFieldWidth));

                // Z
                AxonGUI.UndoName = $"Set {label} Z";
                AxonGUI.SetTooltip(tooltip1);
                value.z = AxonGUI.FieldFloatInline(target, $"  Z", value.z, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Z +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.z = AxonGUI.FieldFloatInline(target, plusMinus.z, GUILayout.Width(MinMaxFieldWidth));
            }
            EndHorizontal();
        }

        public static void FieldVector4MinMax(Object target, string label, ref Vector4 value, ref Vector4 plusMinus, ref bool isMinMax, string tooltip1 = null, string tooltip2 = null)
        {
            BeginHorizontal();

            float baseWidth = AxonGUI.LabelWidth;

            if (string.IsNullOrEmpty(tooltip1)) tooltip1 = isMinMax ? "Min Value" : "Target Value";
            if (string.IsNullOrEmpty(tooltip1)) tooltip2 = isMinMax ? "Max Value" : "Add/Subtract Amount";

            if (isMinMax) {
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);

                Vector4 min = new Vector4(
                    value.x - plusMinus.x,
                    value.y - plusMinus.y,
                    value.z - plusMinus.z,
                    value.w - plusMinus.w
                );
                AxonGUI.Label($"{label} X", GUILayout.Width(baseWidth));

                // X
                min.x = AxonGUI.FieldFloat(target, min.x, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} X Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxX = value.x + plusMinus.x;
                maxX = AxonGUI.FieldFloatInline(target, maxX, GUILayout.Width(MinMaxFieldWidth));
                value.x = (min.x + maxX) / 2f;
                plusMinus.x = (maxX - min.x) / 2f;

                // Y
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                min.y = AxonGUI.FieldFloatInline(target, $"  Y", min.y, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Y Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxY = value.y + plusMinus.y;
                maxY = AxonGUI.FieldFloatInline(target, maxY, GUILayout.Width(MinMaxFieldWidth));
                value.y = (min.y + maxY) / 2f;
                plusMinus.y = (maxY - min.y) / 2f;

                // Z
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                min.z = AxonGUI.FieldFloatInline(target, $"  Z", min.z, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Z Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxZ = value.z + plusMinus.z;
                maxZ = AxonGUI.FieldFloatInline(target, maxZ, GUILayout.Width(MinMaxFieldWidth));
                value.z = (min.z + maxZ) / 2f;
                plusMinus.z = (maxZ - min.z) / 2f;

                // W
                AxonGUI.UndoName = $"Set {label} Min";
                AxonGUI.SetTooltip(tooltip1);
                min.w = AxonGUI.FieldFloatInline(target, $"W", min.w, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} W Max";
                AxonGUI.SetTooltip(tooltip2);
                float maxW = value.w + plusMinus.w;
                maxW = AxonGUI.FieldFloatInline(target, maxW, GUILayout.Width(MinMaxFieldWidth));
                value.w = (min.w + maxW) / 2f;
                plusMinus.w = (maxW - min.w) / 2f;
            }
            else {
                AxonGUI.Label($"{label}", GUILayout.Width(baseWidth));

                // X
                AxonGUI.UndoName = $"Set {label} X";
                AxonGUI.SetTooltip(tooltip1);
                value.x = AxonGUI.FieldFloat(target, $"X", value.x, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} X +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.x = AxonGUI.FieldFloatInline(target, plusMinus.x, GUILayout.Width(MinMaxFieldWidth));

                // Y
                AxonGUI.UndoName = $"Set {label} Y";
                AxonGUI.SetTooltip(tooltip1);
                value.y = AxonGUI.FieldFloatInline(target, $"  Y", value.y, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Y +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.y = AxonGUI.FieldFloatInline(target, plusMinus.y, GUILayout.Width(MinMaxFieldWidth));

                // Z
                AxonGUI.UndoName = $"Set {label} Z";
                AxonGUI.SetTooltip(tooltip1);
                value.z = AxonGUI.FieldFloatInline(target, $"  Z", value.z, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} Z +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.z = AxonGUI.FieldFloatInline(target, plusMinus.z, GUILayout.Width(MinMaxFieldWidth));

                // W
                AxonGUI.UndoName = $"Set {label} W";
                AxonGUI.SetTooltip(tooltip1);
                value.w = AxonGUI.FieldFloatInline(target, $"W", value.w, GUILayout.Width(MinMaxFieldWidth));
                AxonGUI.SetTooltip("Toggle between +/- or min/max fields. Both modes define a value range.");
                isMinMax = AxonGUI.FieldToggleMinMax(target, isMinMax);
                AxonGUI.UndoName = $"Set {label} W +/-";
                AxonGUI.SetTooltip(tooltip2);
                plusMinus.w = AxonGUI.FieldFloatInline(target, plusMinus.w, GUILayout.Width(MinMaxFieldWidth));
            }

            EndHorizontal();
        }

        #endregion
    }
}
#endif