// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(Blend))]
    public class BlendEditor : AxonGenesisEditor<Blend, BlendEdit> { }
    sealed public class BlendEdit : AxonGenesisBehaviorEdit<Blend>
    {
#if TIMEFLOW_PRO
        public const string kAddBlend = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🔀 Blend";
#else
        public const string kAddBlend = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Blend";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Blend";

        [Shortcut(kShortcut, KeyCode.B, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddBlend + TimeflowMenu.Tab + TimeflowShortcutBindings.AddBehaviorBlend, false, 103)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddBlend, false, 103)]
        public static void AddBlend()
        {
            ObjectUtil.GetOrAddComponent<Blend>(TimeflowMenu.GetSelectedOrNewGameObject("Blend"));
        }


        public TimeflowBehaviorSharedEdit behaviorUI;
        public List<string> Tabs;
        public List<string> SetsTab;
        public bool EditIDs;

        private bool showPropertyObjects;
        private bool isRenamingProperties;

        public BlendEdit() { }

        public BlendEdit(Blend _target)
        {
            target = _target;
            behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/blend";
        }


        public override void GUIMenu()
        {
            AxonGUI.UndoName = "Set Update Enabled";
            AxonGUI.SetTooltip("Blend is only applied if Update is enabled. Turn this off when you wish to work with Blend without applying values to target objects.");
            target.EnableUpdate = AxonGUI.FieldToggleInline(target, "Update", target.EnableUpdate);

            Color tc = GUI.color;
            GUI.color = target.IsEditing ? AxonColor.EditingOverride : target.ManualOverride ? AxonColor.ManualOverride : AxonColor.Default;
            if (AxonGUI.ButtonInline(target.IsEditing ? "Editing Set" : "Manual Override")) {
                if (target.IsEditing) {
                    target.StopEdit();
                }
                else {
                    target.ManualOverride = !target.ManualOverride;
                }
            }
            GUI.color = tc;

            AxonGUI.SetTooltip("Creates a new set using the current values of the assigned transforms and properties.");
            if (AxonGUI.ButtonInline("Capture Set")) {
                target.Capture(true, false);
            }
            AxonGUI.SetTooltip("Add a new empty set. Each set stores a set of values, such as the position and orientation of an object.");
            if (AxonGUI.ButtonInline("Add Empty Set")) {
                target.AddSet();
            }
            if (!target.Hold && (!target.ManualOverride || !target.OverrideHold)) {
                if (AxonGUI.ButtonInline("Swap")) {
                    target.SwapSets();
                }
            }

            AxonGUI.UndoName = "Set Show Buttons";
            target.EditorShowTransitions = AxonGUI.FieldToggleInline(target, "Show Buttons", target.EditorShowTransitions);

#if AXON_EXPERIMENTAL
            if (AxonGUI.ButtonInline("Copy AEMB")) {
                target.CopyAEMB();
            }
#endif
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            MainGUI();

            editor.serializedObject.ApplyModifiedProperties();

            if (GUI.changed) {
                if (target.UpdateFrequency != TimeflowBehavior.UpdateFrequencies.Explicit) {
                    target.Refresh();
                }
            }
        }

        public void MainGUI()
        {
            bool isEmpty = target.Sets == null || target.Sets.Count == 0;
            if (isEmpty) {
                AxonGUI.HelpBox("No sets have been added yet.", MessageType.Info);
            }
            else {
                if (target.EditorShowTransitions) {
                    TransitionGUI();
                }
                ControlGUI();
            }

            if (!target.IsEditing) {
                if (Tabs == null) {
                    Tabs = new List<string>();
                    Tabs.Add("Active");
                    Tabs.Add("Setup");
                    Tabs.Add("Properties");
                    Tabs.Add("Sets");
                    Tabs.Add("Channel");
                }
                target.EditorTab = AxonGUI.ButtonRow(Tabs, target.EditorTab);

                AxonGUI.BeginBoxPadded();

                if (target.EditorTab == 0) {
                    ActiveGUI();
                }
                else
                if (target.EditorTab == 1) {
                    SetupGUI();
                }
                else
                if (target.EditorTab == 2) {
                    PropertiesGUI();
                }
                else
                if (target.EditorTab == 3) {
                    SetsGUI();
                }
                else
                if (target.EditorTab == 4) {
                    if (target.Channel != null) {
                        target.Channel.InspectorSettingsGUI();
                    }
                }

                AxonGUI.EndBoxPadded();

            }

            behaviorUI.MainGUI();
        }

        public void ControlGUI()
        {
            string[] setNames = target.SetNames.ToArray();
            string[] setToNames = target.SetToNames.ToArray();

            int count = target.Sets == null ? 0 : target.Sets.Count - 1;

            GUI.color = target.IsEditing ? AxonColor.EditingOverride : target.ManualOverride ? AxonColor.ManualOverride : target.HasAnimation ? target.GUIColor : AxonColor.Default;
            AxonGUI.BeginVertical(target.ManualOverride || target.IsEditing ? AxonUI.HeaderStyleSelected : AxonUI.HeaderStyleDark);
            GUI.color = AxonColor.Default;
            AxonGUI.BeginBoxPadded();

            if (target.IsEditing) {
                Settings(target.OverrideFrom, true, true);
            }
            else {
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Use manual override to preview a specific set or blend, overriding the current selection or any animated blends. A yellow border is drawn around this control when override is enabled as a reminder to disable override when finished using the feature.");

                if (target.ManualOverride) {
                    AxonGUI.UndoName = "Set Direct Control";
                    AxonGUI.SetTooltip("When enabled, direct control allows you to override and set the transform directly. This may be useful when setting up a new world coordinate location.");
                    target.DirectControl = AxonGUI.FieldToggle(target, "Direct Control", target.DirectControl);
                }

                if (target.HasAnimation && target.EditKeyframes && target.CurrentKey != null) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("The current display values are from the active keyframe in the Blend channel. Any changes are applied to the keyframe. Use Manual Override to preview changes without affecting keyframe values.");
                    AxonGUI.Label("* Editing current keyframe", AxonUI.InfoLabelStyle);
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndHorizontal();

                if (target.OverrideFrom < 0) {
                    target.OverrideFrom = 0;
                }
                else
                if (target.OverrideFrom > count) {
                    target.OverrideFrom = count;
                }

                if (target.OverrideTo < 0) {
                    target.OverrideTo = 0;
                }
                else
                if (target.OverrideTo > count) {
                    target.OverrideTo = count;
                }

                if (target.ManualOverride) {
                    if (target.DirectControl) {
                        AxonGUI.UndoName = "Set Override Position";
                        target.OverridePosition = AxonGUI.FieldVector3(target, "Position", target.OverridePosition);

                        AxonGUI.UndoName = "Set Override Rotation";
                        target.OverrideRotation = AxonGUI.FieldVector3(target, "Rotation", target.OverrideRotation);

                        AxonGUI.UndoName = "Set Override Scale";
                        target.OverrideScale = AxonGUI.FieldVector3(target, "Scale", target.OverrideScale);
                    }
                    else {
                        AxonGUI.BeginHorizontal();
                        GUI.color = target.Sets[target.OverrideFrom].GUIColor;
                        AxonGUI.UndoName = "Set Override From";
                        target.OverrideFrom = AxonGUI.FieldPopup(target, "From", target.OverrideFrom, setNames);
                        GUI.color = AxonColor.Default;

                        AxonGUI.UndoName = "Set Override Hold";
                        AxonGUI.SetTooltip("If enabled, the blend is disabled and holds on the From set.");
                        target.OverrideHold = AxonGUI.FieldToggleInline(target, "Hold", target.OverrideHold, GUILayout.Width(100));
                        if (AxonGUI.ButtonInline("Edit")) {
                            target.StartEdit(target.OverrideFrom);
                        }
                        AxonGUI.EndHorizontal();

                        if (!target.OverrideHold) {
                            AxonGUI.BeginHorizontal();
                            GUI.color = target.Sets[target.OverrideTo].GUIColor;
                            AxonGUI.UndoName = "Set Override To";
                            target.OverrideTo = AxonGUI.FieldPopup(target, "To", target.OverrideTo, setToNames);
                            GUI.color = AxonColor.Default;

                            AxonGUI.UndoName = "Set Override Reverse";
                            AxonGUI.SetTooltip("Processes the blend in opposite order, basically swapping To and From");
                            target.OverrideReverse = AxonGUI.FieldToggleInline(target, "Reverse", target.OverrideReverse, GUILayout.Width(100));
                            if (AxonGUI.ButtonInline("Edit")) {
                                target.StartEdit(target.OverrideTo);
                            }
                            AxonGUI.EndHorizontal();

                            AxonGUI.BeginHorizontal();
                            AxonGUI.UndoName = "Set Override Blend";
                            AxonGUI.SetTooltip("Interpolates the blend between From and To.");
                            target.OverrideBlend = AxonGUI.FieldSlider(target, "Blend", target.OverrideBlend, 0, 1f);
                            target.OverrideBlendMode = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.OverrideBlendMode);
                            AxonGUI.EndHorizontal();
                        }
                    }
                }
                else {
                    GUI.color = AxonColor.Default;
                    EditorGUI.BeginChangeCheck();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Blend From";
                    target.From = AxonGUI.FieldPopup(target, "From", target.From, setNames);

                    AxonGUI.UndoName = "Set Blend Hold";
                    AxonGUI.SetTooltip("If enabled, the blend is disabled and holds on the From set.");
                    target.Hold = AxonGUI.FieldToggleInline(target, "Hold", target.Hold, GUILayout.Width(100));
                    if (AxonGUI.ButtonInline("Edit")) {
                        target.StartEdit(target.From);
                    }
                    AxonGUI.EndHorizontal();

                    if (!target.Hold) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Blend To";
                        target.To = AxonGUI.FieldPopup(target, "To", target.To, setToNames);

                        AxonGUI.UndoName = "Set Blend Reverse";
                        AxonGUI.SetTooltip("Processes the blend in opposite order, basically swapping To and From");
                        target.Reverse = AxonGUI.FieldToggleInline(target, "Reverse", target.Reverse, GUILayout.Width(100));
                        if (AxonGUI.ButtonInline("Edit")) {
                            target.StartEdit(target.To);
                        }
                        AxonGUI.EndHorizontal();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Blend Amount";
                        AxonGUI.SetTooltip("Interpolates the blend between From and To.");
                        target.BlendAmount = AxonGUI.FieldSlider(target, "Blend", target.BlendAmount, 0, 1f);
                        target.BlendMode = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.BlendMode);
                        AxonGUI.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck()) {
                        target.OverrideFrom = target.From;
                        target.OverrideTo = target.To;
                        target.OverrideBlend = target.BlendAmount;
                        target.OverrideBlendMode = target.BlendMode;
                        target.OverrideHold = target.Hold;
                        target.OverrideReverse = target.Reverse;

                        if (!target.IsEditing && target.HasAnimation && target.EditKeyframes && target.CurrentKey != null) {
                            target.CurrentKey.FromSet = target.GetID(target.From);
                            target.CurrentKey.ToSet = target.GetID(target.To);
                            target.CurrentKey.Hold = target.Hold;
                            target.CurrentKey.Reverse = target.Reverse;
                            target.CurrentKey.InterpolationMode = target.BlendMode;
                        }
                    }
                }
            }
            AxonGUI.EndBoxPadded();
            AxonGUI.EndVertical();
        }

        public void ActiveGUI()
        {
            AxonGUI.BeginBoxPadded();
            if (target.Sets != null && target.Sets.Count > 0) {
                if (target.ManualOverride) {
                    Settings(target.OverrideFrom, true, false);
                    if (!target.OverrideHold && target.OverrideTo != target.OverrideFrom) {
                        Settings(target.OverrideTo, true, false);
                    }
                }
                else {
                    Settings(target.From, true, false);
                    if (!target.Hold && target.OverrideTo != target.OverrideFrom) {
                        Settings(target.To, true, false);
                    }
                }
            }
            AxonGUI.EndBoxPadded();
        }

        public void SetupGUI()
        {
            AxonGUI.BeginChangeCheck();
            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set World Parent";
            AxonGUI.SetTooltip("This sets the default parent when unparenting an object or calculating in world space. In order to function properly, it must remain within the parent Timeflow group.");
            if (target.WorldParent == null) {
                target.WorldParent = Timeflow.Active != null ? Timeflow.Active.transform : null;
            }
            target.WorldParent = (Transform)AxonGUI.FieldObject(target, "World Parent", target.WorldParent, typeof(Transform), true);

            AxonGUI.UndoName = "Set Force World Space";
            AxonGUI.SetTooltip("When enabled, blends are always calculated in world space and no reparenting is performed. Otherwise, the target is reparented for sets using Parent space.");
            target.ForceWorld = AxonGUI.FieldToggleInline(target, "Force World Space", target.ForceWorld);
            AxonGUI.EndHorizontal();

            // Use Physics option
            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Use Physics";
            AxonGUI.SetTooltip("When enabled, position and rotation are applied using a Rigidbody (if present) instead of the Transform, for physics-friendly movement.");
            target.UsePhysics = AxonGUI.FieldToggle(target, "Use Physics", target.UsePhysics);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Parent of";
            AxonGUI.SetTooltip("Select a game object to reparent to other objects, defined by each Set. For example, this could be used to move a camera rig to various predesignated locations within the scene. Assign the object you want to move.");
            target.EnableReparent = AxonGUI.FieldToggle(target, "Set Parent of", target.EnableReparent);
            if (target.EnableReparent) {
                target.ReparentTransform = (Transform)AxonGUI.FieldObjectInline(target, target.ReparentTransform, typeof(Transform), true);

            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Position of";
            AxonGUI.SetTooltip("Sets the local position of the designated game object. This is applied in local space so that it is always relative to current parent object.");
            target.EnablePosition = AxonGUI.FieldToggle(target, "Set Position of", target.EnablePosition);
            if (target.EnablePosition) {
                target.PositionTransform = (Transform)AxonGUI.FieldObjectInline(target, target.PositionTransform, typeof(Transform), true);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Rotation of";
            AxonGUI.SetTooltip("Sets the euler rotation for the designated object. This can be the same as the position and scale objects, or different. Rotation is applied in local coordinates.");
            target.EnableRotation = AxonGUI.FieldToggle(target, "Set Rotation of", target.EnableRotation);
            if (target.EnableRotation) {
                target.RotationTransform = (Transform)AxonGUI.FieldObjectInline(target, target.RotationTransform, typeof(Transform), true);
            }
            AxonGUI.SetTooltip("Enable Quaternion rotation to calculate the shortest path between rotations for smooth rotations without extra spins. " +
                "Disable this to use Euler angles, which preserves the number of rotational spins.");
            target.EnableQuaternions = AxonGUI.FieldToggleInline(target, "Use Quaternions", target.EnableQuaternions);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Scale of";
            AxonGUI.SetTooltip("Sets the scale of the designated object in local coordinates. This object can be the same as used above, or a different game object.");
            target.EnableScale = AxonGUI.FieldToggle(target, "Set Scale of", target.EnableScale);
            if (target.EnableScale) {
                target.ScaleTransform = (Transform)AxonGUI.FieldObjectInline(target, target.ScaleTransform, typeof(Transform), true);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Field of View of";
            AxonGUI.SetTooltip("Sets the field of view for the designated camera.");
            target.EnableFieldOfView = AxonGUI.FieldToggle(target, "Set Field of View of", target.EnableFieldOfView);
            if (target.EnableFieldOfView) {
                target.Camera = (Camera)AxonGUI.FieldObjectInline(target, target.Camera, typeof(Camera), true);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Activate Objects";
            AxonGUI.SetTooltip("Select game objects to activate and deactivate dynamically using blend sets.");
            target.EnableActivateObjects = AxonGUI.FieldToggle(target, "Activate Objects", target.EnableActivateObjects);
            if (target.EnableActivateObjects) {
                ActivateGUI();
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Trigger Events";
            AxonGUI.SetTooltip("If enabled, each transition set may define an on and off event to be triggered when arriving to and departing from the set.");
            target.EnableEvents = AxonGUI.FieldToggle(target, "Trigger Events", target.EnableEvents);
            AxonGUI.EndHorizontal();

            if (AxonGUI.EndChangeCheck()) {
                target.Setup();
            }
            NotifyGUI();
        }

        public void PropertiesGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Add a new property. This can control component values for any object in the scene.");
            if (AxonGUI.ButtonInline("Add Property")) {
                UndoUtil.Undo(target, "Add Property", true);
                if (target.Properties == null) target.Properties = new List<Property>();
                Property prop = new Property();
                prop.ShowPropertyObject = true;
                prop.Comp = target.transform;
                target.Properties.Add(prop);

                if (target.Sets != null) {
                    foreach (BlendSet node in target.Sets) {
                        node.Values.Add(new PropertyValue(prop));
                    }
                }
            }
            if (AxonGUI.ButtonInline("Clear All")) {
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all properties?", "Yes", "No")) {
                    UndoUtil.Undo(target, "Clear All Properties", true);
                    target.Properties = null;
                    foreach (BlendSet node in target.Sets) {
                        node.Values = null;
                    }
                }
            }
            AxonGUI.Space();

            Blend.GatherModes mode = (Blend.GatherModes)AxonGUI.FieldEnumPopupInline(target, "Gather", Blend.GatherModes.None);
            if (mode != Blend.GatherModes.None) {
                UndoUtil.Undo(target, "Gather Properties", true);
                target.Gather(mode);
            }

            AxonGUI.Space();


            if (target.Properties != null && target.Properties.Count > 0) {
                AxonGUI.SetTooltip("Names default to the target field and attribute name but can be changed manually to customize the display to help in the creative process.");
                if (!isRenamingProperties) {
                    if (AxonGUI.ButtonInline("Edit Names")) {
                        isRenamingProperties = true;
                    }
                }
                else {
                    if (AxonGUI.ButtonInline("Stop Editing Names")) {
                        isRenamingProperties = false;
                    }
                }
                AxonGUI.SetTooltip("Clears all custom names and reverts them to the default property names.");
                if (AxonGUI.ButtonInline("Show Objects")) {
                    showPropertyObjects = !showPropertyObjects;
                    if (target.Properties != null) {
                        for (int x = 0; x < target.Properties.Count; x++) {
                            target.Properties[x].ShowPropertyObject = showPropertyObjects;
                        }
                    }
                }
                if (AxonGUI.ButtonInline("Reset Names")) {
                    isRenamingProperties = false;
                    if (target.Properties != null) {
                        UndoUtil.Undo(target, "Reset Names", true);
                        for (int x = 0; x < target.Properties.Count; x++) {
                            target.Properties[x].ResetName(true);
                        }
                    }
                }
            }
            AxonGUI.EndHorizontal();
            AxonGUI.BeginBoxPadded();

            if (target.Properties != null && target.Properties.Count > 0) {
                int moveUp = -1;
                int moveDown = -1;
                int remove = -1;

                for (int x = 0; x < target.Properties.Count; x++) {
                    AxonGUI.BeginHorizontal();

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
                    if (isRenamingProperties) {
                        AxonGUI.UndoName = "Set Property Display Name";
                        target.Properties[x].DisplayName = AxonGUI.FieldTextInline(target, null, target.Properties[x].DisplayName, true, GUILayout.Width(160));
                    }
                    AxonGUI.PropertySelectInline(target, null, target.Properties[x].GameObject, target.Properties[x], null, true);
                    AxonGUI.EndHorizontal();
                }

                if (remove > -1) {
                    target.Properties.RemoveAt(remove);
                    foreach (BlendSet node in target.Sets) {
                        node.Values.RemoveAt(remove);
                    }
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder", true);
                    Property a = target.Properties[moveUp];
                    Property b = target.Properties[moveUp - 1];
                    target.Properties[moveUp] = b;
                    target.Properties[moveUp - 1] = a;

                    foreach (BlendSet node in target.Sets) {
                        PropertyValue av = node.Values[moveUp];
                        PropertyValue bv = node.Values[moveUp - 1];
                        node.Values[moveUp] = bv;
                        node.Values[moveUp - 1] = av;
                    }
                }
                if (moveDown >= 0 && moveDown < target.Properties.Count - 1) {
                    UndoUtil.Undo(target, "Reorder", true);
                    Property a = target.Properties[moveDown];
                    Property b = target.Properties[moveDown + 1];
                    target.Properties[moveDown] = b;
                    target.Properties[moveDown + 1] = a;

                    foreach (BlendSet node in target.Sets) {
                        PropertyValue av = node.Values[moveDown];
                        PropertyValue bv = node.Values[moveDown + 1];
                        node.Values[moveDown] = bv;
                        node.Values[moveDown + 1] = av;
                    }
                }
            }
            else {
                AxonGUI.HelpBox("No properties have been added yet.", MessageType.Info);
            }
            AxonGUI.EndBoxPadded();
        }

        public void ActivateGUI()
        {
            AxonGUI.Space();
            AxonGUI.SetTooltip("Add objects to activate or deactivate dynamically with each blend set.");
            if (AxonGUI.ButtonInline("Add Object")) {
                if (target.ActivateObjects == null) target.ActivateObjects = new List<BlendObjectActivate>();
                target.ActivateObjects.Add(new BlendObjectActivate());
                target.Refresh();
            }
            if (target.ActivateObjects != null && target.ActivateObjects.Count > 0) {
                if (AxonGUI.ButtonInline("Clear All")) {
                    if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all activate objects?", "Yes", "No")) {
                        UndoUtil.Undo(target, "Clear All Activate Objects", true);
                        target.ActivateObjects = null;
                    }
                }
            }
            AxonGUI.EndHorizontal();


            if (target.ActivateObjects != null) {
                AxonGUI.BeginBox();
                int moveUp = -1;
                int moveDown = -1;
                int insert = -1;
                int remove = -1;

                for (int x = 0; x < target.ActivateObjects.Count; x++) {
                    AxonGUI.BeginVertical("box");
                    AxonGUI.BeginHorizontal();

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Object", true)) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Object", true)) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up", true)) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down", true)) {
                        moveDown = x;
                    }

                    if (target.ActivateObjects[x] == null) target.ActivateObjects[x] = new BlendObjectActivate();
                    target.ActivateObjects[x].Object = (GameObject)AxonGUI.FieldObjectInline(target, target.ActivateObjects[x].Object, typeof(GameObject), true);

                    AxonGUI.SetTooltip("Select a method for determining how objects are activated when blending between sets with different values.");
                    target.ActivateObjects[x].Transition = (BlendObjectActivate.Transitions)AxonGUI.FieldEnumPopupInline(target, "Transition", target.ActivateObjects[x].Transition, GUILayout.Width(220));
                    if (target.ActivateObjects[x].Transition == BlendObjectActivate.Transitions.Midpoint) {
                        if (target.ActivateObjects[x].Midpoint < 0f) target.ActivateObjects[x].Midpoint = 0f;
                        else
                        if (target.ActivateObjects[x].Midpoint > 1f) target.ActivateObjects[x].Midpoint = 1f;
                        AxonGUI.SetTooltip("Sets the position in the blend to activate the object. Must be a value from 0 to 1.");
                        target.ActivateObjects[x].Midpoint = AxonGUI.FieldFloatInline(target, target.ActivateObjects[x].Midpoint);
                    }

                    AxonGUI.SetTooltip("Determines the default active state for this object when creating new sets.");
                    target.ActivateObjects[x].Default = AxonGUI.FieldToggleInline(target, "Default", target.ActivateObjects[x].Default);

                    AxonGUI.Space();
                    AxonGUI.EndHorizontal();

                    AxonGUI.EndVertical();
                }

                if (remove > -1) {
                    UndoUtil.Undo(target, "Remove Set", true);
                    if (target.Sets != null) {
                        foreach (BlendSet set in target.Sets) {
                            if (target.ValidateSet(set)) {
                                set.Activates.RemoveAt(remove);
                            }
                        }
                    }
                    target.ActivateObjects.RemoveAt(remove);
                    target.Refresh();
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder Set", true);
                    BlendObjectActivate a = target.ActivateObjects[moveUp];
                    BlendObjectActivate b = target.ActivateObjects[moveUp - 1];
                    target.ActivateObjects[moveUp] = b;
                    target.ActivateObjects[moveUp - 1] = a;

                    if (target.Sets != null) {
                        foreach (BlendSet set in target.Sets) {
                            if (target.ValidateSet(set)) {
                                bool activateA = set.Activates[moveUp];
                                bool activateB = set.Activates[moveUp - 1];
                                set.Activates[moveUp] = activateB;
                                set.Activates[moveUp - 1] = activateA;
                            }
                        }
                    }
                    target.Refresh();
                }
                if (moveDown >= 0 && moveDown < target.ActivateObjects.Count - 1) {
                    UndoUtil.Undo(target, "Reorder Set", true);
                    BlendObjectActivate a = target.ActivateObjects[moveDown];
                    BlendObjectActivate b = target.ActivateObjects[moveDown + 1];
                    target.ActivateObjects[moveDown] = b;
                    target.ActivateObjects[moveDown + 1] = a;

                    if (target.Sets != null) {
                        foreach (BlendSet set in target.Sets) {
                            if (target.ValidateSet(set)) {
                                bool activateA = set.Activates[moveDown];
                                bool activateB = set.Activates[moveDown + 1];
                                set.Activates[moveDown] = activateB;
                                set.Activates[moveDown + 1] = activateA;
                            }
                        }
                    }
                    target.Refresh();
                }
                if (insert != -1) {
                    UndoUtil.Undo(target, "Insert Set", true);
                    if (target.ActivateObjects == null) target.ActivateObjects = new List<BlendObjectActivate>();
                    int index = insert + 1;

                    if (target.Sets != null) {
                        foreach (BlendSet set in target.Sets) {
                            if (target.ValidateSet(set)) {
                                set.Activates.Insert(index, true);
                            }
                        }
                    }
                    target.ActivateObjects.Insert(index, new BlendObjectActivate());
                    target.Refresh();
                }
                AxonGUI.EndBox();
            }

            AxonGUI.BeginHorizontal(); // intentional to match EndHorizontal in calling method
        }

        public void NotifyGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.Heading("Notify On Change");
            AxonGUI.Space();
            AxonGUI.SetTooltip("Add an object to receive the messages OnBlend and OnBlendChange. This can be used to implement additional logic when changing from one set to another. Please refer to the code for further understanding of how this can be implemented in your scripts. ");
            if (AxonGUI.ButtonInline("Add Object")) {
                target.ObjectsToNotify.Add(null);
                target.Refresh();
            }
            if (target.ObjectsToNotify != null && target.ObjectsToNotify.Count > 0) {
                if (AxonGUI.ButtonInline("Clear All")) {
                    if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all notified objects?", "Yes", "No")) {
                        UndoUtil.Undo(target, "Clear All Objects to Notify", true);

                        target.ObjectsToNotify = null;
                    }
                }
            }
            AxonGUI.EndHorizontal();


            if (target.ObjectsToNotify != null) {
                AxonGUI.Space();
                AxonGUI.Indent++;
                int moveUp = -1;
                int moveDown = -1;
                int insert = -1;
                int remove = -1;

                for (int x = 0; x < target.ObjectsToNotify.Count; x++) {
                    AxonGUI.BeginVertical("box");
                    AxonGUI.BeginHorizontal();

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Object", true)) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Object", true)) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up", true)) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down", true)) {
                        moveDown = x;
                    }

                    target.ObjectsToNotify[x] = (BlendUpdate)AxonGUI.FieldObjectInline(target, target.ObjectsToNotify[x], typeof(BlendUpdate), true);

                    AxonGUI.Space();
                    AxonGUI.EndHorizontal();

                    AxonGUI.EndVertical();
                }

                if (remove > -1) {
                    UndoUtil.Undo(target, "Remove Set", true);
                    target.ObjectsToNotify.RemoveAt(remove);
                    target.Refresh();
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder Set", true);
                    BlendUpdate a = target.ObjectsToNotify[moveUp];
                    BlendUpdate b = target.ObjectsToNotify[moveUp - 1];
                    target.ObjectsToNotify[moveUp] = b;
                    target.ObjectsToNotify[moveUp - 1] = a;
                    target.Refresh();
                }
                if (moveDown >= 0 && moveDown < target.ObjectsToNotify.Count - 1) {
                    UndoUtil.Undo(target, "Reorder Set", true);
                    BlendUpdate a = target.ObjectsToNotify[moveDown];
                    BlendUpdate b = target.ObjectsToNotify[moveDown + 1];
                    target.ObjectsToNotify[moveDown] = b;
                    target.ObjectsToNotify[moveDown + 1] = a;
                    target.Refresh();
                }
                if (insert != -1) {
                    UndoUtil.Undo(target, "Insert Set", true);
                    target.ObjectsToNotify.Insert(insert + 1, null);
                    target.Refresh();
                }
                AxonGUI.Indent--;
            }

            AxonGUI.EndBox();
        }

        public void SetsGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("If enabled, sets are organized in drop down menus by name and the coordinate space they belong to. Otherwise all sets are shown by name in a flat list. This only affects the UI display of sets and has no effect on functionality.");
            target.Categorize = AxonGUI.FieldToggleInline(target, "Categorize Menus", target.Categorize);

            AxonGUI.SetTooltip("If enabled, sets are shaded with yellow for Local transforms, green for Parent, and blue for World This is a visual aid that only affects the UI.");
            BlendSet.EnableGUIColor = target.EditorColorCoded = AxonGUI.FieldToggleInline(target, "Color Coded", target.EditorColorCoded);

            AxonGUI.SetTooltip("Enable to manually change IDs.");
            EditIDs = AxonGUI.FieldToggleInline(target, "Edit IDs", EditIDs);

            if (EditIDs) {
                if (AxonGUI.ButtonInline("Reassign IDs")) {
                    if (EditorUtil.ShowDialog("Are you sure want to reassign all set IDs?", "Doing this may cause keyframes to map to different blends. If you do not have any keyframes set, then it is safe to proceed")) {
                        target.ReassignSetIDs();
                    }
                }
                AxonGUI.Warning("Please beware that changing IDs may affect any behaviors referencing the blend sets and may require remapping if changed.");
            }

            AxonGUI.Space();
            AxonGUI.SetTooltip("Sorts the sets by name in alphabetical order.");
            if (AxonGUI.ButtonInline("Sort")) {
                target.SortSets();
            }
            if (AxonGUI.ButtonInline("Sort by ID")) {
                target.SortSetsByID();
            }
            AxonGUI.SetTooltip("Twirl open or close all of the sets.");
            if (AxonGUI.ButtonInline("Collapse")) {
                for (int x = 0; x < target.Sets.Count; x++) {
                    target.Sets[x].EditorShow = false;
                }
            }
            if (AxonGUI.ButtonInline("Expand All")) {
                for (int x = 0; x < target.Sets.Count; x++) {
                    target.Sets[x].EditorShow = true;
                }
            }
            AxonGUI.EndHorizontal();

            AxonGUI.Space();
            AxonGUI.Indent++;

            if (target.Sets == null) {
                target.Sets = new List<BlendSet>();
            }

            int moveUp = -1;
            int moveDown = -1;
            int insert = -1;
            int remove = -1;

            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleDarkBig);
            if (SetsTab == null) {
                SetsTab = new List<string>();
                SetsTab.Add("All");
                SetsTab.Add("Local");
                SetsTab.Add("Parent");
                SetsTab.Add("World");
            }
            target.EditorSetsTab = AxonGUI.ButtonRowInline(SetsTab, target.EditorSetsTab);

            AxonGUI.Space();
            AxonGUI.SetTooltip("Creates a new set using the current values of the assigned transforms and properties.");
            if (AxonGUI.ButtonInline("Capture Set")) {
                target.Capture(true, false);
            }
            AxonGUI.SetTooltip("Add a new empty set. Each set stores a set of values, such as the position and orientation of an object.");
            if (AxonGUI.ButtonInline("Add Empty Set")) {
                target.AddSet();
            }
            AxonGUI.Space();
            if (target.Sets != null && target.Sets.Count > 0) {
                AxonGUI.SetTooltip("Removes all Sets.");
                if (AxonGUI.ButtonInline("Clear All")) {
                    if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all of the blend sets?", "Yes", "No")) {
                        UndoUtil.Undo(target, "Clear All Blend Sets", true);
                        target.Sets = null;
                        EditorGUIUtility.ExitGUI();
                    }
                }
            }
            AxonGUI.EndHorizontal();

            for (int x = 0; x < target.Sets.Count; x++) {
                if (target.Sets[x] == null) {
                    BlendSet newSet = new BlendSet();
                    target.ValidateSet(newSet);
                    target.Sets[x] = newSet;
                }

                GUI.color = target.Sets[x].GUIColor;

                bool canShow = true;
                if (target.EditorSetsTab == 1) {
                    canShow = target.Sets[x].TransformType == BlendSet.TransformTypes.Local;
                }
                else
                if (target.EditorSetsTab == 2) {
                    canShow = target.Sets[x].TransformType == BlendSet.TransformTypes.Parent;
                }
                else
                if (target.EditorSetsTab == 3) {
                    canShow = target.Sets[x].TransformType == BlendSet.TransformTypes.World;
                }
                if (canShow) {
                    AxonGUI.BeginVertical("box");
                    AxonGUI.BeginHorizontal();
                    target.Sets[x].EditorShow = AxonGUI.FoldoutInline(target.Sets[x].EditorShow, x + "");

                    bool active = x == target.From;
                    bool a = AxonGUI.FieldToggleInline(target, active);
                    if (a && a != active) {
                        target.From = x;
                    }

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Insert Set", true)) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Set", true)) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up", true)) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down", true)) {
                        moveDown = x;
                    }

                    if (EditIDs) {
                        AxonGUI.LabelInline(x + "");
                        target.Sets[x].ID = AxonGUI.FieldIntInline(target, "ID", target.Sets[x].ID);
                    }
                    else {
                        AxonGUI.LabelInline("ID: " + target.Sets[x].ID);
                    }
                    target.Sets[x].Name = AxonGUI.FieldTextInline(target, target.Sets[x].Name);

                    AxonGUI.Space();
                    if (target.IsEditing && target.OverrideFrom == x) {
                        Color c = GUI.color;
                        GUI.color = AxonColor.EditingOverride;
                        if (AxonGUI.ButtonInline("Done")) {
                            target.StopEdit();
                        }
                        GUI.color = c;
                    }
                    else {
                        if (AxonGUI.ButtonInline("Edit")) {
                            target.StartEdit(x);
                        }
                    }
                    if (AxonGUI.ButtonInline("Duplicate")) {
                        UndoUtil.Undo(target, "Duplicate Set", true);
                        BlendSet b = new BlendSet(target.Sets[x]);
                        b.Name = StringUtil.IncrementName(b.Name);
                        target.ValidateSet(b);
                        target.Sets.Add(b);
                        target.Refresh();
                        target.StartEdit(target.Sets.Count - 1);
                    }
                    if (target.Sets[x].EditorShow) {
                        AxonGUI.SetTooltip("Updates the values of the active set with the current values of the assigned transforms and properties.");
                        if (AxonGUI.ButtonInline("Re-Capture")) {
                            target.From = x;
                            target.Capture(false, target.Sets[x].IsWorld);
                        }
                    }

                    AxonGUI.EndHorizontal();

                    Settings(x, false);

                    AxonGUI.EndVertical();

                    GUI.color = AxonColor.Default;
                }
            }

            if (remove > -1) {
                UndoUtil.Undo(target, "Remove Set", true);
                target.Sets.RemoveAt(remove);
                target.Refresh();
            }
            if (moveUp > 0) {
                UndoUtil.Undo(target, "Reorder Set", true);
                BlendSet a = target.Sets[moveUp];
                BlendSet b = target.Sets[moveUp - 1];
                target.Sets[moveUp] = b;
                target.Sets[moveUp - 1] = a;
                target.Refresh();
            }
            if (moveDown >= 0 && moveDown < target.Sets.Count - 1) {
                UndoUtil.Undo(target, "Reorder Set", true);
                BlendSet a = target.Sets[moveDown];
                BlendSet b = target.Sets[moveDown + 1];
                target.Sets[moveDown] = b;
                target.Sets[moveDown + 1] = a;
                target.Refresh();
            }
            if (insert != -1) {
                UndoUtil.Undo(target, "Insert Set", true);
                BlendSet b = new BlendSet(target.Sets[insert]);
                b.Name = StringUtil.IncrementName(b.Name);
                target.ValidateSet(b);
                target.Sets.Insert(insert + 1, b);
                target.Refresh();
            }
            AxonGUI.Space();
            AxonGUI.Indent--;
        }

        public void SettingsRow(int x)
        {
            AxonGUI.BeginHorizontal();
            target.Sets[x].EditorShow = AxonGUI.FoldoutInline(target.Sets[x].EditorShow, x + "");

            bool active = x == target.From;
            bool a = AxonGUI.FieldToggleInline(target, active);
            if (a && a != active) {
                target.From = x;
            }

            target.Sets[x].Name = AxonGUI.FieldTextInline(target, target.Sets[x].Name);
            if (AxonGUI.ButtonInline("Duplicate")) {
                UndoUtil.Undo(target, "Duplicate Set", true);
                BlendSet b = new BlendSet(target.Sets[x]);
                b.Name = StringUtil.IncrementName(b.Name);
                target.ValidateSet(b);
                target.Sets.Add(b);
                target.Refresh();
                target.StartEdit(target.Sets.Count - 1);
                EditorGUIUtility.ExitGUI();
            }
            AxonGUI.SetTooltip("Updates the values of the active set with the current values of the assigned transforms and properties.");
            if (AxonGUI.ButtonInline("Re-Capture")) {
                target.From = x;
                target.Capture(false, target.Sets[x].IsWorld);
            }
            if (AxonGUI.ButtonInline("Delete")) {
                UndoUtil.Undo(target, "Delete Set", true);
                target.Sets.RemoveAt(x);
                EditorGUIUtility.ExitGUI();
            }
            if (target.IsEditing && target.OverrideFrom == x) {
                GUI.color = AxonColor.EditingOverride;
                if (AxonGUI.ButtonInline("Done")) {
                    target.StopEdit();
                }
                GUI.color = AxonColor.Default;
            }
            else {
                if (AxonGUI.ButtonInline("Edit")) {
                    target.StartEdit(x);
                }
            }
            AxonGUI.EndHorizontal();
        }

        public void Settings(int x, bool showRow, bool forceOpen = false)
        {
            if (target.Sets == null || target.Sets.Count == 0) return;
            if (x < 0) x = 0;
            else
            if (x > target.Sets.Count - 1) {
                x = target.Sets.Count - 1;
            }
            GUI.color = target.Sets[x].GUIColor;

            if (showRow) SettingsRow(x);
            if (target.Sets[x].EditorShow || forceOpen) {
                AxonGUI.Indent++;
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("This defines the coordinate space for blending transforms. It has not effect on other property values.\n\n" +
                    "Local: coordinates are defined in local space. If blending another set in Parent or World space, the coordinates of this set are calculated relative to that one.\n\n" +
                    "Parent: coordinates are defined as a child of the specified parent game object. The target object is reparented, unless blending with a World space set in which case the calculation is all handled in world space.\n\n" +
                    "World: defines coordinates in world space. This is always relative to the World Parent.");
                target.Sets[x].TransformType = (BlendSet.TransformTypes)AxonGUI.FieldEnumPopup(target, "Space", target.Sets[x].TransformType);

                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Instead of explicitly defining the transform values, use an existing transform in the scene.");
                target.Sets[x].UseTransform = AxonGUI.FieldToggle(target, "Use Transform", target.Sets[x].UseTransform);
                if (target.Sets[x].UseTransform) {
                    target.Sets[x].Transform = (Transform)AxonGUI.FieldObjectInline(target, target.Sets[x].Transform, typeof(Transform), true);
                }
                if (!target.Sets[x].UseTransform) {
                    AxonGUI.SetTooltip("Creates a new game object in world space using the coordinates of this set, and assigns it to this set in place of the coordinates. This is useful for creating a set of coordinates that are related to a specific object in the scene.");
                    if (AxonGUI.ButtonInline("Convert to Transform")) {
                        target.ConvertToTransform(x);
                    }
                }
                if (!target.Sets[x].IsWorld || target.Sets[x].UseTransform) {
                    AxonGUI.SetTooltip("This converts the coordinates of the assigned transform into world coordinates and unlinks this set from the original transform. This is useful for creating a set of coordinates that are not related to a specific object in the scene.");
                    if (AxonGUI.ButtonInline("Convert to World Coordinates")) {
                        target.ConvertToWorld(x);
                    }
                }

                AxonGUI.EndHorizontal();

                if (target.Sets[x].UseTransform) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label(" ", GUILayout.Width(40));
                    AxonGUI.SetTooltip("Enables setting the position of the target object for this set, relative to the transform.");
                    target.Sets[x].ApplyPosition = AxonGUI.FieldToggleInline(target, "Position", target.Sets[x].ApplyPosition);
                    if (target.Sets[x].ApplyPosition) {
                        target.Sets[x].Position = AxonGUI.FieldVector3Inline(target, "Offset", target.Sets[x].Position);
                    }
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label(" ", GUILayout.Width(40));
                    AxonGUI.SetTooltip("Enables setting the rotation of the target object for this set, relative to the transform.");
                    target.Sets[x].ApplyRotation = AxonGUI.FieldToggleInline(target, "Rotation", target.Sets[x].ApplyRotation);
                    if (target.Sets[x].ApplyRotation) {
                        target.Sets[x].Rotation = AxonGUI.FieldVector3Inline(target, "Offset", target.Sets[x].Rotation);
                    }
                    AxonGUI.EndHorizontal();
                }
                else {
                    if (target.EnableReparent) {
                        if (target.Sets[x].TransformType == BlendSet.TransformTypes.Parent) {
                            AxonGUI.BeginHorizontal();
                            AxonGUI.SetTooltip("The target object is reparented to the specified game object. This is useful for creating blends local to a specific location in the scene. Note that blends between two different parents or to world coordinates is not supported.");
                            target.Sets[x].Parent = (Transform)AxonGUI.FieldObject(target, "Set Parent", target.Sets[x].Parent, typeof(Transform), true);
                            AxonGUI.EndHorizontal();
                        }
                    }
                    if (target.EnablePosition) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.Label(" ", GUILayout.Width(40));
                        AxonGUI.SetTooltip("Enables setting the position of the target object for this set.");
                        target.Sets[x].ApplyPosition = AxonGUI.FieldToggleInline(target, "Position", target.Sets[x].ApplyPosition);
                        if (target.Sets[x].ApplyPosition) {
                            target.Sets[x].Position = AxonGUI.FieldVector3Inline(target, target.Sets[x].Position);
                        }
                        AxonGUI.EndHorizontal();
                    }
                    if (target.EnableRotation) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.Label(" ", GUILayout.Width(40));
                        AxonGUI.SetTooltip("Enables setting the rotation of the target object for this set.");
                        target.Sets[x].ApplyRotation = AxonGUI.FieldToggleInline(target, "Euler", target.Sets[x].ApplyRotation);
                        if (target.Sets[x].ApplyRotation) {
                            target.Sets[x].Rotation = AxonGUI.FieldVector3Inline(target, target.Sets[x].Rotation);
                        }
                        AxonGUI.EndHorizontal();
                    }
                }
                if (target.EnableScale) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label(" ", GUILayout.Width(40));
                    AxonGUI.SetTooltip("Enables setting the scale of the target object for this set.");
                    target.Sets[x].ApplyScale = AxonGUI.FieldToggleInline(target, "Scale", target.Sets[x].ApplyScale);
                    if (target.Sets[x].ApplyScale) {
                        target.Sets[x].Scale = AxonGUI.FieldVector3Inline(target, target.Sets[x].Scale);
                    }
                    AxonGUI.EndHorizontal();
                }
                if (target.EnableFieldOfView) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label(" ", GUILayout.Width(40));
                    AxonGUI.SetTooltip("Sets the field of view for the specified camera.");
                    target.Sets[x].SetFieldOfView = AxonGUI.FieldToggleInline(target, "Field of View", target.Sets[x].SetFieldOfView);
                    if (target.Sets[x].SetFieldOfView) {
                        target.Sets[x].FieldOfView = AxonGUI.FieldSliderInline(target, target.Sets[x].FieldOfView, 0f, 180f);
                    }
                    AxonGUI.EndHorizontal();
                }
                if (target.Properties != null && target.Properties.Count > 0) {
                    float labelWidth = AxonGUI.LabelWidth;
                    int i = 0;
                    if (target.Sets[x].Values == null) target.Sets[x].Values = new List<PropertyValue>();
                    foreach (Property prop in target.Properties) {
                        if (!prop.IsValid()) {
                            GUI.color = AxonColor.Warning;
                            AxonGUI.SetTooltip("Go to the Properties section to assign the undesignated target property.");
                            AxonGUI.Label("UNDEFINED", "No property has been assigned yet");
                            GUI.color = AxonColor.Default;
                        }
                        else {
                            AxonGUI.BeginHorizontal();
                            AxonGUI.Label(" ", GUILayout.Width(40));

                            if (i >= target.Sets[x].Values.Count) {
                                target.Sets[x].Values.Add(new PropertyValue(prop));
                            }
                            if (target.Sets[x].Values[i] == null) {
                                target.Sets[x].Values[i] = new PropertyValue(prop);
                            }

                            AxonGUI.SetTooltip("This property value is only modified by this set if the checkbox is enabled.");
                            target.Sets[x].Values[i].ApplyValue = AxonGUI.FieldToggleInline(target, prop.DisplayName, target.Sets[x].Values[i].ApplyValue);

                            if (target.Sets[x].Values[i].ApplyValue) {
                                if (prop.IsBool) {
                                    target.Sets[x].Values[i].BoolValue = AxonGUI.FieldToggleInline(target, target.Sets[x].Values[i].BoolValue);
                                }
                                else
                                if (prop.IsInt || prop.IsEnum) {
                                    target.Sets[x].Values[i].IntValue = AxonGUI.FieldIntInline(target, target.Sets[x].Values[i].IntValue);
                                }
                                else
                                if (prop.IsFloat || prop.Attribute > -1 || prop.IsUniformValue) {
                                    target.Sets[x].Values[i].FloatValue = AxonGUI.FieldFloatInline(target, target.Sets[x].Values[i].FloatValue);
                                }
                                else
                                if (prop.IsString) {
                                    if (string.IsNullOrEmpty(target.Sets[x].Values[i].StringValue)) target.Sets[x].Values[i].StringValue = "";
                                    target.Sets[x].Values[i].StringValue = AxonGUI.FieldTextInline(target, target.Sets[x].Values[i].StringValue);
                                }
                                else
                                if (prop.IsColor) {
                                    target.Sets[x].Values[i].ColorValue = AxonGUI.FieldColorInline(target, target.Sets[x].Values[i].ColorValue, true);
                                }
                                else
                                if (prop.IsRect) {
                                    target.Sets[x].Values[i].RectValue = AxonGUI.FieldRectInline(target, target.Sets[x].Values[i].RectValue);
                                }
                                else
                                if (prop.IsVector2) {
                                    target.Sets[x].Values[i].Vector2Value = AxonGUI.FieldVector2Inline(target, target.Sets[x].Values[i].Vector2Value);
                                }
                                else
                                if (prop.IsVector3) {
                                    target.Sets[x].Values[i].Vector3Value = AxonGUI.FieldVector3Inline(target, target.Sets[x].Values[i].Vector3Value);
                                }
                                else
                                if (prop.IsVector4) {
                                    target.Sets[x].Values[i].Vector4Value = AxonGUI.FieldVector4Inline(target, target.Sets[x].Values[i].Vector4Value);
                                }
                                else
                                if (prop.IsGameObject) {
                                    target.Sets[x].Values[i].GameObjectValue = (GameObject)AxonGUI.FieldObjectInline(target, target.Sets[x].Values[i].GameObjectValue, typeof(GameObject), true);
                                }
                                else
                                if (prop.IsComponent) {
                                    target.Sets[x].Values[i].ComponentValue = (Component)AxonGUI.FieldObjectInline(target, target.Sets[x].Values[i].ComponentValue, typeof(Component), true);
                                }

                                AxonGUI.SetTooltip("Retrieves the current value of the property.");
                                if (AxonGUI.ButtonInline("Get Value")) {
                                    target.Sets[x].Values[i].ReadValue(prop);
                                }
                            }

                            AxonGUI.EndHorizontal();
                        }
                        i++;
                    }
                }

                if (target.EnableActivateObjects) {
                    if (target.ActivateObjects != null && target.ActivateObjects.Count > 0) {
                        target.ValidateSet(target.Sets[x]);
                        int i = 0;
                        foreach (BlendObjectActivate obj in target.ActivateObjects) {
                            AxonGUI.BeginHorizontal();
                            if (obj.Object == null) {
                                AxonGUI.Label("Null Object", "Assign in Setup");
                            }
                            else {
                                AxonGUI.SetTooltip("This checkbox determines whether this set activates or deactivates the object.");
                                target.Sets[x].Activates[i] = AxonGUI.FieldToggle(target, obj.Object.name, target.Sets[x].Activates[i]);

                                EditorGUI.BeginDisabledGroup(true);
                                AxonGUI.SetTooltip("This object field is for reference only. Please assign objects in the Setup tab.");
                                AxonGUI.FieldObjectInline(target, obj.Object, typeof(GameObject), true);
                                EditorGUI.EndDisabledGroup();
                            }
                            AxonGUI.EndHorizontal();
                            i++;
                        }
                    }
                }

                if (target.EnableEvents) {
                    AxonGUI.BeginBoxPadded();
                    SerializedProperty onEnter = editor.serializedObject.FindProperty("Sets.Array.data[" + x + "].OnEnter");
                    if (onEnter != null) {
                        EditorGUILayout.PropertyField(onEnter, new GUIContent("On Enter"));
                    }
                    else {
                        AxonGUI.Warning("Null property");
                    }

                    SerializedProperty onExit = editor.serializedObject.FindProperty("Sets.Array.data[" + x + "].OnExit");
                    if (onExit != null) {
                        EditorGUILayout.PropertyField(onExit, new GUIContent("On Exit"));
                    }
                    else {
                        AxonGUI.Warning("Null property");
                    }
                    AxonGUI.EndBoxPadded();
                }

                AxonGUI.Indent--;
            }

            GUI.color = AxonColor.Default;
            AxonGUI.Space();
        }

        public void TransitionGUI()
        {
            if (target.EditorShowTransitions && target.Sets != null && target.Sets.Count > 0) {
                AxonGUI.BeginVertical("box");

                int i = 0;
                while (true) {
                    AxonGUI.BeginHorizontal();
                    for (int x = 0; x < 5; x++) {
                        if (i >= target.Sets.Count) break;
                        GUI.backgroundColor = i == target.From ? Color.green : i == target.To ? Color.red : Color.gray;
                        if (AxonGUI.Button(target.Sets[i].Name, GUILayout.Width(120))) {
                            if (Event.current.control || Event.current.command) {
                                target.StartEdit(i);
                            }
                            else {
                                target.TransitionTo(i);
                            }
                        }
                        GUI.backgroundColor = Color.white;
                        i++;
                    }
                    AxonGUI.EndHorizontal();
                    if (i >= target.Sets.Count) break;
                }
                AxonGUI.EndVertical();

                AxonGUI.Space();
                AxonGUI.BeginHorizontalBox();
                if (target.TransitionDuration < 0f) target.TransitionDuration = 0f;
                float max = target.TransitionDuration > 1f ? target.TransitionDuration : 1f;
                target.TransitionDuration = AxonGUI.FieldSlider(target, "Transition Time", target.TransitionDuration, 0, max);
                target.TransitionDuration = AxonGUI.FieldFloatInline(target, "Seconds", target.TransitionDuration);
                AxonGUI.EndHorizontal();
            }
        }

        public override void OnSceneGUI()
        {
            if (target != null) {
                target.OnDrawGizmos();
            }
        }

    }

}//AxonGenesis

#endif