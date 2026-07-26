// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    public static class TimeflowCommands
    {
        public static bool DebugEnabled = false;

        private static List<string> _Commands = null;

        public static List<string> Commands {
            get {
                if (_Commands == null) RebuildCommands();
                return _Commands;
            }
        }

        public static void RebuildCommands()
        {
            _Commands = new List<string>();

            FieldInfo[] fields = typeof(TimeflowCommands).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields) {
                if (field.IsInitOnly && field.IsStatic) {
                    string cmd = field.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(cmd)) _Commands.Add(cmd);
                }
            }
        }


        #region PLAYBACK CONTROLS

        [Shortcut(TimeflowShortcutInfo.Path_TogglePlay, KeyCode.Space)]
        public static void TogglePlay()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.TogglePlay();
        }

        [Shortcut(TimeflowShortcutInfo.Path_TogglePlayReverse)]
        public static void TogglePlayReverse()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.PlayReverse(false);
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleContinuousPlay, KeyCode.Space, ShortcutModifiers.Action)]
        public static void ToggleContinuousPlay()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.ToggleContinuousPlay();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToPreviousFrame, KeyCode.PageUp)]
        public static void GotoPreviousFrame()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.GotoPreviousFrame();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToPreviousSnapTime, KeyCode.PageUp, ShortcutModifiers.Action)]
        public static void GotoPreviousSnapTime()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoPreviousSnap();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToPreviousCustomStep, KeyCode.PageUp, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void GotoPreviousStepCustom()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoPreviousCustomStep();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToPreviousKeyframe, KeyCode.PageUp, ShortcutModifiers.Shift)]
        public static void GotoPreviousKeyframe()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoPreviousKeyframe();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToPreviousMarker, KeyCode.PageUp, ShortcutModifiers.Alt)]
        public static void GotoPreviousMarker()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoPreviousMarker();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToNextFrame, KeyCode.PageDown)]
        public static void GotoNextFrame()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.GotoNextFrame();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToNextSnapTime, KeyCode.PageDown, ShortcutModifiers.Action)]
        public static void GotoNextSnapTime()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoNextSnap();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToNextCustomStep, KeyCode.PageDown, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void GotoNextStepCustom()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoNextCustomStep();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToNextKeyframe, KeyCode.PageDown, ShortcutModifiers.Shift)]
        public static void GotoNextKeyframe()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoNextKeyframe();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToNextMarker, KeyCode.PageDown, ShortcutModifiers.Alt)]
        public static void GotoNextMarker()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoNextMarker();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToStart, KeyCode.Home)]
        public static void GotoStart()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.GotoStart();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToEnd, KeyCode.End)]
        public static void GotoEnd()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.GotoEnd();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToStartGlobal, KeyCode.Home, ShortcutModifiers.Alt)]
        public static void GotoStartGlobal()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.GotoStart(false);
        }

        public const string _GotoEndGlobal = "Timeflow/Go to End (Global)";
        [Shortcut(TimeflowShortcutInfo.Path_GoToEndGlobal, KeyCode.End, ShortcutModifiers.Alt)]
        public static void GotoEndGlobal()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.GotoEnd(false);
        }

        #endregion

        #region SCROLL

        [Shortcut(TimeflowShortcutInfo.Path_ScrollZoomOut, typeof(TimeflowWindow), KeyCode.Minus)]
        public static void ScrollZoomOut()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.ScrollZoomOut();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ScrollZoomOutAlternate, typeof(TimeflowWindow), KeyCode.KeypadMinus)]
        public static void ScrollZoomOutAlt()
        {
            ScrollZoomOut();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ScrollZoomIn, typeof(TimeflowWindow), KeyCode.Equals)]
        public static void ScrollZoomIn()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.ScrollZoomIn();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ScrollZoomInAlternate, typeof(TimeflowWindow), KeyCode.KeypadPlus)]
        public static void ScrollZoomInAlt()
        {
            ScrollZoomIn();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ScrollZoomToggle, typeof(TimeflowWindow), KeyCode.Semicolon)]
        public static void ScrollZoomToggle()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.ScrollZoomToggle();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleLocalTimeScope, typeof(TimeflowWindow), KeyCode.S)]
        public static void ToggleLocalTimeScope()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.ToggleLocalTimeScope();
        }

        #endregion

        #region SELECTION

        [Shortcut(TimeflowShortcutInfo.Path_SelectAll, typeof(TimeflowWindow), KeyCode.A, ShortcutModifiers.Action)]
        public static void SelectAll()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SelectAll();
        }

        [Shortcut(TimeflowShortcutInfo.Path_DeselectAll, KeyCode.BackQuote)]
        public static void DeselectAll()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.DeselectAll();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetStartOfSelectedTracks, typeof(TimeflowWindow), KeyCode.I, ShortcutModifiers.Alt)]
        public static void SetStartOfSelection()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SetStartOfSelection();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToStartOfSelection, typeof(TimeflowWindow), KeyCode.I)]
        public static void GotoStartOfSelection()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoStartOfSelection();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetEndOfSelectedTracks, typeof(TimeflowWindow), KeyCode.O, ShortcutModifiers.Alt)]
        public static void SetEndOfSelection()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SetEndOfSelection();
        }

        [Shortcut(TimeflowShortcutInfo.Path_GoToEndOfSelection, typeof(TimeflowWindow), KeyCode.O)]
        public static void GotoEndOfSelection()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GotoEndOfSelection();
        }

        #endregion

        #region STANDARD OPERATIONS

        [Shortcut(TimeflowShortcutInfo.Path_DuplicateSelected, typeof(TimeflowWindow), KeyCode.D, ShortcutModifiers.Action)]

        public static void DuplicateSelected()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.DuplicateSelection();
        }

        #endregion


        #region GRID

        [Shortcut(TimeflowShortcutInfo.Path_ToggleGrid, typeof(TimeflowWindow), KeyCode.R)]
        public static void ToggleGrid()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GridEnabled = !Timeflow.Active.View.GridEnabled;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DecreaseGrid, typeof(TimeflowWindow), KeyCode.Alpha1)]
        public static void DecreaseGrid()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GridSnap--;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DecreaseGridAlternate, typeof(TimeflowWindow), KeyCode.Keypad1)]
        public static void DecreaseGridAlternate()
        {
            DecreaseGrid();
        }

        [Shortcut(TimeflowShortcutInfo.Path_IncreaseGrid, typeof(TimeflowWindow), KeyCode.Alpha2)]
        public static void IncreaseGrid()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.GridSnap++;
        }

        [Shortcut(TimeflowShortcutInfo.Path_IncreaseGridAlternate, typeof(TimeflowWindow), KeyCode.Keypad2)]
        public static void IncreaseGridAlternate()
        {
            IncreaseGrid();
        }

        #endregion

        #region ENABLED STATES

        [Shortcut(TimeflowShortcutInfo.Path_SelectionToggleEnabled, typeof(TimeflowWindow), KeyCode.Alpha0)]
        public static void ToggleEnabled()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SelectionToggleEnabled();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectionToggleEnabledAlternate, typeof(TimeflowWindow), KeyCode.Keypad0)]
        public static void ToggleEnabledAlt()
        {
            ToggleEnabled();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectionToggleLocked, typeof(TimeflowWindow), KeyCode.Alpha0, ShortcutModifiers.Action)]
        public static void ToggleLocked()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SelectionToggleLocked();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectionToggleLockedAlternate, typeof(TimeflowWindow), KeyCode.Keypad0, ShortcutModifiers.Action)]
        public static void ToggleLockedAlt()
        {
            ToggleLocked();
        }


        #endregion

        #region WORK AREA & LOOP

        [Shortcut(TimeflowShortcutInfo.Path_ToggleWorkArea, typeof(TimeflowWindow), KeyCode.W)]
        public static void ToggleWorkArea()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.ToggleWorkArea();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleLoop, typeof(TimeflowWindow), KeyCode.L)]
        public static void ToggleLoop()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.ToggleLoop();
        }

        [Shortcut(TimeflowShortcutInfo.Path_LoopSelected, typeof(TimeflowWindow), KeyCode.L, ShortcutModifiers.Action)]
        public static void LoopSelected()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.LoopSelected();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetWorkAreaToSelected, typeof(TimeflowWindow), KeyCode.W, ShortcutModifiers.Shift)]
        public static void SetWorkAreaToSelected()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SetWorkAreaWithSelected();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetWorkAreaStart, typeof(TimeflowWindow), KeyCode.B)]
        public static void SetWorkAreaStart()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.SetWorkAreaStart();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetWorkAreaStartKeepDuration, typeof(TimeflowWindow), KeyCode.B, ShortcutModifiers.Shift)]
        public static void SetWorkAreaStartKeepDuration()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.SetWorkAreaStartKeepDuration();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetWorkAreaEnd, typeof(TimeflowWindow), KeyCode.N)]
        public static void SetWorkAreaEnd()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.SetWorkAreaEnd();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetWorkAreaEndKeepDuration, typeof(TimeflowWindow), KeyCode.N, ShortcutModifiers.Shift)]
        public static void SetWorkAreaEndKeepDuration()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.SetWorkAreaKeepDuration();
        }

        #endregion

        #region KEYFRAMES

        [Shortcut(TimeflowShortcutInfo.Path_AddKeyframe, KeyCode.K)]
        public static void AddKeyframe()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.AddKeyframeOnSelectedChannels();
        }

        [Shortcut(TimeflowShortcutInfo.Path_CopySelection, typeof(TimeflowWindow), KeyCode.C, ShortcutModifiers.Action)]
        public static void CopyKeyframes()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.CopyKeyframes();
        }

        [Shortcut(TimeflowShortcutInfo.Path_CutSelection, typeof(TimeflowWindow), KeyCode.X, ShortcutModifiers.Action)]
        public static void CutKeyframes()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.CutKeyframes();
        }

        [Shortcut(TimeflowShortcutInfo.Path_PasteAtCurrentTime, typeof(TimeflowWindow), KeyCode.V, ShortcutModifiers.Action)]
        public static void PasteKeyframesAtCurrentTime()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.PasteKeysAtCurrentTime();
        }

        [Shortcut(TimeflowShortcutInfo.Path_PastePreserveTime, typeof(TimeflowWindow), KeyCode.V, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void PasteKeyframesPreserveTime()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.PasteKeysPreserveTime();
        }

        [Shortcut(TimeflowShortcutInfo.Path_PasteTangentsOnly, typeof(TimeflowWindow), KeyCode.V, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        public static void PasteKeyframeTangents()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.PasteKeyTangents();
        }

        [Shortcut(TimeflowShortcutInfo.Path_TangentsTool, typeof(TimeflowWindow), KeyCode.C)]
        public static void TangentsTool()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.Input.ToggleTangentsTool();
        }

        [Shortcut(TimeflowShortcutInfo.Path_KeysOnlyTool, typeof(TimeflowWindow), KeyCode.C, ShortcutModifiers.Shift)]
        public static void KeysOnlyTool()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.Input.SelectKeysOnlyTool();
        }

        [Shortcut(TimeflowShortcutInfo.Path_JoinSelectedTracks, typeof(TimeflowWindow), KeyCode.J, ShortcutModifiers.Action)]
        public static void JoinSelectedTracks()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.JoinSelectedTracks();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SplitSelectedTracksAtCurrentTime, typeof(TimeflowWindow), KeyCode.J, ShortcutModifiers.Alt)]
        public static void SplitSelectedTracks()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SplitSelectedTracksAtTime(Timeflow.Active.CurrentTime);
        }

        [Shortcut(TimeflowShortcutInfo.Path_SplitSelectedTracksByWorkArea, typeof(TimeflowWindow), KeyCode.J, ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        public static void SplitSelectedTracksByWorkArea()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SplitSelectedTracksByWorkArea();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SetSelectedTracksToWorkArea, typeof(TimeflowWindow), KeyCode.J, ShortcutModifiers.Shift | ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        public static void SetSelectedTracksToWorkArea()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SetSelectedTracksToWorkArea();
        }

        #endregion

        #region VIEW

        [Shortcut(TimeflowShortcutInfo.Path_RenameSelectedObject, typeof(TimeflowWindow), KeyCode.R, ShortcutModifiers.Action)]
        public static void RenameSelectedObject()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.Input.RenameSelectedObject();
        }

        [Shortcut(TimeflowShortcutInfo.Path_Fit, typeof(TimeflowWindow), KeyCode.F)]
        public static void FitView()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.Input.FitView(false);
        }

        [Shortcut(TimeflowShortcutInfo.Path_FitGraphAuto, typeof(TimeflowWindow), KeyCode.F, ShortcutModifiers.Action)]
        public static void FitViewAuto()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.IsGraphAutoFit = !Timeflow.Active.View.IsGraphAutoFit;
        }

        [Shortcut(TimeflowShortcutInfo.Path_FitTimeOnly, typeof(TimeflowWindow), KeyCode.F, ShortcutModifiers.Shift)]
        public static void FitTime()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.Input.FitView(true);
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleGraphTrackMode, typeof(TimeflowWindow), KeyCode.G)]
        public static void ToggleGraphMode()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.IsGraphMode = !Timeflow.Active.View.IsGraphMode;
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleGraphLock, typeof(TimeflowWindow), KeyCode.L, ShortcutModifiers.Alt)]
        public static void ToggleGraphLock()
        {
            if (Timeflow.Active == null) return;
            //if (DebugEnabled) Debug.Log(_ToggleGraphLock);
            Timeflow.Active.View.ToggleGraphLock();
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleMarkers, typeof(TimeflowWindow), KeyCode.M)]
        public static void ToggleMarkers()
        {
            if (Timeflow.Active == null) return;
            //if (DebugEnabled) Debug.Log(_ToggleMarkers);
            Timeflow.Active.ShowMarkers = !Timeflow.Active.ShowMarkers;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddMarkerAtCurrentTime, typeof(TimeflowWindow), KeyCode.M, ShortcutModifiers.Action)]
        public static void AddMarker()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.AddMarker(Timeflow.Active.CurrentTime);
        }

        [Shortcut(TimeflowShortcutInfo.Path_SnapTime, typeof(TimeflowWindow), KeyCode.H)]
        public static void ToggleSnapTime()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SnapTimeEnabled = !Timeflow.Active.View.SnapTimeEnabled;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SnapValue, typeof(TimeflowWindow), KeyCode.J)]
        public static void ToggleSnapValue()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SnapValueEnabled = !Timeflow.Active.View.SnapValueEnabled;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SnapTimesOfSelectedKeyframesQuantize, typeof(TimeflowWindow), KeyCode.U, ShortcutModifiers.Action)]
        public static void SnapTimesOfSelectedKeyframesQuantize()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SnapTimeOfSelectedKeyframes();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SnapValuesOfSelectedKeyframesQuantize, typeof(TimeflowWindow), KeyCode.U, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void SnapValuesOfSelectedKeyframesQuantize()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.SnapValuesOfSelectedKeyframes();
        }

        #endregion

        #region ALIGN TOOLS


        [Shortcut(TimeflowShortcutInfo.Path_ToggleKeyframeBoundingBox, typeof(TimeflowWindow), KeyCode.T)]
        public static void ToggleKeyframeBoundingBox()
        {
            if (Timeflow.Active == null) return;
            Timeflow.Active.View.AlignTools.IsEnabled = !Timeflow.Active.View.AlignTools.IsEnabled;

        }
        #endregion

    }

}//AxonGenesis

#endif
