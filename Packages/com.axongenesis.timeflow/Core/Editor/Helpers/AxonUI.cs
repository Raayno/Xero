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
    /// <summary>
    /// This defines a collection of images and other resources used by the editor UI in AxonGenesis
    /// windows and views. 
    /// </summary>
    public static class AxonUI
    {
        public static bool IsLoaded;
        private static EditorIcons _Icons;

        public static EditorIcons Icons {
            get {
                if (_Icons == null) {
                    _Icons = (EditorIcons)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("e4711c0d211474b448c69dc75f4f6634"), typeof(EditorIcons));
                }
                return _Icons;
            }
        }

        #region GUISTYLES
        public static GUIStyle
            AxonGenesisLogoStyle,
            PresetStyle,
            AdvancedPresetStyle,
            PrefabIconStyle,
            TabNextStyle,
            TabPrevStyle,
            FoldoutStyle,
            SolidStyle,
            RowStyle,
            SmallLabelStyle,
            SmallLabelCenterStyle,
            SmallLabelRightStyle,
            InfoLabelStyle,
            HeaderStyle,
            HeaderStyleNoPad,
            HeaderStyleDark,
            HeaderStyleSelected,
            HeaderStyleDarkBig,
            HeaderStyleOpen,
            HeaderStyleClosed,
            BehaviorOnStyle,
            BehaviorOffStyle,
            BehaviorOffFadedStyle,
            BehaviorDisabledStyle,
            MinMaxFieldToggleOnStyle,
            MinMaxFieldToggleOffStyle,
            DisplayChannelOnStyle,
            DisplayChannelOffStyle,
            DisplayChannelSoloOnStyle,
            DisplayChannelSoloOffStyle,
            DragAndDropAreaStyle,
            LockOnStyle,
            LockOffStyle,
            LockOffFadedStyle,
            LockUnlockedStyle,
            LockLockedStyle,
            LockHalfStyle,
            LockBigOnStyle,
            LockBigOffStyle,
            ClearSearchStyle,
            SearchTypeSortingAlphabeticalStyle,
            SearchTypeSortingPrioritizedStyle,
            SearchTypeSortingCountStyle,
            SaveStyle,
            SearchStyleOn,
            SearchStyleOff,
            UniformValueOnStyle,
            UniformValueOffStyle,
            DarkBoxStyle,
            ToolbarBoxStyle,
            HierarchyBoxStyle,
            HierarchyToolsStyle,
            TextureButtonStyle,
            OutsideWorkAreaStyle,
            MarqueeStyle,
            ScrollbarInStyle,
            ScrollbarOutStyle,
            WorkAreaInMarkerStyle,
            WorkAreaOutMarkerStyle,
            PlayheadStyle,
            EndTimeStyle,
            TimeRangeStyle,
            TimeRangeEmptyStyle,
            MarkerStyle,
            MarkerSelStyle,
            MarkerLabelStyle,
            EventLabelStyle,
            ObjectStyle,
            ObjectSelectedStyle,
            ObjectSelectedDefocusStyle,
            ObjectDragStyle,
            ObjectTextFieldStyle,
            ChannelTextFieldStyle,
            SubObjectStyle,
            SubObjectDeselectedStyle,
            SubObjectSelectedStyle,
            AudioTrackStyle,
            ChannelLabelStyle,
            TrackStyle,
            TrackShadowStyle,
            TrackSelectedStyle,
            TrackSelectedPatternStyle,
            TrackSelectedCopyStyle,
            TrackEmptyStyle,
            TrackOffStyle,
            TrackDisabledStyle,
            TrackLoopStyle,
            TrackMixLinearStyle,
            TrackMixEaseStyle,
            TrackMixLinearInverseStyle,
            TrackMixEaseInverseStyle,
            TrackMixFadedStyle,
            TrackMixFadedInverseStyle,
            TrackMixHoldStyle,
            MidiNoteStyle,
            ColumnExpandOnStyle,
            ColumnExpandOffStyle,
            ColumnExpandRightOnStyle,
            ColumnExpandRightOffStyle,
            FoldoutDownStyle,
            FoldoutUpStyle,
            FoldoutDownPlusStyle,
            FoldoutUpPlusStyle,
            FoldoutNoneStyle,
            KeyframeStyle,
            KeyframeSelectedStyle,
            KeyframeSelectedCopyStyle,
            KeyframeObjectStyle,
            KeyframeObjectSelectedStyle,
            KeyframeHoldStyle,
            KeyframeHoldSelectedStyle,
            EventStyle,
            EventDisabledStyle,
            EventDisabledSelStyle,
            EventSelectedStyle,
            BezierBrokenHandleStyle,
            BezierUnifiedHandleStyle,
            BezierEqualHandleStyle,
            AudioOnStyle,
            AudioOffStyle,
            AudioWaveformOnStyle,
            AudioWaveformOffStyle,
            LoopHandleStyle,
            LoopHandleVerticalStyle,
            ToolbarDivider,
            InterpChanNone,
            InterpChanLinear,
            InterpChanBezier,
            InterpChanQuad,
            InterpLinearStyle,
            InterpLinearStyleOff,
            InterpHoldStyle,
            InterpHoldStyleOff,
            InterpFlatStyle,
            InterpVerticalStyle,
            InterpFlatLeftStyle,
            InterpFlatRightStyle,
            InterpAutoOnStyle,
            InterpAutoOffStyle,
            ToolSelectStyle,
            ToolEditTangentsStyle,
            ToolsEditKeysOnlyStyle,
            UnifyTangentsOn,
            UnifyTangentsOff,
            UnifyTangentLengths,
            HierarchyAddOnStyle,
            HierarchyAddOffStyle,
            PrevKeyStyle,
            PrevKeyNoneStyle,
            NextKeyStyle,
            NextKeyNoneStyle,
            ToggleKeyOnStyle,
            ToggleKeyOffStyle,
            ChannelLinkOnStyle,
            ChannelLinkOffStyle,
            ChannelLinkRemoveStyle,
            ChannelLinkSelectStyle,
            ChannelLinkTargetStyle,
            GraphOnStyle,
            GraphLockedStyle,
            GraphOffStyle,
            DeleteOnStyle,
            DeleteOffStyle,
            TrackDragTimeOffsetOnStyle,
            TrackDragTimeOffsetOffStyle,
            TimeRulerLabelStyle,
            ChannelXOnStyle,
            ChannelXOffStyle,
            ChannelYOnStyle,
            ChannelYOffStyle,
            ChannelZOnStyle,
            ChannelZOffStyle,
            ChannelAOnStyle,
            ChannelAOffStyle,
            ChannelLoopOnStyle,
            ChannelLoopOffStyle,
            ChannelLoopPingPongStyle,
            ChannelLoopHalfStyle,
            ChannelLoopFreeStyle,
            ChannelLoopAutoOnStyle,
            ChannelLoopAutoOffStyle,
            ChannelLoopInOnStyle,
            ChannelLoopInOffStyle,
            ChannelLoopOutOnStyle,
            ChannelLoopOutOffStyle,
            ChannelLoopMatchStyle,
            VisibilityOnStyle,
            VisibilityHalfStyle,
            VisibilityOffStyle,
            VisibilityOffFadedStyle,
            GridOnStyle,
            GridOffStyle,
            GridSnapOnStyle,
            GridSnapOffStyle,
            GridSnapVertOnStyle,
            GridSnapVertOffStyle,
            MarkersOnStyle,
            MarkersOffStyle,
            KeyframeValuesOnStyle,
            KeyframeValuesOffStyle,
            DirectorSyncOnStyle,
            DirectorSyncOffStyle,
            IsMinimizedOnStyle,
            IsMinimizedOffStyle,
            AutoKeyframingOnStyle,
            AutoKeyframingOffStyle,
            WorkAreaOnStyle,
            WorkAreaOffStyle,
            WorkAreaLockedStyle,
            LoopOnRedStyle,
            LoopOnWhiteStyle,
            LoopOffStyle,
            FollowPlayheadOnStyle,
            FollowPlayheadOffStyle,
            TimeScopeOnStyle,
            TimeScopeOffStyle,
            TimeScopeLocalizeOnStyle,
            TimeScopeLocalizeOffStyle,
            MusicalTimingOnStyle,
            MusicalTimingOffStyle,
            KeySelectModeAllStyle,
            KeySelectModeKeysStyle,
            KeySelectModeTracksStyle,
            AlignTimeLeftStyle,
            AlignTimeRightStyle,
            AlignTimeCenterStyle,
            AlignTimeDistributeStyle,
            AlignTimeMirrorStyle,
            AlignValueBottomStyle,
            AlignValueTopStyle,
            AlignValueCenterStyle,
            AlignValueDistributeStyle,
            AlignValueMirrorStyle,
            FitViewStyle,
            FitViewAutoStyle,
            AlignToolsOnStyle,
            AlignToolsOffStyle,
            GraphViewOnStyle,
            GraphViewOffStyle,
            GraphLockOnStyle,
            GraphLockOffStyle,
            SettingsStyle,
            KeyframeToolsOffStyle,
            KeyframeToolsOnStyle,
            BoundingBoxStyle,
            TimecodeStyle,
            TimeMeasureStyle,
            TimeFramesStyle,
            TimeFramesSmallStyle,
            ArrangementTimeStyle,
            PopupStyle,
            PlayerFirstStyle,
            PlayerPrevStyle,
            PlayerPlayStyle,
            PlayerPlayReverseStyle,
            PlayerNextStyle,
            PlayerLastStyle,
            PlayerFirstOnStyle,
            PlayerPrevOnStyle,
            PlayerPlayReverseOnStyle,
            PlayerPlayOnStyle,
            PlayerPlayContinuousStyle,
            PlayerNextOnStyle,
            PlayerLastOnStyle = null,
            MarkerPrevStyle,
            MarkerNextStyle,
            MarkerPrevOnStyle,
            MarkerNextOnStyle,
            EditFieldStyle,
            TimeFieldLabelStyle,
            PopupFieldStyle,
            HorizontalStyle;
        #endregion

        #region GUICONTENTS
        public static GUIContent
            AxonGenesisLogoLabel,
            PrefabCloseLabel,
            PrefabOpenLabel,
            UniformValueLabel,
            DisplayUnlockedOnlyLabel,
            TrackColorLabel,
            LockBigLabel,
            SaveLabel,
            SearchLabel,
            ClearSearchLabel,
            SearchTypeSortingLabel,
            TimeModeLabel,
            DisplayAnimatedOnlyLabel,
            DisplayShowChannelsLabel,
            InterpChanNoneLabel,
            InterpChanLinearLabel,
            InterpChanBezierLabel,
            InterpChanQuadLabel,
            InterpLinearLabel,
            InterpHoldLabel,
            InterpFlatLabel,
            InterpVerticalLabel,
            InterpFlatLeftLabel,
            InterpFlatRightLabel,
            InterpAutoLabel,
            ToolSelectLabel,
            ToolEditTangentsLabel,
            ToolEditKeysOnlyLabel,
            UnifiedTangentsLabel,
            ChannelXLabel,
            ChannelYLabel,
            ChannelZLabel,
            ChannelALabel,
            ChannelLoopLabel,
            ChannelLoopAutoOnLabel,
            ChannelLoopAutoOffLabel,
            ChannelLoopInOnLabel,
            ChannelLoopInOffLabel,
            ChannelLoopOutOnLabel,
            ChannelLoopOutOffLabel,
            ChannelLoopMatchLabel,
            GridLabel,
            DisplayVisibileLabel,
            GridSnapLabel,
            GridSnapVertLabel,
            MarkersLabel,
            ShowKeyframeValuesLabel,
            DirectorSyncLabel,
            IsMinimizedLabel,
            AutoKeyframingLabel,
            WorkAreaLabel,
            TimeScopeLabel,
            TimeScopeLocalizeLabel,
            FollowPlayheadLabel,
            LoopLabel,
            ClipLoopLabel,
            BPMLabel,
            KeySelectModeLabel,
            AlignTimeLeftLabel,
            AlignTimeRightLabel,
            AlignTimeCenterLabel,
            AlignTimeDistributeLabel,
            AlignTimeMirrorLabel,
            AlignValueBottomLabel,
            AlignValueTopLabel,
            AlignValueCenterLabel,
            AlignValueDistributeLabel,
            AlignValueMirrorLabel,
            FitViewLabel,
            AlignToolsLabel,
            GraphViewLabel,
            GraphLockLabel,
            GraphSoloLabel,
            MusicalTimingLabel,
            SettingsLabel,
            DrawCurveLabel,
            PlayerFirstLabel,
            PlayerPrevLabel,
            PlayerPlayReverseLabel,
            PlayerPlayLabel,
            PlayerNextLabel,
            PlayerLastLabel,
            MarkerPrevLabel,
            MarkerNextLabel,
            DisplayPrevLabel,
            DisplayNextLabel,
            ColumnExpandLabel,
            EndTimeLabel,
            ColumnExpandRightLabel;

        #endregion

        public static void Load(bool force = false)
        {
            if (!IsLoaded || force) {
                IsLoaded = true;
                AxonColor.Load();
                Icons.Setup();

                HeaderStyle = new GUIStyle();
                HeaderStyle.margin = new RectOffset(0, 0, 4, 4);
                HeaderStyle.normal.background = Icons.Border;
                HeaderStyle.border = new RectOffset(3, 3, 3, 3);
                HeaderStyle.padding = new RectOffset(5, 5, 5, 5);

                HeaderStyleNoPad = new GUIStyle(HeaderStyle);
                HeaderStyleNoPad.padding = new RectOffset(0, 5, 0, 0);

                HeaderStyleDark = new GUIStyle();
                HeaderStyleDark.margin = new RectOffset(0, 0, 4, 4);
                HeaderStyleDark.normal.background = Icons.BorderDark;
                HeaderStyleDark.border = new RectOffset(2, 2, 6, 2);
                HeaderStyleDark.padding = new RectOffset(1, 5, 5, 2);

                HeaderStyleSelected = new GUIStyle();
                HeaderStyleSelected.margin = new RectOffset(0, 0, 4, 4);
                HeaderStyleSelected.normal.background = Icons.BorderSelected;
                HeaderStyleSelected.border = new RectOffset(4, 4, 4, 4);
                HeaderStyleSelected.padding = new RectOffset(5, 5, 5, 5);


                HeaderStyleDarkBig = new GUIStyle();
                HeaderStyleDarkBig.margin = new RectOffset(0, 0, 4, 4);
                HeaderStyleDarkBig.normal.background = Icons.BorderDark;
                HeaderStyleDarkBig.border = new RectOffset(2, 2, 6, 2);
                HeaderStyleDarkBig.padding = new RectOffset(5, 5, 5, 2);
                HeaderStyleDarkBig.fontSize = 12;
                HeaderStyleDarkBig.fontStyle = FontStyle.Bold;

                HeaderStyleOpen = new GUIStyle();// GUI.skin.window);
                HeaderStyleOpen.normal.background = Icons.BorderLight;
                HeaderStyleOpen.active.background = Icons.BorderLight;
                HeaderStyleOpen.border = new RectOffset(3, 3, 3, 3);
                HeaderStyleOpen.padding = new RectOffset(0, 5, 4, 4);

                HeaderStyleClosed = new GUIStyle(HeaderStyleOpen);
                HeaderStyleClosed.fixedHeight = 24;

                FoldoutStyle = new GUIStyle();
                FoldoutStyle.fontSize = 11;
                FoldoutStyle.fontStyle = FontStyle.Bold;
                FoldoutStyle.padding = new RectOffset(0, 0, 0, 8);
                FoldoutStyle.border = new RectOffset(0, 0, 0, 6);
                FoldoutStyle.normal.textColor = GUI.skin.button.normal.textColor;
                FoldoutStyle.hover.textColor = GUI.skin.button.normal.textColor;
                Icons.FoldoutDown = Icons.FoldoutDown;
                Icons.FoldoutUp = Icons.FoldoutUp;
                Icons.FoldoutNone = Icons.FoldoutNone;


                RowStyle = new GUIStyle();
                RowStyle.padding = new RectOffset(0, 0, 4, 4);
                RowStyle.border = new RectOffset(0, 0, 4, 0);
                RowStyle.margin = new RectOffset(0, 0, 0, 0);
                RowStyle.normal.background = Icons.Row;
                RowStyle.hover.background = Icons.RowHover;


                SmallLabelStyle = new GUIStyle(GUI.skin.label);
                SmallLabelStyle.padding = new RectOffset(2, 0, 0, 0);
                SmallLabelStyle.fontSize = 10;
                SmallLabelStyle.fontStyle = FontStyle.Bold;
                SmallLabelStyle.alignment = TextAnchor.MiddleLeft;

                SmallLabelCenterStyle = new GUIStyle(GUI.skin.label);
                SmallLabelCenterStyle.padding = new RectOffset(2, 0, 0, 0);
                SmallLabelCenterStyle.fontSize = 10;
                SmallLabelCenterStyle.fontStyle = FontStyle.Italic;
                SmallLabelCenterStyle.alignment = TextAnchor.MiddleCenter;

                SmallLabelRightStyle = new GUIStyle(GUI.skin.label);
                SmallLabelRightStyle.padding = new RectOffset(2, 0, 0, 0);
                SmallLabelRightStyle.fontSize = 12;
                SmallLabelRightStyle.fontStyle = FontStyle.Bold;
                SmallLabelRightStyle.alignment = TextAnchor.UpperRight;

                InfoLabelStyle = new GUIStyle(EditorStyles.miniLabel);
                InfoLabelStyle.fontStyle = FontStyle.Italic;
                InfoLabelStyle.alignment = TextAnchor.MiddleLeft;

                TimeFieldLabelStyle = new GUIStyle(GUI.skin.label);
                TimeFieldLabelStyle.alignment = TextAnchor.MiddleLeft;
                TimeFieldLabelStyle.fontStyle = FontStyle.Bold;
                TimeFieldLabelStyle.fontSize = 8;

                SolidStyle = new GUIStyle();
                SolidStyle.border = new RectOffset(0, 0, 0, 0);
                SolidStyle.margin = new RectOffset(0, 0, 0, 0);
                SolidStyle.padding = new RectOffset(0, 0, 0, 0);
                SolidStyle.stretchWidth = true;
                SolidStyle.stretchHeight = true;
                SolidStyle.normal.background = new Texture2D(1, 1);
                SolidStyle.normal.background.SetPixel(1, 1, Color.white);
                SolidStyle.normal.background.filterMode = FilterMode.Point;
                SolidStyle.active.background = SolidStyle.normal.background;

                TextureButtonStyle = new GUIStyle(GUIStyle.none);
                TextureButtonStyle.border = new RectOffset(0, 0, 0, 0);
                TextureButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                TextureButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                TextureButtonStyle.normal.textColor = new Color(1f, 1f, 1f, 1.0f);
                TextureButtonStyle.active.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

                AxonGenesisLogoLabel = new GUIContent();
                AxonGenesisLogoLabel.tooltip = "Refresh view and select Timeflow instance";

                AxonGenesisLogoStyle = new GUIStyle(TextureButtonStyle);
                AxonGenesisLogoStyle.normal.background = Icons.AxonGenesisLogo;
                AxonGenesisLogoStyle.active.background = Icons.AxonGenesisLogoOn;

                PresetStyle = new GUIStyle(TextureButtonStyle);
                PresetStyle.alignment = TextAnchor.MiddleCenter;
                PresetStyle.stretchWidth = true;
                PresetStyle.stretchHeight = true;
                PresetStyle.normal.background = Icons.Presets;
                PresetStyle.active.background = Icons.Presets;

                AdvancedPresetStyle = new GUIStyle(TextureButtonStyle);
                AdvancedPresetStyle.alignment = TextAnchor.MiddleCenter;
                AdvancedPresetStyle.stretchWidth = true;
                AdvancedPresetStyle.stretchHeight = true;
                AdvancedPresetStyle.normal.background = Icons.AdvancedPresets;
                AdvancedPresetStyle.active.background = Icons.AdvancedPresets;

                PrefabCloseLabel = new GUIContent();
                PrefabCloseLabel.tooltip = "Click to exit editing mode";

                PrefabOpenLabel = new GUIContent();
                PrefabOpenLabel.tooltip = "Click to open for editing";

                PrefabIconStyle = new GUIStyle(TextureButtonStyle);
                PrefabIconStyle.normal.background = Icons.PrefabIcon;
                PrefabIconStyle.active.background = Icons.PrefabIcon;

                TabNextStyle = new GUIStyle(TextureButtonStyle);
                TabNextStyle.normal.background = Icons.TabNext;
                TabNextStyle.active.background = Icons.TabNext;

                TabPrevStyle = new GUIStyle(TextureButtonStyle);
                TabPrevStyle.normal.background = Icons.TabPrev;
                TabPrevStyle.active.background = Icons.TabPrev;

                BehaviorOnStyle = new GUIStyle(TextureButtonStyle);
                BehaviorOnStyle.normal.background = Icons.BehaviorOn;

                BehaviorOffStyle = new GUIStyle(TextureButtonStyle);
                BehaviorOffStyle.normal.background = Icons.BehaviorOff;
                BehaviorOffStyle.active.background = BehaviorOnStyle.normal.background;


                BehaviorOffFadedStyle = new GUIStyle(TextureButtonStyle);
                BehaviorOffFadedStyle.normal.background = Icons.BehaviorOffFaded;
                BehaviorOffFadedStyle.active.background = BehaviorOnStyle.normal.background;

                BehaviorDisabledStyle = new GUIStyle(TextureButtonStyle);
                BehaviorDisabledStyle.normal.background = Icons.BehaviorDisabled;
                BehaviorDisabledStyle.active.background = BehaviorOnStyle.normal.background;

                DisplayChannelOffStyle = new GUIStyle(TextureButtonStyle);
                DisplayChannelOffStyle.normal.background = Icons.DisplayChannelOff;
                DisplayChannelOffStyle.active.background = Icons.DisplayChannelOn;

                DisplayChannelOnStyle = new GUIStyle(TextureButtonStyle);
                DisplayChannelOnStyle.normal.background = DisplayChannelOffStyle.active.background;
                DisplayChannelOnStyle.active.background = Icons.ChannelLinkOn;

                DisplayChannelSoloOnStyle = new GUIStyle(TextureButtonStyle);
                DisplayChannelSoloOnStyle.normal.background = Icons.ChannelLinkOn;
                DisplayChannelSoloOnStyle.active.background = Icons.ChannelLinkOff;

                DisplayChannelSoloOffStyle = new GUIStyle(TextureButtonStyle);
                DisplayChannelSoloOffStyle.normal.background = Icons.ChannelLinkOff;
                DisplayChannelSoloOffStyle.active.background = Icons.ChannelLinkOn;

                DragAndDropAreaStyle = new GUIStyle(EditorStyles.helpBox);
                DragAndDropAreaStyle.fontSize = 11;
                DragAndDropAreaStyle.fontStyle = FontStyle.Normal;
                DragAndDropAreaStyle.alignment = TextAnchor.MiddleCenter;
                DragAndDropAreaStyle.margin = new RectOffset(10, 10, 10, 10);
                DragAndDropAreaStyle.border = new RectOffset(3, 3, 3, 3);
                DragAndDropAreaStyle.padding = new RectOffset(3, 3, 3, 3);


                TrackColorLabel = new GUIContent();
                TrackColorLabel.tooltip = "Track/Channel Color";

                DisplayUnlockedOnlyLabel = new GUIContent();
                DisplayUnlockedOnlyLabel.tooltip = "Display Unlocked Only";

                LockOnStyle = new GUIStyle(TextureButtonStyle);
                LockOnStyle.normal.background = AxonUI.Icons.LockOn;
                LockOnStyle.active.background = AxonUI.Icons.LockOff;

                LockOffStyle = new GUIStyle(TextureButtonStyle);
                LockOffStyle.normal.background = AxonUI.Icons.LockOff;
                LockOffStyle.active.background = LockOnStyle.normal.background;

                LockOffFadedStyle = new GUIStyle(TextureButtonStyle);
                LockOffFadedStyle.normal.background = AxonUI.Icons.LockOffFaded;
                LockOffFadedStyle.active.background = LockOnStyle.normal.background;

                LockLockedStyle = new GUIStyle(TextureButtonStyle);
                LockLockedStyle.normal.background = AxonUI.Icons.LockOn;
                LockLockedStyle.active.background = LockOffStyle.normal.background;

                LockUnlockedStyle = new GUIStyle(TextureButtonStyle);
                LockUnlockedStyle.normal.background = AxonUI.Icons.LockOff;
                LockUnlockedStyle.active.background = LockOffStyle.normal.background;

                LockHalfStyle = new GUIStyle(TextureButtonStyle);
                LockHalfStyle.normal.background = AxonUI.Icons.LockHalf;
                LockHalfStyle.active.background = LockOnStyle.normal.background;

                LockBigLabel = new GUIContent();
                LockBigLabel.tooltip = "Lock Display List";

                LockBigOnStyle = new GUIStyle(TextureButtonStyle);
                LockBigOnStyle.normal.background = AxonUI.Icons.LockBigOn;
                LockBigOnStyle.active.background = AxonUI.Icons.LockBigOff;

                LockBigOffStyle = new GUIStyle(TextureButtonStyle);
                LockBigOffStyle.normal.background = AxonUI.Icons.LockBigOff;
                LockBigOffStyle.active.background = LockBigOnStyle.normal.background;

                SaveLabel = new GUIContent();
                SaveLabel.tooltip = "Save Display List";

                SaveStyle = new GUIStyle(TextureButtonStyle);
                SaveStyle.normal.background = AxonUI.Icons.SaveOff;
                SaveStyle.active.background = AxonUI.Icons.SaveOn;

                SearchLabel = new GUIContent();
                SearchLabel.tooltip = "Search Display List";

                SearchTypeSortingLabel = new GUIContent();
                SearchTypeSortingLabel.tooltip = "Search Type Sorting Mode\n" +
                    "AB -> Alphabetical\n" +
                    "P -> Prioritized by preferences\n" +
                    "# -> Ordered by highest occurrence";

                ClearSearchLabel = new GUIContent();
                ClearSearchLabel.tooltip = "Clear Search";

                ClearSearchStyle = new GUIStyle(TextureButtonStyle);
                ClearSearchStyle.normal.background = AxonUI.Icons.DeleteOff;
                ClearSearchStyle.active.background = AxonUI.Icons.DeleteOn;

                SearchTypeSortingAlphabeticalStyle = new GUIStyle(TextureButtonStyle);
                SearchTypeSortingAlphabeticalStyle.normal.background = AxonUI.Icons.SearchTypeSortingAlphabetical;
                SearchTypeSortingAlphabeticalStyle.active.background = AxonUI.Icons.SearchTypeSortingAlphabetical;

                SearchTypeSortingPrioritizedStyle = new GUIStyle(TextureButtonStyle);
                SearchTypeSortingPrioritizedStyle.normal.background = AxonUI.Icons.SearchTypeSortingPrioritized;
                SearchTypeSortingPrioritizedStyle.active.background = AxonUI.Icons.SearchTypeSortingPrioritized;

                SearchTypeSortingCountStyle = new GUIStyle(TextureButtonStyle);
                SearchTypeSortingCountStyle.normal.background = AxonUI.Icons.SearchTypeSortingCount;
                SearchTypeSortingCountStyle.active.background = AxonUI.Icons.SearchTypeSortingCount;

                SearchStyleOn = new GUIStyle(TextureButtonStyle);
                SearchStyleOn.normal.background = AxonUI.Icons.SearchOn;
                SearchStyleOn.active.background = AxonUI.Icons.SearchOff;

                SearchStyleOff = new GUIStyle(TextureButtonStyle);
                SearchStyleOff.normal.background = AxonUI.Icons.SearchOff;
                SearchStyleOff.active.background = AxonUI.Icons.SearchOn;

                UniformValueLabel = new GUIContent();
                UniformValueLabel.tooltip = "Uniform Value";

                UniformValueOnStyle = new GUIStyle(TextureButtonStyle);
                UniformValueOnStyle.normal.background = Icons.UniformValueOn;
                UniformValueOnStyle.active.background = Icons.UniformValueOff;

                UniformValueOffStyle = new GUIStyle(TextureButtonStyle);
                UniformValueOffStyle.normal.background = UniformValueOnStyle.active.background;
                UniformValueOffStyle.active.background = UniformValueOnStyle.normal.background;

                DarkBoxStyle = new GUIStyle();
                DarkBoxStyle.padding = new RectOffset(0, 0, 0, 0);
                DarkBoxStyle.alignment = TextAnchor.MiddleLeft;
                DarkBoxStyle.border = new RectOffset(2, 2, 2, 2);
                DarkBoxStyle.normal.background = Icons.DarkBox;

                OutsideWorkAreaStyle = new GUIStyle(DarkBoxStyle);
                OutsideWorkAreaStyle.normal.background = Icons.OutsideWorkArea;

                ToolbarBoxStyle = new GUIStyle();
                ToolbarBoxStyle.padding = new RectOffset(0, 0, 0, 0);
                ToolbarBoxStyle.alignment = TextAnchor.MiddleLeft;
                ToolbarBoxStyle.border = new RectOffset(0, 0, 0, 0);
                ToolbarBoxStyle.normal.background = Icons.ToolbarBox;

                HierarchyBoxStyle = new GUIStyle();
                HierarchyBoxStyle.padding = new RectOffset(0, 0, 0, 0);
                HierarchyBoxStyle.alignment = TextAnchor.MiddleLeft;
                HierarchyBoxStyle.border = new RectOffset(0, 0, 0, 0);
                HierarchyBoxStyle.normal.background = Icons.HierarchyBox;

                HierarchyToolsStyle = new GUIStyle(HierarchyBoxStyle);
                HierarchyToolsStyle.normal.background = Icons.HierarchyTools;

                MarkerLabelStyle = new GUIStyle(GUI.skin.label);
                MarkerLabelStyle.normal.textColor = AxonColor.Default;
                MarkerLabelStyle.fontSize = 12;

                EventLabelStyle = new GUIStyle(GUI.skin.label);
                EventLabelStyle.normal.background = Icons.SubSelected;
                EventLabelStyle.normal.textColor = AxonColor.Default;
                EventLabelStyle.fontSize = 10;

                ObjectStyle = new GUIStyle(GUI.skin.label);
                ObjectSelectedStyle = new GUIStyle(GUI.skin.label);
                ObjectSelectedStyle.normal.background = Icons.Selected;
                ObjectSelectedStyle.normal.textColor = AxonColor.Selected;

                ObjectSelectedDefocusStyle = new GUIStyle(GUI.skin.label);
                ObjectSelectedDefocusStyle.normal.background = Icons.SelectedDefocus;
                ObjectSelectedDefocusStyle.normal.textColor = AxonColor.Selected;

                ObjectDragStyle = new GUIStyle(GUI.skin.label);
                ObjectDragStyle.normal.background = Icons.Selected;
                ObjectDragStyle.normal.textColor = AxonColor.LabelDrag;
                ObjectDragStyle.fontStyle = FontStyle.Bold;

                ObjectTextFieldStyle = new GUIStyle(GUI.skin.textField);
                ObjectTextFieldStyle.alignment = TextAnchor.MiddleLeft;

                ChannelTextFieldStyle = new GUIStyle(GUI.skin.textField);
                ChannelTextFieldStyle.alignment = TextAnchor.MiddleLeft;

                SubObjectStyle = new GUIStyle(ObjectStyle);
                SubObjectStyle.padding.top = 3;
                SubObjectStyle.margin = new RectOffset(0, 0, 0, 0);
                SubObjectStyle.normal.background = Icons.Sub;

                SubObjectSelectedStyle = new GUIStyle(GUI.skin.label);
                SubObjectSelectedStyle.padding.top = 3;
                SubObjectSelectedStyle.margin = new RectOffset(0, 0, 0, 0);
                SubObjectSelectedStyle.normal.background = Icons.SubSelected;


                SubObjectDeselectedStyle = new GUIStyle(GUI.skin.label);
                SubObjectDeselectedStyle.padding.top = 3;
                SubObjectDeselectedStyle.margin = new RectOffset(0, 0, 0, 0);
                SubObjectDeselectedStyle.normal.background = Icons.SubDeselected;

                ChannelLabelStyle = new GUIStyle();
                ChannelLabelStyle.alignment = TextAnchor.MiddleRight;
                ChannelLabelStyle.clipping = TextClipping.Clip;


                // TRACK STYLES
                TrackStyle = new GUIStyle();
                TrackStyle.padding = new RectOffset(6, 0, 0, 0);
                TrackStyle.alignment = TextAnchor.MiddleLeft;
                TrackStyle.border = new RectOffset(3, 3, 3, 3);
                TrackStyle.normal.background = Icons.Track;
                TrackStyle.normal.textColor = Color.white;

                TrackShadowStyle = new GUIStyle();
                TrackShadowStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackShadowStyle.alignment = TextAnchor.MiddleLeft;
                TrackShadowStyle.border = new RectOffset(0, 0, 0, 0);
                TrackShadowStyle.normal.background = Icons.DarkBox;

                TrackSelectedStyle = new GUIStyle();
                TrackSelectedStyle.padding = TrackStyle.padding;
                TrackSelectedStyle.alignment = TextAnchor.MiddleLeft;
                TrackSelectedStyle.border = new RectOffset(4, 4, 4, 4);
                TrackSelectedStyle.normal.background = Icons.TrackSelected;
                TrackSelectedStyle.normal.textColor = Color.white;

                TrackSelectedPatternStyle = new GUIStyle();
                TrackSelectedPatternStyle.padding = TrackStyle.padding;
                TrackSelectedPatternStyle.alignment = TextAnchor.MiddleCenter;
                TrackSelectedPatternStyle.normal.background = Icons.TrackSelectedPattern;
                TrackSelectedPatternStyle.normal.background.wrapMode = TextureWrapMode.Repeat;

                TrackSelectedCopyStyle = new GUIStyle();
                TrackSelectedCopyStyle.padding = TrackStyle.padding;
                TrackSelectedCopyStyle.alignment = TextAnchor.MiddleLeft;
                TrackSelectedCopyStyle.border = new RectOffset(4, 4, 4, 4);
                TrackSelectedCopyStyle.normal.background = Icons.TrackSelectedCopy;

                TrackEmptyStyle = new GUIStyle();
                TrackEmptyStyle.padding = TrackStyle.padding;
                TrackEmptyStyle.alignment = TextAnchor.MiddleLeft;
                TrackEmptyStyle.border = new RectOffset(2, 2, 2, 2);
                TrackEmptyStyle.normal.background = Icons.TrackEmpty;

                TrackOffStyle = new GUIStyle();
                TrackOffStyle.padding = TrackStyle.padding;
                TrackOffStyle.alignment = TextAnchor.MiddleLeft;
                TrackOffStyle.border = new RectOffset(2, 2, 2, 2);
                TrackOffStyle.normal.background = Icons.TrackOff;

                TrackDisabledStyle = new GUIStyle();
                TrackDisabledStyle.padding = TrackStyle.padding;
                TrackDisabledStyle.alignment = TextAnchor.MiddleLeft;
                TrackDisabledStyle.border = new RectOffset(3, 3, 3, 3);
                TrackDisabledStyle.normal.background = Icons.TrackDisabled;

                TrackLoopStyle = new GUIStyle(TrackStyle);
                TrackLoopStyle.fontSize = 9;
                TrackLoopStyle.normal.background = Icons.TrackLoop;
                TrackLoopStyle.normal.textColor = Color.white;
                TrackLoopStyle.active.textColor = Color.black;

                TrackMixLinearStyle = new GUIStyle();
                TrackMixLinearStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixLinearStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixLinearStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixLinearStyle.normal.background = Icons.TrackMixLinear;

                TrackMixEaseStyle = new GUIStyle();
                TrackMixEaseStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixEaseStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixEaseStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixEaseStyle.normal.background = Icons.TrackMixEase;

                TrackMixLinearInverseStyle = new GUIStyle();
                TrackMixLinearInverseStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixLinearInverseStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixLinearInverseStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixLinearInverseStyle.normal.background = Icons.TrackMixLinearInverse;

                TrackMixEaseInverseStyle = new GUIStyle();
                TrackMixEaseInverseStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixEaseInverseStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixEaseInverseStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixEaseInverseStyle.normal.background = Icons.TrackMixEaseInverse;

                TrackMixFadedStyle = new GUIStyle();
                TrackMixFadedStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixFadedStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixFadedStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixFadedStyle.normal.background = Icons.TrackMixFaded;

                TrackMixFadedInverseStyle = new GUIStyle();
                TrackMixFadedInverseStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixFadedInverseStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixFadedInverseStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixFadedInverseStyle.normal.background = Icons.TrackMixFadedInverse;

                TrackMixHoldStyle = new GUIStyle();
                TrackMixHoldStyle.padding = new RectOffset(0, 0, 0, 0);
                TrackMixHoldStyle.alignment = TextAnchor.MiddleLeft;
                TrackMixHoldStyle.border = new RectOffset(0, 0, 0, 0);
                TrackMixHoldStyle.normal.background = Icons.TrackMixHold;

                AudioTrackStyle = new GUIStyle(GUI.skin.box);

                // MIDI STYLES
                MidiNoteStyle = new GUIStyle();
                MidiNoteStyle.padding = new RectOffset(0, 0, 0, 0);
                MidiNoteStyle.alignment = TextAnchor.MiddleLeft;
                MidiNoteStyle.border = new RectOffset(3, 3, 3, 3);
                MidiNoteStyle.normal.background = Icons.MidiNote;

                TimeRangeStyle = new GUIStyle(DarkBoxStyle);
                TimeRangeStyle.normal.background = Icons.TimeRange;

                TimeRangeEmptyStyle = new GUIStyle(DarkBoxStyle);
                TimeRangeEmptyStyle.normal.background = Icons.TimeRangeEmpty;

                MarqueeStyle = new GUIStyle();
                MarqueeStyle.padding = new RectOffset(0, 0, 0, 0);
                MarqueeStyle.alignment = TextAnchor.MiddleLeft;
                MarqueeStyle.border = new RectOffset(2, 2, 2, 2);
                MarqueeStyle.normal.background = Icons.Marquee;

                ScrollbarInStyle = new GUIStyle(TextureButtonStyle);
                ScrollbarInStyle.normal.background = Icons.TimeRangeIn;

                ScrollbarOutStyle = new GUIStyle(TextureButtonStyle);
                ScrollbarOutStyle.normal.background = Icons.TimeRangeOut;

                DisplayPrevLabel = new GUIContent();
                DisplayPrevLabel.tooltip = "Display Previous";

                DisplayNextLabel = new GUIContent();
                DisplayNextLabel.tooltip = "Display Next";

                ColumnExpandLabel = new GUIContent();
                ColumnExpandLabel.tooltip = "Expand Switches Column";

                ColumnExpandRightLabel = new GUIContent();
                ColumnExpandRightLabel.tooltip = "Expand Values Column";

                ColumnExpandOnStyle = new GUIStyle(TextureButtonStyle);
                ColumnExpandOnStyle.normal.background = Icons.ColumnExpandOn;
                ColumnExpandOnStyle.active.background = Icons.ColumnExpandOff;

                ColumnExpandOffStyle = new GUIStyle(TextureButtonStyle);
                ColumnExpandOffStyle.normal.background = ColumnExpandOnStyle.active.background;
                ColumnExpandOffStyle.active.background = ColumnExpandOnStyle.normal.background;

                ColumnExpandRightOnStyle = new GUIStyle(TextureButtonStyle);
                ColumnExpandRightOnStyle.normal.background = Icons.ColumnExpandRightOn;
                ColumnExpandRightOnStyle.active.background = Icons.ColumnExpandRightOff;

                ColumnExpandRightOffStyle = new GUIStyle(TextureButtonStyle);
                ColumnExpandRightOffStyle.normal.background = ColumnExpandRightOnStyle.active.background;
                ColumnExpandRightOffStyle.active.background = ColumnExpandRightOnStyle.normal.background;

                WorkAreaInMarkerStyle = new GUIStyle(TextureButtonStyle);
                WorkAreaInMarkerStyle.normal.background = Icons.WorkAreaIn;

                WorkAreaOutMarkerStyle = new GUIStyle(TextureButtonStyle);
                WorkAreaOutMarkerStyle.normal.background = Icons.WorkAreaOut;

                PlayheadStyle = new GUIStyle(TextureButtonStyle);
                PlayheadStyle.normal.background = Icons.Playhead;

                EndTimeStyle = new GUIStyle(TextureButtonStyle);
                EndTimeStyle.normal.background = Icons.EndTime;

                EndTimeLabel = new GUIContent();
                EndTimeLabel.tooltip = "End Time (Click to Edit)";

                MarkerStyle = new GUIStyle(TextureButtonStyle);
                MarkerStyle.normal.background = Icons.Marker;
                MarkerStyle.fontSize = 9;

                MarkerSelStyle = new GUIStyle(TextureButtonStyle);
                MarkerSelStyle.normal.background = Icons.MarkerSelected;
                MarkerSelStyle.fontSize = 9;

                FoldoutDownStyle = new GUIStyle(TextureButtonStyle);
                FoldoutDownStyle.normal.background = AxonUI.Icons.FoldoutDown;

                FoldoutUpStyle = new GUIStyle(TextureButtonStyle);
                FoldoutUpStyle.normal.background = AxonUI.Icons.FoldoutUp;

                FoldoutDownPlusStyle = new GUIStyle(TextureButtonStyle);
                FoldoutDownPlusStyle.normal.background = AxonUI.Icons.FoldoutDownPlus;

                FoldoutUpPlusStyle = new GUIStyle(TextureButtonStyle);
                FoldoutUpPlusStyle.normal.background = AxonUI.Icons.FoldoutUpPlus;

                FoldoutNoneStyle = new GUIStyle(TextureButtonStyle);
                FoldoutNoneStyle.normal.background = AxonUI.Icons.FoldoutNone;


                AudioOnStyle = new GUIStyle(TextureButtonStyle);
                AudioOnStyle.normal.background = Icons.AudioOn;
                AudioOnStyle.active.background = Icons.AudioOff;

                AudioOffStyle = new GUIStyle(TextureButtonStyle);
                AudioOffStyle.normal.background = AudioOnStyle.active.background;
                AudioOffStyle.active.background = AudioOnStyle.normal.background;

                MinMaxFieldToggleOnStyle = new GUIStyle(TextureButtonStyle);
                MinMaxFieldToggleOnStyle.normal.background = Icons.MinMaxFieldToggleOn;
                MinMaxFieldToggleOnStyle.active.background = Icons.MinMaxFieldToggleOff;

                MinMaxFieldToggleOffStyle = new GUIStyle(TextureButtonStyle);
                MinMaxFieldToggleOffStyle.normal.background = MinMaxFieldToggleOnStyle.active.background;
                MinMaxFieldToggleOffStyle.active.background = MinMaxFieldToggleOnStyle.normal.background;


                AudioWaveformOnStyle = new GUIStyle(TextureButtonStyle);
                AudioWaveformOnStyle.normal.background = Icons.AudioWaveformOn;
                AudioWaveformOnStyle.active.background = Icons.AudioWaveformOff;

                AudioWaveformOffStyle = new GUIStyle(TextureButtonStyle);
                AudioWaveformOffStyle.normal.background = AudioWaveformOnStyle.active.background;
                AudioWaveformOffStyle.active.background = AudioWaveformOnStyle.normal.background;

                TimeModeLabel = new GUIContent();
                TimeModeLabel.tooltip = "Enable Behavior";

                KeyframeStyle = new GUIStyle(TextureButtonStyle);
                KeyframeStyle.normal.background = Icons.Keyframe;

                KeyframeSelectedStyle = new GUIStyle(TextureButtonStyle);
                KeyframeSelectedStyle.normal.background = Icons.KeyframeSelected;

                KeyframeSelectedCopyStyle = new GUIStyle(TextureButtonStyle);
                KeyframeSelectedCopyStyle.normal.background = Icons.KeyframeSelectedCopy;

                KeyframeObjectStyle = new GUIStyle(TextureButtonStyle);
                KeyframeObjectStyle.normal.background = Icons.KeyframeObject;

                KeyframeObjectSelectedStyle = new GUIStyle(TextureButtonStyle);
                KeyframeObjectSelectedStyle.normal.background = Icons.KeyframeObjectSelected;

                KeyframeHoldStyle = new GUIStyle(TextureButtonStyle);
                KeyframeHoldStyle.normal.background = Icons.KeyframeHold;

                KeyframeHoldSelectedStyle = new GUIStyle(TextureButtonStyle);
                KeyframeHoldSelectedStyle.normal.background = Icons.KeyframeHoldSelected;

                BezierBrokenHandleStyle = new GUIStyle(TextureButtonStyle);
                BezierBrokenHandleStyle.normal.background = Icons.BezierBrokenHandle;

                BezierUnifiedHandleStyle = new GUIStyle(TextureButtonStyle);
                BezierUnifiedHandleStyle.normal.background = Icons.BezierUnifiedHandle;

                BezierEqualHandleStyle = new GUIStyle(TextureButtonStyle);
                BezierEqualHandleStyle.normal.background = Icons.BezierEqualHandle;

                LoopHandleStyle = new GUIStyle(TextureButtonStyle);
                LoopHandleStyle.normal.background = Icons.LoopHandle;

                LoopHandleVerticalStyle = new GUIStyle(TextureButtonStyle);
                LoopHandleVerticalStyle.normal.background = Icons.LoopHandleVertical;


                EventStyle = new GUIStyle(TextureButtonStyle);
                EventStyle.normal.background = Icons.EventEnabled;

                EventDisabledStyle = new GUIStyle(TextureButtonStyle);
                EventDisabledStyle.normal.background = Icons.EventDisabled;

                EventDisabledSelStyle = new GUIStyle(TextureButtonStyle);
                EventDisabledSelStyle.normal.background = Icons.EventDisabledSelected;

                EventSelectedStyle = new GUIStyle(TextureButtonStyle);
                EventSelectedStyle.normal.background = Icons.EventSelected;


                ToolbarDivider = new GUIStyle(TextureButtonStyle);
                ToolbarDivider.normal.background = Icons.ToolbarDivider;

                InterpChanNoneLabel = new GUIContent();
                InterpChanNoneLabel.tooltip = "Interpolate None";

                InterpChanNone = new GUIStyle(TextureButtonStyle);
                InterpChanNone.normal.background = Icons.InterpChanNone;

                InterpChanLinearLabel = new GUIContent();
                InterpChanLinearLabel.tooltip = "Interpolate Linear";

                InterpChanLinear = new GUIStyle(TextureButtonStyle);
                InterpChanLinear.normal.background = Icons.InterpChanLinear;

                InterpChanBezierLabel = new GUIContent();
                InterpChanBezierLabel.tooltip = "Interpolate Bezier";

                InterpChanBezier = new GUIStyle(TextureButtonStyle);
                InterpChanBezier.normal.background = Icons.InterpChanBezier;

                InterpChanQuadLabel = new GUIContent();
                InterpChanQuadLabel.tooltip = "Interpolate Quadratic";

                InterpChanQuad = new GUIStyle(TextureButtonStyle);
                InterpChanQuad.normal.background = Icons.InterpChanQuad;

                InterpLinearLabel = new GUIContent();
                InterpLinearLabel.tooltip = "Interpolate Linear";

                InterpLinearStyle = new GUIStyle(TextureButtonStyle);
                InterpLinearStyle.normal.background = Icons.InterpLinearOn;
                InterpLinearStyle.active.background = Icons.InterpLinearPress;

                InterpLinearStyleOff = new GUIStyle(TextureButtonStyle);
                InterpLinearStyleOff.normal.background = Icons.InterpLinearOff;
                InterpLinearStyleOff.active.background = Icons.InterpLinearPress;

                InterpHoldLabel = new GUIContent();
                InterpHoldLabel.tooltip = "Interpolate Hold";

                InterpHoldStyle = new GUIStyle(TextureButtonStyle);
                InterpHoldStyle.normal.background = Icons.InterpHoldOn;
                InterpHoldStyle.active.background = Icons.InterpHoldPress;

                InterpHoldStyleOff = new GUIStyle(TextureButtonStyle);
                InterpHoldStyleOff.normal.background = Icons.InterpHoldOff;
                InterpHoldStyleOff.active.background = Icons.InterpHoldPress;

                InterpVerticalLabel = new GUIContent();
                InterpVerticalLabel.tooltip = "Vertical Tangents";

                InterpVerticalStyle = new GUIStyle(TextureButtonStyle);
                InterpVerticalStyle.normal.background = Icons.InterpVerticalOff;
                InterpVerticalStyle.active.background = Icons.InterpVerticalOn;

                InterpFlatLabel = new GUIContent();
                InterpFlatLabel.tooltip = "Flatten Tangents";

                InterpFlatStyle = new GUIStyle(TextureButtonStyle);
                InterpFlatStyle.normal.background = Icons.InterpFlatOff;
                InterpFlatStyle.active.background = Icons.InterpFlatOn;

                InterpFlatLeftLabel = new GUIContent();
                InterpFlatLeftLabel.tooltip = "Flatten Left Tangent";

                InterpFlatLeftStyle = new GUIStyle(TextureButtonStyle);
                InterpFlatLeftStyle.normal.background = Icons.InterpFlatLeftOff;
                InterpFlatLeftStyle.active.background = Icons.InterpFlatLeftOn;

                InterpFlatRightLabel = new GUIContent();
                InterpFlatRightLabel.tooltip = "Flatten Right Tangent";

                InterpFlatRightStyle = new GUIStyle(TextureButtonStyle);
                InterpFlatRightStyle.normal.background = Icons.InterpFlatRightOff;
                InterpFlatRightStyle.active.background = Icons.InterpFlatRightOn;

                InterpAutoLabel = new GUIContent();
                InterpAutoLabel.tooltip = "Auto Calculate Tangents";

                InterpAutoOnStyle = new GUIStyle(TextureButtonStyle);
                InterpAutoOnStyle.normal.background = Icons.InterpAutoOn;
                InterpAutoOnStyle.active.background = Icons.InterpAutoOff;

                InterpAutoOffStyle = new GUIStyle(TextureButtonStyle);
                InterpAutoOffStyle.normal.background = InterpAutoOnStyle.active.background;
                InterpAutoOffStyle.active.background = InterpAutoOnStyle.normal.background;

                ToolSelectLabel = new GUIContent();
                ToolSelectLabel.tooltip = "Select Tool";

                ToolSelectStyle = new GUIStyle(TextureButtonStyle);
                ToolSelectStyle.normal.background = Icons.ToolSelect;
                ToolSelectStyle.active.background = Icons.ToolEditTangents;

                ToolEditTangentsLabel = new GUIContent();
                ToolEditTangentsLabel.tooltip = "Edit Tangents Tool";

                ToolEditTangentsStyle = new GUIStyle(TextureButtonStyle);
                ToolEditTangentsStyle.normal.background = ToolSelectStyle.active.background;
                ToolEditTangentsStyle.active.background = ToolSelectStyle.normal.background;

                ToolEditKeysOnlyLabel = new GUIContent();
                ToolEditKeysOnlyLabel.tooltip = "Edit Keys Only Tool";

                ToolsEditKeysOnlyStyle = new GUIStyle(TextureButtonStyle);
                ToolsEditKeysOnlyStyle.normal.background = Icons.ToolEditKeysOnly;
                ToolsEditKeysOnlyStyle.active.background = ToolSelectStyle.normal.background;

                UnifiedTangentsLabel = new GUIContent();
                UnifiedTangentsLabel.tooltip = "Unify Tangents";

                UnifyTangentsOn = new GUIStyle(TextureButtonStyle);
                UnifyTangentsOn.normal.background = Icons.UnifyTangentsOn;

                UnifyTangentsOff = new GUIStyle(TextureButtonStyle);
                UnifyTangentsOff.normal.background = Icons.UnifyTangentsOff;

                UnifyTangentLengths = new GUIStyle(TextureButtonStyle);
                UnifyTangentLengths.normal.background = Icons.UnifyTangentLengths;

                HierarchyAddOnStyle = new GUIStyle(TextureButtonStyle);
                HierarchyAddOnStyle.normal.background = Icons.HierarchyAdd;

                HierarchyAddOffStyle = new GUIStyle(TextureButtonStyle);
                HierarchyAddOffStyle.normal.background = Icons.HierarchyAddOff;

                PrevKeyStyle = new GUIStyle(TextureButtonStyle);
                PrevKeyStyle.normal.background = Icons.PrevKey;

                PrevKeyNoneStyle = new GUIStyle(TextureButtonStyle);
                PrevKeyNoneStyle.normal.background = Icons.PrevKeyNone;

                NextKeyStyle = new GUIStyle(TextureButtonStyle);
                NextKeyStyle.normal.background = Icons.NextKey;

                NextKeyNoneStyle = new GUIStyle(TextureButtonStyle);
                NextKeyNoneStyle.normal.background = Icons.NextKeyNone;

                ToggleKeyOnStyle = new GUIStyle(TextureButtonStyle);
                ToggleKeyOnStyle.normal.background = Icons.ToggleKeyOn;
                ToggleKeyOnStyle.active.background = Icons.ToggleKeyOff;

                ToggleKeyOffStyle = new GUIStyle(TextureButtonStyle);
                ToggleKeyOffStyle.normal.background = Icons.ToggleKeyOff;
                ToggleKeyOffStyle.active.background = Icons.ToggleKeyOn;



                ChannelLinkOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelLinkOnStyle.normal.background = Icons.ChannelLinkOn;
                ChannelLinkOnStyle.active.background = Icons.ChannelLinkSelect;

                ChannelLinkOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelLinkOffStyle.normal.background = Icons.ChannelLinkOff;
                ChannelLinkOffStyle.active.background = Icons.ChannelLinkSelect;

                ChannelLinkRemoveStyle = new GUIStyle(TextureButtonStyle);
                ChannelLinkRemoveStyle.normal.background = Icons.ChannelLinkRemove;
                ChannelLinkRemoveStyle.active.background = Icons.ChannelLinkRemove;

                ChannelLinkSelectStyle = new GUIStyle(TextureButtonStyle);
                ChannelLinkSelectStyle.normal.background = Icons.ChannelLinkSelect;
                ChannelLinkSelectStyle.active.background = Icons.ChannelLinkSelect;

                ChannelLinkTargetStyle = new GUIStyle(TextureButtonStyle);
                ChannelLinkTargetStyle.normal.background = Icons.ChannelLinkTarget;
                ChannelLinkTargetStyle.active.background = Icons.ChannelLinkTarget;


                GraphOnStyle = new GUIStyle(TextureButtonStyle);
                GraphOnStyle.normal.background = Icons.GraphOn;
                GraphOnStyle.active.background = Icons.GraphOff;

                GraphLockedStyle = new GUIStyle(TextureButtonStyle);
                GraphLockedStyle.normal.background = Icons.GraphLocked;
                GraphLockedStyle.active.background = Icons.GraphOff;

                GraphOffStyle = new GUIStyle(TextureButtonStyle);
                GraphOffStyle.normal.background = GraphOnStyle.active.background;
                GraphOffStyle.active.background = GraphOnStyle.normal.background;

                Icons.DeleteOn = Icons.DeleteOn;
                Icons.DeleteOff = Icons.DeleteOff;

                DeleteOnStyle = new GUIStyle(TextureButtonStyle);
                DeleteOnStyle.normal.background = Icons.DeleteOn;
                DeleteOnStyle.active.background = Icons.DeleteOff;

                DeleteOffStyle = new GUIStyle(TextureButtonStyle);
                DeleteOffStyle.normal.background = DeleteOnStyle.active.background;
                DeleteOffStyle.active.background = DeleteOnStyle.normal.background;

                TrackDragTimeOffsetOnStyle = new GUIStyle(TextureButtonStyle);
                TrackDragTimeOffsetOnStyle.normal.background = Icons.TrackDragTimeOffsetOn;
                TrackDragTimeOffsetOnStyle.active.background = Icons.TrackDragTimeOffsetOff;

                TrackDragTimeOffsetOffStyle = new GUIStyle(TextureButtonStyle);
                TrackDragTimeOffsetOffStyle.normal.background = TrackDragTimeOffsetOnStyle.active.background;
                TrackDragTimeOffsetOffStyle.active.background = TrackDragTimeOffsetOnStyle.normal.background;


                TimeRulerLabelStyle = new GUIStyle(GUI.skin.label);
                TimeRulerLabelStyle.fontSize = 10;
                TimeRulerLabelStyle.alignment = TextAnchor.UpperCenter;


                ChannelXLabel = new GUIContent();
                ChannelXLabel.tooltip = "Toggle Channel X | R";

                ChannelYLabel = new GUIContent();
                ChannelYLabel.tooltip = "Toggle Channel Y | G";

                ChannelZLabel = new GUIContent();
                ChannelZLabel.tooltip = "Toggle Channel Z | B";

                ChannelALabel = new GUIContent();
                ChannelALabel.tooltip = "Toggle Channel W / A";

                ChannelLoopLabel = new GUIContent();
                ChannelLoopLabel.tooltip = "Loop Channel";

                ChannelLoopAutoOnLabel = new GUIContent();
                ChannelLoopAutoOnLabel.tooltip = "Auto Loop";

                ChannelLoopAutoOffLabel = new GUIContent();
                ChannelLoopAutoOffLabel.tooltip = "Auto Loop (Off)";

                ChannelLoopInOnLabel = new GUIContent();
                ChannelLoopInOnLabel.tooltip = "Loop In";

                ChannelLoopInOffLabel = new GUIContent();
                ChannelLoopInOffLabel.tooltip = "Loop In (Off)";

                ChannelLoopOutOnLabel = new GUIContent();
                ChannelLoopOutOnLabel.tooltip = "Loop Out";

                ChannelLoopOutOffLabel = new GUIContent();
                ChannelLoopOutOffLabel.tooltip = "Loop Out (Off)";

                ChannelLoopMatchLabel = new GUIContent();
                ChannelLoopMatchLabel.tooltip = "Match End Points";

                ChannelXOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelXOnStyle.normal.background = Icons.ChannelXOn;
                ChannelXOnStyle.active.background = Icons.ChannelXOff;

                ChannelXOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelXOffStyle.normal.background = ChannelXOnStyle.active.background;
                ChannelXOffStyle.active.background = ChannelXOnStyle.normal.background;

                ChannelYOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelYOnStyle.normal.background = Icons.ChannelYOn;
                ChannelYOnStyle.active.background = Icons.ChannelYOff;

                ChannelYOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelYOffStyle.normal.background = ChannelYOnStyle.active.background;
                ChannelYOffStyle.active.background = ChannelYOnStyle.normal.background;

                ChannelZOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelZOnStyle.normal.background = Icons.ChannelZOn;
                ChannelZOnStyle.active.background = Icons.ChannelZOff;

                ChannelZOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelZOffStyle.normal.background = ChannelZOnStyle.active.background;
                ChannelZOffStyle.active.background = ChannelZOnStyle.normal.background;

                ChannelAOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelAOnStyle.normal.background = Icons.ChannelAOn;
                ChannelAOnStyle.active.background = Icons.ChannelAOff;

                ChannelAOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelAOffStyle.normal.background = ChannelAOnStyle.active.background;
                ChannelAOffStyle.active.background = ChannelAOnStyle.normal.background;

                ChannelLoopOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopOnStyle.normal.background = Icons.ChannelLoopOn;
                ChannelLoopOnStyle.active.background = Icons.ChannelLoopPingPong;

                ChannelLoopPingPongStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopPingPongStyle.normal.background = Icons.ChannelLoopPingPong;
                ChannelLoopPingPongStyle.active.background = Icons.ChannelLoopOff;

                ChannelLoopOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopOffStyle.normal.background = Icons.ChannelLoopOff;
                ChannelLoopOffStyle.active.background = Icons.ChannelLoopOn;

                ChannelLoopHalfStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopHalfStyle.normal.background = Icons.ChannelLoopHalf;
                ChannelLoopHalfStyle.active.background = Icons.ChannelLoopOn;

                ChannelLoopFreeStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopFreeStyle.normal.background = Icons.ChannelLoopFree;
                ChannelLoopFreeStyle.active.background = Icons.ChannelLoopMatch;

                ChannelLoopAutoOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopAutoOnStyle.normal.background = Icons.ChannelLoopAutoOn;
                ChannelLoopAutoOnStyle.active.background = Icons.ChannelLoopAutoOff;

                ChannelLoopAutoOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopAutoOffStyle.normal.background = Icons.ChannelLoopAutoOff;
                ChannelLoopAutoOffStyle.active.background = Icons.ChannelLoopAutoOn;

                ChannelLoopInOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopInOnStyle.normal.background = Icons.ChannelLoopInOn;
                ChannelLoopInOnStyle.active.background = Icons.ChannelLoopInOff;

                ChannelLoopInOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopInOffStyle.normal.background = Icons.ChannelLoopInOff;
                ChannelLoopInOffStyle.active.background = Icons.ChannelLoopInOn;

                ChannelLoopOutOnStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopOutOnStyle.normal.background = Icons.ChannelLoopOutOn;
                ChannelLoopOutOnStyle.active.background = Icons.ChannelLoopOutOff;

                ChannelLoopOutOffStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopOutOffStyle.normal.background = Icons.ChannelLoopOutOff;
                ChannelLoopOutOffStyle.active.background = Icons.ChannelLoopOutOn;

                ChannelLoopMatchStyle = new GUIStyle(TextureButtonStyle);
                ChannelLoopMatchStyle.normal.background = Icons.ChannelLoopMatch;
                ChannelLoopMatchStyle.active.background = Icons.ChannelLoopFree;

                DisplayVisibileLabel = new GUIContent();
                DisplayVisibileLabel.tooltip = "Display Visibile Only";

                VisibilityOnStyle = new GUIStyle(TextureButtonStyle);
                VisibilityOnStyle.normal.background = AxonUI.Icons.VisibilityOn;
                VisibilityOnStyle.active.background = AxonUI.Icons.VisibilityHalf;

                VisibilityHalfStyle = new GUIStyle(TextureButtonStyle);
                VisibilityHalfStyle.normal.background = AxonUI.Icons.VisibilityHalf;
                VisibilityHalfStyle.active.background = VisibilityOnStyle.normal.background;

                VisibilityOffStyle = new GUIStyle(TextureButtonStyle);
                VisibilityOffStyle.normal.background = AxonUI.Icons.VisibilityOff;
                VisibilityOffStyle.active.background = AxonUI.Icons.VisibilityOn;

                VisibilityOffFadedStyle = new GUIStyle(TextureButtonStyle);
                VisibilityOffFadedStyle.normal.background = AxonUI.Icons.VisibilityOffFaded;
                VisibilityOffFadedStyle.active.background = AxonUI.Icons.VisibilityOn;


                GridSnapLabel = new GUIContent();
                GridSnapLabel.tooltip = "Grid Snap Time (H)";

                GridSnapOnStyle = new GUIStyle(TextureButtonStyle);
                GridSnapOnStyle.normal.background = Icons.GridSnapOn;
                GridSnapOnStyle.active.background = Icons.GridSnapOff;

                GridSnapOffStyle = new GUIStyle(TextureButtonStyle);
                GridSnapOffStyle.normal.background = GridSnapOnStyle.active.background;
                GridSnapOffStyle.active.background = GridSnapOnStyle.normal.background;


                GridSnapVertLabel = new GUIContent();
                GridSnapVertLabel.tooltip = "Grid Snap Values (J)";

                GridSnapVertOnStyle = new GUIStyle(TextureButtonStyle);
                GridSnapVertOnStyle.normal.background = Icons.GridSnapVertOn;
                GridSnapVertOnStyle.active.background = Icons.GridSnapVertOff;

                GridSnapVertOffStyle = new GUIStyle(TextureButtonStyle);
                GridSnapVertOffStyle.normal.background = GridSnapVertOnStyle.active.background;
                GridSnapVertOffStyle.active.background = GridSnapVertOnStyle.normal.background;


                MarkersLabel = new GUIContent();
                MarkersLabel.tooltip = "Show Markers (M)";

                MarkersOnStyle = new GUIStyle(TextureButtonStyle);
                MarkersOnStyle.normal.background = Icons.MarkersOn;
                MarkersOnStyle.active.background = Icons.MarkersOff;

                MarkersOffStyle = new GUIStyle(TextureButtonStyle);
                MarkersOffStyle.normal.background = MarkersOnStyle.active.background;
                MarkersOffStyle.active.background = MarkersOnStyle.normal.background;


                ShowKeyframeValuesLabel = new GUIContent();
                ShowKeyframeValuesLabel.tooltip = "Show Keyframe Values";

                KeyframeValuesOnStyle = new GUIStyle(TextureButtonStyle);
                KeyframeValuesOnStyle.normal.background = Icons.KeyframeValuesOn;
                KeyframeValuesOnStyle.active.background = Icons.KeyframeValuesOff;

                KeyframeValuesOffStyle = new GUIStyle(TextureButtonStyle);
                KeyframeValuesOffStyle.normal.background = KeyframeValuesOnStyle.active.background;
                KeyframeValuesOffStyle.active.background = KeyframeValuesOnStyle.normal.background;


                DirectorSyncLabel = new GUIContent();
                DirectorSyncLabel.tooltip = "Sync Timeline Director";

                DirectorSyncOnStyle = new GUIStyle(TextureButtonStyle);
                DirectorSyncOnStyle.normal.background = Icons.DirectorSyncOn;
                DirectorSyncOnStyle.active.background = Icons.DirectorSyncOff;

                DirectorSyncOffStyle = new GUIStyle(TextureButtonStyle);
                DirectorSyncOffStyle.normal.background = DirectorSyncOnStyle.active.background;
                DirectorSyncOffStyle.active.background = DirectorSyncOnStyle.normal.background;


                GridLabel = new GUIContent();
                GridLabel.tooltip = "Toggle Grid (R)";

                GridOnStyle = new GUIStyle(TextureButtonStyle);
                GridOnStyle.normal.background = Icons.GridOn;
                GridOnStyle.active.background = Icons.GridOff;

                GridOffStyle = new GUIStyle(TextureButtonStyle);
                GridOffStyle.normal.background = GridOnStyle.active.background;
                GridOffStyle.active.background = GridOnStyle.normal.background;

                DisplayAnimatedOnlyLabel = new GUIContent();
                DisplayAnimatedOnlyLabel.tooltip = "Display active behaviors only";


                DisplayShowChannelsLabel = new GUIContent();
                DisplayShowChannelsLabel.tooltip = "Show channels";


                IsMinimizedLabel = new GUIContent();
                IsMinimizedLabel.tooltip = "Minimize Window";

                IsMinimizedOnStyle = new GUIStyle(TextureButtonStyle);
                IsMinimizedOnStyle.normal.background = Icons.IsMinimizedOn;
                IsMinimizedOnStyle.active.background = Icons.IsMinimizedOff;

                IsMinimizedOffStyle = new GUIStyle(TextureButtonStyle);
                IsMinimizedOffStyle.normal.background = IsMinimizedOnStyle.active.background;
                IsMinimizedOffStyle.active.background = IsMinimizedOnStyle.normal.background;


                BPMLabel = new GUIContent();
                BPMLabel.tooltip = "Musical Timing";

                MusicalTimingOnStyle = new GUIStyle(TextureButtonStyle);
                MusicalTimingOnStyle.normal.background = Icons.MusicalTimingOn;
                MusicalTimingOnStyle.active.background = Icons.MusicalTimingOff;

                MusicalTimingOffStyle = new GUIStyle(TextureButtonStyle);
                MusicalTimingOffStyle.normal.background = MusicalTimingOnStyle.active.background;
                MusicalTimingOffStyle.active.background = MusicalTimingOnStyle.normal.background;


                KeySelectModeLabel = new GUIContent();
                KeySelectModeLabel.tooltip = "Selection Mode";

                KeySelectModeAllStyle = new GUIStyle(TextureButtonStyle);
                KeySelectModeAllStyle.normal.background = Icons.KeySelectModeAll;
                KeySelectModeAllStyle.active.background = Icons.KeySelectModeKeys;

                KeySelectModeKeysStyle = new GUIStyle(TextureButtonStyle);
                KeySelectModeKeysStyle.normal.background = Icons.KeySelectModeKeys;
                KeySelectModeKeysStyle.active.background = Icons.KeySelectModeTracks;

                KeySelectModeTracksStyle = new GUIStyle(TextureButtonStyle);
                KeySelectModeTracksStyle.normal.background = Icons.KeySelectModeTracks;
                KeySelectModeTracksStyle.active.background = Icons.KeySelectModeAll;


                AlignTimeLeftStyle = new GUIStyle(TextureButtonStyle);
                AlignTimeLeftStyle.normal.background = Icons.AlignTimeLeftOff;
                AlignTimeLeftStyle.active.background = Icons.AlignTimeLeftOn;

                AlignTimeRightStyle = new GUIStyle(TextureButtonStyle);
                AlignTimeRightStyle.normal.background = Icons.AlignTimeRightOff;
                AlignTimeRightStyle.active.background = Icons.AlignTimeRightOn;

                AlignTimeCenterStyle = new GUIStyle(TextureButtonStyle);
                AlignTimeCenterStyle.normal.background = Icons.AlignTimeCenterOff;
                AlignTimeCenterStyle.active.background = Icons.AlignTimeCenterOn;

                AlignTimeDistributeStyle = new GUIStyle(TextureButtonStyle);
                AlignTimeDistributeStyle.normal.background = Icons.AlignTimeDistributeOff;
                AlignTimeDistributeStyle.active.background = Icons.AlignTimeDistributeOn;

                AlignTimeMirrorStyle = new GUIStyle(TextureButtonStyle);
                AlignTimeMirrorStyle.normal.background = Icons.AlignTimeMirrorOff;
                AlignTimeMirrorStyle.active.background = Icons.AlignTimeMirrorOn;


                AlignValueBottomStyle = new GUIStyle(TextureButtonStyle);
                AlignValueBottomStyle.normal.background = Icons.AlignValueBottomOff;
                AlignValueBottomStyle.active.background = Icons.AlignValueBottomOn;

                AlignValueTopStyle = new GUIStyle(TextureButtonStyle);
                AlignValueTopStyle.normal.background = Icons.AlignValueTopOff;
                AlignValueTopStyle.active.background = Icons.AlignValueTopOn;

                AlignValueCenterStyle = new GUIStyle(TextureButtonStyle);
                AlignValueCenterStyle.normal.background = Icons.AlignValueCenterOff;
                AlignValueCenterStyle.active.background = Icons.AlignValueCenterOn;

                AlignValueDistributeStyle = new GUIStyle(TextureButtonStyle);
                AlignValueDistributeStyle.normal.background = Icons.AlignValueDistributeOff;
                AlignValueDistributeStyle.active.background = Icons.AlignValueDistributeOn;

                AlignValueMirrorStyle = new GUIStyle(TextureButtonStyle);
                AlignValueMirrorStyle.normal.background = Icons.AlignValueMirrorOff;
                AlignValueMirrorStyle.active.background = Icons.AlignValueMirrorOn;


                MusicalTimingLabel = new GUIContent();
                MusicalTimingLabel.tooltip = "Musical Timing BPM";

                FitViewLabel = new GUIContent();
                FitViewLabel.tooltip = "Fit View (F) - Auto (Control + F)";

                FitViewStyle = new GUIStyle(TextureButtonStyle);
                FitViewStyle.normal.background = Icons.FitViewOff;
                FitViewStyle.active.background = Icons.FitViewOn;

                FitViewAutoStyle = new GUIStyle(TextureButtonStyle);
                FitViewAutoStyle.normal.background = Icons.FitViewAuto;
                FitViewAutoStyle.active.background = Icons.FitViewOn;

                AlignToolsLabel = new GUIContent();
                AlignToolsLabel.tooltip = "Alignment Tools (T)";

                AlignToolsOnStyle = new GUIStyle(TextureButtonStyle);
                AlignToolsOnStyle.normal.background = Icons.AlignToolsOn;
                AlignToolsOnStyle.active.background = Icons.AlignToolsOff;

                AlignToolsOffStyle = new GUIStyle(TextureButtonStyle);
                AlignToolsOffStyle.normal.background = AlignToolsOnStyle.active.background;
                AlignToolsOffStyle.active.background = AlignToolsOnStyle.normal.background;

                GraphViewLabel = new GUIContent();
                GraphViewLabel.tooltip = "Graph View (G)";

                GraphViewOnStyle = new GUIStyle(TextureButtonStyle);
                GraphViewOnStyle.normal.background = Icons.GraphViewOn;
                GraphViewOnStyle.active.background = Icons.GraphViewOff;

                GraphViewOffStyle = new GUIStyle(TextureButtonStyle);
                GraphViewOffStyle.normal.background = GraphViewOnStyle.active.background;
                GraphViewOffStyle.active.background = GraphViewOnStyle.normal.background;

                GraphLockLabel = new GUIContent();
                GraphLockLabel.tooltip = "Graph Lock (L)";

                GraphSoloLabel = new GUIContent();
                GraphSoloLabel.tooltip = "Graph Lock Solo Selected Channels";

                GraphLockOnStyle = new GUIStyle(TextureButtonStyle);
                GraphLockOnStyle.normal.background = Icons.LockBigOn;
                GraphLockOnStyle.active.background = Icons.LockBigOff;

                GraphLockOffStyle = new GUIStyle(TextureButtonStyle);
                GraphLockOffStyle.normal.background = GraphLockOnStyle.active.background;
                GraphLockOffStyle.active.background = GraphLockOnStyle.normal.background;


                AlignTimeLeftLabel = new GUIContent();
                AlignTimeLeftLabel.tooltip = "Align Time Left";

                AlignTimeRightLabel = new GUIContent();
                AlignTimeRightLabel.tooltip = "Align Time Right";

                AlignTimeCenterLabel = new GUIContent();
                AlignTimeCenterLabel.tooltip = "Align Time Center";

                AlignTimeDistributeLabel = new GUIContent();
                AlignTimeDistributeLabel.tooltip = "Align Time Distribute";

                AlignTimeMirrorLabel = new GUIContent();
                AlignTimeMirrorLabel.tooltip = "Align Time Mirror";

                AlignValueBottomLabel = new GUIContent();
                AlignValueBottomLabel.tooltip = "Align Value Bottom";

                AlignValueTopLabel = new GUIContent();
                AlignValueTopLabel.tooltip = "Align Value Top";

                AlignValueCenterLabel = new GUIContent();
                AlignValueCenterLabel.tooltip = "Align Value Center";

                AlignValueDistributeLabel = new GUIContent();
                AlignValueDistributeLabel.tooltip = "Align Value Distribute";

                AlignValueMirrorLabel = new GUIContent();
                AlignValueMirrorLabel.tooltip = "Align Value Mirror";


                AutoKeyframingLabel = new GUIContent();
                AutoKeyframingLabel.tooltip = "Auto Keyframing";

                AutoKeyframingOnStyle = new GUIStyle(TextureButtonStyle);
                AutoKeyframingOnStyle.normal.background = Icons.AutoKeyframingOn;
                AutoKeyframingOnStyle.active.background = Icons.AutoKeyframingOff;

                AutoKeyframingOffStyle = new GUIStyle(TextureButtonStyle);
                AutoKeyframingOffStyle.normal.background = AutoKeyframingOnStyle.active.background;
                AutoKeyframingOffStyle.active.background = AutoKeyframingOnStyle.normal.background;

                WorkAreaLabel = new GUIContent();
                WorkAreaLabel.tooltip = "Work Area (W)";

                WorkAreaOnStyle = new GUIStyle(TextureButtonStyle);
                WorkAreaOnStyle.normal.background = Icons.WorkAreaOn;
                WorkAreaOnStyle.active.background = Icons.WorkAreaOff;

                WorkAreaOffStyle = new GUIStyle(TextureButtonStyle);
                WorkAreaOffStyle.normal.background = WorkAreaOnStyle.active.background;
                WorkAreaOffStyle.active.background = WorkAreaOnStyle.normal.background;

                WorkAreaLockedStyle = new GUIStyle(TextureButtonStyle);
                WorkAreaLockedStyle.normal.background = Icons.WorkAreaLocked;
                WorkAreaLockedStyle.active.background = Icons.WorkAreaOn;


                LoopLabel = new GUIContent();
                LoopLabel.tooltip = "Loop Work Area (L)";

                ClipLoopLabel = new GUIContent();
                ClipLoopLabel.tooltip = "Loop";

                LoopOnRedStyle = new GUIStyle(TextureButtonStyle);
                LoopOnRedStyle.normal.background = Icons.WorkAreaLoopOnRed;
                LoopOnRedStyle.active.background = Icons.WorkAreaLoopOff;

                LoopOnWhiteStyle = new GUIStyle(TextureButtonStyle);
                LoopOnWhiteStyle.normal.background = Icons.WorkAreaLoopOnWhite;
                LoopOnWhiteStyle.active.background = Icons.WorkAreaLoopOff;

                LoopOffStyle = new GUIStyle(TextureButtonStyle);
                LoopOffStyle.normal.background = LoopOnRedStyle.active.background;
                LoopOffStyle.active.background = LoopOnRedStyle.normal.background;

                FollowPlayheadLabel = new GUIContent();
                FollowPlayheadLabel.tooltip = "Follow Playhead";

                FollowPlayheadOnStyle = new GUIStyle(TextureButtonStyle);
                FollowPlayheadOnStyle.normal.background = Icons.FollowPlayheadOn;
                FollowPlayheadOnStyle.active.background = Icons.FollowPlayheadOff;

                FollowPlayheadOffStyle = new GUIStyle(TextureButtonStyle);
                FollowPlayheadOffStyle.normal.background = Icons.FollowPlayheadOff;
                FollowPlayheadOffStyle.active.background = Icons.FollowPlayheadOn;

                TimeScopeLabel = new GUIContent();
                TimeScopeLabel.tooltip = "Local Time Scope (S)";

                TimeScopeOnStyle = new GUIStyle(TextureButtonStyle);
                TimeScopeOnStyle.normal.background = Icons.TimeScopeOn;
                TimeScopeOnStyle.active.background = Icons.TimeScopeOff;

                TimeScopeOffStyle = new GUIStyle(TextureButtonStyle);
                TimeScopeOffStyle.normal.background = Icons.TimeScopeOff;
                TimeScopeOffStyle.active.background = Icons.TimeScopeOn;

                TimeScopeLocalizeLabel = new GUIContent();
                TimeScopeLocalizeLabel.tooltip = "Localize Time";

                TimeScopeLocalizeOnStyle = new GUIStyle(TextureButtonStyle);
                TimeScopeLocalizeOnStyle.normal.background = Icons.TimeScopeLocalizeOn;
                TimeScopeLocalizeOnStyle.active.background = Icons.TimeScopeLocalizeOff;

                TimeScopeLocalizeOffStyle = new GUIStyle(TextureButtonStyle);
                TimeScopeLocalizeOffStyle.normal.background = Icons.TimeScopeLocalizeOff;
                TimeScopeLocalizeOffStyle.active.background = Icons.TimeScopeLocalizeOn;

                DrawCurveLabel = new GUIContent();
                DrawCurveLabel.tooltip = "Draw Curve";

                TimecodeStyle = new GUIStyle(GUI.skin.label);
                TimecodeStyle.fontSize = 16;
                TimecodeStyle.fontStyle = FontStyle.Bold;
                TimecodeStyle.normal.textColor = AxonColor.RedDark;
                TimecodeStyle.border = new RectOffset(1, 1, 1, 1);
                TimecodeStyle.normal.background = Icons.EditFieldLight;
                TimecodeStyle.active.background = TimecodeStyle.normal.background;
                TimecodeStyle.alignment = TextAnchor.MiddleLeft;

                TimeFramesStyle = new GUIStyle(GUI.skin.label);
                TimeFramesStyle.fontSize = 10;
                TimeFramesStyle.fontStyle = FontStyle.Normal;
                TimeFramesStyle.border = new RectOffset(1, 1, 1, 1);
                TimeFramesStyle.normal.textColor = AxonColor.BoldText;
                TimeFramesStyle.normal.background = null;
                TimeFramesStyle.active.background = TimeFramesStyle.normal.background;
                TimeFramesStyle.alignment = TextAnchor.MiddleLeft;

                TimeFramesSmallStyle = new GUIStyle(TimeFramesStyle);
                TimeFramesSmallStyle.alignment = TextAnchor.MiddleRight;
                TimeFramesSmallStyle.normal.textColor = AxonColor.LightText;
                TimeFramesSmallStyle.normal.background = null;
                TimeFramesSmallStyle.fontSize = 8;

                ArrangementTimeStyle = new GUIStyle(TimecodeStyle);
                ArrangementTimeStyle.fontStyle = FontStyle.Bold;
                ArrangementTimeStyle.fontSize = 11;
                ArrangementTimeStyle.normal.textColor = AxonColor.BoldText;
                ArrangementTimeStyle.normal.background = null;

                TimeMeasureStyle = new GUIStyle(GUI.skin.label);
                TimeMeasureStyle.fontSize = 15;
                TimeMeasureStyle.fontStyle = FontStyle.Bold;
                TimeMeasureStyle.normal.textColor = AxonColor.BlackText;
                TimeMeasureStyle.border = new RectOffset(1, 1, 1, 1);
                TimeMeasureStyle.normal.background = Icons.EditFieldLight;
                TimeMeasureStyle.active.background = TimecodeStyle.normal.background;
                TimeMeasureStyle.alignment = TextAnchor.MiddleCenter;

                PopupStyle = new GUIStyle(GUI.skin.label);
                PopupStyle.fontSize = 14;
                PopupStyle.fontStyle = FontStyle.Bold;
                PopupStyle.fixedHeight = 24;
                PopupStyle.border = new RectOffset(1, 1, 1, 1);
                PopupStyle.normal.background = Icons.Popup;
                PopupStyle.active.background = TimecodeStyle.normal.background;
                PopupStyle.alignment = TextAnchor.MiddleCenter;

                PlayerFirstLabel = new GUIContent();
                PlayerFirstLabel.tooltip = "First Frame (Home)";

                PlayerFirstStyle = new GUIStyle(TextureButtonStyle);
                PlayerFirstStyle.normal.background = Icons.PlayerFirstOff;
                PlayerFirstStyle.active.background = Icons.PlayerFirstOn;

                PlayerFirstOnStyle = new GUIStyle(TextureButtonStyle);
                PlayerFirstOnStyle.normal.background = PlayerFirstStyle.active.background;


                PlayerPrevLabel = new GUIContent();
                PlayerPrevLabel.tooltip = "Previous Frame (Page Up)";

                PlayerPrevStyle = new GUIStyle(TextureButtonStyle);
                PlayerPrevStyle.normal.background = Icons.PlayerPrevOff;
                PlayerPrevStyle.active.background = Icons.PlayerPrevOn;

                PlayerPrevOnStyle = new GUIStyle(TextureButtonStyle);
                PlayerPrevOnStyle.normal.background = PlayerPrevStyle.active.background;


                PlayerPlayReverseLabel = new GUIContent();
                PlayerPlayReverseLabel.tooltip = "Play Reverse";

                PlayerPlayLabel = new GUIContent();
                PlayerPlayLabel.tooltip = "Play (Spacebar)";

                PlayerPlayReverseStyle = new GUIStyle(TextureButtonStyle);
                PlayerPlayReverseStyle.normal.background = Icons.PlayerPlayReverseOff;
                PlayerPlayReverseStyle.active.background = Icons.PlayerPlayReverseOn;

                PlayerPlayReverseOnStyle = new GUIStyle(TextureButtonStyle);
                PlayerPlayReverseOnStyle.normal.background = PlayerPlayReverseStyle.active.background;

                PlayerPlayStyle = new GUIStyle(TextureButtonStyle);
                PlayerPlayStyle.normal.background = Icons.PlayerPlayOff;
                PlayerPlayStyle.active.background = Icons.PlayerPlayOn;

                PlayerPlayOnStyle = new GUIStyle(TextureButtonStyle);
                PlayerPlayOnStyle.normal.background = PlayerPlayStyle.active.background;

                PlayerPlayContinuousStyle = new GUIStyle(TextureButtonStyle);
                PlayerPlayContinuousStyle.normal.background = Icons.PlayerPlayContinuous;

                PlayerNextLabel = new GUIContent();
                PlayerNextLabel.tooltip = "Next Frame (Page Down)";

                PlayerNextStyle = new GUIStyle(TextureButtonStyle);
                PlayerNextStyle.normal.background = Icons.PlayerNextOff;
                PlayerNextStyle.active.background = Icons.PlayerNextOn;

                PlayerNextOnStyle = new GUIStyle(TextureButtonStyle);
                PlayerNextOnStyle.normal.background = PlayerNextStyle.active.background;

                PlayerLastLabel = new GUIContent();
                PlayerLastLabel.tooltip = "Last Frame (End)";

                PlayerLastStyle = new GUIStyle(TextureButtonStyle);
                PlayerLastStyle.normal.background = Icons.PlayerLastOff;
                PlayerLastStyle.active.background = Icons.PlayerLastOn;


                MarkerPrevLabel = new GUIContent();
                MarkerPrevLabel.tooltip = "Previous Marker (Alt + PageUp)";

                MarkerPrevStyle = new GUIStyle(TextureButtonStyle);
                MarkerPrevStyle.normal.background = Icons.MarkerPrevOff;
                MarkerPrevStyle.active.background = Icons.MarkerPrevOn;

                MarkerPrevOnStyle = new GUIStyle(TextureButtonStyle);
                MarkerPrevOnStyle.normal.background = MarkerPrevStyle.active.background;

                MarkerNextLabel = new GUIContent();
                MarkerNextLabel.tooltip = "Next Marker (Alt + PageDown)";

                MarkerNextStyle = new GUIStyle(TextureButtonStyle);
                MarkerNextStyle.normal.background = Icons.MarkerNextOff;
                MarkerNextStyle.active.background = Icons.MarkerNextOn;

                MarkerNextOnStyle = new GUIStyle(TextureButtonStyle);
                MarkerNextOnStyle.normal.background = MarkerNextStyle.active.background;


                SettingsLabel = new GUIContent();
                SettingsLabel.tooltip = "Show Settings";

                SettingsStyle = new GUIStyle(TextureButtonStyle);
                SettingsStyle.normal.background = Icons.SettingsOff;
                SettingsStyle.active.background = Icons.SettingsOn;

                KeyframeToolsOffStyle = new GUIStyle(TextureButtonStyle);
                KeyframeToolsOffStyle.normal.background = Icons.KeyframeToolsOff;
                KeyframeToolsOffStyle.active.background = Icons.KeyframeToolsOn;

                KeyframeToolsOnStyle = new GUIStyle(TextureButtonStyle);
                KeyframeToolsOnStyle.normal.background = Icons.KeyframeToolsOn;
                KeyframeToolsOnStyle.active.background = Icons.KeyframeToolsOff;

                BoundingBoxStyle = new GUIStyle(TextureButtonStyle);
                BoundingBoxStyle.normal.background = Icons.BoundingBox;
                BoundingBoxStyle.border = new RectOffset(3, 3, 3, 3);

                EditFieldStyle = new GUIStyle(GUI.skin.textField);
                EditFieldStyle.margin.top = 1;

                PopupFieldStyle = new GUIStyle(EditorStyles.toolbarDropDown);
                PopupFieldStyle.margin.top = 0;

                HorizontalStyle = new GUIStyle();
                HorizontalStyle.margin.top = 32;

            }
        }
    }

}//AxonGenesis

#endif