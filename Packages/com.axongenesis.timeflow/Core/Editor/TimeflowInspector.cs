// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This creates the Timeflow Behaviors window which shows AxonGenesisBehavior components based on
    /// selected objects and channels in the Timeflow view. This is similar to the insepctor window however
    /// only shows AxonGenesisBehavior objects.
    /// </summary>
    public class TimeflowInspector : EditorWindow
    {
        #region STATIC
        public static TimeflowInspector Instance { get; private set; }

        private static PropertyInfo cachedTitleContent;

        [UnityEditor.MenuItem("Window/Timeflow/Timeflow Inspector", false, 801)]
        public static void Init()
        {
            if (Instance != null) {
                Instance.Show();
            }
            else {
                Instance = EditorWindow.GetWindow(typeof(TimeflowInspector), false, "Timeflow Inspector") as TimeflowInspector;
                if (Instance == null) {
                    Debug.LogError("Failed loading Timeflow Behaviors window");
                }
                else {
                    Instance.minSize = new Vector2(100.0f, 50.0f);
                }
            }
            Instance.autoRepaintOnSceneChange = true;
        }

        public static bool IsVisible {
            get {
                return Instance != null;
            }
        }

        public static bool IsShowing(AxonGenesisBehavior target)
        {
            if (Instance != null) {
                if (Instance.Editors != null && Instance.Editors.Count > 0) {
                    foreach (Editor edit in Instance.Editors) {
                        if (edit.target == target) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void Refresh()
        {
            if (Instance != null) Instance.doRefresh();
        }

        #endregion

        private Vector2 scrollPos = Vector2.zero;
        private List<Editor> Editors;

        public void OnDestroy() { }

        public void OnEnable()
        {
            if (Instance == null) Instance = this;
            wantsMouseMove = true;
            if (cachedTitleContent == null) {
                cachedTitleContent = base.GetType().GetProperty("cachedTitleContent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            }
            if (cachedTitleContent != null) {
                GUIContent content = cachedTitleContent.GetValue(this, null) as GUIContent;
                if (content != null) {
                    content.image = AxonUI.Icons.Grouped;
                }
            }
            EditorApplication.hierarchyChanged -= OnSceneChange;
            EditorApplication.hierarchyChanged += OnSceneChange;
            EditorApplication.projectChanged -= OnSceneChange;
            EditorApplication.projectChanged += OnSceneChange;

            Refresh();
        }

        void OnSelectionChange()
        {
            Refresh();
        }

        public void OnSceneChange()
        {
            Refresh();
        }

        public void doRefresh()
        {
            LoadEditors();
            Repaint();
        }

        public void OnGUI()
        {
            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            box.alignment = TextAnchor.MiddleCenter;

            Rect winRect = new Rect(0, 0, position.width, position.height);

            if (TimeflowInspector.Instance == null) {
                GUI.Box(winRect, new GUIContent("Please open the Actions window to use this inspector."), box);
            }
            else
            if (Editors == null || Editors.Count == 0) {
                GUI.Box(winRect, new GUIContent("No items selected."), box);
            }
            else {
                DisplayEditors();
            }
        }

        private void DisplayEditors()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            AxonGenesisBaseEdit.IsTimeflowInspector = true;

            for (int i = 0; i < Editors.Count; i++) {
                Editor edit = Editors[i];
                if (edit != null) {
                    bool canDisplay = true;
                    if (Timeflow.Active != null) {
                        if (Timeflow.Active.View.Info.AnySelectedChannels) {
                            canDisplay = false;
                            if (typeof(TimeflowBehavior).IsAssignableFrom(edit.target.GetType())) {
                                TimeflowBehavior behavior = (TimeflowBehavior)edit.target;
                                if (behavior != null && behavior.Channels != null && behavior.Channels.Count > 0) {
                                    foreach (TimeflowChannel channel in behavior.Channels) {
                                        if (channel != null && channel.IsSelected) {
                                            canDisplay = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (canDisplay) {
                        Editor.DrawFoldoutInspector(edit.target, ref edit);
                    }
                }
            }
            AxonGenesisBaseEdit.IsTimeflowInspector = false;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        public void ReleaseEditors()
        {
            if (Editors != null) {
                Editors = null;
            }
        }

        public void LoadEditors()
        {
            ReleaseEditors();

            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;

            Editors = new List<Editor>();
            foreach (GameObject obj in Selection.gameObjects) {
                GetEditors(obj);
            }
        }

        private void GetEditors(GameObject obj)
        {
            AxonGenesisBehavior[] behaviors = obj.GetComponents<AxonGenesisBehavior>();
            if (behaviors != null && behaviors.Length > 0) {
                foreach (AxonGenesisBehavior behavior in behaviors) {
                    string uiName = behavior.GetType() + "UI";
                    Type editorType = Type.GetType(behavior.GetType() + "Editor, AxonGenesis");
                    if (editorType != null) {
                        Editor editor = Editor.CreateEditor(behavior, editorType);
                        if (editor != null) {
                            Editors.Add(editor);
                        }
                        else {
                            Debug.LogWarning("Failed to create editor for behavior:" + behavior.Name + " editor:" + editorType);
                        }
                    }
                }
            }
        }
    }

}//AxonGenesis

#endif