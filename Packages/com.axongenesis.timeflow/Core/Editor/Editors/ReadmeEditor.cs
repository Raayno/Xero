// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(Readme))]
    [InitializeOnLoad]
    public class ReadmeEditor : Editor
    {
        private const float _space = 16f;

        static ReadmeEditor() { }

        private bool _initialized;

        [SerializeField] GUIStyle m_LinkStyle;
        [SerializeField] GUIStyle m_TitleStyle;
        [SerializeField] GUIStyle m_HeadingStyle;
        [SerializeField] GUIStyle m_BodyStyle;

        GUIStyle LinkStyle { get { return m_LinkStyle; } }

        GUIStyle TitleStyle { get { return m_TitleStyle; } }

        GUIStyle HeadingStyle { get { return m_HeadingStyle; } }

        GUIStyle BodyStyle { get { return m_BodyStyle; } }

        void Init()
        {
            if (_initialized) return;
            m_BodyStyle = new GUIStyle(EditorStyles.label);
            m_BodyStyle.wordWrap = true;
            m_BodyStyle.fontSize = 14;

            m_TitleStyle = new GUIStyle(m_BodyStyle);
            m_TitleStyle.fontSize = 26;

            m_HeadingStyle = new GUIStyle(m_BodyStyle);
            m_HeadingStyle.fontSize = 18;

            m_LinkStyle = new GUIStyle(m_BodyStyle);
            m_LinkStyle.wordWrap = false;
            // Match selection color which works nicely for both light and dark skins
            m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            m_LinkStyle.stretchWidth = false;

            _initialized = true;
        }

        bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, LinkStyle);
        }

        protected override void OnHeaderGUI()
        {
            var readme = (Readme)target;
            Init();

            var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

            GUILayout.BeginHorizontal("In BigTitle");
            {
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
                GUILayout.Label(readme.title, TitleStyle);
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            var readme = (Readme)target;
            Init();

            if (readme.sections != null && readme.sections.Length > 0) {

                foreach (var section in readme.sections) {
                    if (!string.IsNullOrEmpty(section.heading)) {
                        if (readme.isEditing) {
                            section.heading = EditorGUILayout.TextField(section.heading, HeadingStyle);
                        }
                        else {
                            GUILayout.Label(section.heading, HeadingStyle);
                        }
                    }
                    if (!string.IsNullOrEmpty(section.text)) {
                        if (readme.isEditing) {
                            section.text = EditorGUILayout.TextArea(section.text, BodyStyle);
                        }
                        else {
                            GUILayout.Label(section.text, BodyStyle);
                        }
                    }
                    if (!string.IsNullOrEmpty(section.linkText)) {
                        if (readme.isEditing) {
                            section.url = EditorGUILayout.TextField(section.url, LinkStyle);
                        }
                        else {
                            if (LinkLabel(new GUIContent(section.linkText))) {
                                Application.OpenURL(section.url);
                            }
                        }
                    }
                    GUILayout.Space(_space);
                }
            }

            if (AxonGUI.Button(readme.isEditing ? "Done Editing" : "Edit")) {
                readme.isEditing = !readme.isEditing;
            }

        }
    }

}//AxonGenesis

#endif