// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [System.Serializable]
    public class TimeflowChannelLoopPreset
    {
        public bool ApplyLoopPreset = false;
        public bool EnableLoop = false;
        public bool AutoLoop = false;
        public bool LoopIn = true;
        public bool LoopOut = true;
        public float LoopStart = 0;
        public float LoopEnd = 10;
        public float LoopLimit;
        public bool LoopPingPong;
        public bool LoopMatchEnds;
        public bool EditorShowLoop = true;

        public virtual void Apply(TimeflowChannel target)
        {
            if (target == null || !ApplyLoopPreset) return;
            target.EnableLoop = EnableLoop;
            target.EnableAutoLoop = AutoLoop;
            target.EnableLoopIn = LoopIn;
            target.EnableLoopOut = LoopOut;
            target.LoopStart = LoopStart;
            target.LoopEnd = LoopEnd;
            target.LoopLimit = LoopLimit;
            target.LoopPingPong = LoopPingPong;
            target.LoopMatchEnds = LoopMatchEnds;
        }

        public virtual void Get(TimeflowChannel target)
        {
            if (target == null) return;
            EnableLoop = target.EnableLoop;
            AutoLoop = target.EnableAutoLoop;
            LoopIn = target.EnableLoopIn;
            LoopOut = target.EnableLoopOut;
            LoopStart = target.LoopStart;
            LoopEnd = target.LoopEnd;
            LoopLimit = target.LoopLimit;
            LoopPingPong = target.LoopPingPong;
            LoopMatchEnds = target.LoopMatchEnds;
        }

        public void OnGUI(ComponentPreset target)
        {
            GUI.color = ApplyLoopPreset ? ComponentPresetWindow.SelectionColor : Color.white;

            AxonGUI.SetLabelWidth(120);

            AxonGUI.BeginHorizontal();
            EditorShowLoop = AxonGUI.Foldout(EditorShowLoop, "Loop Settings");
            ApplyLoopPreset = AxonGUI.FieldToggleInline(target, "Apply", ApplyLoopPreset);
            AxonGUI.EndHorizontal();
            if (!EditorShowLoop || !ApplyLoopPreset) return;
            AxonGUI.BeginBoxPadded();
            EnableLoop = AxonGUI.FieldToggle(target, "Loop", EnableLoop);
            if (EnableLoop) {
                AxonGUI.BeginHorizontal();
                AxonGUI.Label("Loop Duration", GUILayout.Width(120));
                AutoLoop = AxonGUI.FieldToggleInline(target, "Auto", AutoLoop);
                if (!AutoLoop) {
                    LoopStart = AxonGUI.FieldFloatInline(target, "Start Time", LoopStart);
                    LoopEnd = AxonGUI.FieldFloatInline(target, "End Time", LoopEnd);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                LoopLimit = AxonGUI.FieldFloat(target, "Limit", LoopEnd);
                LoopIn = AxonGUI.FieldToggleInline(target, "Loop In", LoopIn);
                LoopOut = AxonGUI.FieldToggleInline(target, "Loop Out", LoopOut);
                LoopMatchEnds = AxonGUI.FieldToggleInline(target, "Match Ends", LoopMatchEnds);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                LoopPingPong = AxonGUI.FieldToggle(target, "Ping Pong", LoopPingPong);
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBoxPadded();

            GUI.color = Color.white;
        }
    }

}//AxonGenesis

#endif