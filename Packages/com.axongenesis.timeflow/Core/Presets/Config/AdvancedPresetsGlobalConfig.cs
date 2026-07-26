// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

namespace AxonGenesis
{
    [Serializable]
    /// <summary>
    /// The purpose of this object is to persist data for the AdvancedPresetsWindow. The majority of the impelmentation is 
    /// static because there 
    /// </summary>
    public class AdvancedPresetsGlobalConfig : ScriptableObject
    {
        #region STATIC

        private static readonly string GUID = "f2b2e947a2829844a910dc28378fb9bd";

        private static AdvancedPresetsGlobalConfig _Instance = null;

        public static AdvancedPresetsGlobalConfig Instance {
            get {
                if (_Instance == null) {
                    string path = AssetDatabase.GUIDToAssetPath(GUID);
                    if (string.IsNullOrEmpty(path)) {
                        string[] guids = AssetDatabase.FindAssets("t:AdvancedPresetsGlobalConfig");
                        foreach (string guid in guids) {
                            path = AssetDatabase.GUIDToAssetPath(guid);
                            _Instance = AssetDatabase.LoadAssetAtPath<AdvancedPresetsGlobalConfig>(path);
                        }
                    }
                    else {
                        _Instance = AssetDatabase.LoadAssetAtPath<AdvancedPresetsGlobalConfig>(path);
                    }
                    if (_Instance == null) {
                        _Instance = ScriptableObject.CreateInstance<AdvancedPresetsGlobalConfig>();
                        if (string.IsNullOrEmpty(path)) {
                            path = "Assets/Plugins/Timeflow/AdvancedPresets/AdvancedPresetsConfig.asset";
                        }
                        string directory = System.IO.Path.GetDirectoryName(path);
                        if (!System.IO.Directory.Exists(directory)) {
                            System.IO.Directory.CreateDirectory(directory);
                        }
                        AssetDatabase.CreateAsset(_Instance, path);
                    }
                }
                return _Instance;
            }
            set {
                if (_Instance != value && value != null) {
                    _Instance = value;
                }
            }
        }

        private const string kStandardPresetsCollectionGUID = "e5cbba3312d080b43b250f63279f38d5";

        public static bool ArePresetsInstalled()
        {
            return AdvancedPresetsCollection.AllCollections.Count > 0;
        }

        public static void ImportDemoSamples()
        {
            if (EditorUtility.DisplayDialog(
                "Import the Demo Samples",
                "Please go to the Package Manager and locate the Timeflow Animation System Samples package. Then install the Demo samples for your current render pipeline. The presets are included with the demo samples.",
                "OK"
            )) {
                EditorUtil.OpenPackageManager();
            }
        }

        public static MatchModes MatchMode {
            get {
                return Instance == null ? MatchModes.MatchBySiblingIndex : _Instance._MatchMode;
            }
            set {
                if (Instance == null) return;
                _Instance._MatchMode = value;
            }
        }

        public static bool CanAddChildren {
            get {
                return Instance == null ? false : _Instance._CanAddChildren;
            }
            set {
                if (Instance == null) return;
                _Instance._CanAddChildren = value;
            }
        }

        public static bool CanSetTrackColors {
            get {
                return Instance == null ? false : _Instance._CanSetTrackColors;
            }
            set {
                if (Instance == null) return;
                _Instance._CanSetTrackColors = value;
            }
        }

        public static bool CanRenameObjects {
            get {
                return Instance == null ? false : _Instance._CanRenameObjects;
            }
            set {
                if (Instance == null) return;
                _Instance._CanRenameObjects = value;
            }
        }

        public static bool UnpackPrefabs {
            get {
                return Instance == null ? false : _Instance._UnpackPrefabs;
            }
            set {
                if (Instance == null) return;
                _Instance._UnpackPrefabs = value;
            }
        }

        public static bool AutoHideCollections {
            get {
                return Instance == null ? true : _Instance._AutoHideCollections;
            }
            set {
                if (Instance == null) return;
                _Instance._AutoHideCollections = value;
            }
        }

        public static bool ShowColoredButtons {
            get {
                return Instance == null ? true : _Instance._ShowColoredButtons;
            }
            set {
                if (Instance == null) return;
                _Instance._ShowColoredButtons = value;
            }
        }

        public static bool ShowColoredHeadings {
            get {
                return Instance == null ? true : _Instance._ShowColoredHeadings;
            }
            set {
                if (Instance == null) return;
                _Instance._ShowColoredHeadings = value;
            }
        }

        public static float ButtonSaturation {
            get {
                return Instance == null ? 1f : _Instance._ButtonSaturation;
            }
            set {
                if (Instance == null) return;
                if (_Instance._ButtonSaturation != value) {
                    _Instance._ButtonSaturation = value;
                }
            }
        }

        public static float HeadingSaturation {
            get {
                return Instance == null ? 1f : _Instance._HeadingSaturation;
            }
            set {
                if (Instance == null) return;
                _Instance._HeadingSaturation = value;
            }
        }

        public static int PopupWidth {
            get {
                return Instance == null ? 400 : _Instance._PopupWidth;
            }
            set {
                if (Instance == null) return;
                _Instance._PopupWidth = value;
            }
        }

        public static void GUI()
        {
            AxonGUI.Setup(200);
            AxonGUI.Heading("Advanced Presets Global Config");
            GUI_Settings();
        }

        public static void GUI_Settings()
        {
            if (Instance == null) {
                AxonGUI.SetTooltip("Assign an Advanced Presets Config asset.");
                _Instance = (AdvancedPresetsGlobalConfig)AxonGUI.FieldObject(null, "Configuration", Instance, typeof(AdvancedPresetsGlobalConfig), false);
            }

            if (Instance == null) {
                AxonGUI.HelpBox("Please assign an Advanced Presets Config asset.", MessageType.Warning);
            }
            else {
                AxonGUI.Heading("Global Config");
                AxonGUI.BeginBoxPadded();
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("This setting is only applicable when applying a preset with children. The child objects in the target hierarchy can either be matched " +
                    "on their sibling index (order in the hierarchy) or by exact name match. Matching by index provides greater versatility, however using name matching can " +
                    "be more precise to target the right child objects. You may consider using matching by name if your hierarchy doesn't match up exactly or has additional " +
                    "objects you want to leave unaffected.");
                MatchMode = (MatchModes)AxonGUI.FieldEnumPopupInline(null, "Match Mode", MatchMode);

                AdvancedPresetsWindow.MinifiedRowBreak();
                AxonGUI.SetTooltip("If enabled, if matching children are not found then new objects will be instantiated. Otherwise they are skipped. This is only applicable when " +
                    "applying presets with children.");
                CanAddChildren = AxonGUI.FieldToggleInline(null, "Add Children", CanAddChildren);

                AxonGUI.EndHorizontal();

                //AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("When instantiating a new object this option unlinks the reference to the source prefab. " +
                    "This is recommended to avoid accidentally making changes to the source prefabs. However, if you wish to preserve prefab linkage you may disable this option.");
                UnpackPrefabs = AxonGUI.FieldToggleInline(null, "Unpack Prefabs", UnpackPrefabs);

                AdvancedPresetsWindow.MinifiedRowBreak();
                AxonGUI.SetTooltip("With this option enabled, the Collections menu is only displayed if the project contains more than 1 collection.");
                AutoHideCollections = AxonGUI.FieldToggleInline(null, "Auto Hide Collections", AutoHideCollections);

                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("If enabled, the target game object name is updated when a preset is applied.");
                CanRenameObjects = AxonGUI.FieldToggleInline(null, "Rename Objects", CanRenameObjects);

                AdvancedPresetsWindow.MinifiedRowBreak();
                AxonGUI.SetTooltip("If enabled, the target object track and/or channel color is set by the preset color.");
                CanSetTrackColors = AxonGUI.FieldToggleInline(null, "Set Track Colors", CanSetTrackColors);

                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Use this to adjust the display color of preset folders and groups.");
                ShowColoredHeadings = AxonGUI.FieldToggleInline(null, "Colored Headings", ShowColoredHeadings);
                if (ShowColoredHeadings) {
                    HeadingSaturation = AxonGUI.FieldSliderInline(null, "", HeadingSaturation, 0f, 1f);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("This adjusts the color of preset buttons.");
                ShowColoredButtons = AxonGUI.FieldToggleInline(null, "Colored Buttons", ShowColoredButtons);
                if (ShowColoredButtons) {
                    ButtonSaturation = AxonGUI.FieldSliderInline(null, "", ButtonSaturation, 0f, 1f);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AdvancedPresetsWindow.MinifiedRowBreak();
                AxonGUI.SetTooltip("Sets the width of the Advanced Presets popup menu when clicking the presets button in the Timeflow view switches panel.");
                PopupWidth = AxonGUI.FieldIntInline(null, "Popup Size", PopupWidth);
                AxonGUI.EndHorizontal();

                //AxonGUI.EndBox();
                AxonGUI.EndBoxPadded();

                Instance.Layout.Object = Instance;
                Instance.Layout.GUI(false);
            }
            if (UnityEngine.GUI.changed) {
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
            }
        }

        #endregion

        public enum MatchModes
        {
            MatchByName,
            MatchBySiblingIndex
        }

        [SerializeField] private MatchModes _MatchMode = MatchModes.MatchBySiblingIndex;
        [SerializeField] private bool _AutoHideCollections = true;
        [SerializeField] private bool _CanSetTrackColors = true;
        [SerializeField] private bool _CanRenameObjects = true;
        [SerializeField] private bool _CanAddChildren = true;
        [SerializeField] private bool _UnpackPrefabs = true;
        [SerializeField] private bool _ShowColoredHeadings = true;
        [SerializeField] private bool _ShowColoredButtons = true;
        [SerializeField] private float _ButtonSaturation = 1f;
        [SerializeField] private float _HeadingSaturation = 1f;
        [SerializeField] private int _PopupWidth = 400;

        [SerializeField] protected AdvancedPresetsLayout _Layout = null;
        [SerializeField] public AdvancedPreset _EditPreset = null;

        public AdvancedPresetsLayout Layout {
            get {
                if (_Layout == null) {
                    _Layout = new AdvancedPresetsLayout();
                }
                return _Layout;
            }
        }
    }

}

#endif