// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;
using System.Linq;

namespace AxonGenesis
{
    public class AdvancedPresetsWindow : EditorWindow
    {
        #region STATIC  

#if TIMEFLOW_PRO
        public const string kOpenAdvancedPresets = "🎛️ Open Advanced Presets";
#else
        public const string kOpenAdvancedPresets = "Open Advanced Presets";
#endif
        [Shortcut(TimeflowShortcutInfo.Path_OpenAdvancedPresets, KeyCode.P, ShortcutModifiers.Action | ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kOpenAdvancedPresets + TimeflowMenu.Tab + TimeflowShortcutBindings.OpenAdvancedPresets, false, -90)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kOpenAdvancedPresets, false, -90)]
        public static void OpenWindow()
        {
            GetOpenWindow();
        }

        public static AdvancedPresetsWindow GetOpenWindow()
        {
            var window = CreateInstance<AdvancedPresetsWindow>();
            window.titleContent = new GUIContent("Advanced Presets");
            window.Show();
            return window;
        }

        public static AdvancedPresetsWindow SelectOrOpenWindow()
        {
            var window = Resources.FindObjectsOfTypeAll<AdvancedPresetsWindow>().FirstOrDefault();
            if (window != null) {
                window.Focus();
            }
            else {
                window = GetOpenWindow();
            }
            return window;
        }

        public static void EditPreset(AdvancedPreset preset)
        {
            if (preset == null) return;
            AdvancedPresetsWindowContext context = AdvancedPresetsWindowContext.GetContext(preset);
            if (context == null) {
                //Debug.Log($"<color=orange>Advanced Presets:</color> No context found for preset '{preset.name}' ({preset.GetType().Name}). Cannot edit preset.");
                var window = SelectOrOpenWindow();
                window.Context.EditPreset(preset);
                return;
            }
            context.EditPreset(preset);
        }

        public static bool IsMinified => Screen.width < 500;

        public static bool IsSplit => Screen.width < 100;

        public static void MinifiedRowBreak()
        {
            if (AdvancedPresetsWindow.IsMinified) {
                AxonGUI.EndHorizontal();
                AxonGUI.BeginHorizontal();
            }
        }

        public static void Refresh() => Refresh(true);

        public static void Refresh(bool exitGUI)
        {
            if(exitGUI) EditorGUIUtility.ExitGUI();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var windows = Resources.FindObjectsOfTypeAll<AdvancedPresetsWindow>();
            foreach (var window in windows) {
                if (window != null) {
                    window.OnRefresh();
                }
            }
        }

        #endregion

        private AdvancedPresetsWindowContext _Context;

        public AdvancedPresetsWindowContext Context {
            get {
                if(_Context == null) {
                    _Context = new AdvancedPresetsWindowContext(this);
                }
                return _Context;
            }
        }

        private PropertyInfo cachedTitleContent;

        public void OnEnable()
        {
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
            Undo.undoRedoPerformed += OnUndoPerformed;

            OnRefresh();
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoPerformed;
        }

        public void OnRefresh()
        {
            if(_Context == null) _Context = new AdvancedPresetsWindowContext(this);
            _Context.IsPopupMenu = false;
            Load();
            Repaint();
        }

        public void Load()
        {
            minSize = new Vector2(300.0f, 100.0f);

            Context.Load();
        }

        public void OnGUI()
        {
            minSize = new Vector2(200.0f, 100.0f);
            AxonGUI.Setup(70);

            Context.MainGUI();
        }

        public void OnUndoPerformed()
        {
            //Debug.Log("Undo operation performed.");
            OnRefresh();
        }
    }

}//AxonGenesis

#endif