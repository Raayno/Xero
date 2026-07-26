// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AxonGenesis
{
    /// <summary>
    /// Stores icon asset references used by the Timeflow editor UI.
    /// </summary>
    public class EditorIcons : ScriptableObject
    {
        #region STATIC

        private static readonly string[] Subfolders = { "Components", "GUI" };

        private static string _ComponentIconsPath = null;
        private static string _GUIIconsPath = null;

        public static string ComponentIconsPath {
            get {
                if (string.IsNullOrEmpty(_ComponentIconsPath)) {
                    _ComponentIconsPath = AssetDatabase.GUIDToAssetPath("79156c0d1c0c6104c926a39a2e72063c");
                }
                //Debug.Log(_ComponentIconsPath);
                return _ComponentIconsPath;
            }
        }

        public static string GUIIconsPath {
            get {
                if (string.IsNullOrEmpty(_GUIIconsPath)) {
                    _GUIIconsPath = AssetDatabase.GUIDToAssetPath("bda11e3705c558a49b1b9532df7ab829");
                }
                //Debug.Log(_GUIIconsPath);
                return _GUIIconsPath;
            }
        }


        private static AssetBundle _EditorAssetBundle;

        public static AssetBundle EditorAssetBundle {
            get {
                if (_EditorAssetBundle == null) {
                    MethodInfo method = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", BindingFlags.NonPublic | BindingFlags.Static);

                    _EditorAssetBundle = (AssetBundle)method.Invoke(null, new object[] { });
                }
                return _EditorAssetBundle;
            }
        }

        /// <summary>
        /// Loads a Texture2D image. Assets should be set to truecolor GUI images.
        /// </summary>
        /// <param name="name">The name of the file including the extension. And it must be in the Path
        ///     defined above</param>
        public static Texture2D GetImage(string name)
        {
            Texture2D img = GetImageAtPath(Path.Combine(GUIIconsPath, name));
            if (img == null) {
                img = GetImageAtPath(Path.Combine(ComponentIconsPath, name));
            }
            return img;
        }

        public static Texture2D GetImageAtPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
        }

        public static Texture2D GetEditorImage(string name)
        {
            return EditorAssetBundle.LoadAsset<Texture2D>(name);
        }

        #endregion

        #region VARS

        // Behavior Icons
        public Texture2D
            AdvancedPresets,
            AlignChildren,
            AnimationClips,
            AnimationSequencer,
            AnimatorInfo,
            AudioReactive,
            AudioSample,
            AudioSpectrum,
            AudioTrack,
            AutoBank,
            AutoRotate,
            Blend,
            BoolField,
            ColorField,
            Comment,
            ComponentField,
            Distance,
            Event,
            Flyby,
            Follow,
            FPSCounter,
            FloatField,
            GameObjectField,
            Graph,
            Keyframer,
            LookAt,
            LookAtTarget,
            LoopTimeOffset,
            MidiCloner,
            MidiFile,
            MidiReceiver,
            MidiTween,
            MotionPath,
            Noise,
            ParticleSystemUpdate,
            SplinePathProvider,
            PlaceOnPath,
            PlaceOnSurface,
            PropertyLink,
            RectField,
            RenderQueue,
            RenderToDisk,
            Rotator,
            SpineAnimator,
            StringField,
            TimeDisplay,
            Timeflow,
            TimeflowBehavior,
            TimeflowController,
            TimeflowObject,
            TrailRendererUpdate,
            PhysicsUpdate,
            RigUpdate,
            Tween,
            Vector2Field,
            Vector3Field,
            Vector4Field,
            VideoPlayerUpdate;


        // Using custom icons
        public Texture2D
            Add,
            AlignTimeCenterOff,
            AlignTimeCenterOn,
            AlignTimeDistributeOff,
            AlignTimeDistributeOn,
            AlignTimeLeftOff,
            AlignTimeLeftOn,
            AlignTimeMirrorOff,
            AlignTimeMirrorOn,
            AlignTimeRightOff,
            AlignTimeRightOn,
            AlignToolsOff,
            AlignToolsOn,
            AlignValueBottomOff,
            AlignValueBottomOn,
            AlignValueCenterOff,
            AlignValueCenterOn,
            AlignValueDistributeOff,
            AlignValueDistributeOn,
            AlignValueMirrorOff,
            AlignValueMirrorOn,
            AlignValueTopOff,
            AlignValueTopOn,
            AttributesCombined,
            AttributesSeparated,
            AudioOff,
            AudioOn,
            AudioWaveformOff,
            AudioWaveformOn,
            AutoKeyframingOff,
            AutoKeyframingOn,
            AxonGenesisLogo,
            AxonGenesisLogoOn,
            BehaviorDisabled,
            BehaviorOff,
            BehaviorOffFaded,
            BehaviorOn,
            BezierBrokenHandle,
            BezierUnifiedHandle,
            BezierEqualHandle,
            Border,
            BorderDark,
            BorderLight,
            BorderSelected,
            BoundingBox,
            Button,
            Cancel,
            ChannelAOff,
            ChannelAOn,
            ChannelLinkOff,
            ChannelLinkOn,
            ChannelLinkRemove,
            ChannelLinkSelect,
            ChannelLinkTarget,
            ChannelLoopAutoOff,
            ChannelLoopAutoOn,
            ChannelLoopFree,
            ChannelLoopHalf,
            ChannelLoopInOff,
            ChannelLoopInOn,
            ChannelLoopMatch,
            ChannelLoopOff,
            ChannelLoopOn,
            ChannelLoopOutOff,
            ChannelLoopOutOn,
            ChannelLoopPingPong,
            ChannelXOff,
            ChannelXOn,
            ChannelYOff,
            ChannelYOn,
            ChannelZOff,
            ChannelZOn,
            ColumnExpandOff,
            ColumnExpandOn,
            ColumnExpandRightOff,
            ColumnExpandRightOn,
            DarkBox,
            Deactivate,
            DebugOff,
            DebugOn,
            Delete,
            DeleteOff,
            DeleteOn,
            DirectorSyncOff,
            DirectorSyncOn,
            Discord,
            DisplayChannelOff,
            DisplayChannelOn,
            DisplayChannelSolo,
            DisplayChannelSoloOff,
            Docs,
            DragHandle,
            DrawCurveOff,
            DrawCurveOn,
            EditFieldLight,
            EditOff,
            EditOn,
            EditorAndRuntime,
            EditorOnly,
            EndTime,
            Error,
            EventDisabled,
            EventDisabledSelected,
            EventEnabled,
            EventSelected,
            FitViewAuto,
            FitViewOff,
            FitViewOn,
            Foldout,
            FoldoutDown,
            FoldoutDownPlus,
            FoldoutNone,
            FoldoutUp,
            FoldoutUpPlus,
            FollowPlayheadOff,
            FollowPlayheadOn,
            GraphLine,
            GraphLocked,
            GraphOff,
            GraphOn,
            GraphViewOff,
            GraphViewOn,
            GridOff,
            GridOn,
            GridSnapOff,
            GridSnapOn,
            GridSnapVertOff,
            GridSnapVertOn,
            Grouped,
            HierarchyAdd,
            HierarchyAddOff,
            HierarchyBox,
            HierarchyTools,
            Info,
            InterpAutoOff,
            InterpAutoOn,
            InterpChanBezier,
            InterpChanLinear,
            InterpChanNone,
            InterpChanQuad,
            InterpFlatLeftOff,
            InterpFlatLeftOn,
            InterpFlatOff,
            InterpFlatOn,
            InterpFlatRightOff,
            InterpFlatRightOn,
            InterpHoldOff,
            InterpHoldOn,
            InterpHoldPress,
            InterpLinearOff,
            InterpLinearOn,
            InterpLinearPress,
            InterpVerticalOff,
            InterpVerticalOn,
            IsMinimizedOff,
            IsMinimizedOn,
            Keyframe,
            KeyframeHold,
            KeyframeHoldSelected,
            KeyframeObject,
            KeyframeObjectSelected,
            KeyframeSelected,
            KeyframeSelectedCopy,
            KeyframeToolsOff,
            KeyframeToolsOn,
            KeyframeValuesOff,
            KeyframeValuesOn,
            KeySelectModeAll,
            KeySelectModeKeys,
            KeySelectModeTracks,
            LayoutGrid,
            LayoutList,
            LockBigOff,
            LockBigOn,
            LockHalf,
            LockOff,
            LockOffFaded,
            LockOn,
            LockUnlockOn,
            LoopHandle,
            LoopHandleVertical,
            Marker,
            MarkerNextOff,
            MarkerNextOn,
            MarkerPrevOff,
            MarkerPrevOn,
            MarkerSelected,
            MarkersOff,
            MarkersOn,
            Marquee,
            MeshInstance,
            MicroAdjustOff,
            MicroAdjustOn,
            MidiNote,
            MinMaxFieldToggleOn,
            MinMaxFieldToggleOff,
            More,
            MoveDown,
            MoveUp,
            MusicalTimingOff,
            MusicalTimingOn,
            NameReset,
            NextKey,
            NextKeyNone,
            OutsideWorkArea,
            PlayerFirstOff,
            PlayerFirstOn,
            PlayerLastOff,
            PlayerLastOn,
            PlayerNextOff,
            PlayerNextOn,
            PlayerPlayContinuous,
            PlayerPlayOff,
            PlayerPlayOn,
            PlayerPlayReverseOff,
            PlayerPlayReverseOn,
            PlayerPrevOff,
            PlayerPrevOn,
            Playhead,
            Popup,
            PrefabIcon,
            PresetModeCombine,
            PresetModeInstantiate,
            PresetModeInstantiateAsParent,
            PresetModeReplace,
            PrevKey,
            PrevKeyNone,
            PropertyCopy,
            PropertyPaste,
            Readme,
            RefreshOff,
            RefreshOn,
            Remove,
            RenameObjectsOff,
            RenameObjectsOn,
            Reset,
            ResetRangeOff,
            ResetRangeOn,
            Review,
            Row,
            RowHover,
            RuntimeOnly,
            SaveOff,
            SaveOn,
            SearchOff,
            SearchOn,
            SearchTypeSortingAlphabetical,
            SearchTypeSortingPrioritized,
            SearchTypeSortingCount,
            Select,
            Selected,
            SelectedDefocus,
            SettingsOff,
            SettingsOn,
            Sub,
            SubDeselected,
            SubSelected,
            TimeRange,
            TimeRangeEmpty,
            TimeRangeIn,
            TimeRangeOut,
            TimeScopeLocalizeOff,
            TimeScopeLocalizeOn,
            TimeScopeOff,
            TimeScopeOn,
            ToggleKeyOff,
            ToggleKeyOn,
            ToolbarBox,
            ToolbarDivider,
            ToolEditKeysOnly,
            ToolEditKeysOnlyCursor,
            ToolEditTangents,
            ToolEditTangentsCursor,
            ToolSelect,
            Track,
            TrackColorsOff,
            TrackColorsOn,
            TrackDisabled,
            TrackDragTimeOffsetOff,
            TrackDragTimeOffsetOn,
            TrackEmpty,
            TrackLoop,
            TrackMixLinear,
            TrackMixEase,
            TrackMixLinearInverse,
            TrackMixEaseInverse,
            TrackMixFaded,
            TrackMixFadedInverse,
            TrackMixHold,
            TrackOff,
            TrackSelected,
            TrackSelectedCopy,
            TrackSelectedPattern,
            Ungrouped,
            UniformValueOff,
            UniformValueOn,
            UnifyTangentLengths,
            UnifyTangentsOff,
            UnifyTangentsOn,
            VisibilityHalf,
            VisibilityOff,
            VisibilityOffFaded,
            VisibilityOn,
            Warning,
            WorkAreaIn,
            WorkAreaLocked,
            WorkAreaLoopOff,
            WorkAreaLoopOnRed,
            WorkAreaLoopOnWhite,
            WorkAreaOff,
            WorkAreaOn,
            WorkAreaOut;

        // Using built-in Unity icons
        public Texture2D
            Presets,
            Settings,
            Local,
            Global,
            Linked,
            Unlinked,
            TabNext,
            TabPrev;

        #endregion

        #region SETUP

        private void OnEnable()
        {
            Setup();
            //RebuildList();
        }

        private void RebuildList()
        {
            string path = $"E:/Timeflow/DEV/Assets/AxonGenesis/Timeflow/Core/Editor/Icons/";
            string[] files = Directory.GetFiles(path);
            string[] filteredFiles = files.Where(file => !file.EndsWith(".meta")).ToArray();

            string log = "public Texture2D\n";
            foreach (string file in filteredFiles) {
                string basename = Path.GetFileNameWithoutExtension(file);
                log += $"{basename},\n";
            }

            log += "\n";
            log += "\n";
            foreach (string file in filteredFiles) {
                string filename = Path.GetFileName(file);
                string basename = Path.GetFileNameWithoutExtension(file);
                log += $"{basename} = GetImage(\"{filename}\");\n";
            }

            Debug.Log(log);//--KEEP

            string infoPath = $"E:/Timeflow/DEV/Assets/IconsInfo.txt";
            File.WriteAllText(infoPath, log);

        }

        public void Setup()
        {
            Presets ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_Preset.Context.png");
            Settings ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_Settings.png");
            SettingsOff ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "Settings.png");
            SettingsOn ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_TerrainInspector.TerrainToolSettings On.png");

            Local ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_ToolHandleLocal.png");
            Global ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_ToolHandleGlobal.png");
            Linked ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_Linked.png");
            Unlinked ??= GetEditorImage(UnityEditor.Experimental.EditorResources.iconsPath + "d_Unlinked.png");

            TabNext ??= GetImage("TabNext.png");
            TabPrev ??= GetImage("TabPrev.png");

            Add ??= GetImage("Add.png");
            AdvancedPresets ??= GetImage("AdvancedPresets.png");
            AlignTimeCenterOff ??= GetImage("AlignTimeCenterOff.png");
            AlignTimeCenterOn ??= GetImage("AlignTimeCenterOn.png");
            AlignTimeDistributeOff ??= GetImage("AlignTimeDistributeOff.png");
            AlignTimeDistributeOn ??= GetImage("AlignTimeDistributeOn.png");
            AlignTimeMirrorOff ??= GetImage("AlignTimeMirrorOff.png");
            AlignTimeMirrorOn ??= GetImage("AlignTimeMirrorOn.png");
            AlignTimeLeftOff ??= GetImage("AlignTimeLeftOff.png");
            AlignTimeLeftOn ??= GetImage("AlignTimeLeftOn.png");
            AlignTimeRightOff ??= GetImage("AlignTimeRightOff.png");
            AlignTimeRightOn ??= GetImage("AlignTimeRightOn.png");
            AlignToolsOff ??= GetImage("AlignToolsOff.png");
            AlignToolsOn ??= GetImage("AlignToolsOn.png");
            AlignValueBottomOff ??= GetImage("AlignValueBottomOff.png");
            AlignValueBottomOn ??= GetImage("AlignValueBottomOn.png");
            AlignValueCenterOff ??= GetImage("AlignValueCenterOff.png");
            AlignValueCenterOn ??= GetImage("AlignValueCenterOn.png");
            AlignValueDistributeOff ??= GetImage("AlignValueDistributeOff.png");
            AlignValueDistributeOn ??= GetImage("AlignValueDistributeOn.png");
            AlignValueMirrorOff ??= GetImage("AlignValueMirrorOff.png");
            AlignValueMirrorOn ??= GetImage("AlignValueMirrorOn.png");
            AlignValueTopOff ??= GetImage("AlignValueTopOff.png");
            AlignValueTopOn ??= GetImage("AlignValueTopOn.png");
            AttributesCombined ??= GetImage("AttributesCombined.png");
            AttributesSeparated ??= GetImage("AttributesSeparated.png");
            AudioOff ??= GetImage("AudioOff.png");
            AudioOn ??= GetImage("AudioOn.png");
            AudioWaveformOff ??= GetImage("AudioWaveformOff.png");
            AudioWaveformOn ??= GetImage("AudioWaveformOn.png");
            AutoKeyframingOff ??= GetImage("AutoKeyframingOff.png");
            AutoKeyframingOn ??= GetImage("AutoKeyframingOn.png");
            AxonGenesisLogo ??= GetImage("AxonGenesisLogo.png");
            AxonGenesisLogoOn ??= GetImage("AxonGenesisLogoOn.png");
            BehaviorDisabled ??= GetImage("BehaviorDisabled.png");
            BehaviorOff ??= GetImage("BehaviorOff.png");
            BehaviorOffFaded ??= GetImage("BehaviorOffFaded.png");
            BehaviorOn ??= GetImage("BehaviorOn.png");
            BezierBrokenHandle ??= GetImage("BezierBrokenHandle.png");
            BezierUnifiedHandle ??= GetImage("BezierUnifiedHandle.png");
            BezierEqualHandle ??= GetImage("BezierEqualHandle.png");
            Border ??= GetImage("Border.png");
            BorderDark ??= GetImage("BorderDark.png");
            BorderLight ??= GetImage("BorderLight.png");
            BorderSelected ??= GetImage("BorderSelected.png");
            BoundingBox ??= GetImage("BoundingBox.png");
            Button ??= GetImage("Button.png");
            Cancel ??= GetImage("Cancel.png");
            ChannelAOff ??= GetImage("ChannelAOff.png");
            ChannelAOn ??= GetImage("ChannelAOn.png");
            ChannelLinkOff ??= GetImage("ChannelLinkOff.png");
            ChannelLinkOn ??= GetImage("ChannelLinkOn.png");
            ChannelLinkRemove ??= GetImage("ChannelLinkRemove.png");
            ChannelLinkSelect ??= GetImage("ChannelLinkSelect.png");
            ChannelLinkTarget ??= GetImage("ChannelLinkTarget.png");
            ChannelLoopAutoOff ??= GetImage("ChannelLoopAutoOff.png");
            ChannelLoopAutoOn ??= GetImage("ChannelLoopAutoOn.png");
            ChannelLoopFree ??= GetImage("ChannelLoopFree.png");
            ChannelLoopHalf ??= GetImage("ChannelLoopHalf.png");
            ChannelLoopInOff ??= GetImage("ChannelLoopInOff.png");
            ChannelLoopInOn ??= GetImage("ChannelLoopInOn.png");
            ChannelLoopMatch ??= GetImage("ChannelLoopMatch.png");
            ChannelLoopOff ??= GetImage("ChannelLoopOff.png");
            ChannelLoopOn ??= GetImage("ChannelLoopOn.png");
            ChannelLoopOutOff ??= GetImage("ChannelLoopOutOff.png");
            ChannelLoopOutOn ??= GetImage("ChannelLoopOutOn.png");
            ChannelLoopPingPong ??= GetImage("ChannelLoopPingPong.png");
            ChannelXOff ??= GetImage("ChannelXOff.png");
            ChannelXOn ??= GetImage("ChannelXOn.png");
            ChannelYOff ??= GetImage("ChannelYOff.png");
            ChannelYOn ??= GetImage("ChannelYOn.png");
            ChannelZOff ??= GetImage("ChannelZOff.png");
            ChannelZOn ??= GetImage("ChannelZOn.png");
            ColumnExpandOff ??= GetImage("ColumnExpandOff.png");
            ColumnExpandOn ??= GetImage("ColumnExpandOn.png");
            ColumnExpandRightOff ??= GetImage("ColumnExpandRightOff.png");
            ColumnExpandRightOn ??= GetImage("ColumnExpandRightOn.png");
            Comment ??= GetImage("Comment.png");
            DarkBox ??= GetImage("DarkBox.png");
            Deactivate ??= GetImage("Deactivate.png");
            DebugOff ??= GetImage("DebugOff.png");
            DebugOn ??= GetImage("DebugOn.png");
            Delete ??= GetImage("Delete.png");
            DeleteOff ??= GetImage("DeleteOff.png");
            DeleteOn ??= GetImage("DeleteOn.png");
            DirectorSyncOff ??= GetImage("DirectorSyncOff.png");
            DirectorSyncOn ??= GetImage("DirectorSyncOn.png");
            Discord ??= GetImage("Discord.png");
            DisplayChannelOff ??= GetImage("DisplayChannelOff.png");
            DisplayChannelOn ??= GetImage("DisplayChannelOn.png");
            DisplayChannelSolo ??= GetImage("DisplayChannelSolo.png");
            DisplayChannelSoloOff ??= GetImage("DisplayChannelSoloOff.png");
            Docs ??= GetImage("Docs.png");
            DragHandle ??= GetImage("DragHandle.png");
            DrawCurveOff ??= GetImage("DrawCurveOff.png");
            DrawCurveOn ??= GetImage("DrawCurveOn.png");
            EditFieldLight ??= GetImage("EditFieldLight.png");
            EditOff ??= GetImage("EditOff.png");
            EditOn ??= GetImage("EditOn.png");
            EditorAndRuntime ??= GetImage("EditorAndRuntime.png");
            EditorOnly ??= GetImage("EditorOnly.png");
            EndTime ??= GetImage("EndTime.png");
            Error ??= GetImage("Error.png");
            Event ??= GetImage("Event.png");
            EventEnabled ??= GetImage("EventEnabled.png");
            EventDisabled ??= GetImage("EventDisabled.png");
            EventDisabledSelected ??= GetImage("EventDisabledSelected.png");
            EventSelected ??= GetImage("EventSelected.png");
            FitViewAuto ??= GetImage("FitViewAuto.png");
            FitViewOff ??= GetImage("FitViewOff.png");
            FitViewOn ??= GetImage("FitViewOn.png");
            Flyby ??= GetImage("Flyby.png");
            Foldout ??= GetImage("Foldout.png");
            FoldoutDown ??= GetImage("FoldoutDown.png");
            FoldoutDownPlus ??= GetImage("FoldoutDownPlus.png");
            FoldoutNone ??= GetImage("FoldoutNone.png");
            FoldoutUp ??= GetImage("FoldoutUp.png");
            FoldoutUpPlus ??= GetImage("FoldoutUpPlus.png");
            FollowPlayheadOff ??= GetImage("FollowPlayheadOff.png");
            FollowPlayheadOn ??= GetImage("FollowPlayheadOn.png");
            GraphLine ??= GetImage("GraphLine.png");
            GraphLocked ??= GetImage("GraphLocked.png");
            GraphOff ??= GetImage("GraphOff.png");
            GraphOn ??= GetImage("GraphOn.png");
            GraphViewOff ??= GetImage("GraphViewOff.png");
            GraphViewOn ??= GetImage("GraphViewOn.png");
            GridOff ??= GetImage("GridOff.png");
            GridOn ??= GetImage("GridOn.png");
            GridSnapOff ??= GetImage("GridSnapOff.png");
            GridSnapOn ??= GetImage("GridSnapOn.png");
            GridSnapVertOff ??= GetImage("GridSnapVertOff.png");
            GridSnapVertOn ??= GetImage("GridSnapVertOn.png");
            Grouped ??= GetImage("Grouped.png");
            HierarchyAdd ??= GetImage("HierarchyAdd.png");
            HierarchyAddOff ??= GetImage("HierarchyAddOff.png");
            HierarchyBox ??= GetImage("HierarchyBox.png");
            HierarchyTools ??= GetImage("HierarchyTools.png");
            Info ??= GetImage("Info.png");
            InterpAutoOff ??= GetImage("InterpAutoOff.png");
            InterpAutoOn ??= GetImage("InterpAutoOn.png");
            InterpChanBezier ??= GetImage("InterpChanBezier.png");
            InterpChanLinear ??= GetImage("InterpChanLinear.png");
            InterpChanNone ??= GetImage("InterpChanNone.png");
            InterpChanQuad ??= GetImage("InterpChanQuad.png");
            InterpFlatLeftOff ??= GetImage("InterpFlatLeftOff.png");
            InterpFlatLeftOn ??= GetImage("InterpFlatLeftOn.png");
            InterpVerticalOff ??= GetImage("InterpVerticalOff.png");
            InterpVerticalOn ??= GetImage("InterpVerticalOn.png");
            InterpFlatOff ??= GetImage("InterpFlatOff.png");
            InterpFlatOn ??= GetImage("InterpFlatOn.png");
            InterpFlatRightOff ??= GetImage("InterpFlatRightOff.png");
            InterpFlatRightOn ??= GetImage("InterpFlatRightOn.png");
            InterpHoldOff ??= GetImage("InterpHoldOff.png");
            InterpHoldOn ??= GetImage("InterpHoldOn.png");
            InterpHoldPress ??= GetImage("InterpHoldPress.png");
            InterpLinearOff ??= GetImage("InterpLinearOff.png");
            InterpLinearOn ??= GetImage("InterpLinearOn.png");
            InterpLinearPress ??= GetImage("InterpLinearPress.png");
            IsMinimizedOff ??= GetImage("IsMinimizedOff.png");
            IsMinimizedOn ??= GetImage("IsMinimizedOn.png");
            Keyframe ??= GetImage("Keyframe.png");
            KeyframeHold ??= GetImage("KeyframeHold.png");
            KeyframeHoldSelected ??= GetImage("KeyframeHoldSelected.png");
            KeyframeObject ??= GetImage("KeyframeObject.png");
            KeyframeObjectSelected ??= GetImage("KeyframeObjectSelected.png");
            Keyframer ??= GetImage("Keyframer.png");
            KeyframeSelected ??= GetImage("KeyframeSelected.png");
            KeyframeSelectedCopy ??= GetImage("KeyframeSelectedCopy.png");
            KeyframeToolsOff ??= GetImage("KeyframeToolsOff.png");
            KeyframeToolsOn ??= GetImage("KeyframeToolsOn.png");
            KeyframeValuesOff ??= GetImage("KeyframeValuesOff.png");
            KeyframeValuesOn ??= GetImage("KeyframeValuesOn.png");
            KeySelectModeAll ??= GetImage("KeySelectModeAll.png");
            KeySelectModeKeys ??= GetImage("KeySelectModeKeys.png");
            KeySelectModeTracks ??= GetImage("KeySelectModeTracks.png");
            LayoutGrid ??= GetImage("LayoutGrid.png");
            LayoutList ??= GetImage("LayoutList.png");
            PresetModeInstantiate ??= GetImage("PresetModeInstantiate.png");
            PresetModeInstantiateAsParent ??= GetImage("PresetModeInstantiateAsParent.png");
            PresetModeCombine ??= GetImage("PresetModeCombine.png");
            PresetModeReplace ??= GetImage("PresetModeReplace.png");
            LockBigOff ??= GetImage("LockBigOff.png");
            LockBigOn ??= GetImage("LockBigOn.png");
            LockHalf ??= GetImage("LockHalf.png");
            LockOff ??= GetImage("LockOff.png");
            LockOffFaded ??= GetImage("LockOffFaded.png");
            LockOn ??= GetImage("LockOn.png");
            LockUnlockOn ??= GetImage("LockUnlockOn.png");
            LookAt ??= GetImage("LookAt.png");
            LookAtTarget ??= GetImage("LookAtTarget.png");
            LoopTimeOffset ??= GetImage("LoopTimeOffset.png");
            LoopHandle ??= GetImage("LoopHandle.png");
            LoopHandleVertical ??= GetImage("LoopHandleVertical.png");
            Marker ??= GetImage("Marker.png");
            MarkerNextOff ??= GetImage("MarkerNextOff.png");
            MarkerNextOn ??= GetImage("MarkerNextOn.png");
            MarkerPrevOff ??= GetImage("MarkerPrevOff.png");
            MarkerPrevOn ??= GetImage("MarkerPrevOn.png");
            MarkerSelected ??= GetImage("MarkerSelected.png");
            MarkersOff ??= GetImage("MarkersOff.png");
            MarkersOn ??= GetImage("MarkersOn.png");
            Marquee ??= GetImage("Marquee.png");
            MeshInstance ??= GetImage("MeshInstance.png");
            MicroAdjustOff ??= GetImage("MicroAdjustOff.png");
            MicroAdjustOn ??= GetImage("MicroAdjustOn.png");
            MidiFile ??= GetImage("MidiFile.png");
            MidiNote ??= GetImage("MidiNote.png");
            MinMaxFieldToggleOn ??= GetImage("MinMaxFieldToggleOn.png");
            MinMaxFieldToggleOff ??= GetImage("MinMaxFieldToggleOff.png");
            MidiTween ??= GetImage("MidiTween.png");
            More ??= GetImage("More.png");
            MotionPath ??= GetImage("MotionPath.png");
            ParticleSystemUpdate ??= GetImage("ParticleSystemUpdate.png");
            PlaceOnPath ??= GetImage("PlaceOnPath.png");
            SplinePathProvider ??= GetImage("SplinePathProvider.png");
            Noise ??= GetImage("Noise.png");
            MoveDown ??= GetImage("MoveDown.png");
            MoveUp ??= GetImage("MoveUp.png");
            MusicalTimingOff ??= GetImage("MusicalTimingOff.png");
            MusicalTimingOn ??= GetImage("MusicalTimingOn.png");
            NameReset ??= GetImage("NameReset.png");
            NextKey ??= GetImage("NextKey.png");
            NextKeyNone ??= GetImage("NextKeyNone.png");
            OutsideWorkArea ??= GetImage("OutsideWorkArea.png");
            PlayerFirstOff ??= GetImage("PlayerFirstOff.png");
            PlayerFirstOn ??= GetImage("PlayerFirstOn.png");
            PlayerLastOff ??= GetImage("PlayerLastOff.png");
            PlayerLastOn ??= GetImage("PlayerLastOn.png");
            PlayerNextOff ??= GetImage("PlayerNextOff.png");
            PlayerNextOn ??= GetImage("PlayerNextOn.png");
            PlayerPlayContinuous ??= GetImage("PlayerPlayContinuous.png");
            PlayerPlayOff ??= GetImage("PlayerPlayOff.png");
            PlayerPlayOn ??= GetImage("PlayerPlayOn.png");
            PlayerPlayReverseOff ??= GetImage("PlayerPlayReverseOff.png");
            PlayerPlayReverseOn ??= GetImage("PlayerPlayReverseOn.png");
            PlayerPrevOff ??= GetImage("PlayerPrevOff.png");
            PlayerPrevOn ??= GetImage("PlayerPrevOn.png");
            Playhead ??= GetImage("Playhead.png");
            Popup ??= GetImage("Popup.png");
            PrefabIcon ??= GetImage("Prefab.png");
            //PrefabIcon ??= EditorGUIUtility.IconContent("PrefabOverlayAdded Icon").image as Texture2D;
            PrevKey ??= GetImage("PrevKey.png");
            PrevKeyNone ??= GetImage("PrevKeyNone.png");
            PropertyCopy ??= GetImage("PropertyCopy.png");
            PropertyPaste ??= GetImage("PropertyPaste.png");
            RefreshOff ??= GetImage("RefreshOff.png");
            RefreshOn ??= GetImage("RefreshOn.png");
            Remove ??= GetImage("Remove.png");
            RenameObjectsOn ??= GetImage("RenameObjectsOn.png");
            RenameObjectsOff ??= GetImage("RenameObjectsOff.png");
            Reset ??= GetImage("Reset.png");
            ResetRangeOff ??= GetImage("ResetRangeOff.png");
            ResetRangeOn ??= GetImage("ResetRangeOn.png");
            Review ??= GetImage("Review.png");
            Rotator ??= GetImage("Rotator.png");
            Row ??= GetImage("Row.png");
            RowHover ??= GetImage("RowHover.png");
            RuntimeOnly ??= GetImage("RuntimeOnly.png");
            SaveOff ??= GetImage("SaveOff.png");
            SaveOn ??= GetImage("SaveOn.png");
            SearchOff ??= GetImage("SearchOff.png");
            SearchOn ??= GetImage("SearchOn.png");
            SearchTypeSortingAlphabetical ??= GetImage("SearchTypeSortingAlphabetical.png");
            SearchTypeSortingCount ??= GetImage("SearchTypeSortingCount.png");
            SearchTypeSortingPrioritized ??= GetImage("SearchTypeSortingPrioritized.png");
            Select ??= GetImage("Select.png");
            Selected ??= GetImage("Selected.png");
            SelectedDefocus ??= GetImage("SelectedDefocus.png");
            SettingsOff ??= GetImage("SettingsOff.png");
            SettingsOn ??= GetImage("SettingsOn.png");
            Sub ??= GetImage("Sub.png");
            SubDeselected ??= GetImage("SubDeselected.png");
            SubSelected ??= GetImage("SubSelected.png");
            Readme ??= GetImage("Readme.png");
            TimeRange ??= GetImage("TimeRange.png");
            TimeRangeEmpty ??= GetImage("TimeRangeEmpty.png");
            TimeRangeIn ??= GetImage("TimeRangeIn.png");
            TimeRangeOut ??= GetImage("TimeRangeOut.png");
            TimeScopeOff ??= GetImage("TimeScopeOff.png");
            TimeScopeOn ??= GetImage("TimeScopeOn.png");
            TimeScopeLocalizeOff ??= GetImage("TimeScopeLocalizeOff.png");
            TimeScopeLocalizeOn ??= GetImage("TimeScopeLocalizeOn.png");
            ToggleKeyOff ??= GetImage("ToggleKeyOff.png");
            ToggleKeyOn ??= GetImage("ToggleKeyOn.png");
            ToolbarBox ??= GetImage("ToolbarBox.png");
            ToolbarDivider ??= GetImage("ToolbarDivider.png");
            ToolEditKeysOnly ??= GetImage("ToolEditKeysOnly.png");
            ToolEditKeysOnlyCursor ??= GetImage("ToolEditKeysOnlyCursor.png");
            ToolEditTangents ??= GetImage("ToolEditTangents.png");
            ToolEditTangentsCursor ??= GetImage("ToolEditTangentsCursor.png");
            ToolSelect ??= GetImage("ToolSelect.png");
            Track ??= GetImage("Track.png");
            TrackColorsOn ??= GetImage("TrackColorsOn.png");
            TrackColorsOff ??= GetImage("TrackColorsOff.png");
            TrackDisabled ??= GetImage("TrackDisabled.png");
            TrackDragTimeOffsetOff ??= GetImage("TrackDragTimeOffsetOff.png");
            TrackDragTimeOffsetOn ??= GetImage("TrackDragTimeOffsetOn.png");
            TrackEmpty ??= GetImage("TrackEmpty.png");
            TrackLoop ??= GetImage("TrackLoop.png");
            TrackMixLinear ??= GetImage("TrackMixLinear.png");
            TrackMixEase ??= GetImage("TrackMixEase.png");
            TrackMixLinearInverse ??= GetImage("TrackMixLinearInverse.png");
            TrackMixEaseInverse ??= GetImage("TrackMixEaseInverse.png");
            TrackMixFaded ??= GetImage("TrackMixFaded.png");
            TrackMixFadedInverse ??= GetImage("TrackMixFadedInverse.png");
            TrackMixHold ??= GetImage("TrackMixHold.png");
            TrackOff ??= GetImage("TrackOff.png");
            TrackSelected ??= GetImage("TrackSelected.png");
            TrackSelectedCopy ??= GetImage("TrackSelectedCopy.png");
            TrackSelectedPattern ??= GetImage("TrackSelectedPattern.png");
            Ungrouped ??= GetImage("Ungrouped.png");
            UniformValueOff ??= GetImage("UniformValueOff.png");
            UniformValueOn ??= GetImage("UniformValueOn.png");
            UnifyTangentLengths ??= GetImage("UnifyTangentLengths.png");
            UnifyTangentsOff ??= GetImage("UnifyTangentsOff.png");
            UnifyTangentsOn ??= GetImage("UnifyTangentsOn.png");
            VisibilityHalf ??= GetImage("VisibilityHalf.png");
            VisibilityOff ??= GetImage("VisibilityOff.png");
            VisibilityOffFaded ??= GetImage("VisibilityOffFaded.png");
            VisibilityOn ??= GetImage("VisibilityOn.png");
            Warning ??= GetImage("Warning.png");
            WorkAreaIn ??= GetImage("WorkAreaIn.png");
            WorkAreaLocked ??= GetImage("WorkAreaLocked.png");
            WorkAreaLoopOff ??= GetImage("WorkAreaLoopOff.png");
            WorkAreaLoopOnRed ??= GetImage("WorkAreaLoopOn.png");
            WorkAreaLoopOnWhite ??= GetImage("WorkAreaLoopOnWhite.png");
            WorkAreaOff ??= GetImage("WorkAreaOff.png");
            WorkAreaOn ??= GetImage("WorkAreaOn.png");
            WorkAreaOut ??= GetImage("WorkAreaOut.png");

        }

        #endregion
    }

}

#endif
