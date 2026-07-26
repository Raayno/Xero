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
    [CustomEditor(typeof(AlignChildren))]
    public class AlignChildrenEditor : AxonGenesisEditor<AlignChildren, AlignChildrenEdit> { }
    sealed public class AlignChildrenEdit : AxonGenesisBehaviorEdit<AlignChildren>
    {
#if TIMEFLOW_PRO
        public const string kAddAlignChildren = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🧮 Align Children";
#else
        public const string kAddAlignChildren = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Align Children";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Align Children";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAlignChildren, false, 140)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAlignChildren, false, 140)]
        public static void AddAlignChildren()
        {
            ObjectUtil.GetOrAddComponent<AlignChildren>(TimeflowMenu.GetSelectedOrNewGameObject("Align Children"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public AlignChildrenEdit() { }

        public AlignChildrenEdit(AlignChildren _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/align-children";
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void Refresh()
        {
            base.Refresh();
            target.UpdateLayout();
        }

        public override void GUIDropDown(ref GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Rename Children"), false, RenameChildren, (object)target);
            menu.AddItem(new GUIContent("Sort by Size Ascending"), false, SortSizeAsc, (object)target);
            menu.AddItem(new GUIContent("Sort by Size Descending"), false, SortSizeDesc, (object)target);
        }

        public static void RenameChildren(object target)
        {
            AlignChildren t = (AlignChildren)target;
            t.RenameChildren();
        }

        public static void SortSizeAsc(object target)
        {
            AlignChildren t = (AlignChildren)target;
            t.RenameChildren();
        }

        public static void SortSizeDesc(object target)
        {
            AlignChildren t = (AlignChildren)target;
            t.RenameChildren();
        }

        public override void GUIMenu()
        {
            AxonGUI.SetTooltip("When Auto Update is enabled, any changes made to the parameters below immediately update the layout.");
            AxonGUI.UndoName = "Set Auto Update";
            target.AutoLayout = AxonGUI.FieldToggleInline(target, "Auto Update", target.AutoLayout);
            target.AutoLayoutOnChange = AxonGUI.FieldToggleInline(target, "Update On Change", target.AutoLayoutOnChange);

            AxonGUI.Info("Use Gather Children if children have been added, removed, or rearranged to ensure that the " +
                "stored transforms and randomization value lists match the child list. Note that this operations also stores the initial " +
                "position, rotation, and scale of each child object, so run this operation at a point when the children are placed as desired.");

            AxonGUI.SetTooltip("This regathers all of the child transforms and rebuilds the stored values and randomizations. " +
                "Use this any time the child list has been modified.");
            if (AxonGUI.ButtonInline("Gather Children")) {
                target.GatherChildren(true);
            }

            AxonGUI.SetTooltip("Random seed is applied when gathering the children. Change this value to alter the randomization results.");
            AxonGUI.UndoName = "Set Random Seed";
            target.RandomSeed = AxonGUI.FieldIntInline(target, "Random Seed", target.RandomSeed);

            AxonGUI.SetTooltip("Regenerates the randomization values based on the current seed value. Change the seed and click this button to create variations.");
            AxonGUI.UndoName = "Set Randomize";
            if (AxonGUI.ButtonInline("Randomize")) {
                target.Randomize();
            }
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Apply Positon";
            target.PositionEnabled = AxonGUI.FieldToggle(target, "Apply Position", target.PositionEnabled);
            if (target.PositionEnabled) {
                AxonGUI.UndoName = "Set Position Reverse Order";
                target.PositionReverse = AxonGUI.FieldToggleInline(target, "Reverse Order", target.PositionReverse);

                AxonGUI.UndoName = "Set Position Relative Offset";
                target.PositionRelative = AxonGUI.FieldToggleInline(target, "Relative Offset", target.PositionRelative);

                AxonGUI.UndoName = "Set Position Center";
                target.PositionCenter = AxonGUI.FieldToggleInline(target, "Center", target.PositionCenter);
                if (target.PositionCenter) {
                    AxonGUI.UndoName = "Set Absolute Value";
                    target.PositionAbs = AxonGUI.FieldToggleInline(target, "Abs", target.PositionAbs);
                }
            }
            AxonGUI.EndHorizontal();
            if (target.PositionEnabled) {
                AxonGUI.UndoName = "Set Position All";
                target.Position = AxonGUI.FieldVector3(target, "Position All", target.Position);

                AxonGUI.UndoName = "Set Position Each";
                target.PositionEach = AxonGUI.FieldVector3(target, "Position Each", target.PositionEach);

                AxonGUI.UndoName = "Set Position Randomize";
                target.PositionRandomize = AxonGUI.FieldVector3(target, "Randomize", target.PositionRandomize);

                AxonGUI.BeginHorizontal();
                AxonGUI.Label("Lock", "");
                AxonGUI.UndoName = "Set Position Lock X";
                target.PositionLockX = AxonGUI.FieldToggleInline(target, "X", target.PositionLockX, GUILayout.Width(150));

                AxonGUI.UndoName = "Set Position Lock Y";
                target.PositionLockY = AxonGUI.FieldToggleInline(target, "Y", target.PositionLockY, GUILayout.Width(80));

                AxonGUI.UndoName = "Set Position Lock Z";
                target.PositionLockZ = AxonGUI.FieldToggleInline(target, "Z", target.PositionLockZ, GUILayout.Width(80));
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();


            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Apply Rotation";
            target.RotationEnabled = AxonGUI.FieldToggle(target, "Apply Rotation", target.RotationEnabled);
            if (target.RotationEnabled) {
                AxonGUI.UndoName = "Set Rotation Reverse Order";
                target.RotationReverse = AxonGUI.FieldToggleInline(target, "Reverse Order", target.RotationReverse);

                AxonGUI.UndoName = "Set Rotation Relative Offset";
                target.RotationRelative = AxonGUI.FieldToggleInline(target, "Relative Offset", target.RotationRelative);

                AxonGUI.UndoName = "Set Rotation Center";
                target.RotationCenter = AxonGUI.FieldToggleInline(target, "Center", target.RotationCenter);

                if (target.RotationCenter) {
                    AxonGUI.UndoName = "Set Rotation Absolute Value";
                    target.RotationAbs = AxonGUI.FieldToggleInline(target, "Abs", target.RotationAbs);
                }
            }
            AxonGUI.EndHorizontal();
            if (target.RotationEnabled) {
                AxonGUI.UndoName = "Set Rotate All";
                target.Rotation = AxonGUI.FieldVector3(target, "Rotate All", target.Rotation);

                AxonGUI.UndoName = "Set Rotate Each";
                target.RotationEach = AxonGUI.FieldVector3(target, "Rotate Each", target.RotationEach);

                AxonGUI.UndoName = "Set Rotation Randomize";
                target.RotationRandomize = AxonGUI.FieldVector3(target, "Randomize", target.RotationRandomize);

                AxonGUI.BeginHorizontal();
                AxonGUI.Label("Lock", "");
                AxonGUI.UndoName = "Set Rotation Lock X";
                target.RotationLockX = AxonGUI.FieldToggleInline(target, "X", target.RotationLockX, GUILayout.Width(150));

                AxonGUI.UndoName = "Set Rotation Lock Y";
                target.RotationLockY = AxonGUI.FieldToggleInline(target, "Y", target.RotationLockY, GUILayout.Width(80));

                AxonGUI.UndoName = "Set Rotation Lock Z";
                target.RotationLockZ = AxonGUI.FieldToggleInline(target, "Z", target.RotationLockZ, GUILayout.Width(80));
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();


            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Apply Scale";
            target.ScaleEnabled = AxonGUI.FieldToggle(target, "Apply Scale", target.ScaleEnabled);
            if (target.ScaleEnabled) {
                AxonGUI.UndoName = "Set Scale Reverse Order";
                target.ScaleReverse = AxonGUI.FieldToggleInline(target, "Reverse Order", target.ScaleReverse);

                AxonGUI.UndoName = "Set Scale Relative Offset";
                target.ScaleRelative = AxonGUI.FieldToggleInline(target, "Relative Offset", target.ScaleRelative);

                AxonGUI.UndoName = "Set Scale Center";
                target.ScaleCenter = AxonGUI.FieldToggleInline(target, "Center", target.ScaleCenter);

                if (target.ScaleCenter) {
                    AxonGUI.UndoName = "Set Scale Absolute Value";
                    target.ScaleAbs = AxonGUI.FieldToggleInline(target, "Abs", target.ScaleAbs);
                }
            }
            AxonGUI.EndHorizontal();
            if (target.ScaleEnabled) {
                AxonGUI.UndoName = "Set Scale Uniform";
                target.ScaleUniform = AxonGUI.FieldToggle(target, "Uniform", target.ScaleUniform);
                if (target.ScaleUniform) {
                    AxonGUI.UndoName = "Set Scale All";
                    target.Scale.x = target.Scale.y = target.Scale.z = AxonGUI.FieldFloat(target, "Scale All", target.Scale.x);

                    AxonGUI.UndoName = "Set Scale Each";
                    target.ScaleEach.x = target.ScaleEach.y = target.ScaleEach.z = AxonGUI.FieldFloat(target, "Scale Each", target.ScaleEach.x);

                    AxonGUI.UndoName = "Set Scale Locked";
                    target.ScaleLockZ = target.ScaleLockY = target.ScaleLockX = AxonGUI.FieldToggle(target, "Lock Scale", target.ScaleLockX, GUILayout.Width(150));

                    AxonGUI.UndoName = "Set Scale Randomize";
                    target.ScaleRandomize.x = target.ScaleRandomize.y = target.ScaleRandomize.z = AxonGUI.FieldFloat(target, "Randomize", target.ScaleRandomize.x);
                }
                else {
                    AxonGUI.UndoName = "Set Scale All";
                    target.Scale = AxonGUI.FieldVector3(target, "Scale All", target.Scale);

                    AxonGUI.UndoName = "Set Scale Each";
                    target.ScaleEach = AxonGUI.FieldVector3(target, "Scale Each", target.ScaleEach);

                    AxonGUI.UndoName = "Set Scale Randomize";
                    target.ScaleRandomize = AxonGUI.FieldVector3(target, "Randomize", target.ScaleRandomize);
                }

                if (!target.ScaleUniform) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label("Lock", "");
                    AxonGUI.UndoName = "Set Scale Lock X";
                    target.ScaleLockX = AxonGUI.FieldToggleInline(target, "X", target.ScaleLockX, GUILayout.Width(150));

                    AxonGUI.UndoName = "Set Scale Lock Y";
                    target.ScaleLockY = AxonGUI.FieldToggleInline(target, "Y", target.ScaleLockY, GUILayout.Width(80));

                    AxonGUI.UndoName = "Set Scale Lock Z";
                    target.ScaleLockZ = AxonGUI.FieldToggleInline(target, "Z", target.ScaleLockZ, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();
                }
            }
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            target.EditorShowNaming = AxonGUI.Foldout(target.EditorShowNaming, "Naming");
            if (target.EditorShowNaming) {
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Base Name";
                target.Basename = AxonGUI.FieldText(target, "Base Name", target.Basename);

                AxonGUI.UndoName = "Set Base Name Pad";
                target.BaseamePad = AxonGUI.FieldIntInline(target, "Pad", target.BaseamePad);
                if (AxonGUI.ButtonInline("Rename Children")) {
                    target.RenameChildren();
                }
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                EditorUtil.SetDirty(target);
                if (target.AutoLayout || target.AutoLayoutOnChange) {
                    target.UpdateLayout();
                }
            }
        }
    }

}//AxonGenesis

#endif