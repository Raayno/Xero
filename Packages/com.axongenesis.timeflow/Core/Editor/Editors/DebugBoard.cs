// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AxonGenesis
{
    public class DebugBoard : EditorWindow
    {
        private class DebugEntry
        {
            public string name;
            public object value;
            public Type type;
            public UnityEngine.Object context;
            public Color color = Color.white;
        }

        private static Dictionary<string, DebugEntry> entries = new Dictionary<string, DebugEntry>();

        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + TimeflowMenu.kEditorDebugBoard, false, 10041)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + TimeflowMenu.kEditorDebugBoard, false, 10041)]
        public static void ShowWindow()
        {
            GetWindow<DebugBoard>("Debug Board");
        }

        private void OnGUI()
        {
            AxonGUI.Setup(140);
            AxonGUI.BeginBoxPadded();

            foreach (var kvp in entries) {
                var entry = kvp.Value;

                GUI.color = entry.color;
                AxonGUI.BeginHorizontal(AxonUI.DarkBoxStyle);
                GUI.color = Color.white;

                // Highlight field background
               // var prevColor = GUI.backgroundColor;
                //GUI.backgroundColor = entry.color;

                int width = 320;
                if (entry.type == typeof(bool)) {
                    entry.value = AxonGUI.FieldToggle(entry.context, entry.name, (bool)entry.value, GUILayout.Width(width));
                }
                else if (entry.type == typeof(int)) {
                    entry.value = AxonGUI.FieldInt(entry.context, entry.name, (int)entry.value, GUILayout.Width(width));
                }
                else if (entry.type == typeof(float)) {
                    entry.value = AxonGUI.FieldFloat(entry.context, entry.name, (float)entry.value, GUILayout.Width(width));
                }
                else if (entry.type == typeof(string)) {
                    entry.value = AxonGUI.FieldText(entry.context, entry.name, (string)entry.value, GUILayout.Width(width));
                }
                else {
                    AxonGUI.Label($"Unsupported: {entry.type.Name}");
                }

                // Object reference
                entry.context = AxonGUI.FieldObjectInline(entry.context, entry.context, typeof(UnityEngine.Object), true);

                // Color field
                //entry.color = AxonGUI.FieldColorInline(entry.context, entry.color, false);

                AxonGUI.EndHorizontal();
            }

            AxonGUI.Space();
            AxonGUI.BeginHorizontal();
            if (GUILayout.Button("Clear All")) {
                entries.Clear();
            }
            AxonGUI.EndHorizontal();

            AxonGUI.EndBoxPadded();

            Repaint(); // keep updating
        }

        // === Public static registration methods ===

        public static void Register(string name, bool value, UnityEngine.Object context)
        {
            AddOrUpdate(name, value, typeof(bool), context);
        }

        public static void Register(string name, int value, UnityEngine.Object context)
        {
            AddOrUpdate(name, value, typeof(int), context);
        }

        public static void Register(string name, float value, UnityEngine.Object context)
        {
            AddOrUpdate(name, value, typeof(float), context);
        }

        public static void Register(string name, string value, UnityEngine.Object context)
        {
            AddOrUpdate(name, value, typeof(string), context);
        }

        private static void AddOrUpdate(string name, object value, Type type, UnityEngine.Object context)
        {
            if (entries.TryGetValue(name, out var entry)) {
                entry.value = value;
                entry.type = type;
                entry.context = context;
            }
            else {
                entries[name] = new DebugEntry {
                    name = name,
                    value = value,
                    type = type,
                    context = context
                };
            }
        }

        // === Reset on play mode enter ===
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) {
                entries.Clear();
            }
        }
    }

}
#endif