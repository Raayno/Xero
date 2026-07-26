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
    public partial class TrackColorPalette : ScriptableObject
    {
        public static readonly string SettingsAssetGUID = "f82d13a353279ad4aa06d9765812b343";
        public static readonly string DefaultAssetGUID = "a7c14a2b4e8174a48b32b8ae7c8ae488";

        //[UnityEditor.MenuItem("Assets/Timeflow/Create Track Color Palette")]
        public static TrackColorPalette NewAsset()
        {
            TrackColorPalette newAsset = ScriptableObject.CreateInstance<TrackColorPalette>();

            // Check if the settings directory exists
            string settingsPath = AssetDatabase.GUIDToAssetPath(SettingsAssetGUID);
            if (!AssetDatabase.IsValidFolder(settingsPath)) {
                // Cannot create new asset in non-existant directory so return a new temporary asset
                return newAsset;
            }

            string assetPath = settingsPath + "/NewTrackColors.asset";
            TrackColorPalette p = AssetDatabase.LoadAssetAtPath<TrackColorPalette>(assetPath);
            int i = 1;
            while (p != null) {
                assetPath = settingsPath + $"/NewTrackColors{i}.asset";
                p = AssetDatabase.LoadAssetAtPath<TrackColorPalette>(assetPath);
                i++;
            }

            AssetDatabase.CreateAsset(newAsset, assetPath);
            AssetDatabase.SaveAssets();

            TimeflowPreferences.Current.TrackColors = newAsset;

            // Select the newly created asset
            EditorUtility.FocusProjectWindow();
            Debug.Log("New TrackColorPalette asset created.", newAsset);//--KEEP
            return newAsset;
        }

        public static TrackColorPalette CreateOrFindAsset()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(DefaultAssetGUID);
            TrackColorPalette existingAsset = AssetDatabase.LoadAssetAtPath<TrackColorPalette>(assetPath);

            // Check if the settings directory exists
            string settingsPath = AssetDatabase.GUIDToAssetPath(SettingsAssetGUID);
            if (!AssetDatabase.IsValidFolder(settingsPath)) {
                // Cannot create new asset in non-existant directory so return a new temporary asset
                TrackColorPalette newAsset = ScriptableObject.CreateInstance<TrackColorPalette>();
                return null;
            }

            if (existingAsset != null) {
                // If the asset exists, select it in the Project window
                EditorUtility.FocusProjectWindow();
                return existingAsset;
            }
            else {
                // If the asset does not exist, create a new one
                TrackColorPalette newAsset = ScriptableObject.CreateInstance<TrackColorPalette>();
                AssetDatabase.CreateAsset(newAsset, assetPath);
                AssetDatabase.SaveAssets();

                // Select the newly created asset
                EditorUtility.FocusProjectWindow();
                Debug.Log("New TrackColorPalette asset created.", newAsset);//--KEEP
                return newAsset;
            }
        }

        public static void RevealAsset(TrackColorPalette palette)
        {
            if (palette == null) palette = CreateOrFindAsset();
            string assetPath = AssetDatabase.GetAssetPath(palette);
            if (!string.IsNullOrEmpty(assetPath)) {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                SelectionUtil.Select(asset);
                EditorGUIUtility.PingObject(asset);
            }
            else {
                Debug.LogWarning("TrackColorPalette asset not found.");
            }
        }

        public static void UpdateTrackColor(TimeflowObject obj)
        {
            if (TimeflowPreferences.Current.TrackColors != null) {
                TimeflowPreferences.Current.TrackColors.AutoAssignColor(obj);
            }
        }

        public static void UpdateChannelColor(TimeflowChannel ch)
        {
            if (TimeflowPreferences.Current.TrackColors != null) {
                TimeflowPreferences.Current.TrackColors.AutoAssignColor(ch);
            }
        }

        public static bool IsAutomaticColor {
            get {
                if (TimeflowPreferences.Current.TrackColors != null) {
                    return TimeflowPreferences.Current.TrackColors.IsAutomatic;
                }
                return false;
            }
        }

        public static bool IsAutomaticColorForced {
            get {
                if (TimeflowPreferences.Current.TrackColors != null) {
                    return TimeflowPreferences.Current.TrackColors.IsAutomaticForced;
                }
                return false;
            }
        }

        public static bool GlobalColorAdjustment {
            get {
                if (TimeflowPreferences.Current.TrackColors != null) {
                    return TimeflowPreferences.Current.TrackColors.EnableColorAdjustment;
                }
                return false;
            }
        }

        public static int GlobalHue {
            get {
                if (TimeflowPreferences.Current.TrackColors != null) {
                    return TimeflowPreferences.Current.TrackColors.Hue;
                }
                return 0;
            }
        }

        public static int GlobalSaturation {
            get {
                if (TimeflowPreferences.Current.TrackColors != null) {
                    return TimeflowPreferences.Current.TrackColors.Saturation;
                }
                return 0;
            }
        }

        public static int GlobalLightness {
            get {
                if (TimeflowPreferences.Current.TrackColors != null) {
                    return TimeflowPreferences.Current.TrackColors.Lightness;
                }
                return 0;
            }
        }

    }
}
#endif
