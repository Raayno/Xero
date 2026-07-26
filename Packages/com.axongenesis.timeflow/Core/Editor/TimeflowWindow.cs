// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    public class TimeflowWindow : EditorWindow
    {
        #region STATIC VARS

        public static TimeflowWindow Instance { get; private set; }

        private static bool DebugEnabled = false;

        private static bool IsAlt = false;
        private static bool IsControl = false;
        private static bool IsShift = false;

        #endregion

        #region PRIVATE

        private PropertyInfo cachedTitleContent;
        private DateTime lastHierarchyChangeTime;
        private Timeflow timeflow;
        private bool needsRepaint;
        private bool canDraw;

        private class PropertyChange
        {
            public string Name;
            public string Alias;
            public TimeflowObject Object;
            public Component Component;
            public int Attribute;
            public Material Material;

            public PropertyChange(string name, string alias, TimeflowObject obj, Component component, int attr, Material mat = null)
            {
                //Debug.Log($"new PropertyChange:{name} component:{component.GetType()} alias:{alias} attr:{attr}");
                Name = name;
                Alias = alias;
                Object = obj;
                Component = component;
                Attribute = attr;
                Material = mat;
            }
        }

        private static Dictionary<string, PropertyChange> _PropertyMods = new Dictionary<string, PropertyChange>();

        #endregion

        #region WINDOW

        public static bool IsOpen => Instance != null;

#if TIMEFLOW_PRO
        public const string kOpenTimeflowWindow = "🎬 Open Timeflow Window";
        public const string kDocumentation = "📕 Documentation";
        public const string kGettingStarted = "👉 Getting Started...";
        public const string kToggleWindowMinimized = TimeflowMenu.kEditor + TimeflowMenu.Sep + "➖ Toggle Timeflow Window Minimized";
        public const string kWindowSequencing = "Window/Sequencing/" + TimeflowMenu.kTimeflow;
#else
        public const string kOpenTimeflowWindow = "Open Timeflow Window";
        public const string kDocumentation = "Documentation";
        public const string kGettingStarted = "Getting Started...";
        public const string kToggleWindowMinimized = "Editor/Toggle Timeflow Window Minimized";
        public const string kWindowSequencing = "Window/Sequencing/Timeflow";
#endif

        [Shortcut(TimeflowShortcutInfo.Path_OpenTimeflowWindow, KeyCode.T, ShortcutModifiers.Action | ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kOpenTimeflowWindow + TimeflowMenu.Tab + TimeflowShortcutBindings.OpenTimeflowWindow, false, -100)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kOpenTimeflowWindow + TimeflowMenu.Tab + TimeflowShortcutBindings.OpenTimeflowWindow, false, -100)]
        public static void OpenWindow()
        {
            if (Instance == null) {
                Instance = EditorWindow.GetWindow(typeof(TimeflowWindow), false, "Timeflow") as TimeflowWindow;
                if (Instance == null) {
                    Debug.LogError("Failed loading Timeflow window");
                }
                else {
                    Instance.minSize = new Vector2(68.0f, 44.0f);
                }
            }
            else {
                Instance.Show();
            }
            Instance.autoRepaintOnSceneChange = true;

            Undo.undoRedoPerformed += OnUndo;
        }

        [UnityEditor.MenuItem(kWindowSequencing + TimeflowMenu.Tab + TimeflowShortcutBindings.OpenTimeflowWindow, false, 0)]
        public static void OpenTimeflow()
        {
            OpenWindow();
        }

        [Shortcut(TimeflowShortcutInfo.Path_OpenDocumentation)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kDocumentation, false, 11000)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kDocumentation, false, 11000)]
        public static void OpenDocumentation()
        {
            string url = "https://axongenesis.gitbook.io/timeflow/";
            Debug.Log("Opening documentation at:" + url);//--KEEP  
            Application.OpenURL(url);
        }

        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kGettingStarted, false, 11000)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kGettingStarted, false, 11000)]
        public static void OpenGettingStarted()
        {
            string path = AssetDatabase.GUIDToAssetPath("ea74309b8a397064785761c6c171cad7");
            if (string.IsNullOrEmpty(path)) return;
            path = path.Substring(7);
            path = Path.Combine(Application.dataPath, path);
            Debug.Log("Opening readme:" + path);//--KEEP
            Application.OpenURL(path);
        }

        [Shortcut(TimeflowShortcutInfo.Path_ToggleTimeflowWindowMinimized, KeyCode.W, ShortcutModifiers.Action | ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kToggleWindowMinimized + TimeflowMenu.Tab + TimeflowShortcutBindings.ToggleTimeflowWindowMinimized, false, 10001)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kToggleWindowMinimized + TimeflowMenu.Tab2 + TimeflowShortcutBindings.ToggleTimeflowWindowMinimized, false, 10001)]
        public static void ToggleWindowMinimized()
        {
            if (Instance == null) return;
            Instance.ToggleMinimized();
        }

        #endregion

        #region CALLBACK DELEGATES

        public static void OnAddNull(GameObject obj)
        {
            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        public static void OnAddChild(GameObject obj)
        {
            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);

                TimeflowObject pobj = ObjectUtil.GetComponentInParent<TimeflowObject>(obj);
                if (pobj != null) {
                    pobj.ShowChildren = true;
                    pobj.GetChildren();
                }
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        public static void OnGroupObjects(GameObject obj, GameObject newGroup)
        {
            if (Timeflow.Active != null) {
                if (obj.TryGetComponent<TimeflowObject>(out TimeflowObject tobj)) {
                    if (Timeflow.Active.View.Display.IsObjectDisplayed(obj)) {
                        Timeflow.Active.View.Display.AddObjectToDisplay(newGroup);
                    }
                    Timeflow.Active.View.OnSelectionChange();
                }
            }
        }

        public static void OnRefresh()
        {
            if (Instance != null) Instance.Repaint();
        }

        public static void OnUndo()
        {
            if (Timeflow.Active != null) {
                Timeflow.Active.Input.OnUndo();
            }
        }

        public static bool HasFocus()
        {
            return EditorWindow.focusedWindow == Instance;
        }

        public static bool IsMouseOverWindow()
        {
            return Instance == EditorWindow.mouseOverWindow;
        }

        #endregion

        private bool wantsAxonUIReload = true;

        #region ACCESSORS

        private bool CanDisplay => timeflow != null && timeflow.Input != null;

        #endregion

        #region EVENTS

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= OnUndo;
            Undo.postprocessModifications -= AutoKeyframeDetection;
            Timeflow.GlobalRefresh();
            Instance = null;
        }

        public void OnEnable()
        {
            if (Instance == null) Instance = this;

            canDraw = false; // prevent UI drawing until ready

            Timeflow.Editor = this;

            TimeflowMenu.OnAddNull -= OnAddNull;
            TimeflowMenu.OnAddNull += OnAddNull;
            TimeflowMenu.OnAddChild -= OnAddChild;
            TimeflowMenu.OnAddChild += OnAddChild;
            TimeflowMenu.OnGroupObjects -= OnGroupObjects;
            TimeflowMenu.OnGroupObjects += OnGroupObjects;

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;

            Undo.postprocessModifications -= AutoKeyframeDetection;
            Undo.postprocessModifications += AutoKeyframeDetection;

            EditorApplication.update -= ContinuousUpdate;
            EditorApplication.update += ContinuousUpdate;

            //Selection.selectionChanged -= OnSelectionChange;
            //Selection.selectionChanged += OnSelectionChange;

            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            autoRepaintOnSceneChange = true;
            wantsAxonUIReload = true;

            CacheTitleContent();

            Timeflow.GlobalRefresh();
        }

        public void OnDisable()
        {
            Timeflow.Editor = null;
            TimeflowMenu.OnAddNull -= OnAddNull;
            TimeflowMenu.OnAddChild -= OnAddChild;
            TimeflowMenu.OnGroupObjects -= OnGroupObjects;
            Timeflow.IsAutoKeyframingEnabled = false;
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }

        public void OnLostFocus()
        {
            if (!CanDisplay) return;
            timeflow.Input.OnLostFocus();
        }

        public void OnFocus()
        {
            if (!CanDisplay) return;
            timeflow.Input.GainedFocus();
            OnRefresh();
        }

        public void OnSelectionChange()
        {
            if (!CanDisplay) return;
            timeflow.View.OnSelectionChange();
        }

        public void OnHierarchyChange()
        {
            if (!CanDisplay) return;
            if (!Application.isPlaying && !timeflow.IsPlaying) {
                /// This is a hack to reduce repeated calls to OnHierarchyChanged which can cause excessive
                /// update calls when doing nothing other than selecting objects. This may be a bug in
                /// Unity, but responding to this callback is necessary to properly handle changes to
                /// object hierarchies that may affect Timeflow setups.
                TimeSpan elapsed = DateTime.Now.Subtract(lastHierarchyChangeTime);

                if (elapsed.TotalSeconds > 1f) {
                    if (Timeflow.Instances != null) {
                        foreach (Timeflow t in Timeflow.Instances) {
                            if (t == null) continue;
                            t.OnHierarchyChange();
                        }
                    }
                }
                lastHierarchyChangeTime = DateTime.Now;
            }
        }

        public void ContinuousUpdate()
        {
            if (!CanDisplay) return;
            if (needsRepaint) {
                needsRepaint = false;
                Repaint();
            }
        }

        private void CacheTitleContent()
        {
            if (cachedTitleContent == null) {
                cachedTitleContent = base.GetType().GetProperty("cachedTitleContent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            }
            if (cachedTitleContent != null) {
                GUIContent content = cachedTitleContent.GetValue(this, null) as GUIContent;
                if (content != null) {
                    content.image = AxonUI.Icons.AxonGenesisLogo;
                }
            }
        }

        #endregion

        #region GUI

        private static bool _IsMinimized = false;
        private static readonly int _MinimizedHeight = 30;
        private static Rect _RestorePosition;

        public static float GUIStartTime { get; private set; } = 0f;

        public static float GUIElapsed => Time.realtimeSinceStartup - GUIStartTime;

        public static bool IsDrawLimited = false;

        //private static float lastDrawLimitTime = 0f;

        public static bool IsMinimized {
            get {
                return _IsMinimized;
            }
            set {
                if (_IsMinimized != value) {
                    _IsMinimized = value;
                    Instance?.OnMinimizedChanged();
                }
            }
        }

        private void OnMinimizedChanged()
        {
            if (IsMinimized) {
                Minimize();
            }
            else {
                Unminimize();
            }
        }

        public void ToggleMinimized()
        {
            IsMinimized = !IsMinimized;
        }

        public void Minimize()
        {
            _IsMinimized = true;

            if (docked) {
                //maxSize = new Vector2(position.width, _MinimizedHeight);
            }
            else {
                Rect p = position;
                if (TimeflowPreferences.Current.MinimizeFloatingViewToBottom) p.y = Screen.currentResolution.height - _MinimizedHeight;
                p.height = _MinimizedHeight;
                position = p;
            }
        }

        public void Unminimize()
        {
            _IsMinimized = false;
            if (docked) {
                //maxSize = new Vector2(_RestorePosition.width, _RestorePosition.height);
            }
            else {
                position = _RestorePosition;
            }
        }

        private bool IsGUIReady()
        {
            if (!canDraw) {
                /// This ensures that the first OnGUI call is for the layout and is not preceeded by a
                /// paint call, which can occur when scripts recompile and results in an error since
                /// controls are being drawn with invalid rects.
                if (Event.current.type == EventType.Layout) {
                    AxonUI.Load(true);
                    canDraw = true;
                    return true;
                }
                return false;
            }
            return true;
        }

        private int lastGUIFrame = -1;
        public void OnGUI()
        {
            if (!IsGUIReady()) return;
            Timeflow.IsAutoKeyframingInvalidThisFrame = false;

            GUIStartTime = Time.realtimeSinceStartup;
            if (wantsAxonUIReload) AxonUI.Load(true);

            GUISetup();

            if (!IsMinimized) _RestorePosition = position;
            else {
                _RestorePosition.x = position.x;
                _RestorePosition.width = position.width;
            }

            if (timeflow == null) timeflow = Timeflow.Active;
            //Debug.Log($"TimeflowWindow.OnGUI() timeflow:{(timeflow == null ? "NULL" : timeflow.name)}");
            if (timeflow == null || !timeflow.gameObject.activeInHierarchy) {
                ShowEmptyView();
            }
            else {
                //if (Event.current.shift) Debug.Log(lastGUIFrame);
                if (Event.current.type == EventType.Layout) {
                    timeflow.View.GUIStartLayout(position);
                }
                else {
                    timeflow.View.GUIStart();
                }
                OnInput();
            }

            if (Event.current.type == EventType.Repaint) {
                // Delay the draw limit check to avoid excessive flickering
                // Doesn't work as expected and causes UI conflicts, so disabling for now
                //bool isDrawLimited = GUIElapsed > 0.03f;
                //if (isDrawLimited || Time.realtimeSinceStartup - lastDrawLimitTime > 1f) {
                //    lastDrawLimitTime = Time.realtimeSinceStartup;
                //    IsDrawLimited = isDrawLimited;
                //}
                //DebugBoard.Register("GUI", $"Elapsed:{elapsed:F4} seconds", Timeflow.Active);
                //DebugBoard.Register("IsDrawLimited", $"{IsDrawLimited}", Timeflow.Active);
            }
            lastGUIFrame++;
        }


        public void GUISetup()
        {
            // The instance can be lost when recompiling scripts while the view is still open
            if (Instance == null) Instance = this;

            // Make sure UI resources are loaded
            AxonGUI.Setup();

            // Handle properties detected by auto keyframing
            //HandlePropertyModifications();

            if (timeflow == null || timeflow != Timeflow.Active) {
                if (timeflow != null) {
                    /// detach previous Timeflow instance from window
                    timeflow.View.OnRefreshed -= OnRefresh;
                }

                timeflow = Timeflow.Active;
                if (timeflow != null) {
                    timeflow.Refresh(true);
                    timeflow.View.FitTime(false, true);

                    if (timeflow.View == null) timeflow.View = new TimeflowView(timeflow);
                    timeflow.View.OnRefreshed += OnRefresh;
                }
            }
            if (timeflow != null) {
                timeflow.IsActive = true;
                timeflow.enabled = true;
                if (timeflow.View.NeedsRefresh) {
                    timeflow.View.NeedsRefresh = false;
                    timeflow.Refresh(true);
                }
            }
        }

        #endregion

        #region INPUT

        bool isSearching = false;

        public void OnInput()
        {
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout && Event.current.type != EventType.MouseMove) {
                //Debug.Log($"<color=yellow>OnInput: {Event.current.type}</color>");
            }


            if (!CanDisplay) return;
            if (GUI.GetNameOfFocusedControl() == "SearchDisplay") {
                if (!isSearching) {
                    isSearching = true;
                    //Debug.Log($"<color=yellow>Is Searching</color>");
                }
                //return;
            }
            else {
                if (isSearching) {
                    isSearching = false;
                    //Debug.Log($"<color=orange>Stopped Searching</color> FocusedControl:{GUI.GetNameOfFocusedControl()}");
                }
            }

            // Cache these values since Event.current is null in some contexts
            IsAlt = EditorInput.IsAlt;
            IsControl = EditorInput.IsControl;
            IsShift = EditorInput.IsShift;

            needsRepaint = timeflow.Input.OnInput();
        }

        private static DateTime _lastTimeflowPoll = DateTime.MinValue;

        public void ShowEmptyView()
        {
            float pad = 4f;
            float pad2 = pad * 2f;
            Rect rect = new GUIRect(pad, pad, position.width - pad2, position.height - pad2);

            GUI.Box(rect, new GUIContent(), GUI.skin.box);

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.background = null;
            style.fontSize = 16;

            rect.width = 427;
            rect.height = 27;
            rect.x = (position.width / 2f) - (rect.width / 2f);
            rect.y = (position.height / 2f) - (rect.height / 2f);
            if (GUI.Button(rect, new GUIContent("Add Timeflow to this scene", "Adds a new Timeflow instance to the current scene."), style)) {
                Timeflow.CreateNewTimeflow();
            }
            DateTime now = DateTime.Now;

            if ((now - _lastTimeflowPoll).TotalSeconds > 1f) {
                _lastTimeflowPoll = now;
                Timeflow.RegisterAllInstances();
            }
        }

        public void OpenSettings()
        {
            if (timeflow != null) {
                SelectionUtil.Select(timeflow.gameObject);
            }
        }

        private UndoPropertyModification[] AutoKeyframeDetection(UndoPropertyModification[] modifications)
        {
            if (Timeflow.Active == null || !Timeflow.IsAutoKeyframingEnabled) return modifications;
            if (Timeflow.Active.Input.IsDragging || Timeflow.Active.IsPlaying) return modifications;

            //if (DebugEnabled) Debug.Log($"<color=yellow>AutoKeyframeDetection Count:{modifications.Length} ++++++++++++++++++</color>");

            foreach (UndoPropertyModification mod in modifications) {
                Type targetType = mod.currentValue.target.GetType();
                Component comp = null;

                bool isComponent = false;
                if (mod.currentValue.target is Component c) {
                    comp = c;
                    isComponent = true;
                    mod.currentValue.target = c;
                }
                else {
                    comp = Property.GetComponentForObjectType(mod.currentValue.target);
                }

                if (comp == null) {
                    //Debug.LogWarning($"Component type not supported:{targetType} path:{mod.currentValue.propertyPath}");
                    continue;
                }
                else {
                    string propPath = mod.currentValue.propertyPath;

                    // Determine whether this property can be animated
                    if (targetType == typeof(Transform)) {
                        // Scene view expresses rotation via EulerAnglesHint, which is not a valid Transform property
                        propPath = propPath.Replace("EulerAnglesHint", "EulerAngles");
                    }

                    string displayName = Property.GetAnimatablePropertyName(targetType, propPath, true, false);
                    if (!string.IsNullOrEmpty(displayName)) {
                        displayName = StringUtil.ToCamelCase(displayName.Replace("m_", "").Replace(".", ""));
                        displayName = Property.RemoveAttributeFromName(displayName);
                    }
                    string propertyName = Property.GetAnimatablePropertyName(targetType, propPath, false, false);

                    //if (DebugEnabled) Debug.Log($"<color=yellow>Modification:</color>{mod.currentValue.target.name} type:{targetType} property:{propPath} value:{mod.currentValue.value} displayName:{displayName} propertyName:{propertyName}");

                    if (!isComponent) {
                        propertyName = mod.currentValue.target.name.Replace("(Clone)", "") + "/" + propertyName;
                        displayName = propertyName;
                        propPath = propertyName;
                    }

                    if (string.IsNullOrEmpty(propertyName)) {
                        AxonTools.RefreshTransformOverride();
                        //if (DebugEnabled) Debug.Log($"SKIP:{mod.currentValue.target.name} type:{targetType} property:{propPath} value:{mod.currentValue.value}");
                        continue;
                    }

                    //if (DebugEnabled) Debug.Log($"<color=cyan>DETECTED: {mod.currentValue.target.name} type:{targetType} property:{propPath} value:{mod.currentValue.value}</color>");

                    string inPath = propPath;
                    string finalPropertyName = propPath;
                    int attribute = -1;
                    Type fieldType;
                    if (Property.FindFieldOrProperty(targetType, inPath, out finalPropertyName, out fieldType, ref attribute, DebugEnabled, DebugEnabled)) {

                        if (finalPropertyName.Contains(".")) {
                            string[] parts = finalPropertyName.Split('.');
                            if (parts.Length > 1) {
                                // The first part is the property name
                                finalPropertyName = parts[0];

                                // The attribute is the last part
                                string attr = parts[parts.Length - 1].ToLower();
                                attribute = Property.GetAttribute(attr);
                                ////if (DebugEnabled) Debug.Log($"<color=lime>{finalPropertyName}</color> attribute:{attr} :{attribute}");
                            }
                        }
                        //if (DebugEnabled) Debug.Log($"<color=lime>MAPPING:</color> {finalPropertyName}:{attribute} Mode:{TimeflowPreferences.Current.NewChannelAttributeMode} fieldType:{fieldType}");

                        if (!isComponent) {
                            // Re-append the component name to the property name
                            finalPropertyName = mod.currentValue.target.name.Replace("(Clone)", "") + "/" + finalPropertyName;
                        }
                        RecordPropertyChange(comp, fieldType, finalPropertyName, displayName, attribute);
                    }
                }
            }
            HandlePropertyModifications();
            return modifications;
        }

        public static void RecordPropertyChange(Component comp, Type fieldType, string propName, string propAlias, int attribute, Material mat = null)
        {
            if (string.IsNullOrEmpty(propName)) return;
            attribute = GetAutoKeyframeAttributeModifiers(fieldType, attribute);

            // Determine whether this property can be animated
            string propertyName = Property.GetAnimatablePropertyName(comp.GetType(), propName, false, mat != null);
            if (string.IsNullOrEmpty(propertyName)) return;

            //if (DebugEnabled) Debug.Log($"<color=lime>RecordPropertyChange:</color>{propertyName}({propName}) attribute:{attribute}");

            // Determine whether any combined or separate channels already exist for this property
            bool hasCombined = false;
            bool hasSeparate = false;
            TimeflowObject tobj = Timeflow.SetupTimeflowObject(comp.gameObject);
            if (tobj.AllChannels != null) {
                foreach (TimeflowChannel ch in tobj.AllChannels) {
                    string modName = StripNameToCompare(propertyName);
                    string aliasName = StripNameToCompare(propAlias);
                    string pName = ch.ToProperty == null ? propertyName : StripNameToCompare(ch.ToProperty.Name);

                    if (ch.ToProperty != null && ch.ToProperty.Component == comp && (pName == propertyName || pName == aliasName)) {
                        if (ch.ToProperty.Attribute < 0) {
                            hasCombined = true;
                        }
                        else {
                            hasSeparate = true;
                        }
                        if (hasCombined && hasSeparate) break;
                    }
                }
            }

            if (_PropertyMods == null) _PropertyMods = new Dictionary<string, PropertyChange>();
            string uniquePropName = $"{comp.GetInstanceID()}_{propertyName}.{attribute}";
            ////if (DebugEnabled) Debug.Log($"RecordPropertyChange:{propertyName} uniquePropName:{uniquePropName} attribute:{attribute} hasCombined:{hasCombined} hasSeparate:{hasSeparate}");

            if (hasCombined && attribute > -1 && _PropertyMods.ContainsKey(uniquePropName)) {
                // Set multiple attributes for the same property
                _PropertyMods[uniquePropName].Attribute = -1;
                //if (DebugEnabled) Debug.Log($"RecordPropertyChange:{uniquePropName} -1");
            }
            else {
                if (!_PropertyMods.ContainsKey(uniquePropName)) {
                    //if (DebugEnabled) Debug.Log($"<color=green>RECORDED:</color> {propertyName} ({uniquePropName})");
                    _PropertyMods.Add(uniquePropName, new PropertyChange(propertyName, propAlias, tobj, comp, attribute, mat));
                }
                else {
                    //if (DebugEnabled) Debug.Log($"<color=red>Not Recorded: Duplicate Name:</color>{propertyName} :{uniquePropName} _PropertyMods:{_PropertyMods.Count}");
                }
            }
        }

        public static void HandlePropertyModifications()
        {
            if (_PropertyMods == null || _PropertyMods.Count == 0) {
                //if (DebugEnabled) Debug.Log($"_PropertyMods empty");
                return;
            }
            //if (DebugEnabled) Debug.Log($"<color=lime>HandlePropertyModifications: Count:{(_PropertyMods == null ? "NULL" : _PropertyMods.Count)}</color>");

            List<TimeflowChannel> channels = new List<TimeflowChannel>();
            foreach (KeyValuePair<string, PropertyChange> mod in _PropertyMods) {
                TimeflowChannel channel = null;

                if (mod.Value.Object.AllChannels != null) {
                    ////if (DebugEnabled) Debug.Log($"mod.Value.Object.AllChannels:{mod.Value.Object.AllChannels.Count}");
                    int i = 0;
                    foreach (TimeflowChannel ch in mod.Value.Object.AllChannels) {
                        if (ch == null || ch.ToProperty == null || string.IsNullOrEmpty(ch.ToProperty.Name)) continue;

                        string propName = StripNameToCompare(ch.ToProperty.Name);
                        string modName = StripNameToCompare(mod.Value.Name);
                        string aliasName = StripNameToCompare(mod.Value.Alias);
                        //if (DebugEnabled) Debug.Log($"<color=yellow>CheckChannel[{i}]:</color> {propName} <> {modName} <> {aliasName} attr:{ch.Attribute} <> {mod.Value.Attribute}");

                        if (ch.ToProperty.Component == mod.Value.Component && (propName == modName || propName == aliasName)) {
                            if (!Property.HasMultipleAttributes(ch.ToProperty.PropertyType)) {
                                //if (DebugEnabled) Debug.Log($"<color=lime>Found channel:</color> {ch.PathName}");
                                channel = ch;
                                break;
                            }
                            else
                            if (ch.ToProperty.Attribute < 0) {
                                // If a combined property channel already exists, don't create separate attribute channels
                                mod.Value.Attribute = -1;
                                channel = ch;
                                //if (DebugEnabled) Debug.Log($"<color=lime>Found channel:</color> {ch.PathName}");
                                break;
                            }
                            else
                            if (ch.ToProperty.Attribute == mod.Value.Attribute) {
                                //if (DebugEnabled) Debug.Log($"<color=lime>Found channel:</color> {ch.PathName}");
                                channel = ch;
                                break;
                            }
                            else {
                                //if (DebugEnabled) Debug.Log($"<color=orange>Unmatched attributes:</color> {ch.Attribute} != {mod.Value.Attribute}");
                            }
                        }
                        i++;
                    }
                }

                if (channel == null) {
                    Keyframer keyframer = ObjectUtil.GetOrAddComponent<Keyframer>(mod.Value.Object.gameObject);

                    if (TimeflowPreferences.Current.NewChannelAttributeMode == TimeflowPreferences.NewChannelAttributeModes.Combined || mod.Value.Attribute < 0) {
                        if (mod.Value.Attribute >= 0) mod.Value.Attribute = -1;
                        channel = NewChannel(mod, keyframer, mod.Value.Attribute);
                    }
                    else
                    if (TimeflowPreferences.Current.NewChannelAttributeMode == TimeflowPreferences.NewChannelAttributeModes.Separate) {
                        // Split into separate channels
                        if (mod.Value.Attribute < 0) mod.Value.Attribute = 0;
                        Property p = new Property(mod.Value.Component, mod.Value.Name, mod.Value.Attribute);

                        int attributes = Property.GetAttributeCount(Property.GetPropertyType(p.GetDataType(true)));
                        channel = NewChannel(mod, keyframer, mod.Value.Attribute);
                    }
                    else {
                        channel = NewChannel(mod, keyframer, mod.Value.Attribute);
                    }

                    //if (DebugEnabled) Debug.Log($"<color=yellow>NewChannel:</color> {channel.ToProperty.GetPathName(true)} propName:{mod.Value.Name} attribute:{mod.Value.Attribute}");
                    keyframer.AddChannel(channel);

                    Timeflow.Active.View.Display.AddObjectToDisplay(mod.Value.Component.gameObject);
                    Timeflow.Active.Refresh();
                }

                if (channel != null) {
                    if (channel.SupportsKeyframes) {
                        //if (DebugEnabled) Debug.Log($"{channel.Name}.AddKey:{Timeflow.Active.CurrentTime}");
                        channel.AddKey(Timeflow.Active.CurrentTime, true);

                        // Select the channel in the Timeflow view
                        if (Timeflow.Active.View != null && !channel.IsSelected) {
                            Timeflow.Active.View.SelectChannel(channel, false);
                        }
                    }
                    else {
                        //if (DebugEnabled) Debug.Log($"Channel does not support keyframes:{channel.PathName}");
                    }
                }
            }

            // Clear for next detected
            _PropertyMods = null;
        }

        public static int GetAutoKeyframeAttributeModifiers(Type fieldType, int attribute)
        {
            fieldType = Property.GetFieldType(fieldType);
            if (IsAlt || TimeflowPreferences.Current.NewChannelAttributeMode == TimeflowPreferences.NewChannelAttributeModes.Combined) {
                // Force combined channel creation
                attribute = -1;
            }
            else
            if (!IsControl || TimeflowPreferences.Current.NewChannelAttributeMode == TimeflowPreferences.NewChannelAttributeModes.Auto) {
                if (typeof(Color).IsAssignableFrom(fieldType) || typeof(Rect).IsAssignableFrom(fieldType)) {
                    // Force combined channel creation
                    attribute = -1;
                }
            }
            ////if (DebugEnabled) Debug.Log($"GetAutoKeyframeAttributeModifiers:{fieldType} attribute:{attribute}");

            return attribute;
        }

        private static string StripNameToCompare(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return name.ToLower()
                .Replace(" ", "")
                .Replace("eulerangles", "rotation");// ensures display name is matched
        }

        private static TimeflowChannel NewChannel(KeyValuePair<string, PropertyChange> mod, Keyframer keyframer, int attribute)
        {
            TimeflowChannel channel = new TimeflowChannel(keyframer);
            channel.ToProperty = new Property(mod.Value.Component, mod.Value.Name, attribute);
            channel.GUIColor = mod.Value.Object.GUIColor;

            if (channel.ToProperty.IsRect) {
                // Only create combined channels for Rect objects
                channel.ToProperty.Attribute = -1;
            }

            channel.Attribute = attribute;
            //if(DebugEnabled) Debug.Log($"channel.ToProperty:{channel.ToProperty.Name} attribute:{channel.ToProperty.Attribute}");

            if (mod.Value.Material != null) {
                channel.ToProperty.IsMaterial = true;

                PropertiesOfMaterial matProp = new PropertiesOfMaterial();
                matProp.GameObject = mod.Value.Object.gameObject;
                matProp.MaterialName = mod.Value.Material.name;
                matProp.GetMaterial();
                matProp.Name = mod.Value.Name;

                channel.ToProperty.Handler = matProp;
                channel.ToProperty.GetDataType(true);
            }

            return channel;
        }

        #endregion
    }

}//AxonGenesis

#endif
