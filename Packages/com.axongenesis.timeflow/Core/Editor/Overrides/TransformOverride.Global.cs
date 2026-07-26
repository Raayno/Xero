// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

#if TIMEFLOW_OVERRIDES_DISABLED
//public abstract class TransformOverride {}
#else

namespace AxonGenesis
{
    public partial class TransformOverride : Editor
    {
        public static TransformOverride Instance { get; private set; }

        private const int _labelWidth = 50;
        private const int _labelWidthShort = 25;
        private const float _split1value = 500;
        private const float _split2value = 800;

        private static TransformOverrideData _SharedData = null;
        private static PivotRotation PivotMode = PivotRotation.Local;
        private static bool HasCopiedValue = false;
        private static bool ShowDialogChannelConflict = true;
        private static bool ShowDialogCombineChannels = true;
        private static float CopiedValue = 0f;
        private static bool HasAnyConflicts = false;

        private static GUIStyle _SelectedStyle = null;
        private static GUIStyle _DeselectedStyle = null;
        private static GUIStyle _UncolorizedStyle = null;

        private static bool ShowInColor {
            get { return TimeflowPreferences.Current.TransformOverride.ShowInColor; }
            set {
                TimeflowPreferences.Current.TransformOverride.ShowInColor = value;
            }
        }

        private static bool IsLocal => Tools.pivotRotation == PivotRotation.Local;

        private static bool ShowResetCopyPaste {
            get {
                return TimeflowPreferences.Current.ShowResetCopyPaste;
            }
            set {
                TimeflowPreferences.Current.ShowResetCopyPaste = value;
            }
        }

        private static bool ShortLabels {
            get {
                return TimeflowPreferences.Current.TransformOverrideShortLabels;
            }
            set {
                TimeflowPreferences.Current.TransformOverrideShortLabels = value;
            }
        }

        public static void UpdateGroupAutoKeyframing(TransformOverrideGroup group)
        {
            // When working with mixed channel setups this makes it so only the active
            // mode (combined or separated) allows auto keyframing on its respective
            // channel(s). Otherwise multiple channels may record the same keyframes.
            if (group.XYZ != null) group.XYZ.EnableAutoKeyframing = group.ShowCombined;
            if (group.X != null) group.X.EnableAutoKeyframing = !group.ShowCombined;
            if (group.Y != null) group.Y.EnableAutoKeyframing = !group.ShowCombined;
            if (group.Z != null) group.Z.EnableAutoKeyframing = !group.ShowCombined;
        }

        private static GUIStyle SelectedStyle {
            get {
                if (_SelectedStyle == null) {
                    _SelectedStyle = new GUIStyle(AxonUI.SubObjectSelectedStyle);
                }
                SetStyleSettings(_SelectedStyle);
                return _SelectedStyle;
            }
        }

        private static GUIStyle DeselectedStyle {
            get {
                if (_DeselectedStyle == null) {
                    _DeselectedStyle = new GUIStyle(AxonUI.SubObjectDeselectedStyle);
                }
                SetStyleSettings(_DeselectedStyle);
                return _DeselectedStyle;
            }
        }

        private static GUIStyle UncolorizedStyle {
            get {
                if (_UncolorizedStyle == null) {
                    _UncolorizedStyle = new GUIStyle(AxonUI.SubObjectStyle);
                }
                SetStyleSettings(_UncolorizedStyle);
                return _UncolorizedStyle;
            }
        }

        private static void SetStyleSettings(GUIStyle style)
        {
            style.margin = new RectOffset(0, 0, -2, 0);
            style.padding = new RectOffset(0, 0, 2, 2);
            style.fixedHeight = 0; // Allow flexible height
        }
    }
}
#endif
#endif
