// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(FPSCounter))]
    public class FPSCounterEditor : AxonGenesisEditor<FPSCounter, FPSCounterEdit> { }

    public class FPSCounterEdit : AxonGenesisBehaviorEdit<FPSCounter>
    {
        public override void GUIMenu()
        {
            base.GUIMenu();
            if (target.DisplayProperty == null) target.DisplayProperty = new Property();
            AxonGUI.PropertySelectInline(target, typeof(FPSCounter), target.gameObject, target.DisplayProperty, "Display", false);
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup();

            AxonGUI.BeginBoxPadded();

            AxonGUI.UndoName = "Set Text Prefix";
            AxonGUI.SetTooltip("When displaying to a string value or text field, this optional text is prefixed to the FPS value. For example: 'FPS:60'");
            target.TextPrefix = AxonGUI.FieldText(target, "Text Prefix", target.TextPrefix);

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Target FPS Color";
            target.ShowTargetFPS = AxonGUI.FieldToggle(target, "Target FPS Color", target.ShowTargetFPS);
            if (target.ShowTargetFPS) {
                AxonGUI.SetTooltip("Set the desired FPS.");
                AxonGUI.UndoName = "Set Target FPS";
                target.TargetFPS = AxonGUI.FieldIntInline(target, target.TargetFPS);
                AxonGUI.UndoName = "Set Target FPS Low";
                target.LowFPSColor = AxonGUI.FieldColorInline(target, "Low", target.LowFPSColor, false);
                AxonGUI.UndoName = "Set Target FPS High";
                target.HighFPSColor = AxonGUI.FieldColorInline(target, "High", target.HighFPSColor, false);
            }
            AxonGUI.EndHorizontal();
            if (target.ShowTargetFPS) {
                if (target.ColorProperty == null) target.ColorProperty = new Property();
                AxonGUI.PropertySelect(target, typeof(FPSCounter), target.gameObject, target.ColorProperty, Property.PropertyFilters.ColorOnly, "Set Color", false);
            }
            else {
                target.ColorProperty = null;
            }

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Graph";
            AxonGUI.SetTooltip("Write FPS data to a Graph behavior (available in the editor only). Make sure the target Graph channel matches the name provided.");
            target.GraphFPS = AxonGUI.FieldToggle(target, "Graph", target.GraphFPS);
            if (target.GraphFPS) {
                if (target.GraphName == null) target.GraphName = "";
                AxonGUI.UndoName = "Set Graph Name";
                target.GraphName = AxonGUI.FieldTextInline(target, "Graph Name", target.GraphName);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.EndBoxPadded();
        }
    }

}//AxonGenesis

#endif