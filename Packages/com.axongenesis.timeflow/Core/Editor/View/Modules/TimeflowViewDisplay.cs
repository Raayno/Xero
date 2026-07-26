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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    [Serializable]
    public partial class TimeflowViewDisplay : TimeflowViewModuleBase
    {
        private const int _addBarHeightLocked = 25;
        private const int _addBarHeightSelection = 50;

        private const int _switchPadLeft = 2;
        private const int _switchPadTop = 2;
        private const int _menuDisplayNameLeftPad = 2;
        private const int _menuDisplayNameHeight = 24;
        private const int _menuDisplayNameRightPad = 84;
        private const int _menuDisplayPrevRightOffset = 90;
        private const int _menuDisplayNextPadLeft = 2;
        private const int _menuDisplayLockOffset = 4;
        private const int _menuDisplaySaveOffset = 18;
        private const int _menuIconPadTop = 4;
        private const int _menuAddBarPadWidth = 40;
        private const int _menuAddBarPadTop = 25;

        #region ENUMS

        public enum ObjectModes
        {
            Nothing,
            Everything,
            SelectedObject,
            SelectedGroup,
            SavedDisplay,
            UserControlled
        }

        public enum ChannelModes
        {
            None,
            Displayed,
            Objects,
            Solo
        }

        #endregion

        #region PUBLIC

        public bool EnableTimeScope;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public List<TimeflowObject> Objects;

        [NonSerialized]
        public bool AnyObjectsHidden;

        [NonSerialized]
        public string Name = "None";

        [NonSerialized]
        public int IndexToRename = -1;

        [NonSerialized]
        public bool IsLocked = false;

        [NonSerialized]
        public bool HasChanged = false;

        [NonSerialized]
        public bool IsEditingName = false;


        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField, FormerlySerializedAs("_ObjectMode")]
        private ObjectModes __ObjectMode = ObjectModes.Everything;
        private ObjectModes _ObjectMode {
            get { return __ObjectMode; }
            set {
                if (__ObjectMode != value) {
                    __ObjectMode = value;
                    //Debug.Log($"{Timeflow.name}.ObjectMode:{value}");
                }
            }
        }

        [SerializeField]
        private ChannelModes _ChannelMode = ChannelModes.None;

        [SerializeField]
        private bool _VisibleOnly;

        [SerializeField]
        private bool _UnlockedOnly;

        [SerializeField]
        private bool _LockedOnly;

        [SerializeField]
        private bool _EnabledOnly;

        [SerializeField]
        private int _Index;

        [NonSerialized]
        private GUIRect _addBarArea;

        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private bool lastOn;

        [NonSerialized]
        private bool lastLockedOn;

        [NonSerialized]
        private bool hasEditNameBeenFocused;

        [NonSerialized]
        private bool isDisplayingPrefab;

        [NonSerialized]
        private TimeflowDisplayViewPrefab prefabView;

        [NonSerialized]
        private GUIRect switchLockedRect;

        [NonSerialized]
        private GUIRect switchVisibleRect;

        [NonSerialized]
        private GUIRect switchEnableRect;

        [NonSerialized]
        private GUIRect switchChannelModeRect;

        [NonSerialized]
        private GUIRect switchColorPickerRect;

        [NonSerialized]
        private GUIRect menuDisplayNameRect;

        [NonSerialized]
        private GUIRect menuDisplayPrevRect;

        [NonSerialized]
        private GUIRect menuDisplayNextRect;

        [NonSerialized]
        private GUIRect menuDisplayLockRect;

        [NonSerialized]
        private GUIRect menuDisplaySaveRect;

        [NonSerialized]
        private PrefabStage activePrefabStage;

        [NonSerialized]
        private string activePrefabPath;

        [NonSerialized]
        private bool _IsPrefabMode = false;

        #endregion

        #region CONSTRUCTORS

        public TimeflowViewDisplay(Timeflow timeflow) : base(timeflow) { }

        #endregion

        #region ACCESSORS

        public bool DebugEnabled {
            get {
                return Timeflow.DebugEnabled;
            }
        }

        public bool VisibleOnly {
            get {
                return _VisibleOnly;
            }
            set {
                if (_VisibleOnly != value) {
                    _VisibleOnly = value;
                    ApplyFilter();
                }
            }
        }

        public bool UnlockedOnly {
            get {
                return _UnlockedOnly;
            }
            set {
                if (_UnlockedOnly != value) {
                    _UnlockedOnly = value;
                    ApplyFilter();
                }
            }
        }

        public bool LockedOnly {
            get {
                return _LockedOnly;
            }
            set {
                if (_LockedOnly != value) {
                    _LockedOnly = value;
                    ApplyFilter();
                }
            }
        }

        public bool EnabledOnly {
            get {
                return _EnabledOnly;
            }
            set {
                if (_EnabledOnly != value) {
                    _EnabledOnly = value;
                    ApplyFilter();
                }
            }
        }

        public ChannelModes ChannelMode {
            get {
                return _ChannelMode;
            }
            set {
                if (_ChannelMode != value) {
                    _ChannelMode = value;
                    //Debug.Log($"ChannelMode:{_ChannelMode}");
                    ApplyFilter();
                }
            }
        }

        public List<TimeflowObject> RootObjects {
            get {
                if (Timeflow == null) return null;
                return Timeflow.RootObjects;
            }
            set {
                if (Timeflow.RootObjects != value) {
                    Timeflow.RootObjects = value;
                }
            }
        }

        public List<TimeflowDisplayItem> Displays {
            get {
                return Timeflow.Displays;
            }
            set {
                Timeflow.Displays = value;
            }
        }

        public List<TimeflowChannel> SelectedChannels {
            get {
                return Timeflow.View.SelectedChannels;
            }
            set {
                Timeflow.View.SelectedChannels = value;
            }
        }

        public Vector2 ScrollOffset {
            get {
                return Timeflow.View.ScrollOffset;
            }
            set {
                Timeflow.View.ScrollOffset = value;
            }
        }

        public bool IsPrefabMode {
            get => _IsPrefabMode;
            set {
                if (_IsPrefabMode != value) {
                    _IsPrefabMode = value;
                    OnObjectModeChanged();
                }
            }
        }

        public ObjectModes ObjectMode {
            get {
                return _ObjectMode;
            }
            set {
                if (_ObjectMode != value) {
                    _ObjectMode = value;
                    HasChanged = false;
                    OnObjectModeChanged();
                }
            }
        }

        public int Index {
            get {
                return _Index;
            }
            set {
                if (_Index != value) {
                    _Index = value;
                }
            }
        }

        public bool IsDisplayingPrefab {
            get {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                bool isPrefabMode = prefabStage != null;
                if (isDisplayingPrefab != isPrefabMode || (isPrefabMode && prefabStage.assetPath != activePrefabPath)) {
                    isDisplayingPrefab = isPrefabMode;
                    if (isDisplayingPrefab) {
                        OnOpenPrefab(prefabStage);
                    }
                    else {
                        OnClosePrefab();
                    }
                }

                return isPrefabMode;
            }
        }

        #endregion

        #region LIST OPERATIOS

        public override void Setup(Timeflow timeflow)
        {
            base.Setup(timeflow);
            IndexToRename = -1;
            IsEditingName = false;
        }

        public void Clear()
        {
            Index = -1;
            DisplayNothing();
        }

        public void Edit()
        {
            SelectionUtil.Select(Timeflow.gameObject);
            Timeflow.EditorShowTimeSettings = false;
            Timeflow.EditorShowSettings = false;
            Timeflow.EditorShowDisplayLists = true;
        }

        public void Previous()
        {
            if (IsDisplayingPrefab) return;
            Index--;
            if (Index < 0) {
                Index = Displays.Count - 1;
            }
            Load(Index);
        }

        public void Next()
        {
            if (IsDisplayingPrefab) return;
            Index++;
            if (Index >= Displays.Count) {
                Index = 0;
            }
            Load(Index);
        }

        public void Remove(int index)
        {
            if (Displays != null && index >= 0 && index < Displays.Count) {
                UndoUtil.Undo(Timeflow, "Remove Saved Display", true);
                Displays.RemoveAt(index);
            }
        }

        public void LoadDisplay(TimeflowDisplayItem display, bool select = false)
        {
            int index = 0;
            foreach (TimeflowDisplayItem d in Displays) {
                if (d == display) {
                    break;
                }
                index++;
            }
            Load(index, select);
        }

        public void Load(int index, bool select = false)
        {
            if (IsLocked) return;
            Index = index;
            _ObjectMode = ObjectModes.SavedDisplay;
            if (Displays != null) {
                if (Index < 0) {
                    /// Do nothing
                }
                else
                if (Index >= Displays.Count) {
                    Debug.LogWarning("Invalid recent index:" + index);
                    Index = Displays.Count - 1;
                }
                else {
                    TimeflowDisplayItem display = Displays[Index];
                    if (display == null || display.Objects == null || display.Objects.Count == 0) {
                        Debug.LogWarning("The selected group is null.");
                        Remove(Index);
                    }
                    else {
                        Name = display.Name;
                        DisplayHierarchies(display.Objects);
                        if (select) Selection.objects = display.Objects.ToArray();
                        HasChanged = false;


                        if (display != null && EnableTimeScope) {
                            if (display.IsTimeScopeEnabled) {
                                Timeflow.SetTimeScope(display.TimeScopeStart, display.TimeScopeEnd);
                                Timeflow.IsTimeScopeLocalized = display.IsTimeScopeLocalized;
                                Timeflow.SetTimeScopeColor(display.Objects[0].GUIColor);
                            }
                            else {
                                Timeflow.IsTimeScopeEnabled = false;
                            }
                        }
                    }
                }
            }
        }

        public bool Save(List<TimeflowObject> objects, string name)
        {
            bool saved = false;
            TimeflowDisplayItem display = null;

            // Remove any duplicates
            List<TimeflowObject> objectsToSave = new List<TimeflowObject>();
            foreach (TimeflowObject obj in objects) {
                if (!objectsToSave.Contains(obj) && obj != Timeflow.gameObject) {
                    objectsToSave.Add(obj);
                }
            }

            if (objectsToSave.Count > 0) {
                if (Displays == null) Displays = new List<TimeflowDisplayItem>();
                Index = Displays.FindIndex(s => s.Name == name);
                if (Index < 0) {
                    Index = Displays.Count;
                    display = new TimeflowDisplayItem {
                        Objects = objectsToSave,
                        Name = name
                    };
                    Displays.Add(display);
                }
                else {
                    // Overwrite objects for item with same name
                    display = Displays[Index];
                    display.Objects = objectsToSave;
                }
                saved = true;
            }

            _ObjectMode = ObjectModes.SavedDisplay;

            if (display != null && EnableTimeScope) {
                display.IsTimeScopeEnabled = Timeflow.IsTimeScopeEnabled;
                display.IsTimeScopeLocalized = Timeflow.IsTimeScopeLocalized;
                display.TimeScopeStart = Timeflow.TimeScopeStart;
                display.TimeScopeEnd = Timeflow.TimeScopeEnd;
            }

            if (saved) HasChanged = false;
            return saved;
        }

        public void Save(string name = null)
        {
#if UNITY_2021_1_OR_NEWER
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) {
                //Debug.LogWarning("Prefab objects from the project view cannot be saved in Timeflow display lists.");
                return;
            }
#endif
            if (string.IsNullOrEmpty(name)) name = Name;

            UndoUtil.Undo(Timeflow, "Save Display", true);
            if (RootObjects != null && RootObjects.Count > 0) {
                bool canSave = !IsSaved(RootObjects[0]);
                List<TimeflowObject> objects = new List<TimeflowObject>();

                if (string.IsNullOrEmpty(name)) {
                    foreach (TimeflowObject obj in RootObjects) {
                        if (obj != null) {
                            objects.Add(obj);
                            if (string.IsNullOrEmpty(name)) name = obj.name;
                        }
                    }
                }
                else {
                    // Build object list with named object as the first
                    TimeflowObject first = null;
                    foreach (TimeflowObject obj in RootObjects) {
                        if (obj != null) {
                            if (name.Equals(obj.name)) {
                                first = obj;
                                break;
                            }
                        }
                    }
                    if (first != null) objects.Add(first);
                    foreach (TimeflowObject obj in RootObjects) {
                        if (obj != null && obj != first && !objects.Contains(obj)) {
                            objects.Add(obj);
                        }
                    }
                }

                Save(objects, name);
            }
        }

        public void SaveSelected(string name = null)
        {
            Timeflow.IsActive = true;
            HasChanged = false;
            UndoUtil.Undo(Timeflow, "Save Display", true);
            if (Selection.gameObjects != null) {
                if (Selection.gameObjects.Length == 1 && Displays != null && Displays.Count > 0) {
                    TimeflowObject tobj = Timeflow.SetupTimeflowObject(Selection.gameObjects[0]);
                    int recent = Displays.FindIndex(s => s.Objects.Contains(tobj));
                    if (recent != -1) {
                        Load(recent, false);
                        return;
                    }
                }

                List<TimeflowObject> objects = new List<TimeflowObject>();

                if (string.IsNullOrEmpty(name)) {
                    foreach (GameObject obj in Selection.gameObjects) {
                        TimeflowObject tobj = Timeflow.SetupTimeflowObject(obj);
                        if (tobj != null && !objects.Contains(tobj)) {
                            objects.Add(tobj);
                            if (string.IsNullOrEmpty(name)) name = obj.name;
                        }
                    }
                }
                else {
                    // Build object list with named object as the first
                    TimeflowObject first = null;
                    foreach (GameObject ob in Selection.gameObjects) {
                        TimeflowObject obj = Timeflow.SetupTimeflowObject(ob);
                        if (obj != null && !objects.Contains(obj)) {
                            objects.Add(obj);
                            if (name.Equals(obj.name)) {
                                first = obj;
                                break;
                            }
                        }
                    }
                    if (first != null) objects.Add(first);
                    foreach (GameObject ob in Selection.gameObjects) {
                        TimeflowObject obj = Timeflow.SetupTimeflowObject(ob);
                        if (obj != null && obj != first && !objects.Contains(obj)) {
                            objects.Add(obj);
                        }
                    }
                }

                Save(objects, name);

                Index = Displays.FindIndex(s => s.Name == name);
                Load(Index, false);
            }
        }

        public bool IsSaved(TimeflowObject obj)
        {
            bool isSaved = false;

            if (Displays != null && Displays.Count > 0) {
                foreach (TimeflowDisplayItem d in Displays) {
                    if (d.Objects != null) {
                        foreach (TimeflowObject o in d.Objects) {
                            if (o == obj) {
                                isSaved = true;
                                break;
                            }
                        }
                    }
                    if (isSaved) break;
                }
            }

            return isSaved;
        }

        public int GetSaved(TimeflowObject obj)
        {
            int index = -1;

            if (Displays != null && Displays.Count > 0) {
                int i = 0;
                foreach (TimeflowDisplayItem d in Displays) {
                    if (d.Objects != null) {
                        foreach (TimeflowObject o in d.Objects) {
                            if (o == obj) {
                                index = i;
                                break;
                            }
                        }
                    }
                    if (index > -1) break;
                    i++;
                }
            }

            return index;
        }

        #endregion

        #region MODES

        public void DisplayNothing()
        {
            ObjectMode = ObjectModes.Nothing;
            ChannelMode = ChannelModes.None;
        }

        /// <summary>
        /// Displays all the TimeflowObjects that are in the scene. 
        /// </summary>
        public void DisplayEverything()
        {
            if (ObjectMode == ObjectModes.Everything) {
                ObjectMode = ObjectModes.Nothing; // force to refresh
            }
            ObjectMode = ObjectModes.Everything;
            ChannelMode = ChannelModes.None;
        }

        /// <summary>
        /// Displays all the TimeflowObjects that are in the scene. 
        /// </summary>
        public void DisplaySelectedObject()
        {
            ObjectMode = ObjectModes.SelectedObject;
            ChannelMode = ChannelModes.None;
        }

        /// <summary>
        /// Displays all the TimeflowObjects that are in the scene. 
        /// </summary>
        public void DisplaySelectedGroup()
        {
            ObjectMode = ObjectModes.SelectedGroup;
            ChannelMode = ChannelModes.None;
        }

        /// <summary>
        /// Displays only the selected object hierarchies. 
        /// </summary>
        public void DisplaySelectedHierarchy()
        {
            /// Set to ignore to force change
            if (ObjectMode == ObjectModes.UserControlled) {
                ObjectMode = ObjectModes.Nothing; // force to refresh
            }
            ObjectMode = ObjectModes.UserControlled;
            ChannelMode = ChannelModes.None;
        }

        public void OnOpenPrefab(PrefabStage prefabStage)
        {
            if (activePrefabStage == prefabStage) return;
            activePrefabStage = prefabStage;
            activePrefabPath = prefabStage.assetPath;
            prefabView = new TimeflowDisplayViewPrefab(this, prefabStage);
            // Showing is deferred until the next update to avoid errors reading prefab contents during OnEnable
        }

        public void UpdatePrefabDisplay()
        {
            if (activePrefabStage == null || prefabView == null || prefabView.IsShowing) return;
            prefabView.Show();
        }

        public void OnClosePrefab()
        {
            if (string.IsNullOrEmpty(activePrefabPath)) {
                return;
            }
            activePrefabStage = null;
            activePrefabPath = null;
            if (prefabView != null) prefabView.Hide();
        }

        private void OnObjectModeChanged()
        {
            if (IsLocked) return;
            if (IsPrefabMode) {
                OnObjectModePrefab();
            }
            if (ObjectMode == ObjectModes.Nothing) {
                OnObjectModeNone();
            }
            else
            if (ObjectMode == ObjectModes.Everything) {
                OnObjectModeEverything();
            }
            else
            if (ObjectMode == ObjectModes.SelectedObject) {
                OnObjectModeSelectedObject();
            }
            else
            if (ObjectMode == ObjectModes.SelectedGroup) {
                OnObjectModeSelectedGroup();
            }
            else
            if (ObjectMode == ObjectModes.SavedDisplay) {
                Load(Index, false);
            }
            else
            if (ObjectMode == ObjectModes.UserControlled) {
                OnObjectModeUserControlled();
            }
            ApplyFilter();
#if UNITY_EDITOR
            EditorUtil.SetDirty(Timeflow);
#endif
        }

        private void OnObjectModeNone()
        {
            if (IsLocked) return;
            //Debug.Log($"OnObjectModeNone");
            Index = 0;
            _ObjectMode = ObjectModes.Nothing;
            Name = "Select Display";
            Objects = null;
            RootObjects = null;
            HasChanged = false;
            //Timeflow.Refresh(true);
        }

        private void OnObjectModeEverything()
        {
            if (IsLocked) return;
            Index = 0;
            bool displayChanged = _ObjectMode != ObjectModes.Everything;

            _ObjectMode = ObjectModes.Everything;
            Name = "Everything";
            HasChanged = false;
            ScrollOffset = new Vector2(ScrollOffset.x, 0f);
            Timeflow.View.FindRootObjects(TimeflowObject.GetAllInstances());

            List<TimeflowObject> roots = Timeflow.RootObjects;
            TimeflowObject.SortObjects(ref roots);
            Timeflow.RootObjects = roots;

            GetObjectsDisplayed(false);

            if (displayChanged) {
                Timeflow.View.DeselectAllInternal(); // Prevents holding the selection of objects no longer in view
            }
            ApplyFilter();
        }

        private void OnObjectModeSelectedObject()
        {
            if (IsLocked) return;
            Index = 0;
            _ObjectMode = ObjectModes.SelectedObject;
            HasChanged = false;
            Timeflow.View.OnSelectionChange();
            ApplyFilter();
        }

        private void OnObjectModeSelectedGroup()
        {
            if (IsLocked) return;
            Index = 0;
            _ObjectMode = ObjectModes.SelectedGroup;
            HasChanged = false;
            Timeflow.View.OnSelectionChange();
            ApplyFilter();
        }

        private void OnObjectModeUserControlled()
        {
            if (IsLocked) return;
            List<TimeflowObject> tobjs = new List<TimeflowObject>();
            if (Objects != null && Objects.Count > 0) {
                foreach (TimeflowObject obj in Objects) {
                    tobjs.Add(obj);
                }
            }
            if (Selection.activeGameObject != null) {
                foreach (GameObject obj in Selection.gameObjects) {
                    if (obj.TryGetComponent(out TimeflowObject tobj)) {
                        if (tobj.Timeflow != Timeflow) {
                            // Switch to the other Timeflow view
                            tobj.Timeflow.IsActive = true;
                            tobj.Timeflow.View.Display.OnObjectModeUserControlled();
                            break;
                        }
                        if (!tobjs.Contains(tobj)) tobjs.Add(tobj);
                    }
                }
            }

            _ObjectMode = ObjectModes.UserControlled;
            ScrollOffset = new Vector2(ScrollOffset.x, 0f);

            if (tobjs != null && tobjs.Count > 0) {
                Index = -1;
                Timeflow.View.FindRootObjects(tobjs);
                GetObjectsDisplayed(true);
                if (RootObjects != null && RootObjects.Count > 0) {
                    Name = RootObjects[0].name;
                    HasChanged = true;
                    Timeflow.View.DeselectAllInternal(); // Prevents holding the selection of objects no longer in view
                    if (Timeflow.AutoSaveDisplay) Save(Name);
                }
                else {
                    DisplayEverything();
                }
            }
            ApplyFilter();
        }

        private void OnObjectModePrefab()
        {
            List<TimeflowObject> tobjs = new List<TimeflowObject>();
            if (Selection.gameObjects != null) {
                foreach (GameObject obj in Selection.gameObjects) {
                    if (obj.TryGetComponent(out TimeflowObject tobj)) {
                        if (!tobjs.Contains(tobj)) tobjs.Add(tobj);
                    }
                }
            }
        }

        public void SetUserControlledPreserveView()
        {
            if (ObjectMode == ObjectModes.UserControlled) return;
            List<TimeflowObject> objects = GetObjectsDisplayed(false);
            ObjectMode = ObjectModes.UserControlled;
            DisplayHierarchies(objects);
        }

        #endregion

        #region DISPLAY HELPERS

        private void _DisplayChannels(TimeflowObject obj, bool enabled, bool isRecursive)
        {
            if (obj == null) return;
            //Debug.Log($"DisplayChannels:{obj.name} enabled:{enabled} isRecursive:{isRecursive}");
            UndoUtil.Undo(obj, "Toggle Display Channels");
            bool isSolo = false;
            if (IsAlt) {
                obj.DisplayChannels = true;
                obj.DisplaySolo = enabled;
                isSolo = true;
            }
            else {
                obj.DisplaySolo = false;
                obj.DisplayChannels = enabled;
            }
            if (isRecursive) {
                if (obj.AllChannels != null) {
                    foreach (TimeflowChannel ch in obj.AllChannels) {
                        if (ch == null) continue;
                        if (isSolo) ch.DisplayChannelSolo = enabled;
                        else {
                            ch.DisplayChannelSolo = false;
                            ch.DisplayChannel = enabled;
                        }
                    }
                }
            }
        }

        public void DisplayChannels(TimeflowObject obj, bool enabled, bool isRecursive)
        {
            if (obj == null) return;
            //Debug.Log($"DisplayChannels:{obj.name} enabled:{enabled} isRecursive:{isRecursive}");
            _DisplayChannels(obj, enabled, isRecursive);

            if (isRecursive) {
                List<TimeflowObject> children = ObjectUtil.GetComponentsRecursive<TimeflowObject>(obj.gameObject);
                if (children != null) {
                    foreach (TimeflowObject o in children) {
                        if (o == obj) continue;
                        _DisplayChannels(o, enabled, isRecursive);
                    }
                }
            }
        }

        public void DisplayChannels(TimeflowChannel ch, bool enabled)
        {
            //Debug.Log($"DisplayChannels:{ch.Name} enabled:{enabled} isRecursive:{isRecursive}");
            if (ch == null) return;
            UndoUtil.Undo(ch.Behavior, "Toggle Display Channels");
            if (IsAlt) {
                ch.DisplayChannelSolo = enabled;
            }
            else {
                if (!enabled && ch.DisplayChannelSolo) {
                    ch.DisplayChannel = true;
                    ch.DisplayChannelSolo = false;
                }
                else {
                    ch.DisplayChannel = enabled;
                }
            }
        }

        public void DisplayChannelsOnSelectedObjects(bool enabled, bool isRecursive)
        {
            //Debug.Log($"DisplayChannelsOnSelectedObjects:{enabled} isRecursive:{isRecursive}");
            if (View.SelectedObjects != null && View.SelectedObjects.Count > 0) {
                foreach (TimeflowObject obj in View.SelectedObjects) {
                    DisplayChannels(obj, enabled, isRecursive);
                }
            }
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    DisplayChannels(ch, enabled);
                }
            }
        }

        public void DisplaySoloSelectedObjects(bool enabled, bool isRecursive, bool includeChannels = true)
        {
            //Debug.Log($"DisplaySoloSelectedObjects:{enabled} isRecursive:{isRecursive}");
            if (View.SelectedObjects != null && View.SelectedObjects.Count > 0) {
                foreach (TimeflowObject obj in View.SelectedObjects) {
                    UndoUtil.Undo(obj, "Toggle Solo Mode");
                    obj.DisplayChannels = true;
                    obj.DisplaySolo = enabled;

                    if (isRecursive) {
                        List<TimeflowObject> children = ObjectUtil.GetComponentsRecursive<TimeflowObject>(obj.gameObject);
                        if (children != null) {
                            foreach (TimeflowObject o in children) {
                                if (o == obj) continue;
                                UndoUtil.Undo(o, "Toggle Solo Mode");
                                o.DisplayChannels = true;
                                o.DisplaySolo = enabled;

                                if (includeChannels && o.AllChannels != null) {
                                    foreach (TimeflowChannel ch in o.AllChannels) {
                                        UndoUtil.Undo(ch.Behavior, "Toggle Solo Mode");
                                        ch.DisplayChannel = true;
                                        ch.DisplayChannelSolo = enabled;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    ch.DisplayChannelSolo = enabled;
                }
            }
        }

        public void DisplayChannelsOnAllObjects(bool enabled, bool includeChannels, bool overrideLock)
        {
            if (Objects != null && Objects.Count > 0) {
                foreach (TimeflowObject obj in Objects) {
                    if ((overrideLock || !obj.IsLocked) && obj.IsDisplayed) {
                        obj.DisplayChannels = enabled;
                        obj.DisplaySolo = false;
                    }
                    if (includeChannels) {
                        if (obj.AllChannels != null && obj.AllChannels.Count > 0) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (overrideLock || !ch.IsLocked) {
                                    ch.DisplayChannel = enabled;
                                    ch.DisplayChannelSolo = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region OBJECTS

        /// <summary>
        /// Create a master list of all the TimeflowObjects in view. The order should already be sorted,
        /// but if any changes have occurred to the hierarchy or sorting order by drag-drop, then the
        /// sorting should be updated to ensure proper indexing.
        /// </summary>
        public List<TimeflowObject> GetObjectsDisplayed(bool updateSorting = true)
        {
            //Debug.Log($"GetObjectsDisplayed:{ObjectMode}");
            Timeflow.View.SetupRootObjects();

            Objects = new List<TimeflowObject>();
            if (RootObjects != null && RootObjects.Count > 0) {
                foreach (TimeflowObject obj in RootObjects) {
                    if (obj == null) continue;
                    //Debug.Log($"ROOT:{obj.name}");
                    GetAllObjectsDisplayedRecursive(obj, updateSorting);
                }
            }
            if (updateSorting) {
                TimeflowObject.SortObjects(ref Objects);
            }
            ApplyFilter();

            return Objects;
        }

        /// <summary>
        /// Search through each subhierarchy to create the AllObjects list in the exact order they are
        /// displayed.
        /// </summary>
        private void GetAllObjectsDisplayedRecursive(TimeflowObject obj, bool updateSorting = false)
        {
            if (obj == null) return;
            //Debug.Log($"GetAllObjectsDisplayedRecursive:{obj.name}");

            if (Objects != null && !Objects.Contains(obj)) {
                //Debug.Log($"{Timeflow.name}.Objects.Add:{obj.name} Timeflow:{obj.Timeflow.name}");
                Objects.Add(obj);
            }

            if (obj.Children != null && obj.Children.Count > 0) {
                foreach (TimeflowObject child in obj.Children) {
                    GetAllObjectsDisplayedRecursive(child, updateSorting);
                }
            }
        }

        public List<TimeflowChannel> GetChannelsDisplayed()
        {
            GetObjectsDisplayed();
            List<TimeflowChannel> channels = new List<TimeflowChannel>();
            if (Objects != null) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            channels.Add(ch);
                        }
                    }
                }
            }

            return channels;
        }

        /// <summary>
        /// Show only the currently selected GameObjects in the view. This will also setup TimeflowObject
        /// instances as needed.
        /// </summary>
        public void DisplaySelectedObjects(bool deselect = true)
        {
            //Debug.Log($"DisplaySelectedObjects");
            if (IsLocked) return;

            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                if (EditorUtil.IsPrefabAsset(Selection.activeGameObject)) {
                    // If the selection is a prefab asset, we cannot display it in the view
                    //Debug.LogWarning("Cannot display prefab assets in Timeflow view. Please open the prefab in Prefab Mode to edit.");
                    return;
                }
                RootObjects = null;
                Objects = null;
                ScrollOffset = new Vector2(ScrollOffset.x, 0f);

                TimeflowMenu.ActivateTimeflowForObject(Selection.activeGameObject);

                GameObject[] objs = (GameObject[])Selection.gameObjects.Clone();
                if (objs != null && objs.Length > 0) {
                    List<TimeflowObject> list = new List<TimeflowObject>();
                    foreach (GameObject obj in objs) {
                        if (obj.TryGetComponent<TimeflowObject>(out TimeflowObject tobj)) {
                            tobj = GetObjectToDisplay(tobj, ObjectMode == ObjectModes.SelectedGroup);
                            if (!list.Contains(tobj)) {
                                list.Add(tobj);
                            }
                        }
                    }
                    Timeflow.View.FindRootObjects(list);
                    if (RootObjects != null && RootObjects.Count > 0) {
                        if (Timeflow.AutoSaveDisplay) Save(RootObjects[0].name);
                    }
                }

                GetObjectsDisplayed();
            }
            if (RootObjects != null && RootObjects.Count > 0) {
                Name = RootObjects[0].name;
            }
            else {
                if (Timeflow.WorkAreaEnabled) {
                    Name = "Work Area: " + StringUtil.SecondsToTimecode(Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart);
                }
                else {
                    Name = "Duration: " + StringUtil.SecondsToTimecode(Timeflow.Duration);
                }
            }
            if (deselect) Timeflow.View.DeselectAllInternal(); // Prevents holding the selection of objects no longer in view
        }

        public TimeflowObject GetObjectToDisplay(TimeflowObject obj, bool showParent)
        {
            TimeflowObject display = obj;

            if (showParent && obj.transform.parent != null) {
                TimeflowObject parent = ObjectUtil.GetComponentInSelfOrParent<TimeflowObject>(obj.transform.parent.gameObject);

                if (parent != null) {
                    if (obj != parent && parent != Timeflow) {
                        if (parent.ShowChildren) {
                            display = GetObjectToDisplay(parent, showParent);
                        }
                    }
                }
            }

            return display;
        }

        public void DisplayHierarchies(List<GameObject> objects)
        {
            if (objects == null || objects.Count == 0) return;
            List<TimeflowObject> list = new List<TimeflowObject>();
            foreach (GameObject obj in objects) {
                if (obj == null) continue;
                TimeflowObject tobj = Timeflow.SetupTimeflowObject(obj);
                if (tobj != null) list.Add(tobj);
            }
            DisplayHierarchies(list);
        }

        public void DisplayHierarchies(List<TimeflowObject> objects)
        {
            //Debug.Log($"DisplayHierarchies:{objects.Count} {(objects.Count > 0 ? objects[0].name : "NONE")}");
            RootObjects = null;
            Objects = null;

            Timeflow view = null;

            if (objects != null && objects.Count > 0) {
                ScrollOffset = new Vector2(ScrollOffset.x, 0f);

                TimeflowObject.SortObjects(ref objects);

                int i = 1;
                List<TimeflowObject> list = new List<TimeflowObject>();
                foreach (TimeflowObject tobj in objects) {
                    if (tobj != null) {
                        if (view == null) {
                            view = tobj.Timeflow;
                        }

                        /// Increments of 100 to allow hierarchy changes more easily
                        tobj.SortOrder = i * 100;
                        i++;

                        list.Add(tobj);
                    }
                }
                Timeflow.View.FindRootObjects(list);
                GetObjectsDisplayed();
            }
            if (view != null) {
                Timeflow.Active = view;
                Timeflow.Active.gameObject.SetActive(true);
            }

            // Gather the selected objects currently in view only
            List<GameObject> selected = new List<GameObject>();
            if (Selection.gameObjects != null) {
                foreach (GameObject obj in Selection.gameObjects) {
                    if (IsObjectDisplayed(obj)) {
                        selected.Add(obj);
                    }
                }
            }

            Timeflow.View.DeselectAllInternal(); // Prevents holding the selection of objects no longer in view

            SelectionUtil.Select(selected.ToArray());
            Timeflow.View.OnSelectionChange();
        }

        public float SnapTimeToDisplayed(float time, float snappedTime, float threshold)
        {
            threshold = Mathf.Abs(threshold);
            if (threshold > 0f && Objects != null) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj.AllChannels == null) continue;
                    foreach (TimeflowChannel ch in obj.AllChannels) {
                        if (ch == null || ch.Keys == null || ch.Keys.Count == 0) continue;
                        foreach (Keyframe key in ch.Keys) {
                            if (ch.CustomSnapTime(time, ref threshold, out float snapped)) {
                                snappedTime = snapped;
                            }
                            float k = key.KeyTimeWorld;
                            float dif = Mathf.Abs(k - time);
                            if (dif < threshold) {
                                threshold = dif;
                                snappedTime = k;
                            }
                            else
                            if (ch.IsTrack) {
                                k = key.KeyEndTimeWorld;
                                dif = Mathf.Abs(k - time);
                                if (dif < threshold) {
                                    threshold = dif;
                                    snappedTime = k;
                                }
                            }
                        }
                    }
                }
            }

            if (Timeflow.ShowMarkers && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                foreach (TimeflowMarker marker in Timeflow.MarkerList) {
                    float dif = Mathf.Abs(marker.Time - time);
                    if (dif < threshold) {
                        threshold = dif;
                        snappedTime = marker.Time;
                    }
                }
            }

            return snappedTime;
        }

        /// <summary>
        /// Checks whether a specific GameObject is currently in the Timeflow view.
        /// </summary>
        public bool IsObjectDisplayed(GameObject obj, bool allowTimeflow = true)
        {
            if (obj == null) return false;
            bool inview = false;
            if (allowTimeflow && obj == Timeflow.gameObject) {
                inview = true;
            }
            else
            if (obj != null && Objects != null) {
                foreach (TimeflowObject t in Objects) {
                    if (t != null && t.gameObject == obj) {
                        inview = true;
                        break;
                    }
                }
            }
            return inview;
        }

        /// <summary>
        /// Checks whether the object or any of its parents are in the Timeflow view
        /// </summary>
        public bool IsObjectOrParentDisplayed(GameObject obj)
        {
            if (obj == null) return false;
            bool inview = IsObjectDisplayed(obj);

            if (!inview) {
                Timeflow.SetupTimeflowObject(obj);

                int overflow = 0;
                Transform p = obj.transform.parent;
                while (p != null) {
                    if (IsObjectDisplayed(p.gameObject, false)) {
                        inview = true;
                        break;
                    }
                    p = p.transform.parent;
                    if (p != null && p.gameObject == Timeflow.gameObject) break;
                    overflow++;
                    if (overflow > 10000) {
                        Debug.LogWarning("Loop overflow!");
                    }

                    break;// force check only 1 parent up
                }
            }
            //Debug.Log($"IsObjectOrParentDisplayed:{obj.name} inview:{inview}");
            return inview;
        }

        public void AddSelectedObjectsToDisplay()
        {
            if (Selection.gameObjects != null) {
                foreach (GameObject obj in Selection.gameObjects) {
                    //Debug.Log($"AddSelectedObjectsToDisplay:{obj.name}");
                    AddObjectToDisplay(obj);
                }
            }
        }

        public void AddObjectsToDisplay(List<GameObject> objects)
        {
            if (objects != null) {
                foreach (GameObject obj in objects) {
                    AddObjectToDisplay(obj);
                }
            }
        }

        /// <summary>
        /// Adds the GameObject to the view if it is not already. This will regather the objects to update
        /// the hierarchy view.
        /// </summary>
        public TimeflowObject AddObjectToDisplay(GameObject obj, bool updateSorting = true)
        {
            if (obj == null) {
                Debug.LogWarning("Cannot add null object to display");
                return null;
            }
            //Debug.Log($"AddObjectToDisplay:{obj.name}");
            TimeflowObject t = Timeflow.SetupTimeflowObject(obj);
            if (t == null) {
                //Debug.Log($"AddObjectToDisplay:{obj.name} t==null");
                return null;
            }
            else
            if (IsObjectOrParentDisplayed(obj)) {
                //Debug.Log($"Already in view:{obj.name}");
            }
            else {
                //Debug.Log($"AddObjectToDisplay:{obj.name}");
                if (t != null) {
                    bool add = false;
                    if (RootObjects == null || RootObjects.Count == 0) {
                        RootObjects = new List<TimeflowObject>();
                        if (updateSorting) t.SortOrder = 0;
                        add = true;
                    }
                    else
                    if (t != null && !RootObjects.Contains(t)) {
                        if (updateSorting) t.SortOrder = RootObjects[RootObjects.Count - 1].SortOrder + 1;
                        add = true;
                    }
                    if (add) {
                        if (TimeflowPreferences.Current.AddObjectsToTopOfList) {
                            if (updateSorting) t.SortOrder = 0;
                        }
                        //Debug.Log($"+ROOT:{t.name}"); 
                        Timeflow.AddRootObject(t);
                    }
                    if (Timeflow.AutoSaveDisplay) {
                        if (Index > 0 && Index < Displays.Count) {
                            if (!Displays[Index].Objects.Contains(t)) {
                                Displays[Index].Objects.Add(t);
                            }
                        }
                    }
                    else {
                        HasChanged = true;
                    }
                }
            }
            GetObjectsDisplayed();
            return t;
        }

        private ChannelModes lastChannelMode = ChannelModes.None;

        public void SoloObjectToggle(TimeflowObject obj, bool forceOn = false)
        {
            //Debug.Log($"SoloObjectToggle:{obj.name}", obj);
            if (obj.DisplaySolo && !forceOn) {
                ChannelMode = lastChannelMode;
                obj.DisplaySolo = false;
            }
            else {
                foreach (TimeflowObject o in Objects) {
                    o.DisplaySolo = false;
                }
                obj.DisplaySolo = true;

                if (ChannelMode != ChannelModes.Solo) {
                    lastChannelMode = ChannelMode;
                }
                ChannelMode = ChannelModes.Solo;
            }
        }

        public void RemoveObjectsFromDisplay(GameObject[] objects)
        {
            if (objects == null) objects = Selection.gameObjects;
            if (objects == null) {
                EditorUtility.DisplayDialog("Hide From Timeflow View", "Please select the objects you want to hide from the view.", "Ok");
            }
            else {
                ObjectMode = ObjectModes.UserControlled;

                foreach (GameObject obj in objects) {
                    RemoveObjectFromDisplayRecursive(obj);
                }
                Timeflow.Refresh(true);

                if (TimeflowPreferences.Current.ShowCantRemoveObjectWarning) {
                    // Check to see if objects weren't removed
                    if (Objects != null && objects != null && objects.Length > 0) {
                        bool showMessage = false;
                        foreach (TimeflowObject t in Objects) {
                            if (t.gameObject == objects[0]) {
                                showMessage = true;
                                break;
                            }
                        }
                        if (showMessage) {
                            string msg = "The selected object(s) cannot be removed from the view because they belong to a parent group. " +
                                "Alternatively, use the hide feature from the main menu Tools/Timeflow/Display/Toggle Hidden which uses " +
                                "the display filter to hide selected objects and channels.";
                            if (EditorUtil.ShowDialog("Cannot Remove Object", msg, "Don't show again", "Dismiss")) {
                                TimeflowPreferences.Current.ShowCantRemoveObjectWarning = false;
                            }
                        }
                    }
                }
            }
        }

        public void ToggleObjectsHiddenInDisplay(GameObject[] objects)
        {
            if (objects == null) objects = Selection.gameObjects;
            if (objects == null || objects.Length == 0) {
                Timeflow.Display.ChannelMode = ChannelModes.None;
                Timeflow.Active.View.Display.DisplayChannelsOnAllObjects(true, true, true);
            }
            else {
                foreach (GameObject obj in objects) {
                    TimeflowObject t;
                    if (obj.TryGetComponent<TimeflowObject>(out t)) {
                        UndoUtil.Undo(t, "Hide Objects In Timeflow View", true);
                        t.DisplayChannels = false;
                    }
                }
                Timeflow.Display.ChannelMode = ChannelModes.Displayed;
                Timeflow.Display.ApplyFilter();
            }
        }

        public void RemoveObjectFromDisplayRecursive(GameObject obj)
        {
            //Debug.Log($"RemoveObjectFromDisplayRecursive:{obj.name}", obj);
            if (obj.TryGetComponent<TimeflowObject>(out var item)) {
                if (RootObjects != null && RootObjects.Contains(item)) {
                    RootObjects.Remove(item);
                    Timeflow.OnRootObjectsChanged();
                }
                if (Objects != null && Objects.Contains(item)) {
                    Objects.Remove(item);
                }
                if (Timeflow != null && Timeflow.AutoSaveDisplay) {
                    if (Index >= 0 && Displays != null && Index < Displays.Count) {
                        if (Displays[Index].Objects.Contains(item)) {
                            Displays[Index].Objects.Remove(item);
                        }
                    }
                }
                else {
                    HasChanged = true;
                }
            }

            foreach (Transform child in obj.transform) {
                RemoveObjectFromDisplayRecursive(child.gameObject);
            }
        }

        public void RemoveNonAnimatedObjectsFromDisplay(GameObject obj)
        {
            if (obj != null) {
                if (!obj.TryGetComponent<TimeflowBehavior>(out var behavior)) {
                    RemoveObjectFromDisplayRecursive(obj);
                }
                foreach (Transform child in obj.transform) {
                    RemoveNonAnimatedObjectsFromDisplay(child.gameObject);
                }
            }
        }

        public void GetTimeRangeOfDisplayedObjects(out float min, out float max)
        {
            min = Timeflow.StartTime;
            max = Timeflow.EndTime;
            bool isSet = false;
            if (Timeflow.View.Display.Objects != null) {
                // Search the display for the min/max time of actuall keyframes
                foreach (TimeflowObject obj in Timeflow.View.Display.Objects) {
                    if (obj.BehaviorsEnabled) {
                        foreach (Keyframe k in obj.Track.Keys) {
                            float kt = k.KeyTimeWorld;
                            float kv = k.KeyEndTimeWorld;
                            if (!isSet) {
                                min = kt;
                                max = kv;
                                isSet = true;
                            }
                            else {
                                if (kt < min) min = kt;
                                if (kt > max) max = kt;
                                if (kv > max) max = kv;
                            }
                        }

                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.Keys != null && ch.Keys.Count > 0) {
                                foreach (Keyframe k in ch.Keys) {
                                    float kt = k.KeyTimeWorld;
                                    if (!isSet) {
                                        min = kt;
                                        max = kt;
                                        isSet = true;
                                    }
                                    else {
                                        if (kt < min) min = kt;
                                        if (kt > max) max = kt;
                                    }

                                }
                            }
                        }
                        if (obj.Events != null && obj.Events.Count > 0) {
                            foreach (TimeflowEvent e in obj.Events) {
                                float t = e.TriggerTimeWorld;
                                if (!isSet) {
                                    min = t;
                                    max = t;
                                    isSet = true;
                                }
                                else {
                                    if (t < min) min = t;
                                    if (t > max) max = t;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region GUI

        public void GUISwitches()
        {
            if (IsLayout) {
                switchLockedRect = new GUIRect(_switchPadLeft, _switchPadTop, TimeflowViewLayout.SmallIconSize, TimeflowViewLayout.SmallIconSize);

                switchVisibleRect = switchLockedRect;
                switchVisibleRect.x += TimeflowViewLayout.SmallIconSize + 1;

                switchEnableRect = switchVisibleRect;
                switchEnableRect.x += TimeflowViewLayout.SmallIconSize + 1;

                switchChannelModeRect = switchEnableRect;
                switchChannelModeRect.x += TimeflowViewLayout.SmallIconSize + 1;

                switchColorPickerRect = switchChannelModeRect;
                switchColorPickerRect.x += TimeflowViewLayout.SmallIconSize + 3;
                switchColorPickerRect.y += 3;
                switchColorPickerRect.width = switchColorPickerRect.height = switchColorPickerRect.width - 4;
            }
            else {
                GUISwitchLocked();
                GUISwitchVisible();
                GUISwitchEnabled();
                GUISwitchChannelMode();
                GUISwitchColorPicker();
                GUISwitchesPresets();
            }
        }

        private void GUISwitchLocked()
        {
            if (LockedOnly || UnlockedOnly) {
                TimeflowView.IndicateSwitchFilterIsOn(switchLockedRect);
            }
            if (!LockedOnly && !UnlockedOnly) {
                GUI.color = AxonColor.Faded;
            }
            if (GUI.Button(switchLockedRect, AxonUI.DisplayUnlockedOnlyLabel,
                UnlockedOnly ? AxonUI.LockUnlockedStyle :
                LockedOnly ? AxonUI.LockLockedStyle : AxonUI.LockOffStyle)) {
                if (IsControl) {
                    lastLockedOn = !lastLockedOn;
                    bool val = lastLockedOn;
                    if (Objects != null) {
                        foreach (TimeflowObject obj in Objects) {
                            UndoUtil.Undo(obj, "Unlock All");
                            obj.IsLocked = val;
                            if (obj.AllChannels != null) {
                                foreach (TimeflowChannel ch in obj.AllChannels) {
                                    if (ch != null) ch.IsLocked = val;
                                }
                            }
                        }
                    }
                    ApplyFilter();
                }
                else
                if (IsShift) {
                    LockedOnly = true;
                    UnlockedOnly = false;
                }
                else {
                    LockedOnly = false;
                    UnlockedOnly = !UnlockedOnly;
                }
            }
        }

        private void GUISwitchVisible()
        {
            if (VisibleOnly) {
                TimeflowView.IndicateSwitchFilterIsOn(switchVisibleRect);
            }
            else {
                GUI.color = AxonColor.Faded;
            }
            if (GUI.Button(switchVisibleRect, AxonUI.DisplayVisibileLabel, VisibleOnly ? AxonUI.VisibilityOnStyle : AxonUI.VisibilityOffStyle)) {
                if (IsControl) {
                    lastOn = !lastOn;
                    if (Objects != null) {
                        foreach (TimeflowObject obj in Objects) {
                            UndoUtil.Undo(obj, "Make All Visible");
                            obj.gameObject.SetActive(lastOn);
                        }
                    }
                    ApplyFilter();
                }
                else {
                    VisibleOnly = !VisibleOnly;
                }
            }
        }

        private void GUISwitchEnabled()
        {
            if (EnabledOnly) {
                TimeflowView.IndicateSwitchFilterIsOn(switchEnableRect);
            }
            else {
                GUI.color = AxonColor.Faded;
            }
            if (GUI.Button(switchEnableRect, AxonUI.DisplayAnimatedOnlyLabel, EnabledOnly ? AxonUI.BehaviorOnStyle : AxonUI.BehaviorDisabledStyle)) {
                if (IsControl) {
                    lastOn = !lastOn;
                    if (Objects != null) {
                        foreach (TimeflowObject obj in Objects) {
                            UndoUtil.Undo(obj, lastOn ? "Enable All" : "Disable All");
                            obj.BehaviorsEnabled = lastOn;
                            if (obj.AllChannels != null) {
                                foreach (TimeflowChannel ch in obj.AllChannels) {
                                    ch.IsEnabled = lastOn;
                                }
                            }
                        }
                    }
                    ApplyFilter();
                }
                else {
                    EnabledOnly = !EnabledOnly;
                }
            }
        }

        private void GUISwitchChannelMode()
        {
            if (ChannelMode != ChannelModes.None) {
                TimeflowView.IndicateSwitchFilterIsOn(switchChannelModeRect);
            }

            // Note that the icons are intentionally reversed
            bool isAlt = Event.current != null && Event.current.alt;
            GUIStyle s = AxonUI.DisplayChannelOnStyle;
            if (ChannelMode == ChannelModes.Displayed) {
                s = AxonUI.DisplayChannelOffStyle;
            }
            else
            if (ChannelMode != ChannelModes.None || isAlt) {
                if (ChannelMode == ChannelModes.Solo || isAlt) {
                    GUI.color = AxonColor.Solo;
                    s = AxonUI.DisplayChannelSoloOnStyle;
                }
                else {
                    s = AxonUI.DisplayChannelOnStyle;
                }
            }
            else {
                GUI.color = AxonColor.Faded;
            }

            if (GUI.Button(switchChannelModeRect, AxonUI.DisplayShowChannelsLabel, s)) {
                if (IsAlt) {
                    if (ChannelMode == ChannelModes.Solo) ChannelMode = ChannelModes.None;
                    else ChannelMode = ChannelModes.Solo;
                }
                else
                if (IsShift) {
                    if (ChannelMode == ChannelModes.Objects) ChannelMode = ChannelModes.None;
                    else ChannelMode = ChannelModes.Objects;
                }
                else
                if (IsControl) {
                    lastOn = !lastOn;
                    DisplayChannelsOnAllObjects(lastOn, true, true);
                }
                else {
                    ChannelMode = ChannelMode != ChannelModes.None ? ChannelModes.None : ChannelModes.Displayed;
                }
            }
            GUI.color = AxonColor.Default;
        }

        private void GUISwitchColorPicker()
        {
            GUI.color = AxonColor.Faded;
            if (GUI.Button(switchColorPickerRect, AxonUI.TrackColorLabel, AxonUI.SolidStyle)) {
                TrackColorMenu.InitAll();
            }
            GUI.color = AxonColor.Default;
        }

        private void GUISwitchesPresets()
        {
            if (!Timeflow.Layout.ShowAdvancedPresets) return;

            GUI.color = Color.white;
            Rect switchesPresetRect = new Rect(switchColorPickerRect.x + switchColorPickerRect.width + 2, switchColorPickerRect.y - 2, 16, 16);
            if (GUI.Button(switchesPresetRect, GUIContent.none, AxonUI.AdvancedPresetStyle)) {
                AdvancedPresetsPopup.Invoke(null);
            }
        }

        public void GUIMenu()
        {
            bool isLayout = IsLayout;
            if (isLayout) {
                GUIMenuLayout();
            }
            else {
                if (IsEditingName) {
                    GUIMenuEditName();
                    return;
                }
                if (GUI.Button(menuSearchButtonRect, AxonUI.SearchLabel, IsSearching ? AxonUI.SearchStyleOn : AxonUI.SearchStyleOff)) {
                    Search();
                }
            }
            if (IsSearching) {
                GUIMenuSearch();
                return;
            }

            if (!isLayout && !IsEditingName) GUIMenuDisplay();
        }

        public void GUIMenuLayout()
        {
            menuSearchButtonRect.y = _menuIconPadTop;
            menuSearchButtonRect.x = _menuSearchButtonLeftPad;
            if (Layout.ShowSwitches) {
                menuSearchButtonRect.x += Layout.Switches.Width;
            }
            menuSearchButtonRect.height = menuSearchButtonRect.width = TimeflowViewLayout.SmallIconSize;

            menuDisplayNameRect.y = 0;
            menuDisplayNameRect.x = menuSearchButtonRect.x + menuSearchButtonRect.width + _menuDisplayNameLeftPad;
            menuDisplayNameRect.width = Layout.SeparatorH1.Left - menuDisplayNameRect.x - 90;// - _menuDisplayNameRightPad;
            menuDisplayNameRect.height = _menuDisplayNameHeight;

            menuDisplayPrevRect.y = _menuIconPadTop;
            menuDisplayPrevRect.width = menuDisplayPrevRect.height = TimeflowViewLayout.SmallIconSize;
            menuDisplayPrevRect.x = Layout.SeparatorH1.Left - _menuDisplayPrevRightOffset;

            menuDisplayNextRect = menuDisplayPrevRect;
            menuDisplayNextRect.x = menuDisplayPrevRect.x + menuDisplayPrevRect.width + _menuDisplayNextPadLeft;

            menuDisplayNextRect = menuDisplayPrevRect;
            menuDisplayNextRect.x = menuDisplayPrevRect.x + menuDisplayPrevRect.width + _menuDisplayNextPadLeft;

            menuDisplayLockRect = menuDisplayPrevRect;
            menuDisplayLockRect.x = Layout.SeparatorH1.Left - menuDisplayLockRect.width - _menuDisplayLockOffset - TimeflowViewLayout.SmallIconSize;

            menuDisplaySaveRect = menuDisplayLockRect;
            menuDisplaySaveRect.x = menuDisplayLockRect.x - menuDisplaySaveRect.width - _menuDisplayLockOffset;
        }

        private void GUIMenuDisplayPrefab()
        {
            if (IsDisplayingPrefab) {
                GUI.color = Color.gray;
                GUI.Button(menuDisplayNameRect, new GUIContent("Prefab edit mode"), GUI.skin.label);
                GUI.color = AxonColor.Default;
            }
        }

        private void GUIMenuEditName()
        {
            GUI.color = AxonColor.Default;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.textField);
            labelStyle.alignment = TextAnchor.MiddleLeft;

            GUI.SetNextControlName("EditDisplayName");
            Timeflow.tempEditContainingRect = Layout.HierarchyTools;
            Timeflow.tempEditRect = menuDisplayNameRect;
            Timeflow.tempEditName = GUI.TextField(menuDisplayNameRect, Timeflow.tempEditName, GUI.skin.textField);
            if (!hasEditNameBeenFocused) {
                hasEditNameBeenFocused = true;
                AxonGUI.FocusControl("EditDisplayName");
            }
        }

        private void GUIMenuDisplay()
        {
            if (Layout == null || Layout.SeparatorH1 == null) return;
            EditorGUI.BeginDisabledGroup(IsLocked);
            bool isWideEnough = Layout.SeparatorH1.Left > TimeflowViewLayout.SeparatorHNameThresh;

            if (isWideEnough) GUIMenuDisplayName();

            if (isWideEnough && Displays != null && Displays.Count > 1) {
                if (GUI.Button(menuDisplayPrevRect, AxonUI.DisplayPrevLabel, AxonUI.PrevKeyStyle)) {
                    Previous();
                }
                if (GUI.Button(menuDisplayNextRect, AxonUI.DisplayNextLabel, AxonUI.NextKeyStyle)) {
                    Next();
                }
            }
            EditorGUI.EndDisabledGroup();

            if (GUI.Button(menuDisplayLockRect, AxonUI.LockBigLabel, IsLocked ? AxonUI.LockBigOnStyle : AxonUI.LockBigOffStyle)) {
                IsLocked = !IsLocked;
            }
            if (GUI.Button(menuDisplaySaveRect, AxonUI.SaveLabel, AxonUI.SaveStyle)) {
                Save();
            }
        }

        private void GUIMenuDisplayName()
        {
            if (IsEditingName) return;
            bool isEmpty = RootObjects == null || RootObjects.Count == 0 || RootObjects[0] == null;
            if ((string.IsNullOrEmpty(Name) || Name == "None") && !isEmpty) {
                Name = RootObjects[0].name;
            }
            GUI.color = isEmpty ? AxonColor.Inactive : AxonColor.Active;

            bool isSaved = false;
            string displayName = Name;
            if (!isEmpty) {
                isSaved = IsSaved(RootObjects[0]);
                if (!isSaved || HasChanged) displayName = displayName + " *";
                if (!isSaved) {
                    GUI.color = Color.gray;
                }
            }
            if (GUI.Button(menuDisplayNameRect, new GUIContent(displayName), GUI.skin.label)) {
                GUIMenuDropdown();
            }
            GUI.color = AxonColor.Default;
        }

        private void GUIMenuDropdown()
        {
            GenericMenu menu = new GenericMenu();
            TimeflowDisplayMenuItem item = new TimeflowDisplayMenuItem(this, 0);

            if (Name != "None" && Name != "Select Display") {
                item = new TimeflowDisplayMenuItem(this, Index);
                if (IsDisplayingPrefab) {
                    menu.AddItem(new GUIContent("Cannot save prefab display"), false, null);
                }
                else {
                    menu.AddItem(new GUIContent("Save Display: " + Name), false, TimeflowDisplayMenuItem.Save, item);
                    menu.AddItem(new GUIContent("Remove Display: " + Name), false, TimeflowDisplayMenuItem.Remove, item);
                    menu.AddItem(new GUIContent("Rename"), false, TimeflowDisplayMenuItem.Rename);
                }
                menu.AddSeparator("");
            }
            bool hasDisplays = false;
            if (Displays != null && Displays.Count > 0) {
                hasDisplays = true;
                for (int i = 0; i < Displays.Count; i++) {
                    item = new TimeflowDisplayMenuItem(this, i);
                    menu.AddItem(new GUIContent(Displays[i].Name), Index == i && ObjectMode == ObjectModes.SavedDisplay, TimeflowDisplayMenuItem.Load, item);
                }
                menu.AddSeparator("");
            }
            item = new TimeflowDisplayMenuItem(this, 0);
            if (hasDisplays) {
                menu.AddItem(new GUIContent("Clear All Display Lists"), false, TimeflowDisplayMenuItem.Clear, item);
            }
            else {
                menu.AddItem(new GUIContent("Clear All Display Lists"), false, null);
            }
            menu.AddItem(new GUIContent("Edit Lists"), false, TimeflowDisplayMenuItem.Edit, item);

            menu.AddSeparator("");
            if (Timeflow.WorkAreaEnabled) {
                menu.AddItem(new GUIContent("Show All Animated in Work Area"), false, TimeflowDisplayMenuItem.ShowAllAnimatedInWorkArea);
            }
            else {
                menu.AddItem(new GUIContent("Show All Animated in Work Area"), false, null);
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Nothing"), ObjectMode == ObjectModes.Nothing, TimeflowDisplayMenuItem.DisplayNothing, item);
            menu.AddItem(new GUIContent("Everything"), ObjectMode == ObjectModes.Everything, TimeflowDisplayMenuItem.DisplayEverything, item);
            menu.AddItem(new GUIContent("Selected Object"), ObjectMode == ObjectModes.SelectedObject, TimeflowDisplayMenuItem.DisplaySelectedObject, item);
            menu.AddItem(new GUIContent("Selected Group"), ObjectMode == ObjectModes.SelectedGroup, TimeflowDisplayMenuItem.DisplaySelectedGroup, item);

            if (menu != null) {
                Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
                menu.DropDown(new GUIRect(menuDisplayNameRect.x, menuDisplayNameRect.y, size.x, size.y));
            }
        }

        private List<GameObject> GetSelectedItemsListToAdd()
        {
            List<GameObject> selected = null;
            if (Selection.gameObjects != null) {
                selected = new List<GameObject>();
            }
            if (selected != null) {
                foreach (GameObject obj in Selection.gameObjects) {
                    // Check that the object is not already in the hierarchy view
                    if (!IsObjectDisplayed(obj)) {
                        selected.Add(obj);
                    }
                }
            }
            return selected;
        }

        public bool GUIAddBarClicked()
        {
            return _addBarArea.Contains(MousePosition);
        }

        public void GUIAddBar()
        {
            List<GameObject> selected = GetSelectedItemsListToAdd();

            bool hasSelection = selected != null && selected.Count > 0;
            bool isLayout = IsLayout;
            if (isLayout) {
                float left = _menuAddBarPadWidth + (Layout.ShowSwitches ? Layout.Switches.Width : 0);
                float height = IsLocked ? _addBarHeightLocked : hasSelection ? _addBarHeightSelection : 0;

                if (AnyObjectsHidden) height += _menuAddBarPadTop;
                _addBarArea = new GUIRect(left, Layout.Hierarchy.Rect.yMax - height - _menuAddBarPadTop, Layout.SeparatorH1.Left - left - _menuAddBarPadWidth, height);
            }
            GUILayout.BeginArea(_addBarArea);
            GUI.enabled = true;

            if (AnyObjectsHidden) {
                GUI.color = Color.gray;
                GUILayout.Label(new GUIContent("Some Objects Hidden by Filter"), EditorStyles.miniBoldLabel);
            }
            GUI.color = AxonColor.Default;

            if (IsLocked) {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(new GUIContent("Display is Locked"), GUI.skin.button);
                EditorGUI.EndDisabledGroup();
            }
            else
            if (hasSelection) {
                GUIRect rect = new GUIRect(Layout.Hierarchy.Left, Layout.SeparatorH3.Left - 25f, Layout.Hierarchy.Width / 2f, 25f);
                if (GUI.Button(rect, new GUIContent("Add Selected"), GUI.skin.button)) {
                    //if (GUILayout.Button(new GUIContent("Add Selected"), GUI.skin.button)) {
                    if (Timeflow.AutoSaveDisplay && (RootObjects == null || RootObjects.Count < 1)) {
                        SaveSelected();
                    }
                    else {
                        //Debug.Log($"Add Selected Objects:{selected.Count}");
                        foreach (GameObject obj in selected) {
                            AddObjectToDisplay(obj);
                        }
                    }
                    Event.current.Use();
                }
                //rect = new GUIRect(Layout.Hierarchy.Left, Layout.SeparatorH3.Left - 25f, Layout.Hierarchy.Width / 2f, 25f);
                rect.Left = rect.Width;
                if (GUI.Button(rect, new GUIContent("Show Selected Only"), GUI.skin.button)) {
                    //if (GUILayout.Button(new GUIContent("Show Selected Only"), GUI.skin.button)) {
                    DisplaySelectedHierarchy();
                    Event.current.Use();
                }
            }
            GUILayout.EndArea();
        }

        public void StartEditingName()
        {
            if (!IsEditingName) {
                IsEditingName = true;
                IndexToRename = Index;
                Timeflow.tempEditName = Displays[Index].Name;
                hasEditNameBeenFocused = false;
            }
        }

        public void StopEditingName(bool commit = true)
        {
            IsEditingName = false;
            IndexToRename = -1;
            AxonGUI.FocusControl(null);

            if (Displays == null || Displays.Count == 0) return;

            if (commit) {
                UndoUtil.Undo(Timeflow.gameObject, "Edit Display Name");
                if (Index >= 0 && Index < Displays.Count) {
                    Displays[Index].Name = Name = Timeflow.tempEditName;
                }
            }
        }

        public void ShowAllAnimatedInWorkArea()
        {
            List<TimeflowObject> objects = ObjectUtil.GetComponentsRecursive<TimeflowObject>(Timeflow.gameObject);
            if (objects == null || objects.Count == 0) return;

            if (!Timeflow.WorkAreaEnabled) {
                Debug.LogWarning("Make sure the Work Area is enabled and set to the desired time range before using this feature.");
                return;
            }

            List<GameObject> toDisplay = new List<GameObject>();
            foreach (TimeflowObject obj in objects) {
                if (obj.Track == null || obj.Track.Keys.Count == 0) continue;
                bool addObject = false;
                List<Keyframe> keys = obj.Track.GetKeysInTimeRange(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd);
                if (keys != null && keys.Count > 0) {
                    addObject = true;
                }
                if (!addObject) {
                    // Search all channels for active keys
                    if (obj.AllChannels != null && obj.AllChannels.Count > 0) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            keys = ch.GetKeysInTimeRange(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd);
                            if (keys != null && keys.Count > 0) {
                                addObject = true;
                                break;
                            }
                        }
                    }
                }

                if (addObject && !toDisplay.Contains(obj.gameObject)) {
                    toDisplay.Add(obj.gameObject);
                }
            }

            SelectionUtil.Select(toDisplay.ToArray());
            DisplaySelectedObjects();
        }

        #endregion

    }

}//AxonGenesis
#endif
