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
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;

namespace AxonGenesis
{

    public partial class AxonGUI
    {

        #region PRESETS

        public static bool TypeHasPreset(Type type)
        {
            if (type == null) return false;



            return true;
        }

        public static void PresetsMenuPopup(Component target)
        {
            if (target == null) return;

            ComponentPresetPopup.ShowWindow(target);
            //GenericMenu menu = new GenericMenu();

            //PresetsMenu(menu, "", "", target, target.gameObject);

            //Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
            //menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
        }

        public static bool PresetsMenu(GenericMenu menu, string path, string prefix, Component target, GameObject obj)
        {
            bool displayed = false;
            if (menu.GetItemCount() > 0) {
                menu.AddSeparator(path);
            }
            menu.AddItem(new GUIContent(path + prefix + "Save Preset"), false, ComponentPresetWindow.OpenFromMenu, target);

            ComponentPreset[] presets = ComponentPresetMenu.FindPresetsForComponent(target);
            if (presets != null && presets.Length > 0) {
                displayed = true;
                menu.AddSeparator(path + prefix);
                foreach (ComponentPreset preset in presets) {
                    string baseName = path + prefix + preset.DisplayName;
                    AddPresetsMenuItem(baseName, preset, target, menu, PresetMenuItemSelected);
                }
            }
            else {
                menu.AddSeparator(path + prefix);
                menu.AddDisabledItem(new GUIContent(path + prefix + "No Presets Available"));
            }
            return displayed;
        }

        public static void AddPresetsMenuItem(string baseName, ComponentPreset preset, Component target, GenericMenu menu, GenericMenu.MenuFunction2 onselect)
        {
            if (target == null) {
                Debug.LogError("Cannot apply preset to null object");
                return;
            }
            PresetMenuItem minfo = new PresetMenuItem(preset, target);
            menu.AddItem(new GUIContent(baseName), false, onselect, minfo);
        }

        public static void PresetMenuItemSelected(object info)
        {
            PresetMenuItem item = (PresetMenuItem)info;
            if (item != null && item.Preset != null) {
                if (item.Preset == null) {
                    Debug.LogError("Invalid preset type selected '" + item.Preset.name + "'. Must be a type of ComponentPreset.");
                }
                else
                if (item.Target == null) {
                    Debug.LogError("Cannot apply preset '" + item.Preset.name + "' to a null game object.");
                }

                item.Preset.Apply(item.Target);
                EditorGUIUtility.ExitGUI();
            }
            else {
                Debug.LogError("Invalid preset selected. No changes were applied.");
            }
        }

        #endregion
    }
}
#endif
