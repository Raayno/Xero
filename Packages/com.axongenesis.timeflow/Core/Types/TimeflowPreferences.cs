// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Stores preference values for Timeflow as an asset. This data is only used for serialization.
    /// </summary>
    [ExecuteAlways]
    public class TimeflowPreferences : ScriptableObject
    {
        #region STATIC

        public static readonly int ChannelMinHeight = 20;
        public static readonly int ChannelMaxHeight = 500;

        /// This allows roughly 2.7 hours of animation. This limit is to avoid potential issues with
        /// drawing and time calculations that may result otherwise. This value may be increased ten-fold
        /// if need. This is not exposed in the preferences editor since it is an advanced setting that
        /// in most cases should never be changed.
        public static readonly float MaxDuration = 100000f;

        private static bool _DebugEnabledTemp = true;

#if UNITY_EDITOR
        private static int preferencesNotInstalledCount = 0;
#endif

        /// <summary>
        /// The setting allows specific functions such as when drawing GUIs to disable debug logging
        /// to avoid massive log dumps due to iterative feedback, which can also crash Unity or make the UI
        /// unresponsive.
        /// </summary>
        public static bool DebugEnabled {
            get {
                return Current.EnableDebug && _DebugEnabledTemp;
            }
            set {
                _DebugEnabledTemp = value;
            }
        }


        private static TimeflowPreferences _Current;

        public static TimeflowPreferences Current {
            get {
                if (_Current == null) LoadOrCreateSettings();
                return _Current;
            }
            set {
                if (_Current != value) {
                    if (value != null) {
                        _Current = value;
                    }
                    else {
                        GetDefaults();
                    }
                    SaveSettings();
                }
            }
        }

        public static TimeflowPreferences GetDefaults()
        {
            if (_Current == null) {
                _Current = ScriptableObject.CreateInstance<TimeflowPreferences>();
            }
            _Current.KeyTolerance = 0.0001f;
            _Current.TimeToleranceMode = TimeToleranceModes.Frame;
            _Current.TimeTolerance = 0.01666667f;
            _Current.CustomTimeStep = 1f;
            _Current.TimeToleranceStrict = false;
            _Current.UseFractionalTime = true;
            _Current.EnableDebug = false;

#if UNITY_EDITOR
            _Current.ScrollTimeWithoutAlt = false;
            _Current.ScrollTimeCenterOnMouse = false;
            _Current.DrawMinorGridLines = false;
            _Current.KeyMicroAdjust = 0.1f;
            _Current.TrackAutoLock = false; // To allow easier drag offset
            _Current.ZoomToggleRange = 1f;
            _Current.AllowCommaForDecimalPoints = false;
            _Current.DefaultChannelHeight = 20;
            _Current.ChannelHeightDragSensitivity = 1f;
            _Current.ChannelHeights = null;
            _Current.UndoTimeChanges = true;
            _Current.MinTimeScale = 0.0001f;
            _Current.TrackColorMenuMode = TrackColorMenu.Modes.Palette;
            _Current.TrackColors = null;
            _Current.DefaultTrackMode = TimeflowTrack.VisibilityModes.On;
            _Current.DefaultTracksLocked = false;
            _Current.DefaultCanDragTimeOffset = true;
            _Current.TracksSelectObjects = true;
            _Current.ObjectsSelectTracks = false;
            _Current.AllowHierarchySorting = true;
            _Current.OpenOnPrecompose = true;
            _Current.CopyLockedTracksAndKeys = false;
            _Current.ShowTrackColorsInInspector = true;
            _Current.ShowLoopedKeyframes = true;
            _Current.DrawLinkedChannels = true;
            _Current.DeleteAction = DeleteActions.RemoveFromView;
            _Current.ControlDeleteAction = DeleteActions.DeleteGameObject;
            _Current.EnforceStartTime = true;
            _Current.EnforceEndTime = false;
            _Current.UnifiedTimeDisplay = true;
            _Current.CacheChannelValues = true;
            _Current.ShowComponentIcons = true;
            _Current.NewChannelAttributeMode = NewChannelAttributeModes.Auto;
            _Current.ReverseChannelOrder = true;
            _Current.AddObjectsToTopOfList = false;
            _Current.DisplayScrollbarOnLeft = true;
            _Current.DisplayScrollbarOnTop = true;
            _Current.AutoScrollToSelection = true;
            _Current.MinimizeFloatingViewToBottom = false;
            _Current.ShowPresets = true;
            _Current.ExposeAllProperties = false;
            _Current.EnableTrackShadows = true;
            _Current.TrackShadowColor = new Color(0.1f, 0.1f, 0.1f, 0.15f);
            _Current.TrackSelectedPattern = 0.25f;
            _Current.KeyframeLabelColor = new Color(1, 1f, 1f, 0.7f);
            _Current.EnableKeyframeTicks = true;
            _Current.EnableSetKeysWhilePlaying = false;
            _Current.EnableMidiFileRenaming = true;
            _Current.SceneIncrementalBackupPath = "Assets/_SceneBackup";
            _Current.PrefabSavePath = "Assets/Prefabs";
            _Current.DefaultTimeflowName = "Timeflow";
            _Current.DefaultPrecompName = "Precomp";
            _Current.AlwaysNumberNames = false;
            _Current.PadNumberFormat = " {0:D2}";
            _Current.ForceSharedMaterials = false;

            _Current.ShowCantRemoveObjectWarning = true;
            _Current.ShowDeleteObjectsWarning = true;
            _Current.ShowResetCopyPaste = false;
            _Current.TransformOverrideShortLabels = false;

            _Current.ShowOtherTypesInSubmenu = true;
            _Current.ShowOtherTypesInSubmenu = true;
            _Current.PrioritizedFilterTypes = new List<string>() { "Keyframer", "Tween", "MeshRenderer", "Light", "Camera", "AudioTrack" };
            _Current.PrioritizedChannelNames = new List<string>() { "Position", "Rotation", "Scale", "Color" };

            _Current.TransformOverride.Reset();

            _Current.ApplySettings();

#endif
            return _Current;
        }

        private static bool isLoadingAsset = false;

        public static TimeflowPreferences LoadOrCreateSettings()
        {
            if (isLoadingAsset) return _Current;
#if UNITY_EDITOR

            string path = AssetDatabase.GUIDToAssetPath(GUID);
            if (string.IsNullOrEmpty(path)) {
                // The preferences assets do not exist yet, so Restore the defaults
                RestoreDefaultAssets();
            }
            else {
                // Load the preferences asset
                _Current = AssetDatabase.LoadAssetAtPath<TimeflowPreferences>(path);
            }
            if (_Current == null) {
                isLoadingAsset = true;
                GetDefaults();
            }
            else {
                isLoadingAsset = false;
                if (_Current.TrackColors == null) {
                    _Current.TrackColors = TrackColorPalette.CreateOrFindAsset();
                }
            }
#else
            if (_Current == null) {
                GetDefaults();
            }
#endif
            return _Current;
        }

        private static DateTime lastRestoreTime;

        public static void RestoreDefaultAssets()
        {
#if UNITY_EDITOR
            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - lastRestoreTime;

            // Wait for the previous restore operation to complete
            if (elapsedTime.TotalSeconds < 30) {
                return;
            }

            // Update the last execution time.
            lastRestoreTime = DateTime.Now;

            // Get the path to the DefaultSettings.unitypackage containing the assets
            string path = AssetDatabase.GUIDToAssetPath(DefaultSettingsGUID);

            // Implicitly import the package containing the default assets required
            AssetDatabase.ImportPackage(path, false);

            // Verify that the GUID for the preferences is valid
            path = AssetDatabase.GUIDToAssetPath(GUID);
            if (string.IsNullOrEmpty(path)) {
                preferencesNotInstalledCount++;
                if (preferencesNotInstalledCount > 5) {
                    EditorUtil.ShowDialog("Missing Timeflow Preferences",
                        "Unable to locate the TimeflowPreferences asset. " +
                        "Please remove and reinstall Timeflow from the Package Manager");
                }
                return;
            }
            else {
                // Load the preferences asset
                _Current = AssetDatabase.LoadAssetAtPath<TimeflowPreferences>(path);
                Debug.Log("Restored default Timeflow preferences and settings");//--KEEP
                preferencesNotInstalledCount = 0;
            }
#endif
        }

        public static void SaveSettings()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GUIDToAssetPath(GUID);
            if (_Current == null && !string.IsNullOrEmpty(path)) {
                _Current = AssetDatabase.LoadAssetAtPath<TimeflowPreferences>(path);
            }
            if (_Current != null) {
                path = AssetDatabase.GetAssetPath(_Current);
                if (string.IsNullOrEmpty(path) || path.StartsWith("Package")) {
                    // Invalid asset path means that the settings are not properly installed
                    RestoreDefaultAssets();
                }
                else {
                    // The asset is valid and can be saved directly
                    EditorUtil.SetDirty(_Current);
                    AssetDatabase.SaveAssetIfDirty(_Current);
                }
                isLoadingAsset = false;
            }
#endif
        }

        #endregion

        #region ENUMS

        public enum TimeToleranceModes
        {
            Float,
            Frame
        }

        #endregion

        #region PUBLIC SERIALIZED

        public float KeyTolerance;
        public float TimeTolerance = 0.001f;
        public bool UseFractionalTime = true;
        public float CustomTimeStep = 1f;
        public bool TimeToleranceStrict;
        public TimeToleranceModes TimeToleranceMode = TimeToleranceModes.Frame;
        public bool ExposeAllProperties;
        public bool EnableDebug;
        public float MinTimeScale = 0.0001f;
        public bool AlwaysNumberNames = false;
        public string PadNumberFormat = " {0:D2}";
        public bool ForceSharedMaterials = false;

        #endregion

#if UNITY_EDITOR

        #region EDITOR

        public enum DeleteActions
        {
            DeleteGameObject,
            RemoveFromView,
            RemoveTimeflow
        }

        public enum NewChannelAttributeModes
        {
            Auto,
            Combined,
            Separate
        }
#if TIMEFLOW_PRO
        public const string kOpenPreferences = "⚙️ Editor/⚙️ Open Preferences";
#else
        public const string kOpenPreferences = "Editor/Open Preferences";
#endif
        [Shortcut(TimeflowShortcutInfo.Path_OpenPreferences)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kOpenPreferences, false, 10606)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kOpenPreferences, false, 10606)]
        public static void Open() => SettingsService.OpenUserPreferences("Preferences/TimeflowPreferences");

        public bool HideReview = false;
        public bool ShowResetCopyPaste = false;
        public bool TransformOverrideShortLabels = false;

        public bool ScrollTimeWithoutAlt = true;
        public bool ScrollTimeCenterOnMouse = true;
        public bool DrawMinorGridLines = false;
        public float KeyMicroAdjust = 0.1f;
        public float ZoomToggleRange = 1f;
        public bool AllowCommaForDecimalPoints = false;
        public bool UndoTimeChanges = true;
        public TrackColorPalette TrackColors = null;
        public TrackColorMenu.Modes TrackColorMenuMode = TrackColorMenu.Modes.Palette;
        public TimeflowTrack.VisibilityModes DefaultTrackMode = TimeflowTrack.VisibilityModes.On;
        public bool TrackAutoLock = false;
        public bool DefaultTracksLocked = false;
        public bool DefaultCanDragTimeOffset = true;
        public bool TracksSelectObjects = true;
        public bool ObjectsSelectTracks = false;
        public bool AllowHierarchySorting = true;
        public bool OpenOnPrecompose = true;
        public bool CopyLockedTracksAndKeys = false;
        public bool ShowTrackColorsInInspector = true;
        public bool ShowLoopedKeyframes = true;
        public bool DrawLinkedChannels = true;
        public DeleteActions DeleteAction = DeleteActions.RemoveFromView;
        public DeleteActions ControlDeleteAction = DeleteActions.DeleteGameObject;
        public bool EnforceStartTime = true;
        public bool EnforceEndTime = false;
        public bool UnifiedTimeDisplay = true;
        public int DefaultChannelHeight = 20;
        public float ChannelHeightDragSensitivity = 1f;
        public bool ReverseChannelOrder = true;
        public bool AddObjectsToTopOfList = false;
        public bool DisplayScrollbarOnLeft = true;
        public bool DisplayScrollbarOnTop = true;
        public bool AutoScrollToSelection = true;
        public bool MinimizeFloatingViewToBottom = false;
        public bool ShowPresets = true;
        public bool CacheChannelValues = true;
        public bool ShowComponentIcons = true;
        public NewChannelAttributeModes NewChannelAttributeMode = NewChannelAttributeModes.Auto;
        public bool EnableTrackShadows = true;
        public Color TrackShadowColor = new Color(0.1f, 0.1f, 0.1f, 0.15f);
        public Color KeyframeLabelColor = new Color(1, 1f, 1f, 1f);

        [Range(0f, 1f)]
        public float TrackSelectedPattern = 0.25f;
        public bool EnableKeyframeTicks = true;
        public bool EnableSetKeysWhilePlaying = false;
        public bool EnableMidiFileRenaming = true;
        public bool ExpandLoopedKeyframesOverwrite = false;
        public string SceneIncrementalBackupPath = "Assets/_SceneBackup";
        public string DefaultTimeflowName = "Timeflow";
        public string DefaultPrecompName = "Precomp";
        public string PrefabSavePath = "Assets/Prefabs";
        public string CustomRenderPath = "";
        public string FFMPEGPath = "";

        public bool ShowOtherTypesInSubmenu = true;
        public List<string> PrioritizedFilterTypes = new List<string>() { "Keyframer", "Tween", "MeshRenderer", "Light", "Camera", "AudioTrack" };
        public List<string> PrioritizedChannelNames = new List<string>() { "Position", "Rotation", "Scale", "Color" };

        public bool ShowCantRemoveObjectWarning = true;
        public bool ShowDeleteObjectsWarning = true;

        public TimeflowViewDisplay.SearchTypeSorting SearchTypeSorting = TimeflowViewDisplay.SearchTypeSorting.Count;

        [SerializeField] private TransformOverridePreferences _TransformOverride;
        [SerializeField] private List<int> _ChannelHeights;

        public List<int> ChannelHeights {
            get {
                if (_ChannelHeights == null || _ChannelHeights.Count == 0) _ChannelHeights = new List<int>() { 25, 50, 75, 100, 150, 200, 300 };
                return _ChannelHeights;
            }
            set {
                _ChannelHeights = value;
            }
        }

        public TransformOverridePreferences TransformOverride {
            get {
                if (_TransformOverride == null) {
                    _TransformOverride = new TransformOverridePreferences();
                }
                return _TransformOverride;
            }
        }

        public bool ShowTime = true;
        public bool ShowKeyframes = true;
        public bool ShowChannels = true;
        public bool ShowTracks = true;
        public bool ShowInput = true;
        public bool ShowImport = true;
        public bool ShowRendering = true;
        public bool ShowExtras = true;
        public bool ShowShortcuts = true;
        public bool ShowOverrides = true;
        public bool ShowSearchFilters = true;

        public void OnValidate()
        {
            if (KeyTolerance < 0.0001f) KeyTolerance = 0.0001f;
            if (ZoomToggleRange < 0.01f) ZoomToggleRange = 0.01f;
            else
            if (ZoomToggleRange > 100f) ZoomToggleRange = 100f;

            if (KeyMicroAdjust < 0.0001f) KeyMicroAdjust = 0.0001f;
            if (MinTimeScale <= 0f) MinTimeScale = 0.0001f;

            if (Timeflow.Active != null) {
                if (TimeToleranceMode == TimeToleranceModes.Frame) {
                    TimeTolerance = 1f / Timeflow.Active.FPS;
                }
            }
            if (TimeTolerance < 0.0001f) TimeTolerance = 0.0001f;
            if (CustomTimeStep < TimeTolerance) CustomTimeStep = TimeTolerance;

            if (ChannelHeightDragSensitivity < 0.1f) ChannelHeightDragSensitivity = 0.1f;
            else
            if (ChannelHeightDragSensitivity > 2f) ChannelHeightDragSensitivity = 2f;

            if (DefaultChannelHeight < TimeflowPreferences.ChannelMinHeight) DefaultChannelHeight = TimeflowPreferences.ChannelMinHeight;
            else
            if (DefaultChannelHeight > TimeflowPreferences.ChannelMaxHeight) DefaultChannelHeight = TimeflowPreferences.ChannelMaxHeight;

            if (string.IsNullOrEmpty(SceneIncrementalBackupPath)) {
                SceneIncrementalBackupPath = "Assets/_SceneBackup";
            }
            if (string.IsNullOrEmpty(PrefabSavePath)) {
                PrefabSavePath = "Assets/Prefabs";
            }
            if (string.IsNullOrEmpty(DefaultTimeflowName)) {
                DefaultTimeflowName = "Timeflow";
            }
            if (string.IsNullOrEmpty(DefaultPrecompName)) {
                DefaultPrecompName = "Precomp";
            }
        }

        public void ApplySettings()
        {
            OnValidate();
            Current = this;
            TimeflowPreferences.DebugEnabled = EnableDebug;
            SaveSettings();
        }

        public static void ReviewCheck()
        {
            if (Application.isPlaying) return; // only check in edit mode
            int count = EditorPrefs.GetInt("TimeflowReview", 1);
            if (count < 100) {
                if (count == 10) {
                    int r = EditorUtility.DisplayDialogComplex("Enjoying Timeflow?",
                        "Please consider writing a review for Timeflow in the Unity Asset Store. Your feedback matters and helps support the development of Timeflow. Would you like to write a review now?",
                        "Yes", "Remind Me Later", "No, Don't Remind");
                    if (r == 0) {
                        Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/timeflow-animation-system-247895#reviews");
                        count = 100;
                    }
                    else
                    if (r == 1) {
                        count++;
                        EditorPrefs.SetInt("TimeflowReview", count);
                    }
                    else {
                        count = 100; // don't show again
                    }
                }
                else {
                    count++;
                }
                EditorPrefs.SetInt("TimeflowReview", count);
            }
        }

        // GUID to the TimeflowPreferences asset
        public static readonly string GUID = "ddec13a35193cca4b9a89e8be0f49db8";

        // GUID to the included DefaultSettings.unitypackage that contains preferences and other assets
        public static readonly string DefaultSettingsGUID = "b09fc1776b787a94f9709dbad9d5a779";

        internal static SerializedObject GetSerializedSettings()
        {
            TimeflowPreferences prefs = LoadOrCreateSettings();
            if (prefs == null) {
                prefs = GetDefaults();
            }
            return new SerializedObject(prefs);
        }

        public static Color GetNextTrackColor()
        {
            if (Current.TrackColors != null) {
                return Current.TrackColors.GetNextColor();
            }
            return ColorUtil.Random();
        }

        public static Color GetRandomTrackColor()
        {
            if (Current.TrackColors != null) {
                if (Current.TrackColors != null) {
                    return Current.TrackColors.GetRandomColor();
                }
            }
            return ColorUtil.Random();
        }

        public int GetNextChannelHeight(int value, bool next)
        {
            if (ChannelHeights.Count == 0) return DefaultChannelHeight;
            if (ChannelHeights.Count == 1) return ChannelHeights[0];
            int height = -1;
            foreach (int h in ChannelHeights) {
                if (h == value) continue;
                if (height < 0) {
                    if (next) {
                        if (h > value) height = h;
                    }
                    else {
                        if (h < value) height = h;
                    }
                }
                else
                if (next) {
                    if (h > value) height = Mathf.Min(height, h);
                }
                else {
                    if (h < value) height = Mathf.Max(height, h);
                }
            }
            if (height <= 0) {
                if (next) {
                    height = ChannelHeights[ChannelHeights.Count - 1];
                }
                else {
                    height = ChannelHeights[0];
                }
            }
            return height;
        }

        #endregion
#endif
    }

#if UNITY_EDITOR

    /// <summary>
    /// Registers a SettingsProvider using IMGUI
    /// </summary>
    static class TimeflowPreferencesIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateTimeflowPreferencesProvider()
        {
            var provider = new SettingsProvider("Preferences/TimeflowPreferences", SettingsScope.User) {
                label = "Timeflow",

                titleBarGuiHandler = () => {
                    OnTitleGUI();
                },
                guiHandler = (searchContext) => {
                    OnGUI();
                },

                keywords = new HashSet<string>(new[] {
                    "Key Tolerance",
                    "Time Tolerance",
                    "Time Tolerance Strict",
                    "Custom Time Step",
                    "Scroll Time Without Alt",
                    "Scroll Time Centered On Mouse",
                    "Draw Minor Grid Lines",
                    "Key Micro Adjust",
                    "Track Auto Lock",
                    "Zoom Toggle Range",
                    "Default Channel Height",
                    "Channel Height Drag Sensitivity",
                    "Channel Heights",
                    "Undo Time Changes",
                    "Track Color Menu Mode",
                    "Track Colors",
                    "Default Track Mode",
                    "Default Tracks Locked",
                    "Default Can Drag Time Offset",
                    "Tracks Select Objects",
                    "Objects Select Tracks",
                    "Allow Hierarchy Sorting",
                    "Open On Precompose",
                    "Copy Locked Tracks And Keys",
                    "Show Track Colors In Inspector",
                    "Show Looped Keyframes",
                    "Draw Linked Channels",
                    "Delete Action",
                    "Control Delete Action",
                    "Enforce Start Time",
                    "Enforce End Time",
                    "Unified Time Display",
                    "Cache Channel Values",
                    "Show Component Icons",
                    "New Channel Attribute Mode",
                    "Reverse Channel Order",
                    "Add Objects To Top Of List",
                    "Display Scrollbar On Left",
                    "Auto Scroll To Selection",
                    "Minimize Floating View To Bottom",
                    "Expose All Properties",
                    "Enable Track Shadows",
                    "Track Shadow Color",
                    "Enable Keyframe Ticks",
                    "Enable Set Keys While Playing",
                    "Enable MIDI File Renaming",
                    "Scene Incremental Backup Path",
                    "Prefab Save Path",
                    "Custom Render Path",
                    "FFMPEG Path",
                    "Show Cant Remove Object Warning",
                    "Show Delete Objects Warning",
                    "Show Reset Copy Paste",
                    "Transform Override",
                    "Expand Looped Keyframes Overwrite",
                    "Punch Zoom Duration",
                    "Auto Rename MIDI Files .bytes",
                    "$CUSTOM Render Path",
                    "Default Position Min",
                    "Default Position Max",
                    "Default Rotation Min",
                    "Default Rotation Max",
                    "Default Scale Min",
                    "Default Scale Max"
                })

            };

            return provider;
        }

        private static string tooltip;

        private static void OnTitleGUI()
        {
            AxonGUI.Setup(140);
            AxonGUI.FlexibleSpace();
            AxonGUI.LabelInline($"v{Timeflow.Version}");
            if (AxonGUI.Button("Updates & Version History")) {
                Application.OpenURL("https://axongenesis.gitbook.io/timeflow/reference/version-history");
            }
            AxonGUI.ButtonDocs("Timeflow Preferences Documentation", "https://axongenesis.gitbook.io/timeflow/user-guide/menus-and-shortcuts/preferences");
            AxonGUI.ButtonIconUrl(AxonUI.Icons.Discord, "Join the Timeflow Discord", "https://discord.com/invite/sJgtnAF4Tq");
        }

        private static void OnGUI()
        {
            AxonGUI.Setup(140);
            SerializedObject settings = TimeflowPreferences.GetSerializedSettings();
            TimeflowPreferences prefs = (TimeflowPreferences)settings.targetObject;

            AxonGUI.BeginChangeCheck();
            Settings(settings, prefs);

            bool changed = AxonGUI.EndChangeCheck();

            AxonGUI.BeginHorizontalBox();
            if (AxonGUI.Button("Restore Default Settings")) {
                TimeflowPreferences.GetDefaults();
            }
            AxonGUI.EndHorizontal();

            if (changed) {
                settings.ApplyModifiedProperties();
                TimeflowPreferences.Current.ApplySettings();
                if (Timeflow.Active != null) Timeflow.Active.Refresh(true);
            }

            AxonGUI.Space();
            AxonGUI.Space();

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            string label = "If you are enjoying Timeflow, please consider leaving a review";
            EditorGUILayout.LabelField(label, centeredStyle, GUILayout.ExpandWidth(true));

            if (!TimeflowPreferences.Current.HideReview) {
                AxonGUI.BeginHorizontal();
                AxonGUI.FlexibleSpace();
                AxonGUI.ButtonIconUrl(AxonUI.Icons.Review, "Review Timeflow in the Unity Asset Store", "https://u3d.as/31KB#reviews");
                AxonGUI.FlexibleSpace();
                AxonGUI.EndHorizontal();
                AxonGUI.Space();
            }

            AxonGUI.Space();
            AxonGUI.ResetLabelWidth();
        }

        public static void Settings(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.SetLabelWidth(200);

            TimeGUI(settings, prefs);
            TracksGUI(settings, prefs);
            ObjectsGUI(settings, prefs);
            ChannelsGUI(settings, prefs);
            KeyframesGUI(settings, prefs);
            SearchFiltersGUI(settings, prefs);
            InputGUI(settings, prefs);
            ImportGUI(settings, prefs);
            RenderingGUI(settings, prefs);
            OverridesGUI(settings, prefs);
            ExtrasGUI(settings, prefs);
            PresetsGUI(settings, prefs);
            ShortcutsGUI(settings, prefs);

            EditorGUILayout.Space();

            settings.ApplyModifiedPropertiesWithoutUndo();
            AxonGUI.Space();
        }

        private static void TimeGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowTime = AxonGUI.Foldout(prefs.ShowTime, "Time");
            if (prefs.ShowTime) {
                AxonGUI.BeginBox();

                tooltip = "When enabled, the grid and time display show the same unit (seconds, frames, timecode, measures). Disable this option to set the grid time display independently of the primary time display.";
                EditorGUILayout.PropertyField(settings.FindProperty("UnifiedTimeDisplay"), new GUIContent("Unified Time Display", tooltip));

                tooltip = "This is a global setting which sets the smallest change in value allowed when setting keyframes, which can avoid the generation of unwanted keyframes due to precision errors.";
                SerializedProperty timeToleranceMode = settings.FindProperty("TimeToleranceMode");
                EditorGUILayout.PropertyField(timeToleranceMode, new GUIContent("Time Mode", tooltip));

                tooltip = "This sets the exact time interval to base time increments on. If the time mode is based on frames, this value is calculated by dividing 1 second by the FPS.";
                EditorGUILayout.PropertyField(settings.FindProperty("TimeTolerance"), new GUIContent("Time Tolerance", tooltip));

                tooltip = "If strict is enabled, Timeflow only renders time on integral frames or by the time tolerance set. This means that if the current time falls between frames, that the time is rounded to the nearest frame. This mode is useful for frame accuracy when rendering to disk or creating stop motion effects, however in realtime setups strict time tolerance can introduce unwanted stepping in the playback since Unity engine under normal circumstances renders the highest frame rate possible.";
                EditorGUILayout.PropertyField(settings.FindProperty("TimeToleranceStrict"), new GUIContent("Strict Tolerance", tooltip));

                tooltip = "If enabled, timecode is displayed ending in frame numbers rather than milliseconds";
                EditorGUILayout.PropertyField(settings.FindProperty("UseFractionalTime"), new GUIContent("Use Fractional Time", tooltip));

                tooltip = "This sets the time step when using the keyboard shortcut Control + Shift + Page Up/Down.";
                EditorGUILayout.PropertyField(settings.FindProperty("CustomTimeStep"), new GUIContent("Custom Time Step", tooltip));

                tooltip = "When enabled, keyframes and tracks are prevented from being dragged before the start time. Disable this if you wish to drag keys into negative time.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnforceStartTime"), new GUIContent("Enforce Start Time", tooltip));

                tooltip = "When enabled, dragging keyframes and tracks are prevented from passing the end time. Disable this if you wish to drag keys beyond the Timeflow duration.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnforceEndTime"), new GUIContent("Enforce End Time", tooltip));

                tooltip = "Time Scale controls playback speed and must be a value greater than 0. Set the minimimum Time Scale threshold as desired.";
                EditorGUILayout.PropertyField(settings.FindProperty("MinTimeScale"), new GUIContent("Minimum Time Scale", tooltip));

                tooltip = "When enabled, explicit changes to the time in Timeflow are registered as undoable events. Disable this to reduce the number of undo operations which can improve editor performance with large scenes.";
                EditorGUILayout.PropertyField(settings.FindProperty("UndoTimeChanges"), new GUIContent("Undo Time Changes", tooltip));

                tooltip = "Enable this setting to draw lighter grid lines subdividing the selected time snap increment. If this option is off, only the snappable grid times are displayed.";
                EditorGUILayout.PropertyField(settings.FindProperty("DrawMinorGridLines"), new GUIContent("Draw Minor Grid Lines", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void TracksGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowTracks = AxonGUI.Foldout(prefs.ShowTracks, "Tracks");
            if (prefs.ShowTracks) {
                AxonGUI.BeginBox();

                tooltip = "This setting determines the default visibility modes for track channels, which controls how tracks affect object visibility " +
                    "and/or behavior execution.\n" +
                    "On: This is the default mode. The track does not affect visibility but does control whether behaviors execute.\n" +
                    "Activate: This activates/deactivates the game object with the track sections.\n" +
                    "Renderer: Enables/disables renderer components, making objects invisible but keeping them alive and active.\n" +
                    "RendererIndependent: Same as Renderer mode, but only affects this object, not children.\n" +
                    "ActivateChildren: Activates/deactivates child objects, but keeps the parent object active.\n" +
                    "OnSelfOnly: Same as On mode, but tracks only affect this object and do not affect child objects.";
                EditorGUILayout.PropertyField(settings.FindProperty("DefaultTrackMode"), new GUIContent("Default Track Mode", tooltip));

                tooltip = "Sets the default locked state for new tracks. Locked tracks cannot be moved or changed until they are unlocked. Automatically locking tracks can help" +
                    "prevent them from being moved accidentally when working with keyframes in the track view.";
                EditorGUILayout.PropertyField(settings.FindProperty("DefaultTracksLocked"), new GUIContent("Default Locked", tooltip));

                tooltip = "Enable this setting if you want all new tracks to have 'Drag Time Offset' enabled. Otherwise, it defaults off and allows tracks to be independently " +
                    "moved and split like keyframes. Which mode you use depends on personal preference and the type of animation being created with Timeflow.";
                EditorGUILayout.PropertyField(settings.FindProperty("DefaultCanDragTimeOffset"), new GUIContent("Default Drag Time Offset", tooltip));

                tooltip = "If enabled, tracks set to Auto Full Length are automatically locked to prevent modification. You may disable this if you prefer tracks to remain " +
                    "unlocked, which facilitates working with Drag Time Offset.";
                EditorGUILayout.PropertyField(settings.FindProperty("TrackAutoLock"), new GUIContent("Track Auto Full Length Lock", tooltip));

                tooltip = "When enabled, selecting keyframes and tracks in the timeline, it also selects the channels and objects they belong to. If disabled, object and channel " +
                    "selection is unaffected by keyframe and track selection.";
                EditorGUILayout.PropertyField(settings.FindProperty("TracksSelectObjects"), new GUIContent("Tracks Select Objects", tooltip));

                tooltip = "When enabled, selecting objects in the hieararchy panel also selects their tracks in the timeline view. " +
                    "If disabled, track selection remains unchanged when selecting objects.";
                EditorGUILayout.PropertyField(settings.FindProperty("ObjectsSelectTracks"), new GUIContent("Objects Select Tracks", tooltip));

                tooltip = "If enabled, locked tracks and keyframes can be copied and pasted. Otherise if disabled, only unlocked tracks and keyframes may be copy-pasted. " +
                    "This defaults off to make keyframe editing more friendly by ignoring full length tracks which are locked by default.";
                EditorGUILayout.PropertyField(settings.FindProperty("CopyLockedTracksAndKeys"), new GUIContent("Copy Locked Tracks and Keys", tooltip));

                EditorGUILayout.BeginHorizontal();
                if (TimeflowPreferences.Current.TrackColors == null) TrackColorPalette.CreateOrFindAsset();
                EditorGUILayout.PropertyField(settings.FindProperty("TrackColors"), new GUIContent("Track Colors"));
                if (AxonGUI.ButtonInline("New")) {
                    TimeflowPreferences.Current.TrackColors = TrackColorPalette.NewAsset();
                }
                if (AxonGUI.ButtonInline("Edit")) {
                    TrackColorPalette.RevealAsset(TimeflowPreferences.Current.TrackColors);
                }
                EditorGUILayout.EndHorizontal();

                tooltip = "If enabled, select objects and channels have their corresponding components in the inspector outlined by the track color. This is to help visually" +
                    " associate selected items in the Timeflow view with the inspector window";
                EditorGUILayout.PropertyField(settings.FindProperty("ShowTrackColorsInInspector"), new GUIContent("Show Track Colors In Inspector", tooltip));

                tooltip = "Controls the opacity of the selected track diagonal bar pattern";
                EditorGUILayout.PropertyField(settings.FindProperty("TrackSelectedPattern"), new GUIContent("Track Selected Pattern", tooltip));

                tooltip = "When enabled, areas outside of or between track ranges are shaded slightly darker to indicate areas of inactivity, where behaviors do not update.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnableTrackShadows"), new GUIContent("Enable Track Shadows", tooltip));

                if (TimeflowPreferences.Current.EnableTrackShadows) {
                    tooltip = "When enabled, areas outside of or between track ranges are shaded slightly darker to indicate areas of inactivity, where behaviors do not update.";
                    EditorGUILayout.PropertyField(settings.FindProperty("TrackShadowColor"), new GUIContent("Track Shadow Color", tooltip));
                }

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void ObjectsGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowChannels = AxonGUI.Foldout(prefs.ShowChannels, "Objects");
            if (prefs.ShowChannels) {
                AxonGUI.BeginBox();

                tooltip = "If enabled, new objects instantiated will automatically be named with a number (ex. Object 01). Otherwise if off, a number is only appended " +
                    "when a game object by the same name already exists. This option is useful to turn on when you want all objects to be numbered. You can customize the number " +
                    "format in the next field.";
                EditorGUILayout.PropertyField(settings.FindProperty("AlwaysNumberNames"), new GUIContent("Always Number Object Names", tooltip));

                tooltip = "This defines the string formatting template for appending numbers to object names. Examples:\n" +
                    "Format     => Object Name\n" +
                    "D2         => Object01\n" +
                    "_{0:D2}    => Object_01\n" +
                    "_{0:D3}    => Object_001\n";
                EditorGUILayout.PropertyField(settings.FindProperty("PadNumberFormat"), new GUIContent("Pad Number Format", tooltip));

                tooltip = "Sets the base name for new Timeflow instances";
                EditorGUILayout.PropertyField(settings.FindProperty("DefaultTimeflowName"), new GUIContent("Default Timeflow Name", tooltip));

                tooltip = "Sets the base name for new precomps";
                EditorGUILayout.PropertyField(settings.FindProperty("DefaultPrecompName"), new GUIContent("Default Precomp Name", tooltip));

                tooltip = "Enable this setting to show the icons for each object and channel's component type. For objects with multiple components, you may click " +
                    "the icon next to the object in the Timeflow view to select which of the component icons is displayed.";
                EditorGUILayout.PropertyField(settings.FindProperty("ShowComponentIcons"), new GUIContent("Show Component Icons", tooltip));

                tooltip = "If enabled, when objects are added to the Timeflow view, they are placed at the top of the view list. Otherwise if off, " +
                    "objects are added at the bottom of the list.";
                EditorGUILayout.PropertyField(settings.FindProperty("AddObjectsToTopOfList"), new GUIContent("Add Objects to Top of List", tooltip));

                tooltip = "Sets the behavior when selected objects are deleted using the Delete or Backspace key. By default the game object is destroyed. " +
                    "Alternatively, it can remove the object from the timeflow view (without destroying anything). Or it can remove " +
                    "the object from Timeflow completely by destroying all Timeflow behaviors on the selected objects.";
                EditorGUILayout.PropertyField(settings.FindProperty("DeleteAction"), new GUIContent("Delete Action", tooltip));

                tooltip = "Defines the behavior of the Delete or Backspace key while the Control key is held. ";
                EditorGUILayout.PropertyField(settings.FindProperty("ControlDeleteAction"), new GUIContent("Control + Delete Action", tooltip));

                tooltip = "If enabled, sorting objects in the Timeflow view also affects sorting of objects in the Hierarchy view. Use this option to sync sibling indices " +
                    "so that the order of objects in the Timeflow view matches the order in the Hiearchy. Turn this option off if you do not want the object hierarchy " +
                    "sorting changed by Timeflow";
                EditorGUILayout.PropertyField(settings.FindProperty("AllowHierarchySorting"), new GUIContent("Allow Hierarchy Sorting", tooltip));

                tooltip = "If enabled, the Timeflow view will switch to the newly created Timeflow group when Precompose is selected from the menu.";
                EditorGUILayout.PropertyField(settings.FindProperty("OpenOnPrecompose"), new GUIContent("Open On Precompose", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void ChannelsGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowChannels = AxonGUI.Foldout(prefs.ShowChannels, "Channels");
            if (prefs.ShowChannels) {
                AxonGUI.BeginBox();

                tooltip = "Determines whether channels are prioritized from top down or bottom up. Enable this setting to list from the bottom up, " +
                    "with channels on top processing after those below it (overwriting values if applicable).";
                EditorGUILayout.PropertyField(settings.FindProperty("ReverseChannelOrder"), new GUIContent("Reverse Channel Order", tooltip));

                tooltip = "If enabled, linked channels display ghosted keyframes or channel curve in the target channel as reference. This can help " +
                    "see how the data is being replicated across channels. The ghosted data is view only and can only be edited on the original channel.";
                EditorGUILayout.PropertyField(settings.FindProperty("DrawLinkedChannels"), new GUIContent("Draw Linked Channel Data", tooltip));

                tooltip = "When using auto keyframe detection on a multi-attibute property (such as a vector or color) this setting determines whether the " +
                    "new channel is created as a combined value (XYZ), or as separate channels for each attribute (X, Y, Z). If Auto mode is selected, " +
                    "Color and Rect types are treated as combined, while vector values are treated as separate.";
                EditorGUILayout.PropertyField(settings.FindProperty("NewChannelAttributeMode"), new GUIContent("New Channel Attribute Mode", tooltip));

                tooltip = "Defines how much mouse movement applies to the channel height. A sensitivity lower than 1 makes it easier to make fine tuned adjustments.";
                EditorGUILayout.PropertyField(settings.FindProperty("ChannelHeightDragSensitivity"), new GUIContent("Channel Height Drag Sensitivity", tooltip));

                tooltip = "This sets the vertical spacing of channels as displayed in the object panel.";
                EditorGUILayout.PropertyField(settings.FindProperty("DefaultChannelHeight"), new GUIContent("Default Channel Height", tooltip));

                if (prefs.ChannelHeights.Count == 0) {
                    Debug.LogWarning("Failed to allocate default channel heights.");
                }
                tooltip = "Define preset channel heights to list in the channel context menu (right click a channel in the Timeflow view).";
                EditorGUILayout.PropertyField(settings.FindProperty("_ChannelHeights"), new GUIContent("Channel Height Presets", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void KeyframesGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowKeyframes = AxonGUI.Foldout(prefs.ShowKeyframes, "Keyframes");
            if (prefs.ShowKeyframes) {
                AxonGUI.BeginBox();

                tooltip = "Sets the label color for keyframe values";
                EditorGUILayout.PropertyField(settings.FindProperty("KeyframeLabelColor"), new GUIContent("Label Color", tooltip));

                tooltip = "This is a global setting which sets the smallest change in value allowed when setting keyframes, which can avoid the generation of unwanted keyframes due to precision errors.";
                EditorGUILayout.PropertyField(settings.FindProperty("KeyTolerance"), new GUIContent("Key Tolerance", tooltip));

                tooltip = "When moving keyframes in the Graph view, hold the Alt key (after starting the drag) to make micro adjustments to the value. A lower (non-zero) number will produce finer tweaks.";
                EditorGUILayout.PropertyField(settings.FindProperty("KeyMicroAdjust"), new GUIContent("Key Micro Adjust", tooltip));

                tooltip = "When enabled, keyframes are indicated on the object track as small 'tick' marks. This is to help see where keyframes are set on objects even if their channels are hidden from the display.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnableKeyframeTicks"), new GUIContent("Enable Keyframe Ticks", tooltip));

                tooltip = "Sets the color of keyframe value labels, when enabled in the Timeflow view.";
                EditorGUILayout.PropertyField(settings.FindProperty("KeyframeLabelColor"), new GUIContent("Keyframe Label Color", tooltip));

                tooltip = "This allows keyframes to be set on value change while Timeflow is playing, resulting in recording a stream of input as keyframes. This is usually disabled to avoid creating unwanted keyframes, but can be used to record user input over time.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnableSetKeysWhilePlaying"), new GUIContent("Set Keyframes While Playing", tooltip));

                tooltip = "When looping is enabled on a keyframe channel and this option is enabled, repeated kefyrames in the looped region of time are displayed ghosted for references. This helps provide a visual for the looping occuring on the channel. The ghosted keyframes cannot be selected or edited directly and are only for display. This option may be turned off to improve GUI drawing speed if it is not needed.";
                EditorGUILayout.PropertyField(settings.FindProperty("ShowLoopedKeyframes"), new GUIContent("Show Looped Keyframes", tooltip));

                tooltip = "If enabled, when using the context menu command 'Expand Looped Keyframes', any existing keyframes in the loop or time span are overwritten. To keep all keyframes, disable this setting.";
                EditorGUILayout.PropertyField(settings.FindProperty("ExpandLoopedKeyframesOverwrite"), new GUIContent("Looped Keyframes Overwrite", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void InputGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowInput = AxonGUI.Foldout(prefs.ShowInput, "Input");
            if (prefs.ShowInput) {
                AxonGUI.BeginBox();

                tooltip = "If enabled, the Timeflow view automatically scrolls to the object selected, when selected in the scene or from the Hierarchy view.";
                EditorGUILayout.PropertyField(settings.FindProperty("AutoScrollToSelection"), new GUIContent("Auto Scroll to Selection", tooltip));

                tooltip = "If enabled, the mouse scroll wheel alone can be used to zoom the timeline view in and out (without holding the Alt key).";
                EditorGUILayout.PropertyField(settings.FindProperty("ScrollTimeWithoutAlt"), new GUIContent("Scroll Time Without Alt Key", tooltip));

                tooltip = "If enabled, the scrolling to zoom the timeline centers on the mouse position, or if disabled the zoom is centered on the current time.";
                EditorGUILayout.PropertyField(settings.FindProperty("ScrollTimeCenterOnMouse"), new GUIContent("Scroll Time Centered On Mouse", tooltip));

                tooltip = "Enable this setting to display the vertical scrollbar on the lefthand side of the timeline, otherwise by default it is displayed on the right edge of the view.";
                EditorGUILayout.PropertyField(settings.FindProperty("DisplayScrollbarOnLeft"), new GUIContent("Display Scrollbar On Left", tooltip));

                tooltip = "Enable this setting to display the horizontal scrollbar above the timeline, otherwise by default it is displayed at the bottom of the view.";
                EditorGUILayout.PropertyField(settings.FindProperty("DisplayScrollbarOnTop"), new GUIContent("Display Scrollbar On Top", tooltip));

                tooltip = "(formerly Punch Zoom) This sets the duration of time to fit in the timeline when using the Zoom Toggle command. The value is in seconds, so a smaller value zooms in more. Adding the Shift key when zooming results in a 4x zoom factor.";
                EditorGUILayout.PropertyField(settings.FindProperty("ZoomToggleRange"), new GUIContent("Zoom Toggle (time range in seconds)", tooltip));

                tooltip = "If enabled, typing a number such as 1,234 is interpretted as 1.234. Note that this may conflict with system cultural settings and should be turned off if your system already uses commas for decimal numbers.";
                EditorGUILayout.PropertyField(settings.FindProperty("AllowCommaForDecimalPoints"), new GUIContent("Allow Comma for Decimals", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void ImportGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowImport = AxonGUI.Foldout(prefs.ShowImport, "Import");
            if (prefs.ShowImport) {
                AxonGUI.BeginBox();

                tooltip = "If enabled, any assets files with the .mid or .midi extension are automatically renamed to .bytes for proper import as a TextAsset. " +
                "This feature may be disabled if you wish to manually control file names. Note that the .bytes extension is required to properly read the " +
                "binary data from the TextAsset, otherwise MidiFile will generate errors when attempting to read the file.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnableMidiFileRenaming"), new GUIContent("Auto Rename MIDI Files .bytes", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void RenderingGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowRendering = AxonGUI.Foldout(prefs.ShowRendering, "Rendering");
            if (prefs.ShowRendering) {
                AxonGUI.BeginBox();

                tooltip = "If enabled, material properties apply to shared materials (the original assets), rather than the runtime instanced materials (default behavior in Unity). " +
                    "Only enable this setting if you are sure about the implications. To force shared materials for a specific object only, apply the component MaterialPropertyOptions to override the setting.";
                EditorGUILayout.PropertyField(settings.FindProperty("ForceSharedMaterials"), new GUIContent("Force Shared Materials Only", tooltip));

                tooltip = "Enter a full file system path to use the $CUSTOM wildcard property when setting output paths for RenderToDisk.";
                EditorGUILayout.PropertyField(settings.FindProperty("CustomRenderPath"), new GUIContent("$CUSTOM Render Path", tooltip));

                tooltip = "Enter a full file system path to the ffmpeg executable. For typical installs where ffmpeg is defined in the system path variables, this setting " +
                "is not needed and should be left blank. However, you may enter a full path to the executable if needed.";
                EditorGUILayout.PropertyField(settings.FindProperty("FFMPEGPath"), new GUIContent("FFMPEG Path", tooltip));

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void OverridesGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowOverrides = AxonGUI.Foldout(prefs.ShowOverrides, "Overrides");
            if (prefs.ShowOverrides) {
                AxonGUI.SetTooltip("Enabling overrides allows the Transform inspector to be customized for Timeflow. Disabling overrides adds the scripting define " +
                    $"symbol {TimeflowEditorOverrides._TIMEFLOW_OVERRIDES_DISABLED} to the Player Settings. Please wait for scripts to recompile after making a change to this setting.");
                if (AxonGUI.Button(TimeflowEditorOverrides.IsOverrideDisabled ? "Enable Overrides" : "Disable Overrides")) {
                    TimeflowEditorOverrides.EnableOverrides(TimeflowEditorOverrides.IsOverrideDisabled);
                }
                AxonGUI.Space();

                if (!TimeflowEditorOverrides.IsOverrideDisabled) {
                    AxonGUI.BeginBox();
                    AxonGUI.Heading("Transform Override Global Default Settings");

                    AxonGUI.BeginBox();
                    tooltip = "Set the global default setting for the transform inspector.";

                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip(tooltip);
                    AxonGUI.Label("Position", GUILayout.Width(180));
                    AxonGUI.UndoName = "Transform Override Position Min";
                    prefs.TransformOverride.DefaultPositionMin = AxonGUI.FieldFloatInline(prefs, "Min", prefs.TransformOverride.DefaultPositionMin);

                    AxonGUI.UndoName = "Transform Override Position Max";
                    prefs.TransformOverride.DefaultPositionMax = AxonGUI.FieldFloatInline(prefs, "Max", prefs.TransformOverride.DefaultPositionMax);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label("Rotation", GUILayout.Width(180));
                    AxonGUI.UndoName = "Transform Override Rotation Min";
                    prefs.TransformOverride.DefaultRotationMin = AxonGUI.FieldFloatInline(null, "Min", prefs.TransformOverride.DefaultRotationMin);

                    AxonGUI.UndoName = "Transform Override Rotation Max";
                    prefs.TransformOverride.DefaultRotationMax = AxonGUI.FieldFloatInline(null, "Max", prefs.TransformOverride.DefaultRotationMax);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label("Scale", GUILayout.Width(180));
                    AxonGUI.UndoName = "Transform Override Scale Min";
                    prefs.TransformOverride.DefaultScaleMin = AxonGUI.FieldFloatInline(null, "Min", prefs.TransformOverride.DefaultScaleMin);

                    AxonGUI.UndoName = "Transform Override Scale Max";
                    prefs.TransformOverride.DefaultScaleMax = AxonGUI.FieldFloatInline(null, "Max", prefs.TransformOverride.DefaultScaleMax);
                    AxonGUI.EndHorizontal();

                    AxonGUI.EndBox();

                    AxonGUI.Space();
                    AxonGUI.EndBox();
                }
            }
            AxonGUI.EndBox();
        }

        private static void SearchFiltersGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowSearchFilters = AxonGUI.Foldout(prefs.ShowSearchFilters, "Search Filters");
            if (prefs.ShowExtras) {
                AxonGUI.BeginBox();

                AxonGUI.BeginChangeCheck();
                tooltip = "Determines how components are ordered in the search type drop down menu.\n" +
                    "Alphabetical - lists component types by name in ascending order\n" +
                    "Count - lists component types with the highest occurrence first\n" +
                    "Prioritized - matches the custom sorting of the Prioritized Filter Types list";
                EditorGUILayout.PropertyField(settings.FindProperty("SearchTypeSorting"), new GUIContent("Search Type Sorting", tooltip));

                if (prefs.SearchTypeSorting == TimeflowViewDisplay.SearchTypeSorting.Prioritized) {
                    tooltip = "Enable this setting to put all filter types and channel names other than those prioritized into a submenu under 'More'. Otherwise they are all displayed in one long menu.";
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowOtherTypesInSubmenu"), new GUIContent("Show Others in Submenu", tooltip));

                    tooltip = "Specifies component type names that are given first priority in the drop down menu when searching in the Timeflow view.";
                    EditorGUILayout.PropertyField(settings.FindProperty("PrioritizedFilterTypes"), new GUIContent("Prioritized Filter Types", tooltip));

                    tooltip = "Specifies channel names (or partial names) that are given first priority in the drop down menu when searching in the Timeflow view.";
                    EditorGUILayout.PropertyField(settings.FindProperty("PrioritizedChannelNames"), new GUIContent("Prioritized Channel Names", tooltip));
                }
                if(AxonGUI.EndChangeCheck()) {
                    if (Timeflow.Active != null && Timeflow.Active.View != null) {
                        settings.ApplyModifiedProperties();
                        TimeflowPreferences.Current.ApplySettings();
                        Timeflow.Active.View.Display.BuildSearchMenus();
                    }
                }

                AxonGUI.EndBox();

            }
            AxonGUI.EndBox();
        }

        private static void PresetsGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowExtras = AxonGUI.Foldout(prefs.ShowExtras, "Presets");
            if (prefs.ShowExtras) {
                AxonGUI.BeginBox();

                tooltip = "If enabled, the presets column is displayed in the switches panel allowing you quick access to the presets menu for object and behaviors. Turn this option of if you do not wish to see presets in the Timeflow view.";
                EditorGUILayout.PropertyField(settings.FindProperty("ShowPresets"), new GUIContent("Show Presets in Timeflow View", tooltip));

                AxonGUI.Space();
                AxonGUI.BeginHorizontalBox();
                bool isInstalled = AdvancedPresetsGlobalConfig.ArePresetsInstalled();
                GUI.color = isInstalled ? AxonColor.SoftWhite : AxonColor.LightGreen;
                if (AxonGUI.Button((isInstalled ? "Re-" : "") + "Import Demo Samples & Presets")) {
                    AdvancedPresetsGlobalConfig.ImportDemoSamples();
                }
                GUI.color = Color.white;
                AxonGUI.EndHorizontal();

                AxonGUI.EndBox();

            }
            AxonGUI.EndBox();
        }

        private static void ExtrasGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowExtras = AxonGUI.Foldout(prefs.ShowExtras, "Extras");
            if (prefs.ShowExtras) {
                AxonGUI.BeginBox();

                tooltip = "This setting is only applicable when the Timeflow view is floating (undocked). If enabled, the Timeflow view minimizes to the bottom of the screen. Othewise if off, the view keeps its position but shrinks vertically when minimized. " +
                    "With either option, unminimizing the view restores it to its previous size and placement.";
                EditorGUILayout.PropertyField(settings.FindProperty("MinimizeFloatingViewToBottom"), new GUIContent("Minimize Floating View To Bottom", tooltip));

                tooltip = "Sets the assets directory path to save incremental scene backups. Each time a backup is saved, a numbered copy of the current scene is saved to the folder, and then the current scene is saved. Note that the backup is made of the existing scene file before saving, so it is backing up the last version while the current scene remains the active version.";
                EditorGUILayout.PropertyField(settings.FindProperty("SceneIncrementalBackupPath"), new GUIContent("Scene Backup Path", tooltip));

                tooltip = "Sets the assets directory path to save new prefabs. Use this with the menu command 'Save Selected Prefabs' (Alt + Shift + Control + S)";
                EditorGUILayout.PropertyField(settings.FindProperty("PrefabSavePath"), new GUIContent("Prefab Save Path", tooltip));

                tooltip = "This controls whether debug logging is enabled for all TimeflowBehavior derrived objects. When enabled, a debug icon is displayed in the top bar of inspectors and next to properties and channels, which toggles debug logging for individual objects and properties.";
                EditorGUILayout.PropertyField(settings.FindProperty("EnableDebug"), new GUIContent("Enable Debug Logging", tooltip));

                tooltip = "Only use this for debugging purposes. When enabled, all object properties are displayed (when selecting or adding a new property channel) without filtering by type. This may allow for invalid property assignments. ";
                EditorGUILayout.PropertyField(settings.FindProperty("ExposeAllProperties"), new GUIContent("Expose All Properties", tooltip));

                AxonGUI.Space();
                AxonGUI.SetTooltip("Enable Pro mode to show Timeflow in the main menu bar, instead of under Tools. Enabling Timeflow Pro adds the scripting define " +
                     $"symbol {TimeflowEditorOverrides._TIMEFLOW_PRO} to the Player Settings. Timeflow Pro mode is an editor-only feature and does not affect functionality or performance. Please wait for scripts to recompile after making a change to this setting.");
                if (AxonGUI.Button(TimeflowEditorOverrides.IsTimeflowPro ? "Disable Timeflow Pro" : "Enable Timeflow Pro")) {
                    TimeflowEditorOverrides.ToggleTimeflowPro();
                }

                AxonGUI.Space();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private static void ShortcutsGUI(SerializedObject settings, TimeflowPreferences prefs)
        {
            AxonGUI.BeginBox();
            prefs.ShowShortcuts = AxonGUI.Foldout(prefs.ShowShortcuts, "Shortcuts");
            if (prefs.ShowShortcuts) {
                AxonGUI.BeginHorizontalBox();
                AxonGUI.FlexibleSpace();
                AxonGUI.SetTooltip("Export the Timeflow keyboard shortcuts currently assigned to a CSV file as reference and backup. These shortcuts can then be reloaded later.");
                if (AxonGUI.ButtonInline("Export")) {
                    TimeflowShortcuts.ExportShortcuts();
                }
                AxonGUI.SetTooltip("Import Timeflow keyboard shortcuts from a previously saved CSV file. Please note that externally editing the file is discouraged and may cause errors during import.");
                if (AxonGUI.ButtonInline("Import")) {
                    TimeflowShortcuts.ImportShortcuts();
                }
                // This is not properly implemented yet.
                //AxonGUI.SetTooltip("Rebuilds the shortcuts displayed in the menus, updating them to any customized shortcuts.");
                //if (AxonGUI.ButtonInline("Update Bindings")) {
                //    TimeflowShortcuts.UpdateShortcutBindings();
                //}
                AxonGUI.SetTooltip("Revert all keyboard shortcuts for Timeflow to the original default settings.");
                if (AxonGUI.ButtonInline("Reset Shortcuts")) {
                    TimeflowShortcuts.ResetShortcutsToDefault();
                }
                AxonGUI.SetTooltip("Open the Shortcuts Manager to the Timeflow category.");
                if (AxonGUI.ButtonInline("Shortcuts Manager...")) {
                    TimeflowShortcuts.OpenShortcutsManager();
                }
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();
        }
    }

#endif

}//AxonGenesis
