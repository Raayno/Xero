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
    [CustomEditor(typeof(Rotator))]
    [CanEditMultipleObjects]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/rotator")]
    public class RotatorEditor : Editor
    {
        public Rotator rotator => (Rotator)target;

        bool eulerChanged;
        bool worldChanged;
        bool physicsChanged;

        public bool IsEditingMultiple => targets != null && targets.Length > 1;

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup();
#if TIMEFLOW_OVERRIDES_DISABLED
#else
            AxonGUI.HelpBox("Use the Transform component to set rotation");
#endif
            GUIMain();
        }


        public void GUIMain()
        {
            GUI(rotator, IsEditingMultiple);
        }

        public static void GUI(Rotator rotator, bool editMultiple, string label = null, bool showOptions = true)
        {
            AxonGUI.BeginHorizontal(GUILayout.ExpandWidth(true));

            // Instead of using serialized property fields, this editor handles multiobject editing
            // explcitly so that the UI formatting doesn't change

            AxonGUI.SetTooltip("The rotation in Euler angles. This overrides the rotation of the Transform component.");
            Vector3 euler = rotator.Euler;

            if (!string.IsNullOrEmpty(label)) {
                AxonGUI.Label(label, "");
            }
            if (AxonGUI.ButtonTexture(rotator.LockX ? AxonUI.Icons.LockOn : AxonUI.Icons.LockOff, "Lock X Rotation")) {
                rotator.LockX = !rotator.LockX;
            }
            AxonGUI.BeginDisabledGroup(rotator.LockX);
            AxonGUI.UndoName = "Set Rotation X";
            euler.x = AxonGUI.FieldFloatInline(rotator, "x", euler.x);
            AxonGUI.EndDisabledGroup();


            if (AxonGUI.ButtonTexture(rotator.LockY ? AxonUI.Icons.LockOn : AxonUI.Icons.LockOff, "Lock Y Rotation")) {
                rotator.LockY = !rotator.LockY;
            }
            AxonGUI.BeginDisabledGroup(rotator.LockY);
            AxonGUI.UndoName = "Set Rotation Y";
            euler.y = AxonGUI.FieldFloatInline(rotator, "y", euler.y);
            AxonGUI.EndDisabledGroup();

            if (AxonGUI.ButtonTexture(rotator.LockZ ? AxonUI.Icons.LockOn : AxonUI.Icons.LockOff, "Lock Z Rotation")) {
                rotator.LockZ = !rotator.LockZ;
            }
            AxonGUI.BeginDisabledGroup(rotator.LockZ);
            AxonGUI.UndoName = "Set Rotation Z";
            euler.z = AxonGUI.FieldFloatInline(rotator, "z", euler.z);
            AxonGUI.EndDisabledGroup();


            bool eulerChanged = euler != rotator.Euler;
            if (eulerChanged) {
                UndoUtil.Undo(rotator.transform, "Set Rotation");
                rotator.Euler = euler;
            }

            bool worldChanged = false;
            bool physicsChanged = false;
            if (showOptions) {
                AxonGUI.UndoName = "Set Use World Space";
                AxonGUI.SetTooltip("Enable this to apply values in absolute world coordinates. Some behaviors such as LookAt require this to be enabled.");
                bool isWorld = AxonGUI.FieldToggleInline(rotator, "World Space", rotator.IsWorldSpace);
                worldChanged = rotator.IsWorldSpace != isWorld;
                rotator.IsWorldSpace = isWorld;

                AxonGUI.UndoName = "Set Use Physics";
                AxonGUI.SetTooltip("If enabled, rotation is set on the Rigibody using MoveRotation. Use this for objects that interact with physics.");
                bool usePhysics = AxonGUI.FieldToggleInline(rotator, "Use Physics", rotator.UsePhysics);
                physicsChanged = rotator.UsePhysics != usePhysics;
                rotator.UsePhysics = usePhysics;
            }
            AxonGUI.EndHorizontal();

            if (eulerChanged || worldChanged || physicsChanged) {
                if (editMultiple) {
                    foreach (GameObject obj in Selection.gameObjects) {
                        if (obj.TryGetComponent<Rotator>(out Rotator r)) {
                            UndoUtil.Undo(obj.transform, "Set Rotation");
                            if (eulerChanged) r.Euler = rotator.Euler;
                            if (worldChanged) r.IsWorldSpace = rotator.IsWorldSpace;
                            if (physicsChanged) r.UsePhysics = rotator.UsePhysics;
                            r.ApplyValue();
                        }
                    }
                }
                else {
                    UndoUtil.Undo(rotator.transform, "Set Rotation");
                    rotator.ApplyValue();
                }
            }
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Flip X")]
        static void FlipX(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Flip X", true);
            rotator.Euler = new Vector3(360f - rotator.Euler.x, rotator.Euler.y, rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Flip Y")]
        static void FlipY(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Flip Y", true);
            rotator.Euler = new Vector3(rotator.Euler.x, 360f - rotator.Euler.y, rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Flip Z")]
        static void FlipZ(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Flip Z", true);
            rotator.Euler = new Vector3(rotator.Euler.x, rotator.Euler.y, 360f - rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Flip All Axes")]
        static void FlipAll(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Flip All Axes", true);
            rotator.Euler = new Vector3(360f - rotator.Euler.x, 360f - rotator.Euler.y, 360f - rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Invert X")]
        static void InvertX(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Invert X", true);
            rotator.Euler = new Vector3(-rotator.Euler.x, rotator.Euler.y, rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Invert Y")]
        static void InvertY(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Invert Y", true);
            rotator.Euler = new Vector3(rotator.Euler.x, -rotator.Euler.y, rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Invert Z")]
        static void InvertZ(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Invert Z", true);
            rotator.Euler = new Vector3(rotator.Euler.x, rotator.Euler.y, -rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Invert All")]
        static void InvertAll(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Invert All Axes", true);
            rotator.Euler = new Vector3(-rotator.Euler.x, -rotator.Euler.y, -rotator.Euler.z);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Round Values")]
        static void RoundAxes(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Round Values", true);
            rotator.Euler = new Vector3(Mathf.Round(rotator.Euler.x), Mathf.Round(rotator.Euler.y), Mathf.Round(rotator.Euler.z));
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Wrap 90")]
        static void Wrap90(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Wrap 90", true);
            rotator.Euler = MathUtil.Wrap90(rotator.Euler);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Wrap 180")]
        static void Wrap180(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Wrap 180", true);
            rotator.Euler = MathUtil.Wrap180(rotator.Euler);
        }

        [UnityEditor.MenuItem("CONTEXT/Rotator/Wrap 360")]
        static void Wrap360(MenuCommand command)
        {
            Rotator rotator = (Rotator)command.context;
            UndoUtil.Undo(rotator, "Wrap 360", true);
            rotator.Euler = MathUtil.Wrap360(rotator.Euler);
        }

    }

}//AxonGenesis 

#endif