// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(PropertyLink))]
    public class PropertyLinkEditor : AxonGenesisEditor<PropertyLink, PropertyLinkEdit> { }

    sealed public class PropertyLinkEdit : AxonGenesisBehaviorEdit<PropertyLink>
    {
#if TIMEFLOW_PRO
        public const string kAddPropertyLink = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🔗 Property Link";
#else
        public const string kAddPropertyLink = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Property Link";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Property Link";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddPropertyLink, false, 128)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddPropertyLink, false, 128)]
        public static void AddPropertyLink()
        {
            ObjectUtil.GetOrAddComponent<PropertyLink>(TimeflowMenu.GetSelectedOrNewGameObject("Property Link"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public PropertyLinkEdit() { }

        public PropertyLinkEdit(PropertyLink _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/PropertyLink";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();
            if (target.Channel != null) {
                AxonGUI.PropertySelect(target, typeof(PropertyLink), target.gameObject, target.Channel.ToProperty, Property.PropertyFilters.All, null, true, true, false, false);
            }
            AxonGUI.EndHorizontal();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            MainGUI();
            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.UpdateTime();
                EditorUtil.SetDirty(target);
            }
        }

        private void MainGUI()
        {
            AxonGUI.BeginBox();

            if (target.Channel == null || target.Channel.ToProperty == null) {
                AxonGUI.Info("The channel and source property must be assigned first.");
            }
            else {
                PropertiesGUI();
            }

            AxonGUI.EndBox();
        }


        private void PropertiesGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Add a new property. This can control component values for any object in the scene.");
            if (AxonGUI.ButtonInline("+")) {
                target.AddProperty();
            }
            if (AxonGUI.ButtonInline("+ Selected")) {
                if (Selection.gameObjects != null) {
                    target.AddSelectedObjects();
                }
            }
            if (AxonGUI.ButtonInline("+ Children")) {
                target.GatherChildren();
            }
            AxonGUI.SetTooltip("Applies the first property mapping to all the others in the list. Use this after changing " +
                "the property of the first item to update the list to match.");
            if (AxonGUI.ButtonInline("Resetup")) {
                target.ResetupObjects();
            }
            if (AxonGUI.ButtonInline("Clear All")) {
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all properties?", "Yes", "No")) {
                    target.ClearAll();
                }
            }
            if (AxonGUI.ButtonInline("First Only")) {
                if (EditorUtility.DisplayDialog("First Only", "Are you sure you want to clear all properties except for the first one?", "Yes", "No")) {
                    target.ClearAllKeepFirst();
                }
            }
            if (AxonGUI.ButtonInline("Sort")) {
                target.Properties.Sort((a, b) => a.GameObject.name.CompareTo(b.GameObject.name));
            }
            target.EditorShowObjects = AxonGUI.FieldToggleInline(target, "Show Objects", target.EditorShowObjects);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginBoxPadded();

            if (target.Properties != null && target.Properties.Count > 0) {
                int moveUp = -1;
                int moveDown = -1;
                int remove = -1;

                for (int x = 0; x < target.Properties.Count; x++) {
                    AxonGUI.BeginHorizontal();

                    target.Properties[x].ShowPropertyObject = target.EditorShowObjects;

                    AxonGUI.LabelInline(x + ":");
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove")) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                        moveDown = x;
                    }

                    if (target.Properties[x] == null) target.Properties[x] = new Property();
                    target.Properties[x].ShowComponentName = true;
                    AxonGUI.PropertySelectInline(target, null, target.Properties[x].GameObject, target.Properties[x], null, true);
                    AxonGUI.EndHorizontal();
                }

                if (remove > -1) {
                    target.Properties.RemoveAt(remove);
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder", true);
                    Property a = target.Properties[moveUp];
                    Property b = target.Properties[moveUp - 1];
                    target.Properties[moveUp] = b;
                    target.Properties[moveUp - 1] = a;
                }
                if (moveDown >= 0 && moveDown < target.Properties.Count - 1) {
                    UndoUtil.Undo(target, "Reorder", true);
                    Property a = target.Properties[moveDown];
                    Property b = target.Properties[moveDown + 1];
                    target.Properties[moveDown] = b;
                    target.Properties[moveDown + 1] = a;
                }
            }
            else {
                AxonGUI.HelpBox("No properties have been added yet. Add properties to link to the source property above.\n" +
                    "TIP: Lock the inspector to select objects to add as target properties.", MessageType.Info);
            }
            AxonGUI.EndBoxPadded();
        }
    }

}//AxonGenesis

#endif