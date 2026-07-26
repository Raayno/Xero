// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(TimeflowGroup))]
    public class TimeflowGroupEditor : AxonGenesisEditor<TimeflowGroup, TimeflowGroupEdit> { }

    sealed public class TimeflowGroupEdit : AxonGenesisBehaviorEdit<TimeflowGroup>
    {
        public TimeflowObjectEdit TimeflowObjectUI;

        public TimeflowGroupEdit() { }

        public TimeflowGroupEdit(TimeflowGroup _target, Editor _editor)
        {
            target = _target;
            editor = _editor;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/timeflow-object";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (TimeflowObjectUI == null) TimeflowObjectUI = new TimeflowObjectEdit(target, editor);
            TimeflowObjectUI.GUISetup();
        }

        public override void GUIMenu()
        {
            TimeflowObjectUI.GUIMenu();
        }

        public override void OnInspectorGUI()
        {
            MainGUI();
        }

        public void MainGUI()
        {
            TimeflowObjectUI.MainGUI();
        }

        public void ObjectsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowObjects = AxonGUI.Foldout(target.EditorShowObjects, "Managed Objects");
            AxonGUI.EndHorizontal();

            if (target.EditorShowObjects) {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                if (target.Objects == null) target.Objects = new List<TimeflowObject>();

                for (int x = 0; x < target.Objects.Count; x++) {
                    AxonGUI.BeginHorizontalIndent();

                    EditorGUI.BeginDisabledGroup(true);
                    AxonGUI.FieldObjectInline(target, target.Objects[x], typeof(TimeflowObject), true);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();

                if (AxonGUI.ButtonInline("Regather Objects")) {
                    target.GetObjects();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            AxonGUI.EndBox();
        }

    }

}//AxonGenesis

#endif