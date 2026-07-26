// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is the base class for all AxonGenesis scripts. It standardizes common features and defines an
    /// improved programming interface over MonoBehavior by providing a more manageable inheritance
    /// structure. Please use this method structure creating overrides in derrived classes to ensure proper
    /// behavior.  ExecuteInEditMode is intentionally not enabled for the base behavior but may be addded
    /// by subclasses. It is highly recommended to use ExecuteInEditMode whenever possible to provide the
    /// same behavior in edit mode as runtime.
    /// </summary>
    [HelpURL("https://axongenesis.gitbook.io/timeflow")]
    public partial class AxonGenesisBehavior : MonoBehaviour, IBehavior, IBehaviorPresets, IBehaviorProperties, IBehaviorEditor
    {
        #region ENUMS

        public enum PlaybackModes
        {
            EditorAndRuntime,
            EditorOnly,
            RuntimeOnly
        }

        #endregion

        #region PUBLIC

        /// <summary>
        /// This controls whether the component works in edit mode and/or in play mode. 
        /// </summary>
        public PlaybackModes PlaybackMode = PlaybackModes.EditorAndRuntime;

        #endregion

        #region PRIVATE

        [SerializeField]
        private bool _Enabled = true;

        [SerializeField]
        private bool _DebugEnabled = false;

        [NonSerialized]
        private bool _IsAwake = false;

        [NonSerialized]
        private Rotator rotator = null;

        #endregion

        #region ACCESSORS

        /// <summary>
        /// To help avoid ambiguity between multiple instances on the same object, behaviors may display a
        /// user-defined name per component instance. By default this uses the object name but may be
        /// overridden to implement separate naming per-component. 
        /// </summary>
        public virtual string Name {
            get {
                return gameObject.name;
            }
            set {
                gameObject.name = value;
            }
        }

        /// <summary>
        /// This is different from the built-in enabled property, which disables a component entirely. If
        /// Enabled is false, the component remains active while pausing or stopping any behaviors it
        /// implements. This allows the component to stay alive and be ready immediately when Enabled.
        /// </summary>
        public virtual bool Enabled {
            get {
                return _Enabled;
            }
            set {
                if (_Enabled != value) {
                    _Enabled = value;
                    if (_Enabled) {
                        OnEnable();
                    }
                    else {
                        OnDisable();
                    }
                }
            }
        }

        /// <summary>
        /// Enables logging to the console for this component
        /// </summary>
        public virtual bool DebugEnabled {
            get {
                return _DebugEnabled && TimeflowPreferences.DebugEnabled;
            }
            set {
                if (_DebugEnabled != value) {
                    _DebugEnabled = value;
                    OnDebugEnabled();
                }
            }
        }

        /// <summary>
        /// This helps resolve strange situations in the editor when Update is called before Awake.
        /// </summary>
        public bool IsAwake {
            get {
                return _IsAwake;
            }
            protected set {
                _IsAwake = value;
            }
        }

        /// <summary>
        /// All behaviors whenever possible, and when it makes sense to do so, should implement behaviors
        /// that performs identical or as similar as possible in both edit and play mode. 
        /// </summary>
        public bool IsEditorAndRuntime {
            get {
                return PlaybackMode == PlaybackModes.EditorAndRuntime;
            }
        }

        /// <summary>
        /// Any behaviors with this setting are immediately destroyed upon awake at runtime. Only use this
        /// for components which are strictly used in the editor.
        /// </summary>
        public bool IsEditorOnly {
            get {
                return PlaybackMode == PlaybackModes.EditorOnly;
            }
        }

        /// <summary>
        /// When this mode is enabled, the behavior only updates during play mode or app runtime. This may
        /// be required for behaviors that use simulation or other calculations that don't map to linear
        /// time. This can also be used to temporarily disable animation channels in edit mode to improve
        /// performance while working in other areas of a scene.
        /// </summary>
        public bool IsRuntimeOnly {
            get {
                return PlaybackMode == PlaybackModes.RuntimeOnly;
            }
        }

        /// <summary>
        /// The Rotator component stabilizes Euler rotations on objects by allowing values to exceed 360.
        /// This is to overcome problems with the transform Euler angles not retaining the number of turns.
        /// Without this stabilization, rotations are choppy and upredictable when interpolated.
        /// </summary>
        public Rotator Rotator {
            get {
                if (rotator == null) {
                    rotator = Rotator.Setup(gameObject, true);
                }
                return rotator;
            }
        }

        #endregion

        #region UNITY MESSAGES

        /// <summary>
        /// OnEnable is a Unity message that is called right after Awake and anytime the game object is
        /// activated from a deactive state. Override this method to implement any reset actions required
        /// when the object wakes up. OnAwake is only called once per created object, whereas OnEnable
        /// occurs each time the object is activated. 
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!IsAwake) OnAwake();
        }

        /// <summary>
        /// OnDisable is called anytime an object is about to be deactivated or destroyed. Override this
        /// method to clean up references and other associated data.
        /// </summary>
        protected virtual void OnDisable() { }

        private void Awake()
        {
            IsAwake = true;
            OnAwake();
        }

        private void OnDestroy()
        {
            IsAwake = false;
            OnDestruct();
        }

        private void Start()
        {
            OnStart();
        }

        #endregion

        #region SETUP & SETDOWN

        /// <summary>
        /// Derived classes should override OnAwake rather than using Awake so that inheritance can be
        /// applied. If an object has EditorOnly set to true, it will be destoyed upon awake during play
        /// mode.
        /// </summary>
        protected virtual void OnAwake()
        {
            IsAwake = true;
            if (IsEditorOnly && Application.isPlaying) {
                Destroy(this);
            }
        }

        /// <summary>
        /// Override OnDestruct to dispose of items before the object is destroyed, instead of using
        /// OnDestroy. Its important to note that objects are destroyed when deleted from a scene, but also
        /// when unloading a scene (after serialization).
        /// </summary>
        protected virtual void OnDestruct() { }

        /// <summary>
        /// Derrived classes should override OnStart (instead of Start) to be processed in the correct
        /// order. OnStart is called after OnAwake and before OnUpdate.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Override to customize any setup/setdown when DebugEnabled is changed.
        /// </summary>
        protected virtual void OnDebugEnabled() { }

        #endregion

        #region OBJECT REFERENCES

        public Component GetComponent()
        {
            return this;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public Type GetComponentType()
        {
            return GetType();
        }

        #endregion

        #region PUBLIC UTILS

        /// <summary>
        /// Override to implement any setup operations that are required when forcing a refresh of objects.
        /// This is also called after undo operations to resetup any unserialized parameters.
        /// </summary>
        public virtual void Refresh()
        {
        }

        /// <summary>
        /// To facilitate in creating duplicates, the Copy method allows each class the opportunity to
        /// control how specific attributes are copied (or not) from one instance to another. When an
        /// object is copied using this method, the current object may copy values from the src object.
        /// Derrived classes can cast src to the subclass type as needed. Non-serialized members are
        /// skipped since they typically relate to state or setup conditions that each behavior should
        /// handle by overriding this method. This base method is only a starting point to copy the main
        /// serialized configuration of the object.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="includeChannels">if true, the component is copied with only the selected channels,
        ///     otherwise all channels are included, or as per each behaviors implementation.</param>
        public virtual void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            foreach (System.Reflection.FieldInfo f in src.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                if (!f.IsStatic && !f.IsNotSerialized && f.GetValue(src) != null) {
                    Type t = f.GetValue(src).GetType();
                    /// Copy everything except for channels which must be handled by the derrived class
                    if (!typeof(TimeflowChannel).IsAssignableFrom(t)) {
                        f.SetValue(this, f.GetValue(src));
                    }
                }
            }
        }

        #endregion

        #region EDITOR

#if UNITY_EDITOR

        /// <summary>
        /// This controls the main foldout for the UI in displayed in the Inspector. To promote less
        /// cluttered inspector views, the UI starts collapsed.
        /// </summary>
        [HideInInspector]
        public bool EditorShowUI = true;

        /// <summary>
        /// Allows editor scripts to force a behavior to update, but not allowed from runtime code.
        /// </summary>
        public void EditorUpdate() { /*OnUpdate();*/ }

        #region PRESETS

        public virtual void OnSavePreset(AdvancedPreset objPreset = null, ComponentPreset compPreset = null) { }

        public virtual void OnPresetApplied(AdvancedPreset objPreset = null, ComponentPreset compPreset = null) { }

        /// <summary>
        /// This method is called before a preset is saved to allow the behavior to modify the list of
        /// items that will be saved in the preset. This is useful for removing items that should not
        /// be saved, such as references to other objects that are not part of the preset. The items
        /// can also be selected for saving or not.
        public virtual void OnBeforeSavePreset(ref List<ComponentPresetListItem> items)
        {
            if (items == null || items.Count == 0) return;
            List<ComponentPresetListItem> toremove = new List<ComponentPresetListItem>();
            foreach (ComponentPresetListItem item in items) {
                if (item.Name == "Current Time" || item.Name == "Debug Enabled" || item.Name == "Enabled" || item.Name.StartsWith("Editor")) {
                    toremove.Add(item);
                }
                if (item.Name == "Update After" || item.Name == "Update Frequency" || item.Name == "Time Offset" || item.Name.StartsWith("Time Scale")) {
                    item.IsSelected = false;
                }
                // Don't automatically include object references since most cases they are in scene and cannot be saved
                if (item.Type == SerializedPropertyType.ObjectReference) {
                    item.IsSelected = false;
                }
            }

            if (toremove.Count > 0) {
                foreach (ComponentPresetListItem item in toremove) {
                    items.Remove(item);
                }
            }
        }

#if TIMEFLOW_LEGACY_PRESETS

        public Component PresetTarget => this;

        /// <summary>
        /// Override to implement custom behavior when a preset has been applied. This could be to update
        /// object and property references to local objects.
        /// </summary>
        public virtual void LegacyOnPresetApplied(BehaviorPreset preset) { }

        /// <summary>
        /// This method is called any time a preset is being saved so that the behavior may perform
        /// additional operations, such as assinging data not handled by reflection.
        /// </summary>
        public virtual void LegacyOnSavePreset(BehaviorPreset preset) { }

        /// <summary>
        /// Remembers user choice to not be warned when overwriting a preset
        /// </summary>
        private static bool showPresetOvewriteWarning = true;

        public virtual void LegacySavePreset()
        {
            string typeName = GetType() + "Preset";
            Type type = Type.GetType(typeName);

            /// Strip away namespace names such as AxonGenesis.
            int i = typeName.LastIndexOf('.');
            if (i >= 0) {
                i++;
                typeName = typeName.Substring(i);
            }

            /// Remove 'Preset' to get final type name
            typeName = typeName.Replace("Preset", "");

            string presetsPath = AssetDatabase.GUIDToAssetPath("0156945b9b3bfe34c8862670ca09d15e");
            if (string.IsNullOrEmpty(presetsPath)) {
                presetsPath = "Assets/Presets";
            }
            //Debug.Log($"presetsPath:{presetsPath}");
            string path = $"{presetsPath}/{typeName}/";

            BehaviorPresetInfo info = new BehaviorPresetInfo();
            info.Name = Name;
            info.Path = path;
            info.PresetType = type;
            info.Target = this;
            NamePopup.Show("Please enter a preset name", info.Name, LegacyOnSavePreset, (object)info);
        }

        public bool LegacyOnSavePreset(NameData userData)
        {
            bool saved = false;
            if (userData != null) {
                if (string.IsNullOrEmpty(userData.Name)) {
                    userData.Name = "Default";
                }
                BehaviorPresetInfo info = (BehaviorPresetInfo)userData.Data;
                if (info == null) {
                    Debug.LogError("Invalid preset data");
                }
                else {
                    info.Name = userData.Name;
                    string path = info.Path + info.Name + ".asset";
                    if (CanSavePreset(info, path)) {
                        BehaviorPreset preset = (BehaviorPreset)ScriptableObject.CreateInstance(info.PresetType);
                        if (preset == null) {
                            Debug.LogError("Invalid preset type: " + info.PresetType);
                        }
                        else {
                            preset.ReadFrom(this);
                            AssetDatabase.CreateAsset(preset, path);
                            AssetDatabase.SaveAssets();
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, preset.GetType());
                            EditorGUIUtility.PingObject(obj);
                            Debug.Log("Saved Preset: " + path);//--KEEP
                            saved = true;
                        }
                    }
                }
            }
            return saved;
        }

        private bool CanSavePreset(BehaviorPresetInfo info, string path)
        {
            bool canSave = true;
            if (showPresetOvewriteWarning) {
                /// Check for an existing asset and warn about overwriting it
                BehaviorPreset asset = (BehaviorPreset)AssetDatabase.LoadAssetAtPath(path, info.PresetType);
                if (asset != null) {
                    canSave = false;
                    int v = EditorUtility.DisplayDialogComplex("Overwrite Preset?",
                        "Do you want to overwrite the preset named '" + info.Name + "'", "Yes", "Cancel", "Yes, don't show again");

                    if (v == 0 || v == 2) {
                        canSave = true;
                    }
                    if (v == 2) {
                        showPresetOvewriteWarning = false;
                    }
                }
            }
            return canSave;
        }
#endif


        #endregion

        /// <summary>
        /// Any derrived behaviors that don't want selection hilighting in the inspector may override this
        /// to return false.
        /// </summary>
        public virtual bool ShowSelected { get { return true; } }

        /// <summary>
        /// This is a virtual method allowing objects to implement selection behavior.
        /// </summary>
        public virtual bool IsSelected {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// This defines the display color for the object in the Inspector and Timeflow window.  Subclasses
        /// must override this to provide a meaningful definition related to the type of object.
        /// </summary>
        public virtual Color GUIColor {
            get {
                return Color.cyan;
            }
            set {
            }
        }

        /// <summary>
        /// Override and return true to hide a component entirely from property menus, making it
        /// inaccessible to animation channels which can lead to self-referencing issues otherwise
        /// </summary>
        public virtual bool ArePropertiesHidden {
            get {
                return false;
            }
        }

        /// <summary>
        /// Override this to provide a list of specific property names that are allowed. This only applies
        /// if ArePropertiesHidden returns false. If the list returned is null, then all properties are
        /// allowed.
        /// </summary>
        public virtual List<string> PropertiesList {
            get {
                return null;
            }
        }

        /// <summary>
        /// Override to provide a list of specific allowed properties, ignoring all others not in the list.
        /// If null is returned, all properties will be listed if ArePropertiesHidden is false.
        /// </summary>
        public virtual SDictionary<string, Type> GetProperties()
        {
            /// Returning null prevents hiding default properties in subclasses. Subclasses should override
            /// this to provide the property list for that component.
            return null;
        }

        /// <summary>
        /// Clear and restore the original behavior name (if other than the game object name). 
        /// </summary>
        public virtual void ResetName() { }

        public virtual void OnPropertyChanged(Property property, Property.PropertyTypes originalType, int originalAttribute)
        {
            //if (DebugEnabled) Debug.Log(name + ".OnPropertyChanged");
            ResetName();
        }

        /// <summary>
        /// This gives behaviors an opportunity to clean up any items before an object is saved as a
        /// prefab. Override this to remove any objects which should not or cannot be saved into a prefab.
        /// </summary>
        public virtual void OnSavePrefab() { }


        public void OnDrawGizmos()
        {
            DrawGizmos();
        }

        public virtual void DrawGizmos()
        {
        }

#endif
        #endregion
    }

} //AxonGenesis