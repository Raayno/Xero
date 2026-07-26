// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    public class ShortcutInfo
    {
        public string Binding { get; set; }
        public string Path { get; set; }
    }

    public static class TimeflowShortcuts
    {
        public static bool DebugEnabled = false;

        private static List<ShortcutInfo> _AllShortcuts = null;

        public static List<ShortcutInfo> AllShortcuts {
            get {
                if (_AllShortcuts == null) _AllShortcuts = GetShortcuts();
                return _AllShortcuts;
            }
        }

        public static string GetShortcut(string match, bool isMenu = false)
        {
            string binding = string.Empty;
            if (string.IsNullOrEmpty(match)) return binding;

            foreach (var shortcut in AllShortcuts) {
                string path = shortcut.Path;
                if (path.Contains(match, StringComparison.OrdinalIgnoreCase)) {
                    binding = shortcut.Binding;
                    break;
                }
            }
            if (isMenu) binding = MenuString(binding);
            return binding;
        }

        public static void UpdateShortcutBindings(string assetGuid = "b890e38bd64861d4ba622654f087521c")
        {
            /*
             * Disabled since it cannot output the correct shortcut name yet. There needs to be a way to
             * add the shortcut name to ShortcutInfo (using some sort of reverse lookup to the const string names).
             * For now, the shortcuts displayed in menus are fixed to the original mappings.
            StringBuilder script = new StringBuilder("namespace AxonGenesis\n{\n");
            script.AppendLine("\tpublic static class TimeflowShortcutBindings\n{\n");

            int i = 1;
            foreach (var shortcut in AllShortcuts) {
                script.AppendLine($"\t\tpublic const string shortcut_{i} = \"{MenuString(shortcut.Binding)}\";//{shortcut.Path}\n");
                i++;
            }
            script.AppendLine("\t}\n");
            script.AppendLine("}\n");

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath)) {
                Debug.LogError($"Invalid GUID: {assetGuid}. Could not find asset.");
                return;
            }

            try {
                File.WriteAllText(assetPath, script.ToString());
                Debug.Log($"Shortcut bindings written to: {assetPath}");
            }
            catch (Exception ex) {
                Debug.LogError($"Failed to write to file: {assetPath}. Exception: {ex.Message}");
            }
            */
        }

        private static ShortcutModifiers GetModifiersForShortcut(string shortcutName)
        {
            if (string.IsNullOrEmpty(shortcutName)) return ShortcutModifiers.None;
            ShortcutBinding binding = ShortcutManager.instance.GetShortcutBinding(shortcutName);

            ShortcutModifiers modifiers = ShortcutModifiers.None;
            foreach (var key in binding.keyCombinationSequence) {
                modifiers = key.modifiers;
                break;
            }
            return modifiers;
        }

        public static bool IsModifierPressed(string command)
        {
            bool pressed = IsModifierPressed(GetModifiersForShortcut(command));
            return pressed;
        }

        public static bool IsModifierPressed(ShortcutModifiers modifiers)
        {
            if (modifiers == ShortcutModifiers.None)
                return true;

            if (Event.current == null)
                return false;

            if ((modifiers & ShortcutModifiers.Control) != 0 && !Event.current.control)
                return false;

            if ((modifiers & ShortcutModifiers.Alt) != 0 && !Event.current.alt)
                return false;

            if ((modifiers & ShortcutModifiers.Shift) != 0 && !Event.current.shift)
                return false;

            // Check if Action modifier is part of the flag and if it is currently pressed
            if ((modifiers & ShortcutModifiers.Action) != 0) {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
                if (!Event.current.control)
                    return false;
#else
                if (!Event.current.command) 
                    return false;
#endif
            }

            return true; // All required modifiers are pressed
        }

        public static List<ShortcutInfo> GetShortcuts(bool forDisplay = false)
        {
            var ids = ShortcutManager.instance.GetAvailableShortcutIds();

            List<ShortcutInfo> shortcuts = new List<ShortcutInfo>();
            if (!forDisplay) {
                shortcuts.Add(new ShortcutInfo { Binding = $"// Timeflow Shortcuts v{Timeflow.Version}", Path = string.Empty });
            }
            foreach (var id in ids) {
                if (!id.StartsWith("Timeflow")) continue;
                var shortcut = ShortcutManager.instance.GetShortcutBinding(id);
                string key = DecodeString(ShortcutToString(shortcut), forDisplay);
                if (string.IsNullOrEmpty(key)) continue;
                shortcuts.Add(new ShortcutInfo { Path = id, Binding = key });
            }

            shortcuts.Sort((a, b) => string.Compare(a.Binding, b.Binding, StringComparison.Ordinal));
            return shortcuts;
        }

#if TIMEFLOW_PRO
        public const string kExportShortcuts = "⚙️ Editor/📤 Export Shortcuts";
        public const string kImportShortcuts = "⚙️ Editor/📥 Import Shortcuts";
        public const string kResetShortcuts = "⚙️ Editor/🔄 Reset Shortcuts to Default";
        public const string kOpenShortcutsManager = "⚙️ Editor/🎹 Open Shortcuts Manager";
#else
        public const string kExportShortcuts = "Editor/Export Shortcuts";
        public const string kImportShortcuts = "Editor/Import Shortcuts";
        public const string kResetShortcuts = "Editor/Reset Shortcuts to Default";
        public const string kOpenShortcutsManager = "Editor/Open Shortcuts Manager";
#endif

        [Shortcut(TimeflowShortcutInfo.Path_ExportShortcuts)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kExportShortcuts, false, 10603)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kExportShortcuts, false, 10603)]
        public static void ExportShortcuts()
        {
            List<ShortcutInfo> shortcuts = GetShortcuts();

            string parentDirectory = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(parentDirectory, "TimeflowShortcuts.csv");

            string log = "";
            foreach (var shortcut in shortcuts) {
                log += $"{shortcut.Path},{shortcut.Binding}\n";
            }
            File.WriteAllText(path, log);

            Application.OpenURL(path);
        }


        [Shortcut(TimeflowShortcutInfo.Path_ImportShortcuts)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kImportShortcuts, false, 10604)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kImportShortcuts, false, 10604)]
        public static void ImportShortcuts()
        {
            string parentDirectory = Directory.GetParent(Application.dataPath).FullName;
            string path = EditorUtility.OpenFilePanel("Import Shortcuts (CSV)", parentDirectory, "csv");

            string[] lines = File.ReadAllLines(path);
            if (lines == null || lines.Length == 0) return;

            foreach (string line in lines) {
                if (string.IsNullOrEmpty(line)) continue;
                string[] parts = line.Split(',');
                if (parts.Length < 2) continue;
                if (parts.Length > 2) {
                    Debug.LogWarning("More shortcuts on one line than expected");
                }

                string id = parts[0];
                string key = parts[1];

                if (string.IsNullOrEmpty(key)) continue; // no key is mapped
                if (!id.StartsWith("Timeflow")) continue; // not a Timeflow shortcut

                ShortcutBinding binding;
                if (TryParseStringToShortcut(key, out binding)) {
                    ShortcutManager.instance.RebindShortcut(id, binding);
                    Debug.Log($"Rebinding:{id} : {key}");//--KEEP
                }
                else {
                    Debug.LogWarning($"Failed to read shortcut:{id} : {key}");
                }
            }
        }

        [Shortcut(TimeflowShortcutInfo.Path_ResetShortcuts)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kResetShortcuts, false, 10605)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kResetShortcuts, false, 10605)]
        public static void ResetShortcutsToDefault()
        {
            if (EditorUtility.DisplayDialog("Reset Shorcuts?", "Are you sure you want to revert all Timeflow shortcuts to their " +
                "default settings? This will clear all customized shortcuts for Timeflow. " +
                "Other keyboard shortcuts not pertaining to Timeflow are not changed by this operation.", "Reset Shortcuts", "Cancel")) {
                var ids = ShortcutManager.instance.GetAvailableShortcutIds();
                foreach (var id in ids) {
                    if (!id.StartsWith("Timeflow")) continue;
                    ShortcutManager.instance.ClearShortcutOverride(id);
                }
            }
        }

        [Shortcut(TimeflowShortcutInfo.Path_OpenShortcutsManager)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kOpenShortcutsManager, false, 10606)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kOpenShortcutsManager, false, 10606)]
        public static void OpenShortcutsManager()
        {
            // Open the Shortcut Manager window. There is no API to go directly to Timeflow
            EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
        }

        public static string DecodeString(string shortcut, bool forDisplay)
        {
            if (forDisplay) {
                return shortcut
                    .Replace("#", "Shift + ")
                    .Replace("%", "Control + ")
                    .Replace("^", "Control + ")
                    .Replace("&", "Alt + ")
                    .Replace("Shift + Alt", "Alt + Shift")
                    .Replace("Shift + Control", "Control + Shift")
                    .Replace("Alt + Control", "Control + Alt")
                    .Replace("Control", "Ctrl")
                    .Replace(" + ", "+")
                    .Replace("Alpha", "")
                    .Replace("Keypad", "Num")
                    .Replace("NumEnter", "Enter")
                    .Replace("NumMinus", "Minus")
                    .Replace("RightBracket", "]")
                    .Replace("LeftBracket", "[")
                    .Replace("Scroll Wheel", "Scroll")
                    //.Replace("+1", "+1-9")
                    //.Replace("+F1", "+F1-12")
                    .Replace("BackQuote", "`");
            }
            else {
                return shortcut.Replace("#", "[Shift]").Replace("%", "[Action]").Replace("^", "[Control]").Replace("&", "[Alt]");
            }
        }

        private static string EncodeString(string shortcut)
        {
            return shortcut.Replace("[Shift]", "#").Replace("[Action]", "%").Replace("[Control]", "^").Replace("[Alt]", "&");
        }

        private static string MenuString(string shortcut)
        {
            return shortcut.Replace("]", "+").Replace("[", "").Replace("Control", "Ctrl").Replace("Action", Application.platform == RuntimePlatform.OSXEditor ? "Cmd" : "Ctrl");
        }

        private static string ShortcutToString(ShortcutBinding shortcutBinding)
        {
            string shortcut = "";
            foreach (KeyCombination keyCombination in shortcutBinding.keyCombinationSequence) {
                if (shortcut.Length > 0) shortcut += ",";
                shortcut += ShortcutToString(keyCombination.keyCode, keyCombination.modifiers);
            }
            return shortcut;
        }

        private static string ShortcutToString(KeyCode keyCode, ShortcutModifiers modifiers)
        {
            if (keyCode == KeyCode.None) {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            if ((modifiers & ShortcutModifiers.Alt) != 0) {
                stringBuilder.Append("&");
            }
            if ((modifiers & ShortcutModifiers.Shift) != 0) {
                stringBuilder.Append("#");
            }
            if ((modifiers & ShortcutModifiers.Action) != 0) {
                stringBuilder.Append("%");
            }
            if ((modifiers & ShortcutModifiers.Control) != 0) {
                stringBuilder.Append("^");
            }

            ConvertKeyCodeToString(keyCode, stringBuilder);
            return stringBuilder.ToString();
        }

        private static bool TryParseStringToShortcut(string shortcut, out ShortcutBinding shortcutBinding)
        {
            KeyCombination keyCombination;
            if (TryParseStringToKeyCombination(EncodeString(shortcut), out keyCombination)) {
                shortcutBinding = new ShortcutBinding(keyCombination);
                return true;
            }
            return false;
        }

        private static void ConvertKeyCodeToString(KeyCode keyCode, StringBuilder builder)
        {
            builder.Append($"{keyCode}");
        }

        private static bool TryParseStringToKeyCombination(string shortcut, out KeyCombination keyCombination)
        {
            if (string.IsNullOrEmpty(shortcut)) {
                keyCombination = default(KeyCombination);
                return false;
            }

            ShortcutModifiers shortcutModifiers = ShortcutModifiers.None;
            int num = 0;
            bool flag = false;
            do {
                flag = true;
                if (num >= shortcut.Length) {
                    flag = false;
                    break;
                }

                switch (shortcut[num]) {
                    case '&':
                        shortcutModifiers |= ShortcutModifiers.Alt;
                        num++;
                        break;
                    case '%':
                        shortcutModifiers |= ShortcutModifiers.Action;
                        num++;
                        break;
                    case '#':
                        shortcutModifiers |= ShortcutModifiers.Shift;
                        num++;
                        break;
                    case '^':
                        shortcutModifiers |= ShortcutModifiers.Control;
                        num++;
                        break;
                    case '_':
                        num++;
                        break;
                    default:
                        flag = false;
                        break;
                }
            }
            while (flag);
            string keyCodeString = shortcut.Substring(num, shortcut.Length - num);
            if (!TryParseString(keyCodeString, out var keyCode)) {
                keyCombination = default(KeyCombination);
                return false;
            }

            keyCombination = new KeyCombination(keyCode, shortcutModifiers);
            return true;
        }

        private static bool TryParseString(string keyCodeString, out KeyCode keyCode)
        {
            keyCode = KeyCode.None;

            if (string.IsNullOrEmpty(keyCodeString)) {
                return false;
            }
            if (Enum.TryParse(keyCodeString, out keyCode)) {
                return true;
            }
            Debug.LogWarning($"Failed to parse key code string:{keyCodeString}");

            if (keyCodeString.Length != 1) {
                Debug.LogWarning($"Unrecognized key code:{keyCodeString}");
                return false;
            }

            char c = (char)(keyCode = (KeyCode)char.ToLowerInvariant(keyCodeString[0]));
            return Enum.IsDefined(typeof(KeyCode), keyCode);
        }
    }
}//AxonGenesis

#endif
