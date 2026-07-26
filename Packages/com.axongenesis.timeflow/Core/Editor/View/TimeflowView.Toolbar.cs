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
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region CONSTANTS

        private const int _toolbarIconTopPad = 3;
        private const int _toolbarIconPadLeft = 3;
        private const int _toolbarItemLeftPad = 2;
        private const int _toolbarTimeflowInstancesTopPad = 3;
        private const int _toolbarTimeflowInstancesWidth = 120;
        private const int _toolbarTimeflowColorLabelHeight = 2;

        private const int _toolbarIconSpacing = 24;
        private const int _toolbarTopPadding = 2;
        private const int _toolbarSectionGap = 10;
        private const int _toolbarTimeDisplayModePrimaryTop = 2;
        private const int _toolbarTimeDisplayModePrimaryWidth = 16;
        private const int _toolbarTimeDisplayModePrimaryLeftAdjust = -4;
        private const int _toolbarTimeDisplayModePrimaryHeight = 20;
        private const int _toolbarTimeDisplayModeSecondaryHeight = 12;
        private const int _toolbarTimeDisplayModeSecondaryPadTop = 17;
        private const int _toolbarTimeDisplaymodeSecondaryTop = 15;
        private const int _toolbarTimeDisplayPrimaryWidth = 110;
        private const int _toolbarFPSWidth = 30;
        private const int _toolbarFPSPadLeft = 60;
        private const int _toolbarBPMFieldHeight = 20;
        private const int _toolbarBPMFieldWidth = 30;
        private const int _toolbarBPMLabelHeight = 12;
        private const int _toolbarBPMBeatsPerBarTopOffset = 0;
        private const int _toolbarBPMBeatsPerBarWidth = 120;
        private const int _toolbarBPMBeatsPerBarLabelWidth = 10;
        private const int _toolbarSnapMenuWidth = 55;
        private const int _toolbarGridTimeDisplayMenuWidth = 27;
        private const int _toolbarMarkersRightOffset = 220;
        private const int _toolbarJumpMenuWidth = 75;
        private const int _toolbarJumpMenuRightOffset = 120;
        private const int _toolbarJumpMenuTopPad = 5;
        private const int _toolbarJumpMenuHeight = 16;
        private const int _toolbarPreferencesRightOffset = 38;
        private const int _toolbarMinimizeRightOffset = 18;
        private const int _toolbarMinimizeTopPad = 4;

        #endregion

        #region STATIC

        public static void SetTimeDisplaySeconds()
        {
            Timeflow.Active.View.TimeDisplay = TimeDisplayModes.Seconds;
        }

        public static void SetTimeDisplayFrames()
        {
            Timeflow.Active.View.TimeDisplay = TimeDisplayModes.Frames;
        }

        public static void SetTimeDisplayTimecode()
        {
            Timeflow.Active.View.TimeDisplay = TimeDisplayModes.Timecode;
        }

        public static void SetTimeDisplayMeasures()
        {
            Timeflow.Active.View.TimeDisplay = TimeDisplayModes.Measures;
        }

        public static void SetTimeDisplay2ndSeconds()
        {
            Timeflow.Active.View.TimeDisplay2nd = TimeDisplayModes.Seconds;
        }

        public static void SetTimeDisplay2ndFrames()
        {
            Timeflow.Active.View.TimeDisplay2nd = TimeDisplayModes.Frames;
        }

        public static void SetTimeDisplay2ndTimecode()
        {
            Timeflow.Active.View.TimeDisplay2nd = TimeDisplayModes.Timecode;
        }

        public static void SetTimeDisplay2ndMeasures()
        {
            Timeflow.Active.View.TimeDisplay2nd = TimeDisplayModes.Measures;
        }

        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private Rect _toolbarLogoRect;

        [NonSerialized]
        private Rect _toolbarTimeflowInstancesRect;

        [NonSerialized]
        private Rect _toolbarPlayerRewindRect;

        [NonSerialized]
        private Rect _toolbarTimeflowColorLabelRect;

        [NonSerialized]
        private Rect _toolbarGotoStartRect;

        [NonSerialized]
        private Rect _toolbarGotoPrevRect;

        [NonSerialized]
        private Rect _toolbarPlayRect;

        [NonSerialized]
        private Rect _toolbarPlayReverseRect;

        [NonSerialized]
        private Rect _toolbarGotoNextRect;

        [NonSerialized]
        private Rect _toolbarGotoEndRect;

        [NonSerialized]
        private Rect _toolbarWorkAreaRect;

        [NonSerialized]
        private Rect _toolbarAutoKeyframingRect;

        [NonSerialized]
        private Rect _toolbarLoopRect;

        [NonSerialized]
        private Rect _toolbarTimeScopeRect;

        [NonSerialized]
        private Rect _toolbarTimeScopeLocalizeRect;

        [NonSerialized]
        private Rect _toolbarFollowPlayheadRect;

        [NonSerialized]
        private Rect _toolbarTimeDisplaymodePrimaryRect;

        [NonSerialized]
        private Rect _toolbarTimeDisplayFieldRect;

        [NonSerialized]
        private Rect _toolbarTimeDisplayModeSecondaryRect;

        [NonSerialized]
        private Rect _toolbarTimeDisplayPrimaryRect;

        [NonSerialized]
        private Rect _toolbarTimeDisplaySecondaryRect;

        [NonSerialized]
        private Rect _toolbarFPSRect;

        [NonSerialized]
        private Rect _toolbarMusicalTimingToggleRect;

        [NonSerialized]
        private Rect _toolbarBPMFieldRect;

        [NonSerialized]
        private Rect _toolbarBPMLabelRect;

        [NonSerialized]
        private Rect _toolbarBPMBeatsPerBarRect;

        [NonSerialized]
        private Rect _toolbarBPMBeatsPerBarLabelRect;

        [NonSerialized]
        private Rect _toolbarBPMBeatsNoteSizeRect;

        [NonSerialized]
        private Rect _toolbarGridToggleRect;

        [NonSerialized]
        private Rect _toolbarSnapTimeToggleRect;

        [NonSerialized]
        private Rect _toolbarSnapValueToggleRect;

        [NonSerialized]
        private Rect _toolbarSnapMenuRect;

        [NonSerialized]
        private Rect _toolbarGridTimeDisplayMenuRect;

        [NonSerialized]
        private Rect _toolbarShowDirectorSyncToggleRect;

        [NonSerialized]
        private Rect _toolbarPrefabIconRect;

        [NonSerialized]
        private Rect _toolbarShowMarkersToggleRect;

        [NonSerialized]
        private Rect _toolbarShowKeyframeValuesToggleRect;

        [NonSerialized]
        private Rect _toolbarMarkersPrevRect;

        [NonSerialized]
        private Rect _toolbarMarkersNextRect;

        [NonSerialized]
        private Rect _toolbarMarkerJumpMenuRect;

        [NonSerialized]
        private Rect _toolbarPreferencesRect;

        [NonSerialized]
        private Rect _toolbarMinimizeRect;

        #endregion

        public int InfoHeight { get; set; }

        private void GUIToolbarLayout()
        {
            Layout.Toolbar.Width = WindowWidth;
            int iconTop = _toolbarTopPadding + _toolbarIconTopPad;

            _toolbarLogoRect = new Rect(_toolbarIconPadLeft, iconTop, TimeflowViewLayout.LargeIconSize, TimeflowViewLayout.LargeIconSize);
            if (Timeflow.HasTimeflowParent || Timeflow.IsDisplayingPrefab) {
                _toolbarPrefabIconRect = _toolbarLogoRect;
                _toolbarLogoRect.x += _toolbarLogoRect.width;
            }

            _toolbarTimeflowInstancesRect = _toolbarLogoRect;
            if (!Timeflow.IsDisplayingPrefab && Timeflow.Instances.Count > 1) {
                _toolbarTimeflowInstancesRect.x += TimeflowViewLayout.LargeIconSize + _toolbarItemLeftPad;
                _toolbarTimeflowInstancesRect.y += _toolbarTimeflowInstancesTopPad;
                _toolbarTimeflowInstancesRect.width = _toolbarTimeflowInstancesWidth;
            }
            if (Timeflow.Director != null) {
                if (Timeflow.Instances.Count > 1) {
                    _toolbarShowDirectorSyncToggleRect.x = _toolbarTimeflowInstancesRect.x + _toolbarTimeflowInstancesRect.width + _toolbarIconPadLeft;
                }
                else {
                    _toolbarShowDirectorSyncToggleRect.x = TimeflowViewLayout.LargeIconSize + _toolbarItemLeftPad;
                }
                _toolbarShowDirectorSyncToggleRect.y = iconTop;
                _toolbarShowDirectorSyncToggleRect.width = TimeflowViewLayout.LargeIconSize;
                _toolbarShowDirectorSyncToggleRect.height = TimeflowViewLayout.LargeIconSize;
            }
            else {
                _toolbarShowDirectorSyncToggleRect = _toolbarTimeflowInstancesRect;
            }
            _toolbarTimeflowColorLabelRect = new Rect(0f, Layout.Toolbar.Height, Layout.Toolbar.Width, _toolbarTimeflowColorLabelHeight);

            _toolbarAutoKeyframingRect.x = _toolbarTimeflowInstancesRect.x + _toolbarTimeflowInstancesRect.width;// + _toolbarIconSpacing;// + _toolbarSectionGap;
            _toolbarAutoKeyframingRect.y = iconTop;
            _toolbarAutoKeyframingRect.width = _toolbarAutoKeyframingRect.height = TimeflowViewLayout.LargeIconSize;
            if (Timeflow.Instances.Count > 1) {
                _toolbarAutoKeyframingRect.x = _toolbarTimeflowInstancesRect.x + _toolbarTimeflowInstancesRect.width + _toolbarItemLeftPad;
            }
            if (Timeflow.Director != null) {
                _toolbarAutoKeyframingRect.x = _toolbarShowDirectorSyncToggleRect.x + _toolbarShowDirectorSyncToggleRect.width + _toolbarIconPadLeft;
            }

            _toolbarGotoStartRect.x = _toolbarAutoKeyframingRect.width + _toolbarAutoKeyframingRect.x + _toolbarIconPadLeft;
            _toolbarGotoStartRect.y = _toolbarAutoKeyframingRect.y;
            _toolbarGotoStartRect.width = _toolbarGotoStartRect.height = TimeflowViewLayout.LargeIconSize;

            _toolbarGotoPrevRect = _toolbarGotoStartRect;
            _toolbarGotoPrevRect.x += _toolbarIconSpacing;

            _toolbarPlayReverseRect = _toolbarGotoPrevRect;
            _toolbarPlayReverseRect.x += _toolbarIconSpacing;

            _toolbarPlayRect = _toolbarPlayReverseRect;
            _toolbarPlayRect.x += _toolbarIconSpacing;

            _toolbarGotoNextRect = _toolbarPlayRect;
            _toolbarGotoNextRect.x += _toolbarIconSpacing;

            _toolbarGotoEndRect = _toolbarGotoNextRect;
            _toolbarGotoEndRect.x += _toolbarIconSpacing;

            _toolbarWorkAreaRect = _toolbarGotoEndRect;
            _toolbarWorkAreaRect.x += _toolbarIconSpacing + _toolbarSectionGap;

            _toolbarLoopRect = _toolbarWorkAreaRect;
            _toolbarLoopRect.x += _toolbarIconSpacing;

            _toolbarTimeScopeRect = _toolbarLoopRect;
            _toolbarTimeScopeRect.x += _toolbarIconSpacing + _toolbarSectionGap;

            _toolbarTimeScopeLocalizeRect = _toolbarTimeScopeRect;
            if (Timeflow.IsTimeScopeEnabled) {
                _toolbarTimeScopeLocalizeRect.x += _toolbarIconSpacing;
            }

            _toolbarFollowPlayheadRect = _toolbarTimeScopeLocalizeRect;
            _toolbarFollowPlayheadRect.x += _toolbarIconSpacing + _toolbarSectionGap;

            _toolbarTimeDisplaymodePrimaryRect = _toolbarFollowPlayheadRect;
            _toolbarTimeDisplaymodePrimaryRect.x += _toolbarIconSpacing + _toolbarSectionGap;
            _toolbarTimeDisplaymodePrimaryRect.y = _toolbarTimeDisplayModePrimaryTop;
            _toolbarTimeDisplaymodePrimaryRect.width = _toolbarTimeDisplayModePrimaryWidth;
            _toolbarTimeDisplaymodePrimaryRect.height = _toolbarTimeDisplayModePrimaryHeight;

            _toolbarTimeDisplayModeSecondaryRect = _toolbarTimeDisplaymodePrimaryRect;
            _toolbarTimeDisplayModeSecondaryRect.y = _toolbarTimeDisplaymodeSecondaryTop;

            _toolbarTimeDisplayPrimaryRect.x = _toolbarTimeDisplaymodePrimaryRect.x + _toolbarTimeDisplaymodePrimaryRect.width + _toolbarTimeDisplayModePrimaryLeftAdjust;
            _toolbarTimeDisplayPrimaryRect.y = 0;
            _toolbarTimeDisplayPrimaryRect.width = _toolbarTimeDisplayPrimaryWidth;
            _toolbarTimeDisplayPrimaryRect.height = _toolbarTimeDisplayModePrimaryHeight;

            _toolbarTimeDisplaySecondaryRect.x = _toolbarTimeDisplayModeSecondaryRect.x + _toolbarTimeDisplayModeSecondaryRect.width + _toolbarTimeDisplayModePrimaryLeftAdjust;
            _toolbarTimeDisplaySecondaryRect.y = _toolbarTopPadding + _toolbarTimeDisplayModeSecondaryPadTop;
            _toolbarTimeDisplaySecondaryRect.width = _toolbarTimeDisplayPrimaryWidth;
            _toolbarTimeDisplaySecondaryRect.height = _toolbarTimeDisplayModeSecondaryHeight;

            _toolbarFPSRect = _toolbarTimeDisplaySecondaryRect;
            _toolbarFPSRect.x += _toolbarFPSPadLeft;
            _toolbarFPSRect.width = _toolbarTimeDisplayPrimaryRect.xMax - _toolbarFPSRect.x;

            _toolbarTimeDisplayFieldRect = new Rect(_toolbarTimeDisplaymodePrimaryRect.x - 3, 1, (_toolbarTimeDisplayPrimaryRect.xMax - _toolbarTimeDisplaymodePrimaryRect.x) + 8, Layout.Toolbar.Height - 2);

            _toolbarMusicalTimingToggleRect.width = _toolbarMusicalTimingToggleRect.height = TimeflowViewLayout.LargeIconSize;
            _toolbarMusicalTimingToggleRect.x = _toolbarTimeDisplayFieldRect.x + _toolbarTimeDisplayFieldRect.width + _toolbarSectionGap;
            _toolbarMusicalTimingToggleRect.y = iconTop;

            _toolbarTimeDisplayPrimaryRect.width = _toolbarTimeDisplayPrimaryRect.width - 10; // leave space for Time Scale field

            _toolbarBPMFieldRect = _toolbarMusicalTimingToggleRect;
            _toolbarBPMFieldRect.x += _toolbarIconSpacing;
            _toolbarBPMFieldRect.y = _toolbarTopPadding;
            _toolbarBPMFieldRect.height = _toolbarBPMFieldHeight;
            _toolbarBPMFieldRect.width = _toolbarBPMFieldWidth;

            _toolbarBPMLabelRect = _toolbarBPMFieldRect;
            _toolbarBPMLabelRect.y = _toolbarTimeDisplaySecondaryRect.y;
            _toolbarBPMLabelRect.height = _toolbarBPMLabelHeight;


            _toolbarBPMBeatsPerBarRect.x = _toolbarBPMLabelRect.x + _toolbarBPMLabelRect.width;
            _toolbarBPMBeatsPerBarRect.y = _toolbarBPMBeatsPerBarTopOffset;
            _toolbarBPMBeatsPerBarRect.height = _toolbarBPMFieldHeight;
            _toolbarBPMBeatsPerBarRect.width = AxonUI.ArrangementTimeStyle.CalcSize(new GUIContent("" + Timeflow.BeatsPerBar)).x;

            _toolbarBPMBeatsPerBarLabelRect = _toolbarBPMBeatsPerBarRect;
            _toolbarBPMBeatsPerBarLabelRect.y += _toolbarIconTopPad;
            _toolbarBPMBeatsPerBarLabelRect.width = _toolbarBPMBeatsPerBarLabelWidth;

            int sizeIndex = BeatsNoteSizeIndex();
            _toolbarBPMBeatsNoteSizeRect = _toolbarBPMBeatsPerBarLabelRect;
            _toolbarBPMBeatsNoteSizeRect.y += 12f;
            _toolbarBPMBeatsNoteSizeRect.width = AxonUI.ArrangementTimeStyle.CalcSize(new GUIContent("" + sizesOptions[sizeIndex].text)).x;

            _toolbarGridToggleRect = _toolbarBPMBeatsNoteSizeRect;
            _toolbarGridToggleRect.y = iconTop;
            _toolbarGridToggleRect.x += _toolbarBPMBeatsNoteSizeRect.width + _toolbarItemLeftPad;
            _toolbarGridToggleRect.width = _toolbarGridToggleRect.height = TimeflowViewLayout.LargeIconSize;

            _toolbarSnapTimeToggleRect = _toolbarGridToggleRect;
            _toolbarSnapTimeToggleRect.x += _toolbarGridToggleRect.width;
            _toolbarSnapTimeToggleRect.width = _toolbarSnapTimeToggleRect.height = TimeflowViewLayout.LargeIconSize;

            _toolbarSnapValueToggleRect = _toolbarSnapTimeToggleRect;
            _toolbarSnapValueToggleRect.x += _toolbarSnapTimeToggleRect.width - _toolbarItemLeftPad;

            _toolbarSnapMenuRect = _toolbarSnapValueToggleRect;
            _toolbarSnapMenuRect.y += _toolbarIconTopPad;
            _toolbarSnapMenuRect.x += _toolbarSnapValueToggleRect.width + _toolbarIconPadLeft;
            //_toolbarSnapMenuRect.width = _toolbarSnapMenuWidth;
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(GridSnapUnits[GridSnap]));
            _toolbarSnapMenuRect.width = textSize.x + 15;

            _toolbarGridTimeDisplayMenuRect = _toolbarSnapMenuRect;
            _toolbarGridTimeDisplayMenuRect.width = _toolbarGridTimeDisplayMenuWidth;
            _toolbarGridTimeDisplayMenuRect.x += _toolbarSnapMenuRect.width;

            _toolbarShowKeyframeValuesToggleRect.x = _toolbarGridTimeDisplayMenuRect.x + _toolbarGridTimeDisplayMenuRect.width + 6;
            _toolbarShowKeyframeValuesToggleRect.y = _toolbarTopPadding + _toolbarIconPadLeft - 4;
            _toolbarShowKeyframeValuesToggleRect.width = _toolbarShowKeyframeValuesToggleRect.height = TimeflowViewLayout.LargerIconSize;

            if (GridTimeDisplay == TimeDisplayModes.Measures && TimeflowPreferences.Current.TimeToleranceMode == TimeflowPreferences.TimeToleranceModes.Frame) {
                _toolbarGridTimeDisplayMenuRect.width += 3; // wider M
                _toolbarShowKeyframeValuesToggleRect.x += TimeflowViewLayout.SmallIconSize;
            }
            _toolbarShowMarkersToggleRect.x = Layout.Toolbar.Width - _toolbarMarkersRightOffset;
            _toolbarShowMarkersToggleRect.y = _toolbarTopPadding + _toolbarIconPadLeft - 1;
            _toolbarShowMarkersToggleRect.x += _toolbarIconSpacing;
            _toolbarShowMarkersToggleRect.width = _toolbarShowMarkersToggleRect.height = TimeflowViewLayout.LargeIconSize;

            _toolbarMarkersPrevRect = _toolbarShowMarkersToggleRect;
            _toolbarMarkersPrevRect.y += 1;
            _toolbarMarkersPrevRect.x += _toolbarShowMarkersToggleRect.width;

            _toolbarMarkersNextRect = _toolbarMarkersPrevRect;
            _toolbarMarkersNextRect.x += _toolbarMarkersPrevRect.width - 3;

            _toolbarMarkerJumpMenuRect.x = Layout.Toolbar.Width - _toolbarJumpMenuRightOffset;
            _toolbarMarkerJumpMenuRect.y = _toolbarTopPadding + _toolbarJumpMenuTopPad;
            _toolbarMarkerJumpMenuRect.width = _toolbarJumpMenuWidth;
            _toolbarMarkerJumpMenuRect.height = _toolbarJumpMenuHeight;

            _toolbarPreferencesRect.x = Layout.Toolbar.Width - _toolbarPreferencesRightOffset;
            _toolbarPreferencesRect.y = _toolbarTopPadding + _toolbarMinimizeTopPad + 1;
            _toolbarPreferencesRect.width = _toolbarPreferencesRect.height = TimeflowViewLayout.SmallIconSize;

            _toolbarMinimizeRect.x = Layout.Toolbar.Width - _toolbarMinimizeRightOffset;
            _toolbarMinimizeRect.y = _toolbarTopPadding + _toolbarMinimizeTopPad;
            _toolbarMinimizeRect.width = _toolbarMinimizeRect.height = TimeflowViewLayout.SmallIconSize;
        }

        public void GUIToolbar()
        {
            if (IsLayout) {
                GUIToolbarLayout();
            }

            GUI.color = Timeflow.AutoKeyframingEnabled ? Color.red : Color.white;
            GUI.Box(Layout.Toolbar, "", AxonUI.ToolbarBoxStyle);
            GUI.color = Color.white;
            GUILayout.BeginArea(Layout.Toolbar.Rect);

            GUI.enabled = true;
            GUI.color = AxonColor.Default;

            GUIToolbarPrefabIcon();
            GUIToolbarLogo();
            GUIToolbarTimeflowInstancesMenu();
            if (Timeflow.Director != null) {
                GUIToolbarShowDirectorSyncToggle();
            }
            GUIToolbarAutoKeyframing();
            if (Layout.ScreenWidth > 310) {
                GUIToolbarPlayerButtons();
            }
            if (Layout.ScreenWidth > 380) {
                GUIToolbarWorkArea();
            }
            if (Layout.ScreenWidth > 450) {
                GUIToolbarTimeScope();
            }

            if (Layout.ScreenWidth > 500) {
                GUIToolbarFollowPlayhead();
            }
            if (Layout.ScreenWidth > 560) {
                GUI.color = AxonColor.TimeDisplayField;
                GUI.Box(_toolbarTimeDisplayFieldRect, "", AxonUI.ToolbarBoxStyle);
                GUI.color = AxonColor.Default;

                GUIToolbarTimeDisplayModePrimary();
                GUIToolbarTimeDisplayModeSecondary();
                GUIToolbarTimeDisplayPrimary();
                GUIToolbarTimeDisplaySecondary();
            }

            int offset = 0;
            if (Timeflow.Instances.Count > 1) offset = 150;

            #region MUSICAL TIMING
            if (Layout.ScreenWidth > 600 + offset) {
                GUIToolbarMusicalTimingToggle();
            }
            if (UseMusicalTiming) {
                if (Layout.ScreenWidth > 650 + offset) {
                    GUIToolbarBPMField();
                    GUIToolbarBPMLabel();
                }
                if (Layout.ScreenWidth > 670 + offset) {
                    GUIToolbarBPMBeatsPerBar();
                    GUIToolbarBPMBeatsNoteSize();
                }
            }
            #endregion

            if (Layout.ScreenWidth > 750 + offset) {
                GUIToolbarGridToggle();
            }
            if (Layout.ScreenWidth > 800 + offset) {
                GUIToolbarSnapToggle();
            }
            if (Layout.ScreenWidth > 820 + offset) {
                GUIToolbarGridSnapIncrementMenu();
                GUIToolbarGridTimeDisplayMode();
            }
            if (Layout.ScreenWidth > 900 + offset) {
                GUIToolbarShowKeyframeValuesToggle();
            }
            if (Layout.ScreenWidth > 857 + offset) {
                GUIToolbarShowMarkersToggle();
            }
            if (Layout.ScreenWidth > 750 + offset) {
                GUIToolbarMarkersPrevNext();
            }

            if (Layout.ScreenWidth > 570 + offset) {
                GUIToolbarMarkerJumpMenu();
            }
            //Debug.Log($"ScreenWidth:{Layout.ScreenWidth}  + offset:{offset}");
            GUIToolbarPreferences();
            GUIToolbarMinimize();

            GUILayout.EndArea();
        }

        private void GUIToolbarPrefabIcon()
        {
            if (!Timeflow.HasTimeflowParent && !Timeflow.IsDisplayingPrefab) return;
            if (GUI.Button(_toolbarPrefabIconRect, AxonUI.PrefabCloseLabel, AxonUI.TabPrevStyle)) {
                if (Timeflow.IsDisplayingPrefab) {
                    PrefabUtil.ExitPrefab();
                    GUIUtility.ExitGUI();
                }
                else {
                    Timeflow.TimeflowParent.IsActive = true;
                }
            }
            GUI.color = AxonColor.Prefab;
            GUI.Box(_toolbarTimeflowColorLabelRect, new GUIContent(), AxonUI.TrackStyle);
            GUI.color = Color.white;
        }

        private void GUIToolbarLogo()
        {
            if (GUI.Button(_toolbarLogoRect, AxonUI.AxonGenesisLogoLabel, AxonUI.AxonGenesisLogoStyle)) {
                AxonUI.Load(true);
                Timeflow.GlobalRefresh();
                SelectionUtil.Select(Timeflow.gameObject);
                GUIUtility.ExitGUI();
            }
        }

        private void GUIToolbarTimeflowInstancesMenu()
        {
            if (Timeflow.IsDisplayingPrefab) return;
            if (Timeflow.Instances.Count > 1) {
                GUI.SetNextControlName("TimeflowMenu");
                //int i = 0;
                //int current = -1;
                //string[] list = new string[Timeflow.Instances.Count];
                //foreach (Timeflow tf in Timeflow.Instances) {
                //    if (tf == null) continue;
                //    if (tf == Timeflow.Active) {
                //        current = i;
                //    }
                //    string name = tf.gameObject.name;

                //    list[i] = tf.gameObject.name;
                //    i++;
                //}

                GUI.color = Timeflow.GUIColor;
                GUI.Box(_toolbarTimeflowColorLabelRect, new GUIContent(), AxonUI.TrackStyle);

                GUI.color = AxonColor.Default;
                if (EditorGUI.DropdownButton(_toolbarTimeflowInstancesRect, new GUIContent(Timeflow.name), FocusType.Passive)) {
                    List<Timeflow> timeflows = new List<Timeflow>();
                    List<Timeflow> unsorted = new List<Timeflow>();
                    for (int t = 0; t < Timeflow.Instances.Count; t++) {
                        Timeflow tf = Timeflow.Instances[t];
                        if (tf == null) continue;
                        unsorted.Add(tf);
                    }
                    for (int t = 0; t < unsorted.Count; t++) {
                        Timeflow tf = unsorted[t];
                        if (tf == null || tf.Parent != null) continue;
                        timeflows.Add(tf);
                    }
                    int overflow = 0;
                    while (timeflows.Count < unsorted.Count) {
                        for (int t = 0; t < unsorted.Count; t++) {
                            Timeflow tf = unsorted[t];
                            if (tf == null || tf.Parent == null) continue;
                            for (int i = 0; i < timeflows.Count; i++) {
                                if (timeflows[i].gameObject == tf.Parent.gameObject) {
                                    timeflows.Insert(i + 1, tf);
                                    break;
                                }
                            }
                        }
                        overflow++;
                        if (overflow > 100) {
                            Debug.LogError("TimeflowView recursion depth overflow");
                            break;
                        }
                    }

                    GenericMenu menu = new GenericMenu();

                    foreach (Timeflow tf in timeflows) {
                        bool isActive = tf.gameObject.activeInHierarchy && tf.enabled;
                        string n = tf.gameObject.name;
                        if (tf.Parent != null) {
                            string indent = "";
                            for (int i = 0; i < tf.NestedDepth(); i++) {
                                indent += "-";
                            }
                            n = indent + " " + n;
                        }
                        menu.AddItem(new GUIContent(n), tf == Timeflow.Active, () => Timeflow.Active = tf);
                        if (!isActive) {
                            menu.AddDisabledItem(new GUIContent(tf.gameObject.name));
                        }
                    }
                    menu.ShowAsContext();
                }
            }
        }

        private void GUIToolbarPlayerButtons()
        {
            if (GUI.Button(_toolbarGotoStartRect, AxonUI.PlayerFirstLabel, AxonUI.PlayerFirstStyle)) {
                Timeflow.GotoStart();
            }
            if (GUI.Button(_toolbarGotoPrevRect, AxonUI.PlayerPrevLabel, AxonUI.PlayerPrevStyle)) {
                GotoPrevious();
            }
            if (GUI.Button(_toolbarPlayReverseRect, AxonUI.PlayerPlayReverseLabel,
                (Timeflow.IsPlaying && Timeflow.IsPlayReverse) ? AxonUI.PlayerPlayReverseOnStyle : AxonUI.PlayerPlayReverseStyle)) {
                if (!Timeflow.IsPlaying || !Timeflow.IsPlayReverse) {
                    Timeflow.PlayReverse(false);
                }
                else {
                    Timeflow.Stop();
                }
            }
            if (GUI.Button(_toolbarPlayRect, AxonUI.PlayerPlayLabel,
                (Timeflow.IsPlaying && !Timeflow.IsPlayReverse) ? (Timeflow.ContinuousPlay ? AxonUI.PlayerPlayContinuousStyle : AxonUI.PlayerPlayOnStyle) : AxonUI.PlayerPlayStyle)) {
                if (EditorInput.IsControl) {
                    Timeflow.Play(false);
                    Timeflow.ContinuousPlay = true;
                }
                else
                if (Timeflow.IsPlaying && Timeflow.IsPlayReverse) {
                    Timeflow.Play(false);
                }
                else {
                    Timeflow.TogglePlay();
                }
            }
            if (GUI.Button(_toolbarGotoNextRect, AxonUI.PlayerNextLabel, AxonUI.PlayerNextStyle)) {
                GotoNext();
            }
            if (GUI.Button(_toolbarGotoEndRect, AxonUI.PlayerLastLabel, AxonUI.PlayerLastStyle)) {
                Timeflow.GotoEnd();
            }
        }

        private void GUIToolbarAutoKeyframing()
        {
            if (GUI.Button(_toolbarAutoKeyframingRect, AxonUI.AutoKeyframingLabel, Timeflow.AutoKeyframingEnabled ? AxonUI.AutoKeyframingOnStyle : AxonUI.AutoKeyframingOffStyle)) {
                Timeflow.AutoKeyframingEnabled = !Timeflow.AutoKeyframingEnabled;
            }
        }

        private void GUIToolbarWorkArea()
        {
            if (GUI.Button(_toolbarWorkAreaRect, AxonUI.WorkAreaLabel, Timeflow.WorkAreaEnabled ? Timeflow.WorkAreaLocked ? AxonUI.WorkAreaLockedStyle : AxonUI.WorkAreaOnStyle : AxonUI.WorkAreaOffStyle)) {
                if (Event.current.alt) {
                    Timeflow.WorkAreaLocked = !Timeflow.WorkAreaLocked;
                }
                else {
                    Timeflow.WorkAreaEnabled = !Timeflow.WorkAreaEnabled;
                }
                Refresh(true);
            }
            if (GUI.Button(_toolbarLoopRect, AxonUI.LoopLabel, Timeflow.LoopEnabled ? AxonUI.LoopOnRedStyle : AxonUI.LoopOffStyle)) {
                Timeflow.LoopEnabled = !Timeflow.LoopEnabled;
            }
        }

        private void GUIToolbarTimeScope()
        {
            if (GUI.Button(_toolbarTimeScopeRect, AxonUI.TimeScopeLabel, Timeflow.IsTimeScopeEnabled ? AxonUI.TimeScopeOnStyle : AxonUI.TimeScopeOffStyle)) {
                Timeflow.ToggleLocalTimeScope();
            }
            if (Timeflow.IsTimeScopeEnabled) {
                if (GUI.Button(_toolbarTimeScopeLocalizeRect, AxonUI.TimeScopeLocalizeLabel, Timeflow.IsTimeScopeLocalized ? AxonUI.TimeScopeLocalizeOnStyle : AxonUI.TimeScopeLocalizeOffStyle)) {
                    Timeflow.IsTimeScopeLocalized = !Timeflow.IsTimeScopeLocalized;
                }
            }
        }

        private void GUIToolbarFollowPlayhead()
        {
            if (GUI.Button(_toolbarFollowPlayheadRect, AxonUI.FollowPlayheadLabel, FollowPlayhead ? AxonUI.FollowPlayheadOnStyle : AxonUI.FollowPlayheadOffStyle)) {
                FollowPlayhead = !FollowPlayhead;
                if (FollowPlayhead) {
                    ScrollFollowPlayheadSetup();
                }
            }
        }

        private void GUIToolbarTimeDisplayModePrimary()
        {
            GUI.color = Color.white;
            string tooltip = "";
            string modeLabel = "";

            if (!UseMusicalTiming && TimeDisplay == TimeDisplayModes.Measures) {
                //TimeDisplay = TimeDisplayModes.Seconds;
                UseMusicalTiming = true;
            }

            if (TimeDisplay == TimeDisplayModes.Seconds) {
                tooltip = "Seconds";
                modeLabel = "S";
            }
            else
            if (TimeDisplay == TimeDisplayModes.Frames) {
                tooltip = "Frames";
                modeLabel = "F";
            }
            else
            if (TimeDisplay == TimeDisplayModes.Timecode) {
                tooltip = "Timecode";
                modeLabel = "T";
            }
            else
            if (TimeDisplay == TimeDisplayModes.Measures) {
                tooltip = "Measures";
                modeLabel = "M";
            }

            GUI.SetNextControlName("TimeDisplayMode");
            if (GUI.Button(_toolbarTimeDisplaymodePrimaryRect, new GUIContent(modeLabel + ":", tooltip), EditorStyles.centeredGreyMiniLabel)) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Seconds"), TimeDisplay == TimeDisplayModes.Seconds, SetTimeDisplaySeconds);
                menu.AddItem(new GUIContent("Frames"), TimeDisplay == TimeDisplayModes.Frames, SetTimeDisplayFrames);
                menu.AddItem(new GUIContent("Timecode"), TimeDisplay == TimeDisplayModes.Timecode, SetTimeDisplayTimecode);
                if (UseMusicalTiming) {
                    menu.AddItem(new GUIContent("Measures"), TimeDisplay == TimeDisplayModes.Measures, SetTimeDisplayMeasures);
                }
                else {
                    menu.AddItem(new GUIContent("Measures"), false, null);
                }
                menu.ShowAsContext();
            }
        }

        private void GUIToolbarTimeDisplayModeSecondary()
        {
            string tooltip = "";
            string modeLabel = "";

            if (!UseMusicalTiming && TimeDisplay2nd == TimeDisplayModes.Measures) {
                TimeDisplay2nd = TimeDisplayModes.Seconds;
            }

            if (TimeDisplay2nd == TimeDisplayModes.Seconds) {
                tooltip = "Seconds";
                modeLabel = "S";
            }
            else
            if (TimeDisplay2nd == TimeDisplayModes.Frames) {
                tooltip = "Frames";
                modeLabel = "F";
            }
            else
            if (TimeDisplay2nd == TimeDisplayModes.Timecode) {
                tooltip = "Timecode";
                modeLabel = "T";
            }
            else
            if (TimeDisplay2nd == TimeDisplayModes.Measures) {
                tooltip = "Measures";
                modeLabel = "M";
            }

            GUI.SetNextControlName("TimeDisplay2ndMode");
            if (GUI.Button(_toolbarTimeDisplayModeSecondaryRect, new GUIContent(modeLabel + ":", tooltip), EditorStyles.centeredGreyMiniLabel)) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Seconds"), TimeDisplay2nd == TimeDisplayModes.Seconds, SetTimeDisplay2ndSeconds);
                menu.AddItem(new GUIContent("Frames"), TimeDisplay2nd == TimeDisplayModes.Frames, SetTimeDisplay2ndFrames);
                menu.AddItem(new GUIContent("Timecode"), TimeDisplay2nd == TimeDisplayModes.Timecode, SetTimeDisplay2ndTimecode);
                if (UseMusicalTiming) {
                    menu.AddItem(new GUIContent("Measures"), TimeDisplay2nd == TimeDisplayModes.Measures, SetTimeDisplay2ndMeasures);
                }
                else {
                    menu.AddItem(new GUIContent("Measures"), false, null);
                }
                menu.ShowAsContext();
            }
            GUI.color = Color.white;
        }

        private void GUIToolbarTimeDisplayPrimary()
        {
            int frame = Mathf.RoundToInt((float)Timeflow.FPS * Timeflow.CurrentTime);

            if (GUI.GetNameOfFocusedControl().Equals("TimeField")) {
                AxonUI.TimecodeStyle.normal.background = AxonUI.TimecodeStyle.active.background;
                AxonUI.TimecodeStyle.normal.textColor = AxonColor.BlackText;
            }
            else {
                AxonUI.TimecodeStyle.normal.background = null;
                AxonUI.TimecodeStyle.normal.textColor = AxonColor.Timecode;
            }
            GUI.SetNextControlName("TimeField");
            float newTime = AxonGUI.DisplayTimeField(Timeflow, _toolbarTimeDisplayPrimaryRect, Timeflow.CurrentTime, TimeDisplay, AxonUI.TimecodeStyle);
            if (!Mathf.Approximately(newTime, Timeflow.CurrentTime)) {
                Timeflow.CurrentTimeExact = newTime;
            }

            GUI.color = AxonColor.BrandRed;
            Timeflow.TimeScale = EditorGUI.FloatField(_toolbarTimeDisplayFieldRect, Timeflow.TimeScale, AxonUI.SmallLabelRightStyle);
            GUI.color = Color.white;
        }

        private void GUIToolbarTimeDisplaySecondary()
        {
            GUI.SetNextControlName("FramesField");
            float newTime2nd = AxonGUI.DisplayTimeField(Timeflow, _toolbarTimeDisplaySecondaryRect, Timeflow.CurrentTime, TimeDisplay2nd, AxonUI.TimeFramesStyle);
            if (!Mathf.Approximately(newTime2nd, Timeflow.CurrentTime)) {
                Timeflow.CurrentTimeExact = newTime2nd;
            }

            if (!GUI.GetNameOfFocusedControl().Equals("FramesField")) { // hide while editing field
                EditorGUI.LabelField(_toolbarFPSRect, "" + Timeflow.FPS + " fps", AxonUI.TimeFramesSmallStyle);
            }
        }

        private void GUIToolbarMusicalTimingToggle()
        {
            if (GUI.Button(_toolbarMusicalTimingToggleRect, AxonUI.MusicalTimingLabel, UseMusicalTiming ? AxonUI.MusicalTimingOnStyle : AxonUI.MusicalTimingOffStyle)) {
                UseMusicalTiming = !UseMusicalTiming;
                RecalculateSnap();
            }
        }

        private void GUIToolbarBPMField()
        {
            if (GUI.GetNameOfFocusedControl().Equals("BPMField")) {
                AxonUI.TimecodeStyle.normal.background = AxonUI.TimecodeStyle.active.background;
            }
            else {
                AxonUI.TimecodeStyle.normal.background = null;
            }
            GUI.SetNextControlName("BPMField");
            float bpm = Mathf.RoundToInt(Timeflow.BPM);
            float nbpm = EditorGUI.FloatField(_toolbarBPMFieldRect, GUIContent.none, bpm, AxonUI.ArrangementTimeStyle);
            if (bpm != nbpm) {
                Timeflow.BPM = nbpm;
            }
        }

        private void GUIToolbarBPMLabel()
        {
            AxonUI.TimeFramesStyle.normal.background = null;
            EditorGUI.LabelField(_toolbarBPMLabelRect, new GUIContent("BPM"), AxonUI.TimeFramesStyle);
        }

        private void GUIToolbarBPMBeatsPerBar()
        {
            if (Timeflow.BeatsPerBar == 0 || Timeflow.BeatsPerBar > 16) Timeflow.BeatsPerBar = 4;
            Timeflow.BeatsPerBar = EditorGUI.Popup(_toolbarBPMBeatsPerBarRect, GUIContent.none, Timeflow.BeatsPerBar - 1, beatsOptions, AxonUI.ArrangementTimeStyle) + 1;

            EditorGUI.LabelField(_toolbarBPMBeatsPerBarLabelRect, new GUIContent("__"), GUIContent.none, AxonUI.ArrangementTimeStyle);
        }

        private int BeatsNoteSizeIndex()
        {
            int sizeIndex = 0;
            string size = "" + Timeflow.BeatNoteSize;
            for (int i = 0; i < sizesOptions.Length; i++) {
                if (size.Equals(sizesOptions[i].text)) {
                    sizeIndex = i;
                    break;
                }
            }
            return sizeIndex;
        }

        private void GUIToolbarBPMBeatsNoteSize()
        {
            if (Timeflow.BeatNoteSize == 0 || Timeflow.BeatNoteSize > 16) Timeflow.BeatNoteSize = 4;
            int sizeIndex = BeatsNoteSizeIndex();
            int newIndex = EditorGUI.Popup(_toolbarBPMBeatsNoteSizeRect, GUIContent.none, sizeIndex, sizesOptions, AxonUI.ArrangementTimeStyle);
            if (newIndex != sizeIndex) {
                Timeflow.BeatNoteSize = StringUtil.ParseInt(sizesOptions[newIndex].text);
            }
        }

        private void GUIToolbarGridToggle()
        {
            if (GUI.Button(_toolbarGridToggleRect, AxonUI.GridLabel, GridEnabled ? AxonUI.GridOnStyle : AxonUI.GridOffStyle)) {
                GridEnabled = !GridEnabled;
            }
        }

        private void GUIToolbarSnapToggle()
        {
            if (GUI.Button(_toolbarSnapTimeToggleRect, AxonUI.GridSnapLabel, SnapTimeEnabled ? AxonUI.GridSnapOnStyle : AxonUI.GridSnapOffStyle)) {
                SnapTimeEnabled = !SnapTimeEnabled;
            }
            if (GUI.Button(_toolbarSnapValueToggleRect, AxonUI.GridSnapVertLabel, SnapValueEnabled ? AxonUI.GridSnapVertOnStyle : AxonUI.GridSnapVertOffStyle)) {
                SnapValueEnabled = !SnapValueEnabled;
            }
        }

        private void GUIToolbarGridSnapIncrementMenu()
        {
            //EditorGUI.BeginDisabledGroup(GridTimeDisplay == TimeDisplayModes.Frames);
            GUI.SetNextControlName("SnapMenu");
            int snap = EditorGUI.Popup(_toolbarSnapMenuRect, GridSnap, GridSnapUnits);
            if (snap != GridSnap) GridSnap = snap;
            //EditorGUI.EndDisabledGroup();
        }

        private void GUIToolbarGridTimeDisplayMode()
        {
            TimeDisplayModes gridMode = (TimeDisplayModes)EditorGUI.EnumPopup(_toolbarGridTimeDisplayMenuRect, GridTimeDisplay);
            if (GridTimeDisplay != gridMode) {
                EditorInput.SetEventUsed();
                GridTimeDisplay = gridMode;
                RecalculateSnap();
            }
            if (gridMode == TimeDisplayModes.Measures && TimeflowPreferences.Current.TimeToleranceMode == TimeflowPreferences.TimeToleranceModes.Frame) {
                Rect rect = _toolbarGridTimeDisplayMenuRect;
                rect.x += rect.width;
                rect.width = rect.height = TimeflowViewLayout.SmallIconSize;
                string message = "The accuracy of the grid display and snap depends on the Time Tolerance mode in the Preferences. " +
                    "When using musical timing, it may be necessary to use a smaller time increment to avoid time values being rounded to the nearest frame. " +
                    "Select Float mode to work with a finer level of time detail.";
                GUIStyle style = AxonUI.TextureButtonStyle;
                style.normal.background = AxonUI.Icons.Info;
                style.active.background = AxonUI.Icons.Info;
                style.alignment = TextAnchor.MiddleCenter;

                GUIContent content = new GUIContent("", message);
                if (GUI.Button(rect, content, style)) {
                    if (EditorUtility.DisplayDialog("Info", message, "Switch to Float Mode", "Dismiss")) {
                        TimeflowPreferences.Current.TimeToleranceMode = TimeflowPreferences.TimeToleranceModes.Float;
                        TimeflowPreferences.Current.TimeTolerance = 0.001f;
                        TimeflowPreferences.LoadOrCreateSettings();
                    }
                }
            }
        }

        private void GUIToolbarShowDirectorSyncToggle()
        {
            if (GUI.Button(_toolbarShowDirectorSyncToggleRect, AxonUI.DirectorSyncLabel, Timeflow.DirectorSyncEnabled ? AxonUI.DirectorSyncOnStyle : AxonUI.DirectorSyncOffStyle)) {
                Timeflow.DirectorSyncEnabled = !Timeflow.DirectorSyncEnabled;
            }
        }

        private void GUIToolbarShowKeyframeValuesToggle()
        {
            if (GUI.Button(_toolbarShowKeyframeValuesToggleRect, AxonUI.ShowKeyframeValuesLabel, Timeflow.ShowKeyframeValues ? AxonUI.KeyframeValuesOnStyle : AxonUI.KeyframeValuesOffStyle)) {
                Timeflow.ShowKeyframeValues = !Timeflow.ShowKeyframeValues;
            }
        }

        private void GUIToolbarShowMarkersToggle()
        {
            if (GUI.Button(_toolbarShowMarkersToggleRect, AxonUI.MarkersLabel, Timeflow.ShowMarkers ? AxonUI.MarkersOnStyle : AxonUI.MarkersOffStyle)) {
                if (EditorUtil.ControlKey) {
                    Timeflow.Markers.AddMarker(Timeflow.CurrentTime);
                }
                else {
                    Timeflow.ShowMarkers = !Timeflow.ShowMarkers;
                }
            }
        }

        private void GUIToolbarMarkersPrevNext()
        {
            if (Layout.ScreenWidth > 340) {
                if (GUI.Button(_toolbarMarkersPrevRect, AxonUI.MarkerPrevLabel, AxonUI.MarkerPrevStyle)) {
                    Timeflow.Markers.GotoPreviousMarker();
                }

                if (GUI.Button(_toolbarMarkersNextRect, AxonUI.MarkerNextLabel, AxonUI.MarkerNextStyle)) {
                    Timeflow.Markers.GotoNextMarker();
                }
            }
        }

        private string[] GetMarkerJumpMenuArray()
        {
            string[] markers = null;
            int c = 2;
            if (Timeflow.MarkerList != null) c = Timeflow.MarkerList.Count + 3;
            markers = new string[c];
            markers[0] = "Jump to...";
            markers[1] = "Full Duration";
            if (Timeflow.MarkerList != null) {
                markers[2] = null;
                int mi = 3;
                foreach (TimeflowMarker ma in Timeflow.MarkerList) {
                    markers[mi] = ma.Name;
                    mi++;
                }
            }
            return markers;
        }

        private void GUIToolbarMarkerJumpMenu()
        {
            int m = EditorGUI.Popup(_toolbarMarkerJumpMenuRect, Markers.MarkerJumpMenuIndex, GetMarkerJumpMenuArray());
            if (Markers.MarkerJumpMenuIndex != m) {
                Markers.MarkerJumpMenuIndex = m;
                Timeflow.Markers.GotoMarker(Markers.MarkerJumpMenuIndex - 3);
            }
        }

        private void GUIToolbarPreferences()
        {
            if (GUI.Button(_toolbarPreferencesRect, AxonUI.SettingsLabel, AxonUI.SettingsStyle)) {
                SettingsService.OpenUserPreferences("Preferences/TimeflowPreferences");
            }
        }

        private void GUIToolbarMinimize()
        {
            if (GUI.Button(_toolbarMinimizeRect, AxonUI.IsMinimizedLabel, TimeflowWindow.IsMinimized ? AxonUI.IsMinimizedOnStyle : AxonUI.IsMinimizedOffStyle)) {
                TimeflowWindow.IsMinimized = !TimeflowWindow.IsMinimized;
            }
        }

    }
}//AxonGenesis

#endif
