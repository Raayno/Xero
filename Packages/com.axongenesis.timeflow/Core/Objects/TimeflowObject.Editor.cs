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
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    /// <summary>
    /// A TimeflowObject defines a track with a collection of channels and behaviors it manages. All update
    /// calls pass through the object to the child channels and behaviors.
    /// </summary>
    public partial class TimeflowObject : TimeflowBehavior
    {
        #region STATIC

        private static bool autoConfirmRemoveTimeflowObjects = false;

        private static int tracksJoined = 0;

        public static void RemoveTimeflowObjects()
        {
            bool canApply = autoConfirmRemoveTimeflowObjects;
            if (!canApply) {
                int confirm = EditorUtility.DisplayDialogComplex("Remove All Timeflow Behaviors?", "This destroys all components in the Axon Genesis " +
                    "namespace on the selected game objects. This can be used to strip away Timeflow animation behaviors while otherwise " +
                    "leaving objects intact with any other components left as-is.", "Continue", "Cancel", "Yes, don't show again");

                canApply = confirm == 0 || confirm == 2;
                if (confirm == 2) autoConfirmRemoveTimeflowObjects = true;
            }

            if (canApply) {
                foreach (GameObject obj in Selection.gameObjects) {
                    ObjectUtil.DestroyComponentsInChildren<AxonGenesisBehavior>(obj, typeof(TimeflowObject));
                    ObjectUtil.DestroyComponentsInChildren<Rotator>(obj);
                    ObjectUtil.DestroyComponentsInChildren<TimeflowObject>(obj);
                }
            }
        }

        public static bool ValidateRemoveTimeflowObjects()
        {
            return Selection.activeGameObject != null;
        }

        public static void ResetSelectedTracksFullLength()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                foreach (GameObject gobj in Selection.gameObjects) {
                    List<TimeflowObject> timeflowObjects = ObjectUtil.GetComponentsRecursive<TimeflowObject>(gobj);
                    if (timeflowObjects != null && timeflowObjects.Count > 0) {
                        foreach (TimeflowObject obj in timeflowObjects) {
                            if (obj != null) {
                                UndoUtil.Undo(obj, "Auto Full Length");
                                obj.Track.AutoFullLength = true;
                            }
                        }
                    }
                }
            }
        }

        public static bool ValidateResetSelectedTracksFullLength()
        {
            return Selection.activeGameObject != null;
        }

        public static void ResetAllTracksFullLength()
        {
            TimeflowObject[] objects = UnityEngine.Object.FindObjectsByType(typeof(TimeflowObject), FindObjectsInactive.Include, FindObjectsSortMode.None) as TimeflowObject[];
            if (objects != null && objects.Length > 0) {
                foreach (TimeflowObject obj in objects) {
                    if (obj != null) {
                        UndoUtil.Undo(obj, "Auto Full Length");
                        obj.Track.AutoFullLength = true;
                    }
                }
            }
            if (Timeflow.Active != null) Timeflow.Active.Refresh(true);
        }

        public static bool ValidateResetAllTracksFullLength()
        {
            return Selection.activeGameObject != null;
        }

        public static void JoinAdjacentTracks()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;
            tracksJoined = 0;

            List<TimeflowObject> objects = new List<TimeflowObject>();
            foreach (GameObject obj in Selection.gameObjects) {
                List<TimeflowObject> objs = ObjectUtil.GetComponentsRecursive<TimeflowObject>(obj);
                if (objs != null && objs.Count > 0) {
                    foreach (TimeflowObject o in objs) {
                        if (!objects.Contains(o)) objects.Add(o);
                    }
                }
            }
            if (objects.Count > 0) {
                foreach (TimeflowObject obj in objects) {
                    if (obj != null) {
                        UndoUtil.Undo(obj, "Auto Full Length");
                        if (obj.Track.Keys != null && obj.Track.Keys.Count > 1) {
                            obj.Track.Keys.Sort(KeyframeSort.ByTimeAsc);
                            while (JoinAdjacentTracks(obj.Track)) {
                                tracksJoined++;
                            }
                        }
                    }
                }
            }

            Debug.Log($"{tracksJoined} tracks joined");//--KEEP
        }

        public static bool ValidateJoinAdjacentTracks()
        {
            return Selection.activeGameObject != null;
        }

        private static bool JoinAdjacentTracks(TimeflowTrack track)
        {
            bool joined = false;
            Keyframe first = null;
            Keyframe prev = null;
            float endTime = 0;
            List<Keyframe> adjacent = new List<Keyframe>();
            foreach (Keyframe k in track.Keys) {
                if (prev != null) {
                    if (first == null) {
                        first = prev;
                    }
                    float dif = Mathf.Abs(k.KeyTime - prev.KeyValue);
                    if (dif <= TimeflowPreferences.Current.KeyTolerance) {
                        adjacent.Add(k);
                        endTime = k.KeyValue;
                        joined = true;
                        break;
                    }
                }
                prev = k;
            }
            if (joined) {
                track.JoinTracks(first, adjacent, endTime);
            }
            return joined;

        }

        //[MenuItem(TimeflowMenu.MenuPath + "Update Auto Full Full Length Tracks", false, 828)]
        public static void UpdateAllAutoFullLengthTracks()
        {
            TimeflowObject[] objects = UnityEngine.Object.FindObjectsByType(typeof(TimeflowObject), FindObjectsInactive.Include, FindObjectsSortMode.None) as TimeflowObject[];
            if (objects != null && objects.Length > 0) {
                foreach (TimeflowObject obj in objects) {
                    if (obj != null && obj.Track != null && obj.Track.AutoFullLength) {
                        UndoUtil.Undo(obj, "Update Auto Full Length");
                        obj.Track.UpdateAutoFullLength();
                    }
                }
            }
        }

        public static List<TimeflowObject> GetAll()
        {
            List<TimeflowObject> all = new List<TimeflowObject>();
            TimeflowObject[] objects = UnityEngine.Object.FindObjectsByType(typeof(TimeflowObject), FindObjectsInactive.Include, FindObjectsSortMode.None) as TimeflowObject[];
            foreach (TimeflowObject go in objects) {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                all.Add(go);
            }
            return all;
        }

        #endregion

        #region PUBLIC

        public bool IsLocked;
        public bool EditorShowObject;
        public bool EditorShowTrack;
        public bool EditorShowTrackKeys;
        public bool EditorShowViewOptions;
        public bool EditorShowBehaviors;
        public bool EditorShowEvents;
        public bool EditorShowTimeflow;
        public bool EditorAnimationOffsetMinMax;

        [SerializeField, FormerlySerializedAs("DisplaySolo")]
        private bool _DisplaySolo;

        public TransformOverrideData OverrideData;
        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField, FormerlySerializedAs("DisplayChannels")]
        private bool _DisplayChannels = true;

        [SerializeField]
        private bool _IsCollapsed;

        [SerializeField]
        public bool _ShowChildren = true;

        [SerializeField] protected int _IconIndex = -1;

        #endregion

        #region PRIVATE

        private bool _IsSelected;
        private bool _IsChannelSelected;
        private bool _IsDisplayed = true;
        private bool _IsPrefab = false;
        private bool _IsPrefabRoot = false;
        private bool _IsTimeflowInstance = false;
        private bool _IsParentCollapsed = false;

        #endregion

        [NonSerialized]
        public bool WasSelected = false;

        [NonSerialized]
        public GUIRect GUIRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUISelectRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIColorRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIRectFoldout = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public bool _GUICull = false;

        [NonSerialized]
        public List<Rect> PropertyRects = null;

        [NonSerialized]
        public GUIObject EventGUI = null;

        [NonSerialized]
        public bool _IsEditingName;

        [NonSerialized]
        public string tempEditName = "";

        [NonSerialized]
        public GUIRect tempEditRect;

        [NonSerialized]
        public GUIRect tempEditContainingRect;

        /// <summary>
        /// The sort order actually displayed based on its position vertically in the view. This value can 
        /// change any time the view changes.
        /// </summary>
        [NonSerialized]
        public int SortOrderInView = 0;

        [NonSerialized]
        public List<TimeflowChannel> _AllChannelsReverse;

        protected Texture2D _Icon = null;

        protected GUIStyle _IconStyle = null;

        #region ACCESSORS

        public List<TimeflowChannel> AllChannelsReverse {
            get {
                if (AllChannels != null && AllChannels.Count > 0) {
                    if (_AllChannelsReverse == null || _AllChannelsReverse.Count != AllChannels.Count) {
                        _AllChannelsReverse = new List<TimeflowChannel>(AllChannels);
                        _AllChannelsReverse.Sort(new SortTimeflowChannelDescending());
                    }
                }
                else {
                    _AllChannelsReverse = null;
                }
                return _AllChannelsReverse;
            }
            set {
                _AllChannelsReverse = value;
            }
        }

        /// <summary>
        /// For display purposes only in the editor, either returns AllChannels or AllChannelsReverse
        /// </summary>
        public List<TimeflowChannel> AllChannelsForDisplay {
            get {
                return TimeflowPreferences.Current.ReverseChannelOrder ? AllChannelsReverse : AllChannels;
            }
        }

        public bool DisplayChannels {
            get { return _DisplayChannels; }
            set {
                _DisplayChannels = value;
            }
        }

        public bool GUICull {
            get { return _GUICull; }
            set {
                if (_GUICull != value) {
                    _GUICull = value;
                }
            }
        }

        public Color GUIColorWhite {
            get {
                bool locked = IsLocked;
                return locked ? AxonColor.LockedOverlay : AxonColor.Default;
            }
        }

        public bool DisplaySolo {
            get { return _DisplaySolo; }
            set {
                if (_DisplaySolo != value) {
                    _DisplaySolo = value;
                }
            }
        }

        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        public bool IsDisplayed {
            get {
                if (!_IsDisplayed) return false;
                if (HasParentContainer && ParentContainer.IsDisplayed) {
                    return !ParentContainer.IsCollapsed;
                }
                return _IsDisplayed;
            }
            set {
                if (_IsDisplayed != value) {
                    _IsDisplayed = value;
                    //Debug.Log($"{name}.IsDisplayed:{value}", gameObject);
                }
            }
        }

        public bool IsSelectable {
            get {
                if (IsParentCollapsed) return false;
                if (!_IsDisplayed) return false;
                if (HasParentContainer && ParentContainer.IsDisplayed) {
                    return !ParentContainer.IsCollapsed;
                }
                return _IsDisplayed;
            }
        }

        public bool IsParentCollapsed {
            get {
                if (ParentContainer != null) {
                    return ParentContainer.IsDisplayed && (ParentContainer.IsCollapsed || ParentContainer.IsParentCollapsed);
                }
                return false;
            }
            set {
                _IsParentCollapsed = value;
            }
        }

        public bool IsEditingName {
            get {
                return _IsEditingName;
            }
            set {
                if (_IsEditingName != value) {
                    _IsEditingName = value;
                }
            }
        }

        public override bool IsSelected {
            get {
                return _IsSelected;
            }
            set {
                if (_IsSelected != value) {
                    _IsSelected = value;
                    //Debug.Log($"{name}.IsSelected:{value}");
                    if (TimeflowPreferences.Current.ObjectsSelectTracks) {
                        SelectTracks(_IsSelected);
                    }
                }
            }
        }

        public void DisplaySoloWithChannels(bool show)
        {
            DisplaySolo = show;
            if (AllChannels != null) {
                foreach (TimeflowChannel ch in AllChannels) {
                    ch.DisplayChannelSolo = show;
                }
            }
        }

        public void SelectTracks(bool select)
        {
            if (Track == null || Track.Keys.Count == 0) return;
            if (Timeflow.Active == null || Timeflow.Active.View == null || Timeflow.Active.View.IsGraphMode) return;
            if (select) Timeflow.Active.View.SelectKeysInChannel(Track);
            else Timeflow.Active.View.DeselectKeysInChannel(Track);
        }

        public bool IsChannelSelected {
            get {
                return _IsChannelSelected;
            }
            set {
                if (_IsChannelSelected != value) {
                    _IsChannelSelected = value;
                }
            }
        }

        public virtual bool IsCollapsed {
            get {
                if (HasParentContainer) {
                    if (!_IsCollapsed) {
                        return ParentContainer.IsCollapsed;
                    }
                }
                return _IsCollapsed;
            }
            set {
                if (_IsCollapsed != value) {
                    _IsCollapsed = value;
                    //Debug.Log($"_IsCollapsed:{value}");
                }
            }
        }

        /// <summary>
        /// Returns whether or not the current object has any TimeflowEvents
        /// </summary>
        public bool HasEvents {
            get {
                return Events != null && Events.Count > 0;
            }
        }

        public override void OnBeforeSavePreset(ref List<ComponentPresetListItem> items)
        {
            base.OnBeforeSavePreset(ref items);
            if (items == null || items.Count == 0) return;
            List<ComponentPresetListItem> toremove = new List<ComponentPresetListItem>();
            foreach (ComponentPresetListItem item in items) {
                if (item.Name == "Is Locked" || item.Name == "Is Collapsed" || item.Name.Contains("Timeflow Parent Override") ||
                    item.Name.Contains("Icon Index") || item.Name.Contains("Display Solo") || item.Name.Contains("Display Channels") ||
                    item.Name.Contains("Show Children") || item.Name.Contains("Sort Order") || item.Name.Contains("Default Value") ||
                    item.Name.Contains("Max Value") || item.Name.Contains("Min Value") || item.Name.Contains("Override Timeflow Parent")
                    ) {
                    toremove.Add(item);
                }
                if (item.Name.StartsWith("Time") || item.Name.StartsWith("Update")) {
                    item.IsSelected = true;
                }
            }

            if (toremove.Count > 0) {
                foreach (ComponentPresetListItem item in toremove) {
                    items.Remove(item);
                }
            }
        }

        #endregion

        #region OPERATIONS

        private void OnAwakeEditor()
        {
            _SetupAutoKeyframing();
            Setup();
        }

        public override void OnNewInstance()
        {
            base.OnNewInstance();
        }

        public override void Refresh()
        {
            //Debug.Log($"{name}.Refresh");
            _Icon = null; // force icon to refresh

            if (!isActiveAndEnabled) return;

            Setup();
            Timeflow.Active.SetupDirector();

#if !TIMEFLOW_OVERRIDES_DISABLED
            if (OverrideData != null) OverrideData.Refresh(transform);
#endif
            _Renderer = null;

            if (HasBehaviors) {
                foreach (TimeflowBehavior behavior in Behaviors) {
                    if (behavior.isActiveAndEnabled) {
                        behavior.Refresh();
                    }
                }
            }

            if (Children != null) {
                foreach (TimeflowObject child in Children) {
                    if (child.isActiveAndEnabled) {
                        child.Refresh();
                    }
                }
            }

            DoUpdate();
        }

        public override void OnHierarchyChange()
        {
            base.OnHierarchyChange();
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.OnHierarchyChange();
                }
            }
        }

        public void GetObjectAndChildrenDisplayedRecursive(ref List<TimeflowObject> childObjects)
        {
            if (!childObjects.Contains(this)) {
                childObjects.Add(this);
            }
            if (ShowChildren && Children != null) {
                foreach (TimeflowObject child in Children) {
                    child.GetObjectAndChildrenDisplayedRecursive(ref childObjects);
                }
            }
        }

        public override void InsertTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            foreach (TimeflowChannel ch in AllChannels) {
                if (isGlobal || !ch.IsLocked) {
                    UndoUtil.Undo(ch.Behavior, "Insert Time", true);
                    ch.InsertTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public override void DuplicateTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            foreach (TimeflowChannel ch in AllChannels) {
                if (isGlobal || !ch.IsLocked) {
                    UndoUtil.Undo(ch.Behavior, "Duplicate Time in Work Area", true);
                    ch.DuplicateTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public override void DeleteTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            foreach (TimeflowChannel ch in AllChannels) {
                if (isGlobal || !ch.IsLocked) {
                    UndoUtil.Undo(ch.Behavior, "Delete Time in Work Area", true);
                    ch.DeleteTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public override void ClearTime(float start, float end, bool isLocalTime, bool isGlobal, TimeflowView.SelectionModes mode)
        {
            foreach (TimeflowChannel ch in AllChannels) {
                bool canSelect = true;
                if (!isGlobal) {
                    if (IsLocked) {
                        canSelect = false;
                    }
                    else
                    if (mode == TimeflowView.SelectionModes.KeyframesOnly) {
                        canSelect = !ch.IsTrack;
                    }
                    else
                    if (mode == TimeflowView.SelectionModes.TracksOnly) {
                        canSelect = ch.IsTrack;
                    }
                }
                if (canSelect) {
                    UndoUtil.Undo(ch.Behavior, "Delete Time in Work Area", true);
                    ch.ClearTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public override void ScaleTime(float scale)
        {
            base.ScaleTime(scale);
            UndoUtil.Undo(this, "Scale Time");

            if (Track != null && !Track.AutoFullLength) {
                Track.ScaleTime(scale);
            }

            GetBehaviors();
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.ScaleTime(scale);
                }
            }
        }

        /// <summary>
        /// Removes all of the TimeflowBehavior components from the object. 
        /// </summary>
        public void RemoveAllTimeflowBehaviors()
        {
            TimeflowBehavior[] behaviors = GetComponents<TimeflowBehavior>();
            foreach (TimeflowBehavior b in behaviors) {
                UndoUtil.UndoDestroy(b);
            }
            Keyframer = null;
            Behaviors = null;
            HasBehaviors = false;

            Timeflow.Active.Refresh();
        }

        public override void OnKeyChange()
        {
            base.OnKeyChange();
            OnTrackChange();
        }

        public override void OnTrackChange()
        {
            base.OnTrackChange();

            if (Track == null) return;
            Track.Object = this;
            Track.Behavior = this;
            Track.UpdateAutoFullLength();

            if (BehaviorsEnabled && Behaviors != null && Behaviors.Count > 0) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b == null || !b.Enabled) continue;
                    b.OnTrackChange();
                }
            }
            if (Children != null) {
                foreach (TimeflowObject child in Children) {
                    if (child == null || !child.Enabled) continue;
                    child.OnTrackChange();
                }
            }
        }

        public override void OnPresetApplied(AdvancedPreset objPreset = null, ComponentPreset compPreset = null)
        {
            //Debug.Log($"{name}.OnPresetApplied: {objPreset?.Name ?? "null"} {compPreset?.Name ?? "null"}", gameObject);
            //if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
            //    if (compPreset != null) {
            //        Track.GUIColor = compPreset.Color;
            //    }
            //    else
            //    if (objPreset != null) {
            //        Track.GUIColor = objPreset.Color;
            //    }
            //    IsDisplayed = true;
            //    IsSelected = true;
            //    if(Timeflow.Active != null && Timeflow.Active.Display != null) {
            //        Timeflow.Active.Display.AddObjectToDisplay(gameObject);
            //    }
            //}
        }

        #endregion

        #region GUI

        public override Color GUIColor {
            get {
                if (Track != null) {
                    return Track.GUIColor;
                }
                // No channels means something is wrong. The first channel is always the track channel.
                if (Timeflow.Active != null && Timeflow.Active.View != null) {
                    Timeflow.Active.View.NeedsRefresh = true;
                }
                return Color.black;
            }
            set {
                if (Track != null) {
                    Track.GUIColor = value;
                }
            }
        }

        public void SetGUIColorRecursive(Color color)
        {
            if(Track != null) Track.GUIColor = color;
            if (AllChannels != null && AllChannels.Count > 0) {
                foreach (var channel in AllChannels) {
                    if (channel != null) {
                        channel.GUIColor = color;
                    }
                }
            }
        }

        public override Texture2D Icon {
            get {
                if (_Icon == null) {
                    GetIcon();
                }
                return _Icon;
            }
        }

        public Texture2D GetIcon()
        {
            Component[] components = gameObject.GetComponents<Component>();
            if (_IconIndex > -1 && _IconIndex < components.Length) {
                _Icon = EditorGUIUtility.ObjectContent(components[_IconIndex], typeof(Transform)).image as Texture2D;
            }
            else
            if (components.Length == 1) {
                _Icon = EditorGUIUtility.ObjectContent(components[0], typeof(Transform)).image as Texture2D;
            }
            else {
                if (components[0] is not RectTransform) {
                    Component component = components[1];
                    if (component != null) {
                        _Icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image as Texture2D;
                    }
                    else {
                        _Icon = AxonUI.Icons.Warning;
                    }
                }
                else {
                    _Icon = DetermineUIIcon(components);
                }
            }
            return _Icon;
        }

        private Texture2D DetermineUIIcon(Component[] components)
        {
            if (components.Length < 2) return null;

            int index = -1;
            if (components.Length >= 4 && components[1] is CanvasRenderer && components[2] is UnityEngine.UI.Image) {
                index = 3;
            }
            else
            if (components.Length >= 3 && components[1] is CanvasRenderer) {
                index = 2;
            }
            else
            if (components.Length >= 2) {
                index = 1;
            }

            if (index != -1 && index < components.Length) {
                Component mainComponent = components[index];
                if (mainComponent == null) {
                    return AxonUI.Icons.Warning;
                }
                return EditorGUIUtility.ObjectContent(mainComponent, mainComponent.GetType()).image as Texture2D;
            }
            return null;
        }

        protected virtual GUIStyle IconStyle {
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

        public void SelectComponentIcon()
        {
            Component[] components = gameObject.GetComponents<Component>();

            GenericMenu menu = new GenericMenu();
            int i = 0;
            menu.AddItem(new GUIContent("Default"), _IconIndex == -1, () => {
                _IconIndex = -1;
                GetIcon();
            });
            foreach (Component component in components) {
                if (component == null) continue;
                int index = i;
                menu.AddItem(new GUIContent(component.GetType().Name), _IconIndex == index, () => {
                    _IconIndex = index;
                    GetIcon();
                });
                i++;
            }
            menu.ShowAsContext();
        }

        public void AssignComponentIcon(Component component)
        {
            Component[] components = gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                if (components[i] == component) {
                    _IconIndex = i;
                    GetIcon();
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true if the provided position (ex. mouse position) is within the GUI rect. 
        /// </summary>
        /// <param name="pos"></param>
        public bool HitTest(Vector2 pos)
        {
            bool hit = false;
            if (IsSelectable) {
                hit = GUISelectRect.Contains(pos);
                //if(DebugEnabled) Debug.Log($"{name}.HitTest:{hit} GUISelectRect:{GUISelectRect}", gameObject);
            }
            return hit;
        }

        public void StartEditingName()
        {
            if (!IsEditingName) {
                IsEditingName = true;
                AxonGUI.FocusControl("EditObjectName");
                tempEditName = gameObject.name;
                //Debug.Log($"StartEditingName:{tempEditName}");
            }
        }

        public virtual void StopEditingName(bool commit = true)
        {
            if (IsEditingName) {
                IsEditingName = false;

                if (commit) {
                    UndoUtil.Undo(gameObject, "Edit Name");
                    gameObject.name = tempEditName;
                }
            }
        }

        /// <summary>
        /// Handles drawing the GUI for the object row in the hierarchy view of the Timeflow window
        /// </summary>
        public virtual void GUIHierarchySwitches()
        {
            if (gameObject == null) return;
            Rect rect = new Rect(GUIRect);
            float yOffset = GUIRect.y + (GUIRect.height * 0.5f - 8f);
            rect.x = 2f;
            rect.y = yOffset;
            rect.width = rect.height = 16;

            if (IsSelected) {
                Rect selectedRect = GUIRect;
                selectedRect.width = 2;

                GUI.color = AxonColor.Selected;
                GUI.Box(selectedRect, GUIContent.none, AxonUI.SolidStyle);
            }

            #region LOCK
            bool islock = IsLocked;
            GUI.color = Color.white;
            GUI.Box(rect, GUIContent.none, AxonUI.DarkBoxStyle);
            if (Timeflow.Active.Input.Button(rect, AxonUI.DisplayUnlockedOnlyLabel, IsLocked ? AxonUI.LockOnStyle : AxonUI.LockOffFadedStyle, TimeflowViewInput.SwitchesLockPaintIndex, ref islock)) {
                if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                    Timeflow.Active.View.LockSelectedObjects(islock, EditorInput.IsShift);
                }
                else {
                    Timeflow.Active.View.LockObject(this, islock, EditorInput.IsShift);
                }
                Timeflow.Active.Input.InputHandled = true;
            }
            rect.x += rect.width + 1;
            #endregion

            GUI.color = IsLocked ? AxonColor.LockedOverlay : AxonColor.Default;
            GUIStyle style;

            #region VISIBILITY
            rect.y = yOffset;
            if (gameObject.activeSelf) {
                if (!gameObject.activeInHierarchy && !Timeflow.Active.IsDisplayingPrefab) {
                    style = AxonUI.VisibilityHalfStyle;
                }
                else {
                    style = AxonUI.VisibilityOnStyle;
                }
            }
            else {
                style = AxonUI.VisibilityOffFadedStyle;
            }
            bool visible = gameObject.activeSelf;
            GUI.Box(rect, GUIContent.none, AxonUI.DarkBoxStyle);
            if (Timeflow.Active.Input.Button(rect, AxonUI.DisplayVisibileLabel, style, TimeflowViewInput.SwitchesEnablePaintIndex, ref visible)) {
                if (!IsLocked) {
                    if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                        Timeflow.Active.View.ActivateSelectedObjects(visible, EditorInput.IsShift);
                    }
                    else {
                        Timeflow.Active.View.ActivateObject(this, visible, EditorInput.IsShift);
                    }
                    Timeflow.Active.Input.InputHandled = true;
                }
            }
            rect.x += rect.width + 1;
            #endregion

            #region ENABLE BEHAVIORS
            GUIStyle enableTimeStyle = AxonUI.BehaviorOnStyle;
            if (!BehaviorsEnabled) {
                if (HasBehaviors) {
                    enableTimeStyle = AxonUI.BehaviorOffFadedStyle;
                }
                else {
                    enableTimeStyle = AxonUI.BehaviorOffStyle;
                }
            }

            rect.width = rect.height = 16;
            bool enabled = BehaviorsEnabled;
            GUI.Box(rect, GUIContent.none, AxonUI.DarkBoxStyle);
            if (Timeflow.Active.Input.Button(rect, AxonUI.TimeModeLabel, enableTimeStyle, TimeflowViewInput.SwitchesEnablePaintIndex, ref enabled) && !IsLocked) {
                if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                    if (EditorInput.IsControl) {
                        int option = EditorUtility.DisplayDialogComplex("Destroy All Timeflow Behaviors", "Are you sure you want to remove all time-based behaviors on the selected object(s)?", "Ok", "Cancel", "");
                        if (option == 0) {
                            Timeflow.Active.View.RemoveAllBehaviorsOnSelectedObjects();
                        }
                    }
                    else {
                        Timeflow.Active.View.EnableBehaviorsForSelectedObjects(!BehaviorsEnabled, EditorInput.IsShift);
                    }
                }
                else {
                    UndoUtil.Undo(this, "Destroy All Timeflow Behaviors");
                    if (!Timeflow.Active.Input.IsButtonPainting && HasBehaviors && EditorInput.IsControl) {
                        int option = EditorUtility.DisplayDialogComplex("Destroy All Timeflow Behaviors", "Are you sure you want to remove all time-based behaviors on this object?", "Ok", "Cancel", "");
                        if (option == 0) {
                            RemoveAllTimeflowBehaviors();
                            Timeflow.Active.Refresh(true);
                        }
                    }
                    else {
                        Timeflow.Active.View.EnableBehaviors(this, enabled, EditorInput.IsShift);
                    }
                }
                Timeflow.Active.Input.InputHandled = true;
                Timeflow.Active.View.Display.ApplyFilter();
            }
            rect.x += rect.width + 1;
            #endregion

            bool isAlt = EditorInput.IsAlt;
            GUI.Box(rect, GUIContent.none, AxonUI.DarkBoxStyle);

            if (DisplaySolo) {
                GUI.color = AxonColor.Solo;
                if (GUI.Button(rect, AxonUI.DisplayShowChannelsLabel, AxonUI.DisplayChannelSoloOnStyle)) {
                    DisplaySolo = !DisplaySolo;
                    if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                        Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplaySolo, EditorInput.IsShift);
                    }
                    else {
                        Timeflow.Active.View.Display.DisplayChannels(this, DisplaySolo, EditorInput.IsShift);
                    }
                    Timeflow.Active.Input.InputHandled = true;
                    Timeflow.Active.View.Display.ApplyFilter();
                }
            }
            else
            if (Timeflow.Active.View.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Objects || isAlt) {
                GUI.color = new Color(1f, 1f, 1f, isAlt ? 1f : 0.1f);
                if (GUI.Button(rect, AxonUI.DisplayShowChannelsLabel, isAlt ? DisplaySolo ? AxonUI.DisplayChannelSoloOnStyle : AxonUI.DisplayChannelSoloOffStyle : AxonUI.DisplayChannelOnStyle)) {
                    if (isAlt) {
                        DisplaySolo = !DisplaySolo;
                        if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                            Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplaySolo, EditorInput.IsShift);
                        }
                        else {
                            Timeflow.Active.View.Display.DisplayChannels(this, DisplaySolo, EditorInput.IsShift);
                        }
                    }
                    else
                    if (DisplaySolo) {
                        DisplaySolo = false;
                        DisplayChannels = true;
                        if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                            Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplaySolo, EditorInput.IsShift);
                        }
                    }
                    else
                    if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                        DisplaySolo = !DisplaySolo;
                        Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplaySolo, EditorInput.IsShift);
                    }
                    else {
                        Timeflow.Active.View.Display.DisplayChannels(this, DisplaySolo, EditorInput.IsShift);
                    }

                    Timeflow.Active.Input.InputHandled = true;
                    Timeflow.Active.View.Display.ApplyFilter();
                }
            }
            else {
                if (Timeflow.Active.View.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.None) {
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                }

                GUIStyle displayChannelStyle = DisplayChannels ? AxonUI.DisplayChannelOnStyle : AxonUI.DisplayChannelOffStyle;
                bool displayChannels = DisplayChannels;
                if (Timeflow.Active.Input.Button(rect, AxonUI.DisplayShowChannelsLabel, displayChannelStyle, TimeflowViewInput.SwitchesDisplayPaintIndex, ref displayChannels) && !IsLocked) {
                    if (isAlt) {
                        DisplaySolo = !DisplaySolo;
                        if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                            Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplaySolo, EditorInput.IsShift);
                        }
                    }
                    else
                    if (DisplaySolo) {
                        DisplaySolo = false;
                        DisplayChannels = true;
                        if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                            Timeflow.Active.View.Display.DisplaySoloSelectedObjects(DisplaySolo, EditorInput.IsShift);
                        }
                    }
                    else {
                        DisplayChannels = !DisplayChannels;
                        if (!Timeflow.Active.Input.IsButtonPainting && IsSelected) {
                            if (Event.current.control || Event.current.command) {
                                Timeflow.Active.View.Display.DisplayChannelsOnAllObjects(DisplayChannels, false, true);
                            }
                            else {
                                Timeflow.Active.View.Display.DisplayChannelsOnSelectedObjects(DisplayChannels, EditorInput.IsShift);
                            }
                        }
                        else {
                            Timeflow.Active.View.Display.DisplayChannels(this, DisplayChannels, EditorInput.IsShift);
                        }
                    }
                    Timeflow.Active.Input.InputHandled = true;
                    Timeflow.Active.View.Display.ApplyFilter();
                }
                GUI.color = AxonColor.Default;
            }
            rect.x += rect.width + 1f;

            GUI.color = Color.white;
            GUI.Box(rect, GUIContent.none, AxonUI.DarkBoxStyle);

            GUIColorRect = rect;
            GUIColorRect.x += 2;
            GUIColorRect.y += 2;
            GUIColorRect.width = GUIColorRect.height = (int)rect.width - 4;
            bool isColor = true;
            GUI.color = GUIColor;
            if (Timeflow.Active.Input.Button(GUIColorRect, AxonUI.TrackColorLabel, AxonUI.SolidStyle, 0, ref isColor)) {
                TrackColorMenu.Init(this);
                Timeflow.Active.Input.EventMode = TimeflowViewInput.EventModes.ColorSelect;
                Timeflow.Active.Input.InputHandled = true;
                Event.current.Use();
            }
            rect.x += rect.width;

            GUISwitchesPresets();
        }

        private void GUISwitchesPresets()
        {
            if (!Timeflow.Layout.ShowAdvancedPresets) return;

            GUI.color = Color.white;
            Rect switchesPresetRect = new Rect(GUIColorRect.x + GUIColorRect.width + 2, GUIColorRect.y - 2, 16, 16);
            if (GUI.Button(switchesPresetRect, GUIContent.none, AxonUI.AdvancedPresetStyle)) {
                AdvancedPresetsPopup.Invoke(this);
            }
        }

        private void GUIChannelSelected()
        {
            if (IsSelected) {
                GUIRect full = GUIRect;
                full.x = 0;
                full.width = Timeflow.Active.Layout.Values.Width;
                GUI.color = IsSelected ? GUIColor : GUIColorWhite;
                GUI.Box(full, "", AxonUI.SubObjectSelectedStyle);
                GUI.color = GUIColorWhite;
            }
        }

        public virtual void GUIChannelValues()
        {
            GUIChannelSelected();
            if (BehaviorsEnabled) {
                float w = Timeflow.Active.Layout.Values.Width - 10;
                Rect rect = new Rect(GUIRect);
                rect.y = GUIRect.y + (GUIRect.height * 0.5f - 8f) - 1f;
                rect.x = 5;
                rect.width = w;

                TimeflowTrack.VisibilityModes mode = (TimeflowTrack.VisibilityModes)EditorGUI.EnumPopup(rect, new GUIContent("", "Determines how the track channel affects objects visibility."), Track.VisibilityMode);
                if (mode != Track.VisibilityMode) {
                    Track.VisibilityMode = mode;

                    if (IsSelected && Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                        // Apply mode to all selected objects
                        foreach (GameObject obj in Selection.gameObjects) {
                            if (obj.TryGetComponent<TimeflowObject>(out TimeflowObject tobj)) {
                                if (tobj != null) {
                                    tobj.Track.VisibilityMode = mode;
                                }
                            }
                        }
                    }
                    Refresh();
                }
            }
        }

        public virtual void GUITimeOffsetColumn()
        {
            GUIChannelSelected();
            float w = Timeflow.Active.Layout.TimeOffset.Width / 2f;
            float y = GUIRect.y + (GUIRect.height * 0.5f - 8f);
            Rect rect = new Rect(GUIRect);
            rect.x = 0;
            rect.y = y;

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 13;
            float checkboxWidth = 16f;

            rect.height = checkboxWidth;
            rect.width = checkboxWidth;

            if (GUI.Button(rect, new GUIContent("", "Lock or unlock the tracks for this object"),
                Track.IsTrackLocked ? AxonUI.LockOnStyle : AxonUI.LockOffStyle)) {
                Track.IsTrackLocked = !Track.IsTrackLocked;
                if (!Track.IsTrackLocked) {
                    // By default, disable auto full length otherwise it may confuse users
                    Track.IsLocked = false;
                }
            }

            EditorGUI.BeginDisabledGroup(Track.IsTrackLocked);

            rect.x += rect.width - 4;
            rect.height = 20;
            rect.width = w - checkboxWidth - 4f;

            float offset = EditorGUI.FloatField(rect, ":", TimeOffset);
            if (offset != TimeOffset) {
                UndoUtil.Undo(this, "Set Time Offset");
                TimeOffset = offset;
            }

            rect.x += rect.width + 4f;
            rect.width = checkboxWidth;
            rect.height = checkboxWidth;

            if (GUI.Button(rect, new GUIContent("", "Enables modifying the time offset when dragging the track. Or if disabled, tracks are moved independently like keyframes."),
                CanDragTimeOffset ? AxonUI.TrackDragTimeOffsetOnStyle : AxonUI.TrackDragTimeOffsetOffStyle)) {
                CanDragTimeOffset = !CanDragTimeOffset;
                if (CanDragTimeOffset) {
                    // By default, disable auto full length otherwise it may confuse users
                    Track.IsLocked = false;
                }
            }

            rect.x += rect.width + 4f;
            rect.width = 50f;

            float timeScale = EditorGUI.FloatField(rect, new GUIContent("*", "Time Scale"), TimeScale);
            if (timeScale != TimeScale) {
                UndoUtil.Undo(this, "Set Time Scale");
                TimeScale = timeScale;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /// <summary>
        /// Handles drawing the GUI for the object row in the hierarchy view of the Timeflow window
        /// </summary>
        public virtual void GUIHierarchy()
        {
            if (this == null || transform == null) return;
            Rect rect = GUIRect;

            if (GUICull) return;
            Color white = IsLocked ? GUIColor : AxonColor.Default;
            Color color = ((IsSelected || IsChannelSelected) && Track != null) ? MathUtil.Interpolate(Track.GUIColor, white, IsSelected ? 0.2f : 0.5f) : GUIColor;
            color.a = 1f;
            GUI.color = color;

            GUIStyle style = IsSelected ? (Timeflow.Active.View.HasFocus ? AxonUI.ObjectSelectedStyle : AxonUI.ObjectSelectedDefocusStyle) : AxonUI.ObjectStyle;

            if (typeof(Timeflow).IsAssignableFrom(GetType())) {
                if (!IsSelected) {
                    style = AxonUI.ObjectSelectedDefocusStyle;
                    // Shade other Timeflow instances slightly so they are more obvious
                    color = GUIColor;// MathUtil.Interpolate(GUIColor, GUIColorWhite, 0.2f);
                    color.a = 0.25f;
                }
            }
            if (Timeflow.Active.Input.DragObject == this) {
                style = AxonUI.ObjectDragStyle;
                style.normal.textColor = AxonColor.LabelDrag;
            }

            if (IsLocked) {
                GUI.color = AxonColor.LockedUnderlay;
                GUI.Box(rect, GUIContent.none, GUI.skin.box);
            }
            GUI.color = IsLocked ? AxonColor.LockedUnderlay : color;
            GUI.Box(rect, "", style);

            if (Track.GUIHeightLocked) {
                GUI.color = AxonColor.ChannelHeightLocked;
                Rect r = rect;
                r.y = rect.y + rect.height - 2;
                r.height = 2;
                GUI.Box(r, "", AxonUI.TrackEmptyStyle);
            }

            rect.y = GUIRect.y + (GUIRect.height * 0.5f - 8f);
            int iconLeft = 0;
            if (Timeflow.Active.Layout.ShowSwitches) {
                if (Timeflow.Active.Layout.Switches != null) {
                    rect.x += Timeflow.Active.Layout.Switches.Width;
                    iconLeft += Timeflow.Active.Layout.Switches.Width + 3;
                }
            }

            if (TimeflowPreferences.Current.ShowComponentIcons) {
                GUI.color = GUIColorWhite;
                Rect r = new Rect(iconLeft, rect.y, 16, 16);
                GUI.Box(r, GUIContent.none, AxonUI.DarkBoxStyle);
                if (GUI.Button(r, GUIContent.none, IconStyle)) {
                    SelectComponentIcon();
                }
                rect.x += r.width;
            }

            #region COLLAPSE
            float nameIndent = Timeflow.Active.View.GetIndent();
            rect.x += nameIndent;
            rect.width = AxonUI.FoldoutDownStyle.normal.background.width;
            rect.height = AxonUI.FoldoutDownStyle.normal.background.height;
            GUIRectFoldout = rect;

            GUI.color = GUIColorWhite;
            bool hasBehaviors = BehaviorsEnabled && HasBehaviors && AllChannels.Count > 1;

            if (transform.childCount > 0 || hasBehaviors) {
                GUIStyle foldoutStyle = IsCollapsed ? AxonUI.FoldoutUpStyle : AxonUI.FoldoutDownStyle;
                if (!ShowChildren) {
                    foldoutStyle = IsCollapsed ? AxonUI.FoldoutUpPlusStyle : AxonUI.FoldoutDownPlusStyle;
                }
                if (GUI.Button(GUIRectFoldout, GUIContent.none, foldoutStyle)) {
                    Timeflow.Active.View.CollapseRecursive(this, !IsCollapsed, Event.current.alt || Event.current.shift);
                    EditorInput.SetEventUsed();
                }
            }
            else {
                if (GUI.Button(GUIRectFoldout, GUIContent.none, AxonUI.FoldoutNoneStyle)) {
                    if (IsSelected) {
                        Timeflow.Active.View.CollapseSelected(!IsCollapsed, Event.current.alt || Event.current.shift);
                    }
                    else {
                        Timeflow.Active.View.CollapseRecursive(this, !IsCollapsed, Event.current.alt || Event.current.shift);
                    }
                    EditorInput.SetEventUsed();
                }
            }

            rect.x += rect.width;
            #endregion

            #region NAME

            if (Timeflow.Active.Layout.Hierarchy == null) return;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.clipping = TextClipping.Clip;

            rect.y -= 2f;
            rect.width = Timeflow.Active.Layout.Hierarchy.Width - (rect.x + 16);
            rect.height = 20;


            if (IsEditingName) {
                GUI.color = AxonColor.Default;

                labelStyle = new GUIStyle(GUI.skin.textField);

                GUI.SetNextControlName("EditObjectName");
                Timeflow.Active.tempEditRect = rect;
                Timeflow.Active.tempEditContainingRect = Timeflow.Active.Layout.Hierarchy;
                tempEditName = GUI.TextField(rect, tempEditName, AxonUI.ObjectTextFieldStyle);
            }
            else {
                GUI.color = IsSelected ? AxonColor.SelectedText : AxonColor.Default;

                if (Timeflow.Active.Input.DragObject == this) {
                    labelStyle.normal.textColor = AxonColor.LabelDrag;
                }
                else
                if (!gameObject.activeInHierarchy) {
                    labelStyle.normal.textColor = IsSelected ? AxonColor.LabelSelected : AxonColor.Label;
                }
                else
                if (IsSelected) {
                    labelStyle.normal.textColor = AxonColor.LabelSelected;
                }
                if (_IsPrefab) {
                    labelStyle.normal.textColor = AxonColor.Prefab;
                }
                labelStyle.hover.textColor = labelStyle.normal.textColor;

                GUI.Label(rect, new GUIContent(gameObject.name), labelStyle);
            }

            rect.x += rect.width;
            #endregion

            if (_IsPrefabRoot || _IsTimeflowInstance) {
                rect.x -= 24f;
                rect.width = 24;
                rect.height = 24;
                if (GUI.Button(rect, AxonUI.PrefabOpenLabel, AxonUI.TabNextStyle)) {
                    if (_IsPrefabRoot) {
                        PrefabUtil.OpenPrefab(gameObject);
                    }
                    else {
                        if (gameObject.TryGetComponent<Timeflow>(out var t)) {
                            t.IsActive = true;
                        }
                    }
                }
            }
            #region INDICATE SELECTED TRACK KEYS

            if (Timeflow.Active.View.SelectedKeys != null) {
                int c = 0;
                bool keysSelected = false;
                if (Track != null && Track.Keys != null) {
                    foreach (Keyframe k in Track.Keys) {
                        if (Timeflow.Active.View.SelectedKeys.Contains(k)) {
                            keysSelected = true;
                            c++;
                        }
                    }
                }
                if (keysSelected) {
                    rect.x -= 20f;
                    rect.width = 20;
                    GUI.Label(rect, new GUIContent("" + c), AxonUI.TimeFramesSmallStyle);
                }
            }

            #endregion

            #region BEHAVIOR ICONS

            bool useLabels = false;
            List<Texture2D> icons = new List<Texture2D>();
            List<string> labels = new List<string>();

            if (hasBehaviors) {
                List<TimeflowBehavior> behaviors = new List<TimeflowBehavior>();
                foreach (TimeflowBehavior b in Behaviors) {
                    //b != null && 
                    if ((b.Channels == null || b.Channels.Count == 0)) {
                        /// Commented out for future consideration.
                        //#if UNITY_2021_1_OR_NEWER
                        //Texture2D icon = EditorGUIUtility.GetIconForObject(b);
                        //if (icon != null) {
                        //    icons.Add(icon);
                        //    behaviors.Add(b);
                        //}
                        //#else
                        labels.Add(b.name);
                        behaviors.Add(b);
                        //#endif
                    }
                }
                if (useLabels) {
                    if (labels.Count > 0) {
                        float size = 20f;
                        float linkLine = (Timeflow.Active.Layout.Hierarchy.Left + Timeflow.Active.Layout.Hierarchy.Width) - 64f;
                        rect.x = linkLine - ((size + 1f) * icons.Count);
                        Rect iconRect = new Rect(rect.x, rect.y + 4f, size, size);
                        int i = 0;
                        foreach (string label in labels) {
                            TimeflowBehavior b = behaviors[i];
                            GUI.color = new Color(1f, 1f, 1f, b.Enabled ? 0.75f : 0.1f);
                            if (GUI.Button(iconRect, label.Substring(0, 1))) {
                                b.ViewInspector();
                            }
                            GUI.color = AxonColor.Default;
                            iconRect.x += size;
                            i++;
                        }
                    }
                }
                else
                if (icons.Count > 0) {
                    float size = 12f;
                    float linkLine = (Timeflow.Active.Layout.Hierarchy.Left + Timeflow.Active.Layout.Hierarchy.Width) - 64f;
                    rect.x = linkLine - ((size + 1f) * icons.Count);
                    Rect iconRect = new Rect(rect.x, rect.y + 4f, size, size);
                    int i = 0;
                    foreach (Texture2D icon in icons) {
                        TimeflowBehavior b = behaviors[i];
                        GUI.color = new Color(1f, 1f, 1f, b.Enabled ? 0.75f : 0.1f);
                        if (AxonGUI.ButtonTexture(iconRect, icon, icon, "Select " + b.Name, new RectOffset(0, 0, 0, 0), true)) {
                            b.ViewInspector();
                        }
                        GUI.color = AxonColor.Default;
                        iconRect.x += size;
                        i++;
                    }
                }
            }

            #endregion

        }


        #endregion
    }

    public class SortTimeflowObjectInViewOrder : IComparer<TimeflowObject>
    {
        public int Compare(TimeflowObject a, TimeflowObject b)
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
