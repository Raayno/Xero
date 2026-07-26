// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    /// <summary>
    /// The base class for any object that wants to synchronize with Timeflow. 
    /// </summary>
    public partial class TimeflowBehavior : AxonGenesisBehavior, ITimeflowBehavior, ITimeflowInterpolate, ITimeflowBehaviorEditor
    {
        #region ENUMS

        public enum UpdateMethods
        {
            Update,
            LateUpdate,
            FixedUpdate
        }

        public enum UpdateFrequencies
        {
            EveryFrame,
            ForceFramerate,
            TimeInterval,
            UpdateAfter,
            Explicit
        }

        #endregion

        #region SERIALIZED

        /// <summary>
        /// Specifies when this behavior updates. During edit mode the late and fixed modes are simulated
        /// by Timeflow since the corresponding Unity methods are only called at runtime. Timeflow
        /// simulates late and fixed update timing to give more accurate previews.
        /// </summary>
        public UpdateMethods UpdateMethod = UpdateMethods.Update;

        /// <summary>
        /// The following values are set through a custom inspector but hidden to prevent direct
        /// modification from the default inspector for derrived classes.
        /// </summary>
        [HideInInspector]
        public bool TrackActivated;

        public float ForceFramerate = 15f;
        public float TimeInterval = 0.1f;

        public List<TimeflowBehavior> LinkedBehaviors;

        public float DefaultValue;
        public float MinValue;
        public float MaxValue = 1f;

        [SerializeField]
        private UpdateFrequencies _UpdateFrequency = UpdateFrequencies.EveryFrame;

        [SerializeField]
        private TimeflowBehavior _UpdateAfter;

        [SerializeField]
        protected float _CurrentTime = 0;

        [SerializeField]
        protected float _TimeOffset;

        [SerializeField]
        protected float _TimeScale = 1f;

        [HideInInspector]
        [SerializeField]
        [FormerlySerializedAs("CanDragTimeOffset")]
        private bool _CanDragTimeOffset;

        public UnityEvent TrackOn;
        public UnityEvent TrackOff;
        public UnityEvent<bool> TrackVisibilityChanged;

        #endregion

        #region NON-SERIALIZED

        [NonSerialized]
        public bool IsUpdateAfter = false;

        [NonSerialized]
        protected Timeflow _timeflow;

        [NonSerialized]
        private List<TimeflowChannel> _channels;

        [NonSerialized]
        private TimeflowObject _parentObject;

        [NonSerialized]
        private bool _WasUpdatedThisFrame;

        protected bool WasUpdatedThisFrame {
            get {
                return _WasUpdatedThisFrame;
            }
            set {
                if (_WasUpdatedThisFrame != value) {
                    _WasUpdatedThisFrame = value;
                }
            }
        }

        [NonSerialized]
        protected bool areChannelsSetup;

        [NonSerialized]
        protected float lastUpdateTime;

        [NonSerialized]
        protected bool _canUpdate;

        [NonSerialized]
        private bool lastCanUpdate;

        [NonSerialized]
        protected bool IsOrphan = false;

        [NonSerialized]
        protected bool IsTrackOn = false;

        [NonSerialized]
        protected bool _shownHierarchyWarning = false;

        #endregion

        #region DELEGATES

        public delegate void OnUpdateTimeDelegate(float time);
        public event OnUpdateTimeDelegate OnUpdateTime;

        #endregion

        #region ACCESSORS

        public virtual Timeflow Timeflow {
            get {
                if (ParentObject != null && ParentObject != this && ParentObject.Timeflow != this) {
                    return ParentObject.Timeflow;
                }
                if (_timeflow == null) _timeflow = Timeflow.Active;
                return _timeflow;
            }
            set {
                //Debug.Log($"{name}.Timeflow={(value == null ? "NULL" : value.name)}");
                _timeflow = value;
            }
        }

        public TimeflowObject ParentObject {
            get { return _parentObject; }
            set {
                if (_parentObject != value) {
                    if (value == null) {
                        _parentObject = null;
                    }
                    else
                    if (ObjectUtil.IsDescendant(value.gameObject, gameObject)) {
                        if (!_shownHierarchyWarning) {
                            _shownHierarchyWarning = true;
                            Debug.LogWarning($"'{value.name}' is a descendant of '{name}' and cannot be set as its parented. Please check your object " +
                                $"hierarchy to ensure that each TimeflowObject has a Timeflow parent that is not a child or descendant of this game object.", gameObject);
                        }
                        _parentObject = null;
                    }
                    else {
                        _parentObject = value;
                        _shownHierarchyWarning = false;
                        //Debug.Log($"{name}.ParentObject:{(value == null ? "NULL" : value.name)}");
                    }
                    IsOrphan = _parentObject == null;
                }
            }
        }

        public virtual bool CanDragTimeOffset {
            get { return _CanDragTimeOffset; }
            set { _CanDragTimeOffset = value; }
        }

        /// <summary>
        /// Channels stores every channel of the current behavior, which varies depending on derrived
        /// classes. For a list of all TimeflowBehaviors, use TimeflowObject.AllChannels. IMPORTANT: These
        /// channels are NOT serialized so it is the responsibility of each behavior to store its own
        /// channels.
        /// </summary>
        public List<TimeflowChannel> Channels {
            get {
                return _channels;
            }
            protected set {
                _channels = value;
            }
        }

        public UpdateFrequencies UpdateFrequency {
            get {
                return _UpdateFrequency;
            }
            set {
                if (value != _UpdateFrequency) {
                    _UpdateFrequency = value;
                    if (UpdateFrequency != UpdateFrequencies.UpdateAfter) {
                        UpdateAfter = null;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the behavior can update based on its current settings and play state. The
        /// set method is protected since only the component itself or a baseclass should set the value.
        /// </summary>
        public virtual bool CanUpdate {
            get {
                if (!Enabled || !enabled || (IsRuntimeOnly && !Application.isPlaying)) return false;
                if (UpdateFrequency == UpdateFrequencies.UpdateAfter || UpdateFrequency == UpdateFrequencies.Explicit) {
                    _canUpdate = false; // Update is forced explicitly
                }
                else
                if (UpdateFrequency == UpdateFrequencies.EveryFrame) {
                    _canUpdate = true;
                }
                else
                if (UpdateFrequency == UpdateFrequencies.ForceFramerate) {
                    if (ForceFramerate < 0.001f) ForceFramerate = 1f;
                    float elapsed = CurrentTime - lastUpdateTime;
                    _canUpdate = elapsed < 0f || elapsed >= (1f / ForceFramerate);
                    //if (DebugEnabled) Debug.Log(Name + ".CanUpdate:" + _canUpdate + " elapsed:" + elapsed + $" CurrentTime:{CurrentTime} lastUpdateTime:{lastUpdateTime}");
                }
                else
                if (UpdateFrequency == UpdateFrequencies.TimeInterval) {
                    if (TimeInterval < 0f) TimeInterval = 0.1f;
                    float elapsed = CurrentTime - lastUpdateTime;
                    _canUpdate = elapsed < 0f || elapsed >= TimeInterval;
                }
                if (lastCanUpdate != _canUpdate) {
                    lastCanUpdate = _canUpdate;
                }
                return _canUpdate; // gameObject.activeInHierarchy is handled by track activation
            }
            protected set {
                _canUpdate = value;
            }
        }

        /// <summary>
        /// This behavior uses the standard Update call to preform its function.
        /// </summary>
        public virtual bool UseUpdate {
            get {
                return UpdateMethod == UpdateMethods.Update && CanUpdate;
            }
            set {
                if (value) {
                    UpdateMethod = UpdateMethods.Update;
                }
            }
        }

        /// <summary>
        /// This behavior executes during LateUpdate.
        /// </summary>
        public bool UseLateUpdate {
            get {
                return UpdateMethod == UpdateMethods.LateUpdate && CanUpdate;
            }
            set {
                if (value) {
                    UpdateMethod = UpdateMethods.LateUpdate;
                }
            }
        }

        /// <summary>
        /// This behavior executes only during FixedUpdate, based on the physics time.
        /// </summary>
        public bool UseFixedUpdate {
            get {
                return UpdateMethod == UpdateMethods.FixedUpdate && CanUpdate;
            }
            set {
                if (value) {
                    UpdateMethod = UpdateMethods.FixedUpdate;
                }
            }
        }

        /// <summary>
        /// The local time for the object is determined by its parent object and the TimeflowGroup it
        /// belongs to
        /// </summary>
        public virtual float LocalDeltaTime {
            get {
                if (lastUpdateTime == 0f) return 0f;
                float delta = CurrentTime - lastUpdateTime;
                if (delta < 0f) delta = 0f;
                return delta;
            }
        }

        public virtual float CurrentTime {
            get {
                if (IsOrphan) return GetTime();
                return _CurrentTime * TimeScaleWorld;
            }
            set {
                _CurrentTime = value;
                //Debug.Log($"{name}.{GetType().Name}.CurrentTime:{_CurrentTime} TimeOffsetWorld:{TimeOffsetWorld}");

                if (Channels != null) {
                    foreach (TimeflowChannel channel in Channels) {
                        //Debug.Log($"{name}.{channel.GetType().Name}.CurrentTime:{_CurrentTime} channel.TimeOffset:{channel.TimeOffset}");
                        channel.CurrentTime = _CurrentTime - channel.TimeOffset;
                    }
                }
            }
        }

        public virtual float CurrentTimeWorld {
            get {
                if (IsOrphan) return GetTime();
                return (_CurrentTime * TimeScaleWorld) + TimeOffsetWorld;
            }
            set {
                _CurrentTime = value - TimeOffsetWorld;

                if (Channels != null) {
                    foreach (TimeflowChannel channel in Channels) {
                        channel.CurrentTime = _CurrentTime - channel.TimeOffset;
                    }
                }
            }
        }

        public virtual int CurrentFrame {
            get {
                int frame = 0;
                if (Timeflow != null) {
                    frame = Mathf.RoundToInt(CurrentTime * Timeflow.FPS);
                }
                return frame;
            }
            set {
                // Can only be set by Timeflow instance
            }
        }

        public virtual float EndTime {
            get {
                if (ParentObject != null) {
                    return ParentObject.EndTime;
                }
                else {
                    Debug.LogWarning("The behavior '" + name + "." + (GetType()) + "' is not associated with a TimeflowObject");
                    return 0f;
                }
            }
            set {
            }
        }

        public virtual float TimeOffset {
            get {
                return _TimeOffset;
            }
            set {
                if (_TimeOffset != value) {
                    _TimeOffset = value;
                }
            }
        }

        public virtual float TimeOffsetWorld {
            get {
                float t = TimeOffset;
                if (ParentObject != null) {
                    if (ParentObject != this) {
                        t += ParentObject.TimeOffsetWorld;
                    }
                }
                return t;
            }
            set {
                if (ParentObject != null && ParentObject != this) {
                    TimeOffset = value - ParentObject.TimeOffsetWorld;
                }
                else {
                    TimeOffset = value;
                }
            }
        }

        public virtual float TimeOffsetWorldToLocal {
            get {
                return TimeOffsetWorld - TimeOffset;
            }
        }

        public virtual float TimeScale {
            get {
                if (_TimeScale <= 0f) {
                    _TimeScale = 1f; // Reset to default
                }
                return _TimeScale;
            }
            set {
                if (value <= TimeflowPreferences.Current.MinTimeScale) {
                    value = TimeflowPreferences.Current.MinTimeScale;
                }
                if (_TimeScale != value) {
                    _TimeScale = value;
                    if (ParentObject != null) ParentObject.OnUpdateAutoFullLength();
                }
            }
        }

        public virtual float TimeScaleWorld {
            get {
                float t = TimeScale;
                //Debug.Log($"TimeScale:{t}");
                if (ParentObject != null && ParentObject != this) {
                    //Debug.Log($"ParentObject.TimeScaleWorld:{ParentObject.TimeScaleWorld}");
                    t *= ParentObject.TimeScaleWorld;
                }
                return t;
            }
        }


        public TimeflowBehavior UpdateAfter {
            get {
                return _UpdateAfter;
            }
            set {
                if (_UpdateAfter != value) {
                    if (_UpdateAfter != null) {
                        _UpdateAfter.UnlinkBehavior(this);
                    }

                    _UpdateAfter = value;

                    IsUpdateAfter = _UpdateAfter != null;
                    if (IsUpdateAfter) {
                        _UpdateAfter.LinkBehavior(this);
                    }
                }
            }
        }

        public bool HasLinkedBehaviors {
            get {
                return LinkedBehaviors != null && LinkedBehaviors.Count > 0;
            }
        }

        #endregion

        #region LINKED BEHAVIORS       

        /// <summary>
        /// This pertains to behaviors that have an update frequency set to UpdateAfter and have linked to
        /// this one. Any linked behaviors get updated only immediately following this behaviors update.
        /// This is to solve issues where execution order among scripts matters. This can also link across
        /// objects throughout the scene.
        /// </summary>
        /// <param name="link"></param>
        public void LinkBehavior(TimeflowBehavior link)
        {
            if (LinkedBehaviors == null) LinkedBehaviors = new List<TimeflowBehavior>();
            if (!LinkedBehaviors.Contains(link)) {
                LinkedBehaviors.Add(link);
            }
        }

        public void UnlinkBehavior(TimeflowBehavior link)
        {
            if (LinkedBehaviors != null && LinkedBehaviors.Contains(link)) {
                LinkedBehaviors.Remove(link);
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            base.OnAwake();

            if (GetType() == typeof(TimeflowBehavior)) {
                Debug.LogError(gameObject.name + ": TimeflowBehavior is a base class and should not be used directly. Please remove this component.", gameObject);
            }

            IsUpdateAfter = UpdateAfter != null;

            CheckParent(true);

            Timeflow = GetComponentInParent<Timeflow>();
#if UNITY_EDITOR
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                Timeflow.Active.View.NeedsRefresh = true;
            }
#endif
        }

        protected override void OnDestruct()
        {
            if (Channels != null) {
                // Copy the list to avoid modifying the collection during iteration
                List<TimeflowChannel> tmp = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in Channels) {
                    tmp.Add(ch);
                }
                foreach (TimeflowChannel ch in tmp) {
                    RemoveChannel(ch);
                }
            }
            TimeflowObject.UnregisterBehavior(this);
            base.OnDestruct();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TimeflowObject.RegisterBehavior(this);
        }

        public virtual void OnRenderEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDebugEnabled()
        {
            base.OnDebugEnabled();
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    if (ch != null) {
                        ch.DebugEnabled = false;
                        if (ch.IsLinked) {
                            ch.Link.DebugEnabled = false;
                        }
                        if (ch.ToProperty != null) {
                            ch.ToProperty.DebugEnabled = false;
                        }
                    }
                }
            }
        }

        public virtual void OnStartPlayback()
        {
            //if (DebugEnabled) Debug.Log(GetType() + ": " + name + ".OnStartPlayback");
            OnUpdateTimingMode();
            CheckParent(true);
        }

        public virtual void OnUpdateTimingMode()
        {
            //if (DebugEnabled) Debug.Log(GetType() + ": " + name + ".OnUpdateTimingMode");
            SetupChannels(true);
        }

        /// <summary>
        /// Checked the linked behaviors to remove any that aren't explicitly connected.
        /// </summary>
        public virtual void CheckLinkedBehaviors()
        {
            //if (DebugEnabled) Debug.Log(GetType() + ": " + name + ".CheckLinkedBehaviors");
            if (LinkedBehaviors != null && LinkedBehaviors.Count > 0) {
                bool modified = false;
                List<TimeflowBehavior> newList = new List<TimeflowBehavior>();
                foreach (TimeflowBehavior b in LinkedBehaviors) {
                    if (b == null) {
                        modified = true;
                    }
                    else
                    if (b.UpdateAfter == this) {
                        if (b.UpdateFrequency == UpdateFrequencies.UpdateAfter) {
                            newList.Add(b);
                        }
                        else {
                            b.UpdateAfter = null;
                        }
                    }
                    else {
                        modified = true;
                    }
                }
                if (modified) LinkedBehaviors = newList;
            }
        }

        public override void Refresh()
        {
            //Debug.Log($"{name}.{GetType().Name}.Refresh:{CurrentTime}");
            base.Refresh();
            CheckParent(true);
            SetupChannels(true);
        }

        /// <summary>
        /// Override to update any Property object references to retarget them to the game objects they belong to.
        /// This may be used after copying components from one object to another to ensure properties refer
        /// to the new objects and not the original ones.
        /// </summary>
        public virtual void RemapProperties()
        {
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    if (ch != null) {
                        ch.RemapProperties(this);
                    }
                }
            }
        }

        #endregion

        #region CHANNELS

        /// <summary>
        /// Every derrived class of TimeflowBehavior must implement SetupChannels to register the
        /// TimeflowChannels it uses. Or if your class does not use channels, then override this to do
        /// nothing. During setup, each channel must also be registered with TimeflowObject or it will not
        /// be visible to the Timeflow window.
        /// </summary>
        public virtual void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (Channels == null) {
                Channels = new List<TimeflowChannel>();
            }
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    ch.OnSetup(this);
                }
            }

            CheckLinkedBehaviors();
        }

        /// <summary>
        /// This method is called only by TimeflowObject to gather all the channels on the current object.
        /// This class (or any derrived class) is responsible for caling obj.RegisterChannel() for each
        /// channel you want to be processed normally. Override this behavior if you have custom channels
        /// definitions not included in ChannelList.
        /// </summary>
        /// <param name="obj"></param>
        public virtual void RegisterChannels(TimeflowObject obj)
        {
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    if (ch.Behavior == null) ch.Behavior = this; // May be unset during undo operations
                    obj.RegisterChannel(ch);
                }
            }
        }
        public virtual void AfterSetup()
        {
        }

        public virtual void AddChannel(TimeflowChannel channel)
        {
#if UNITY_EDITOR
            EditorUtil.SetDirty(this);
            TrackColorPalette.UpdateChannelColor(channel);
#endif
            channel.Behavior = this;
            if (Channels == null) {
                Channels = new List<TimeflowChannel>();
            }
            if (!HasChannel(channel)) {
                if (Channels.Count > 0) {
                    channel.SortOrder = Channels[Channels.Count - 1].SortOrder + 100;
                }
                Channels.Add(channel);
#if UNITY_EDITOR
                channel.Select();
#endif
            }
            if (ParentObject == null) CheckParent(true);
            TimeflowObject obj = ParentObject;
#if UNITY_EDITOR
            obj.AllChannelsReverse = null; // force to rebuild list
#endif
            if (obj.AllChannels == null) {
                obj.AllChannels = new List<TimeflowChannel>();
            }
            if (!obj.HasChannel(channel)) {
                obj.AllChannels.Add(channel);
            }

            // Initialize the time 
            channel.CurrentTime = _CurrentTime;
        }

        public virtual void RemoveChannel(TimeflowChannel channel)
        {
            if (channel == null) return;
#if UNITY_EDITOR
            EditorUtil.SetDirty(this);
#endif
            channel.RemoveAllLinks();
            if (HasChannel(channel)) {
                Channels.Remove(channel);
            }
            if (ParentObject != null) {
                if (ParentObject.HasChannel(channel)) {
                    ParentObject.AllChannels.Remove(channel);
#if UNITY_EDITOR
                    ParentObject.AllChannelsReverse = null; // force to rebuild list
#endif
                }
            }
            channel.Destruct();
        }

        /// <summary>
        /// Determines the behavior of duplicating a channel, which could be from another behavior and/or
        /// gameobject. The default behavior of this method is to make a copy of the whole component. But
        /// if your derived class handles multiple channels at once, then override this function to make a
        /// copy of that channel within your component.
        /// </summary>
        /// <param name="channel">The source channel to copy</param>
        /// <param name="dstObject">The target gameobject to which the new channel should belong</param>
        public virtual TimeflowChannel DuplicateChannel(TimeflowChannel channel, GameObject dstObject = null, bool deleteOriginal = false)
        {
            if (channel == null) {
                Debug.LogError("Channel to duplicate is null");
                return null;
            }
            if (channel.Behavior == null) {
                Debug.LogError("Attempting to duplicate a channel that has no parent");
                return null;
            }
            if (dstObject == null) {
                dstObject = gameObject;
            }
            TimeflowChannel dup = null;
            string newName = StringUtil.IncrementName(channel.Name);
#if UNITY_EDITOR
            UndoUtil.Undo(dstObject, "Duplicate Channels", true);
#endif
            bool isSameObject = channel.Behavior.gameObject == dstObject;

            if (isSameObject && SupportsMultipleChannels()) {
                TimeflowBehavior comp = (TimeflowBehavior)dstObject.GetComponent(channel.Behavior.GetType());
                if (comp == null) {
                    comp = (TimeflowBehavior)ObjectUtil.AddComponent(dstObject, channel.Behavior.GetType());
#if UNITY_EDITOR
                    UndoUtil.UndoCreate(comp, "Duplicate Channels");
#endif
                }
                dup = comp.CopyChannel(channel);
                if (deleteOriginal) {
                    channel.Behavior.RemoveChannelWithUndo(channel);
                }
            }
            else {
#if UNITY_EDITOR
                UndoUtil.Undo(dstObject, "Duplicate Channels");
#endif
                TimeflowBehavior newComp = (TimeflowBehavior)dstObject.GetComponent(channel.Behavior.GetType());
                bool useExisting = true;
                bool supportsMulti = channel.Behavior.SupportsMultipleChannels();
                if (!supportsMulti && newComp != null) {
                    EditorUtil.ShowDialog($"Failed to duplicate the channel for {newComp.GetType()}", "The object already has a behavior " +
                        $"of this type and does not allow multiple instances on the same game object.");
                    return null;
                }
                if (newComp == null) {
                    newComp = (TimeflowBehavior)ObjectUtil.AddComponent(dstObject, channel.Behavior.GetType());
                    if (newComp == null) {
                        EditorUtil.ShowDialog($"Failed to duplicate the channel for {newComp.GetType()}", "This could be due to a special " +
                            "channel type controlled by specific behavior settings, enabled by a special checkbox " +
                            "or mode. As an alternate method, try copying the entire component behavior which owns the channel");
                    }
                    else {
#if UNITY_EDITOR
                        UndoUtil.UndoCreate(newComp, "Duplicate Channels");
#endif
                    }
                    useExisting = false;
                }
                if (newComp != null) {
                    if (!useExisting) {
                        /// Behaviors that accept multiple channels such as Keyframer must only
                        /// copy over the selected channels, whereas other behaviors with a fixed
                        /// number of channels (usually 1) will copy the channel when the component
                        /// is duplicated.
                        newComp.Copy(channel.Behavior, !supportsMulti);
                        if (supportsMulti) {
                            dup = newComp.CopyChannel(channel);
                        }
                        else {
                            dup = newComp.GetChannel(channel.Name);
                        }
                    }
                    else
                    if (supportsMulti) {
                        dup = newComp.CopyChannel(channel);
                    }

                    if (deleteOriginal) {
                        if (supportsMulti) {
                            channel.Behavior.RemoveChannelWithUndo(channel);
                        }
                    }
                }
            }

            channel.CleanUp();
            if (dup != null) {
                dup.Name = newName;
                dup.CleanUp();
            }
            return dup;
        }

        public virtual void DeleteAllChannels()
        {
            if (Channels != null) {
#if UNITY_EDITOR
                UndoUtil.Undo(this, "Delete All Channels", true);
#endif
                List<TimeflowChannel> list = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in Channels) {
                    list.Add(ch);
                }
                foreach (TimeflowChannel ch in list) {
                    RemoveChannelWithUndo(ch);
                }
                Channels = null;
            }
        }

        /// <summary>
        /// Override to implement custom behavior to fully delete the channel
        /// </summary>
        public virtual void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            if (channel != null) {
#if UNITY_EDITOR
                UndoUtil.Undo(this, "Delete Channel", true);
#endif
                RemoveChannel(channel);

            }
        }

        public virtual void CleanUp()
        {
            if (Channels != null) {
                foreach (TimeflowChannel ch in Channels) {
                    ch.CleanUp();
                    if (!ch.IsTrack) {
                        ch.SetParent(this);
                    }
                }
            }
        }

        public virtual TimeflowChannel CopyChannel(TimeflowChannel src)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
#endif
            TimeflowChannel copy = new TimeflowChannel(this);
            copy.Copy(src);
            copy.SetParent(this);
            if (src.ToProperty != null) {
                copy.ToProperty = new Property(this, src.ToProperty);
                if (copy.ToProperty.IsDataOnly) {
                    copy.ToProperty.Comp = transform;
                }
                else {
                    copy.ToProperty.SwitchGameObject(gameObject);
                }
            }
            AddChannel(copy);
            return copy;
        }

        /// <summary>
        /// Override for behaviors that can have multiple channels.
        /// </summary>
        /// <returns></returns>
        public virtual bool SupportsMultipleChannels()
        {
            return false;
        }

        public virtual void SortChannels()
        {
#if UNITY_EDITOR
            if (Channels != null) {
                if (Channels.Count > 1) {
                    if (TimeflowPreferences.Current.ReverseChannelOrder) {
                        Channels.Sort(new SortTimeflowChannelDescending());
                    }
                    else {
                        Channels.Sort(new SortTimeflowChannelAscending());
                    }
                }
                foreach (TimeflowChannel ch in Channels) {
                    if (ch.Keys != null) {
                        ch.Keys.Sort(KeyframeSort.ByTimeAsc);
                    }
                }
            }

#endif
        }

        public virtual void SortAlphabetically()
        {
#if UNITY_EDITOR
            if (Channels == null || Channels.Count == 0) return;

            if (TimeflowPreferences.Current.ReverseChannelOrder) {
                Channels.Sort((x, y) => {
                    return y.Name.CompareTo(x.Name);
                });
            }
            else {
                Channels.Sort((x, y) => {
                    return x.Name.CompareTo(y.Name);
                });
            }
            for (int i = 0; i < Channels.Count; i++) {
                Channels[i].SortOrder = i * 100;
            }
#endif
        }

        /// <summary>
        /// Any behaviors that support multiple channels should override this method to implement any
        /// additional sorting operations.
        /// </summary>
        public virtual void OnSortChannels() { }

        public virtual TimeflowChannel GetChannel(string name)
        {
            if (Channels == null || Channels.Count == 0) return null;
            TimeflowChannel channel = null;
            foreach (TimeflowChannel ch in Channels) {
                if (ch == null || string.IsNullOrEmpty(ch.Name)) continue;
                if (ch.Name.Equals(name)) {
                    channel = ch;
                    break;
                }
            }
            return channel;
        }

        public virtual TimeflowChannel GetChannel(string propertyName, int attribute)
        {
            if (Channels == null || Channels.Count == 0) return null;
            TimeflowChannel channel = null;
            foreach (TimeflowChannel ch in Channels) {
                if (ch != null && ch.ToProperty.Name.Equals(propertyName) && ch.ToProperty.Attribute == attribute) {
                    channel = ch;
                    break;
                }
            }
            return channel;
        }

        public virtual TimeflowChannel GetChannelByID(string id)
        {
            if (Channels == null || Channels.Count == 0) return null;
            TimeflowChannel channel = null;
            foreach (TimeflowChannel ch in Channels) {
                if (ch != null && ch.UniqueID.Equals(id)) {
                    channel = ch;
                    break;
                }
            }
            return channel;
        }

        public virtual bool HasChannelNamed(string name)
        {
            bool hasChannel = false;
            if (Channels == null) return false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch != null && ch.HasProperty && ch.ToProperty != null && ch.ToProperty.Name.Equals(name)) {
                    hasChannel = true;
                    break;
                }
            }
            return hasChannel;
        }

        public virtual bool HasChannelNamed(string name, int index)
        {
            bool hasChannel = false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.ToProperty != null && ch.ToProperty.Name.Equals(name) && ch.ToProperty.Attribute == index) {
                    hasChannel = true;
                    break;
                }
            }
            return hasChannel;
        }

        public virtual bool HasChannel(string pathName)
        {
            if (Channels == null) return false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.PathName == pathName) {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasChannel(Component obj, string name)
        {
            bool hasChannel = false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.ToProperty != null && ch.ToProperty.Comp == obj && ch.ToProperty.Name.Equals(name)) {
                    hasChannel = true;
                    break;
                }
            }
            return hasChannel;
        }

        public virtual bool HasChannel(Component obj, string name, int index)
        {
            bool hasChannel = false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.ToProperty != null && ch.ToProperty.Comp == obj && ch.ToProperty.Name.Equals(name) && ch.ToProperty.Attribute == index) {
                    hasChannel = true;
                    break;
                }
            }
            return hasChannel;
        }

        public virtual bool HasChannel(TimeflowChannel channel)
        {
            if (Channels == null) return false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.UniqueID == channel.UniqueID) {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasComponent(Component obj)
        {
            bool hasComponent = false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.ToProperty != null && ch.ToProperty.Comp == obj) {
                    hasComponent = true;
                    break;
                }
            }
            return hasComponent;
        }

        /// <summary>
        /// Call this to notify the channels that a vector value has changed. This allows channels to
        /// reprocess interpolation and other setup that depend on the keyframe vector values.
        /// </summary>
        public virtual void OnVectorChanged()
        {
            if (Channels != null) {
                foreach (TimeflowChannel ch in Channels) {
                    if (ch.IsVector) ch.IsVectorChanged = true;
                }
            }
        }

        /// <summary>
        /// Receives notification that the interpolation settings have changed. Behaviors may override this
        /// to perform additional setup operation for proper keyframe interpolation.
        /// </summary>
        public virtual void OnInterpolationChanged()
        {
        }

        /// <summary>
        /// Returns a list of Property objects. Subclasses of TimelineBehavior should implement this to return a list of properties
        /// which the component manages but are not in the main field interface. For example, Keyframer has properties deeper in its
        /// TimelineChannel instances that would not be found by traversing FieldInfo or PropertyInfo.
        /// </summary>
        public virtual List<Property> GetSubProperties()
        {
            List<Property> list = new List<Property>();
            if (Channels != null) {
                foreach (TimeflowChannel channel in Channels) {
                    list.Add(channel.ToProperty);
                }
            }
            return list;
        }

        #endregion

        #region UPDATE
        /// All derrrived classes should override the following methods instead of using the built-in Unity
        /// messages to work properly with inherited and overridden behaviors.

        /// <summary>
        /// Derived classes should override OnUpdate (rather than Update) to support class inheritance.
        /// </summary>
        protected virtual void OnUpdate()
        {
            WasUpdatedThisFrame = true;
        }

        /// <summary>
        /// This is the fixed update call which all behaviors receive. Derived classes can optionally
        /// override OnFixedUpdate if fixed update calls are desired. This is called at the interval
        /// defined in Projects Settings > Time > Fixed Timestep, used by physics. NOTE: OnUpdate() is
        /// called also for fixed update objects, so subclasses should only implement OnFixedUpdate() if
        /// there is special behavior that must not be done in OnUpdate. The main idea is to impelement
        /// behaviors that work the same with any update mode and to allow the user to select the mode for
        /// performance optimization.
        /// </summary>
        public virtual void OnFixedUpdate()
        {
            WasUpdatedThisFrame = true;
        }

        /// <summary>
        /// Derrived classes can override OnLateUpdate to receive calls at the end of rendering after
        /// OnLateUpdate. This requires setting the UseLateUpdate option to true. This is used by
        /// components which require the frame to be fully rendered before executing.
        /// </summary>
        protected virtual void OnLateUpdate()
        {
            WasUpdatedThisFrame = true;
        }

        /// <summary>
        /// This is a late update call for all behaviors. OnFinalUpdate ignores the UseLateUpdate
        /// setting and receives the call at the end of every frame.  This may seem redundant however,
        /// there is a special usecase when a baseclass requires a call at the end of each frame but still
        /// allows subclasses the ability to set whether they get the late update call, which is frequently
        /// an option for the end-user to decide depending on the situation.
        /// </summary>
        public virtual void OnFinalUpdate()
        {
            if (WasUpdatedThisFrame) {
                //Debug.Log($"{name}.{GetType().Name}.OnFinalUpdate wasUpdatedThisFrame:{CurrentTime}");
                lastUpdateTime = CurrentTime;
                WasUpdatedThisFrame = false;
            }
        }

        public virtual void OnPlay()
        {
            // Override to get notified of call to Play()
        }

        public virtual void OnStop()
        {
            // Override to get notified of call to Stop()
        }

        public virtual void OnTrackStart()
        {
            if (IsTrackOn) return;
            IsTrackOn = true;
            // Override to get notified of when Timeflow track section begins
            //if (DebugEnabled) Debug.Log($"{name}.OnTrackStart", gameObject);
            if (TrackOn != null) TrackOn.Invoke();
            if (TrackVisibilityChanged != null) TrackVisibilityChanged.Invoke(true);
        }

        public virtual void OnTrackEnd()
        {
            if (!IsTrackOn) return;
            IsTrackOn = false;

            // Override to get notified of when Timeflow track section ends
            //if (DebugEnabled) Debug.Log($"{name}.OnTrackEnd", gameObject);
            if (TrackOff != null) TrackOff.Invoke();
            if (TrackVisibilityChanged != null) TrackVisibilityChanged.Invoke(false);
        }

        /// <summary>
        /// This should only be called by core Timeflow methods and cannot be overriden. This ensures that
        /// the timing and linked behaviors are processed indepently of overrides of UdpateTime.
        /// </summary>
        public void DoUpdate()
        {
            if (!IsAwake) {
                return;
                //OnAwake(); // To ensure scripts are re-awoken after compiling
            }
            DoUpdate(false);
        }

        public void DoUpdate(bool explicitChannels)
        {
            if(DebugEnabled) Debug.Log($"{name}.DoUpdate  explicitChannels:{explicitChannels}");
            UpdateTime();
            if (explicitChannels) {
                UpdateTimeChannelsExplicit();
                UpdateTimeLinked();
            }
        }

        /// <summary>
        /// Any derrived classes that implement time-based behavior using channels must override
        /// UpdateTimeChannel. This allows channels to be be processed in a specific order across multiple
        /// behaviors, by TimeflowObject. If your class does not use channels but still wants to be synced
        /// with the Timeflow, you can override UpdateTime directly.
        /// </summary>
        public virtual void UpdateTime()
        {
            if(DebugEnabled) Debug.Log($"{name}.{GetType().Name}.UpdateTime:{CurrentTime}");
            WasUpdatedThisFrame = true;
            if (OnUpdateTime != null) OnUpdateTime(CurrentTime);
        }

        /// <summary>
        /// Use this method to force an update of the behavior outside of the normal update cycle. This is
        /// useful in the editor while not playing to see immediate results when changing values.
        /// </summary>
        public virtual void ForceUpdate()
        {
            UpdateTime();
            UpdateTimeChannelsExplicit();
        }

        /// <summary>
        /// Updates any behaviors which have been set to UpdateAfter this behavior.
        /// </summary>
        public virtual void UpdateTimeLinked()
        {
            if (LinkedBehaviors != null && LinkedBehaviors.Count > 0) {
                /// Linked behaviors are any that have registered to update after this one (using UpdateAfter)
                foreach (TimeflowBehavior t in LinkedBehaviors) {
                    if (t != null && t != this) {
                        t.DoUpdate(true);
                    }
                }
            }
        }

        /// <summary>
        /// This method is only used when explicitly updating a behavior outside of the normal update
        /// methods. This only affects the channels on this behavior (separate from other behaviors on the
        /// same object)
        /// </summary>
        public virtual void UpdateTimeChannelsExplicit()
        {
            //if (DebugEnabled) Debug.Log("UpdateTimeChannels:" + (Channels == null ? "NULL" : Channels.Count + ""));
            if (Channels != null) {
                for (int i = 0; i < Channels.Count; i++) {
                    TimeflowChannel ch = Channels[i];
                    if (ch != null && ch.IsEnabled) {
                        UpdateTimeChannel(ch);
                    }
                }
            }

        }

        /// <summary>
        /// Any derrived classes that implement time-based behavior must override UpdateTimeChannel to
        /// properly handle updates in order.  Since channels can be processed in a specific order, it is
        /// possible to layer them, however it is the responsibility of each channel (or behavior that
        /// implements them) to handle the layering. The primary layer processing is handled automatically
        /// by TimeflowObject, so do not bypass that functionality unless you need to do something that
        /// requires it.
        /// </summary>
        public virtual void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (!Enabled) return;
            if (!channel.HasProperty || (channel.ToProperty != null && channel.ToProperty.IsValid())) {
                if(DebugEnabled) Debug.Log($"{name}.{GetType().Name}.UpdateTimeChannel:{channel.Name} Enabled:{Enabled}");
                WasUpdatedThisFrame = true;
                //Debug.Log($"{name}.{GetType().Name}.UpdateTimeChannel:{channel.Name} CurrentTime:{CurrentTime} channel.CurrentTime:{channel.CurrentTime}");
                channel.Interpolate(channel.CurrentTime);
            }
        }

#if UNITY_EDITOR

        private float _lastAutoKeyframeTime = 0;
        private int _lastAutoKeyframeCount = 0;

        private void Update()
        {
            // Only check for keyframe change when not in play mode
            if (Application.isPlaying || Timeflow == null || !Timeflow.AutoKeyframingEnabled || Timeflow.IsPlaying) return;

            if (Timeflow.Active.View != null && Timeflow.Active.View.Input != null && Timeflow.Active.View.Input.IsDragging) {
                // Don't detect auto keyframing while dragging anywhere
                return;
            }

            // Detect on selected objects only
            if (Selection.gameObjects == null) return;
            bool isSelected = false;
            foreach (GameObject go in Selection.gameObjects) {
                if (go == gameObject) {
                    isSelected = true;
                    break;
                }
            }
            if (!isSelected) return;

            if (_lastAutoKeyframeTime == CurrentTime) {
                if (_lastAutoKeyframeCount < 3) {
                    _lastAutoKeyframeCount++;
                    return;
                }
                AutoKeyframingDetect();

                if (Channels != null) {
                    for (int i = 0; i < Channels.Count; i++) {
                        TimeflowChannel ch = Channels[i];
                        if (ch != null && ch.IsEnabled) {
                            ch.AutoKeyframingDetect();
                        }
                    }
                }
            }
            else {
                // Ensures change is made at same time as last update
                _lastAutoKeyframeTime = CurrentTime;
                _lastAutoKeyframeCount = 0;
            }
        }

        protected virtual void AutoKeyframingDetect() { }
#endif

        public virtual void OnRewind()
        {
            lastUpdateTime = 0f; // Force to 0 to avoid incorrect LocalDeltaTime
        }

        public virtual void CheckParent(bool force)
        {
            //Debug.Log($"{name}.{GetType().Name}.CheckParent:{force}");
            if (force || ParentObject == null) {
                // Gets the TimeflowObject on this game object. 
                ParentObject = gameObject.GetComponent<TimeflowObject>();
                if (ParentObject == null) {
                    ParentObject = ObjectUtil.AddComponent<TimeflowObject>(gameObject);
                }
                ParentObject.enabled = true;
            }
        }

        public virtual float GetTime()
        {
            if (ParentObject == null) CheckParent(false);
            if (ParentObject != null) {
                return ParentObject.GetTime() * TimeScaleWorld - TimeOffset;
            }
            else {
                Debug.LogWarning("The behavior '" + name + "." + (GetType()) + "' is not associated with a TimeflowObject", gameObject);
                return 0f;
            }
        }

        public virtual void SetTime(float time)
        {
            //Debug.Log($"{name}.{GetType().Name}.TimeflowBehavior.SetTime:{time}", gameObject);
            CurrentTime = time;
        }

        #endregion

        #region INTERPOLATION

        /// <summary>
        /// Override the following methods to implement custom data behavior. Note that all interpolation
        /// times are in the channel's local time. It's also important to note that caching is handled
        /// automatically by the channel so each behavior that implements these method need not be
        /// concerned with reading or writing cache data.
        /// </summary>
        public virtual float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.FloatValue;
        }

        public virtual Vector2 InterpolateVector2(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.Vector2Value;
        }

        public virtual Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.Vector3Value;
        }

        public virtual Vector4 InterpolateVector4(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.Vector4Value;
        }

        public virtual Color InterpolateColor(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.ColorValue;
        }

        public virtual string InterpolateString(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.StringValue;
        }

        public virtual Component InterpolateComponent(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.ComponentValue;
        }

        public virtual GameObject InterpolateGameObject(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.GameObjectValue;
        }

        public virtual UnityEngine.Object InterpolateObject(TimeflowChannel channel, float time, bool apply)
        {
            return channel.ToProperty.ObjectValue;
        }

        #endregion

        /// <summary>
        /// Override this method in custom behavior classes to implement copying channels and any other
        /// necessary data.
        /// </summary>
        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, includeChannels);

            /// Clear the channel references and create new default ones. No channels are copied here since
            /// it is each behavior's responsibility to store its own channels.
            Channels = null;
            SetupChannels(true);
        }

    }

}//AxonGenesis
