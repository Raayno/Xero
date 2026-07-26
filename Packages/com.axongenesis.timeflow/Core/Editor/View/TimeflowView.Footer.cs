// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        private const int _footerIconPad = 4;
        private const int _channelLoopFieldTop = 4;
        private const int _attributeIconPad = 8;

        public void GUIFooter()
        {
            if (Layout.WindowHeight < 80) return;
            GUI.Box(Layout.Footer, "", AxonUI.ToolbarBoxStyle);

            float w = Layout.Footer.Rect.width;
            int lockWidth = 45;

            GUIRect rect = new GUIRect();
            if (IsGraphLocked) {
                rect = new GUIRect(Layout.Footer.Rect.x + Layout.Footer.Width - lockWidth, Layout.Footer.Rect.y, lockWidth + 20, 34);
                GUI.color = AxonColor.DarkerGrey;
                GUI.Box(rect, "", AxonUI.SolidStyle);

                rect = Layout.TimeAreaInner;
                GUI.Box(rect, "", AxonUI.TrackSelectedStyle);
                GUI.color = AxonColor.Default;
            }

            GUIRect area = Layout.Footer.Rect;
            area.width -= lockWidth;
            GUILayout.BeginArea(area);
            GUI.enabled = true;
            GUI.color = AxonColor.Default;

            rect = new GUIRect(_footerIconPad, _footerIconPad, TimeflowViewLayout.LargeIconSize, TimeflowViewLayout.LargeIconSize);
            rect = GUIFooter_ToggleTrackGraphView(rect);

            if (!IsGraphMode) {
                rect = GUIFooter_KeyframeSelectionModeToggle(rect);
            }
            else {
                rect = GUIFooter_GraphEditModeToggle(rect);
            }
            rect.x += rect.width + _toolIconSpacing;

            rect = GUIFooter_FitView(rect);
            rect = GUIFooter_AlignToolsToggle(rect);
            rect = GUIFooter_AlignTools(rect);
            rect = GUIFooter_ChannelInterpolationToolbar(rect);
            rect = GUIFooter_ChannelLoopToolbar(rect);

            GUILayout.EndArea();

            rect = new GUIRect(Layout.Footer.Rect.x + Layout.Footer.Width - 18, Layout.Footer.Rect.y + _footerIconPad * 2, TimeflowViewLayout.SmallIconSize, TimeflowViewLayout.SmallIconSize);
            GUIFooter_ToggleTrackGraphLock(rect);

            if (IsGraphMode) {
                rect.x -= 20;
                GUIFooter_ToggleTrackGraphSolo(rect);
            }
            if (Layout.Footer.Width < 300) {
                rect.x = Layout.Footer.Width - 30;
                rect.width = rect.height = TimeflowViewLayout.LargeIconSize;
                GUI.Label(rect, new GUIContent("..."));
            }
            else {
                rect.x = Layout.Footer.Width - 175;
            }
        }

        private GUIRect GUIFooter_FitView(GUIRect rect)
        {
            if (GUI.Button(rect, AxonUI.FitViewLabel, IsGraphAutoFit ? AxonUI.FitViewAutoStyle : AxonUI.FitViewStyle)) {
                if (IsControl) {
                    IsGraphAutoFit = !IsGraphAutoFit;
                }
                else {
                    Input.FitView(IsShift);
                }
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_AlignToolsToggle(GUIRect rect)
        {
            if (GUI.Button(rect, AxonUI.AlignToolsLabel, AlignTools.IsEnabled ? AxonUI.AlignToolsOnStyle : AxonUI.AlignToolsOffStyle)) {
                AlignTools.IsEnabled = !AlignTools.IsEnabled;
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_ToggleTrackGraphView(GUIRect rect)
        {
            if (GUI.Button(rect, AxonUI.GraphViewLabel, IsGraphMode ? AxonUI.GraphViewOnStyle : AxonUI.GraphViewOffStyle)) {
                IsGraphMode = !IsGraphMode;
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_ToggleTrackGraphSolo(GUIRect rect)
        {
            if (!IsGraphMode) return rect;
            EditorGUI.BeginDisabledGroup(!IsGraphMode);
            if (GUI.Button(rect, AxonUI.GraphSoloLabel, IsGraphSolo ? AxonUI.DisplayChannelSoloOnStyle : AxonUI.DisplayChannelSoloOffStyle)) {
                ToggleGraphSolo();
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_ToggleTrackGraphLock(GUIRect rect)
        {
            if (GUI.Button(rect, AxonUI.GraphLockLabel, IsGraphLocked ? AxonUI.GraphLockOnStyle : AxonUI.GraphLockOffStyle)) {
                ToggleGraphLock();
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_KeyframeSelectionModeToggle(GUIRect rect)
        {
            if (SelectionMode == SelectionModes.Any) {
                AxonUI.KeySelectModeLabel.tooltip = "Select Keyframes and Tracks";
                if (GUI.Button(rect, AxonUI.KeySelectModeLabel, AxonUI.KeySelectModeAllStyle)) {
                    SelectionMode = SelectionModes.KeyframesOnly;
                    ApplySelectionModeToSelection();
                }
            }
            else
            if (SelectionMode == SelectionModes.KeyframesOnly) {
                AxonUI.KeySelectModeLabel.tooltip = "Select Keyframes Only";
                if (GUI.Button(rect, AxonUI.KeySelectModeLabel, AxonUI.KeySelectModeKeysStyle)) {
                    SelectionMode = SelectionModes.TracksOnly;
                }
            }
            else
            if (SelectionMode == SelectionModes.TracksOnly) {
                AxonUI.KeySelectModeLabel.tooltip = "Select Tracks Only";
                if (GUI.Button(rect, AxonUI.KeySelectModeLabel, AxonUI.KeySelectModeTracksStyle)) {
                    SelectionMode = SelectionModes.Any;
                    ApplySelectionModeToSelection();
                }
            }
            return rect;
        }

        private GUIRect GUIFooter_GraphEditModeToggle(GUIRect rect)
        {
            if (Input.GraphEditMode == TimeflowViewInput.GraphEditModes.TangentsOnly) {
                AxonUI.ToolSelectLabel.tooltip = "Tangent Tool";
                if (GUI.Button(rect, AxonUI.ToolEditTangentsLabel, AxonUI.ToolEditTangentsStyle)) {
                    Input.SetGraphEditMode(IsShift ? TimeflowViewInput.GraphEditModes.KeysOnly : TimeflowViewInput.GraphEditModes.All);
                }
            }
            else
            if (Input.GraphEditMode == TimeflowViewInput.GraphEditModes.KeysOnly) {
                AxonUI.ToolSelectLabel.tooltip = "Keys Only Tool";
                if (GUI.Button(rect, AxonUI.ToolEditKeysOnlyLabel, AxonUI.ToolsEditKeysOnlyStyle)) {
                    Input.SetGraphEditMode(IsShift ? TimeflowViewInput.GraphEditModes.TangentsOnly : TimeflowViewInput.GraphEditModes.All);
                }
            }
            else
            if (Input.GraphEditMode == TimeflowViewInput.GraphEditModes.All) {
                AxonUI.ToolSelectLabel.tooltip = "Select Tool";
                if (GUI.Button(rect, AxonUI.ToolSelectLabel, AxonUI.ToolSelectStyle)) {
                    Input.SetGraphEditMode(IsShift ? TimeflowViewInput.GraphEditModes.KeysOnly : TimeflowViewInput.GraphEditModes.TangentsOnly);
                }
            }
            return rect;
        }

        private GUIRect GUIFooter_ChannelLoopToolbar(GUIRect rect)
        {
            if (SelectedChannels == null || SelectedChannels.Count == 0) {
                return rect;
            }

            #region LOOP SETTINGS
            rect.width = rect.height = TimeflowViewLayout.LargeIconSize;

            int loopMode = GetChannelLoopMode();
            if (loopMode == -1) {
                AxonUI.ChannelLoopLabel.tooltip = "Loop (Mixed Values)";
                if (GUI.Button(rect, AxonUI.ChannelLoopLabel, AxonUI.ChannelLoopHalfStyle)) {
                    SetLoopModeForSelectedChannels(1);
                }
            }
            else
            if (loopMode == 0) {
                AxonUI.ChannelLoopLabel.tooltip = "Loop Off";
                if (GUI.Button(rect, AxonUI.ChannelLoopLabel, AxonUI.ChannelLoopOffStyle)) {
                    SetLoopModeForSelectedChannels(1);
                }
            }
            else
            if (loopMode == 1) {
                AxonUI.ChannelLoopLabel.tooltip = "Loop On";
                if (GUI.Button(rect, AxonUI.ChannelLoopLabel, AxonUI.ChannelLoopOnStyle)) {
                    SetLoopModeForSelectedChannels(2);
                }
            }
            else
            if (loopMode == 2) {
                AxonUI.ChannelLoopLabel.tooltip = "Loop Ping Pong";
                if (GUI.Button(rect, AxonUI.ChannelLoopLabel, AxonUI.ChannelLoopPingPongStyle)) {
                    SetLoopModeForSelectedChannels(0);
                }
            }
            rect.x += rect.width + _toolIconSpacing;
            if (loopMode != 0) {
                bool loopAuto = GetChannelLoopAuto();
                if (loopAuto) {
                    if (GUI.Button(rect, AxonUI.ChannelLoopAutoOnLabel, AxonUI.ChannelLoopAutoOnStyle)) {
                        SetAutoLoopForSelectedChannels(false);
                    }
                }
                else {
                    if (GUI.Button(rect, AxonUI.ChannelLoopAutoOffLabel, AxonUI.ChannelLoopAutoOffStyle)) {
                        SetAutoLoopForSelectedChannels(true);
                    }
                }
                rect.x += rect.width + _toolIconSpacing;

                bool loopIn = GetChannelLoopIn();
                if (loopIn) {
                    if (GUI.Button(rect, AxonUI.ChannelLoopInOnLabel, AxonUI.ChannelLoopInOnStyle)) {
                        SetLoopInForSelectedChannels(false);
                    }
                }
                else {
                    if (GUI.Button(rect, AxonUI.ChannelLoopInOffLabel, AxonUI.ChannelLoopInOffStyle)) {
                        SetLoopInForSelectedChannels(true);
                    }
                }
                rect.x += rect.width + _toolIconSpacing;

                bool loopOut = GetChannelLoopOut();
                if (loopOut) {
                    if (GUI.Button(rect, AxonUI.ChannelLoopOutOnLabel, AxonUI.ChannelLoopOutOnStyle)) {
                        SetLoopOutForSelectedChannels(false);
                    }
                }
                else {
                    if (GUI.Button(rect, AxonUI.ChannelLoopOutOffLabel, AxonUI.ChannelLoopOutOffStyle)) {
                        SetLoopOutForSelectedChannels(true);
                    }
                }
                rect.x += rect.width + _toolIconSpacing;

                int loopMatch = GetChannelLoopMatchMode();
                if (loopMode != 0) {
                    if (loopMatch == 1) {
                        if (GUI.Button(rect, AxonUI.ChannelLoopMatchLabel, AxonUI.ChannelLoopMatchStyle)) {
                            SetLoopMatchModeForSelectedChannels(0);
                        }
                    }
                    else
                    if (loopMatch == 0) {
                        if (GUI.Button(rect, AxonUI.ChannelLoopMatchLabel, AxonUI.ChannelLoopFreeStyle)) {
                            SetLoopMatchModeForSelectedChannels(1);
                        }
                    }
                    else {
                        GUI.color = new Color(1f, 1f, 1f, 0.4f);
                        if (GUI.Button(rect, AxonUI.ChannelLoopMatchLabel, AxonUI.ChannelLoopFreeStyle)) {
                            SetLoopMatchModeForSelectedChannels(1);
                        }
                        GUI.color = new Color(1f, 1f, 1f, 1f);
                    }
                    rect.x += rect.width + _toolIconSpacing;
                }

                if (loopMode > 0) {
                    float limit = GetChannelLoopLimit();
                    rect.y = _channelLoopFieldTop;
                    rect.height = _channelLoopFieldHeight;
                    rect.width = _channelLoopFieldWidth;
                    EditorGUIUtility.labelWidth = 35;
                    float a = EditorGUI.FloatField(rect, "Limit", limit);
                    if (a != limit) {
                        if (a < 0f) a = 0f;
                        SetLoopLimitForSelectedChannels(a);
                    }
                    rect.x += rect.width + _toolIconSpacing;
                }
            }
            #endregion

            return rect;
        }

        private GUIRect GUIFooter_ChannelInterpolationToolbar(GUIRect rect)
        {
            if (SelectedChannels == null || SelectedChannels.Count == 0) return rect;

            rect.width = rect.height = TimeflowViewLayout.LargeIconSize;
            GUI.Button(rect, GUIContent.none, AxonUI.ToolbarDivider);
            rect.x += rect.width + _toolIconSpacing;

            #region CHANNEL INTERPOLATION

            bool anyBezier = false;
            bool anyCanInterpolate = true;
            bool anyCanHold = true;
            bool anyAutoTangents = false;
            int attributeCount = 1;
            bool uniformOnly = true;

            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        if (ch.Interpolation == TimeflowChannel.Interpolations.Bezier) {
                            anyBezier = true;
                        }
                        if (ch.IsCombinedValue) {
                            attributeCount = Math.Max(attributeCount, ch.AttributeCount);
                        }
                        if (ch.IsVector) {
                            anyCanInterpolate = true;
                        }
                        if (!ch.CanInterpolate) {
                            anyCanInterpolate = false;
                        }
                        if (!ch.CanHold) {
                            anyCanHold = false;
                        }
                        if (attributeCount > 1 && !ch.IsUniformValue) {
                            uniformOnly = false;
                        }
                    }
                }
            }

            rect = GUIFooter_ChannelInterpolationMode(rect);
            rect = GUIFooter_ChannelAttributeToggles(rect, attributeCount, uniformOnly);

            #endregion

            #region KEYFRAME INTERPOLATION

            bool anyHold = false;
            bool anyLinear = false;
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.IsAutoTangents) {
                        anyAutoTangents = true;
                    }
                    if (k.Hold) {
                        anyHold = true;
                    }
                    if (k.Linear) {
                        anyLinear = true;
                    }
                }
            }

            rect.width = rect.height = TimeflowViewLayout.LargeIconSize;
            if (anyCanInterpolate) {
                rect = GUIFooter_KeyframeLinearToggle(rect, anyLinear);
            }
            if (anyCanHold) {
                rect = GUIFooter_KeyframeHoldToggle(rect, anyHold);
            }

            if (!anyBezier) {
                Input.SetGraphEditMode(TimeflowViewInput.GraphEditModes.All);
            }
            else
            if (anyCanInterpolate) {
                rect = GUIFooter_KeyframeTangentToggles(rect, anyAutoTangents);
                rect = GUIFooter_ChannelTangentOptions(rect);
            }
            #endregion

            if (anyCanInterpolate) {
                rect.width = rect.height = TimeflowViewLayout.LargeIconSize;
                GUI.Button(rect, GUIContent.none, AxonUI.ToolbarDivider);
                rect.x += rect.width + _toolIconSpacing;
            }
            return rect;
        }

        private GUIRect GUIFooter_ChannelInterpolationMode(GUIRect rect)
        {
            TimeflowChannel.Interpolations interp = GetChannelInterpolationOfTargetChannels();
            if (interp == TimeflowChannel.Interpolations.None) {
                if (GUI.Button(rect, AxonUI.InterpChanNoneLabel, AxonUI.InterpChanNone)) {
                    SetInterpolationForTargetChannels(TimeflowChannel.Interpolations.Linear);
                }
            }
            else
            if (interp == TimeflowChannel.Interpolations.Linear) {
                if (GUI.Button(rect, AxonUI.InterpChanLinearLabel, AxonUI.InterpChanLinear)) {
                    SetInterpolationForTargetChannels(TimeflowChannel.Interpolations.Bezier);
                }
            }
            else
            if (interp == TimeflowChannel.Interpolations.Bezier) {
                if (GUI.Button(rect, AxonUI.InterpChanBezierLabel, AxonUI.InterpChanBezier)) {
                    SetInterpolationForTargetChannels(TimeflowChannel.Interpolations.Quadratic);
                }
            }
            else
            if (interp == TimeflowChannel.Interpolations.Quadratic) {
                if (GUI.Button(rect, AxonUI.InterpChanQuadLabel, AxonUI.InterpChanQuad)) {
                    SetInterpolationForTargetChannels(TimeflowChannel.Interpolations.Linear);
                }
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_KeyframeTangentToggles(GUIRect rect, bool anyAutoTangents)
        {
            if (GUI.Button(rect, AxonUI.InterpAutoLabel, anyAutoTangents ? AxonUI.InterpAutoOnStyle : AxonUI.InterpAutoOffStyle)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Auto, !anyAutoTangents);
            }
            rect.x += rect.width + _toolIconSpacing;

            if (GUI.Button(rect, AxonUI.InterpFlatLabel, AxonUI.InterpFlatStyle)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Flat);
            }
            rect.x += rect.width + _toolIconSpacing;

            if (GUI.Button(rect, AxonUI.InterpVerticalLabel, AxonUI.InterpVerticalStyle)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Vertical);
            }
            rect.x += rect.width + _toolIconSpacing;

            if (GUI.Button(rect, AxonUI.InterpFlatLeftLabel, AxonUI.InterpFlatLeftStyle)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.FlatLeft);
            }
            rect.x += rect.width + _toolIconSpacing;

            if (GUI.Button(rect, AxonUI.InterpFlatRightLabel, AxonUI.InterpFlatRightStyle)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.FlatRight);
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_ChannelAttributeToggles(GUIRect rect, int attributeCount, bool uniformOnly)
        {
            if (IsGraphMode && attributeCount > 1) {
                int channelsOn = 0;
                if (ShowChannel0) channelsOn++;
                if (ShowChannel1) channelsOn++;
                if (ShowChannel2) channelsOn++;
                if (ShowChannel3) channelsOn++;
                bool soloToggle = channelsOn == 1;

                rect.x += _toolIconSpacing;
                rect.y = _attributeIconPad;
                rect.width = rect.height = TimeflowViewLayout.SmallIconSize;

                if (!uniformOnly) {
                    if (GUI.Button(rect, AxonUI.ChannelXLabel, ShowChannel0 ? AxonUI.ChannelXOnStyle : AxonUI.ChannelXOffStyle)) {
                        if (IsAlt) {
                            bool on = true;
                            if (ShowChannel0 && soloToggle) on = true;
                            else
                            if (ShowChannel0 && !soloToggle) on = false;
                            else
                            if (!ShowChannel0 && soloToggle) on = false;

                            ShowChannel0 = true;
                            ShowChannel1 = on;
                            ShowChannel2 = on;
                            ShowChannel3 = on;
                        }
                        else ShowChannel0 = !ShowChannel0;
                    }
                    rect.x += rect.width + _toolIconSpacing;

                    if (attributeCount > 1) {
                        if (GUI.Button(rect, AxonUI.ChannelYLabel, ShowChannel1 ? AxonUI.ChannelYOnStyle : AxonUI.ChannelYOffStyle)) {
                            if (IsAlt) {
                                bool on = true;
                                if (ShowChannel1 && soloToggle) on = true;
                                else
                                if (ShowChannel1 && !soloToggle) on = false;
                                else
                                if (!ShowChannel1 && soloToggle) on = false;

                                ShowChannel1 = true;
                                ShowChannel0 = on;
                                ShowChannel2 = on;
                                ShowChannel3 = on;
                            }
                            else ShowChannel1 = !ShowChannel1;
                        }
                        rect.x += rect.width + _toolIconSpacing;
                    }

                    if (attributeCount > 2) {
                        if (GUI.Button(rect, AxonUI.ChannelZLabel, ShowChannel2 ? AxonUI.ChannelZOnStyle : AxonUI.ChannelZOffStyle)) {
                            if (IsAlt) {
                                bool on = true;
                                if (ShowChannel2 && soloToggle) on = true;
                                else
                                if (ShowChannel2 && !soloToggle) on = false;
                                else
                                if (!ShowChannel2 && soloToggle) on = false;
                                ShowChannel2 = true;
                                ShowChannel0 = on;
                                ShowChannel1 = on;
                                ShowChannel3 = on;
                            }
                            else ShowChannel2 = !ShowChannel2;
                        }
                        rect.x += rect.width + _toolIconSpacing;
                    }
                    if (attributeCount > 3) {
                        if (GUI.Button(rect, AxonUI.ChannelALabel, ShowChannel3 ? AxonUI.ChannelAOnStyle : AxonUI.ChannelAOffStyle)) {
                            if (IsAlt) {
                                bool on = true;
                                if (ShowChannel3 && soloToggle) on = true;
                                else
                                if (ShowChannel3 && !soloToggle) on = false;
                                else
                                if (!ShowChannel3 && soloToggle) on = false;
                                ShowChannel3 = true;
                                ShowChannel0 = on;
                                ShowChannel1 = on;
                                ShowChannel2 = on;
                            }
                            else ShowChannel3 = !ShowChannel3;
                        }
                        rect.x += rect.width + _toolIconSpacing;
                    }
                }

                rect.y = _footerIconPad;
                rect.width = rect.height = TimeflowViewLayout.LargeIconSize;
                GUI.Button(rect, GUIContent.none, AxonUI.ToolbarDivider);
                rect.x += rect.width + _toolIconSpacing;
            }
            return rect;
        }

        private GUIRect GUIFooter_KeyframeLinearToggle(GUIRect rect, bool anyLinear)
        {
            if (GUI.Button(rect, AxonUI.InterpLinearLabel, anyLinear ? AxonUI.InterpLinearStyle : AxonUI.InterpLinearStyleOff)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Linear);
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_KeyframeHoldToggle(GUIRect rect, bool anyHold)
        {
            if (GUI.Button(rect, AxonUI.InterpHoldLabel, anyHold ? AxonUI.InterpHoldStyle : AxonUI.InterpHoldStyleOff)) {
                SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Hold);
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;
        }

        private GUIRect GUIFooter_ChannelTangentOptions(GUIRect rect)
        {
            int locked = 0;
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.UnifyTangents) {
                        locked = 1;
                        if (k.UnifyTangentLengths) {
                            locked = 2;
                            break;
                        }
                    }
                }
            }

            GUIStyle lockedStyle = AxonUI.UnifyTangentsOff;
            if (locked == 1) {
                lockedStyle = AxonUI.UnifyTangentsOn;
                AxonUI.UnifiedTangentsLabel.tooltip = "Unified Tangent Angles";
            }
            else
            if (locked == 2) {
                lockedStyle = AxonUI.UnifyTangentLengths;
                AxonUI.UnifiedTangentsLabel.tooltip = "Unified Tangent Equal Length";
            }
            else {
                AxonUI.UnifiedTangentsLabel.tooltip = "Independent Tangents";
            }
            if (GUI.Button(rect, AxonUI.UnifiedTangentsLabel, lockedStyle)) {
                locked++;
                if (locked > 2) locked = 0;
                SetUnifiedTangentsOfSelectedKeyframes(locked);
            }
            rect.x += rect.width + _toolIconSpacing;
            return rect;

        }

        private GUIRect GUIFooter_AlignTools(GUIRect rect)
        {
            return AlignTools.DrawToolbar(rect);
        }



    }

}//AxonGenesis

#endif
