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
    [CustomEditor(typeof(Comment))]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/comment")]
    public class CommentEditor : Editor
    {
        private bool isInit;
        private bool isEditing;
        private GUIStyle textStyle;
        private GUIStyle titleStyle;
        private GUIStyle urlStyle;
        private GUIStyle warningStyle;


        private Comment comment => (Comment)target;

        private void Init()
        {
            AxonGUI.Setup();

            if (!isInit) {
                isInit = true;

                textStyle = new GUIStyle(GUI.skin.label);
                textStyle.alignment = TextAnchor.UpperLeft;
                textStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                textStyle.fontSize = 16;
                textStyle.fontStyle = FontStyle.Normal;
                textStyle.padding = new RectOffset(10, 10, 10, 10);
                textStyle.wordWrap = true;

                warningStyle = new GUIStyle(textStyle);
                warningStyle.fontStyle = FontStyle.Italic;

                titleStyle = new GUIStyle(textStyle);
                titleStyle.wordWrap = false;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 20;

                urlStyle = new GUIStyle(textStyle);
                urlStyle.wordWrap = false;
                urlStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
                urlStyle.stretchWidth = false;
            }
        }

        public override void OnInspectorGUI()
        {
            Init();

            AxonGUI.BeginHorizontalBox();
            if (AxonGUI.ButtonTexture(isEditing ? AxonUI.Icons.EditOn : AxonUI.Icons.EditOff, "Edit the channel link")) {
                isEditing = !isEditing;
            }
            AxonGUI.EndHorizontal();

            AxonGUI.SetLabelWidth(100f);
            AxonGUI.BeginBoxPadded(GUILayout.MaxWidth(550));
            if (isEditing) {
                AxonGUI.BeginBox();
                comment.Comments = EditorGUILayout.TextArea(comment.Comments, textStyle);
                AxonGUI.EndBox();

                AxonGUI.Space();
                //target.Warning = EditorGUILayout.TextField("Warning", target.Warning);
                EditorGUILayout.LabelField("Warning", "");
                AxonGUI.BeginBox();
                comment.Warning = EditorGUILayout.TextArea(comment.Warning, textStyle);
                AxonGUI.EndBox();

                AxonGUI.Space();
                comment.URLTitle = EditorGUILayout.TextField("Title", comment.URLTitle);
                comment.URL = EditorGUILayout.TextField("URL", comment.URL);

                AxonGUI.Space();
                comment.URL2Title = EditorGUILayout.TextField("Title", comment.URL2Title);
                comment.URL2 = EditorGUILayout.TextField("URL", comment.URL2);

                AxonGUI.Space();
                if (AxonGUI.Button("Done Editing")) {
                    isEditing = false;
                }
            }
            else {
                if (!string.IsNullOrEmpty(comment.Comments)) {
                    GUILayout.Label(comment.Comments, textStyle);
                }
                if (!string.IsNullOrEmpty(comment.Warning)) {
                    GUI.color = AxonColor.Warning;
                    AxonGUI.BeginHorizontalBox();
                    GUI.color = Color.white;
                    AxonGUI.ButtonTexture(AxonUI.Icons.Warning, comment.Warning);
                    GUILayout.Label(comment.Warning, warningStyle);
                    AxonGUI.EndHorizontal();
                }
                if (!string.IsNullOrEmpty(comment.URL)) {
                    if (!string.IsNullOrEmpty(comment.URLTitle)) {
                        GUILayout.Label(comment.URLTitle, titleStyle);
                    }
                    if (LinkLabel(new GUIContent(comment.URL), GUILayout.ExpandWidth(true))) {
                        Application.OpenURL(comment.URL);
                    }
                }

                if (!string.IsNullOrEmpty(comment.URL2)) {
                    if (!string.IsNullOrEmpty(comment.URL2Title)) {
                        GUILayout.Label(comment.URL2Title, titleStyle);
                    }
                    if (LinkLabel(new GUIContent(comment.URL2), GUILayout.ExpandWidth(true))) {
                        Application.OpenURL(comment.URL2);
                    }
                }
            }
            AxonGUI.EndBoxPadded();
            AxonGUI.ResetLabelWidth();

            if (GUI.changed) EditorUtility.SetDirty(comment);
        }

        bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, urlStyle, options);

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, urlStyle);
        }
    }

}//AxonGenesis 

#endif