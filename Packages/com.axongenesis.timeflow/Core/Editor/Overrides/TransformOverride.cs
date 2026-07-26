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

    [CustomEditor(typeof(Transform))]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/overrides/transform-override")]
    public partial class TransformOverride : Editor
    {
        private RectOffset _ResetMargin;
        private RectOffset _CopyMargin;
        private RectOffset _FieldMargin;
        private RectOffset _KeyframeMargin;
        private RectOffset _LockMargin;
        private RectOffset _PrevNextKeyMargin;
        private RectOffset _EnableMargin;

        private TransformOverrideData _Data = null;
        private float currentTime = 0f;
        private bool hasTimeChanged = false;
        private bool isRefreshNeeded = false;
        private bool isNarrow = false;
        private bool isWide = false;
        private GameObject addObjectToTimeflow = null;
        private Vector2 IconSize = new Vector2(14, 14);
        private bool AnyCombined = false;
        private bool AnySeparated = false;
        private bool AnySeparateAnimated = false;

        public bool IsEditingMultiple => targets != null && targets.Length > 1;

        private void OnUndo()
        {
            isRefreshNeeded = true;
        }

        #region SETUP

        private void SetupTimeflow()
        {
            if (Timeflow.Active == null) {
                Timeflow.CreateNewTimeflow();
            }
        }

        private void RemoveFromTimeflow()
        {
            if (_Data == null || _Data.TimeflowObject == null) return;
            if (EditorUtil.ShowDialog("Remove All Timeflow Behaviors?", "This will remove all Timeflow animations and behaviors from the object", "Remove", "Cancel")) {
                UndoUtil.Undo(_Data.Transform.gameObject, "Remove All Timeflow Behaviors");
                _Data.TimeflowObject.RemoveAllTimeflowBehaviors();
                _Data.TimeflowObject = null;
            }
        }

        private void AddToTimeflowButton()
        {
            if (_Data == null) return;
            if (!_Data.IsTimeflowObject) {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.BehaviorOff, "Animate in Timeflow", _EnableMargin, IconSize)) {
                    AddToTimeflow();
                }
            }
        }

        private void AddToTimeflow()
        {
            if (_Data == null) return;
            SetupTimeflow();
            TimeflowObject obj = Timeflow.SetupTimeflowObject(_Data.Transform.gameObject, true);
            obj.OverrideData = new TransformOverrideData(_Data);
            isRefreshNeeded = true;
        }

        private void Setup()
        {
            AxonGUI.Setup(120);

            Instance = this;

            _ResetMargin = new RectOffset(1, 0, 2, 0);
            _CopyMargin = new RectOffset(1, 0, 4, 0);
            _FieldMargin = new RectOffset(0, 0, 2, 0);
            _KeyframeMargin = new RectOffset(0, 0, 2, 0);
            _LockMargin = new RectOffset(1, 0, 3, 0);
            _PrevNextKeyMargin = new RectOffset(0, 0, 3, 0);
            _EnableMargin = new RectOffset(0, 0, 3, 0);

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;

            isNarrow = EditorGUIUtility.currentViewWidth < _split1value;
            isWide = EditorGUIUtility.currentViewWidth > _split2value;

            bool refresh = PivotMode != Tools.pivotRotation;

            Transform transform = (Transform)target;
            if (_Data == null) {
                refresh = SetupData(transform);
            }
            if (_Data.Transform != transform) {
                _Data.Transform = transform;
                refresh = true;
            }
            if (_Data.WantsRefresh) {
                _Data.Refresh();
            }
            if (refresh || isRefreshNeeded) {
                Refresh();
            }
        }

        private bool SetupData(Transform transform)
        {
            bool refresh;
            AxonGUI.Setup();

            TimeflowObject obj;
            // Check whether the object has been added to Timeflow
            if (transform.TryGetComponent<TimeflowObject>(out obj)) {
                // Use the TimeflowObject to store the editor settings
                if (obj.OverrideData == null) {
                    obj.OverrideData = new TransformOverrideData(transform);
                }
                _Data = obj.OverrideData;
            }
            else {
                // Creates temporary data that isn't serialized to the scene
                if (_SharedData == null) _SharedData = new TransformOverrideData((Transform)target);
                _Data = _SharedData;
            }

            _Data.Position.HasAnimationConflict = false;
            _Data.Rotation.HasAnimationConflict = false;
            _Data.Scale.HasAnimationConflict = false;

            Flyby flyby;
            if (transform.TryGetComponent<Flyby>(out flyby)) {
                _Data.Position.HasAnimationConflict = flyby.Enabled;
                _Data.Rotation.HasAnimationConflict = flyby.Enabled && flyby.ApplyRotation;
            }

            AutoBank autoBank;
            if (transform.TryGetComponent<AutoBank>(out autoBank)) {
                _Data.Rotation.HasAnimationConflict = autoBank.Enabled;
            }

            AutoRotate autoRotate;
            if (transform.TryGetComponent<AutoRotate>(out autoRotate)) {
                _Data.Rotation.HasAnimationConflict = autoRotate.Enabled;
            }

            refresh = true;
            _Data.Transform = transform;
            _Data.Refresh();
            return refresh;
        }

        public void Refresh()
        {
            isRefreshNeeded = false;
            if (_Data.Position != null) _Data.Position.RecalculateSliderMinMax = true;
            if (_Data.Rotation != null) _Data.Rotation.RecalculateSliderMinMax = true;
            if (_Data.Scale != null) _Data.Scale.RecalculateSliderMinMax = true;
            PivotMode = Tools.pivotRotation;
            _Data.Refresh();
        }

        #endregion

        #region GUI
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private void HeaderGUI()
        {
            AxonGUI.BeginVertical(AxonUI.HeaderStyle);
            AxonGUI.BeginHorizontal();

            string label = "Transform";

            bool locked = AxonGUI.FieldToggleLock(_Data.IsLocked, "Lock to prevent changes", IconSize);
            if (locked != _Data.IsLocked) {
                _Data.IsLocked = locked;
                if (Event.current != null && Event.current.control) {
                    _Data.Position.IsLocked = _Data.Position.IsLockedX = _Data.Position.IsLockedY = _Data.Position.IsLockedZ = locked;
                    _Data.Rotation.IsLocked = _Data.Rotation.IsLockedX = _Data.Rotation.IsLockedY = _Data.Rotation.IsLockedZ = locked;
                    _Data.Scale.IsLocked = _Data.Scale.IsLockedX = _Data.Scale.IsLockedY = _Data.Scale.IsLockedZ = locked;
                }
            }
            if (ShowResetCopyPaste || (Event.current != null && Event.current.shift)) {
                bool changed = false;
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Reset, "Reset " + label, _ResetMargin)) {
                    UndoUtil.Undo(_Data.Transform, "Reset " + label);
                    ObjectUtil.ResetTransform(_Data.Transform);
                    if (_Data.Rotator != null) _Data.Rotator.Euler = Vector3.zero;
                    changed = true;
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PropertyCopy, "Copy " + label, _CopyMargin)) {
                    Debug.Log("Copied " + label);//--KEEP
                    AxonTools.CopyTransform(_Data.Transform);
                }
                AxonGUI.BeginDisabledGroup(!AxonTools.HasCopy);
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PropertyPaste, "Paste " + label, _FieldMargin)) {
                    Debug.Log("Pasted " + label);//--KEEP
                    UndoUtil.Undo(_Data.Transform, "Paste " + label);
                    AxonTools.PasteTransform(_Data.Transform);
                    changed = true;
                }
                AxonGUI.EndDisabledGroup();

                if (changed) {
                    Vector3 pos = IsLocal ? _Data.Transform.localPosition : _Data.Transform.position;
                    OnValueChangedAttribute(_Data.Position, 1, pos);
                    if (!_Data.Position.ShowCombined) {
                        OnValueChangedAttribute(_Data.Position, 2, pos);
                        OnValueChangedAttribute(_Data.Position, 3, pos);
                    }

                    Vector3 rot = IsLocal ? _Data.Transform.localEulerAngles : _Data.Transform.eulerAngles;
                    OnValueChangedAttribute(_Data.Rotation, 1, rot);
                    if (!_Data.Rotation.ShowCombined) {
                        OnValueChangedAttribute(_Data.Rotation, 2, rot);
                        OnValueChangedAttribute(_Data.Rotation, 3, rot);
                    }

                    Vector3 scale = _Data.Transform.localScale;//IsLocal ? _Data.Transform.localScale : _Data.Transform.eulerAngles;
                    OnValueChangedAttribute(_Data.Scale, 1, scale);
                    if (!_Data.Scale.ShowCombined) {
                        OnValueChangedAttribute(_Data.Scale, 2, scale);
                        OnValueChangedAttribute(_Data.Scale, 3, scale);
                    }
                }
            }

            if (AxonGUI.ButtonTexture(_Data.IsLocal ? AxonUI.Icons.Local : AxonUI.Icons.Global, "Toggle between local and global coordinates", new RectOffset(2, 0, 2, 0))) {
                _Data.IsLocal = !_Data.IsLocal;
            }

            AxonGUI.LabelInline(_Data.IsLocal ? "Local" : "Global");
            if (!_Data.IsLocal) {
                AxonGUI.Info("Global values are calculated and therefore result in unavoidable roundering errors. " +
                    "Viewing and setting global values is offered as a convenience, but for precision use local coordinates.");
            }

            AxonGUI.BeginDisabledGroup(_Data.IsLocked);
            if (AxonGUI.ButtonTexture(Timeflow.IsAutoKeyframingEnabled ? AxonUI.Icons.AutoKeyframingOn : AxonUI.Icons.AutoKeyframingOff, "Auto Keyframing", new RectOffset(0, 0, -2, 0))) {
                if (Timeflow.Active == null) {
                    UnityEngine.Object[] selection = Selection.objects;
                    Timeflow.CreateNewTimeflow();
                    Timeflow.Active.AutoKeyframingEnabled = true;
                    SelectionUtil.Select(selection);
                    addObjectToTimeflow = _Data.Transform.gameObject; // Defer adding until view has initialized
                }
                else {
                    Timeflow.IsAutoKeyframingEnabled = !Timeflow.IsAutoKeyframingEnabled;
                }
            }
            if (_Data.IsTimeflowObject) {
                if (_Data.TimeflowObject == null || Timeflow.Active == null) {
                    _Data.IsTimeflowObject = false;
                }
                else {
                    float t = _Data.TimeflowObject.CurrentTime;
                    AxonGUI.BeginChangeCheck();
                    float t2 = AxonGUI.FieldTimeInline(_Data.TimeflowObject, "Time", t);
                    if (AxonGUI.EndChangeCheck() && t2 != t) {
                        if (_Data.IsLocal) {
                            float offset = _Data.TimeflowObject.TimeOffsetWorld;
                            float worldTime = t2 + offset;
                            Timeflow.Active.SetTime(worldTime);
                        }
                        else {
                            Timeflow.Active.CurrentTime = t2;
                        }
                    }
                }
            }
            AxonGUI.FlexibleSpace();
            if (_Data.IsTimeflowObject) {
                AxonGUI.SetTooltip("Shortcut Alt+~ : Adds the object to the Timeflow view. Hold the Control key to remove");
                if (Event.current != null && Event.current.control) {
                    GUI.color = AxonColor.Error;
                    AxonGUI.SetTooltip("Removes all of the Timeflow behaviors from the object.");
                    if (AxonGUI.ButtonInline("Remove Timeflow")) {
                        Timeflow.RemoveTimeflowObjects();
                    }
                    GUI.color = AxonColor.Default;
                }
                else
                if (Timeflow.Active != null) {
                    bool isDisplayed = Timeflow.Active.Display.IsObjectDisplayed(_Data.Transform.gameObject);
                    bool isHidden = isDisplayed && _Data.TimeflowObject.DisplayChannels == false &&
                        Timeflow.Active.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Displayed;
                    GUI.color = _Data.TimeflowObject.DisplaySolo ? AxonColor.Solo : AxonColor.Default;
                    string tooltip = _Data.TimeflowObject.DisplaySolo ? "Solo is On" : "Solo is Off";
                    if (AxonGUI.ButtonTexture(_Data.TimeflowObject.DisplaySolo ? AxonUI.Icons.ChannelLinkOn : AxonUI.Icons.ChannelLinkOff, tooltip, new RectOffset(-2, 0, 2, 0))) {
                        bool replace = Timeflow.Active.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Solo && !_Data.TimeflowObject.DisplaySolo;
                        if (!isDisplayed) {
                            replace = true;
                            TimeflowObject obj = Timeflow.SetupTimeflowObject(_Data.Transform.gameObject, true);
                        }
                        Timeflow.Active.Display.SoloObjectToggle(_Data.TimeflowObject, replace);
                    }
                    GUI.color = AxonColor.Default;
                    if (isDisplayed) {
                        if (isHidden) {
                            AxonGUI.SetTooltip("Shows the object from the Timeflow view.");
                            if (AxonGUI.ButtonInline("Show in Timeflow")) {
                                _Data.TimeflowObject.DisplayChannels = true;
                                Timeflow.Active.Display.ChannelMode = TimeflowViewDisplay.ChannelModes.None;
                                Timeflow.Active.Refresh();
                            }
                        }
                        else {
                            AxonGUI.SetTooltip("Removes or hides the object from the Timeflow view.");
                            if (AxonGUI.ButtonInline("Hide in Timeflow")) {
                                Timeflow.Active.Display.SetUserControlledPreserveView();
                                if (_Data.TimeflowObject.DisplaySolo) {
                                    Timeflow.Active.Display.SoloObjectToggle(_Data.TimeflowObject);
                                }
                                Timeflow.Active.Display.RemoveObjectFromDisplayRecursive(_Data.Transform.gameObject);

                                if (Timeflow.Active.Display.IsObjectOrParentDisplayed(_Data.Transform.gameObject)) {
                                    // If the object is displayed anyway because its in a group, then hide it using the display filter
                                    _Data.TimeflowObject.DisplayChannels = false;
                                    Timeflow.Active.Display.ChannelMode = TimeflowViewDisplay.ChannelModes.Displayed;
                                }
                                Timeflow.Active.Refresh();
                            }
                        }
                    }
                    else {
                        AxonGUI.SetTooltip("Adds the object to the Timeflow view.");
                        if (AxonGUI.ButtonInline("Show in Timeflow")) {
                            Timeflow.Active.Display.SetUserControlledPreserveView();
                            TimeflowObject obj = Timeflow.SetupTimeflowObject(_Data.Transform.gameObject, true);
                        }
                    }
                }
            }
            else {
                AxonGUI.SetTooltip("Shortcut Alt+~ : Adds the object to the Timeflow view and performs a one-time setup.");
                if (AxonGUI.ButtonInline("+ Animate")) {
                    if (Timeflow.Active == null) Timeflow.CreateNewTimeflow();
                    addObjectToTimeflow = _Data.Transform.gameObject; // Defer adding until view has initialized
                }
            }
            if (AxonGUI.ButtonTexture(AxonUI.Icons.SettingsOff, "Transform Override Settings", new RectOffset(1, 0, 1, 0))) {
                GenericMenu m = new GenericMenu();

                if (_Data.IsTimeflowObject) {
                    m.AddItem(new GUIContent("Remove this object from Timeflow"), false, RemoveFromTimeflow);
                    m.AddItem(new GUIContent("Show Channels in Color"), ShowInColor, () => {
                        ShowInColor = !ShowInColor;
                    });
                }
                else {
                    m.AddItem(new GUIContent("Add this object to Timeflow"), false, AddToTimeflow);
                }
                m.AddItem(new GUIContent("Show Reset-Copy-Paste (Shift)"), ShowResetCopyPaste, () => {
                    ShowResetCopyPaste = !ShowResetCopyPaste;
                });
                m.AddItem(new GUIContent("Shorten Labels"), ShortLabels, () => {
                    ShortLabels = !ShortLabels;
                });

                m.AddSeparator("");
                m.AddItem(new GUIContent("Disable Overrides"), false, () => {
                    TimeflowEditorOverrides.EnableOverrides(false);
                });
                Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
                m.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
            }

            AxonGUI.EndDisabledGroup();

            AxonGUI.EndHorizontal();
            AxonGUI.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            Setup();
            HeaderGUI();

            AxonGUI.BeginDisabledGroup(_Data.IsLocked);
            TimeflowGUI();
            AxonGUI.EndDisabledGroup();

            if (GUI.changed) {
                if (_Data.IsUniformScale) {
                    float v = _Data.Transform.localScale.x;
                    _Data.Transform.localScale = new Vector3(v, v, v);
                }
                EditorUtil.SetDirty(_Data.Transform);
            }
            if (addObjectToTimeflow != null && Timeflow.Active != null && Timeflow.Active.Display != null) {
                Timeflow.Active.Display.ObjectMode = TimeflowViewDisplay.ObjectModes.UserControlled;
                TimeflowObject obj = Timeflow.SetupTimeflowObject(addObjectToTimeflow, true);
                SelectionUtil.Select(addObjectToTimeflow);
                addObjectToTimeflow = null;
            }
        }

        private void TimeflowGUI()
        {
            AxonGUI.SetLabelWidth(30);

            if (Timeflow.Active != null) {
                hasTimeChanged = currentTime != Timeflow.Active.CurrentTime;
                if (hasTimeChanged) currentTime = Timeflow.Active.CurrentTime;
            }

            HasAnyConflicts = _Data.Position.HasAnimationConflict || _Data.Rotation.HasAnimationConflict || _Data.Scale.HasAnimationConflict;

            AnyCombined = _Data.Position.ShowCombined || _Data.Rotation.ShowCombined || _Data.Scale.ShowCombined;
            AnySeparated = !_Data.Position.ShowCombined || !_Data.Rotation.ShowCombined || !_Data.Scale.ShowCombined;
            AnySeparateAnimated = (!_Data.Position.ShowCombined && _Data.Position.HasAnimation) ||
                (!_Data.Rotation.ShowCombined && _Data.Rotation.HasAnimation) ||
                (!_Data.Scale.ShowCombined && _Data.Scale.HasAnimation);

            AxonGUI.BeginVertical(AxonUI.HeaderStyle);
            PositionGroup();
            RotationGroup();
            ScaleGroup();
            AxonGUI.EndVertical();

            AxonGUI.RestoreLabelWidth();
            hasTimeChanged = false;
        }

        private void PositionGroup()
        {
            AxonGUI.BeginChangeCheck();
            Vector3 p;
            if (IsLocal) {
                p = _Data.Transform.localPosition;
                GroupGUI(_Data.Position, ref p);
            }
            else {
                p = _Data.Transform.position;
                GroupGUI(_Data.Position, ref p);
            }
            if (AxonGUI.EndChangeCheck()) {
                if (IsLocal) {
                    if (p != _Data.Transform.localPosition) {
                        _Data.Transform.localPosition = p;
                    }
                }
                else {
                    if (p != _Data.Transform.position) {
                        _Data.Transform.position = p;
                    }
                }
            }
        }

        private void RotationGroup()
        {
            Vector3 r;
            if (_Data.Rotator != null) {
                _Data.Rotator.IsWorldSpace = !IsLocal;
                r = _Data.Rotator.Euler;
            }
            else {
                // Use intermidiate var to stabilize Euler rotations
                if (IsLocal) {
                    r = _Data.EulerAngles;
                }
                else {
                    r = _Data.Transform.eulerAngles;
                }
            }

            AxonGUI.BeginChangeCheck();

            GroupGUI(_Data.Rotation, ref r);

            if (AxonGUI.EndChangeCheck()) {
                if (_Data.Rotator != null) {
                    _Data.Rotator.Euler = r;
                }
                else {
                    _Data.EulerAngles = r;
                    if (IsLocal) {
                        _Data.Transform.localEulerAngles = r;
                    }
                    else {
                        _Data.Transform.eulerAngles = r;
                    }
                }
            }
        }

        private void ScaleGroup()
        {
            Vector3 v = IsLocal ? _Data.Transform.localScale : _Data.Transform.lossyScale;
            Vector3 v2 = v;
            AxonGUI.BeginChangeCheck();

            GroupGUI(_Data.Scale, ref v2);

            if (AxonGUI.EndChangeCheck()) {
                Transform parent = null;
                if (!IsLocal) {
                    // There is no way to directly set world scale except to unparent temporarily
                    parent = _Data.Transform.parent;
                    _Data.Transform.parent = null;
                }
                if (_Data.IsUniformScale) {
                    if (v2.x != v.x) {
                        _Data.Transform.localScale = new Vector3(v2.x, v2.x, v2.x);
                    }
                    else
                    if (v2.y != v.y) {
                        _Data.Transform.localScale = new Vector3(v2.y, v2.y, v2.y);
                    }
                    else
                    if (v2.z != v.z) {
                        _Data.Transform.localScale = new Vector3(v2.z, v2.z, v2.z);
                    }
                }
                else {
                    _Data.Transform.localScale = v2;
                }
                if (!IsLocal) {
                    _Data.Transform.parent = parent;
                }
            }
        }

        private void GroupGUI(TransformOverrideGroup group, ref Vector3 _value)
        {
            Vector3 value = _value;
            string label;
            string labelShort;
            string channelName;
            float labelWidth = AxonGUI.LabelWidth;

            if (group.IsPosition) {
                label = "Position";
                labelShort = "P";
                channelName = _Data.IsLocal ? "Local Position" : "Position";
            }
            else
            if (group.IsRotation) {
                label = "Rotation";
                labelShort = "R";
                channelName = _Data.IsLocal ? "Local Rotation" : "Rotation";
            }
            else {
                label = "Scale";
                labelShort = "S";
                channelName = "Local Scale";
            }

            bool isEnabled = group.XYZ != null && group.XYZ.IsEnabled;
            bool isSelected = isEnabled && group.XYZ.IsSelected;
            bool isMotionPath = _Data.HasMotionPath && (group.IsPosition || (group.IsRotation && _Data.MotionPath.ApplyRotation));
            bool isFlyby = _Data.HasFlyby && (group.IsPosition || (group.IsRotation && _Data.Flyby.ApplyRotation));

            Color c = Color.white;
            bool colorize = false;
            if (ShowInColor) {
                if (isMotionPath) {
                    colorize = isSelected = true;
                    c = _Data.MotionPath.GUIColor;
                }
                else
                if (isFlyby) {
                    colorize = isSelected = true;
                    c = _Data.Flyby.GUIColor;
                }
                else
                if (isEnabled) {
                    colorize = true;
                    c = group.XYZ.GUIColor;
                }
            }
            if (!isSelected || group.IsLocked) {
                c.a = 0.25f;
            }
            GUI.color = c;
            //Debug.Log($"{group.Type} colorize:{colorize} isSelected:{isSelected} isEnabled:{isEnabled}");
            GUIStyle style = colorize ? isSelected ? SelectedStyle : DeselectedStyle : UncolorizedStyle;
            style.margin = new RectOffset(0, 0, 0, 0);
            AxonGUI.BeginVertical(style);
            GUI.color = Color.white;

            AxonGUI.BeginHorizontal();
            bool locked = AxonGUI.FieldToggleLock(group.IsLocked, "Lock to prevent changes");
            if (locked != group.IsLocked) {
                group.IsLocked = locked;
                if (Event.current != null && Event.current.control) {
                    group.IsLockedX = group.IsLockedY = group.IsLockedZ = locked;
                }
            }

            if (ShowResetCopyPaste || (Event.current != null && Event.current.shift)) {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Reset, "Reset " + label, _ResetMargin)) {
                    UndoUtil.Undo(_Data.Transform, "Reset " + label);
                    if (group.IsScale) {
                        value = Vector3.one;
                    }
                    else {
                        value = Vector3.zero;
                    }
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PropertyCopy, "Copy " + label, _CopyMargin)) {
                    Debug.Log("Copied " + label);//--KEEP
                    AxonTools.CopyTransform(group.IsPosition, group.IsRotation, group.IsScale);
                }
                AxonGUI.BeginDisabledGroup(!AxonTools.HasCopy);
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PropertyPaste, "Paste " + label, _FieldMargin)) {
                    Debug.Log("Pasted " + label);//--KEEP
                    UndoUtil.Undo(_Data.Transform, "Paste " + label);
                    AxonTools.PasteTransform(false, group.IsPosition, group.IsRotation, group.IsScale);
                    _Data.Refresh();
                    EditorGUIUtility.ExitGUI();
                }
                AxonGUI.EndDisabledGroup();
            }

            if (isMotionPath) {
                string tooltip = "This property is being animated by the Motion Path behavior";
                AxonGUI.ButtonIcon(AxonUI.Icons.MotionPath, new RectOffset(2, 0, 1, 0), 16, tooltip);
            }
            else
            if (isFlyby) {
                string tooltip = "This property is being animated by the Flyby behavior";
                AxonGUI.ButtonIcon(AxonUI.Icons.Flyby, new RectOffset(2, 0, 1, 0), 16, tooltip);
            }
            else {
                AxonGUI.BeginDisabledGroup(group.IsLocked);
                string tooltip = "Separate or combined attributes. Combined attributes are treated as a single vector value (XYZ) " +
                    " whereas separate mode allows each attribute (X, Y, Z) to be animated separately.";
                if (AxonGUI.ButtonTexture(group.ShowCombined ? AxonUI.Icons.AttributesCombined : AxonUI.Icons.AttributesSeparated, tooltip, _ResetMargin)) {
                    ToggleCombineGroup(group);
                }
                AxonGUI.EndDisabledGroup();
            }

            group.IsFoldout = AxonGUI.FoldoutInline(group.IsFoldout, null, _FieldMargin);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.margin = new RectOffset(0, 0, 3, 0);

            AxonGUI.BeginDisabledGroup(group.IsLocked);
            if (ShortLabels) {
                if (group.IsScale) {
                    if (GUILayout.Button(labelShort, labelStyle, GUILayout.Width(_labelWidthShort - 16))) {
                        group.IsFoldout = !group.IsFoldout;
                    }
                }
                else {
                    if (GUILayout.Button(labelShort, labelStyle, GUILayout.Width(_labelWidthShort))) {
                        group.IsFoldout = !group.IsFoldout;
                    }
                }
            }
            else {
                if (group.IsScale) {
                    if (GUILayout.Button(label, labelStyle, GUILayout.Width(_labelWidth - 16))) {
                        group.IsFoldout = !group.IsFoldout;
                    }
                }
                else {
                    if (GUILayout.Button(label, labelStyle, GUILayout.Width(_labelWidth))) {
                        group.IsFoldout = !group.IsFoldout;
                    }
                }
            }

            if (group.HasAnimationConflict) {
                AxonGUI.Info("Another channel on this object is controlling this property and may cause a conflict.");
            }
            else
            if (HasAnyConflicts) {
                GUI.color = AxonColor.Invisible;
                AxonGUI.Info("");
                GUI.color = AxonColor.Default;
            }
            if (group.IsScale) {
                if (AxonGUI.ButtonTexture(_Data.IsUniformScale ? AxonUI.Icons.Linked : AxonUI.Icons.Unlinked, "Uniform Scale", new RectOffset(2, 0, 2, 0))) {
                    _Data.IsUniformScale = !_Data.IsUniformScale;
                }
                if (!IsLocal) {
                    AxonGUI.Info("Global scale cannot be set directly and loses precision due to rounding errors. " +
                        "Lossy scale can be set as a convenience, but cannot be animated. Use local scale instead.");
                }
            }

            int attributeChanged = -1;
            if (group.IsFoldout) {
                AxonGUI.Space(50);

                if (AxonGUI.ButtonTexture(group.IsMicroAdjust ? AxonUI.Icons.ResetRangeOff : AxonUI.Icons.ResetRangeOn, "Standard Slider", _FieldMargin)) {
                    group.IsMicroAdjust = false;
                    group.CalculateSliderMinMax(value);
                }
                if (AxonGUI.ButtonTexture(group.IsMicroAdjust ? AxonUI.Icons.MicroAdjustOn : AxonUI.Icons.MicroAdjustOff, "Micro Adjust Mode", _FieldMargin)) {
                    group.IsMicroAdjust = true;
                    group.CalculateSliderMinMax(value);
                }
                AxonGUI.FlexibleSpace();
                if (!group.ShowSettings) {
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.EditOff, "Customize slider ranges", _FieldMargin, IconSize)) {
                        group.ShowSettings = true;
                    }
                }
                else {
                    SlideEditButtons(group);

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.EditOn, "Save and close slider settings", new RectOffset(-2, 0, 2, 0), IconSize)) {
                        group.ShowSettings = false;
                    }
                }
            }
            else {
                if (group.ShowCombined) {
                    if (Timeflow.IsAutoKeyframingEnabled) {
                        bool isAnimated = group.XYZ != null && group.XYZ.IsEnabled && !group.IsLocked;
                        GUI.color = isAnimated ? AxonColor.Recording : Color.white;
                    }
                    if (FloatField(group, "x", 1, ref value.x, false)) attributeChanged = 0;
                    if (FloatField(group, "y", 2, ref value.y, false)) attributeChanged = 0;
                    if (FloatField(group, "z", 3, ref value.z, false, false)) attributeChanged = 0;
                    //Vector3 v = AxonGUI.FieldVector3Inline(null, value);
                    //if (v != value) {
                    //    value = v;
                    //    attributeChanged = 0;
                    //}
                }
                else {
                    AxonGUI.SetLabelWidth(10);
                    if (Timeflow.IsAutoKeyframingEnabled) {
                        bool isAnimated = group.X != null && group.X.IsEnabled && !group.IsLocked && !group.X.IsLocked;
                        GUI.color = isAnimated ? AxonColor.Recording : Color.white;
                    }

                    if (FloatField(group, "x", 1, ref value.x, true)) attributeChanged = 1;

                    if (Timeflow.IsAutoKeyframingEnabled) {
                        bool isAnimated = group.Y != null && group.Y.IsEnabled && !group.IsLocked && !group.Y.IsLocked;
                        GUI.color = isAnimated ? AxonColor.Recording : Color.white;
                    }
                    if (FloatField(group, "y", 2, ref value.y, true)) attributeChanged = 2;

                    if (Timeflow.IsAutoKeyframingEnabled) {
                        bool isAnimated = group.Z != null && group.Z.IsEnabled && !group.IsLocked && !group.Z.IsLocked;
                        GUI.color = isAnimated ? AxonColor.Recording : Color.white;
                    }
                    if (FloatField(group, "z", 3, ref value.z, true)) attributeChanged = 3;

                    AxonGUI.RestoreLabelWidth();
                }
                GUI.color = Color.white;
            }

            if (_Data.IsTimeflowObject) {
                if (group.ShowCombined) {
                    AnimationControls(group, 0);
                }
            }
            else {
                AddToTimeflowButton();
            }
            AxonGUI.EndDisabledGroup();
            AxonGUI.EndHorizontal();

            if (group.IsFoldout) {
                AxonGUI.BeginDisabledGroup(group.IsLocked);

                Vector3 v = value;

                if (group.ShowSettings) {
                    SliderEdit(group, v);
                }

                if (Slider(group, 1, ref v.x)) {
                    value = v;
                    group.SelectChannel(1);
                    attributeChanged = 1;
                }
                if (!group.IsScale || !_Data.IsUniformScale) {
                    if (Slider(group, 2, ref v.y)) {
                        value = v;
                        group.SelectChannel(2);
                        attributeChanged = 2;
                    }
                    if (Slider(group, 3, ref v.z)) {
                        value = v;
                        group.SelectChannel(3);
                        attributeChanged = 3;
                    }
                }

                AxonGUI.EndDisabledGroup();
            }

            AxonGUI.EndVertical();

            if (hasTimeChanged) {
                hasTimeChanged = false;
                group.RecalculateSliderMinMax = true;
            }

            if (_value != value || attributeChanged >= 0) {
                UndoUtil.Undo(target, $"{label} {value}");
                _value = value;
                OnValueChanged(group, attributeChanged, _value);
            }
            else
            if (!GUI.changed && Event.current != null && !Event.current.isMouse &&
                (group.RecalculateSliderMinMax || (Event.current.isKey && Event.current.keyCode == KeyCode.Space))) {
                group.RecalculateSliderMinMax = false;
                group.CalculateSliderMinMax(value);
            }
        }

        private bool FloatField(TransformOverrideGroup group, string label, int attribute, ref float value, bool showControls, bool showInvisible = true)
        {
            bool changed = false;
            TimeflowChannel ch = group.Channel[attribute];
            EditorGUILayout.BeginHorizontal(GetChannelStyle(ch, group, attribute));
            GUI.color = Color.white;

            bool locked = IsLocked(group, attribute);

            AxonGUI.BeginDisabledGroup(locked);
            float x = AxonGUI.FieldFloatInline(null, label, value);
            if (x != value) {
                value = x;
                changed = true;
            }
            AxonGUI.EndDisabledGroup();

            //Debug.Log($"FloatField: showControls:{showControls} showInvisible:{showInvisible} locked:{locked} attribute:{attribute} group:{group.Type}");
            if (showControls) {
                locked = LockAttribute(group, attribute);

                AxonGUI.BeginDisabledGroup(locked);
                AnimationControls(group, attribute, true);
                AxonGUI.EndDisabledGroup();
            }
            else
            if (showInvisible) {
                if (AnySeparated) {
                    InvisibleIcon();
                    if (_Data.IsTimeflowObject) {
                        InvisibleIcon();
                        InvisibleIcon();
                        InvisibleIcon();
                    }
                }
            }
            else
            if (!_Data.IsTimeflowObject) {
                if (AnySeparated) {
                    InvisibleIcon();
                }
            }
            EditorGUILayout.EndHorizontal();
            if (isWide) AxonGUI.Space(1);
            return changed;
        }

        private void AnimationControls(TransformOverrideGroup group, int attribute, bool inline = false)
        {
            if (_Data.IsTimeflowObject) {
                if (!inline) ChannelEnableControl(group, attribute);
                AnimationKeyframeControls(group, attribute, inline);
            }
        }

        private void AnimationKeyframeControls(TransformOverrideGroup group, int attribute, bool inline)
        {
            TimeflowChannel ch = group.Channel[attribute];
            bool isNull = ch == null;

            if (isNull && group.HasAnimationConflict) {
                InvisibleIcon();
                InvisibleIcon();
                InvisibleIcon();
                return;
            }

            bool isDisabled = isNull || !ch.IsEnabled;

            bool hasPrevKey = isNull ? false : ch.GetPrevKey(ch.CurrentTime) != null;
            bool hasNextKey = isNull ? false : ch.GetNextKey(ch.CurrentTime) != null;

            if (!inline || !isNull) {
                if (AxonGUI.ButtonTexture(hasPrevKey ? AxonUI.PrevKeyStyle : AxonUI.PrevKeyNoneStyle, AxonUI.DisplayPrevLabel.text, _PrevNextKeyMargin, IconSize)) {
                    ch.GotoPrevKeyframe();
                }
            }
            else {
                InvisibleIcon();
            }

            if (isNull) {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.ToggleKeyOff, "Set keyframe", _KeyframeMargin, IconSize)) {
                    bool canAnimate = group.HasAnimation ? !group.IsCombined && attribute > 0 : true;
                    ConfirmAddAnimation(group, attribute, canAnimate);
                }
            }
            else
            if (ch.IsKeySet()) {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.ToggleKeyOn, "Unset keyframe", _KeyframeMargin)) {
                    ch.UnsetKey();
                }
            }
            else
            if (AxonGUI.ButtonTexture(AxonUI.Icons.ToggleKeyOff, "Set keyframe", _KeyframeMargin)) {
                ch.AddKey();
            }

            if (!inline || !isNull) {
                if (AxonGUI.ButtonTexture(hasPrevKey ? AxonUI.NextKeyStyle : AxonUI.NextKeyNoneStyle, AxonUI.DisplayNextLabel.text, _PrevNextKeyMargin, IconSize)) {
                    ch.GotoNextKeyframe();
                }
            }
            else {
                InvisibleIcon();
            }
        }

        private void ChannelEnableControl(TransformOverrideGroup group, int attribute)
        {
            TimeflowChannel ch = group.Channel[attribute];
            bool canAnimate = group.HasAnimation ? !group.IsCombined && attribute > 0 : true;
            if (ch == null) {
                if (group.HasAnimationConflict) {
                    // Don't show animation options
                    InvisibleIcon();
                }
                else
                if (AxonGUI.ButtonTexture(AxonUI.Icons.BehaviorOff, "Add animation channel for this property", _EnableMargin, IconSize)) {
                    canAnimate = ConfirmAddAnimation(group, attribute, canAnimate);
                }
            }
            else {
                Texture2D icon = ch != null && ch.IsEnabled ? AxonUI.Icons.BehaviorOn : AxonUI.Icons.BehaviorOff;
                if (Event.current != null && Event.current.control) icon = AxonUI.Icons.DeleteOn;
                if (AxonGUI.ButtonTexture(icon, "Enable/disable this animation channel. Hold Control and click to delete this channel", _EnableMargin, IconSize)) {
                    if (Event.current != null && Event.current.control) {
                        ch.Delete();
                        isRefreshNeeded = true;
                        EditorGUIUtility.ExitGUI();
                    }
                    else {
                        ch.IsEnabled = !ch.IsEnabled;
                    }
                }
            }
        }

        private bool Slider(TransformOverrideGroup group, int attribute, ref float value)
        {
            AxonGUI.BeginChangeCheck();
            AxonGUI.SetLabelWidth(30);
            string label = (attribute == 0 || (group.IsScale && _Data.IsUniformScale)) ? "xyz" : attribute == 1 ? "x" : attribute == 2 ? "y" : "z";

            TimeflowChannel ch = group.Channel[attribute];
            bool hasChannel = ch != null;
            bool isAnimated = hasChannel && ch.IsEnabled;
            if (group.XYZ != null) isAnimated = group.XYZ.IsEnabled;

            GUIStyle style = GetChannelStyle(ch, group, attribute);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.fixedHeight = 0;
            AxonGUI.BeginHorizontal(style, GUILayout.Height(22));
            GUI.color = Color.white;

            bool isLocked = LockAttribute(group, attribute, false);
            bool isKeyframeLocked = IsKeyframeLocked(group, attribute);

            if (ShowResetCopyPaste || (Event.current != null && Event.current.shift)) {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Reset, "Reset " + label, _ResetMargin)) {
                    UndoUtil.Undo(_Data.Transform, "Reset " + label);
                    value = group.IsScale ? 1 : 0;
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PropertyCopy, "Copy " + label, _CopyMargin)) {
                    //Debug.Log("Copied " + label);
                    CopiedValue = value;
                    HasCopiedValue = true;
                }
                AxonGUI.BeginDisabledGroup(!HasCopiedValue);
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PropertyPaste, "Paste " + label, _FieldMargin)) {
                    //Debug.Log("Pasted " + label);
                    UndoUtil.Undo(_Data.Transform, "Paste " + label);
                    value = CopiedValue;
                }
                AxonGUI.EndDisabledGroup();
            }
            float v = value;

            AxonGUI.BeginDisabledGroup(isLocked);
            GUI.color = Timeflow.IsAutoKeyframingEnabled && isAnimated && !isLocked ? AxonColor.Recording : Color.white;

            if (attribute == 1) {
                v = AxonGUI.FieldSlider(null, label, value, Mathf.Min(value, group.SliderMin.x), Mathf.Max(value, group.SliderMax.x));
            }
            else
            if (attribute == 2) {
                v = AxonGUI.FieldSlider(null, label, value, Mathf.Min(value, group.SliderMin.y), Mathf.Max(value, group.SliderMax.y));
            }
            else
            if (attribute == 3) {
                v = AxonGUI.FieldSlider(null, label, value, Mathf.Min(value, group.SliderMin.z), Mathf.Max(value, group.SliderMax.z));
            }
            AxonGUI.EndDisabledGroup();

            GUI.color = Color.white;
            if (v != value) {
                value = v;
                OnValueChanged(group, attribute, new Vector3(value, value, value));
            }

            if (!group.ShowCombined) {
                if (_Data.IsTimeflowObject) {
                    AnimationControls(group, attribute);
                }
                else {
                    AddToTimeflowButton();
                }
            }
            else
            if (AnySeparated) {
                InvisibleIcon();
                if (AnySeparateAnimated) {
                    InvisibleIcon();
                    InvisibleIcon();
                    InvisibleIcon();
                }
            }

            AxonGUI.EndHorizontal();

            return AxonGUI.EndChangeCheck();
        }

        private bool LockAttribute(TransformOverrideGroup group, int attribute, bool inline = true)
        {
            bool isLocked = _Data.IsLocked;
            if (isLocked) return isLocked;
            if (attribute == 1) {
                if (inline) {
                    isLocked = group.IsLockedX = AxonGUI.FieldToggleLock(group.IsLockedX, "Lock to prevent changes", _LockMargin, IconSize);
                }
                else {
                    isLocked = group.IsLockedX = AxonGUI.FieldToggleLock(group.IsLockedX, "Lock to prevent changes");
                }
            }
            else
            if (attribute == 2) {
                if (inline) {
                    isLocked = group.IsLockedY = AxonGUI.FieldToggleLock(group.IsLockedY, "Lock to prevent changes", _LockMargin, IconSize);
                }
                else {
                    isLocked = group.IsLockedY = AxonGUI.FieldToggleLock(group.IsLockedY, "Lock to prevent changes");
                }
            }
            else
            if (attribute == 3) {
                if (inline) {
                    isLocked = group.IsLockedZ = AxonGUI.FieldToggleLock(group.IsLockedZ, "Lock to prevent changes", _LockMargin, IconSize);
                }
                else {
                    isLocked = group.IsLockedZ = AxonGUI.FieldToggleLock(group.IsLockedZ, "Lock to prevent changes");
                }
            }

            return isLocked;
        }

        private void SliderEdit(TransformOverrideGroup group, Vector3 currentValue)
        {
            AxonGUI.SetLabelWidth(60);

            bool changed = false;

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();

            bool uniform = AxonGUI.FieldToggleUniform(null, group.IsUniformMinMax, new RectOffset(0, 0, 3, 0));
            if (group.IsUniformMinMax != uniform) {
                group.IsUniformMinMax = uniform;
                changed = true;
            }

            if (SliderConfigLimit("Min", ref group.Min, group.IsUniformMinMax, currentValue)) changed = true;

            if (!group.IsUniformMinMax && !isWide) {
                AxonGUI.EndHorizontal();
                AxonGUI.BeginHorizontal();

                // Draw an invisible icon to align the layout
                InvisibleIcon();
            }
            if (SliderConfigLimit("Max", ref group.Max, group.IsUniformMinMax, currentValue)) changed = true;
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            InvisibleIcon();
            AxonGUI.SetLabelWidth(120);
            float scale = AxonGUI.FieldSlider(null, "Micro Adjust Scale", group.MicroAdjustScale, TransformOverrideGroup.MicroAdjustScaleMin, 1f);
            if (scale != group.MicroAdjustScale) {
                group.MicroAdjustScale = scale;
                changed = true;
            }
            AxonGUI.EndHorizontal();

            AxonGUI.EndBox();

            if (changed || group.RecalculateSliderMinMax) {
                group.CalculateSliderMinMax(currentValue);
            }
        }

        private void InvisibleIcon()
        {
            // Draw an invisible icon to align the layout
            GUI.color = AxonColor.Invisible;//Color.green;
            AxonGUI.ButtonTexture(AxonUI.Icons.LockOff, "", new RectOffset(1, 1, 5, 0), IconSize);
            GUI.color = AxonColor.Default;
        }

        private void SlideEditButtons(TransformOverrideGroup group)
        {
            if (AxonGUI.ButtonTexture(AxonUI.Icons.Reset, "Reset Min/Max Range", new RectOffset(-1, 0, 2, 0))) {
                group.Min = group.DefaultMin;
                group.Max = group.DefaultMax;
                //Debug.Log($"Min:{group.Min} Max:{group.Max}");
                group.RecalculateSliderMinMax = true;
            }
            if (AxonGUI.ButtonInline("x10")) {
                group.Min *= 10f;
                group.Max *= 10f;
                group.RecalculateSliderMinMax = true;
            }
            if (AxonGUI.ButtonInline("/10")) {
                group.Min /= 10f;
                group.Max /= 10f;
                group.RecalculateSliderMinMax = true;
            }
            if (AxonGUI.ButtonInline("x2")) {
                group.Min *= 2f;
                group.Max *= 2f;
                group.RecalculateSliderMinMax = true;
            }
            if (AxonGUI.ButtonInline("/2")) {
                group.Min /= 2f;
                group.Max /= 2f;
                group.RecalculateSliderMinMax = true;
            }
            AxonGUI.Space();
        }

        private bool SliderConfigLimit(string label, ref Vector3 min, bool isUniform, Vector3 currentValue)
        {
            bool changed = false;
            Vector3 value = min;
            if (isUniform) {
                value.x = AxonGUI.FieldFloatInline(null, label, min.x);
                if (value.x != min.x) {
                    min.y = min.z = min.x = value.x;
                    changed = true;
                }
            }
            else {
                value = AxonGUI.FieldVector3Inline(null, label, min);
                if (value != min) {
                    min = value;
                    changed = true;
                }
            }
            if (AxonGUI.ButtonInline("Set")) {
                changed = true;
                if (isUniform) {
                    min.y = min.z = min.x = currentValue.x;
                }
                else {
                    min = currentValue;
                }
            }
            return changed;
        }

        #endregion

        #region OPERATIONS

        private void DebugListChannels(TimeflowChannel channel)
        {
            if (channel == null) return;
            foreach (TimeflowChannel ch in channel.Behavior.Channels) {
                if (ch == null) continue;
                Debug.Log($"CHANNEL: {ch.PathName}");//--KEEP
            }
        }

        private bool IsAnimated(TransformOverrideGroup group, int attribute)
        {
            TimeflowChannel ch = group.Channel[attribute];
            return ch != null && ch.IsEnabled;
        }

        private bool IsLocked(TransformOverrideGroup group, int attribute)
        {
            bool isLocked = _Data.IsLocked;
            if (isLocked) return isLocked;
            if (attribute == 1) {
                isLocked = group.IsLockedX;
            }
            else
            if (attribute == 2) {
                isLocked = group.IsLockedY;
            }
            else
            if (attribute == 3) {
                isLocked = group.IsLockedZ;
            }

            return isLocked;
        }

        private bool IsKeyframeLocked(TransformOverrideGroup group, int attribute)
        {
            if (Timeflow.IsAutoKeyframingEnabled) return false;
            bool isLocked = false;
            if (attribute == 1) {
                if (group.X != null && group.X.IsEnabled && !Timeflow.IsAutoKeyframingEnabled) {
                    isLocked = true;
                }
            }
            else
            if (attribute == 2) {
                if (group.Y != null && group.Y.IsEnabled && !Timeflow.IsAutoKeyframingEnabled) {
                    isLocked = true;
                }
            }
            else
            if (attribute == 3) {
                if (group.Z != null && group.Z.IsEnabled && !Timeflow.IsAutoKeyframingEnabled) {
                    isLocked = true;
                }
            }

            return isLocked;
        }

        private bool CanAnimate(TransformOverrideGroup group, int attribute)
        {
            return group.HasAnimation ? !group.IsCombined && attribute > 0 : true;
        }

        private void ToggleCombineGroup(TransformOverrideGroup group)
        {
            if (!group.HasAnimation || group.IsMixed) {
                group.ShowCombined = !group.ShowCombined;
            }
            else
            if (group.IsCombined) {
                //DebugListChannels(group.Channel[0]);
                if (ConfirmedSeparateChannels()) {
                    group.Channel[0].SeparateChannel();
                    group.ShowCombined = false;
                    group.Refresh();
                    isRefreshNeeded = true;
                }
            }
            else {
                //DebugListChannels(group.X);
                if (ConfirmCombineChannels()) {
                    TimeflowChannel.CombineChannels(group.X, group.Y, group.Z);
                    group.ShowCombined = true;
                    group.Refresh();
                    isRefreshNeeded = true;
                }
            }
            UpdateGroupAutoKeyframing(group);

            EditorGUIUtility.ExitGUI();
        }

        private GUIStyle GetChannelStyle(TimeflowChannel ch, TransformOverrideGroup group, int attribute)
        {
            bool hasChannel = ch != null;
            bool isSelected = hasChannel && ch.IsSelected;
            bool colorize = ShowInColor && hasChannel;
            Color c = colorize ? ch.GUIColor : Color.white;
            if (!isSelected || group.IsLocked || (colorize && !ch.IsEnabled)) {
                c.a = 0.25f;
            }
            GUI.color = c;
            //Debug.Log($"{group.Type} colorize:{colorize} isSelected:{isSelected} attribute:{attribute}");
            return colorize ? isSelected ? SelectedStyle : DeselectedStyle : UncolorizedStyle;
        }

        private TimeflowChannel AddChannel(TransformOverrideGroup group, int attribute, bool setKey)
        {
            UndoUtil.Undo(_Data.Transform.gameObject, "Add Channel", true);
            if (_Data.TimeflowObject == null) {
                _Data.TimeflowObject = Timeflow.SetupTimeflowObject(_Data.Transform.gameObject, true);
            }

            string name = IsLocal ? "Local " : "Global ";
            if (group.IsPosition) name += "Position";
            else
            if (group.IsRotation) name += "Rotation";
            else
            if (group.IsScale) name = "Local Scale"; // Cannot animate world scale

            //Debug.Log($"AddChannel:{group.Type} attribute:{attribute} name:{name}");
            TimeflowChannel ch = Keyframer.AddChannel(_Data.TimeflowObject.gameObject, new Property(_Data.Transform, name, attribute - 1));
            group.Channel[attribute] = ch;

            if (attribute == 0) {
                group.XYZ = ch;
            }
            else
            if (attribute == 1) {
                group.X = ch;
            }
            else
            if (attribute == 2) {
                group.Y = ch;
            }
            else
            if (attribute == 3) {
                group.Z = ch;
            }

            if (setKey) ch.AddKey();

            isRefreshNeeded = true;

            return ch;
        }

        private bool ConfirmAddAnimation(TransformOverrideGroup group, int attribute, bool canAnimate)
        {
            if (!canAnimate && ShowDialogChannelConflict) {
                string msg;

                if (group.IsCombined) {
                    msg = "An animation channel is already set for this value. " +
                        "Separate channels should not be added until the other channel is removed.";
                }
                else {
                    msg = "Animation channels are already set for this value. The new channel should not " +
                        "be added until the separate X, Y, Z channels are removed.";
                }
                msg += " If you choose to add the channel anyway, it may cause conflicting animation behaviors.";

                int d = EditorUtil.ShowDialog("Channel Conflict", msg, "Add Anyway", "Cancel", "Add and don't show again");

                if (d == 1) {
                    canAnimate = false;
                }
                else {
                    canAnimate = true;
                    if (d == 2) {
                        ShowDialogChannelConflict = false;
                    }
                }
            }
            if (canAnimate) {
                AddChannel(group, attribute, true);
            }

            return canAnimate;
        }

        private bool ConfirmCombineChannels()
        {
            if (!ShowDialogCombineChannels) return true;

            string msg = "Animation already exists on separate channels for this property. Do you wish to merge these channels into a single " +
                "combined value? Doing so requires merging all keyframes from separate channels into a single combined XYZ channel.\n\nThis is a " +
                "destructive process that removes the original separate channels. The resulting animation is likely to change as a result of " +
                "merging the keyframes.";
            int d = EditorUtil.ShowDialog("Combine Channels?", msg, "Combine", "Cancel", "Combine and don't ask again");

            if (d == 2) {
                ShowDialogCombineChannels = false;
            }
            return d != 1;
        }

        private bool ConfirmedSeparateChannels()
        {
            if (!ShowDialogCombineChannels) return true;

            string msg = "Animation already exists as a combined value for this property. Do you wish to separate the X,Y,Z attributes into " +
                "separate channels?\n\nThis is a destructive process that removes the original combined channel and splits the animation into new " +
                "separate channels. The resulting animation may differ slightly from the original due to splitting the keyframes in the channels.";
            int d = EditorUtil.ShowDialog("Separate Channels?", msg, "Separate", "Cancel", "Separate and don't ask again");

            if (d == 2) {
                ShowDialogCombineChannels = false;
            }
            return d != 1;
        }

        #endregion

        #region EVENTS

        private void OnValueChanged(TransformOverrideGroup group, int attribute, Vector3 value)
        {
            if (!Timeflow.IsAutoKeyframingEnabled) return;
            //Debug.Log($"OnValueChanged: group:{group.Type} attribute:{attribute} IsAutoKeyframingEnabled:{Timeflow.IsAutoKeyframingEnabled}");
            if (Timeflow.IsAutoKeyframingEnabled) {
                if (group.ShowCombined) {
                    OnValueChangedAttribute(group, 0, value);
                }
                else {
                    OnValueChangedAttribute(group, attribute, value);
                }
            }
        }

        private void OnValueChangedAttribute(TransformOverrideGroup group, int attribute, Vector3 value)
        {
            if (!Timeflow.IsAutoKeyframingEnabled) return;
            //Debug.Log($"OnValueChangedAttribute: attribute:{attribute}");
            if (group.Channel[attribute] == null) {
                if (group.HasAnimationConflict) {
                    // Do nothing
                    //Debug.Log($"OnValueChangedAttribute: group.HasAnimationConflict");
                }
                else {
                    //Debug.Log($"OnValueChangedAttribute: AddChannel");
                    AddChannel(group, attribute, true);
                }
            }
            else {
                string name = $"Auto Keyframe {group.Type} {TransformOverrideGroup.AttributeName(attribute)}";
                //Debug.Log($"OnValueChangedAttribute: {name} attr:{attribute} value:{value}");
                UndoUtil.Undo(group.Channel[attribute].Behavior, name, true);
                Keyframe key = group.Channel[attribute].SetKey(group.Channel[attribute].CurrentTime);
                if (attribute == 0) {
                    key.KeyVector = value;
                }
                else {
                    key.KeyValue = value[attribute - 1];
                }
            }
        }

        #endregion
    }
}
#endif
#endif
