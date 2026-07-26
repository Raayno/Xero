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
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    /// <summary>
    /// Defines editor-specific implementation
    /// </summary>
    public partial class TimeflowChannel : SerializableObject
    {
        private const int _switchesIconSize = 16;
        private const int _switchesIconLeftPad = 2;
        private const int _switchesIconTopPad = 2;
        private const int _switchesGraphPaintIndex = 132;

        private const float _prevNextKeyframePadTop = 8f;
        private const float _prevNextKeyframeLeftOffset = 54f;
        private const int _toggleKeyframePadLeft = 12;
        private const int _hierarchyNamePadRight = 60;

        // Sets minimum tangent value that can be exported.
        // AnimationCurve cannot deal with vertical tangents
        // so this enforces a max steepness.
        private const float _MinKeyframeTangentExport = 0.001f;
        private const float _MaxKeyframeSlopeExport = 100f;

        #region STATIC

        public static List<Keyframe> CopiedKeys = null;
        public static List<Keyframe> CopiedTracks = null;

        #endregion

        #region PUBLIC

        [SerializeField, FormerlySerializedAs("DisplayChannel")]
        private bool _DisplayChannel = true;

        [SerializeField, FormerlySerializedAs("DisplayChannelSolo")]
        private bool _DisplayChannelSolo;

        public bool EditorShowChannel;
        public bool EditorShowKeys;
        public bool ShowTangents = true;
        public bool GraphFloatValueOnly;
        public bool ExportEveryFrame = true;

        // This is intentionally public so that other scripts may directly set guicolor without influence of the white filter
        public Color _GUIColor = new Color(0, 0, 0, 0);
        public Color GUITextColor = new Color(0, 0, 0, 1f);

        [NonSerialized]
        public bool RecalculateGUIColor = false;

        [SerializeField]
        private Color _GUIColorComputed = new Color(0, 0, 0, 0);

        [SerializeField]
        private bool _GUIColorAdustment = false;

        [SerializeField]
        private int _GUIColorGlobalHue = 0;

        [SerializeField]
        private int _GUIColorGlobalSaturation = 0;

        [SerializeField]
        private int _GUIColorGlobalLightness = 0;

        [SerializeField]
        private float _GraphMinValue;

        [SerializeField]
        private float _GraphMaxValue = 100f;

        public Color GUIHandles = Color.red;
        public bool GUICanDraw = true;
        public bool DrawPath;

        public bool GUIHeightLocked = false;

        [FormerlySerializedAs("GUIHeightOffset")]
        [SerializeField] private int _GUIHeightOffset = 0;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public GUIRect GUIRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUISelectRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public bool GUICull = false;

        [NonSerialized]
        public GUIRect GUIExpandRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUITrackRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUILinkRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIColorRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIControlsRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public bool WasSelected = false;

        [NonSerialized]
        public GUIRect LoopInDragRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect LoopOutDragRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public bool LastKnownSet = false;

        [NonSerialized]
        public float LastKnownTime = 0f;

        [NonSerialized]
        public float LastKnownValue = 0f;

        [NonSerialized]
        public Color LastKnownColor = Color.black;

        [NonSerialized]
        public Vector4 LastKnownVector = Vector4.zero;

        [NonSerialized]
        public string LastKnownString = null;

        [NonSerialized]
        public UnityEngine.Component LastKnownComponent = null;

        [NonSerialized]
        public GameObject LastKnownGameObject = null;

        [NonSerialized]
        public UnityEngine.Object LastKnownObject = null;

        [NonSerialized]
        public Action<Keyframe> NotifyOnKeyValueChanged = null;

        /// <summary>
        /// The sort order actually displayed based on its position vertically in the view. This value can
        /// change any time the view changes.
        /// </summary>
        [NonSerialized]
        public int SortOrderInView = 0;

        [NonSerialized]
        public bool IsGraphLockedOverride;

        #endregion

        #region PUBLIC SERIALIZED

        [SerializeField]
        public bool IsHiddenInGraph;

        [SerializeField]
        private bool _IsHidden;

        [SerializeField]
        private bool _IsLocked;

        [SerializeField]
        private bool IsTimeLocked;

        [SerializeField]
        private bool _IsGraphLocked;

        #endregion

        #region PRIVATE

        [NonSerialized]
        protected bool wasEditingName;

        [NonSerialized]
        private bool isSelected;

        [NonSerialized]
        private bool isEditingName;

        [SerializeField]
        private bool _GUIColorAuto = false;

        [NonSerialized]
        private string _tempEditName = "";

        [NonSerialized]
        private bool isDragChannelLoopOut;

        [NonSerialized]
        private float dragStartLoopLength;

        [NonSerialized]
        private Vector2 dragStart = Vector2.zero;

        [NonSerialized]
        private Vector2 dragOffset = Vector2.zero;

        [NonSerialized]
        private GUIRect channelLinkButtonRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        private bool dragTangentStarted;

        [NonSerialized]
        private GUIRect switchesLockedRect;

        [NonSerialized]
        private GUIRect switchesGraphRect;

        [NonSerialized]
        private GUIRect switchesEnableRect;

        [NonSerialized]
        private GUIRect switchesDisplayRect;

        [NonSerialized]
        private GUIRect switchesColorRect;

        [NonSerialized]
        private GUIRect switchesPresetRect;

        [NonSerialized]
        private GUIRect nameRect;

        [NonSerialized]
        private GUIRect prevKeyframeButtonRect;

        [NonSerialized]
        private GUIRect nextKeyframeButtonRect;

        [NonSerialized]
        private GUIRect toggleKeyframeButtonRect;

        [NonSerialized]
        private GUIStyle _Style;

        [NonSerialized]
        private GUIStyle _LabelStyle;

        [NonSerialized]
        private int _LastKnownSetCount = 0;

        #endregion

        #region ACCESSORS

        private string tempEditName {
            get {
                return _tempEditName;
            }
            set {
                if (_tempEditName != value) {
                    _tempEditName = value;
                }
            }
        }

        public int LastKnownSetCount {
            get { return _LastKnownSetCount; }
            set {
                _LastKnownSetCount = value;
            }
        }

        public bool DisplayChannel {
            get {
                return DisplayChannelSolo || _DisplayChannel;
            }
            set {
                if (_DisplayChannel != value) {
                    _DisplayChannel = value;
                }
            }
        }

        public bool DisplayChannelSolo {
            get {
                return _DisplayChannelSolo;
            }
            set {
                if (_DisplayChannelSolo != value) {
                    _DisplayChannelSolo = value;
                    if (value && Object != null) Object.DisplaySolo = true;
                }
            }
        }

        public bool IsEditingName {
            get {
                return isEditingName;
            }
            set {
                isEditingName = value;
            }
        }


        public bool IsSearchDisplayed;

        public bool IsDisplayed {
            get {
                if (Timeflow.Active == null || Timeflow.Active.View == null) return false;
                bool isDisplayed = false;
                if (Object != null) {
                    if (Object.IsDisplayed) {
                        isDisplayed = true;
                        if (Timeflow.Active.View.Display.UnlockedOnly) {
                            isDisplayed = !IsLocked;
                        }
                        if (isDisplayed && Timeflow.Active.View.Display.EnabledOnly) {
                            isDisplayed = IsEnabled || IsTrack;
                        }
                        if (!IsTrack) {
                            // Display Channel Modes
                            // 0 = Show everything (no filter)
                            // 1 = Allow channels to be hidden per object and per channel
                            // 2 = Show objects only and force hide all channels
                            // 3 = Soloed objects only
                            if (Timeflow.Active.View.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Solo) {
                                isDisplayed = DisplayChannelSolo;
                            }
                            else
                            if (Timeflow.Active.View.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Objects) {
                                isDisplayed = false;
                            }
                            else
                            if (Timeflow.Active.View.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Displayed) {
                                if (!DisplayChannel || !Object.DisplayChannels) {
                                    isDisplayed = false;
                                }
                            }

                            if (Timeflow.Active.View.Display.IsSearching) {
                                isDisplayed = IsSearchDisplayed;
                            }
                        }
                    }
                }
                return isDisplayed;
            }
        }

        public bool IsSelectable {
            get {
                if (IsParentCollapsed) return false;
                return IsDisplayed;
            }
        }

        public bool IsParentCollapsed => Object == null ? false : Object.IsParentCollapsed;

        public virtual string ChannelLinkName {
            get {
                if (ToProperty != null) {
                    return ToProperty.Name;
                }
                return null;
            }
        }

        public float GraphMinValue {
            get { return _GraphMinValue; }
            set {
                if (_GraphMinValue != value) {
                    _GraphMinValue = value;
                }
            }
        }

        public float GraphMaxValue {
            get { return _GraphMaxValue; }
            set {
                if (_GraphMaxValue != value) {
                    _GraphMaxValue = value;
                }
            }
        }

        public Color GUIColor {
            get {
                _UpdateGUIColor();
                return MathUtil.Multiply(_GUIColorComputed, GUIColorWhite);
            }
            set {
                if (_GUIColor != value) {
                    _GUIColor = value;
                    RecalculateGUIColor = true; // force to recalculate computed color
                    GUIColorAuto = false;
                    //Debug.Log($"<color=cyan>{Name}.GUIColor:</color>{_GUIColor} {Behavior.name}");
                }
            }
        }

        private void _UpdateGUIColor()
        {
            if (_GUIColor.a == 0) {
                NewGUIColor();
            }
            if (RecalculateGUIColor ||
                _GUIColorAdustment != TrackColorPalette.GlobalColorAdjustment ||
                _GUIColorGlobalHue != TrackColorPalette.GlobalHue ||
                _GUIColorGlobalSaturation != TrackColorPalette.GlobalSaturation ||
                _GUIColorGlobalLightness != TrackColorPalette.GlobalLightness ||
                _GUIColorComputed.a < 1f) {

                RecalculateGUIColor = false;

                _GUIColorAdustment = TrackColorPalette.GlobalColorAdjustment;
                _GUIColorGlobalHue = TrackColorPalette.GlobalHue;
                _GUIColorGlobalSaturation = TrackColorPalette.GlobalSaturation;
                _GUIColorGlobalLightness = TrackColorPalette.GlobalLightness;

                if (!_GUIColorAdustment || (_GUIColorGlobalSaturation == 0 && _GUIColorGlobalLightness == 0 && _GUIColorGlobalHue == 0)) {
                    // Don't apply any adjustments
                    _GUIColorComputed = _GUIColor;
                }
                else {
                    float hue, sat, val;
                    Color.RGBToHSV(_GUIColor, out hue, out sat, out val);

                    float h = (float)_GUIColorGlobalHue / 100f;
                    hue += h;
                    if (hue < 0) hue += 1f;
                    else
                    if (hue > 1f) hue -= 1f;

                    float s = (float)_GUIColorGlobalSaturation / 100f;
                    if (_GUIColorGlobalSaturation < 0) {
                        s += 1f;
                        sat = MathUtil.Interpolate(0, sat, s);
                    }
                    else {
                        sat = MathUtil.Interpolate(sat, 1f, s);
                    }

                    float v = (float)_GUIColorGlobalLightness / 100f;
                    if (_GUIColorGlobalLightness < 0) {
                        v += 1f;
                        val = MathUtil.Interpolate(0, val, v);
                    }
                    else {
                        val = MathUtil.Interpolate(val, 1f, v);
                    }

                    _GUIColorComputed = Color.HSVToRGB(hue, sat, val);
                }
                _GUIColorComputed.a = 1f;
            }
        }

        public bool GUIColorAuto {
            get {
                if (TrackColorPalette.IsAutomaticColorForced) return true;
                return _GUIColorAuto;
            }
            set {
                _GUIColorAuto = value;
                //Debug.Log($"_GUIColorAuto:{_GUIColorAuto}");
            }
        }

        public Color GUIColorWhite {
            get {
                if (Object != null) {
                    bool locked = IsLocked || Object.IsLocked;
                    return locked ? AxonColor.LockedOverlay : AxonColor.Default;
                }
                else {
                    return AxonColor.Default;
                }
            }
        }

        public int GUIHeight {
            get {
                int height = GUIHeightOffset + TimeflowPreferences.Current.DefaultChannelHeight;
                if (height < TimeflowPreferences.ChannelMinHeight) height = TimeflowPreferences.ChannelMinHeight;
                else
                if (height > TimeflowPreferences.ChannelMaxHeight) height = TimeflowPreferences.ChannelMaxHeight;
                return height;
            }
            set {
                GUIHeightOffset = Mathf.Max(0, value - TimeflowPreferences.Current.DefaultChannelHeight);
            }
        }

        #endregion

        #region EDITOR UTILS

        public virtual void ResetName()
        {
            if (HasProperty) {
                ToProperty.DisplayName = null; // force to regenerate
                _Name = ToProperty.DisplayName;
            }
            else _Name = null;
            IsNameCustom = false;
        }

        private void ValidateHeightOffset()
        {
            if (TimeflowPreferences.Current == null) return;
            int dif = TimeflowPreferences.Current.DefaultChannelHeight - TimeflowPreferences.ChannelMinHeight;
            if (_GUIHeightOffset < -dif) _GUIHeightOffset = -dif;
        }

        /// <summary>
        /// This is called after a property type and/or attribute has been changed. This converts a single
        /// attribute key value into a vector key value based on the original attribute it represented.
        /// This is an attempt to preserve expected keyframe data when a user animates a single attribute
        /// but then decides to change it to a combined or uniform value.
        /// </summary>
        public virtual void OnPropertyChanged(Property.PropertyTypes originalType, int originalAttribute)
        {
            //Debug.Log($"OnPropertyChanged:{Name} IsNameCustom:{IsNameCustom} {originalAttribute} {PropertyType} {Attribute}");
            string propName = ToProperty.GetNameAndAttribute(null, true, true, false);
            if (IsNameCustom) {
                IsNameCustom = Name != propName;
            }
            if (!IsNameCustom && ((originalType != PropertyType || originalAttribute != Attribute) || !propName.Equals(Name))) {
                ResetName();
            }

            if (ToProperty != null) {
                bool uniform = IsUniformValue;
                if (Keys != null && Keys.Count > 0) {
                    foreach (Keyframe k in Keys) {
                        if (k.PropertyType != KeyPropertyType) {
                            if (!Property.HasMultipleAttributes(k.PropertyType) && Property.HasMultipleAttributes(KeyPropertyType)) {
                                if (uniform) {
                                    k.KeyVector = new Vector4(k.KeyValue, k.KeyValue, k.KeyValue, k.KeyValue);
                                }
                                else {
                                    if (originalAttribute <= 0) {
                                        k._KeyVector = new Vector4(k.KeyValue, 0f, 0f, 0f);
                                    }
                                    else
                                    if (originalAttribute == 1) {
                                        k._KeyVector = new Vector4(0f, k.KeyValue, 0f, 0f);
                                    }
                                    else
                                    if (originalAttribute == 2) {
                                        k._KeyVector = new Vector4(0f, 0f, k.KeyValue, 0f);
                                    }
                                    else
                                    if (originalAttribute == 3) {
                                        k._KeyVector = new Vector4(0f, 0f, 0f, k.KeyValue);
                                    }
                                }
                            }
                            k.PropertyType = KeyPropertyType;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Override to implement any behavior updates after keyframe values have been modified
        /// </summary>
        /// <param name="key">The keyframe that was changed.</param>
        public virtual void OnKeyValueChanged(Keyframe key)
        {
            if (NotifyOnKeyValueChanged != null) {
                NotifyOnKeyValueChanged.Invoke(key);
            }
        }

        public virtual void AutoKeyframingDetect()
        {
            if (!CanAddRemoveKeys || !SupportsKeyframes || IsLocked || !EnableAutoKeyframing) return;
            if (HasValueChanged(CurrentTime)) {
                UndoUtil.Undo(Behavior, $"Auto Keyframe {Name} at {CurrentTime}", true);
                SetKey(CurrentTime);
            }
        }

        #endregion

        #region GUI

        public virtual Texture2D Icon => Behavior != null ? Behavior.Icon : AxonUI.Icons.TrackEmpty;

        private GUIStyle _IconStyle = null;

        public GUIStyle IconStyle {
            get {
                if (_IconStyle == null) {
                    _IconStyle = AxonUI.TextureButtonStyle;
                    _IconStyle.alignment = TextAnchor.MiddleCenter;
                    _IconStyle.stretchWidth = true;
                    _IconStyle.stretchHeight = true;
                }
                _IconStyle.normal.background = Icon;
                _IconStyle.active.background = Icon;
                return _IconStyle;
            }
        }

        public int GUIHeightOffset {
            get {
                return _GUIHeightOffset;
            }
            set {
                if (GUIHeightLocked) return;
                if (_GUIHeightOffset != value) {
                    _GUIHeightOffset = value;
                    ValidateHeightOffset();
                }
            }
        }

        public virtual bool HitTest(Vector2 pos)
        {
            return pos.y > GUIRect.y && pos.y < GUIRect.yMax;
        }

        public virtual bool SelectTest(Vector2 pos)
        {
            return GUISelectRect.Contains(pos);
        }

        public virtual void Select(bool clearSelection = false)
        {
            if (Timeflow.Active == null || Timeflow.Active.View == null) return;
            Timeflow.Active.View.SelectChannel(this, clearSelection);
        }

        public void StartEditingName()
        {
            if (!IsEditingName) {
                IsEditingName = true;
                wasEditingName = false;
                tempEditName = Name;
            }
        }

        public void StopEditingName(bool commit = true)
        {
            if (IsEditingName) {
                IsEditingName = false;
                wasEditingName = false;

                if (commit) {
                    if (Behavior != null) UndoUtil.Undo(Behavior, "Edit Channel Name");
                    Name = tempEditName;
                }
            }
        }

        /// <summary>
        /// Override to implement additional selection behavior when a keyframe is selected.
        /// </summary>
        public virtual void OnKeySelected(Keyframe key) { }

        public void GotoPrevKeyframe()
        {
            GotoPrevKeyframe(CurrentTime);
        }

        public void GotoPrevKeyframe(float fromTime)
        {
            Keyframe prev = GetPrevKey(fromTime);
            if (prev != null) {
                Timeflow.Active.View.SelectKeyClear(prev, true);
                Timeflow.Active.CurrentTimeExact = prev.KeyTimeWorld;
                Timeflow.Active.View.ScrollCenter();
            }
        }

        public void GotoNextKeyframe()
        {
            GotoNextKeyframe(CurrentTime);
        }

        public void GotoNextKeyframe(float fromTime)
        {
            Keyframe next = GetNextKey(fromTime);
            if (next != null) {
                Timeflow.Active.View.SelectKeyClear(next, true);
                Timeflow.Active.CurrentTimeExact = next.KeyTimeWorld;
                Timeflow.Active.View.ScrollCenter();
            }
        }

        public virtual void OnDragStart() { }

        public virtual void OnDragUpdate() { }

        public virtual void OnDragEnded() { }

        public virtual void OnDragCancel() { }

        public virtual void GUIHierarchySwitchesLayout()
        {
            if (Behavior == null) return;
            float yOffset = GUIRect.y;

            float y = GUIRect.y + (GUIRect.height * 0.5f - 8f) - 2f;
            switchesLockedRect = new GUIRect(_switchesIconLeftPad, y + _switchesIconTopPad, _switchesIconSize, _switchesIconSize);

            switchesGraphRect = switchesLockedRect;
            switchesGraphRect.x += _switchesIconSize + 1;

            switchesEnableRect = switchesGraphRect;
            switchesEnableRect.x += _switchesIconSize + 1;

            switchesDisplayRect = switchesEnableRect;
            switchesDisplayRect.x += _switchesIconSize + 1;

            switchesColorRect = switchesDisplayRect;
            switchesColorRect.x += _switchesIconSize + 1;

            switchesPresetRect = switchesColorRect;
            switchesPresetRect.x += _switchesIconSize + 1;

            GUIColorRect = switchesColorRect;
            GUIColorRect.y += 2;
            GUIColorRect.x += 2;
            GUIColorRect.width = GUIColorRect.height = _switchesIconSize - 4;
        }

        /// <summary>
        /// Presents the switches column for channels in the far left of the Timeflow window. This can be
        /// overridden for further customization.
        /// </summary>
        /// <param name="wantsDelete">This tracks whether the user pressed the delete button. The actual
        ///     deletion must be deferred otherwise it interferes with the iteration of the channels list.
        ///     </param>
        public virtual void GUIHierarchySwitches()
        {
            if (Behavior == null) return;
            if (EditorInput.IsLayout) GUIHierarchySwitchesLayout();

            if (IsSelected) {
                Rect selectedRect = GUIRect;
                selectedRect.width = 2;

                GUI.color = AxonColor.Selected;
                GUI.Box(selectedRect, GUIContent.none, AxonUI.SolidStyle);
            }

            GUI.color = Color.white;

            GUI.Box(switchesLockedRect, GUIContent.none, AxonUI.DarkBoxStyle);

            bool islock = IsLocked;
            islock = GUISwitchesLock(islock);

            GUI.color = GUIColorWhite;
            GUI.Box(switchesGraphRect, GUIContent.none, AxonUI.DarkBoxStyle);
            GUI.Box(switchesEnableRect, GUIContent.none, AxonUI.DarkBoxStyle);
            GUI.Box(switchesDisplayRect, GUIContent.none, AxonUI.DarkBoxStyle);
            GUI.Box(switchesColorRect, GUIContent.none, AxonUI.DarkBoxStyle);
            //GUI.Box(switchesPresetRect, GUIContent.none, AxonUI.DarkBoxStyle);

            bool isGraphOn = GUICanDraw;
            bool isGraphLock = IsGraphLocked;
            isGraphLock = GUISwitchesGraph(isGraphLock);

            GUIStyle enableTimeStyle = new GUIStyle(AxonUI.BehaviorOnStyle);
            if (!IsEnabled) {
                enableTimeStyle = new GUIStyle(AxonUI.BehaviorOffFadedStyle);
            }

            bool enabled = IsEnabled;
            enabled = GUISwitchesEnable(enableTimeStyle, enabled);
            enabled = DisplayChannel || DisplayChannelSolo;

            bool isAlt = Event.current != null && Event.current.alt;

            GUIStyle s = enabled ? AxonUI.DisplayChannelOnStyle : AxonUI.DisplayChannelOffStyle;
            if (DisplayChannelSolo || isAlt) {
                GUI.color = AxonColor.Solo;
                s = DisplayChannelSolo ? AxonUI.DisplayChannelSoloOnStyle : AxonUI.DisplayChannelSoloOffStyle;
            }
            else
            if (Timeflow.Active.View.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Solo) {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            if (Timeflow.Active.View.Display.ChannelMode == 0) {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            enabled = GUISwitchesDisplay(enabled, s);

            GUISwitchesColor();
            GUISwitchesPresets();

            GUI.color = AxonColor.Default;
        }

        private bool GUISwitchesDisplay(bool enabled, GUIStyle s)
        {
            if (Timeflow.Active.Input.Button(switchesDisplayRect, AxonUI.DisplayShowChannelsLabel, s, TimeflowViewInput.SwitchesDisplayPaintIndex, ref enabled)) {
                UndoUtil.Undo(this.Behavior, "Display Channel");
                if (EditorInput.IsAlt) {
                    enabled = true;
                    DisplayChannelSolo = !DisplayChannelSolo;
                    if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                        Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplayChannelSolo, EditorInput.IsShift, false);
                    }
                }
                else
                if (DisplayChannelSolo) {
                    DisplayChannelSolo = false;
                    DisplayChannel = true;
                    if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                        Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplayChannelSolo, EditorInput.IsShift, false);
                    }
                }
                else {
                    DisplayChannel = enabled;
                    if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                        Timeflow.Active.View.Display.DisplayChannelsOnSelectedObjects(DisplayChannel, EditorInput.IsShift);
                    }
                }

                Timeflow.Active.Input.InputHandled = true;
                Timeflow.Active.View.Display.ApplyFilter();
            }

            return enabled;
        }

        private bool GUISwitchesEnable(GUIStyle enableTimeStyle, bool enabled)
        {
            if (Timeflow.Active.Input.Button(switchesEnableRect, AxonUI.TimeModeLabel, enableTimeStyle, TimeflowViewInput.SwitchesEnablePaintIndex, ref enabled)) {
                if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                    Timeflow.Active.View.EnableSelectedChannels(enabled);
                }
                else {
                    UndoUtil.Undo(this.Behavior, "Set Channel Enabled");
                    if (!IsLocked) IsEnabled = enabled;
                }
                Timeflow.Active.Input.InputHandled = true;
                Timeflow.Active.View.Display.ApplyFilter();
            }

            return enabled;
        }

        private void GUISwitchesColor()
        {
            GUI.color = GUIColor;
            bool isColor = true;
            if (Timeflow.Active.Input.Button(GUIColorRect, AxonUI.TrackColorLabel, AxonUI.SolidStyle, 0, ref isColor)) {
                Timeflow.Active.Input.InputHandled = true;
                Timeflow.Active.Input.EventMode = TimeflowViewInput.EventModes.ColorSelect;
                Event.current.Use();
                TrackColorMenu.Init(this);
            }
            GUI.color = Color.white;
        }

        private void GUISwitchesPresets()
        {
            if (!Timeflow.Layout.ShowAdvancedPresets) return;

            bool hasPresets = AxonGUI.TypeHasPreset(Behavior.GetType());
            GUI.color = hasPresets ? Color.white : AxonColor.Faded;
            EditorGUI.BeginDisabledGroup(!hasPresets);
            if (GUI.Button(switchesPresetRect, GUIContent.none, AxonUI.PresetStyle)) {
                AxonGUI.PresetsMenuPopup(Behavior);
            }
            EditorGUI.EndDisabledGroup();
            GUI.color = Color.white;
        }

        private bool GUISwitchesLock(bool islock)
        {
            if (Timeflow.Active.Input.Button(switchesLockedRect, AxonUI.DisplayUnlockedOnlyLabel, IsLocked ? IsLockedSelf ? AxonUI.LockOnStyle : AxonUI.LockHalfStyle : AxonUI.LockOffFadedStyle, TimeflowViewInput.SwitchesLockPaintIndex, ref islock)) {
                if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                    Timeflow.Active.View.LockSelectedChannels(islock);
                }
                else {
                    Timeflow.Active.View.LockChannel(this, islock);
                }
                Timeflow.Active.Input.InputHandled = true;
                Timeflow.Active.View.Display.ApplyFilter();
            }

            return islock;
        }

        private bool GUISwitchesGraph(bool isGraphLock)
        {
            if (Timeflow.Active.Input.Button(switchesGraphRect, GUIContent.none, IsGraphLocked ? AxonUI.GraphLockedStyle : GUICanDraw ? AxonUI.GraphOnStyle : AxonUI.GraphOffStyle, _switchesGraphPaintIndex, ref isGraphLock)) {
                if (Event.current.shift) {
                    if (IsGraphLocked) {
                        IsGraphLocked = false;
                    }
                    else {
                        GUICanDraw = !GUICanDraw;
                    }

                }
                else {
                    if (GUICanDraw) {
                        IsGraphLocked = !IsGraphLocked;
                    }
                    GUICanDraw = true;
                }
                if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                    Timeflow.Active.View.CanDrawSelectedChannels(GUICanDraw, IsGraphLocked);
                }
                else {
                    Timeflow.Active.View.CanDrawChannel(this, GUICanDraw, IsGraphLocked);
                }
                Timeflow.Active.Input.InputHandled = true;
                Timeflow.Active.View.Display.ApplyFilter();
            }

            return isGraphLock;
        }

        /// <summary>
        /// Displays the channel label row in the hierarchy view. This can be overriden for custom labels.
        /// Also see GUIHierarchyControls for adding buttons or other custom controls.
        /// </summary>
        public virtual void GUIHierarchy()
        {
            if (Behavior == null) return;
            if (!IsHidden && IsSelectable) {
                if (EditorInput.IsLayout) {
                    GUIRect.height = GUIHeight;

                    nameRect = GUIRect;
                    nameRect.x = Timeflow.Active.Layout.Switches.Width + 25;
                    nameRect.width = Timeflow.Active.Layout.Hierarchy.Width - _hierarchyNamePadRight - nameRect.x;

                    GUIHierarchyControlsLayout();
                }

                if (_Style == null) _Style = AxonUI.TextureButtonStyle;
                if (_LabelStyle == null) _LabelStyle = new GUIStyle(GUI.skin.label);

                if (IsLocked) {
                    GUI.color = AxonColor.LockedUnderlay;
                    GUI.Box(GUIRect, GUIContent.none, GUI.skin.box);
                }

                if (GUIHeightLocked) {
                    GUI.color = AxonColor.ChannelHeightLocked;
                    Rect r = GUIRect;
                    r.y = GUIRect.y + GUIRect.height - 2;
                    r.height = 2;
                    GUI.Box(r, "", AxonUI.TrackEmptyStyle);
                }

                GUI.color = IsSelected ? IsLocked ? MathUtil.Interpolate(AxonColor.LockedUnderlay, GUIColor, 0.5f) : GUIColor : GUIColorWhite;
                _LabelStyle = IsSelected ? AxonUI.SubObjectSelectedStyle : AxonUI.SubObjectStyle;
                _LabelStyle.normal.textColor = AxonColor.ActiveState(Behavior.gameObject.activeInHierarchy);
                if (Timeflow.Active.View.IsDragChannel(this)) {
                    _LabelStyle.normal.textColor = AxonColor.LabelDrag;
                }
                _LabelStyle.alignment = TextAnchor.MiddleRight;

                if (IsEditingName) {
                    GUI.color = GUIColorWhite;
                    _LabelStyle = new GUIStyle(GUI.skin.textField);
                    _LabelStyle.alignment = TextAnchor.MiddleRight;

                    GUI.SetNextControlName("EditObjectName");
                    if (tempEditName == null) tempEditName = "";
                    nameRect.height = 20;
                    nameRect.y = GUIRect.y + ((GUIHeight / 2) - (nameRect.height / 2));
                    Timeflow.Active.tempEditRect = nameRect;
                    Timeflow.Active.tempEditContainingRect = Timeflow.Active.Layout.Hierarchy;
                    tempEditName = GUI.TextField(nameRect, tempEditName, AxonUI.ChannelTextFieldStyle);

                    if (!wasEditingName) {
                        wasEditingName = true;
                        AxonGUI.FocusControl("EditObjectName");
                        EditorGUI.FocusTextInControl("EditObjectName");
                    }
                }
                else {
                    GUI.Box(GUIRect, "", _LabelStyle);
                    AxonUI.ChannelLabelStyle.normal.textColor = _LabelStyle.normal.textColor;
                    if (!IsEnabled) {
                        AxonUI.ChannelLabelStyle.normal.textColor = AxonColor.Inactive;
                    }

                    GUI.color = GUIColorWhite;
                    GUI.Label(nameRect, new GUIContent(DisplayName), AxonUI.ChannelLabelStyle);
                }

                if (TimeflowPreferences.Current.ShowComponentIcons) {
                    Rect iconRect = nameRect;
                    if (Timeflow.Active.Layout.ShowSwitches && Timeflow.Active.Layout.Switches != null) {
                        iconRect.x = Timeflow.Active.Layout.Switches.Width + 3;
                    }
                    else {
                        iconRect.x = 1;
                    }
                    iconRect.width = iconRect.height = 16;
                    iconRect.y = GUIRect.y + ((GUIHeight / 2) - (iconRect.height / 2));

                    GUI.color = GUIColorWhite;
                    GUI.Box(iconRect, GUIContent.none, AxonUI.DarkBoxStyle);
                    GUI.Button(iconRect, GUIContent.none, IconStyle);
                }


                GUI.color = GUIColorWhite;
                GUIHierarchyControls();
            }
        }

        /// <summary>
        /// Handles the special control portion of the channel display in the hierarchy view. Override this
        /// to create your own special controls.
        /// </summary>
        public virtual void GUIHierarchyControlsLayout()
        {
            float yPos = GUIRect.y + (GUIRect.height * 0.5f - _prevNextKeyframePadTop);

            if (SupportsKeyframes) {
                float x = Timeflow.Active.Layout.Hierarchy.Width - _prevNextKeyframeLeftOffset;
                prevKeyframeButtonRect = new GUIRect(x, yPos, _switchesIconSize, _switchesIconSize);

                toggleKeyframeButtonRect = prevKeyframeButtonRect;
                toggleKeyframeButtonRect.x += _toggleKeyframePadLeft;

                nextKeyframeButtonRect = toggleKeyframeButtonRect;
                nextKeyframeButtonRect.x += _toggleKeyframePadLeft;

                GUIControlsRect = new GUIRect(prevKeyframeButtonRect.x, GUIRect.y,
                    Timeflow.Active.Layout.Hierarchy.Width - prevKeyframeButtonRect.x, GUIRect.height);
            }

            if (!IsTrack) GUIChannelLinkLayout();
            GUIExpandRegionLayout();
        }

        public virtual void GUIHierarchyControls()
        {
            EditorGUI.BeginDisabledGroup(IsLocked || Object.IsLocked || !IsEnabled);

            float currentTime = CurrentTime;

            bool hasKeys = Keys != null && Keys.Count > 0;
            Color color = GUIColorWhite;
            color.a = hasKeys ? 1f : 0.25f;
            GUI.color = color;

            if (SupportsKeyframes) {
                bool hasPrevKey = GetPrevKey(currentTime) != null;
                if (GUI.Button(prevKeyframeButtonRect, GUIContent.none, hasPrevKey ? AxonUI.PrevKeyStyle : AxonUI.PrevKeyNoneStyle)) {
                    GotoPrevKeyframe();
                    EditorInput.SetEventUsed();
                }
                GUI.color = GUIColorWhite;

                Keyframe k = GetKeyAtTime(currentTime);
                if (CanAddRemoveKeys) {
                    if (GUI.Button(toggleKeyframeButtonRect, GUIContent.none, k == null ? AxonUI.ToggleKeyOffStyle : AxonUI.ToggleKeyOnStyle)) {
                        GUIToggleKeyframe(currentTime, k);
                        EditorInput.SetEventUsed();
                    }
                }

                GUI.color = color;
                bool hasNextKey = GetNextKey(currentTime) != null;
                if (GUI.Button(nextKeyframeButtonRect, GUIContent.none, hasNextKey ? AxonUI.NextKeyStyle : AxonUI.NextKeyNoneStyle)) {
                    GotoNextKeyframe();
                    EditorInput.SetEventUsed();
                }
                GUI.color = GUIColorWhite;
            }

            GUIChannelLink();
            GUIExpandRegion();
            EditorGUI.EndDisabledGroup();
        }

        private Keyframe GUIToggleKeyframe(float currentTime, Keyframe k)
        {
            if (CanAddRemoveKeys && !IsLocked) {
                k = _GUIToggleKeyframe(currentTime, this);
                int mode = k == null ? 2 : 1;
                if (EditorInput.IsAlt && IsSelected && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Contains(this)) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch == this || ch.IsTrack) continue; // already handled
                        k = _GUIToggleKeyframe(currentTime, ch, mode);
                    }
                }
                Timeflow.Active.View.SelectedKeysChanged();
            }

            return k;
        }

        private Keyframe _GUIToggleKeyframe(float currentTime, TimeflowChannel ch, int mode = 0)
        {
            //Debug.Log($"_GUIToggleKeyframe:{ch.Name} mode:{mode}");
            Keyframe k = ch.GetKeyAtTime(currentTime);
            if (k == null || mode == 1) {
                k = Timeflow.Active.View.AddKeyframeAtTime(ch, currentTime, true);
                Timeflow.Active.View.SelectKeyClear(k, false);
                Timeflow.Active.Input.EventMode = TimeflowViewInput.EventModes.InsertKey;
                return k;
            }
            if (k != null && (mode == 0 || mode == 2)) {
                UndoUtil.Undo(ch.Behavior, "Delete Key");
                ch.UnsetKey(k);
                if (Timeflow.Active.View.SelectedKeys != null) {
                    Timeflow.Active.View.SelectedKeys.Remove(k);
                }
                Timeflow.Active.View.SelectedKeysChanged();
                return null;
            }

            return k;
        }

        public virtual void GUIChannelLinkLayout()
        {
            float x = Timeflow.Active.Layout.Hierarchy.Width - 16f;
            float yPos = GUIRect.y + (GUIRect.height * 0.5f - 8f);
            channelLinkButtonRect = new GUIRect(x, yPos, 16, 16);
        }

        public virtual void GUIChannelLink()
        {
            GUIChannelLinkButton(channelLinkButtonRect);
        }

        public virtual void GUIChannelLinkButton(GUIRect buttonRect)
        {
            buttonRect.width = buttonRect.height = 16;
            bool hasLink = IsLinked;
            if (hasLink && EditorInput.IsControl) {
                if (TimeflowView.IsLinking) TimeflowView.StopLinking();
                if (GUI.Button(buttonRect, GUIContent.none, AxonUI.ChannelLinkRemoveStyle)) {
                    RemoveLink();
                }
            }
            else
            if (hasLink && EditorInput.IsAlt) {
                if (TimeflowView.IsLinking) TimeflowView.StopLinking();
                if (GUI.Button(buttonRect, GUIContent.none, AxonUI.ChannelLinkOnStyle)) {
                    IsLinkEnabled = !IsLinkEnabled;
                }
            }
            else {
                GUI.color = hasLink ? Link.GUIColor : GUIColor;
                if (hasLink && !Link.Enabled) {
                    GUI.color = Color.gray;
                }
                GUIStyle style = hasLink ? AxonUI.ChannelLinkOnStyle : AxonUI.ChannelLinkOffStyle;
                if (!CanLink) {
                    GUI.color = AxonColor.Faded;
                    style = AxonUI.ChannelLinkOffStyle;
                }
                else
                if (TimeflowView.IsLinking && TimeflowView.LinkReceiver == this) {
                    GUI.color = AxonColor.Default;
                    style = AxonUI.ChannelLinkOnStyle;
                }
                GUI.DrawTexture(buttonRect, style.normal.background);

                bool isEnabled = true;
                if (TimeflowView.IsLinking) {
                    if (TimeflowView.LinkReceiver != null) {
                        if (TimeflowView.LinkReceiver != this) {
                            isEnabled = TimeflowView.LinkReceiver.IsLinkable(this);
                        }
                        if (isEnabled) {
                            GUI.color = AxonColor.Default;
                            GUI.DrawTexture(buttonRect, AxonUI.ChannelLinkTargetStyle.normal.background);
                        }
                    }
                    else {
                        Debug.LogWarning("LinkReceiver is null");
                    }
                }
                else {
                    if (LinkedFrom != null && LinkedFrom.Count > 0) {
                        if (LinkedFrom.Count > 1) {
                            foreach (TimeflowChannel ch in LinkedFrom) {
                                if (ch != null && ch.IsLinkEnabled) {
                                    GUI.color = ch.Link.GUIColor;
                                    break;
                                }
                            }
                        }
                        else
                        if (LinkedFrom[0] == null || !LinkedFrom[0].IsLinked) {
                            GUI.color = AxonColor.Error;
                            LinkedFrom = null;
                        }
                        else {
                            GUI.color = LinkedFrom[0].Link.GUIColor;
                        }
                        GUI.DrawTexture(buttonRect, AxonUI.ChannelLinkTargetStyle.normal.background);
                        GUI.color = AxonColor.Default;
                    }
                }
                GUI.color = AxonColor.Default;
            }
        }

        public GUIRect GUIChannelValuesLinkMenu(GUIRect rect)
        {
            if (IsLinked) {
                rect.width = 20;

                Link.Mode = (TimeflowChannelLink.Modes)EditorGUI.EnumPopup(rect, Link.Mode);

                rect.x += rect.width;
                string label = Link.GetModeLabel();

                if (Link.Enabled && TimeflowChannelLink.DisplayWarnings && Link.Mode != TimeflowChannelLink.Modes.Overwrite) {

                    if (SupportsKeyframes && (Keys.Count == 0)) {
                        rect.width = 16;
                        rect.height = 16;
                        if (AxonGUI.ButtonTexture(rect, AxonUI.Icons.Warning, "Please set a keyframe on this channel to avoid runaway value calculations.", new RectOffset(0, 0, 0, 0), true)) {
                            Timeflow.Active.Stop();
                            string msg = "It is strongly advised to set at least 1 keyframe on any channel whose value is not being set first by another behavior. " +
                                "This can otherwise lead to runaway conditions using channel links on unkeyframed channels. This warning causes no harm and can be ignored if the channel is performing special calculations and no runaway conditions are occuring. Otherwise, please set 1 keyframe on the channel to define the default/base value for link calculations.";

                            int i = EditorUtility.DisplayDialogComplex("Title", msg, "Don't show again", "Fix", "Ok");
                            if (i == 0) {
                                TimeflowChannelLink.DisplayWarnings = false;
                            }
                            else
                            if (i == 1) {
                                SetKey(0f);
                            }
                        }
                        rect.x += rect.width;
                    }
                }
            }

            // Return a rect starting at the next empty space and the width of the remaining space
            int w = Timeflow.Active.Layout.Values.Width - 10;
            rect.width = w - rect.x;

            return rect;
        }

        public virtual void GUIChannelSelected(bool isTimeOffset = false)
        {
            if (IsSelected) {
                GUIRect full = GUIRect;
                full.x = 0;
                full.width = isTimeOffset ? Timeflow.Active.Layout.TimeOffset.Width : Timeflow.Active.Layout.Values.Width;
                GUI.color = IsSelected ? GUIColor : GUIColorWhite;
                GUI.Box(full, "", AxonUI.SubObjectSelectedStyle);
                GUI.color = GUIColorWhite;
            }
        }

        public virtual void GUIChannelValues()
        {
            GUI.color = GUIColorWhite;
            if (Object == null) {
                Debug.LogWarning("Channel missing parent object");
                return;
            }
            EditorGUI.BeginDisabledGroup(IsLocked || Object.IsLocked || !IsEnabled);
            float time = CurrentTime;
            int w = Timeflow.Active.Layout.Values.Width - 10;
            GUIRect rect = GUIRect;
            float y = GUIRect.y + (GUIRect.height * 0.5f - 8f);
            rect.x = 5;
            rect.y = (int)y;
            rect.width = w;
            rect.height = 16;

            EditorGUIUtility.labelWidth = 10;
            GUIChannelSelected();

            bool forceFloat = IsUniformValue || !IsCombinedValue || Attribute > -1;

            rect = GUIChannelValuesLinkMenu(rect);

            string label = IsLinked ? Link.GetModeLabel() : ":";
            bool changed = false;
            Keyframe key = null;
            if (IsTrack) {
            }
            else
            if (IsString) {
                string obj = GetCurrentString();
                string newObj = EditorGUI.TextField(rect, new GUIContent(label), obj);
                if (obj != newObj) {
                    changed = true;
                    key = SetKeyString(time, newObj);
                }
            }
            else
            if (IsGameObject) {
                GameObject obj = GetCurrentGameObject();
                GameObject newObj = (GameObject)EditorGUI.ObjectField(rect, new GUIContent(label), obj, typeof(GameObject), true);
                if (obj != newObj) {
                    changed = true;
                    key = SetKeyGameObject(time, newObj);
                }
            }
            else
            if (IsComponent) {
                Component obj = GetCurrentComponent();
                Component newObj = (Component)EditorGUI.ObjectField(rect, new GUIContent(label), obj, typeof(Component), true);
                if (obj != newObj) {
                    changed = true;
                    key = SetKeyComponent(time, newObj);
                }
            }
            else
            if (IsObject) {
                UnityEngine.Object obj = GetCurrentObject();
                UnityEngine.Object newObj = (UnityEngine.Object)EditorGUI.ObjectField(rect, new GUIContent(label), obj, typeof(UnityEngine.Object), true);
                if (obj != newObj) {
                    changed = true;
                    key = SetKeyObject(time, newObj);
                }
            }
            else
            if (IsColor && !forceFloat) {
                Color value = CurrentColor;
                Color newValue = EditorGUI.ColorField(rect, new GUIContent(label), value, true, true, true);
                if (MathUtil.IsKeyDifferent(value, newValue)) {
                    changed = true;
                    key = SetKeyColor(time, newValue);
                }
            }
            else
            if (IsVector2 && !forceFloat) {
                Vector2 value = GetCurrentVector();
                AxonGUI.UndoName = "Set Vector";
                Vector2 newValue = AxonGUI.FieldVector2Inline(null, rect, label, value);
                if (MathUtil.IsKeyDifferent(value, newValue)) {
                    changed = true;
                    key = SetKeyVector(time, newValue);
                }
            }
            else
            if (IsVector3 && !forceFloat) {
                Vector3 value = GetCurrentVector();
                AxonGUI.UndoName = "Set Vector";
                Vector3 newValue = AxonGUI.FieldVector3Inline(null, rect, label, value);
                if (MathUtil.IsKeyDifferent(value, newValue)) {
                    changed = true;
                    key = SetKeyVector(time, newValue);
                }
            }
            else
            if (IsVector4 && !forceFloat) {
                Vector4 value = GetCurrentVector();
                AxonGUI.UndoName = "Set Vector";
                Vector4 newValue = AxonGUI.FieldVector4Inline(null, rect, label, value);
                if (MathUtil.IsKeyDifferent(value, newValue)) {
                    changed = true;
                    key = SetKeyVector(time, newValue);
                }
            }
            else
            if (IsRect && !forceFloat) {
                Vector4 value = GetCurrentVector();
                AxonGUI.UndoName = "Set Vector as Rect";
                Vector4 newValue = AxonGUI.FieldVector4AsRectInline(null, rect, label, value);
                if (MathUtil.IsKeyDifferent(value, newValue)) {
                    changed = true;
                    key = SetKeyVector(time, newValue);
                }
            }
            else
            if (IsRectOffset && !forceFloat) {
                Vector4 value = GetCurrentVector();
                AxonGUI.UndoName = "Set Vector as Rect Offset";
                Vector4 newValue = AxonGUI.FieldVector4AsRectOffsetInline(null, rect, label, value);
                if (MathUtil.IsKeyDifferent(value, newValue)) {
                    changed = true;
                    value = newValue;
                    key = SetKeyVector(time, newValue);
                }
            }
            else
            if (IsLayerMask && !forceFloat) {
                int value = (int)GetCurrentValue();
                AxonGUI.UndoName = "Set Layer Mask";
                int newValue = AxonGUI.FieldLayerMaskInline(null, rect, label, value);
                if (value != newValue) {
                    changed = true;
                    value = newValue;
                    key = SetKeyValue(time, newValue);
                }
            }
            else
            if (IsEnum && !forceFloat) {
                int value = (int)GetCurrentValue();
                AxonGUI.UndoName = "Set Enum";
                string[] enumValues = GetEnumValues();
                if (enumValues == null) {
                    int newValue = EditorGUI.IntField(rect, label, value);
                    if (value != newValue) {
                        changed = true;
                        value = newValue;
                        key = SetKeyValue(time, newValue);
                    }
                }
                else {
                    int newValue = EditorGUI.Popup(rect, label, value, enumValues);
                    if (value != newValue) {
                        changed = true;
                        value = newValue;
                        key = SetKeyValue(time, newValue);
                    }

                }
            }
            else
            if (IsBool || ShowValue || ShowFloat || forceFloat) {
                if (IsBool && !forceFloat) {
                    float v = GetCurrentValue();
                    bool value = v > 0f;
                    bool newValue = EditorGUI.Toggle(rect, label, value);

                    if (value != newValue) {
                        changed = true;
                        if (IsSelected && Event.current.alt) {
                            foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                                ch.SetKeyValue(time, newValue ? 1f : 0f);
                            }
                        }
                        else {
                            key = SetKeyValue(time, newValue ? 1f : 0f);
                        }
                    }
                }
                else {
                    float value = GetCurrentValue();
                    float newValue = EditorGUI.FloatField(rect, label, value);
                    if (MathUtil.IsKeyDifferent(value, newValue)) {
                        changed = true;
                        if (IsSelected && Event.current.alt) {
                            foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                                ch.SetKeyValue(time, newValue);
                            }
                        }
                        else {
                            key = SetKeyValue(time, newValue);
                        }
                    }
                }
            }

            if (changed) {
                Timeflow.IsAutoKeyframingInvalidThisFrame = true;
                Behavior.UpdateTimeChannel(this);
            }
            if (key != null) {
                Timeflow.Active.View.SelectKeyClear(key, true);
                if (!IsSelected) {
                    /// commented out because it was causing object selection to change in some cases
                    //Timeflow.Active.SelectChannel(this, false);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        public virtual void GUITimeOffsetColumn()
        {
            GUIChannelSelected(true);

            float w = (float)Timeflow.Active.Layout.TimeOffset.Width / 2f;
            float y = GUIRect.y + (GUIRect.height * 0.5f - 8f);
            float checkboxWidth = 16f;
            Rect rect = new Rect(0, y, w, 20);

            rect.height = checkboxWidth;
            rect.width = checkboxWidth;

            if (GUI.Button(rect, new GUIContent("", "Lock or unlock the time controls for this channel"),
                IsTimeLocked ? AxonUI.LockOnStyle : AxonUI.LockOffStyle)) {
                IsTimeLocked = !IsTimeLocked;
            }

            EditorGUI.BeginDisabledGroup(IsTimeLocked);

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 13;

            rect.x += rect.width - 4;
            rect.width = w - checkboxWidth - 4f;

            float timeOffset = IsLinked ? Link.TimeOffset : TimeOffset;
            float offset = EditorGUI.FloatField(rect, ":", timeOffset);
            if (!Mathf.Approximately(offset, timeOffset)) {
                UndoUtil.Undo(Behavior, "Time Offset");
                if (IsLinked) Link.TimeOffset = offset;
                else TimeOffset = offset;
            }

            rect.x += rect.width + 4f;
            rect.width = checkboxWidth;
            rect.height = checkboxWidth;

            if (GUI.Button(rect, new GUIContent(""), AxonUI.TrackDragTimeOffsetOnStyle)) {
                // Placeholder for layout only. Do nothing
            }

            rect.x += rect.width + 4f;
            rect.width = 50f;
            float timeScale = EditorGUI.FloatField(rect, "*", TimeScale);
            if (!Mathf.Approximately(timeScale, TimeScale)) {
                UndoUtil.Undo(Behavior, "Time Scale");
                TimeScale = timeScale;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public virtual void GUIExpandRegionLayout()
        {
            if (IsHidden || !IsSelectable) return;
            GUIExpandRect.width = Timeflow.Active.Layout.Hierarchy.Width;
            GUIExpandRect.x = 0;
            GUIExpandRect.y = (IsTrack ? Object.GUIRect.y : GUIRect.y) + GUIHeight;
            GUIExpandRect.height = 5;
        }

        public virtual void GUIExpandRegion()
        {
            if (IsHidden || !IsSelectable) return;
            if (!GUIHeightLocked) {
                Rect r = new GUIRect(GUIExpandRect);
                r.x = r.width = r.width / 2;
                EditorGUIUtility.AddCursorRect(GUIExpandRect, MouseCursor.SplitResizeUpDown);
            }

            GUI.color = AxonColor.Separator;
            GUIRect rect = new GUIRect(GUIExpandRect);
            rect.height = 2;
            GUI.Box(rect, GUIContent.none, AxonUI.SolidStyle);
        }

        public virtual void GUIChannel()
        {
            if (!IsHidden) {
                int yOffset = GUIRect.y;

                GUI.color = IsSelected && !IsColor ? GUIColor : GUIColorWhite;
                GUI.color = MathUtil.Multiply(GUIColor, GUI.color);

                GUITrackRect = new GUIRect(0, yOffset, Timeflow.Active.Layout.TimeAreaInner.Width, GUIHeight);
                GUIStyle labelStyle = IsSelected || Object.IsSelected ? AxonUI.SubObjectSelectedStyle : AxonUI.TrackEmptyStyle;
                labelStyle.normal.textColor = AxonColor.BoldText;
                GUI.Box(GUITrackRect, GUIContent.none, labelStyle);

                if (IsLocked) {
                    GUI.color = AxonColor.LockedUnderlay;
                    GUI.Box(GUITrackRect, GUIContent.none, GUI.skin.box);
                }

                if (IsEnabled) {
                    GUI.color = GUIColorWhite;
                    if (IsColor && (GUICanDraw || IsGraphLocked)) {
                        GUIDrawColorGradient();
                    }
                }
            }
        }

        public virtual void GUITracks() { }

        public virtual void GUITracksShade(bool withParent)
        {
            if (!TimeflowPreferences.Current.EnableTrackShadows || Object == null || AlwaysUpdate) return;
            if (TimeflowWindow.IsDrawLimited) return; // reduce draw calls to improve editor performance

            float leftEdge = Timeflow.Active.Layout.TimeAreaInner.Left - Timeflow.Active.Layout.TimeAreaOuter.Left;

            // Shade tracks by parent tracks
            TimeflowObject obj = withParent ? Object.ParentObject : Object;

            if (obj is Timeflow tf) {
                if (tf.Parent == null) {
                    // Don't shade the root timeflow track
                    return;
                }
            }

            while (obj != null && obj.Track != null && obj.Track.Keys != null &&
                obj.Track.VisibilityMode != TimeflowTrack.VisibilityModes.OnSelfOnly &&
                Object.Track.VisibilityMode != TimeflowTrack.VisibilityModes.RendererIndependent) {
                float x = leftEdge;
                float y = GUIRect.y;
                float w = Timeflow.Active.Layout.TimeAreaInner.Width;
                float h = GUIRect.height;

                if (Timeflow.Active.View.IsGraphMode) {
                    y = 0;
                    h = Timeflow.Active.Layout.TimeAreaInner.Height;
                }
                float next = 0f;
                List<GUIRect> gaps = new List<GUIRect>();
                gaps.Add(new GUIRect(0, 0, 0, 0));

                bool isFirst = true;

                foreach (Keyframe t in obj.Track.Keys) {
                    if (t.IsKeyEnabled) {
                        float tx = Timeflow.Active.View.PositionOfTime(t.KeyTimeWorld, true);
                        if (isFirst) {
                            isFirst = false;
                            gaps[0] = new GUIRect(0, y, tx, h);
                        }
                        if (x > 0 && next < t.KeyTime) {
                            gaps.Add(new GUIRect(x, y, tx - x, h));
                        }
                        next = t.KeyValue;
                        //Debug.Log($"GUITracksShade:{Name} t.KeyTime:{t.KeyTime} t.KeyValue:{t.KeyValue} t.KeyEndTimeWorld:{t.KeyEndTimeWorld} t.KeyEndTimeWorld:{t.KeyEndTimeWorld} tx:{tx} x:{x} w:{w}", obj);
                        x = Timeflow.Active.View.PositionOfTime(t.KeyEndTimeWorld, true);
                    }
                }
                if (x < w) {
                    gaps.Add(new GUIRect(x, y, w - x, h));
                }

                if (gaps.Count > 0) {
                    GUI.color = TimeflowPreferences.Current.TrackShadowColor;
                    int j = 0;
                    foreach (GUIRect gap in gaps) {
                        //Debug.Log($"GUITracksShade:{Name} {j} gap:{gap}");
                        GUI.Box(gap, "", AxonUI.TrackShadowStyle);
                        j++;
                    }
                    GUI.color = AxonColor.Default;
                }

                if (obj == obj.ParentObject) break;
                obj = obj.ParentObject;
            }

        }

        public virtual GUIStyle GUIKeyframeStyle(Keyframe key, bool selected)
        {
            GUIStyle style = selected ? AxonUI.KeyframeSelectedStyle : AxonUI.KeyframeStyle;
            if (key != null) {
                style = key.GetGUIStyle(selected);
            }
            return style;
        }

        public virtual void GUIKeyframes()
        {
            if (!IsHidden) {
                GUI.color = GUIColorWhite;

                if (Keys != null) GUIKeyframesLooped();

                if (TimeflowPreferences.Current.DrawLinkedChannels && IsLinked) {
                    //Debug.Log($"GUIKeyframes:{Name} useWorld:{Link.UseWorldTime} Link.TimeOffsetWorld:{Link.TimeOffsetWorld} TimeOffset:{Link.TimeOffset}");
                    Link.Channel.GUIKeyframesDraw(true, Link.UseWorldTime ? -Link.TimeOffset : -(Link.TimeOffsetWorld - Link.Channel.TimeOffsetWorld), GUIRect);
                }
                if (Keys != null) GUIKeyframesDraw(false, 0f, GUIRect);
            }
        }

        protected virtual void GUIKeyframeLayout(Keyframe key, Keyframe nextKey)
        {
        }

        public virtual void GUIKeyframesDraw(bool isLink, float timeOffset, Rect channelGUIRect)
        {
            int i = 0;
            List<Keyframe> keys = Keys;

            if (_Style == null) _Style = AxonUI.TextureButtonStyle;

            bool isFaded = !IsEnabled || (!IsSelected && Timeflow.View.IsGraphMode && Timeflow.View.IsGraphSolo);
            float fadeAlpha = IsEnabled ? 0.25f : 0.1f;

            foreach (Keyframe k in keys) {
                i++;
                float keyTime = k.KeyTimeWorld - timeOffset;
                if (keyTime > Timeflow.Active.View.ViewEndTime || (!k.IsTrackStyle && keyTime < Timeflow.Active.View.ViewStartTime)) {
                    k.GUIRect.Clear();
                }
                else {
                    float y = channelGUIRect.y + (channelGUIRect.height * 0.5f - 8f);
                    Rect keyRect;
                    Rect labelRect;
                    keyRect = new GUIRect(Timeflow.Active.View.PositionOfTime(keyTime, true) - 8, y, 16, 16);
                    labelRect = new GUIRect(keyRect.x + 16, keyRect.y - 2, 100, 20);

                    Keyframe nextKey = null;
                    if (i < keys.Count) {
                        nextKey = keys[i];
                        float nextKeyPos = Timeflow.Active.View.PositionOfTime(nextKey.KeyTimeWorld - timeOffset, true);
                        labelRect.width = nextKeyPos - labelRect.x;
                        if (k.IsTrackStyle) {
                            keyRect.width = nextKeyPos;
                        }
                    }
                    else {
                        if (k.IsTrackStyle) {
                            keyRect.width = Timeflow.Active.View.PositionOfTime(Timeflow.Active.View.ViewEndTime, true);
                        }
                    }
                    if (k.IsTrackStyle) {
                        keyRect.x += 8;
                        keyRect.y = channelGUIRect.y;
                        keyRect.height = GUIHeight - 2;
                    }

                    if (!isLink) {
                        // Only change the GUI rect when rendering the channel the key belongs to
                        k.GUIRect = keyRect;
                        k.GUILabelRect = labelRect;
                        GUIKeyframeLayout(k, nextKey);
                        keyRect = k.GUIRect;
                    }

                    bool isSelected = false;
                    bool isRelated = false;

                    if (!isLink && Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(k)) {
                        isSelected = true;
                        GUI.color = k.OverrideGUIColor ? k.GUIColor : GUIColor;// GUIColorWhite;
                        if (!k.IsKeyEnabled || !IsEnabled) {
                            GUI.color = Color.gray;// ColorUtil.SetAlpha(GUI.color, 0.5f);
                            _Style = GUIKeyframeStyle(k, false);
                        }
                        else {
                            _Style = GUIKeyframeStyle(k, false);
                        }
                    }
                    else
                    if (!isLink && TimeflowView.UseRelatedKeys && Timeflow.Active.View.RelatedKeys != null && Timeflow.Active.View.RelatedKeys.Contains(k)) {
                        if (!k.IsKeyEnabled || !IsEnabled) {
                            GUI.color = GUIColorWhite;
                            if (!IsEnabled) GUI.color = ColorUtil.SetAlpha(GUI.color, 0.2f);
                            _Style = GUIKeyframeStyle(k, false);
                        }
                        else {
                            GUI.color = AxonColor.RelatedKeys;
                            if (!IsEnabled) GUI.color = ColorUtil.SetAlpha(GUI.color, 0.2f);
                            _Style = GUIKeyframeStyle(k, false);
                            isRelated = true;
                        }
                    }
                    else {
                        GUI.color = GUIColor;
                        if (!k.IsKeyEnabled || !IsEnabled) {
                            GUI.color = AxonColor.Faded;
                        }
                        else
                        if (k.OverrideGUIColor) {
                            GUI.color = k.GUIColor;
                        }
                        else
                        if (IsColor && Attribute == -1) {
                            GUI.color = new Color(k.KeyColor.r, k.KeyColor.g, k.KeyColor.b, 1f);
                        }
                        if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);

                        _Style = GUIKeyframeStyle(k, false);
                    }

                    if (!isLink) {
                        if (isLink) GUI.color = ColorUtil.SetAlpha(GUI.color, 0.25f);
                        GUI.Box(keyRect, GUIContent.none, _Style);
                        if (isSelected) {
                            GUI.color = AxonColor.Selected;
                            GUI.Box(keyRect, GUIContent.none, GUIKeyframeStyle(k, true));
                        }
                        GUIDrawKeyframeLabel(k, keyRect, labelRect, k.ToString(), isFaded, isLink);
                    }
                    if (!isLink) {
                        if ((isSelected || isRelated) && Timeflow.Active.Input.IsDraggingCopy && Timeflow.Active.Input.DraggingTimeOffset != 0f) {
                            GUIRect r = new GUIRect(keyRect);
                            r.x = Timeflow.Active.View.PositionOfTime(keyTime + Timeflow.Active.Input.DraggingTimeOffset, true) - 8;
                            GUI.Box(r, GUIContent.none, GUIKeyframeStyle(Timeflow.Active.Input.DragPrimaryKey, true));
                        }

                        if (k.LockTime) {
                            GUI.color = Timeflow.Active.Input.IsDragging ? Color.white : AxonColor.ExtraFaded;
                            GUIRect lockRect = keyRect;
                            lockRect.x -= 8;
                            lockRect.width = lockRect.height = 16;
                            GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);
                        }

                        if ((Event.current.control || Event.current.command) && !Event.current.shift && !Event.current.alt) {
                            channelGUIRect.width = Timeflow.Active.Layout.TimeAreaInner.Width;
                            EditorGUIUtility.AddCursorRect(channelGUIRect, MouseCursor.ArrowPlus);
                        }
                    }
                }
            }
        }

        public virtual void GUIKeyframesLooped()
        {
            if (EnableLoop) {
                if (!TimeflowPreferences.Current.ShowLoopedKeyframes) return;
                //if(TimeflowWindow.IsDrawLimited) return;// reduce draw calls to improve editor performance

                GUI.color = AxonColor.KeyframeGhost * GUIColor;
                float timeOffset = TimeOffsetWorld;
                float loopStart = LoopStart + timeOffset;
                float loopEnd = LoopEnd + timeOffset;
                float loopDuration = LoopEnd - LoopStart;
                if (loopDuration > 0 && (EnableLoopIn || EnableLoopOut)) {
                    Timeflow tm = Timeflow.Active;
                    int loopIndex = EnableLoopIn ? -1 : 1;
                    bool forward = !EnableLoopIn;
                    bool pong = true;
                    bool outofview = false;
                    int keyCount = 0;
                    while (true) {
                        float loopOffset = (float)loopIndex * loopDuration;
                        float keyTime = 0;
                        foreach (Keyframe k in Keys) {
                            keyTime = k.KeyTimeWorld;
                            if (keyTime >= loopStart && keyTime <= loopEnd) {
                                keyCount++;
                                if (LoopPingPong && pong) {
                                    keyTime = (loopEnd - (keyTime - loopStart));
                                }
                                keyTime += loopOffset;
                                if (keyTime >= tm.View.ScrollTimeMin && keyTime <= tm.View.ScrollTimeMax) {
                                    int x = tm.View.PositionOfTime(keyTime, true) - 8;
                                    GUIRect rect = k.GUIRect;
                                    rect.x = x;

                                    GUIStyle style = GUIKeyframeStyle(k, false);
                                    GUI.Box(rect, GUIContent.none, style);
                                }
                                else {
                                    if (forward || !EnableLoopOut) {
                                        /// Outside of view - end operation
                                        outofview = true;
                                    }
                                    else {
                                        /// Show loop out keys
                                        forward = true;
                                        loopIndex = 0;
                                    }
                                    break;
                                }
                                if (TimeflowWindow.IsDrawLimited && keyCount > 10) return;// reduce draw calls to improve editor performance
                            }
                        }
                        pong = !pong;
                        if (forward) {
                            loopIndex++;
                        }
                        else {
                            loopIndex--;
                        }
                        if (outofview || keyCount == 0) break;
                        if (LoopLimit != 0 && loopIndex > LoopLimit) break;
                        if (loopIndex > 1000) {
                            //Debug.LogWarning("Too many keyframes displayed");
                            break; // failsafe to prevent crash in case loop doesn't end for some reason
                        }
                    }
                }
            }
        }

        // Add this field to the TimeflowChannel class (private, non-serialized)
        [NonSerialized]
        private Texture2D _cachedGradientTexture;
        [NonSerialized]
        private int _cachedGradientWidth = -1;

        public virtual void GUIDrawColorGradient()
        {
            if (!IsEnabled || Timeflow.Active.Layout.TimeAreaInner.Width < 100) return;
            //if (TimeflowWindow.IsDrawLimited) return;// reduce draw calls to improve editor performance
            bool debug = TimeflowPreferences.DebugEnabled;
            TimeflowPreferences.DebugEnabled = false;

            float yOffset = GUIRect.y;
            float end = Timeflow.Active.Layout.TimeAreaInner.Width;

            float padHeight = 12f;
            GUIRect r = new GUIRect(0, yOffset + (padHeight * 0.5f), end, GUIRect.height - padHeight);

            GUI.color = Color.black;
            GUI.Box(r, new GUIContent());
            GUI.color = AxonColor.Gradient;

            int sampleStep = 1; // width step in pixels
            int numSamples = Mathf.RoundToInt(Timeflow.Active.Layout.TimeAreaInner.Width / sampleStep);

            if (TimeflowWindow.IsDrawLimited) numSamples = numSamples / 4;// reduce draw calls to improve editor performance

            float startTime = Timeflow.Active.View.TimeOfPosition(0, true, false);
            float endTime = Timeflow.Active.View.TimeOfPosition(end, true, false);

            startTime *= TimeScaleWorld;
            endTime *= TimeScaleWorld;

            float timeStep = (endTime - startTime) / (float)numSamples;

            float t = startTime;
            Color32[] samples = new Color32[numSamples];
            for (int i = 0; i < numSamples; i++) {
                if (Attribute < 0) {
                    samples[i] = InterpolateColor(t, false, false);
                }
                else {
                    Color c = Color.black;
                    float v = InterpolateValue(t, false, false);
                    if (Attribute == 0) {
                        c.r = v;
                    }
                    else
                    if (Attribute == 1) {
                        c.g = v;
                    }
                    else
                    if (Attribute == 2) {
                        c.b = v;
                    }
                    else
                    if (Attribute == 3) {
                        c.r = c.g = c.b = 1f;
                        c.a = v;
                    }
                    samples[i] = c;
                }
                t += timeStep;
            }

            // Cache the gradient texture to avoid allocation every time
            if (_cachedGradientTexture == null || _cachedGradientWidth != numSamples) {
                if (_cachedGradientTexture != null) {
                    UnityEngine.Object.DestroyImmediate(_cachedGradientTexture);
                }
                _cachedGradientTexture = new Texture2D(numSamples, 1, TextureFormat.RGBA32, false);
                _cachedGradientTexture.filterMode = FilterMode.Bilinear;
                _cachedGradientTexture.wrapMode = TextureWrapMode.Clamp;
                _cachedGradientWidth = numSamples;
            }
            _cachedGradientTexture.SetPixels32(samples);
            _cachedGradientTexture.Apply();

            GUI.DrawTexture(r, _cachedGradientTexture, ScaleMode.StretchToFill, true);

            TimeflowPreferences.DebugEnabled = debug;
        }

        public virtual void GUIGraphPass1()
        {
            //Debug.Log($"<color=yellow>{Name}.GUIGraphPass1</color> {Time.time}");
            if (LimitValue && IsSelected) {
                float minLine = Timeflow.Active.View.PositionOfValue(MinValue.x, true);
                float maxLine = Timeflow.Active.View.PositionOfValue(MaxValue.x, true);

                Handles.color = AxonColor.LimitValueLine;
                Handles.DrawLine(new Vector2(0, minLine), new Vector2(Timeflow.Active.Layout.TimeAreaInner.Width, minLine));
                Handles.DrawLine(new Vector2(0, maxLine), new Vector2(Timeflow.Active.Layout.TimeAreaInner.Width, maxLine));
                Handles.color = AxonColor.Default;
            }

            if (EnableLoop) {
                GUIKeyframesLooped();

                GUI.color = IsEnabled ? GUIColor : AxonColor.Faded;
                if (!IsSelected) {
                    GUI.color = ColorUtil.SetAlpha(GUI.color, 0.5f);
                }
                float timeOffset = TimeOffsetWorld;
                float loopStart = LoopStart + timeOffset;
                float loopEnd = LoopEnd + timeOffset;

                float x = Timeflow.Active.View.PositionOfTime(loopStart, true);
                float w = Timeflow.Active.View.PositionOfTime(loopEnd, true) - x;

                x /= TimeScaleWorld;
                w /= TimeScaleWorld;

                GUIRect rect = new GUIRect(x, 0, w, Timeflow.Active.Layout.TimeAreaInner.Height);
                GUI.Box(rect, GUIContent.none, AxonUI.TrackLoopStyle);

                LoopInDragRect = new GUIRect(rect.x - 8f, Timeflow.Active.Layout.TimeAreaInner.Height / 2f, 16f, GUIHeight);
                LoopOutDragRect = new GUIRect(rect.x - 8f + rect.width, Timeflow.Active.Layout.TimeAreaInner.Height / 2f, 16f, GUIHeight);

                if (Event.current.shift) {
                    // Draw line to indicate loop length is locked
                    Handles.DrawLine(new Vector2(LoopInDragRect.x + 8f, LoopInDragRect.y + 8f), new Vector2(LoopOutDragRect.x + 8f, LoopOutDragRect.y + 8f));
                }

                GUI.Box(LoopInDragRect, GUIContent.none, AxonUI.LoopHandleStyle);
                GUI.Box(LoopOutDragRect, GUIContent.none, AxonUI.LoopHandleStyle);
            }
        }

        public virtual void GUIGraphPass2()
        {
            //Debug.Log($"<color=yellow>{Name}.GUIGraphPass2</color> {Time.time}");
            if (IsColor) {
                GUIDrawColorGradient();
            }
            bool hasKeys = Keys.Count > 0;
            if (IsTrack) {
                SortBy(TimeflowChannel.SortingModes.SizeDesc);
            }
            else {
                SortBy(TimeflowChannel.SortingModes.TimeAsc);
            }

            float yOffset = GUIRect.y;

            Vector3[] line = null;
            Vector3[] lineR = null;
            Vector3[] lineG = null;
            Vector3[] lineB = null;
            Vector3[] lineA = null;
            Vector3[] ghostline = null;
            Vector3[] ghostline1 = null;
            Vector3[] ghostline2 = null;
            Vector3[] ghostline3 = null;

            int i = 0;
            int first = -1;

            bool multiChannel = IsMultichannel && !IsUniformValue && IsCombinedValue;

            // Find the starting and ending indices for the keys in view
            if (hasKeys) {
                foreach (Keyframe key in Keys) {
                    float keyTime = key.KeyTimeWorld;
                    if (keyTime >= Timeflow.Active.View.ViewStartTime && keyTime <= Timeflow.Active.View.ViewEndTime) {
                        if (first == -1) first = i;
                    }
                    i++;
                }
                if (first > 0) {
                    // Start one keyframe left of offscreen so the line draws continuously
                    first--;
                }
            }
            int steps = Timeflow.Active.Layout.TimeAreaInner.Width / 4;

            line = new Vector3[steps];

            bool drawGhostLine = (IsLinkEnabled || EnableLoop) && IsSelected && !IsLocked;
            bool isFaded = !IsEnabled || (!IsSelected && Timeflow.View.IsGraphMode && Timeflow.View.IsGraphSolo);
            float fadeAlpha = IsEnabled ? 0.25f : 0.1f;

            bool ch0 = false;
            bool ch1 = false;
            bool ch2 = false;
            bool ch3 = false;
            int attrCount = AttributeCount;

            if (multiChannel) {
                ch0 = Timeflow.Active.View.ShowChannel0;
                ch1 = Timeflow.Active.View.ShowChannel1;
                ch2 = Timeflow.Active.View.ShowChannel2;
                ch3 = Timeflow.Active.View.ShowChannel3;

                if (attrCount < 3) {
                    ch2 = ch3 = false;
                }
                else
                if (attrCount < 4) {
                    ch3 = false;
                }
            }


            if (ch0) lineR = new Vector3[steps];
            if (ch1) lineG = new Vector3[steps];
            if (ch2) lineB = new Vector3[steps];
            if (ch3) lineA = new Vector3[steps];

            if (drawGhostLine) ghostline = new Vector3[steps];
            if (ch1 && drawGhostLine) ghostline1 = new Vector3[steps];
            if (ch2 && drawGhostLine) ghostline2 = new Vector3[steps];
            if (ch3 && drawGhostLine) ghostline3 = new Vector3[steps];

            if (IsLinked && TimeflowPreferences.Current.DrawLinkedChannels) {
                Link.Channel.GUIKeyframesDraw(true, Link.UseWorldTime ? -Link.TimeOffset : -(Link.TimeOffsetWorld - Link.Channel.TimeOffsetWorld), GUIRect);
            }

            float timeStep = (Timeflow.Active.View.ScrollTimeMax - Timeflow.Active.View.ScrollTimeMin) / (float)steps;
            float time = Timeflow.Active.View.ScrollTimeMin;

            for (i = 0; i < steps; i++) {
                float vx = Timeflow.Active.View.PositionOfTime(time, true);
                if (IsColor && multiChannel) {
                    Color tmp = InterpolateColor(time * TimeScaleWorld, false, false);

                    if (ch0) {
                        lineR[i].x = vx;
                        lineR[i].y = Timeflow.Active.View.PositionOfValue(tmp.r, true);
                    }
                    if (ch1) {
                        lineG[i].x = vx;
                        lineG[i].y = Timeflow.Active.View.PositionOfValue(tmp.g, true);
                    }
                    if (ch2) {
                        lineB[i].x = vx;
                        lineB[i].y = Timeflow.Active.View.PositionOfValue(tmp.b, true);
                    }
                    if (ch3) {
                        lineA[i].x = vx;
                        lineA[i].y = Timeflow.Active.View.PositionOfValue(tmp.a, true);
                    }

                    if (drawGhostLine) {
                        bool linked = false;
                        if (IsLinked) {
                            linked = Link.Enabled;
                            Link.Enabled = false;
                        }
                        tmp = InterpolateColor(time * TimeScaleWorld, false, false);
                        if (IsLinked) {
                            Link.Enabled = linked;
                        }
                        if (ch0) {
                            ghostline[i].x = vx;
                            ghostline[i].y = Timeflow.Active.View.PositionOfValue(tmp.r, true);
                        }
                        if (ch1) {
                            ghostline1[i].x = vx;
                            ghostline1[i].y = Timeflow.Active.View.PositionOfValue(tmp.g, true);
                        }
                        if (ch2) {
                            ghostline2[i].x = vx;
                            ghostline2[i].y = Timeflow.Active.View.PositionOfValue(tmp.b, true);
                        }
                        if (ch3) {
                            ghostline3[i].x = vx;
                            ghostline3[i].y = Timeflow.Active.View.PositionOfValue(tmp.a, true);
                        }
                    }
                }
                else
                if (multiChannel) {
                    Vector4 tmp = InterpolateVector4(time * TimeScaleWorld, false, false);

                    if (ch0) {
                        lineR[i].x = vx;
                        lineR[i].y = Timeflow.Active.View.PositionOfValue(tmp.x, true);
                    }
                    if (ch1) {
                        lineG[i].x = vx;
                        lineG[i].y = Timeflow.Active.View.PositionOfValue(tmp.y, true);
                    }
                    if (ch2) {
                        lineB[i].x = vx;
                        lineB[i].y = Timeflow.Active.View.PositionOfValue(tmp.z, true);
                    }
                    if (ch3) {
                        lineA[i].x = vx;
                        lineA[i].y = Timeflow.Active.View.PositionOfValue(tmp.w, true);
                    }

                    if (drawGhostLine) {
                        bool linked = false;
                        if (IsLinked) {
                            linked = Link.Enabled;
                            Link.Enabled = false;
                        }
                        tmp = InterpolateVector4(time * TimeScaleWorld, false, false);
                        if (IsLinked) {
                            Link.Enabled = linked;
                        }

                        if (ch0) {
                            ghostline[i].x = vx;
                            ghostline[i].y = Timeflow.Active.View.PositionOfValue(tmp.x, true);
                        }
                        if (ch1) {
                            ghostline1[i].x = vx;
                            ghostline1[i].y = Timeflow.Active.View.PositionOfValue(tmp.y, true);
                        }
                        if (ch2) {
                            ghostline2[i].x = vx;
                            ghostline2[i].y = Timeflow.Active.View.PositionOfValue(tmp.z, true);
                        }
                        if (ch3) {
                            ghostline3[i].x = vx;
                            ghostline3[i].y = Timeflow.Active.View.PositionOfValue(tmp.w, true);
                        }
                    }
                }
                else {
                    line[i].x = vx;
                    line[i].y = Timeflow.Active.View.PositionOfValue(InterpolateValue(time * TimeScaleWorld, false, false), true);

                    if (drawGhostLine) {
                        bool linked = false;
                        if (IsLinked) {
                            linked = Link.Enabled;
                            Link.Enabled = false;
                        }
                        ghostline[i].x = vx;
                        ghostline[i].y = Timeflow.Active.View.PositionOfValue(InterpolateValue(time * TimeScaleWorld, false, false, false), true);
                        if (IsLinked) {
                            Link.Enabled = linked;
                        }
                    }
                }

                time += timeStep;
            }
            Color colorTemp = GUI.color;
            GUI.color = Color.white;

            if (multiChannel) {
                float w = TimeflowView.GraphCurveThickness + 4f;
                if (drawGhostLine) {
                    float a = 0.18f;
                    if (ch0) {
                        Handles.color = new Color(1f, 0f, 0f, a);
                        Handles.DrawAAPolyLine(w, ghostline);
                    }
                    if (ch1) {
                        Handles.color = new Color(0f, 1f, 0f, a);
                        Handles.DrawAAPolyLine(w, ghostline1);
                    }
                    if (ch2) {
                        Handles.color = new Color(0f, 0f, 1f, a);
                        Handles.DrawAAPolyLine(w, ghostline2);
                    }
                    if (ch3) {
                        Handles.color = new Color(1f, 1f, 1f, a);
                        Handles.DrawAAPolyLine(w, ghostline3);
                    }
                }

                if (ch0) {
                    Handles.color = AxonColor.RedChannel;
                    if (isFaded) Handles.color = ColorUtil.SetAlpha(Handles.color, fadeAlpha);
                    Handles.DrawAAPolyLine(w, lineR);
                    Handles.DrawAAPolyLine(lineR);
                }
                if (ch1) {
                    Handles.color = AxonColor.GreenChannel;
                    if (isFaded) Handles.color = ColorUtil.SetAlpha(Handles.color, fadeAlpha);
                    Handles.DrawAAPolyLine(w, lineG);
                    Handles.DrawAAPolyLine(lineG);
                }
                if (ch2) {
                    Handles.color = AxonColor.BlueChannel;
                    if (isFaded) Handles.color = ColorUtil.SetAlpha(Handles.color, fadeAlpha);
                    Handles.DrawAAPolyLine(w, lineB);
                    Handles.DrawAAPolyLine(lineB);
                }
                if (ch3) {
                    Handles.color = AxonColor.AlphaChannel;
                    if (isFaded) Handles.color = ColorUtil.SetAlpha(Handles.color, fadeAlpha);
                    Handles.DrawAAPolyLine(w, lineA);
                    Handles.DrawAAPolyLine(lineA);
                }
            }
            else {
                if (drawGhostLine) {
                    Color a = Handles.color;
                    a.a = 0.25f;
                    Handles.color = a;
                    Handles.DrawAAPolyLine(TimeflowView.GraphCurveThickness, ghostline);
                }

                Handles.color = GUIColor;
                Handles.color = ColorUtil.SetAlpha(Handles.color, isFaded ? fadeAlpha : 1f);
                Handles.DrawAAPolyLine(TimeflowView.GraphCurveThickness, line);
            }
            GUI.color = colorTemp;

            if (hasKeys) {
                i = 0;
                float timeOffset = 0;
                if (Behavior != null) {
                    timeOffset = Behavior.TimeOffset;
                }
                int ki = 0; // use key index to ignore inTan of first key, and outTan of last key
                foreach (Keyframe key in Keys) {
                    float keyTime = key.KeyTimeWorld;
                    if (keyTime < Timeflow.Active.View.ViewStartTime || keyTime > Timeflow.Active.View.ViewEndTime) {
                        key.GUIRect.Clear();
                        if (multiChannel) {
                            key.GUIRect1.Clear();
                            key.GUIRect2.Clear();
                            key.GUIRect3.Clear();
                        }
                    }
                    else {
                        float keyValue = key.KeyValue;
                        if (IsColor && multiChannel) {
                            if (ch0 && Timeflow.Active.Input.DragChannelIndex == 0) {
                                keyValue = key.KeyColor.r;
                            }
                            else
                            if (ch1 && Timeflow.Active.Input.DragChannelIndex == 1) {
                                keyValue = key.KeyColor.g;
                            }
                            else
                            if (ch2 && Timeflow.Active.Input.DragChannelIndex == 2) {
                                keyValue = key.KeyColor.b;
                            }
                            else
                            if (ch3 && Timeflow.Active.Input.DragChannelIndex == 3) {
                                keyValue = key.KeyColor.a;
                            }
                        }
                        else
                        if (multiChannel) {
                            if (ch0 && Timeflow.Active.Input.DragChannelIndex == 0) {
                                keyValue = key.KeyVector.x;
                            }
                            else
                            if (ch1 && Timeflow.Active.Input.DragChannelIndex == 1) {
                                keyValue = key.KeyVector.y;
                            }
                            else
                            if (ch2 && Timeflow.Active.Input.DragChannelIndex == 2) {
                                keyValue = key.KeyVector.z;
                            }
                            else
                            if (ch3 && Timeflow.Active.Input.DragChannelIndex == 3) {
                                keyValue = key.KeyVector.w;
                            }
                        }
                        float x = Timeflow.Active.View.PositionOfTime(keyTime + timeOffset, true);
                        float y = Timeflow.Active.View.PositionOfValue(keyValue, true);
                        key.GUIRect = new GUIRect(x - 8f, y - 8f, 16, 16);

                        if (IsColor && multiChannel) {
                            key.GUIRect = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyColor.r, true) - 8f, 16, 16);
                            key.GUIRect1 = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyColor.g, true) - 8f, 16, 16);
                            key.GUIRect2 = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyColor.b, true) - 8f, 16, 16);
                            key.GUIRect3 = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyColor.a, true) - 8f, 16, 16);
                        }
                        else
                        if (multiChannel) {
                            key.GUIRect = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyVector.x, true) - 8f, 16, 16);
                            key.GUIRect1 = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyVector.y, true) - 8f, 16, 16);
                            key.GUIRect2 = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyVector.z, true) - 8f, 16, 16);
                            key.GUIRect3 = new GUIRect(x - 8f, Timeflow.Active.View.PositionOfValue(key.KeyVector.w, true) - 8f, 16, 16);
                        }

                        bool isSelected = false;
                        if (Timeflow.Active.View.SelectedKeys != null) {
                            isSelected = Timeflow.Active.View.SelectedKeys.Contains(key);
                        }

                        if ((key.LockTime || key.LockValue) && Timeflow.Active.Input.IsDragging) {
                            GUI.color = Color.white;
                            GUIRect lockRect = key.GUIRect;
                            lockRect.x -= 8;
                            lockRect.width = lockRect.height = 16;
                            GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);
                        }

                        if (!IsLocked && (Interpolation == TimeflowChannel.Interpolations.Bezier)
                            && !key.Linear && key.ShowTangents && key.IsKeyEnabled && (isSelected || Timeflow.Active.View.GraphShowBezierHandles)) {

                            bool canShow = true;
                            if (multiChannel) {
                                if (Timeflow.Active.Input.DragChannelIndex == 0) {
                                    canShow = ch0;
                                }
                                else
                                if (Timeflow.Active.Input.DragChannelIndex == 1) {
                                    canShow = ch1;
                                }
                                else
                                if (Timeflow.Active.Input.DragChannelIndex == 2) {
                                    canShow = ch2;
                                }
                                else
                                if (Timeflow.Active.Input.DragChannelIndex == 3) {
                                    canShow = ch3;
                                }
                            }

                            if (canShow) {
                                Handles.color = key.IsAutoTangents ? AxonColor.KeyAutoTangents :
                                    TimeflowView.GUIGraphPassNumber == 0 ? AxonColor.KeyTangents :
                                    TimeflowView.GUIGraphPassNumber == 1 ? AxonColor.KeyTangents2 : AxonColor.KeyTangents3;

                                GUI.color = Handles.color;
                                if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);

                                if (key.IsAutoTangents) {
                                    _Style = AxonUI.BezierUnifiedHandleStyle;
                                }
                                else
                                if (key.UnifyTangents) {
                                    _Style = AxonUI.BezierUnifiedHandleStyle;
                                }
                                else
                                if (key.UnifyTangentLengths) {
                                    _Style = AxonUI.BezierEqualHandleStyle;
                                }
                                else {
                                    _Style = AxonUI.BezierBrokenHandleStyle;
                                }

                                line = new Vector3[3];
                                if (ki > 0) {
                                    line[0].x = Timeflow.Active.View.PositionOfTime(keyTime + key.InTangent.x + timeOffset, true);
                                    line[0].y = Timeflow.Active.View.PositionOfValue(keyValue + key.InTangent.y, true);
                                    key.InPointRect = new GUIRect(line[0].x - 10f, line[0].y - 10f, 20, 20);

                                    GUI.Box(key.InPointRect, GUIContent.none, _Style);
                                }
                                else {
                                    line[0].x = x;
                                    line[0].y = y;
                                }

                                line[1].x = x;
                                line[1].y = y;

                                if (!key.Hold && ki < Keys.Count - 1) {
                                    line[2].x = Timeflow.Active.View.PositionOfTime(keyTime + key.OutTangent.x + timeOffset, true);
                                    line[2].y = Timeflow.Active.View.PositionOfValue(keyValue + key.OutTangent.y, true);
                                    key.OutPointRect = new GUIRect(line[2].x - 10f, line[2].y - 10f, 20, 20);
                                    GUI.Box(key.OutPointRect, GUIContent.none, _Style);
                                }
                                else {
                                    line[2].x = x;
                                    line[2].y = y;
                                }

                                Handles.DrawAAPolyLine(line);
                            }
                        }
                        i++;
                    }
                    ki++;
                }


                int k = first;
                for (i = 0; i < Keys.Count; i++) {
                    if (k < 0 || k >= Keys.Count) break;
                    Keyframe key = Keys[k]; k++;
                    float keyTime = key.KeyTimeWorld;
                    if (keyTime >= Timeflow.Active.View.ViewStartTime && keyTime <= Timeflow.Active.View.ViewEndTime) {
                        if (multiChannel && Timeflow.Active.View.IsGraphMode) {
                            GUIStyle keyframeStyle = GUIKeyframeStyle(key, false);
                            GUIStyle keyframeSelectedStyle = GUIKeyframeStyle(key, true);
                            if (Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(key)) {
                                if (ch0) {
                                    GUI.color = key.AttributeSelected0 ? GUIColorWhite : Color.gray;
                                    if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                    GUI.Box(key.GUIRect, GUIContent.none, keyframeSelectedStyle);

                                    if (key.AttributeSelected0) {
                                        GUI.color = Color.white;
                                        GUI.Box(key.GUIRect, GUIContent.none, keyframeSelectedStyle);
                                    }
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect, key.KeyVector.x, isFaded, true);
                                }
                                if (ch1) {
                                    GUI.color = key.AttributeSelected1 ? GUIColorWhite : Color.gray;
                                    if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                    GUI.Box(key.GUIRect1, GUIContent.none, keyframeStyle);

                                    if (key.AttributeSelected1) {
                                        GUI.color = Color.white;
                                        GUI.Box(key.GUIRect1, GUIContent.none, keyframeSelectedStyle);
                                    }
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect1, key.KeyVector.y, isFaded, true);
                                }
                                if (ch2) {
                                    GUI.color = key.AttributeSelected2 ? GUIColorWhite : Color.gray;
                                    if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                    GUI.Box(key.GUIRect2, GUIContent.none, key.AttributeSelected2 ? keyframeSelectedStyle : keyframeStyle);

                                    if (key.AttributeSelected2) {
                                        GUI.color = Color.white;
                                        GUI.Box(key.GUIRect2, GUIContent.none, keyframeSelectedStyle);
                                    }
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect2, key.KeyVector.z, isFaded, true);
                                }
                                if (ch3) {
                                    GUI.color = key.AttributeSelected3 ? GUIColorWhite : Color.gray;
                                    if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                    GUI.Box(key.GUIRect3, GUIContent.none, key.AttributeSelected3 ? keyframeSelectedStyle : keyframeStyle);

                                    if (key.AttributeSelected3) {
                                        GUI.color = Color.white;
                                        GUI.Box(key.GUIRect3, GUIContent.none, keyframeSelectedStyle);
                                    }
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect3, key.KeyVector.w, isFaded, true);
                                }
                                GUI.color = GUIColorWhite;
                            }
                            else {
                                GUI.color = GUIColor;
                                if (!key.IsKeyEnabled) GUI.color = Color.gray;
                                if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                if (ch0) {
                                    GUI.Box(key.GUIRect, GUIContent.none, keyframeStyle);
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect, key.KeyVector.x, isFaded);
                                }
                                if (ch1) {
                                    GUI.Box(key.GUIRect1, GUIContent.none, keyframeStyle);
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect1, key.KeyVector.y, isFaded);
                                }
                                if (ch2) {
                                    GUI.Box(key.GUIRect2, GUIContent.none, keyframeStyle);
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect2, key.KeyVector.z, isFaded);
                                }
                                if (ch3) {
                                    GUI.Box(key.GUIRect3, GUIContent.none, keyframeStyle);
                                    GUIDrawKeyframeLabelGraph(key, key.GUIRect3, key.KeyVector.w, isFaded);
                                }
                            }
                        }
                        else {

                            if (Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(key)) {
                                GUI.color = GUIColorWhite;
                                if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                GUI.Box(key.GUIRect, GUIContent.none, GUIKeyframeStyle(key, false));

                                GUI.color = Color.white;
                                GUI.Box(key.GUIRect, GUIContent.none, GUIKeyframeStyle(key, true));

                                if (Timeflow.Active.Input.IsDraggingCopy && Timeflow.Active.Input.DraggingTimeOffset != 0f) {
                                    GUIRect r = new GUIRect(key.GUIRect);
                                    r.x = Timeflow.Active.View.PositionOfTime(keyTime + Timeflow.Active.Input.DraggingTimeOffset, true) - 8;
                                    GUI.Box(r, GUIContent.none, GUIKeyframeStyle(key, true));
                                }
                            }
                            else {
                                GUI.color = GUIColor;
                                if (!key.IsKeyEnabled) GUI.color = Color.gray;
                                if (isFaded) GUI.color = ColorUtil.SetAlpha(GUI.color, fadeAlpha);
                                GUI.Box(key.GUIRect, GUIContent.none, GUIKeyframeStyle(key, false));
                            }

                            GUIDrawKeyframeLabelGraph(key, isFaded);
                        }
                    }
                }
            }
        }

        public virtual void GUIDrawKeyframeLabelGraph(Keyframe key, GUIRect keyRect, float value, bool isFaded = false, bool forceShow = false)
        {
            GUIRect labelRect = new GUIRect(keyRect.x + 16, keyRect.y - 16, 100, GUIHeight);
            GUIDrawKeyframeLabel(key, keyRect, labelRect, value.ToString(), true, isFaded, forceShow);
        }

        public virtual void GUIDrawKeyframeLabelGraph(Keyframe key, bool isFaded = false, bool forceShow = false)
        {
            GUIDrawKeyframeLabelGraph(key, key.ToString(), isFaded, forceShow);
        }

        protected void GUIDrawKeyframeLabelGraph(Keyframe key, string value, bool isFaded = false, bool forceShow = false)
        {
            GUIRect labelRect = new GUIRect(key.GUIRect.x + 16, key.GUIRect.y - 16, 100, GUIHeight);
            GUIDrawKeyframeLabel(key, key.GUIRect, labelRect, value, true, isFaded, forceShow);
        }

        public virtual void GUIDrawKeyframeLabel(Keyframe key, GUIRect keyRect, GUIRect labelRect, string value, bool isGraph, bool isFaded = false, bool forceShow = false)
        {
            if (forceShow || AlwaysShowValues || Timeflow.Active.ShowKeyframeValues || keyRect.Contains(Event.current.mousePosition)) {
                Color c = GUI.color;
                Color g = TimeflowPreferences.Current.KeyframeLabelColor;
                //Color g = AxonColor.TrackGraphLabel;
                if (isFaded) g.a = 0.25f;
                GUI.color = g;
                labelRect.width = (int)Mathf.Min(labelRect.width, AxonGUI.CalculateWidth(value) + 20);
                GUI.Box(labelRect, new GUIContent(value), AxonUI.SmallLabelStyle);
                GUI.color = c;
            }
        }

        public virtual void GUIGraphFit(bool init, bool selectedOnly)
        {
            if (Keys != null && Keys.Count > 0) {
                float worldOffset = TimeOffsetWorld;
                Vector2 range = GetValueRange(Timeflow.Active.View.ViewStartTime - worldOffset, Timeflow.Active.View.ViewEndTime - worldOffset,
                    Timeflow.Active.View.ShowChannel0, Timeflow.Active.View.ShowChannel1, Timeflow.Active.View.ShowChannel2, Timeflow.Active.View.ShowChannel3);

                if (init) {
                    Timeflow.Active.View.GraphMinValue = range.x;
                    Timeflow.Active.View.GraphMaxValue = range.y;
                }
                else {
                    Timeflow.Active.View.GraphMinValue = Mathf.Min(Timeflow.Active.View.GraphMinValue, range.x);
                    Timeflow.Active.View.GraphMaxValue = Mathf.Max(Timeflow.Active.View.GraphMaxValue, range.y);
                }
            }
        }

        public virtual bool GUICustomHit(Vector2 pos)
        {
            bool hit = false;
            return hit;
        }

        public virtual void GUICustomHitEnded() { }

        public virtual void GUICustomDragStart(Vector2 pos)
        {
        }

        public virtual void GUICustomDrag(Vector2 pos)
        {

        }

        public virtual bool GUILoopHandlesHit(Vector2 pos)
        {
            bool hit = false;
            if (EnableLoop) {
                if (LoopInDragRect.Contains(pos)) {
                    hit = true;
                    isDragChannelLoopOut = false;
                }
                else
                if (LoopOutDragRect.Contains(pos)) {
                    hit = true;
                    isDragChannelLoopOut = true;
                }
            }
            return hit;
        }

        public virtual void GUILoopHandlesDragStart(Vector2 pos)
        {
            UndoUtil.Undo(Behavior, "Drag Channel Loop");
            dragStart = pos;
            if (isDragChannelLoopOut) {
                dragOffset.x = dragStart.x - LoopOutDragRect.x;
            }
            else {
                dragOffset.x = dragStart.x - LoopInDragRect.x;
            }

            float loopStart = LoopStart;
            float loopEnd = LoopEnd;

            dragStartLoopLength = loopEnd - loopStart;
        }

        public virtual void GUILoopHandlesDrag(Vector2 pos)
        {
            EnableAutoLoop = false;
            float offset = TimeOffsetWorld;
            float start = LoopStart + offset;
            float end = LoopEnd + offset;

            if (isDragChannelLoopOut) {
                end = Timeflow.Active.View.TimeOfPosition(pos.x, false);
                end *= TimeScaleWorld;
                if (Event.current.shift) {
                    start = end - dragStartLoopLength;
                }
            }
            else {
                start = Timeflow.Active.View.TimeOfPosition(pos.x, false);
                start *= TimeScaleWorld;
                if (Event.current.shift) {
                    end = start + dragStartLoopLength;
                }
            }
            if (start > end) {
                float e = end;
                end = start;
                start = e;
            }

            LoopStart = start - offset;
            LoopEnd = end - offset;

            Timeflow.Active.View.ObjectTouched = true;
        }

        public virtual void GUIChannelContextMenu(GenericMenu menu) { }

        public virtual void GUISelectedKeysContextMenu(GenericMenu menu) { }

        public virtual void GUIDoubleClick() { }

        public virtual bool GUIDragAndHover()
        {
            bool handled = false;
            Vector2 m = Timeflow.Active.Input.GetMousePosition(Timeflow.Active.Layout.Hierarchy.Rect);
            if (GUIRect.Contains(m)) {
                if (channelLinkButtonRect.Contains(m)) {
                    handled = true;
                }
            }
            if (handled) {
                TimeflowView.LinkReceiver = this;
                TimeflowView.IsLinking = true;
            }
            else {
                TimeflowView.LinkReceiver = null;
                TimeflowView.IsLinking = false;
            }
            return false;
        }

        public virtual bool GUIDragAndDrop(List<TimeflowObject> objects)
        {
            bool handled = false;

            Vector2 m = Timeflow.Active.Input.GetMousePosition(Timeflow.Active.Layout.Hierarchy.Rect);
            if (channelLinkButtonRect.Contains(m)) {
                UndoUtil.Undo(Behavior, "Channel Link", true);
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Channel Link", true);
                    Keyframer kf = ObjectUtil.GetOrAddComponent<Keyframer>(obj.gameObject);
                    if (kf != null) {
                        TimeflowChannel ch = kf.GetChannel(ChannelLinkName);
                        if (ch == null) {
                            ch = new TimeflowChannel(kf);
                            ch.ToProperty = new Property(kf, ToProperty);
                            ch.ToProperty.CanBeAssigned = true;
                            if (!ch.ToProperty.SwitchGameObject(obj.gameObject)) {
                                // Indicate to users that this is a data-only channel since no matching property was found on the object
                                ch.ToProperty.Component = kf;
                                ch.Name = ch.ToProperty.DisplayName = ch.ToProperty.DisplayName;
                            }
                            ch.GetDataType();
                            kf.AddChannel(ch);
                        }

                        UndoUtil.Undo(ch.Behavior, "Channel Link", true);
                        ch.Link = new TimeflowChannelLink(ch, this);
                        handled = true;
                    }
                }
            }
            return handled;
        }

        public virtual void GUIDragAndDropEnded() { }

        public virtual void GUIInfo(List<TimeflowChannel> selectedChannels)
        {
            if (!IsSelected || selectedChannels.Count == 0 || Behavior == null) {
                return;
            }
            if (HasProperty) {
                if (ToProperty.Comp == null) ToProperty.Comp = Behavior.transform;

                bool multiChannel = true;// false;
                if (ToProperty != null && ToProperty.Name != null && ToProperty.Name.Contains("Scale")) {
                    multiChannel = true;
                }
                Type type = ToProperty != null && ToProperty.Owner != null ? ToProperty.Owner.GetComponentType() : null;
                AxonGUI.PropertySelect(Behavior, type, Behavior.gameObject, ToProperty, Property.PropertyFilters.All, null, multiChannel, true);

                /// Channels can only update if its property is enabled
                ToProperty.IsEnabled = IsEnabled;
            }

            if (IsLinked) {
                AxonGUI.FieldChannelLink(Behavior, this);
                AxonGUI.Space();
            }
        }

        bool _anychange = false;

        bool anychange {
            get {
                return _anychange;
            }
            set {
                if (_anychange != value) {
                    _anychange = value;
                    //Debug.Log("Change");
                }
            }
        }


        public virtual void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            bool hasFloat = false;
            bool hasBool = false;
            bool hasString = true;
            bool isString = false;
            bool hasEnum = false;
            bool hasLayerMask = false;
            bool hasComponent = false;
            bool hasGameObject = false;
            bool hasObject = false;
            bool hasColor = false;
            bool hasVector = false;
            bool hasRect = false;
            bool hasRectOffset = false;

            bool hasUniform = false;
            bool isUniform = false;
            int attributeCount = 1;
            anychange = false;

            Type compType = GetDataType();
            Type propertyType = typeof(float);

            Keyframe firstKey = null;
            Keyframe lastKey = null;

            foreach (Keyframe key in selectedKeys) {
                if (tracksOnly && !key.IsTrack) continue;
                if (!tracksOnly && key.IsTrack) continue;

                propertyType = key.Channel.GetDataType(); // force refresh data type in case it has changed

                if (firstKey == null || firstKey.KeyTime > key.KeyTime) {
                    firstKey = key;
                }
                if (lastKey == null || lastKey.KeyTime < key.KeyTime) {
                    lastKey = key;
                }

                if (key.IsTrack) {
                    hasColor = false;
                    hasFloat = true;
                    hasString = true;
                }
                else {
                    attributeCount = Mathf.Max(attributeCount, key.AttributeCount);
                    if (key.Channel.ShowVector) {
                        hasVector = true;
                        hasFloat = key.Channel.ShowValue;
                    }
                    if (key.Channel.ShowValue) {
                        if (key.Channel.IsBool) {
                            hasBool = true;
                        }
                        else
                        if (!key.HasMultipleAttributes || key.IsUniformValue || !key.IsCombinedValue) {
                            hasFloat = true;
                            if (key.IsUniformValue) isUniform = true;
                        }
                    }
                    if (key.IsColor || key.Channel.ShowColor) {
                        if (key.IsUniformValue) {
                            hasFloat = true;
                            isUniform = true;
                        }
                        else
                        if (key.IsCombinedValue) {
                            hasColor = true;
                        }
                    }
                    if (key.IsRect) {
                        if (key.IsUniformValue) {
                            hasFloat = true;
                            isUniform = true;
                        }
                        else
                        if (key.IsCombinedValue) {
                            hasRect = true;
                        }
                    }
                    else
                    if (key.IsRectOffset) {
                        if (key.IsUniformValue) {
                            hasFloat = true;
                            isUniform = true;
                        }
                        else
                        if (key.IsCombinedValue) {
                            hasRectOffset = true;
                        }
                    }
                    else
                    if (key.IsVector) {
                        if (key.IsUniformValue) {
                            hasFloat = true;
                            isUniform = true;
                        }
                        else
                        if (key.IsCombinedValue) {
                            hasVector = true;
                            hasFloat = false;
                        }
                    }
                    if (key.IsString) {
                        isString = true;
                        hasString = key.Channel.ShowString;
                        hasFloat = false;
                    }
                    else
                    if (!key.IsString || !key.Channel.ShowString || tracksOnly) {
                        hasString = false;
                    }
                    if (key.IsComponent || key.Channel.ShowComponent) {
                        hasComponent = true;
                    }
                    if (key.IsGameObject || key.Channel.ShowGameObject) {
                        hasGameObject = true;
                    }
                    if (key.IsObject) {
                        hasObject = true;
                    }
                    if (key.IsEnum) {
                        hasEnum = true;
                    }
                    if (key.IsLayerMask) {
                        hasLayerMask = true;
                        hasFloat = false;
                    }
                }
            }
            hasUniform = isUniform || hasVector || hasRect || hasRectOffset || hasColor;

            float value = 0f;
            bool bval = false;
            bool eval = false;
            Component component = null;
            UnityEngine.Object ueObj = null;
            GameObject obj = null;
            string stringVal = null;
            Color color = Color.black;
            Color guiColor = Color.black;
            Color trackLabel = Color.black;
            Vector4 vector = Vector4.zero;
            float time = 0f;
            bool overrideGUIColor = false;
            bool timelocked = false;
            bool valuelocked = false;
            bool uniform = false;
            float timeOffset = 0f;
            float timeScale = 0f;
            bool auto = false;

            LayerMask lmVal = new LayerMask();

            bool first = true;
            bool vsame = true;
            bool esame = true;
            bool tsame = true;
            bool csame = true;
            bool ukcsame = true;
            bool trackLabelSame = true;
            bool uksame = true;
            bool ssame = true;
            bool vecsame = true;
            bool objsame = true;
            bool ueobjsame = true;
            bool tlocksame = true;
            bool vlocksame = true;
            bool bvsame = true;
            bool unisame = true;
            bool tosame = true;
            bool tscsame = true;
            bool lmsame = true;

            bool attr1same = true;
            bool attr2same = true;
            bool attr3same = true;
            bool attr4same = true;

            bool isLocalTimeOffset = false;
            bool firstTrack = true;

            foreach (Keyframe key in selectedKeys) {
                if (tracksOnly && !key.IsTrack) continue;
                if (!tracksOnly && key.IsTrack) continue;
                if (firstTrack && key.IsTrack) {
                    firstTrack = false;
                    timeOffset = key.Channel.Object.TimeOffset;
                    timeScale = key.Channel.Object.TimeScale;
                    if (key.Channel != null) {
                        trackLabel = key.Channel.Object.Track.GUITextColor;
                    }
                    if (key.IsAutoTrackLength) {
                        auto = true;
                    }
                }
                if (first) {
                    time = key.KeyTime;
                    component = key.KeyComponent;
                    ueObj = key.KeyObject;
                    obj = key.KeyGameObject;
                    stringVal = key.KeyString;
                    value = key.KeyValue;
                    color = key.KeyColor;
                    guiColor = key.GUIColor;
                    vector = key.KeyVector;
                    bval = key.KeyBool;
                    overrideGUIColor = key.OverrideGUIColor;
                    timelocked = key.LockTime;
                    valuelocked = key.LockValue;
                    uniform = key.IsUniformValue;
                    eval = key.IsKeyEnabled;
                    lmVal.value = (int)key.KeyValue;
                    first = false;

                    if (key.Behavior.TimeOffsetWorld != 0f) {
                        isLocalTimeOffset = true;
                    }
                }
                else {
                    if (tosame && MathUtil.IsKeyDifferent(timeOffset, key.Channel.Object.TimeOffset)) {
                        tosame = false;
                    }
                    if (tscsame && MathUtil.IsKeyDifferent(timeScale, key.Channel.Object.TimeScale)) {
                        tscsame = false;
                    }
                    if (tsame && MathUtil.IsKeyDifferent(time, key.KeyTime)) {
                        tsame = false;
                    }
                    if (vsame && MathUtil.IsKeyDifferent(value, key.KeyValue)) {
                        vsame = false;
                    }
                    if (esame && eval != key.IsKeyEnabled) {
                        esame = false;
                    }
                    if (bvsame && bval != key.KeyBool) {
                        bvsame = false;
                    }
                    if (ueobjsame && ueObj != key.KeyObject) {
                        ueobjsame = false;
                    }
                    if (objsame && component != key.KeyComponent) {
                        objsame = false;
                    }
                    if (objsame && obj != key.KeyGameObject) {
                        objsame = false;
                    }
                    if (ssame && stringVal != key.KeyString) {
                        ssame = false;
                    }
                    if (csame && hasColor && MathUtil.IsKeyDifferent(color, key.KeyColor)) {
                        csame = false;
                    }
                    if (ukcsame && MathUtil.IsKeyDifferent(guiColor, key.GUIColor)) {
                        ukcsame = false;
                    }
                    if (trackLabelSame && key.Channel != null && MathUtil.IsKeyDifferent(trackLabel, key.Channel.Object.Track.GUITextColor)) {
                        trackLabelSame = false;
                    }
                    if (uksame && overrideGUIColor != key.OverrideGUIColor) {//tracksOnly && 
                        uksame = false;
                    }
                    if (vecsame && MathUtil.IsKeyDifferent(vector, key.KeyVector)) {
                        vecsame = false;
                    }
                    if (tlocksame && timelocked != key.LockTime) {
                        tlocksame = false;
                    }
                    if (vlocksame && valuelocked != key.LockValue) {
                        vlocksame = false;
                    }
                    if (unisame && uniform != key.IsUniformValue) {
                        unisame = false;
                    }
                    if (lmsame && lmVal.value != (int)key.KeyValue) {
                        lmsame = false;
                    }

                    if (hasVector) {
                        if (MathUtil.IsKeyDifferent(vector.x, key.KeyVector.x)) attr1same = false;
                        if (MathUtil.IsKeyDifferent(vector.y, key.KeyVector.y)) attr2same = false;
                        if (MathUtil.IsKeyDifferent(vector.z, key.KeyVector.z)) attr3same = false;
                        if (MathUtil.IsKeyDifferent(vector.w, key.KeyVector.w)) attr4same = false;
                    }
                }
            }

            float v = value;
            bool e = eval;
            bool bv = bval;
            Color c = color;
            Color ukc = guiColor;
            Color tl = trackLabel;
            float t = time;
            Vector4 vec = vector;
            bool uk = overrideGUIColor;
            bool l = timelocked;
            bool lv = valuelocked;
            bool uni = uniform;
            float tof = timeOffset;
            float tsc = timeScale;
            Component comp = component;
            GameObject ob = obj;
            UnityEngine.Object ueo = ueObj;
            LayerMask lm = lmVal;

            //bool anychange = false;
            bool vchanged = false;
            bool evchanged = false;
            bool bvchanged = false;
            bool tchanged = false;
            bool cchanged = false;
            bool ukcchanged = false;
            bool tlchanged = false;
            bool vecchanged = false;
            bool obchanged = false;
            bool ueochanged = false;
            bool compchanged = false;
            bool ukchanged = false;
            bool schanged = false;
            bool lchanged = false;
            bool lvchanged = false;
            bool unichanged = false;
            bool tofchanged = false;
            bool tscchanged = false;
            bool lmchanged = false;

            bool attr1changed = false;
            bool attr2changed = false;
            bool attr3changed = false;
            bool attr4changed = false;

            float maxWidth = 250f;

            AxonGUI.SetLabelWidth(50);
            AxonGUI.BeginHorizontal();

            if (AxonGUI.ButtonLock(l, "Lock the time of the keyframe to prevent changes")) {
                lchanged = true;
                anychange = true;
                l = !l;
            }

            EditorGUI.BeginDisabledGroup(l);
            if (tracksOnly) EditorGUI.BeginDisabledGroup(auto);
            AxonGUI.UndoName = "Set Keyframe Start Time";
            if (!tsame) {
                string tmp = "-";
                tmp = AxonGUI.FieldText(Behavior, tracksOnly ? " Start" : " Time", tmp, true, GUILayout.MaxWidth(maxWidth));
                if (tmp != "-") {
                    tchanged = true;
                    anychange = true;
                    t = TimeflowView.ParseTime(tmp);
                }
            }
            else {
                t = AxonGUI.FieldTime(null, tracksOnly ? " Start" : " Time", t, isLocalTimeOffset, GUILayout.MaxWidth(maxWidth));
                if (MathUtil.IsKeyDifferent(t, time)) {
                    tchanged = true;
                    anychange = true;
                }
            }

            if (tracksOnly) {
                float tmpw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 20;
                AxonGUI.UndoName = "Set Keyframe End Time";
                if (!vsame) {
                    string tmp = "-";
                    tmp = AxonGUI.FieldTextInline(Behavior, " End", tmp, true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != "-") {
                        vchanged = true;
                        anychange = true;
                        t = TimeflowView.ParseTime(tmp);
                    }
                }
                else {
                    v = AxonGUI.FieldTimeInline(Behavior, " End", v, isLocalTimeOffset, GUILayout.MaxWidth(maxWidth));
                    if (MathUtil.IsKeyDifferent(v, value)) {
                        vchanged = true;
                        anychange = true;
                    }
                }
                EditorGUI.EndDisabledGroup();

                AxonGUI.UndoName = "Set Auto Duration";
                bool tmpAuto = AxonGUI.FieldToggleInline(Behavior, "Auto", auto);
                if (tmpAuto != auto) {
                    auto = tmpAuto;
                    foreach (Keyframe key in selectedKeys) {
                        if (!key.IsTrack) continue;
                        TimeflowObject tobj = (TimeflowObject)key.Behavior;
                        if (tobj != null && tobj.Track != null) {
                            tobj.Track.SetFullLength(auto);
                        }
                    }
                }
                EditorGUIUtility.labelWidth = tmpw;
            }

            EditorGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Keyframe Enabled";
            bool ev = AxonGUI.FieldToggleEnabled(null, e);
            if (ev != e) {
                e = ev;
                evchanged = true;
                anychange = true;
            }
            EditorGUI.BeginDisabledGroup(!tsame);
            if (AxonGUI.ButtonInline("Goto")) {
                Timeflow.Active.SetTime(t);
            }
            if (AxonGUI.ButtonInline("<")) {
                GotoPrevKeyframe(firstKey.KeyTime);
            }
            if (AxonGUI.ButtonInline(">")) {
                GotoNextKeyframe(lastKey.KeyTime);
            }
            EditorGUI.EndDisabledGroup();
            AxonGUI.EndHorizontal(false);

            if (tracksOnly) {
                float tmpw = EditorGUIUtility.labelWidth;
                AxonGUI.SetLabelWidth(70);
                EditorGUI.BeginDisabledGroup(l);
                AxonGUI.BeginHorizontal();
                //AxonGUI.FlexibleSpace();
                AxonGUI.UndoName = "Set Time Offset";
                if (!tosame) {
                    string tmp = "-";
                    tmp = AxonGUI.FieldText(Behavior, "Time Offset", tmp, true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != "-") {
                        tofchanged = true;
                        anychange = true;
                        tof = TimeflowView.ParseTime(tmp);
                    }
                }
                else {
                    tof = AxonGUI.FieldTime(null, "Time Offset", tof, isLocalTimeOffset, GUILayout.MaxWidth(maxWidth));
                    if (MathUtil.IsKeyDifferent(tof, timeOffset)) {
                        tofchanged = true;
                        anychange = true;
                    }
                }

                bool dragTimeOffset = selectedKeys[0].Channel.Object.CanDragTimeOffset;
                AxonGUI.UndoName = "Set Drag Time Offset";
                if (AxonGUI.ButtonTexture(dragTimeOffset ? AxonUI.TrackDragTimeOffsetOnStyle : AxonUI.TrackDragTimeOffsetOffStyle,
                    "When enabled, dragging tracks in the track view sets the object's time offset. Or if disabled, dragging changes the track's key start and end time. " +
                    "This option is forced on for precomps.")) {
                    dragTimeOffset = !dragTimeOffset;
                    foreach (Keyframe key in selectedKeys) {
                        if (!key.IsTrack) continue;
                        TimeflowObject tobj = (TimeflowObject)key.Behavior;
                        if (tobj != null && tobj.Track != null) {
                            tobj.CanDragTimeOffset = dragTimeOffset;
                        }
                    }
                }
                AxonGUI.LabelInline(dragTimeOffset ? "Drag Offset" : "Drag Disabled");

                AxonGUI.UndoName = "Set Time Scale";
                if (!tscsame) {
                    string tmp = "-";
                    tmp = AxonGUI.FieldText(Behavior, "Time Scale", tmp, true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != "-") {
                        tscchanged = true;
                        anychange = true;
                        tsc = TimeflowView.ParseTime(tmp);
                    }
                }
                else {
                    tsc = AxonGUI.FieldFloat(null, "Time Scale", tsc, GUILayout.MaxWidth(maxWidth));
                    if (MathUtil.IsKeyDifferent(tsc, timeScale)) {
                        tscchanged = true;
                        anychange = true;
                    }
                }

                AxonGUI.EndHorizontal(false);
                EditorGUI.EndDisabledGroup();
                EditorGUIUtility.labelWidth = tmpw;
            }


            string lockValueTip = "Lock the value of the keyframe to prevent changes";

            if (hasObject) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }
                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Keyframe Object";
                if (!objsame) {
                    UnityEngine.Object tmp = (UnityEngine.Object)AxonGUI.FieldObject(Behavior, "Object", ueo, typeof(UnityEngine.Object), true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != ueo) {
                        ueochanged = true;
                        anychange = true;
                        ueo = tmp;
                    }
                }
                else {
                    UnityEngine.Object tmp = (UnityEngine.Object)AxonGUI.FieldObject(Behavior, "Object", ueo, typeof(UnityEngine.Object), true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != ueo) {
                        ueochanged = true;
                        anychange = true;
                        ueo = tmp;
                    }
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (hasGameObject) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }
                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Keyframe Object";
                if (!objsame) {
                    GameObject tmp = null;
                    tmp = (GameObject)AxonGUI.FieldObject(Behavior, "Object", tmp, typeof(GameObject), true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != null) {
                        obchanged = true;
                        anychange = true;
                        ob = tmp;
                    }
                }
                else {
                    GameObject tmp = (GameObject)AxonGUI.FieldObject(Behavior, "Object", ob, typeof(GameObject), true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != ob) {
                        obchanged = true;
                        anychange = true;
                        ob = tmp;
                    }
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (hasComponent) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }
                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Keyframe Component";
                if (!objsame) {
                    Component tmp = null;
                    tmp = (Component)AxonGUI.FieldObject(Behavior, "Component", tmp, compType, true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != null) {
                        compchanged = true;
                        anychange = true;
                        comp = tmp;
                    }
                }
                else {
                    Component tmp = (Component)AxonGUI.FieldObject(Behavior, "Component", comp, compType, true, GUILayout.MaxWidth(maxWidth));
                    if (tmp != comp) {
                        compchanged = true;
                        anychange = true;
                        comp = tmp;
                    }
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (hasLayerMask) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }

                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Keyframe Value";
                if (!vsame) {
                    int tmp = AxonGUI.FieldLayerMask(null, "Value", (int)v, GUILayout.MaxWidth(maxWidth));
                    if (tmp != v) {
                        lmchanged = true;
                        anychange = true;
                        lm = tmp;
                    }
                }
                else {
                    v = AxonGUI.FieldLayerMask(null, "Value", (int)v, GUILayout.MaxWidth(maxWidth));
                    if (v != (int)value) {
                        lmchanged = true;
                        anychange = true;
                    }
                }

                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (hasEnum) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }

                string[] enumValues = Property.GetEnumValues(propertyType);

                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Keyframe Value";
                if (!vsame) {
                    if (enumValues == null) {
                        int tmp = AxonGUI.FieldInt(null, "Value", (int)v, GUILayout.MaxWidth(maxWidth));
                        if (tmp != v) {
                            vchanged = true;
                            anychange = true;
                            v = tmp;
                        }
                    }
                    else {
                        int tmp = AxonGUI.FieldPopup(null, "Value", (int)v, enumValues, GUILayout.MaxWidth(maxWidth));
                        if (tmp != v) {
                            vchanged = true;
                            anychange = true;
                            v = tmp;
                        }
                    }
                }
                else {
                    if (enumValues == null) {
                        v = AxonGUI.FieldInt(null, "Value", (int)v, GUILayout.MaxWidth(maxWidth));
                        if (v != (int)value) {
                            vchanged = true;
                            anychange = true;
                        }
                    }
                    else {
                        v = AxonGUI.FieldPopup(null, "Value", (int)v, enumValues, GUILayout.MaxWidth(maxWidth));
                        if (v != (int)value) {
                            vchanged = true;
                            anychange = true;
                        }
                    }
                }

                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (!tracksOnly && !hasEnum && (hasFloat || hasBool)) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }
                EditorGUI.BeginDisabledGroup(lv);

                if (hasFloat) {
                    AxonGUI.UndoName = "Set Keyframe Value";
                    if (!vsame) {
                        string tmp = "-";
                        tmp = AxonGUI.FieldText(Behavior, "Value", tmp, true, GUILayout.MaxWidth(maxWidth));
                        if (tmp != "-") {
                            vchanged = true;
                            anychange = true;
                            v = StringUtil.ParseFloat(tmp);
                        }
                    }
                    else {
                        v = AxonGUI.FieldFloat(Behavior, "Value", v, false, GUILayout.MaxWidth(maxWidth));
                        if (MathUtil.IsKeyDifferent(v, value)) {
                            vchanged = true;
                            anychange = true;
                        }
                    }

                    if (hasUniform) {
                        uni = AxonGUI.FieldToggleUniform(Behavior, isUniform);
                        if (isUniform != uni) {
                            unichanged = true;
                            anychange = true;
                        }
                    }
                }

                if (hasBool) {
                    AxonGUI.UndoName = "Set Keyframe Value";
                    if (!bvsame) {
                        bool tmp = bval;
                        tmp = AxonGUI.FieldToggle(Behavior, "Bool", tmp, GUILayout.MaxWidth(maxWidth));
                        if (tmp != bval) {
                            bvchanged = true;
                            anychange = true;
                            bv = tmp;
                        }
                    }
                    else {
                        bv = AxonGUI.FieldToggle(Behavior, "Bool", bv, GUILayout.MaxWidth(maxWidth));
                        if (bv != bval) {
                            bvchanged = true;
                            anychange = true;
                        }
                    }
                }

                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (hasString) {
                AxonGUI.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Keyframe String";
                if (!ssame) {
                    string tmp = "";
                    tmp = AxonGUI.FieldText(Behavior, tracksOnly ? "Label" : isString ? "Value" : "Name", tmp, false);
                    if (tmp != "") {
                        schanged = true;
                        anychange = true;
                        stringVal = tmp;
                    }
                }
                else {
                    string tmp = AxonGUI.FieldText(Behavior, tracksOnly ? "Label" : isString ? "Value" : "Name", stringVal, false);
                    if (tmp != stringVal) {
                        schanged = true;
                        anychange = true;
                        stringVal = tmp;
                    }
                }

                if (tracksOnly) {
                    AxonGUI.UndoName = "Set Label Color";
                    Color tmp = trackLabel;
                    if (tmp.a == 0) tmp.a = 1;
                    tmp = AxonGUI.FieldColorInline(Behavior, tmp, false, GUILayout.MaxWidth(maxWidth));
                    if (tmp != trackLabel) {
                        tlchanged = true;
                        anychange = true;
                        trackLabel = tmp;
                    }
                }


                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            AxonGUI.BeginHorizontal();
            if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                lvchanged = true;
                anychange = true;
                lv = !lv;
            }
            EditorGUI.BeginDisabledGroup(lv);
            AxonGUI.UndoName = "Keyframe Colorize";
            if (!uksame) {
                bool tmp = overrideGUIColor;
                tmp = AxonGUI.FieldToggle(Behavior, "Colorize", tmp);
                if (tmp != overrideGUIColor) {
                    ukchanged = true;
                    anychange = true;
                    uk = tmp;
                }
            }
            else {
                uk = AxonGUI.FieldToggle(Behavior, "Colorize", uk);
                if (uk != overrideGUIColor) {
                    ukchanged = true;
                    anychange = true;
                }
            }
            if (uk) {
                AxonGUI.UndoName = "Set Keyframe Colorize";
                if (!ukcsame) {
                    Color tmp = guiColor;
                    tmp = AxonGUI.FieldColor(Behavior, tmp, !tracksOnly, GUILayout.MaxWidth(maxWidth));
                    if (tmp != guiColor) {
                        ukcchanged = true;
                        anychange = true;
                        ukc = tmp;
                    }
                }
                else {
                    ukc = AxonGUI.FieldColor(Behavior, ukc, !tracksOnly, GUILayout.MaxWidth(maxWidth));
                    if (MathUtil.IsKeyDifferent(ukc, guiColor)) {
                        ukcchanged = true;
                        anychange = true;
                    }
                }

            }
            EditorGUI.EndDisabledGroup();
            AxonGUI.EndHorizontal(false);

            if (hasColor) {
                bool showColor = true;
                if (showColor) {
                    AxonGUI.BeginHorizontal();
                    if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                        lvchanged = true;
                        anychange = true;
                        lv = !lv;
                    }
                    EditorGUI.BeginDisabledGroup(lv);
                    AxonGUI.UndoName = "Set Keyframe Color";
                    if (!csame) {
                        Color tmp = color;
                        tmp = AxonGUI.FieldColor(Behavior, "Color", tmp, !tracksOnly, GUILayout.MaxWidth(maxWidth));
                        if (tmp != color) {
                            cchanged = true;
                            anychange = true;
                            c = tmp;
                        }
                    }
                    else {
                        c = AxonGUI.FieldColor(Behavior, "Color", c, !tracksOnly, GUILayout.MaxWidth(maxWidth));
                        if (MathUtil.IsKeyDifferent(c, color)) {
                            cchanged = true;
                            anychange = true;
                        }
                    }
                    if (hasUniform) {
                        uni = AxonGUI.FieldToggleUniform(Behavior, isUniform);
                        if (isUniform != uni) {
                            unichanged = true;
                            anychange = true;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    AxonGUI.EndHorizontal(false);
                }
            }

            if (hasVector || hasRect) {
                string vlabel = hasRect ? "GUIRect" : "Vector";
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }
                EditorGUI.BeginDisabledGroup(lv);
                if (!vecsame) {
                    Vector4 tmp = vec;
                    AxonGUI.Label(vlabel, GUILayout.Width(AxonGUI.LabelWidth));
                    AxonGUI.UndoName = "Set Keyframe Vector X";
                    if (attr1same) {
                        float tmpx = AxonGUI.FieldFloatInline(Behavior, "x", tmp.x);
                        if (tmp.x != tmpx) {
                            tmp.x = tmpx;
                            attr1changed = true;
                        }
                    }
                    else {
                        string t1 = AxonGUI.FieldTextInline(Behavior, "x", "-", true);
                        if (t1 != "-") {
                            attr1changed = true;
                            tmp.x = StringUtil.ParseFloat(t1);
                        }
                    }
                    AxonGUI.UndoName = "Set Keyframe Vector Y";
                    if (attr2same) {
                        float tmpy = AxonGUI.FieldFloatInline(Behavior, "y", tmp.y);
                        if (tmp.y != tmpy) {
                            tmp.y = tmpy;
                            attr2changed = true;
                        }
                    }
                    else {
                        string t2 = AxonGUI.FieldTextInline(Behavior, "y", "-", true);
                        if (t2 != "-") {
                            attr2changed = true;
                            tmp.y = StringUtil.ParseFloat(t2);
                        }
                    }
                    AxonGUI.UndoName = "Set Keyframe Vector Z";
                    if (attributeCount > 2) {
                        if (attr3same) {
                            float tmpz = AxonGUI.FieldFloatInline(Behavior, "z", tmp.z);
                            if (tmp.z != tmpz) {
                                tmp.z = tmpz;
                                attr3changed = true;
                            }
                        }
                        else {
                            string t3 = AxonGUI.FieldTextInline(Behavior, "z", "-", true);
                            if (t3 != "-") {
                                attr3changed = true;
                                tmp.z = StringUtil.ParseFloat(t3);
                            }
                        }
                    }
                    AxonGUI.UndoName = "Set Keyframe Vector W";
                    if (attributeCount > 3) {
                        if (attr4same) {
                            float tmpw = AxonGUI.FieldFloatInline(Behavior, "w", tmp.w);
                            if (tmp.w != tmpw) {
                                tmp.w = tmpw;
                                attr4changed = true;
                            }
                        }
                        else {
                            string t4 = AxonGUI.FieldTextInline(Behavior, "w", "-", true);
                            if (t4 != "-") {
                                attr4changed = true;
                                tmp.w = StringUtil.ParseFloat(t4);
                            }
                        }
                    }
                    if (tmp != vec || attr1changed || attr2changed || attr3changed || attr4changed) {
                        vec = tmp;
                        vecchanged = true;
                        anychange = true;
                    }
                }
                else {
                    AxonGUI.UndoName = "Set Keyframe Vector";
                    if (hasRect) {
                        vec = AxonGUI.FieldVector4AsRect(Behavior, vlabel, vec, GUILayout.MaxWidth(maxWidth));
                    }
                    else
                    if (attributeCount > 3) {
                        vec = AxonGUI.FieldVector4(Behavior, vlabel, vec, GUILayout.MaxWidth(maxWidth));
                    }
                    else
                    if (attributeCount > 2) {
                        vec = AxonGUI.FieldVector3(Behavior, vlabel, vec, GUILayout.MaxWidth(maxWidth));
                    }
                    else {
                        vec = AxonGUI.FieldVector2(Behavior, vlabel, vec, GUILayout.MaxWidth(maxWidth));
                    }
                    if (MathUtil.IsKeyDifferent(vec, vector)) {
                        vecchanged = true;
                        anychange = true;
                        attr1changed = attr2changed = attr3changed = attr4changed = true;
                    }
                }
                if (hasUniform) {
                    AxonGUI.UndoName = "Set Keyframe Uniform";
                    uni = AxonGUI.FieldToggleUniform(Behavior, isUniform);
                    if (isUniform != uni) {
                        unichanged = true;
                        anychange = true;
                    }
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (hasRectOffset) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, lockValueTip)) {
                    lvchanged = true;
                    anychange = true;
                    lv = !lv;
                }
                EditorGUI.BeginDisabledGroup(lv);
                if (!vecsame) {
                    Vector4 tmp = Vector4.zero;
                    AxonGUI.UndoName = "Set Keyframe Rect Offset";
                    tmp = AxonGUI.FieldVector4AsRectOffset(Behavior, "Rect Offset", tmp, GUILayout.MaxWidth(maxWidth));
                    if (tmp != Vector4.zero) {
                        vecchanged = true;
                        anychange = true;
                        vec = tmp;
                    }
                }
                else {
                    vec = AxonGUI.FieldVector4AsRectOffset(Behavior, "Rect Offset", vec, GUILayout.MaxWidth(maxWidth));
                    if (MathUtil.IsKeyDifferent(vec, vector)) {
                        vecchanged = true;
                        anychange = true;
                    }
                }
                if (hasUniform) {
                    AxonGUI.UndoName = "Set Keyframe Uniform";
                    uni = AxonGUI.FieldToggleUniform(Behavior, isUniform);
                    if (isUniform != uni) {
                        unichanged = true;
                        anychange = true;
                    }
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

            if (anychange) {
                /// Locate all affected channels to record undo before setting changed values
                List<TimeflowChannel> channels = new List<TimeflowChannel>();
                foreach (Keyframe key in selectedKeys) {
                    if (tracksOnly && !key.IsTrack) continue;
                    if (!tracksOnly && key.IsTrack) continue;
                    if (key.Channel != null && !channels.Contains(key.Channel)) {
                        channels.Add(key.Channel);
                        UndoUtil.Undo(key.Channel.Behavior, "Key Value", true);
                    }
                }

                if (lchanged) {
                    // When setting the lock for tracks, and Drag Time Offset is enabled, lock all tracks
                    // in unison, otherwise the user can inadvertently change the time offset of the track.
                    // If they wish to drag tracks independtly, they can disable Drag Time Offset.
                    foreach (TimeflowChannel channel in channels) {
                        if (!channel.IsTrack || channel.Object == null || !channel.Object.CanDragTimeOffset) continue;
                        foreach (Keyframe key in channel.Keys) {
                            key.LockTime = l;
                        }
                    }
                }

                foreach (Keyframe key in selectedKeys) {
                    if (tracksOnly && !key.IsTrack) continue;
                    if (!tracksOnly && key.IsTrack) continue;
                    if (lchanged) {
                        key.LockTime = l;
                        if (tracksOnly) {
                            key.LockValue = l;
                        }
                    }
                    if (evchanged) {
                        key.IsKeyEnabled = e;
                    }
                    if (lvchanged) {
                        key.LockValue = lv;
                    }

                    if (tracksOnly) {
                        if (tchanged || vchanged) {
                            if (t > v) t = v; // prevent invalid value
                            key.SetTrackTime(t, v);
                        }
                    }
                    if (tofchanged) {
                        key.Channel.Object.TimeOffset = tof;
                    }
                    if (tscchanged) {
                        key.Channel.Object.TimeScale = tsc;
                    }
                    if (tchanged && !tracksOnly) {
                        key.KeyTime = t;
                    }
                    if (hasUniform) {
                        if (unichanged) {
                            key.IsUniformValue = uni;
                        }
                    }
                    if (hasFloat) {
                        if (vchanged && !tracksOnly) {
                            key.KeyValue = MathUtil.Validate(v);
                        }
                    }
                    if (hasBool) {
                        if (bvchanged) {
                            key.KeyBool = bv;
                        }
                    }
                    if (ukchanged) key.OverrideGUIColor = uk;
                    if (uk && ukcchanged) {
                        key.GUIColor = MathUtil.Validate(ukc);
                    }
                    if (hasColor && cchanged) {
                        key.KeyColor = MathUtil.Validate(c);
                    }
                    if (tlchanged && key.Channel != null) {
                        key.Channel.Object.Track.GUITextColor = MathUtil.Validate(tl);
                    }
                    if (vecchanged) { // don't check hasVector to allow rect and other types
                        Vector4 keyVec = key.KeyVector;
                        if (attr1changed) keyVec.x = MathUtil.Validate(vec.x);
                        if (attr2changed) keyVec.y = MathUtil.Validate(vec.y);
                        if (attr3changed) keyVec.z = MathUtil.Validate(vec.z);
                        if (attr4changed) keyVec.w = MathUtil.Validate(vec.w);
                        key.KeyVector = keyVec;
                    }
                    if (hasComponent) {
                        if (compchanged) {
                            key.KeyComponent = comp;
                        }
                    }
                    if (hasGameObject) {
                        if (obchanged) {
                            key.KeyGameObject = ob;
                        }
                    }
                    if (hasObject) {
                        if (ueochanged) {
                            key.KeyObject = ueo;
                        }
                    }
                    if (hasString) {
                        if (schanged) {
                            key.KeyString = stringVal;
                        }
                        if (tlchanged && key.Channel != null) {
                            key.Channel.Object.Track.GUITextColor = MathUtil.Validate(trackLabel);
                        }
                    }
                    if (hasLayerMask) {
                        if (lmchanged) {
                            key.KeyValue = lm.value;
                        }
                    }
                }
            }

            AxonGUI.Space();
            AxonGUI.ResetLabelWidth();

            if (anychange) {
                Timeflow.IsAutoKeyframingInvalidThisFrame = true;
                GUIInfoValueChanged(selectedKeys);
                Behavior.UpdateTimeChannel(this);

                Timeflow.Active.View.ObjectTouched = true;
                if (!Application.isPlaying) {
                    Timeflow.Active.FrameID++;// to force cache data update
                    Timeflow.Active.DoUpdate();
                }
            }
        }

        public virtual void GUIInfoValueChanged(List<Keyframe> selectedKeys)
        {
        }

        public virtual void GUIInfoCustom()
        {
            AxonGUI.BeginHorizontal();
            LoopStart = AxonGUI.FieldTimeInline(Behavior, "Loop Start", LoopStart);
            LoopEnd = AxonGUI.FieldTimeInline(Behavior, "End", LoopEnd);

            AxonGUI.EndHorizontal();
        }
        #endregion

        #region INSPECTOR

        public virtual void InspectorGUI(string label)
        {
            EditorShowChannel = AxonGUI.FoldoutInline(EditorShowChannel, "");

            IsEnabled = AxonGUI.FieldToggleEnabled(Behavior, IsEnabled, new RectOffset(1, 0, 2, 0));

            if (AxonGUI.ButtonTexture(IsLocked ? AxonUI.LockOnStyle.normal.background : AxonUI.LockOffStyle.normal.background, "Lock the channel to prevent changes in the Timeflow view. Locked channels may not be selected (nor their keyframes).")) {
                UndoUtil.Undo(Behavior, "Lock Channel");
                IsLocked = !IsLocked;
            }
            EditorGUI.BeginDisabledGroup(IsLocked);

            if (!string.IsNullOrEmpty(label)) {
                AxonGUI.LabelInline(label);
            }

            if (HasProperty && ToProperty != null) {
                if (ToProperty.Comp == null && Behavior != null) ToProperty.Comp = Behavior.transform;

                bool multiChannel = true;
                if (ToProperty != null && ToProperty.Name != null && ToProperty.Name.Contains("Scale")) {
                    multiChannel = true;
                }

                bool hasLink = IsLinked;
                if (hasLink) {
                    if (Link.Enabled) {
                        GUI.color = Link.GUIColor;
                        if (AxonGUI.ButtonTexture(AxonUI.ChannelLinkOnStyle.normal.background, "Channel Link On. Click to toggle off.")) {
                            Link.Enabled = false;
                        }
                        GUI.color = AxonColor.Default;
                        Link.DebugEnabled = AxonGUI.FieldToggleDebug(Link.DebugEnabled);
                    }
                    else {
                        if (AxonGUI.ButtonTexture(AxonUI.ChannelLinkOffStyle.normal.background, CanLink ? "Channel Linking not supported by this channel type" : "Channel Link Off. Click to toggle on.")) {
                            if (hasLink) {
                                Link.Enabled = true;
                                Link.DebugEnabled = false;
                            }
                        }
                    }
                }
                else
                if (CanLink) {
                    if (AxonGUI.ButtonTexture(AxonUI.ChannelLinkOffStyle.normal.background, "No Channel Link has been asssigned. To create a new link, use the channel link tool in the Timeflow view.")) {
                    }
                }
                else
                if (AxonGUI.ButtonTexture(AxonUI.ChannelLinkOffStyle.normal.background, "Channel Linking not supported by this channel type")) {
                }

                AxonGUI.SetTooltip("Display name for the channel");
                AxonGUI.UndoName = "Set Channel Name";
                string name = AxonGUI.FieldTextInline(Behavior, Name, GUILayout.Width(100));
                if (name != Name) {
                    Name = name;
                    IsNameCustom = !string.IsNullOrEmpty(Name);
                }

                if (Behavior == null) {
                    AxonGUI.Error("Parent is null!");
                }
                else {
                    Type type = ToProperty != null && ToProperty.Owner != null ? ToProperty.Owner.GetComponentType() : null;
                    AxonGUI.PropertySelect(Behavior, type, Behavior.gameObject, ToProperty, Property.PropertyFilters.All, null, multiChannel, true);
                }
                GUI.color = IsEnabled ? AxonColor.Active : AxonColor.Inactive;
            }
            else {
                InspectorCustomProperty();
            }
            AxonGUI.UndoName = "Set Channel Color";
            GUIColor = AxonGUI.FieldColorInline(Behavior, _GUIColor, false, GUILayout.Width(60));

            EditorGUI.EndDisabledGroup();

            if (TimeflowPreferences.DebugEnabled) {
                Texture2D icon = DebugEnabled ? AxonUI.Icons.DebugOn : AxonUI.Icons.DebugOff;
                Texture2D iconOn = !DebugEnabled ? AxonUI.Icons.DebugOn : AxonUI.Icons.DebugOff;
                if (AxonGUI.ButtonTexture(icon, iconOn, DebugEnabled ? "Disable Debug" : "Enable Debug")) {
                    DebugEnabled = !DebugEnabled;
                }
            }
            if (AxonGUI.ButtonTexture(AxonUI.Icons.More, "Options")) {
                InspectorOptionsMenu();
            }

            GUI.color = AxonColor.Default;
            if (EditorShowChannel) {
                InspectorSettingsGUI();
            }
        }

        /// <summary>
        /// Override to implement an alternative UI for channels that do not use ToProperty
        /// </summary>
        public virtual void InspectorCustomProperty()
        {
            AxonGUI.UndoName = "Set Channel Name";
            Name = AxonGUI.FieldTextInline(Behavior, Name);
        }

        public virtual void InspectorSettingsGUI()
        {
            AxonGUI.EndHorizontal(false); // Assuming it is called from within a row
            AxonGUI.BeginBox();
            AxonGUI.SetLabelWidth(140);

            InspectorChannelLinkGUI();
            InspectorLoopGUI();
            InspectorLimitGUI();
            InspectorSnapGUI();
            InspectorShaderGlobalGUI();
            InspectorChannelHeightGUI();

            if (IsVector && (ToProperty == null || ToProperty.Attribute == -1)) {
                AxonGUI.BeginHorizontalBox();
                AxonGUI.SetTooltip("Draws a 3D path in the editor scene view when gizmos are enabled. Only available for vector property types.");
                AxonGUI.UndoName = "Set Draw Path";
                DrawPath = AxonGUI.FieldToggle(Behavior, "Draw Path", DrawPath);
                AxonGUI.EndHorizontal(false);
            }

            if (SupportsKeyframes) {
                InspectorKeyframesGUI();
            }

            AxonGUI.ResetLabelWidth();
            AxonGUI.EndBox();
            AxonGUI.BeginHorizontal(); // Resume
        }

        public virtual void InspectorChannelHeightGUI()
        {
            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Channel Height";
            AxonGUI.SetTooltip("Enable to use the vector as a color value. This is useful for color channels.");
            AxonGUI.BeginDisabledGroup(GUIHeightLocked);
            GUIHeight = AxonGUI.FieldInt(Behavior, "Channel Height", GUIHeight);
            AxonGUI.EndDisabledGroup();
            GUIHeightLocked = AxonGUI.FieldToggleLock(GUIHeightLocked, "Lock Height");
            AxonGUI.EndHorizontal(false);
        }

        public virtual void InspectorChannelNameGUI()
        {
            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Name";
            string name = AxonGUI.FieldText(Behavior, "Name", Name);
            if (name != Name) {
                Name = name;
                IsNameCustom = !string.IsNullOrEmpty(Name);
            }
            AxonGUI.UndoName = "Set Name Automatic";
            IsNameCustom = !AxonGUI.FieldToggleInline(Behavior, "Auto", !IsNameCustom);
            AxonGUI.EndHorizontal(false);
        }

        public virtual void InspectorChannelLinkGUI()
        {
            AxonGUI.FieldChannelLink(Behavior, this);
        }

        public virtual void InspectorLoopGUI()
        {
            if (IsLoopSupported) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Enable looping a section of time. This is best viewed in the Timeflow window Graph mode.");
                AxonGUI.UndoName = "Set Loop Time";
                EnableLoop = AxonGUI.FieldToggle(Behavior, "Loop Time", EnableLoop);
                if (EnableLoop) {

                    EditorGUI.BeginChangeCheck();
                    if (AxonGUI.ButtonTexture(EnableLoopIn ? AxonUI.ChannelLoopInOnStyle.normal.background : AxonUI.ChannelLoopInOnStyle.active.background,
                        "Repeat loop before start")) {
                        EnableLoopIn = !EnableLoopIn;
                    }
                    EditorGUI.BeginDisabledGroup(EnableAutoLoop);
                    AxonGUI.SetTooltip("Sets the start time of the loop.");
                    AxonGUI.UndoName = "Set Loop Start Time";
                    LoopStart = AxonGUI.FieldTimeInline(Behavior, "Start", LoopStart);


                    AxonGUI.SetTooltip("Sets the end time of the loop. Any kefyrames outside of the start and end time are ignored.");
                    AxonGUI.UndoName = "Set Loop End Time";
                    LoopEnd = AxonGUI.FieldTimeInline(Behavior, "End", LoopEnd);

                    EditorGUI.EndDisabledGroup();
                    AxonGUI.UndoName = "Set Auto Loop";
                    EnableAutoLoop = AxonGUI.FieldToggleInline(Behavior, "Auto", EnableAutoLoop);

                    if (AxonGUI.ButtonTexture(EnableLoopOut ? AxonUI.ChannelLoopOutOnStyle.normal.background : AxonUI.ChannelLoopOutOnStyle.active.background,
                        "Repeat loop after end")) {
                        EnableLoopOut = !EnableLoopOut;
                    }
                    AxonGUI.EndHorizontal(false);
                    AxonGUI.BeginHorizontal();

                    AxonGUI.SetTooltip("The loop section is repeated alternating playing forward and reverse (to and fro).");
                    AxonGUI.UndoName = "Set Loop Ping Pong";
                    LoopPingPong = AxonGUI.FieldToggle(Behavior, "Ping Pong", LoopPingPong);

                    AxonGUI.SetTooltip("This only works for a loop with a start and end keyframe. If enabled, the end keyframe is automatically assigned the same value as the start keyframe to ensure a seemless forward loop.");
                    AxonGUI.UndoName = "Set Loop Match Ends";
                    LoopMatchEnds = AxonGUI.FieldToggleInline(Behavior, "Match Ends", LoopMatchEnds);

                    AxonGUI.SetTooltip("Enable to limit the number of times the loop is repeated, or leave unlimited to repeat forever.");
                    AxonGUI.UndoName = "Set Loop Repeat Limit";
                    LoopLimit = AxonGUI.FieldFloatInline(Behavior, "Repeat Limit", LoopLimit);
                    if (LoopLimit < 0) LoopLimit = 0;

                    if (EditorGUI.EndChangeCheck()) {
                        if (LoopEnd < LoopStart) {
                            (LoopEnd, LoopStart) = (LoopStart, LoopEnd);
                        }
                        if (!LoopMatchEnds) {
                            ClearLoopEnd();
                        }
                        PrepareLoop();
                    }
                }
                AxonGUI.EndHorizontal(false);
                AxonGUI.EndBox();
            }
        }

        public virtual void InspectorLimitGUI()
        {
            if (!IsBool && !IsComponent && !IsGameObject && !IsObject && !IsCustomType) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Enable a minimum and maximum value. This is helpful to prevent keyframe values from going out of accepted ranges.");
                AxonGUI.UndoName = "Set Channel Limit Value";
                LimitValue = AxonGUI.FieldToggle(Behavior, "Limit Value", LimitValue);
                if (LimitValue) {
                    AxonGUI.UndoName = "Set Channel Limit Min";
                    if (IsColor) {
                        MinValue = AxonGUI.FieldColorInline(Behavior, "Min", MinValue, true);
                    }
                    else
                    if (IsVector2) {
                        MinValue = AxonGUI.FieldVector2Inline(Behavior, "Min", MinValue);
                    }
                    else
                    if (IsVector3) {
                        MinValue = AxonGUI.FieldVector3Inline(Behavior, "Min", MinValue);
                    }
                    else
                    if (IsVector4) {
                        MinValue = AxonGUI.FieldVector4Inline(Behavior, "Min", MinValue);
                    }
                    else
                    if (IsInt) {
                        MinValue.x = AxonGUI.FieldIntInline(Behavior, "Min", (int)MinValue.x);
                    }
                    else {
                        MinValue.x = AxonGUI.FieldFloatInline(Behavior, "Min", MinValue.x);
                    }

                    AxonGUI.UndoName = "Set Channel Limit Max";
                    if (IsColor) {
                        MaxValue = AxonGUI.FieldColorInline(Behavior, "Max", MaxValue, true);
                    }
                    else
                    if (IsVector2) {
                        MaxValue = AxonGUI.FieldVector2Inline(Behavior, "Max", MaxValue);
                    }
                    else
                    if (IsVector3) {
                        MaxValue = AxonGUI.FieldVector3Inline(Behavior, "Max", MaxValue);
                    }
                    else
                    if (IsVector4) {
                        MaxValue = AxonGUI.FieldVector4Inline(Behavior, "Max", MaxValue);
                    }
                    else
                    if (IsInt) {
                        MaxValue.x = AxonGUI.FieldIntInline(Behavior, "Max", (int)MaxValue.x);
                    }
                    else {
                        MaxValue.x = AxonGUI.FieldFloatInline(Behavior, "Max", MaxValue.x);
                    }
                }
                AxonGUI.EndHorizontal(false);
                AxonGUI.EndBox();
            }
        }

        public virtual void InspectorSnapGUI()
        {
            if (!IsBool && !IsComponent && !IsGameObject && !IsObject && !IsCustomType) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("If enabled, values are quantized to the nearest snap value increment");
                AxonGUI.UndoName = "Set Channel Value Snap";
                EnableSnap = AxonGUI.FieldToggle(Behavior, "Snap Values", EnableSnap);
                if (EnableSnap) {
                    AxonGUI.UndoName = "Set Channel Snap Incrmenet";
                    SnapIncrement = AxonGUI.FieldFloatInline(Behavior, "Increment", SnapIncrement);
                }
                AxonGUI.EndHorizontal(false);
                AxonGUI.EndBox();
            }
        }

        public virtual void InspectorShaderGlobalGUI()
        {
            if (!IsBool && !IsComponent && !IsGameObject && !IsObject && !IsCustomType) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Shader Global";
                AxonGUI.SetTooltip("Sets a shader value by name, affecting all instances of the shader globally. ");
                SetGlobalShaderProperty = AxonGUI.FieldToggle(Behavior, "Set Shader Global", SetGlobalShaderProperty);
                if (SetGlobalShaderProperty) {
                    if (GlobalShaderProperty == null) GlobalShaderProperty = "";
                    AxonGUI.SetTooltip("To work with Shader Graph, the property must have the same internal name (ex. _EmissionColor) and NOT be exposed as a property in the shader inspector.");
                    AxonGUI.UndoName = "Set Shader Global Property Name";
                    GlobalShaderProperty = AxonGUI.FieldTextInline(Behavior, "Property Name", GlobalShaderProperty);
                }

                AxonGUI.EndHorizontal(false);
                AxonGUI.EndBox();
            }
        }

        public virtual void InspectorKeyframesGUI()
        {
            AxonGUI.BeginBox();
            EditorShowKeys = AxonGUI.Foldout(EditorShowKeys, "Keyframes");
            if (EditorShowKeys) {
                if (Keys.Count == 0) {
                    AxonGUI.HelpBox("No keyframes have been added to this channel yet.", MessageType.Info);
                }
                else {
                    EditorGUI.BeginChangeCheck();

                    int i = 0;
                    int remove = -1;
                    int insert = -1;
                    int displayLimit = 20; // TODO: Implement paging controls for long keyframe lists
                    foreach (Keyframe key in Keys) {
                        key.Channel = this;

                        AxonGUI.BeginHorizontal();
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Key")) {
                            insert = i;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Key")) {
                            remove = i;
                        }
                        AxonGUI.SetLabelWidth(50);

                        AxonGUI.UndoName = "Set Keyframe Enabled";
                        key.IsKeyEnabled = AxonGUI.FieldToggleEnabled(Behavior, key.IsKeyEnabled);

                        AxonGUI.UndoName = "Set Keyframe Lock Time";
                        if (AxonGUI.ButtonLock(key.LockTime, "Lock Time")) {
                            key.LockTime = !key.LockTime;
                        }
                        AxonGUI.BeginDisabledGroup(key.LockTime);
                        AxonGUI.UndoName = "Set Keyframe Time";
                        key.KeyTime = AxonGUI.FieldTimeInline(Behavior, "Time", key.KeyTime, GUILayout.Width(120));
                        AxonGUI.EndDisabledGroup();

                        if (AxonGUI.ButtonLock(key.LockValue, "Lock Value")) {
                            key.LockValue = !key.LockValue;
                        }
                        AxonGUI.BeginDisabledGroup(key.LockValue);
                        if (!IsSingleAttribute && IsVector) {
                            AxonGUI.UndoName = "Set Keyframe Value";
                            if (IsVector2) {
                                key.KeyVector = AxonGUI.FieldVector2Inline(Behavior, "Value", key.KeyVector, GUILayout.Width(120));
                            }
                            else
                            if (IsVector3) {
                                key.KeyVector = AxonGUI.FieldVector3Inline(Behavior, "Value", key.KeyVector, GUILayout.Width(120));
                            }
                            else {
                                key.KeyVector = AxonGUI.FieldVector4Inline(Behavior, "Value", key.KeyVector, GUILayout.Width(120));
                            }
                            if (Interpolation == TimeflowChannel.Interpolations.Bezier && ShowTangents) {
                                AxonGUI.EndHorizontal(false);

                                AxonGUI.BeginHorizontal();
                                AxonGUI.Label(" ", GUILayout.Width(160));
                                AxonGUI.UndoName = "Set Keyframe In Tangent";
                                key.VectorInTangent = AxonGUI.FieldVector3(Behavior, "In ", key.VectorInTangent);
                                AxonGUI.EndHorizontal(false);

                                AxonGUI.BeginHorizontal();
                                AxonGUI.Label(" ", GUILayout.Width(160));
                                AxonGUI.UndoName = "Set Keyframe Out Tangent";
                                key.VectorOutTangent = AxonGUI.FieldVector3(Behavior, "Out ", key.VectorOutTangent);
                                AxonGUI.EndHorizontal(false);

                                AxonGUI.BeginHorizontal();
                                AxonGUI.Label(" ", GUILayout.Width(160));
                                AxonGUI.UndoName = "Set Keyframe Unify Tangents";
                                key.UnifyTangents = AxonGUI.FieldToggle(Behavior, "Unify Tangents", key.UnifyTangents);
                                AxonGUI.EndHorizontal(false);

                                AxonGUI.BeginHorizontal();
                            }
                        }
                        else {
                            Type type = GetDataType();
                            if (IsBool) {
                                bool v = key.KeyValue != 0f;
                                v = AxonGUI.FieldToggleInline(Behavior, "Value", v);
                                key.KeyValue = v ? 1f : 0f;
                            }
                            else
                            if (IsLayerMask) {
                                AxonGUI.UndoName = "Set Keyframe Value";
                                key.KeyValue = MathUtil.Validate((float)AxonGUI.FieldLayerMaskInline(null, "Value", (int)key.KeyValue, GUILayout.Width(120)));
                            }
                            else
                            if (IsInt) {
                                AxonGUI.UndoName = "Set Keyframe Value";
                                key.KeyValue = MathUtil.Validate((float)AxonGUI.FieldIntInline(null, "Value", (int)key.KeyValue, GUILayout.Width(120)));
                            }
                            else
                            if (IsColor) {
                                AxonGUI.UndoName = "Set Keyframe Color";
                                key.KeyColor = MathUtil.Validate(AxonGUI.FieldColorInline(Behavior, "Color", key.KeyColor, false, GUILayout.Width(120)));
                            }
                            else
                            if (IsString) {
                                AxonGUI.UndoName = "Set Keyframe String";
                                key.KeyString = AxonGUI.FieldTextInline(Behavior, "String", key.KeyString, GUILayout.Width(240));
                            }
                            else
                            if (IsComponent) {
                                AxonGUI.UndoName = "Set Keyframe Component";
                                key.KeyComponent = (Component)AxonGUI.FieldObjectInline(Behavior, "Component", key.KeyComponent, type, true, GUILayout.Width(300));
                            }
                            else
                            if (IsGameObject) {
                                AxonGUI.UndoName = "Set Keyframe Game Object";
                                key.KeyGameObject = (GameObject)AxonGUI.FieldObjectInline(Behavior, "Game Object", key.KeyGameObject, type, true, GUILayout.Width(300));
                            }
                            else
                            if (IsObject) {
                                AxonGUI.UndoName = "Set Keyframe Object";
                                key.KeyObject = (UnityEngine.Object)AxonGUI.FieldObjectInline(Behavior, "Object", key.KeyObject, type, true, GUILayout.Width(300));
                            }
                            else {
                                AxonGUI.UndoName = "Set Keyframe Value";
                                key.KeyValue = AxonGUI.FieldFloatInline(Behavior, "Value", key.KeyValue, GUILayout.Width(120));
                            }
                            if (Interpolation == TimeflowChannel.Interpolations.Bezier && ShowTangents) {
                                if (!Timeflow.Active.View.HasFocus || !Timeflow.Active.View.IsGraphMode) {
                                    // This can interfere with keyframe editing so only draw these fields when inspector has focus
                                    AxonGUI.UndoName = "Set Keyframe In Tangent";
                                    key.InTangent = AxonGUI.FieldVector2Inline(Behavior, "In ", key.InTangent);

                                    AxonGUI.UndoName = "Set Keyframe Out Tangent";
                                    key.OutTangent = AxonGUI.FieldVector2Inline(Behavior, "Out ", key.OutTangent);

                                    AxonGUI.UndoName = "Set Keyframe Unify Tangents";
                                    key.UnifyTangents = AxonGUI.FieldToggleInline(Behavior, "Unify Tangents", key.UnifyTangents);
                                }
                                else {
                                    AxonGUI.Info("The inspector window must have focus to show Bezier tangents, otherwise it interferes with the graph view");
                                }
                            }
                        }
                        AxonGUI.EndDisabledGroup();


                        bool isExposed = key.ExposedID != 0;
                        AxonGUI.SetTooltip("Expose a keyframe to set its value remotely via scripting, referring to it by name using Keyframer.SetExposedKeyframe(). Be sure to use a unique name.");
                        AxonGUI.UndoName = "Set Keyframe Exposed";
                        bool ex = AxonGUI.FieldToggleInline(Behavior, "Expose", isExposed);
                        if (ex != isExposed) {
                            if (ex) {
                                key.ExposedID = (int)(UnityEngine.Random.value * 99999f);
                                Keyframe.RegisterExposedKeyframe(key);
                            }
                            else {
                                key.ExposedID = 0;
                                Keyframe.UnregisterExposedKeyframe(key);
                            }
                        }
                        if (ex) {
                            if (key.ExposedID == 0) key.ExposedID = (int)(UnityEngine.Random.value * 99999f);
                            AxonGUI.UndoName = "Set Keyframe Exposed ID";
                            key.ExposedID = AxonGUI.FieldIntInline(null, key.ExposedID);
                        }

                        AxonGUI.EndHorizontal(false);
                        i++;
                        if (i >= displayLimit) break;
                    }
                    if (remove > -1) {
                        UndoUtil.Undo(Behavior, "Remove Key");
                        Keys.RemoveAt(remove);
                    }
                    if (insert > -1) {
                        UndoUtil.Undo(Behavior, "Add Key");
                        Keyframe newKey = new Keyframe(Keys[insert]);
                        Keys.Insert(insert, newKey);
                    }
                    AxonGUI.Space();

                    if (EditorGUI.EndChangeCheck()) {
                        Timeflow.Active.DoUpdate();
                    }
                }
                if (AxonGUI.Button("Set Keyframe Now")) {
                    SetKey(CurrentTime);
                }
            }
            AxonGUI.EndBox();
        }

        #endregion

        #region CONTEXT MENU

        public void InspectorOptionsMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(ToProperty.PathName()), false, null);

            OnInspectorOptionsMenu(menu);

            Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
            menu.DropDown(new GUIRect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
        }

        public virtual void OnInspectorOptionsMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Always Update"), AlwaysUpdate, ToggleAlwaysUpdate);
            menu.AddItem(new GUIContent("Always Show Values"), AlwaysShowValues, ToggleAlwaysShowValues);

            if (SupportsKeyframes) {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Interpolation/None"), Interpolation == TimeflowChannel.Interpolations.None, InterpolationNone);
                menu.AddItem(new GUIContent("Interpolation/Linear"), Interpolation == TimeflowChannel.Interpolations.Linear, InterpolationLinear);
                menu.AddItem(new GUIContent("Interpolation/Bezier"), Interpolation == TimeflowChannel.Interpolations.Bezier, InterpolationBezier);
                menu.AddItem(new GUIContent("Interpolation/Quadratic"), Interpolation == TimeflowChannel.Interpolations.Quadratic, InterpolationQuadratic);

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Add Key"), false, AddKey);
                if (Keys.Count > 1) {
                    menu.AddItem(new GUIContent("Sort Keys"), false, SortKeys);
                }
                else {
                    menu.AddItem(new GUIContent("Sort Keys"), false, null);
                }
                menu.AddItem(new GUIContent("Copy Keys"), false, CopyKeys);

                bool hasCopiedKeys = TimeflowChannel.CopiedKeys != null && TimeflowChannel.CopiedKeys.Count > 0;
                if (hasCopiedKeys) {
                    menu.AddItem(new GUIContent("Paste Keys"), false, PasteKeys);
                    menu.AddItem(new GUIContent("Merge Keys"), false, MergeKeys);
                }
                else {
                    menu.AddItem(new GUIContent("Paste Keys"), false, null);
                    menu.AddItem(new GUIContent("Merge Keys"), false, null);
                }

                menu.AddSeparator("");
                if (Interpolation == TimeflowChannel.Interpolations.Bezier) {
                    menu.AddItem(new GUIContent("Show Bezier Tangents"), ShowTangents, ToggleShowTangents);
                }
                else {
                    menu.AddItem(new GUIContent("Show Bezier Tangents"), ShowTangents, null);
                }
            }

            if (IsLoopSupported) {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Enable Loop"), EnableLoop, ToggleLoop);
            }
        }

        public void ToggleAlwaysUpdate()
        {
            AlwaysUpdate = !AlwaysUpdate;
        }

        public void ToggleAlwaysShowValues()
        {
            AlwaysShowValues = !AlwaysShowValues;
        }

        public void ToggleLoop()
        {
            EnableLoop = !EnableLoop;
        }

        public void InterpolateMode(TimeflowChannel.Interpolations mode)
        {
            Interpolation = mode;
        }

        public void InterpolationNone()
        {
            Interpolation = TimeflowChannel.Interpolations.None;
        }

        public void InterpolationLinear()
        {
            Interpolation = TimeflowChannel.Interpolations.Linear;
        }

        public void InterpolationBezier()
        {
            Interpolation = TimeflowChannel.Interpolations.Bezier;
        }

        public void InterpolationQuadratic()
        {
            Interpolation = TimeflowChannel.Interpolations.Quadratic;
        }

        public void AddKey()
        {
            AddKey(CurrentTime);
        }

        public Keyframe AddKey(float atTime) => AddKey(atTime, false);

        public Keyframe AddKey(float atTime, bool enforceUnique)
        {
            Undo.IncrementCurrentGroup();
            UndoUtil.Undo(Behavior, "Add Key", true);
            if (IsTrack) {
                return SetKey(atTime, atTime + 1f, true, enforceUnique);
            }
            else {
                return SetKey(atTime, enforceUnique);
            }
        }

        public void SortKeys() => SortKeys(true);

        public void SortKeys(bool undoable)
        {
            if (undoable) UndoUtil.Undo(Behavior, "Sort Keys");
            SortBy(TimeflowChannel.SortingModes.TimeAsc);
        }

        public void ToggleExportEveryFrame()
        {
            ExportEveryFrame = !ExportEveryFrame;
        }

        public void ToggleShowTangents()
        {
            ShowTangents = !ShowTangents;
        }

        public virtual bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) {
                if (IsTrack) Debug.LogWarning($"{PathName} Cannot combine or separate attributes of a track channel");
                else
                if (AttributeCount <= 1) Debug.LogWarning($"{PathName} Cannot combine or separate attributes of a singular value");
                else
                if (ToProperty == null) Debug.LogWarning($"{PathName} No property value has not been assigned to this channel and cannot be combined or separated.");
                else
                if (Behavior == null) Debug.LogWarning($"{PathName} The parent behavior is null");
            }
            return !IsTrack && AttributeCount > 1 && ToProperty != null && Behavior != null;
        }

        public virtual void SeparateChannel()
        {
            //if (DebugEnabled) Debug.Log($"{PathName}.SeparateChannel");
            if (!CanSeparateOrCombineChannel(true)) {
                Debug.LogWarning($"{PathName} Cannot separate into multiple channels");
                return;
            }
            // turn off auto keyframing until finished with setup
            bool autoKeyframing = Timeflow.IsAutoKeyframingEnabled;
            Timeflow.IsAutoKeyframingEnabled = false;

            UndoUtil.Undo(Behavior, "Separate Combined Channel", true);
            List<TimeflowChannel> newChannels = new List<TimeflowChannel>();
            for (int i = 0; i < AttributeCount; i++) {
                // Check for an existing channel first
                TimeflowChannel newChannel = Behavior.GetChannel(ToProperty.Name, i);
                if (newChannel == null) newChannel = new TimeflowChannel(Behavior);

                newChannel.ClearKeys();
                newChannel.Copy(this);
                newChannel.Attribute = i;
                newChannel.IsCombinedValue = false;

                if (newChannel.ToProperty != null) {
                    newChannel.ToProperty.Attribute = i;
                    newChannel.ToProperty.ForcePropertyType = Property.PropertyTypes.Auto;
                    newChannel.Name = Name + " " + Property.GetAttributeName(ToProperty.PropertyType, i);
                    newChannel.ToProperty.ForcePropertyType = PropertyType;
                }
                newChannel.KeyPropertyType = Property.PropertyTypes.Float;

                Behavior.AddChannel(newChannel);

                if (i == 0) {
                    newChannel.GUIColor = AxonColor.RedChannel;
                }
                else
                if (i == 1) {
                    newChannel.GUIColor = AxonColor.GreenChannel;
                }
                else
                if (i == 2) {
                    newChannel.GUIColor = AxonColor.BlueChannel;
                }
                else
                if (i == 3) {
                    newChannel.GUIColor = AxonColor.AlphaChannel;
                }
                newChannels.Add(newChannel);

                newChannel.Select();
            }

            if (Keys != null && Keys.Count > 0) {
                int i = 0;
                foreach (TimeflowChannel ch in newChannels) {
                    int k = 0;
                    foreach (Keyframe key in Keys) {
                        ch.Keys[k].KeyValue = key.KeyVector[i];
                        k++;
                    }
                    i++;
                }
            }

            TimeflowObject obj = Object;

            Behavior.SortAlphabetically();

            // Delete the original channel to avoid conflicts
            Delete();

            obj.Refresh();

            if (Timeflow.Active != null) {
                Timeflow.Active.View.SelectObject(obj);

                // Force the view to rebuild the list order
                Timeflow.Active.Refresh();
            }
            Timeflow.IsAutoKeyframingEnabled = autoKeyframing;
        }

        public static TimeflowChannel CombineChannels(params TimeflowChannel[] channelsToCombine)
        {
            //Debug.Log($"TimeflowChannel.CombineChannels");
            if (channelsToCombine == null || channelsToCombine.Length == 0) {
                Debug.LogWarning("No channels provided to combine");
                return null;
            }

            List<TimeflowChannel> channels = new List<TimeflowChannel>();
            foreach (TimeflowChannel ch in channelsToCombine) {
                if (ch == null) continue;
                channels.Add(ch);
            }
            if (channels == null || channels.Count == 0) {
                Debug.LogWarning("No valid channels provided to combine");
                return null;
            }

            // turn off auto keyframing until finished with setup
            bool autoKeyframing = Timeflow.IsAutoKeyframingEnabled;
            Timeflow.IsAutoKeyframingEnabled = false;

            TimeflowChannel channel = channels[0];
            if (!channel.CanSeparateOrCombineChannel(true)) {
                return null;
            }

            UndoUtil.Undo(channel.Behavior, "Combine Separate Channels", true);

            // Check for an existing channel first
            TimeflowChannel combined = channel.Behavior.GetChannel(channel.ToProperty.Name, -1);

            // If null, create a new channel for the combined value
            if (combined == null) combined = new TimeflowChannel(channel.Behavior);

            // Copy the data from the first channel provided
            combined.ClearKeys();
            combined.Copy(channel);
            combined.Attribute = -1;
            combined.IsCombinedValue = true;

            if (combined.ToProperty != null) {
                // Setup the property for combined attributes
                combined.ToProperty.Attribute = -1;
                combined.ToProperty.ForcePropertyType = Property.PropertyTypes.Auto;
                combined.ToProperty.ForcePropertyType = channel.PropertyType;
                combined.ResetName();
            }
            combined.KeyPropertyType = Property.PropertyTypes.Auto;

            List<float> keyTimes = new List<float>();
            // Loop through the channels and build a list of all key times
            foreach (TimeflowChannel ch in channels) {
                if (ch.Keys == null || ch.Keys.Count == 0) continue;
                foreach (Keyframe k in ch.Keys) {
                    if (!keyTimes.Contains(k.KeyTime)) {
                        keyTimes.Add(k.KeyTime);
                    }
                }
            }

            // Insert keys in the channels where there are any gaps in time
            // to ensure merging the channels goes smoothly
            foreach (float time in keyTimes) {
                foreach (TimeflowChannel ch in channels) {
                    if (ch.Keys == null || ch.Keys.Count == 0) continue;

                    if (!ch.IsKeySet(time)) {
                        ch.SetKey(time);
                    }
                }
            }

            // Loop through the channels to copy and merge keyframes
            foreach (TimeflowChannel ch in channels) {
                combined.CopyKeyframes(ch, false, true);

                // Delete the original channels to avoid conflicts
                ch.Delete();
            }

            // Make sure the new channel gets parented
            channel.Behavior.AddChannel(combined);

            TimeflowObject obj = channel.Object;
            channel.Behavior.SortAlphabetically();

            // Delete the original channel
            channel.Delete();

            obj.Refresh();

            // Select the new channel
            if (Timeflow.Active != null) {
                Timeflow.Active.View.SelectObject(obj);

                // Force the view to rebuild the list order
                Timeflow.Active.Refresh();
            }
            Timeflow.IsAutoKeyframingEnabled = autoKeyframing;
            combined.Select();

            return combined;
        }

        #endregion

        #region TOOLS

        public float GetSlope(float tanX, float tanY)
        {
            float slope = 0;
            if (tanY != 0 && tanX != 0) slope = tanY / tanX;
            return Mathf.Clamp(slope, -_MaxKeyframeSlopeExport, _MaxKeyframeSlopeExport);
        }

        public Vector2 GetTangents(float slope, float weight, bool isLeft)
        {
            float tanX = isLeft ? -0.5f : 0.5f;
            float tanY = slope * tanX;
            return new Vector2(tanX, tanY) * weight;
        }

        public void ImportAnimationCurve(AnimationCurve curve)
        {
            if (curve == null) {
                Debug.LogWarning("ImportAnimationCurve: curve is null");
                return;
            }

            UndoUtil.Undo(Behavior, "Import Animation Curve", true);

            // Clear the channel before importing
            ClearKeys();

            int i = 0;
            foreach (UnityEngine.Keyframe key in curve.keys) {
                Keyframe k = new Keyframe(this, CurrentTime + key.time, key.value);
                KeysAdd(k);

                k.IsAutoTangents = false;
                k.UnifyTangents = key.inTangent == key.outTangent;
                k.UnifyTangentLengths = false;

                AnimationUtility.TangentMode leftMode = AnimationUtility.GetKeyLeftTangentMode(curve, i);
                AnimationUtility.TangentMode rightMode = AnimationUtility.GetKeyRightTangentMode(curve, i);

                bool calcLeft = true;
                bool calcRight = true;
                if (rightMode == AnimationUtility.TangentMode.Constant) {
                    k.Hold = true;
                    calcLeft = calcRight = false;
                }
                else
                if (leftMode == AnimationUtility.TangentMode.Linear && rightMode == AnimationUtility.TangentMode.Linear) {
                    k.Linear = true;
                    calcLeft = calcRight = false;
                }
                else
                if (leftMode == AnimationUtility.TangentMode.Linear) {
                    k.InTangent = Vector2.zero;
                    calcLeft = false;
                }
                else
                if (rightMode == AnimationUtility.TangentMode.Linear) {
                    k.OutTangent = Vector2.zero;
                    calcRight = false;
                }
                if (calcLeft || calcRight) {

                    if (calcLeft && i > 0) {
                        Vector2 inTan = GetTangents(key.inTangent, key.inWeight, true);
                        UnityEngine.Keyframe prev = curve.keys[i - 1];
                        float dif = key.time - prev.time;
                        inTan = inTan * dif;
                        k.SetInTangent(inTan);
                    }

                    if (calcRight && i < curve.keys.Length - 1) {
                        Vector2 outTan = GetTangents(key.outTangent, key.outWeight, false);
                        UnityEngine.Keyframe next = curve.keys[i + 1];
                        float dif = next.time - key.time;
                        Vector2 outTanA = outTan;
                        outTan *= dif;
                        k.SetOutTangent(outTan);
                    }
                }

                i++;
            }
        }

        public AnimationCurve ExportAnimationCurve()
        {
            AnimationCurve curve = new AnimationCurve();

            int i = 0;
            foreach (Keyframe key in Keys) {
                UnityEngine.Keyframe k = new UnityEngine.Keyframe();
                k.time = key.KeyTime;
                k.value = key.KeyValue;

                float inTanX = Mathf.Max(Mathf.Abs(key.InTangent.x), _MinKeyframeTangentExport);
                float outTanX = Mathf.Max(Mathf.Abs(key.OutTangent.x), _MinKeyframeTangentExport);
                inTanX *= -1f;

                float inSlope = GetSlope(inTanX, key.InTangent.y);
                float outSlope = GetSlope(outTanX, key.OutTangent.y);

                k.inTangent = inSlope;
                k.outTangent = outSlope;
                k.weightedMode = WeightedMode.Both;

                // In Timeflow, Bezier tangents are calculated a bit differently than Unity's AnimationCurve.
                // The weight in Timeflow is 1/2 the weight of the tangent in an animation curve for the same
                // visual result.
                if (i > 0) {
                    Keyframe prev = Keys[i - 1];
                    float dif = key.KeyTime - prev.KeyTime;
                    k.inWeight = 2f * (dif == 0 ? 0 : Mathf.Abs(inTanX) / dif);
                }

                if (i == Keys.Count - 1) {
                    k.outWeight = k.inWeight;
                    outSlope = inSlope;
                }
                else {
                    Keyframe next = Keys[i + 1];
                    float dif = next.KeyTime - key.KeyTime;
                    k.outWeight = 2f * (dif == 0 ? 0 : outTanX / dif);
                }
                if (i == 0) {
                    k.inWeight = k.outWeight;
                    inSlope = outSlope;
                }

                AnimationCurveUtil.SetKeyframe(curve, k);
                AnimationCurveUtil.SetKeyframe(curve, k);

                AnimationUtility.SetKeyBroken(curve, i, !key.UnifyTangents);
                if (key.Linear) {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }
                else
                if (key.Hold) {
                    // Hold keyframes in Timeflow only apply to the right tangent
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Free);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                }
                else {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Free);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Free);
                }

                i++;
            }

            return curve;
        }

        public void ImportCurve(Curve curve)
        {
            if (curve == null) {
                Debug.LogWarning("ImportCurve: curve is null");
                return;
            }

            UndoUtil.Undo(Behavior, "Import Curve", true);

            ClearKeys();

            foreach (Keyframe key in curve.Keys) {
                Keyframe nkey = new Keyframe();
                nkey.Copy(in key, this);
                KeysAdd(nkey);
            }
        }

        public Curve ExportCurve()
        {
            Curve curve = new Curve();

            foreach (Keyframe key in Keys) {
                Keyframe nkey = new Keyframe();
                nkey.Copy(in key, null);
                curve.Keys.Add(nkey);
            }

            return curve;
        }

        #endregion
    }

    public class SortTimeflowChannelInViewOrder : IComparer<TimeflowChannel>
    {
        public int Compare(TimeflowChannel a, TimeflowChannel b)
        {
            int c = 0;
            if (a == null) return 1;
            if (b == null) return -1;
            if (a.SortOrderInView < b.SortOrderInView) {
                c = -1;
            }
            else
            if (a.SortOrderInView > b.SortOrderInView) {
                c = 1;
            }
            return c;
        }
    }

}//AxonGenesis

#endif
