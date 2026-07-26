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
    /// <summary>
    /// This defines a collection of images and other resources used by the editor UI in AxonGenesis
    /// windows and views. 
    /// </summary>
    sealed public partial class AxonColor
    {
        public enum Schemes
        {
            Randomize,
            RandomHue,
            RandomSequence,
            Rainbow,
            Warms,
            Cools,
            Greens,
            Golds,
            Tropical,
            Neutral,
            Interpolate,
            TrackColorsSequential,
            TrackColorsRandom
        }

        public static int TrackColorsCount = 15;

        public static Color InterpolateStartColor = Color.blue;
        public static Color InterpolateEndColor = Color.red;

        #region GENERAL COLORS  

        public static readonly Color
            Default = Color.white,
            Red = Color.red,
            Orange = new Color(1f, 0.5f, 0f),
            LightYellow = new Color(1f, 1f, 0.3f),
            Yellow = Color.yellow,
            Green = Color.green,
            Blue = Color.blue,
            Purple = new Color(0.5f, 0f, 0.5f),
            Indigo = new Color(0.29f, 0f, 0.51f),
            DarkGrey = new Color(0.25f, 0.25f, 0.25f),
            DarkerGrey = new Color(0.1f, 0.1f, 0.1f),
            DarkerGreyFaded = new Color(0.1f, 0.1f, 0.1f, 0.5f),
            Transition = new Color(0f, 0f, 0f, 0.5f),
            TransitionFaded = new Color(0f, 0f, 0f, 0.2f),
            MediumGrey = new Color(0.5f, 0.5f, 0.5f),
            LightGrey = new Color(0.75f, 0.75f, 0.75f),
            White = Color.white,
            Cyan = Color.cyan,
            Aquamarine = new Color(0.5f, 1f, 0.83f),
            Magenta = Color.magenta,
            LightGreen = new Color(0.56f, 0.93f, 0.56f),
            DarkGreen = new Color(0f, 0.5f, 0f),
            Lavender = new Color(0.9f, 0.9f, 0.98f),
            Invisible = new Color(0, 0, 0, 0),
            Black = new Color(0, 0, 0, 1);

        #endregion

        #region BRANDING & STATUS COLORS  

        public static Color
            Warning = new Color(1f, 0.7f, 0.2f, 1),
            Error = new Color(1f, 0.2f, 0, 1),
            BrandRed = ColorUtil.NewColor(191, 41, 0),
            Prefab = ColorUtil.NewColor(129, 180, 255),
            TimeflowDefault = ColorUtil.NewColor(0, 126, 255),
            RedDark = ColorUtil.NewColor(150, 0, 0),
            Recording = new Color(1f, 0.338101f, 0.179f, 1f);

        #endregion

        #region UI TEXT & OVERLAY COLORS  

        public static Color
            BlackText = ColorUtil.NewColor(20, 20, 20),
            BoldText = ColorUtil.NewColor(20, 20, 20),
            LightText = ColorUtil.NewColor(90, 90, 90),
            SoftWhite = new Color(0.9f, 0.9f, 0.9f, 1f),
            Timecode = new Color(1f, 1f, 1f, 0.8f),
            Active = new Color(1, 1, 1, 1),
            Inactive = new Color(0.4f, 0.4f, 0.4f, 1),
            Selected = new Color(1f, 1f, 0f, 1),
            SelectedFaded = new Color(1f, 1f, 0f, 0.5f),
            SelectedText = new Color(1f, 1f, 0.8f, 1),
            DimField = new Color(0.8f, 0.8f, 0.8f, 0.8f),
            Gradient = new Color(1f, 1f, 1f, 0.75f);

        #endregion

        #region LABELS & GHOSTING  

        public static Color
            Label = new Color(0.4f, 0.4f, 0.6f),
            LabelSelected = new Color(1, 0, 0, 1),
            LabelDrag = new Color(1, 0.8f, 0, 1),
            TrackGraphLabel = new Color(1, 1f, 1f, 0.7f),
            Faded = new Color(1f, 1f, 1f, 0.3f),
            ExtraFaded = new Color(1f, 1f, 1f, 0.1f),
            Ghost = new Color(0.25f, 0.25f, 0.25f, 0.2f),
            KeyframeGhost = new Color(1f, 1f, 1f, 0.2f);

        #endregion

        #region DRAG & INTERACTION STATES  

        public static Color
            Solo = new Color(1f, 1f, 1f, 1f),
            DragOver = new Color(1, 1, 1, 0.75f),
            DragAccept = new Color(0, 1, 1, 1),
            DragAcceptChild = new Color(0, 1, 0.25f, 1),
            DragNone = new Color(0.5f, 0.5f, 0.5f, 0.9f),
            ManualOverride = new Color(1, 1, 0, 1),
            EditingOverride = new Color(1, 0, 0, 1);

        #endregion

        #region CHANNEL & RENDER STATES  

        public static Color
            LimitValueLine = new Color(0.5f, 0.5f, 0.5f, 1),
            MidiNoteOn = new Color(0, 1, 0, 1),
            RenderPending = new Color(0, 0, 1, 1),
            RenderProgress = new Color(0, 1, 0, 1),
            RenderEncoding = new Color(1, 1, 0, 1),
            ChannelHeightLocked = new Color(0f, 0f, 0f, 0.25f),
            LockedUnderlay = new Color(0f, 0f, 0f, 0.75f),
            LockedOverlay = new Color(1f, 1f, 1f, 0.95f),
            RedChannel = new Color(1, 0, 0, 1),
            GreenChannel = new Color(0, 1, 0, 1),
            BlueChannel = new Color(0, 0, 1, 1),
            AlphaChannel = new Color(1, 1, 1, 1);

        #endregion

        #region TRACK COLORS  

        public static Color
            TrackBase = new Color(0.5f, 0.5f, 0.5f, 1),
            TrackWhite = new Color(1, 1, 1, 1),
            TrackBlack = new Color(0, 0, 0, 1),
            VScrollbar = new Color(0, 0, 0, 0.25f),
            TrackWatermelon = new Color(1f, 0f, 0.22f),
            TrackRed = new Color(1, 0, 0, 1),
            TrackSunset = new Color(1, 0.14f, 0.0f),
            TrackOrange = new Color(1f, 0.3f, 0f),
            TrackPeach = new Color(1, 0.6f, 0.16f),
            TrackGreen = ColorUtil.NewColor(134, 228, 8),
            TrackLightGreen = ColorUtil.NewColor(183, 255, 108),
            TrackDarkGreen = ColorUtil.NewColor(73, 127, 0),
            TrackTeal = ColorUtil.NewColor(0, 127, 115),
            TrackBlue = ColorUtil.NewColor(36, 168, 255),
            TrackDarkBlue = ColorUtil.NewColor(0, 86, 127),
            TrackLightBlue = ColorUtil.NewColor(147, 211, 255),
            TrackPurple = ColorUtil.NewColor(140, 36, 255),
            TrackLavender = ColorUtil.NewColor(158, 119, 255),
            TrackPink = new Color(1f, 0.27f, 0.77f),
            TrackPartialSelection = new Color(0f, 0f, 0f, 0.25f);

        #endregion

        #region TIMEFLOW VIEW  

        public static Color
            TimeLine = new Color(1, 0.3f, 0.2f, 0.5f),
            TimeDisplayField = new Color(0, 0, 0, 1f),
            Separator = new Color(0.5f, 0.5f, 0.5f, 0.1f),
            SeparatorVertical = new Color(0f, 0f, 0f, 0.25f),
            VScrollbarHandle = new Color(0.5f, 0.5f, 0.5f, 1f),
            GraphModeDarken = new Color(0f, 0f, 0f, 0.5f),
            GridLineMajor = new Color(1, 1, 1, 0.5f),
            GridLineMinor = new Color(1, 1, 1, 0.125f),
            GridLineSubMinor = new Color(1, 1, 1, 0.065f),
            TimeMarker = new Color(0, 0, 0, 0.25f),
            TimeMarkerSelected = new Color(1, 1, 0, 0.75f),
            TimeScope = new Color(1, 0.7f, 0, 1f),
            ChannelLinkValid = new Color(1, 1, 1, 1),
            ChannelLinkInvalid = new Color(0.8f, 0, 0, 1),
            KeySelected = new Color(1, 0.95f, 0, 1),
            AlignToolsOff = new Color(1f, 1f, 1f, 0.4f),
            AlignToolsOn = Color.white,
            RelatedKeys = new Color(0, 1, 0, 1),
            ReplaceKeys = new Color(1, 1, 1, 1),
            KeyTangents = new Color(0, 1, 0, 1),
            KeyTangents2 = new Color(0, 1, 1, 1),
            KeyTangents3 = new Color(1, 1, 0, 1),
            KeyAutoTangents = new Color(0, 1, 1, 1);

        #endregion

        #region TROPICAL THEME

        public static Color
            TropicalOcean = new Color(0f, 0.75f, 0.85f),         // Vibrant aqua blue
            CoralPink = new Color(1f, 0.5f, 0.5f),               // Soft, warm coral
            MangoOrange = new Color(1f, 0.65f, 0.1f),            // Ripe mango
            PalmLeaf = new Color(0.13f, 0.55f, 0.13f),           // Deep tropical green
            HibiscusRed = new Color(0.85f, 0.1f, 0.3f),          // Bright flower red
            SandTan = new Color(0.94f, 0.87f, 0.73f);            // Light beach sand

        #endregion

        public static void Load()
        {
            if (EditorGUIUtility.isProSkin) {
                BrandRed = ColorUtil.NewColor(241, 52, 0);
                RedDark = ColorUtil.NewColor(200, 0, 0);
                BoldText = ColorUtil.NewColor(220, 220, 220);
                LightText = ColorUtil.NewColor(120, 120, 120);
                Active = new Color(1f, 1f, 1f, 1);
                Inactive = new Color(0.5f, 0.5f, 0.5f, 1);
                TimeMarker = new Color(0, 0, 0, 1);
                Label = new Color(1, 1, 1, 0.25f);
                LabelSelected = new Color(1, 1, 1, 1);

                // TIMEFLOW VIEW
                TimeLine = ColorUtil.NewColor(241, 52, 40);
                Separator = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                GridLineMajor = new Color(1, 1, 1, 0.1f);
                GridLineMinor = new Color(1, 1, 1, 0.08f);
                GridLineSubMinor = new Color(1, 1, 1, 0.04f);
            }
            else {
                Timecode = BlackText;
            }

        }

        /// <summary>
        /// Returns the shading color for an object depending on whether it is active and if Unity Pro is
        /// being used.
        /// </summary>
        public static Color ActiveState(bool isActive)
        {
            return isActive ? Active : Inactive;
        }

        public static Color GetLegacyTrackColor(int colorIndex)
        {
            int i = MathUtil.Loop(colorIndex, 1, TrackColorsCount);
            Color color = TrackWatermelon;

            if (i == 2) {
                color = TrackRed;
            }
            else
            if (i == 3) {
                color = TrackSunset;
            }
            else
            if (i == 4) {
                color = TrackOrange;
            }
            else
            if (i == 5) {
                color = TrackPeach;
            }
            else
            if (i == 6) {
                color = TrackGreen;
            }
            else
            if (i == 7) {
                color = TrackLightGreen;
            }
            else
            if (i == 8) {
                color = TrackDarkGreen;
            }
            else
            if (i == 9) {
                color = TrackTeal;
            }
            else
            if (i == 10) {
                color = TrackBlue;
            }
            else
            if (i == 11) {
                color = TrackDarkBlue;
            }
            else
            if (i == 12) {
                color = TrackLightBlue;
            }
            else
            if (i == 13) {
                color = TrackPurple;
            }
            else
            if (i == 14) {
                color = TrackLavender;
            }
            else
            if (i == 15) {
                color = TrackPink;
            }

            return color;
        }

        public static string GetTrackColorName(int colorIndex)
        {
            int i = MathUtil.Loop(colorIndex, 1, TrackColorsCount);
            string color = "Watermelon";

            if (i == 2) {
                color = "Red";
            }
            else
            if (i == 3) {
                color = "Sunset";
            }
            else
            if (i == 4) {
                color = "Orange";
            }
            else
            if (i == 5) {
                color = "Peach";
            }
            else
            if (i == 6) {
                color = "Green";
            }
            else
            if (i == 7) {
                color = "Light Green";
            }
            else
            if (i == 8) {
                color = "Dark Green";
            }
            else
            if (i == 9) {
                color = "Teal";
            }
            else
            if (i == 10) {
                color = "Blue";
            }
            else
            if (i == 11) {
                color = "Dark Blue";
            }
            else
            if (i == 12) {
                color = "Light Blue";
            }
            else
            if (i == 13) {
                color = "Purple";
            }
            else
            if (i == 14) {
                color = "Lavender";
            }
            else
            if (i == 15) {
                color = "Pink";
            }

            return color;
        }

        public static void ColorSchemeMenu(int count, Action<int, Color> callback)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Randomize"), false, () => ApplyColorScheme(Schemes.Randomize, count, callback));
            menu.AddItem(new GUIContent("Random Hue"), false, () => ApplyColorScheme(Schemes.RandomHue, count, callback));
            menu.AddItem(new GUIContent("Random Sequence"), false, () => ApplyColorScheme(Schemes.RandomSequence, count, callback));
            menu.AddItem(new GUIContent("Rainbow"), false, () => ApplyColorScheme(Schemes.Rainbow, count, callback));
            menu.AddItem(new GUIContent("Warms"), false, () => ApplyColorScheme(Schemes.Warms, count, callback));
            menu.AddItem(new GUIContent("Cools"), false, () => ApplyColorScheme(Schemes.Cools, count, callback));
            menu.AddItem(new GUIContent("Greens"), false, () => ApplyColorScheme(Schemes.Greens, count, callback));
            menu.AddItem(new GUIContent("Golds"), false, () => ApplyColorScheme(Schemes.Golds, count, callback));
            menu.AddItem(new GUIContent("Tropical"), false, () => ApplyColorScheme(Schemes.Tropical, count, callback));
            menu.AddItem(new GUIContent("Neutral"), false, () => ApplyColorScheme(Schemes.Neutral, count, callback));
            menu.AddItem(new GUIContent("Interpolate"), false, () => ApplyColorScheme(Schemes.Interpolate, count, callback));
            menu.AddItem(new GUIContent("Track Colors/Random"), false, () => ApplyColorScheme(Schemes.TrackColorsRandom, count, callback));
            menu.AddItem(new GUIContent("Track Colors/Sequential"), false, () => ApplyColorScheme(Schemes.TrackColorsSequential, count, callback));

            menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0));
        }

        public static void ApplyColorScheme(Schemes scheme, int count, Action<int, Color> callback)
        {
            if (scheme == Schemes.Interpolate) {
                for (int i = 0; i < count; i++) {
                    Color color = Color.Lerp(InterpolateStartColor, InterpolateEndColor, i / (float)(count - 1));
                    callback?.Invoke(i, color);
                }
                return;
            }
            if (scheme == Schemes.Randomize) {
                for (int i = 0; i < count; i++) {
                    Color color = ColorUtil.Random();
                    callback?.Invoke(i, color);
                }
                return;
            }
            if (scheme == Schemes.RandomHue) {
                float h = MathUtil.Random();
                float s = 0.9f;
                float v = 1f;
                for (int i = 0; i < count; i++) {
                    Color color = ColorUtil.HLSColor(h, s, v);
                    h += 0.5f + MathUtil.Random() * 0.1f; // Randomize hue slightly for each color
                    if (h > 1) h -= 1f;
                    callback?.Invoke(i, color);
                }
                return;
            }
            if (scheme == Schemes.RandomSequence) {
                float h = MathUtil.Random();
                float s = 0.9f;
                float v = 1f;
                for (int i = 0; i < count; i++) {
                    Color color = ColorUtil.HLSColor(h, s, v);
                    h += MathUtil.Random() * 0.1f; // Randomize hue slightly for each color
                    callback?.Invoke(i, color);
                }
                return;
            }
            if (scheme == Schemes.Neutral) {
                for (int i = 0; i < count; i++) {
                    Color color = ColorUtil.Random();
                    color = ColorUtil.SetSaturation(color, 0.2f);
                    callback?.Invoke(i, color);
                }
                return;
            }
            if (scheme == Schemes.TrackColorsRandom) {
                for (int i = 0; i < count; i++) {
                    Color color = TimeflowPreferences.Current.TrackColors.GetRandomColor();
                    callback?.Invoke(i, color);
                }
                return;
            }
            if (scheme == Schemes.TrackColorsSequential) {
                for (int i = 0; i < count; i++) {
                    Color color = TimeflowPreferences.Current.TrackColors.GetColor(i);
                    callback?.Invoke(i, color);
                }
                return;
            }
            switch (scheme) {
                case Schemes.Rainbow:
                    ApplyGradient(count, callback, Red, Orange, Yellow, Green, Blue, Purple);
                    break;
                case Schemes.Warms:
                    ApplyGradient(count, callback, Red, Orange, Yellow);
                    break;
                case Schemes.Cools:
                    ApplyGradient(count, callback, Cyan, Blue, Lavender);
                    break;
                case Schemes.Greens:
                    ApplyGradient(count, callback, Aquamarine, LightGreen, DarkGreen);
                    break;
                case Schemes.Golds:
                    ApplyGradient(count, callback, LightYellow, Yellow, Orange);
                    break;
                case Schemes.Tropical:
                    ApplyGradient(count, callback, TropicalOcean, CoralPink, MangoOrange, PalmLeaf, HibiscusRed, SandTan);
                    break;
                case Schemes.Neutral:
                    ApplySolidColor(count, callback, Color.gray);
                    break;
            }
        }

        public static void ApplyGradient(int count, Action<int, Color> callback, params Color[] colors)
        {
            if (count < 1 || colors == null || colors.Length < 2) return;

            int segmentCount = colors.Length - 1;
            int presetsPerSegment = Mathf.CeilToInt((float)count / segmentCount);

            for (int i = 0; i < count; i++) {
                int segmentIndex = Mathf.FloorToInt((float)i / presetsPerSegment);
                float t = (float)(i % presetsPerSegment) / presetsPerSegment;
                callback?.Invoke(i, Color.Lerp(colors[segmentIndex], colors[segmentIndex + 1], t));
            }
        }

        public static void ApplySolidColor(int count, Action<int, Color> callback, Color color)
        {
            if (count < 1) return;

            for (int i = 0; i < count; i++) {
                callback?.Invoke(i, color);
            }
        }

    }

}//AxonGenesis

#endif