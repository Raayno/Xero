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
    /// <summary>
    /// This provides many of the same features as EditorGUILayout but with specializations added for
    /// creating more streamlined inspector GUIs and working with custom types such as properties. Most GUI
    /// features also have an inline version, which allows multiple GUI controls to be combined on the same
    /// line. These feature generally do not support multi-editing which is only implemented using
    /// serialized properties for select behavior components.
    /// </summary>
    public partial class AxonGUI
    {
        delegate void PropertyMenuSelect(object obj);

        #region PUBLIC

        public static float _LabelWidth = 100f;
        public static float LabelWidth {
            get {
                return _LabelWidth;
            }
            private set {
                _LabelWidth = value;
            }
        }

        private static List<Property> warnings = new List<Property>();

        public static float TinyLabelWidth = 10f;
        public static float MaxWidth = 500f;
        public static bool UseMaxWidth = true;
        public static float FloatWidth = 60f;

        public static bool IsRowHorizontal;
        public static Color DeselectedColor = new Color(0.5f, 0.5f, 0.5f);
        public static Color SelectedColor = new Color(1f, 1f, 1f);

        public static bool ShowPropertyObjectField = false;

        private static bool _UndoEnabled = true;
        private static string _UndoName = null;

        #endregion

        #region PRIVATE

        private static int rowCount;

        private static bool indentCleared;
        private static int tempIndent;

        private static Property copiedProperty;
        private static List<string> layers;
        private static string[] layerNames;

        private static string tooltip;

        #endregion

        #region ACCESSORS

        public static string UndoName {
            get {
                return _UndoName;
            }
            set {
                _UndoName = value;
                _UndoEnabled = !string.IsNullOrEmpty(value);
            }
        }

        public static int RowCount {
            get {
                return rowCount;
            }
            set {
                rowCount = value;
            }
        }

        #endregion

        #region SETUP

        public static T InstantiateUI<T>(Object obj, Editor editor) where T : class
        {
            T ui = null;
            Type shapeClass = obj.GetType();
            string className = shapeClass.Namespace + "." + shapeClass.Name + "SharedUI";
            Type uiType = Type.GetType(className + ", Assembly-CSharp-Editor");
            if (uiType != null) {
                ui = (T)Activator.CreateInstance(uiType, obj, editor);
            }
            else {
                AxonGUI.HelpBox("No inspector UI has been defined for " + className, MessageType.Warning);
            }

            return ui;
        }

        public static void Setup()
        { Setup(140f, EditorGUIUtility.currentViewWidth); }

        public static void Setup(float labelWidth) { Setup(labelWidth, Screen.width); }

        public static void Setup(float labelWidth, float maxWidth)
        {
            Indent = 0;
            LabelWidth = labelWidth;
            EditorGUIUtility.labelWidth = LabelWidth;
            MaxWidth = maxWidth - 50f;
            EditorGUIUtility.wideMode = true;
            bool force = AxonUI.SolidStyle == null || AxonUI.SolidStyle.normal.background == null;
            AxonUI.Load(force);
        }

        #endregion

        #region UNDO

        private static void _Undo(Object obj, string name, string value)
        {
            if (obj == null) return;
            if (!string.IsNullOrEmpty(UndoName)) name = UndoName;
            else {
                string typeName = obj == null ? "" : obj.GetType().Name;
                UndoName = $"{typeName}: {name} = {value}";
            }
            if (!_UndoEnabled) return;
            Undo.FlushUndoRecordObjects();
            UndoUtil.Undo(obj, UndoName, true);

            _UndoEnabled = false; // Clear until UndoName is set again
        }

        #endregion

        #region UTILITIES

        public static int Indent {
            get {
                return EditorGUI.indentLevel;
            }
            set {
                EditorGUI.indentLevel = value;
            }
        }

        public static void ClearIndent()
        {
            if (!indentCleared) {
                indentCleared = true;
                tempIndent = EditorGUI.indentLevel;
            }
            EditorGUI.indentLevel = 0;
        }

        public static void RestoreIndent()
        {
            indentCleared = false;
            EditorGUI.indentLevel = tempIndent;
        }

        public static void ResetLabelWidth()
        {
            EditorGUIUtility.labelWidth = LabelWidth;
        }

        public static float SetLabelWidth(float width)
        {
            float lastWidth = LabelWidth;
            LabelWidth = width;
            EditorGUIUtility.labelWidth = width;
            return lastWidth;
        }

        public static void RestoreLabelWidth()
        {
            EditorGUIUtility.labelWidth = LabelWidth;
        }

        public static float CalculateWidth(string label)
        {
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(label));
            return size.x;
        }

        public static float CalculateWidth(string label, GUIStyle style)
        {
            Vector2 size = style.CalcSize(new GUIContent(label));
            return size.x;
        }

        public static void Error(string message)
        {
            ClearIndent();
            if (ButtonTexture(AxonUI.Icons.Error, message)) {
                EditorUtility.DisplayDialog("Error", message, "Ok", "");
            }
            RestoreIndent();
        }

        public static void Info(string message)
        {
            ClearIndent();
            if (ButtonTexture(AxonUI.Icons.Info, message, new RectOffset(0, 0, 2, 0))) {
                if (!string.IsNullOrEmpty(message)) EditorUtility.DisplayDialog("Info", message, "Ok", "");
            }
            RestoreIndent();
        }

        public static bool InfoDialog(string title, string message, string ok, string cancel)
        {
            bool result = false;
            ClearIndent();
            if (ButtonTexture(AxonUI.Icons.Info, message, new RectOffset(0, 0, 3, 0))) {
                result = EditorUtility.DisplayDialog(title, message, ok, cancel);
            }
            RestoreIndent();
            return result;
        }

        public static int InfoComplex(string title, string message, string ok, string cancel, string alt)
        {
            int result = 0;
            ClearIndent();
            if (ButtonTexture(AxonUI.Icons.Info, message, new RectOffset(0, 0, 3, 0))) {
                result = EditorUtility.DisplayDialogComplex(title, message, ok, cancel, alt);
            }
            RestoreIndent();
            return result;
        }

        public static void Warning(string message)
        {
            ClearIndent();
            if (ButtonTexture(AxonUI.Icons.Warning, message, new RectOffset(0, 0, 2, 0))) {
                EditorUtility.DisplayDialog("Warning", message, "Ok", "");
            }
            RestoreIndent();
        }

        public static void Heading(string heading)
        {
            EditorGUILayout.LabelField(heading, EditorStyles.boldLabel);
        }

        public static void SetTooltip(string tooltip)
        {
            AxonGUI.tooltip = tooltip;
        }

        public static GUIContent GetLabelWithTooltip(string name)
        {
            if (string.IsNullOrEmpty(name)) return GUIContent.none;
            GUIContent content = new GUIContent(name, tooltip);
            tooltip = ""; // Clear for next control
            return content;
        }

        public static string GetLabel(string label)
        {
            return label;
        }

        public static void HelpBox(string message, MessageType type = MessageType.Info, bool wide = true)
        {
            EditorGUILayout.HelpBox(message, type, wide);
            RowCount += 3;
        }

        public static GUILayoutOption[] AddOptions(GUILayoutOption[] options, params GUILayoutOption[] moreOptions)
        {
            return AddOptions(options, false, moreOptions);
        }

        public static GUILayoutOption[] AddOptions(GUILayoutOption[] options, bool useMax, params GUILayoutOption[] moreOptions)
        {
            int originalCount = options != null ? options.Length : 0;
            int newCount = originalCount + moreOptions.Length;

            if (!UseMaxWidth) useMax = false;
            if (useMax) newCount++;

            GUILayoutOption[] newOptions = new GUILayoutOption[newCount];

            if (options != null) {
                for (int i = 0; i < originalCount; i++) {
                    newOptions[i] = options[i];
                }
            }
            int end = newCount;
            if (useMax) end -= 1;
            for (int i = originalCount; i < end; i++) {
                newOptions[i] = moreOptions[i - originalCount];
            }

            if (useMax) newOptions[end] = GUILayout.MaxWidth(MaxWidth);

            return newOptions;
        }

        public static bool KeyValueList(bool foldout, string name, ref List<string> keys, ref List<string> values)
        {
            foldout = AxonGUI.Foldout(foldout, name);
            if (foldout) {
                if (values == null) {
                    values = new List<string>();
                }
                if (keys == null) {
                    keys = new List<string>();
                }
                for (int i = 0; i < values.Count; i++) {
                    BeginHorizontal();
                    EditorGUILayout.LabelField("", "", GUILayout.Width(10));
                    EditorGUILayout.LabelField("Item:" + i, "", GUILayout.Width(50));
                    keys[i] = EditorGUILayout.TextField(keys[i]);
                    values[i] = EditorGUILayout.TextField(values[i]);
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Insert Value")) {
                        keys.Insert(i, "");
                        values.Insert(i, "");
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Value")) {
                        keys.RemoveAt(i);
                        values.RemoveAt(i);
                    }
                    EndHorizontal();
                }

                EditorGUILayout.Space();
                BeginHorizontal();
                EditorGUILayout.LabelField("", "", GUILayout.Width(10));
                if (GUILayout.Button("Add Item", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                    keys.Add("");
                    values.Add("");
                }
                if (GUILayout.Button("Clear All", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                    keys = new List<string>();
                    values = new List<string>();
                }
                EndHorizontal();
            }
            return foldout;
        }

        #endregion

        #region LAYOUT

        public static void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }

        public static bool EndChangeCheck()
        {
            return EditorGUI.EndChangeCheck();
        }

        public static void BeginDisabledGroup(bool disabled)
        {
            EditorGUI.BeginDisabledGroup(disabled);
        }

        public static void EndDisabledGroup()
        {
            EditorGUI.EndDisabledGroup();
        }

        public static void BeginVertical(params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            EditorGUILayout.BeginVertical(AddOptions(options, true));
            RestoreLabelWidth();
        }

        public static void BeginVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            EditorGUILayout.BeginVertical(style, AddOptions(options, true));
            RestoreLabelWidth();
        }

        public static void EndVertical()
        {
            EditorGUILayout.EndVertical();
        }

        public static void BeginBox(params GUILayoutOption[] options)
        {
            BeginVertical("box", options);
        }

        public static void EndBox()
        {
            EndVertical();
        }

        public static void BeginBoxPadded(params GUILayoutOption[] options)
        {
            BeginVertical("box", options);
            Indent++;
        }

        public static void EndBoxPadded()
        {
            Space();
            Indent--;
            EndVertical();
        }

        public static void BeginHorizontalBox()
        {
            IsRowHorizontal = true;
            EditorGUILayout.BeginHorizontal("box");
        }

        public static void BeginHorizontal(params GUILayoutOption[] options)
        {
            IsRowHorizontal = true;
            ResetLabelWidth();
            EditorGUILayout.BeginHorizontal(AddOptions(options, true, GUILayout.Height(22)));
            RestoreLabelWidth();
        }

        public static void BeginHorizontalHeight(params GUILayoutOption[] options)
        {
            IsRowHorizontal = true;
            ResetLabelWidth();
            EditorGUILayout.BeginHorizontal(AddOptions(options, true));
            RestoreLabelWidth();
        }

        public static void BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            IsRowHorizontal = true;
            ResetLabelWidth();
            EditorGUILayout.BeginHorizontal(style, AddOptions(options, true));
            RestoreLabelWidth();
        }

        public static void BeginHorizontalIndent(params GUILayoutOption[] options)
        {
            IsRowHorizontal = true;
            ResetLabelWidth();
            EditorGUILayout.BeginHorizontal(AddOptions(options, true));
            EditorGUILayout.LabelField(" ", "", GUILayout.Width(EditorGUI.indentLevel * 16));
            RestoreLabelWidth();
        }

        public static void BeginHorizontalIndent(GUIStyle style, params GUILayoutOption[] options)
        {
            ResetLabelWidth();
            EditorGUILayout.BeginHorizontal(style, AddOptions(options, true));
            EditorGUILayout.LabelField(" ", "");
            RestoreLabelWidth();
        }

        public static void EndHorizontal()
        {
            EndHorizontal(false);
        }

        public static void EndHorizontal(bool endSpace)
        {
            IsRowHorizontal = false;
            if (endSpace) EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            RowCount++;
        }

        public static void VerticalSpace(float space)
        {
            EditorGUILayout.Space();
            if (!IsRowHorizontal) RowCount++;
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        public static void Space()
        {
            EditorGUILayout.Space();
            if (!IsRowHorizontal) RowCount++;
        }

        public static void Space(float width, float height = 12)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height, GUILayout.Width(width), GUILayout.Height(12));
            rect.x += (EditorGUI.indentLevel * 14);
            rect.width = width;
            rect.height = height;

            GUIContent content = new GUIContent("", tooltip);
            GUI.Label(rect, content);
            //InlineLabelField("", "", GUILayout.Width(width));
        }

        public static bool Foldout(bool value, string label, params GUILayoutOption[] options)
        {
            ResetLabelWidth();

            float width = 16;
            float labelWidth = CalculateWidth(label);
            if (!string.IsNullOrEmpty(label) && label != "") width = labelWidth + 50;
            BeginHorizontal(GUILayout.Height(20), GUILayout.Width(EditorGUIUtility.labelWidth));

            Rect rect = EditorGUILayout.GetControlRect(false, AxonUI.Icons.FoldoutDown.width, GUILayout.Width(AxonUI.Icons.FoldoutDown.width), GUILayout.Height(AxonUI.Icons.FoldoutDown.height));
            rect.x += (EditorGUI.indentLevel * 14) - 2f;
            rect.y -= 2f;

            GUIStyle style = AxonUI.TextureButtonStyle;
            if (value) {
                style.normal.background = AxonUI.Icons.FoldoutDown;
                style.active.background = AxonUI.Icons.FoldoutUp;
            }
            else {
                style.normal.background = AxonUI.Icons.FoldoutUp;
                style.active.background = AxonUI.Icons.FoldoutDown;
            }

            if (GUI.Button(rect, GUIContent.none, style)) {
                value = !value;
            }
            if (!string.IsNullOrEmpty(label) && label != "") {
                EditorGUILayout.LabelField(label, AxonUI.FoldoutStyle, AddOptions(options, GUILayout.Width(labelWidth)));
            }
            EndHorizontal();
            RestoreLabelWidth();
            return value;
        }

        public static bool FoldoutInline(bool value) { return FoldoutInline(value, null); }

        public static bool FoldoutInline(bool value, string tooltip) { return FoldoutInline(value, tooltip, new RectOffset(0, 0, 0, 0)); }

        public static bool FoldoutInline(bool value, string tooltip, RectOffset margin)
        {
            if (ButtonTexture(value ? AxonUI.Icons.FoldoutDown : AxonUI.Icons.FoldoutUp, !value ? AxonUI.Icons.FoldoutDown : AxonUI.Icons.FoldoutUp, tooltip, margin, true)) {
                value = !value;
            }
            return value;
        }

        #endregion

        #region LABELS

        public static void Label(string label, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            EditorGUILayout.LabelField(GetLabelWithTooltip(label), AddOptions(options));

            RestoreLabelWidth();
        }

        public static void Label(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            EditorGUILayout.LabelField(GetLabelWithTooltip(label), style, AddOptions(options));

            RestoreLabelWidth();
        }

        public static void Label(string label, string label2, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            EditorGUILayout.LabelField(GetLabel(label), label2, AddOptions(options));

            RestoreLabelWidth();
        }

        public static void LabelInline(string label) { LabelInline(label, "", null); }

        public static void LabelInline(string label, string label2, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);
            EditorGUILayout.LabelField(GetLabel(label), label2, AddOptions(options, false, GUILayout.Width(EditorGUIUtility.labelWidth + CalculateWidth(label2))));

            RestoreIndent();
            RestoreLabelWidth();
        }

        public static void LabelInline(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label, style);
            EditorGUILayout.LabelField(GetLabel(label), style, AddOptions(options, false, GUILayout.Width(EditorGUIUtility.labelWidth)));

            RestoreIndent();
            RestoreLabelWidth();
        }

        public static void LabelInlineFieldFixedWidth(string label, string label2, params GUILayoutOption[] options)
        {
            ClearIndent();
            ResetLabelWidth();

            EditorGUIUtility.labelWidth = CalculateWidth(label);
            EditorGUILayout.LabelField(GetLabel(label), label2, options);

            RestoreIndent();
            RestoreLabelWidth();
        }

        #endregion

        public static void FocusControl(string name)
        {
            //Debug.Log($"<color=yellow>FocusControl:</color>{name}");
            GUI.FocusControl(name);
        }

    }

}//AxonGenesis
#endif