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

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif


namespace AxonGenesis
{
    /// <summary>
    /// A TimeflowObject defines a track with a collection of channels and behaviors it manages. All update
    /// calls pass through the object to the child channels and behaviors.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/timeflow-object")]
    public partial class TimeflowObject : TimeflowBehavior, IHasPropertyChannels, IBehaviorPresets
    {
        #region STATIC

        public static List<TimeflowObject> AllInstances = null;

        public static List<TimeflowObject> GetAllInstances(bool rebuild = true)
        {
            if (!rebuild && AllInstances != null) return AllInstances;
#if UNITY_EDITOR
            AllInstances = new List<TimeflowObject>(UnityEngine.Object.FindObjectsByType(typeof(TimeflowObject), FindObjectsInactive.Include, FindObjectsSortMode.None) as TimeflowObject[]);

            Stage stage = StageUtility.GetCurrentStage();
            TimeflowObject[] prefabs = stage.FindComponentsOfType<TimeflowObject>();
            foreach (TimeflowObject prefab in prefabs) {
                if (!AllInstances.Contains(prefab)) {
                    AllInstances.Add(prefab);
                }
            }
            return AllInstances;
#else
            AllInstances = new List<TimeflowObject>(UnityEngine.Object.FindObjectsByType(typeof(TimeflowObject), FindObjectsInactive.Include, FindObjectsSortMode.None) as TimeflowObject[]);
            return AllInstances;
#endif
        }

        public static void SetupInstances(List<TimeflowObject> instances)
        {
            if (instances != null) {
                foreach (TimeflowObject obj in instances) {
                    //Debug.Log($"{obj.name}.CheckParent");
                    obj.CheckParent(true);
                }
                foreach (TimeflowObject obj in instances) {
                    obj.GetBehaviors(true);
                }
                foreach (TimeflowObject obj in instances) {
                    obj.AfterSetup();
                }
            }
        }

        public static void RegisterBehavior(TimeflowBehavior obj)
        {
            // Behaviors can be on the same GameObject as TimeflowObject
            obj.CheckParent(true);

            if (obj.ParentObject != null) {
                obj.ParentObject.ReloadBehaviors = true;

                if (!Application.isPlaying && (obj.ParentObject.Timeflow == null || !obj.ParentObject.Timeflow.IsPlaying)) {
                    // Force the parent to register new behaviors added
                    obj.ParentObject.GetBehaviors(false);
                }
            }
        }

        public static void UnregisterBehavior(TimeflowBehavior behavior)
        {
            if (behavior != null && behavior.ParentObject != null) {
                if (behavior.ParentObject.Behaviors != null) {
                    behavior.ParentObject.Behaviors.Remove(behavior);
                    behavior.ParentObject.HasBehaviors = behavior.ParentObject.Behaviors.Count > 0;
                }
                else {
                    behavior.ParentObject.HasBehaviors = false;
                }
                behavior.ParentObject = null;
            }
        }

        #endregion

        #region PUBLIC

        public TimeflowTrack Track;

        [FormerlySerializedAs("UseTimeflowOverride")]
        public bool OverrideTimeflowParent = false;

        [SerializeField] private Timeflow _TimeflowParentOverride = null;

        public bool IsOrphaned => ParentObject == null;

        public bool UpdateAnimation = true;
        public float AnimationSpeed = 1f;
        public float AnimationOffset;
        public float AnimationRand;

        #endregion

        #region PUBLIC NONSERIALIZED

        [NonSerialized]
        public bool IsGroup = false;

        [NonSerialized]
        public TimeflowGroup Group;

        [NonSerialized]
        public List<TimeflowEvent> Events;

        [NonSerialized]
        public TimeflowObject ParentContainer;

        [NonSerialized]
        public bool HasParentContainer = false;

        [NonSerialized]
        public Keyframer Keyframer;

        [NonSerialized]
        public List<TimeflowBehavior> Behaviors;

        [NonSerialized]
        public bool HasBehaviors = false;

        [NonSerialized]
        public bool ReloadBehaviors = true;

        [NonSerialized]
        public List<TimeflowObject> Children;

        [NonSerialized]
        private List<TimeflowChannel> _AllChannels;

        public List<TimeflowChannel> AllChannels {
            get { return _AllChannels; }
            set {
                if (_AllChannels != value) {
                    _AllChannels = value;
                }
            }
        }

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private int _SortOrder;

        protected Timeflow _TimeflowParent = null;

        #endregion

        #region PRIVATE

        protected Animator _animatorComp;
        protected Animation _animationComp;
        protected bool hasAnimator;
        protected bool hasAnimation;

        private float _prevAnimTime;
        private float _animationRand;
        private bool _checkedBehaviors;

        #endregion

        #region ACCESSORS

        public override Timeflow Timeflow {
            get {
                if (OverrideTimeflowParent && TimeflowParentOverride != null) {
                    return TimeflowParentOverride;
                }
                if (ParentObject != null) {
                    if (ParentObject is Timeflow tf) {
                        return tf;
                    }
                    return ParentObject.Timeflow;
                }
                return base.Timeflow;
            }
            set {
                base.Timeflow = value;
            }
        }
        public Timeflow TimeflowParentOverride {
            get { return _TimeflowParentOverride; }
            set {
                if (_TimeflowParentOverride != value) {
                    _TimeflowParentOverride = value;
                    if (_TimeflowParentOverride.TimeflowParent != null) {
                        TimeflowParentOverride = _TimeflowParentOverride.TimeflowParent;
                    }
                }
            }
        }

        public virtual Timeflow TimeflowParent {
            get {
                if (ParentObject != null) return ParentObject.TimeflowParent;
                if (OverrideTimeflowParent) return TimeflowParentOverride;
                return _TimeflowParent;
            }
            set {
                if (_TimeflowParent != value) {
                    if (value == null) {
                        _TimeflowParent = null;
                    }
                    else
                    if (ObjectUtil.IsDescendant(value.gameObject, gameObject)) {
                        if (!_shownHierarchyWarning) {
                            _shownHierarchyWarning = true;
                            Debug.LogWarning($"'{value.name}' is a descendant of '{name}' and cannot be set as its parented. Please check your object " +
                                $"hierarchy to ensure that each TimeflowObject has a Timeflow parent that is not a child or descendant of this game object.", gameObject);
                        }
                        _TimeflowParent = null;
                    }
                    else {
                        _TimeflowParent = value;
                    }
                    base.Timeflow = _TimeflowParent;
                }
            }
        }

        public int SortOrder {
            get {
                return _SortOrder;
            }
            set {
                if (_SortOrder != value) {
                    _SortOrder = value;
                }
            }
        }

        public virtual float StartTime {
            get {
                if (Track != null) {
                    return Track.StartTime;
                }
                else {
                    return Timeflow.StartTime;
                }
            }
            set {
                if (Track != null) {
                    Track.StartTime = value;
                }
            }
        }

        public override float EndTime {
            get {
                if (Track != null) {
                    return Track.EndTime;
                }
                else {
                    return Timeflow.EndTime;
                }
            }
            set {
                if (Track != null) {
                    Track.EndTime = value;
                }
            }
        }

        public virtual float StartTimeWorld {
            get { return StartTime * TimeScale; }
            set { StartTime = value; }
        }

        public virtual float EndTimeWorld {
            get { return EndTime * TimeScale; }
            set { EndTime = value; }
        }

        public override float CurrentTime {
            get {
                return _CurrentTime * TimeScaleWorld;
            }
            set {
                base.CurrentTime = value;
                if (Children != null) {
                    foreach (TimeflowObject child in Children) {
                        if (child == null) continue;
                        child.CurrentTime = _CurrentTime - child.TimeOffset;
                    }
                }
                if (HasBehaviors) {
                    foreach (TimeflowBehavior behavior in Behaviors) {
                        if (behavior == null) continue;
                        behavior.CurrentTime = _CurrentTime - behavior.TimeOffset;
                    }
                }
            }
        }

        public override float TimeOffset {
            get {
                return base.TimeOffset;
            }
            set {
                if (_TimeOffset != value) {
                    _TimeOffset = value;
#if UNITY_EDITOR
                    OnTrackChange();
#endif
                }
            }
        }

        #endregion

        #region HIERARCHY

        private bool hasParentWarning = false;

        public override void CheckParent(bool force)
        {
            if (ParentObject == this) ParentObject = null;
            TimeflowObject parentObject = ObjectUtil.GetComponentInParentOrAncestors<TimeflowObject>(gameObject);
            if (force || ParentObject == null) {
                ParentObject = parentObject;
                if (ParentObject != null) {
                    ParentObject.RegisterChild(this);
                }
            }
            if (force || Group == null) {
                Group = ObjectUtil.GetComponentInParentOrAncestors<TimeflowGroup>(gameObject);
                if (Group != null) {
                    Group.AddObject(this);
                }
            }

            if (IsOrphaned) {
                // Orphaned objects must be assigned to a root level Timeflow instance, not a precomp.
                while (Timeflow != null && Timeflow.TimeflowParent != null) {
                    Timeflow = Timeflow.TimeflowParent;
                }
                if (!Timeflow.IsPrefabMode && Timeflow == null) {
                    Timeflow = Timeflow.Active;
                }
            }
            if (Timeflow == null) {
                Timeflow t = ObjectUtil.GetComponentInParentOrAncestors<Timeflow>(gameObject);
                if (t != null) {
                    Timeflow = t;
                }
                else
                if (!Timeflow.IsPrefabMode) {
                    t = Timeflow.Active;
                    // Get the topmost Timeflow instance
                    while (t != null && t.TimeflowParent != null) {
                        t = t.TimeflowParent;
                    }
                }
            }

            if (IsOrphaned && this is Timeflow tf) {
                // This is a standalone intance of Timeflow
                if (!OverrideTimeflowParent) Timeflow = tf;
            }
            else {
                if (Timeflow != null && ParentObject == Timeflow) {
                    Timeflow.AddObject(this);
                }
                if (!Timeflow.IsPrefabMode && Timeflow == null) {
                    Timeflow = Timeflow.Active;
                }

                if (ParentObject == null && Timeflow == null) {
                    if (!hasParentWarning && !_shownHierarchyWarning) {
                        hasParentWarning = true;
                        Debug.LogWarning($"Could not find parent Timeflow. Please add Timeflow to the scene.", gameObject);
                    }
                }
            }

            // Get the next TimeflowObject up the hierarchy
            ParentContainer = null;
            Transform parent = transform.parent;
            while (parent != null && ParentContainer == null) {
                parent.TryGetComponent<TimeflowObject>(out ParentContainer);
                parent = parent.parent;
            }
            HasParentContainer = ParentContainer != null;

            GetChildren();
            GetEvents();
        }

        protected override void OnDebugEnabled()
        {
            base.OnDebugEnabled();
            if (AllChannels != null && AllChannels.Count > 0) {
                foreach (TimeflowChannel ch in AllChannels) {
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

        public virtual void Setup()
        {
            if (Track == null || Track.Keys.Count < 1) {
                ResetTrack();
            }
            else {
                Track.SetParent(this);
            }
            TrackActivated = (Track != null && Track.VisibilityMode == TimeflowTrack.VisibilityModes.Activate);

            if (_animationRand == 0f && AnimationRand > 0f) _animationRand = AnimationRand * (UnityEngine.Random.value * -0.5f) * 2f;

#if UNITY_EDITOR
            TrackColorPalette.UpdateTrackColor(this);

            _IsPrefabRoot = PrefabUtil.IsPrefabRootInstance(gameObject);
            _IsPrefab = _IsPrefabRoot || PrefabUtil.IsPrefabInstance(gameObject);
            if (gameObject.TryGetComponent<Timeflow>(out var timeflow)) {
                _IsTimeflowInstance = true;
            }
            else {
                _IsTimeflowInstance = false;
            }
#endif

            if (UpdateAnimation) {
                if (_animationComp != null) {
                    // Stop animators since they will be updated automatically
                    _animationComp.Stop();
                }
            }
        }

        public override void AfterSetup()
        {
            if (Behaviors != null && Behaviors.Count > 0) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.AfterSetup();
                }
            }
            if (AllChannels != null && AllChannels.Count > 0) {
                foreach (TimeflowChannel ch in AllChannels) {
                    ch.AfterSetup(this);
                }
            }
            base.AfterSetup();
        }

        /// <summary>
        /// This controls whether or not the children and the hierarchy below are displayed in the
        /// Timeflow. It is always a good idea for performance reasons to exclude children and hierarchies
        /// that do not have any TimeflowBehaviors.
        /// </summary>
        public bool ShowChildren {
            get {
#if UNITY_EDITOR
                return _ShowChildren;
#else
                return false;
#endif
            }
            set {
#if UNITY_EDITOR
                if (_ShowChildren != value) {
                    _ShowChildren = value;
                    OnShowChildren();
                }
#endif
            }
        }

#if UNITY_EDITOR
        public void OnShowChildren()
        {
            if (_ShowChildren) {
                IsCollapsed = false; // Open foldout when exapnding
            }
            Setup();
            DoUpdate();
            if (Timeflow.Active != null) Timeflow.Active.View.NeedsRefresh = true;
        }
#endif
        private void RegisterChild(TimeflowObject child)
        {
            if (child == this || child == null) return;
            if (Children == null) Children = new List<TimeflowObject>();
            if (!Children.Contains(child)) {

                if (!ObjectUtil.IsDescendant(child.gameObject, gameObject)) {
                    Debug.LogWarning($"Objects '{child.name}' is not a child nor descendant of '{name}'.", gameObject);
                    return;
                }
                else {
                    child.ParentObject = this;
                    Children.Add(child);
                }
            }
        }

        /// <summary>
        /// This gathers all the child objects of the current object, unless IncludeChildren is false. The
        /// TimeflowObject is automatically added to all children. Note that it is not necessary for this
        /// to be recursive since each TimeflowObject sets up its own children, so it is essentially
        /// recursive by default.
        /// </summary>
        public List<TimeflowObject> GetChildren(bool addTimeflowObject = false)
        {
            if (Children != null) {
                foreach (TimeflowObject child in Children) {
                    if (child.ParentObject == null) continue;
                    child.ParentObject = null;
                }
            }

            Children = null;
            if (transform.childCount > 0) {
                foreach (Transform child in transform) {
                    if (Children == null) Children = new List<TimeflowObject>();
                    GetChildrenRecursive(child, addTimeflowObject);
                }
            }

            if (Children != null && Children.Count > 0) {
                Children.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            }

            return Children;
        }

        private void GetChildrenRecursive(Transform child, bool addTimeflowObject)
        {
            if (child == null) return;
            TimeflowObject childObj = null;
            child.TryGetComponent<TimeflowObject>(out childObj);
#if UNITY_EDITOR
            if (childObj == null && addTimeflowObject) {
                childObj = Timeflow.SetupTimeflowObject(child.gameObject);
            }
#endif
            if (childObj == null) {
                foreach (Transform child2 in child) {
                    GetChildrenRecursive(child2, addTimeflowObject);
                }
            }
            else
            if (!Children.Contains(childObj)) {
                RegisterChild(childObj);
                childObj.GetChildren();
            }
        }

        /// <summary>
        /// Removes the TimeflowObject component from the children and entire hierarchy, though leaving
        /// intact the current TimeflowObject.
        /// </summary>
        public void RemoveChildren()
        {
            foreach (Transform child in transform) {
                RemoveChildrenRecursive(child);
            }
        }

        /// <summary>
        /// Removes the TimeflowObject component from the parent and entire hierarchy down.
        /// </summary>
        public void RemoveChildrenRecursive(Transform parent)
        {
            if (parent != null) {
                if (parent.TryGetComponent<TimeflowObject>(out var obj)) {
#if UNITY_EDITOR
                    UndoUtil.UndoDestroy(obj);
#else
                    DestroyImmediate(obj);
#endif
                }

                if (parent.transform.childCount > 0) {
                    foreach (Transform child in parent.transform) {
                        RemoveChildrenRecursive(child);
                    }
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sorts TimeflowObjects by their SortOrder, then renumber them to ensure numbers increment
        /// correctly.
        /// </summary>
        public static void SortObjects(ref List<TimeflowObject> list, bool updateHierarchy = false)
        {
            if (list != null) {
                list.Sort(new SortTimeflowObject());

                // Update the ordering numbers so they continue to be sorted correctly
                int i = 1;
                foreach (TimeflowObject obj in list) {
                    if (obj != null) {
                        if (obj.ParentObject != null) {
                            if (updateHierarchy && TimeflowPreferences.Current.AllowHierarchySorting) {
                                obj.gameObject.transform.SetSiblingIndex(i - 1);
                            }
                        }
                        obj.SortOrder = i * 100; // Increments of 100 to allow hierarchy changes more easily
                        i++;
                    }
                }
            }
        }
#endif
        #endregion

        #region BEHAVIORS

        /// <summary>
        /// This controls whether or not this object and any of its TimeflowBehaviors are processed in
        /// time. The user may opt to temporarily turn this off (via the toggle in the UI) to focus on
        /// specific objects.
        /// </summary>
        public bool BehaviorsEnabled {
            get {
                return Enabled;
            }
            set {
                if (Enabled != value) {
                    Enabled = value;
                    this.enabled = value;

                    if (HasBehaviors) {
                        foreach (TimeflowBehavior b in Behaviors) {
                            if (b != null) {
                                b.Enabled = value;
                                b.enabled = value;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds all the TimeflowBehaviors on the object and asks each to register its channels. The
        /// channels are then sorted in their layering order
        /// </summary>
        /// <returns></returns>
        [NonSerialized]
        private int behaviorCount = -1;

        public List<TimeflowBehavior> GetBehaviors(bool update = true)
        {
            _checkedBehaviors = true;
            ReloadBehaviors = false;

            if (Behaviors == null || behaviorCount != Behaviors.Count) update = true;
            if (!update) return Behaviors;

            if (DebugEnabled) Debug.Log($"{name}.{GetType().Name}.GetBehaviors:{update}");

            AllChannels = new List<TimeflowChannel>();
#if UNITY_EDITOR
            AllChannelsReverse = null; // force to rebuild list
#endif
            TryGetComponent<Keyframer>(out Keyframer);
            Behaviors = null;

            TimeflowBehavior[] behaviors = GetComponents<TimeflowBehavior>();
            if (behaviors != null && behaviors.Length > 0) {
                Behaviors = new List<TimeflowBehavior>();
                foreach (TimeflowBehavior b in behaviors) {
                    if (b == null) continue;
                    if (typeof(TimeflowObject).IsAssignableFrom(b.GetType())) continue;
                    // Only include TimeflowBehavior types that are not objects
                    if (b != this) {
                        b.CheckParent(true);
                        b.SetupChannels(true);
                        b.RegisterChannels(this);
                        b.AfterSetup();
                        if (update && b.CanUpdate && b.gameObject.activeInHierarchy) {
                            b.DoUpdate();
                        }
                        if (Behaviors != null && !Behaviors.Contains(b)) {
                            Behaviors.Add(b);
                        }
                    }
                }
            }
            HasBehaviors = Behaviors != null && Behaviors.Count > 0;

            RegisterChannels(this);
            SetupChannels(true);
            SortChannels();
            AfterSetup();

            return Behaviors;
        }

        #endregion

        #region CHANNELS

        public override bool HasChannel(TimeflowChannel channel)
        {
            if (AllChannels == null) return false;
            foreach (TimeflowChannel ch in AllChannels) {
                if (ch.UniqueID == channel.UniqueID) {
                    return true;
                }
            }
            return false;
        }

        public override bool HasChannel(string pathName)
        {
            if (AllChannels == null) return false;
            foreach (TimeflowChannel ch in AllChannels) {
                if (ch.PathName == pathName) {
                    return true;
                }
            }
            return false;
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            if (Track == null) return;
            if (Track.Behavior == null) Track.Behavior = this;
            RegisterChannel(Track);
        }

        /// <summary>
        /// Adds a TimeflowChannel to the AllChannels list so it will be included in processing.
        /// </summary>
        /// <param name="channel"></param>       
        public void RegisterChannel(TimeflowChannel channel)
        {
            if (channel == null) return;
            if (channel.Behavior == null) {
                Debug.LogWarning("Cannot register channel '" + channel.Name + "' without a parent behavior.");
                return;
            }
            //Debug.Log($"Registering channel '{channel.Name}' on {name} ({GetType().Name})", gameObject);
            if (channel.Object == null) {
                channel.Object = this;
            }
            if (AllChannels == null) AllChannels = new List<TimeflowChannel>();
            if (AllChannels.Contains(channel)) {
                if (DebugEnabled) Debug.LogWarning($"Channel already registered", gameObject);
            }
            else {
                if (!channel.IsRegistered) {
                    channel.IsRegistered = true;
                    channel.OnRegisterNewChannel();
                }
                if (!channel.IsUpdated) {
                    // The following is applied once only when the object is first created
                    channel.IsUpdated = true;
                    if (channel.IsTrack) {
                        channel.IsEnabled = true;
                    }
                    else
                    if (channel.ToProperty != null) {
                        if (channel.IsEnabled != channel.ToProperty.IsEnabled) {
                            channel.IsEnabled = channel.ToProperty.IsEnabled;
                        }
                    }
                }
#if UNITY_EDITOR
                AllChannelsReverse = null; // force to rebuild list
#endif
                AllChannels.Add(channel);
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) {
                Track.UpdateAutoFullLength();
                return;
            }
            areChannelsSetup = true;

            if (DebugEnabled) Debug.Log("TimeflowObject[" + name + "].SetupChannels");
            Channels = new List<TimeflowChannel>();
            if (Track == null || Track.Keys.Count == 0) {
                ResetTrack();
            }

#if UNITY_EDITOR
            if (Track.GUIColor.a == 0f) {
                if (TimeflowPreferences.Current.TrackColors.IsAutomaticForced) {
                    TrackColorPalette.UpdateTrackColor(this);
                }
                else {
                    Track.GUIColor = TimeflowPreferences.GetNextTrackColor();
                }
            }
#endif

            Track.SetParent(this);
            Track.OnSetup(this);
            Channels.Add(Track);
        }

        public override TimeflowChannel GetChannel(string name)
        {
            if (AllChannels == null || AllChannels.Count == 0) return null;
            TimeflowChannel channel = null;
            foreach (TimeflowChannel ch in AllChannels) {
                if (ch != null && ch.Name.Equals(name)) {
                    channel = ch;
                    break;
                }
            }
            return channel;
        }

        public override TimeflowChannel GetChannelByID(string id)
        {
            if (AllChannels == null || AllChannels.Count == 0) return null;
            TimeflowChannel channel = null;
            foreach (TimeflowChannel ch in AllChannels) {
                if (ch != null && ch.UniqueID.Equals(id)) {
                    channel = ch;
                    break;
                }
            }
            return channel;
        }

        /// <summary>
        /// Updates the order of channels based on the channels SortOrder. Users can reorder channels in
        /// the Timeflow hierarchy panel to control the order in which channels are displayed and
        /// processed.
        /// </summary>
        public override void SortChannels()
        {
            base.SortChannels();

            if (Track != null) {
                Track.Keys.Sort(KeyframeSort.ByTimeAsc);
            }
            if (AllChannels != null && AllChannels.Count > 0) {
                if (Behaviors != null) {
                    foreach (TimeflowBehavior b in Behaviors) {
                        if (b == null || b == this) continue;
                        b.SortChannels();
                    }
                }

                /// Use ascending order for correct processing sort order
                AllChannels.Sort(new SortTimeflowChannelAscending());

                /// Update the sort order of channels
                int i = 1;
                foreach (TimeflowChannel ch in AllChannels) {
                    ch.SortOrder = i * 100; // increments by 100 for easier reordering
                    i++;
                }

                if (HasBehaviors) {
                    foreach (TimeflowBehavior b in Behaviors) {
                        b.OnSortChannels();
                    }
                }
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();
            Track.CleanUp();
            Track.SetParent(this);
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b == this) continue;
                    b.CleanUp();
                }
            }
        }

        private int _circularIndex = 0;
        private int _thisCircularIndex = 0;

        protected void UpdateAutoFullLengthTracksRecursively()
        {
            _circularIndex++;
            _UpdateAutoFullLengthTracksRecursively(_circularIndex);
        }

        private void _UpdateAutoFullLengthTracksRecursively(int loopIndex)
        {
            if (_thisCircularIndex != loopIndex) {
                _thisCircularIndex = loopIndex;
            }
            else {
                //Debug.LogWarning($"Circular reference detected in {name}.UpdateAutoFullLengthTracksRecursively", gameObject);
                return;
            }
            if (this == null) {
                //Debug.LogError("UpdateAutoFullLengthTracksRecursively: this is null");
                return;
            }
            if (Track == null) return;
            Track.Object = this;
            Track.Behavior = this;
            Track.UpdateAutoFullLength();
            if (Children != null) {
                foreach (TimeflowObject child in Children) {
                    child._UpdateAutoFullLengthTracksRecursively(loopIndex);
                }
            }
        }

        public virtual float TrackStartTime {
            get {
                float time = TimeOffsetWorld;
                if (Track != null && Track.Keys != null) {
                    time += Track.Keys[0].KeyTime;
                }
                return time;
            }
        }

        public virtual float TrackDuration {
            get {
                float duration = 0f;
                if (Track != null && Track.Keys != null) {
                    duration = Track.Keys[0].KeyValue - Track.Keys[0].KeyTime;
                }
                else {
                    Debug.LogWarning(name + ".TrackDuration: Missing Track");
                }
                return duration;
            }
            set {
                if (Track != null && Track.Keys != null) {
                    Track.Keys[0].KeyValue = value + Track.Keys[0].KeyTime;
                }
            }
        }

        public virtual void ResetTrack()
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Reset Track");
            bool hasTrack = Track != null;
            Color color = hasTrack ? Track._GUIColor : TimeflowPreferences.GetNextTrackColor();
            if (color.a == 0) {
                color = TimeflowPreferences.GetNextTrackColor();
            }
#endif
            TimeOffset = 0f;

            if (Track == null) {
                Track = new TimeflowTrack(this);
#if UNITY_EDITOR
                // Make the track colors for Timeflow instances much lighter
                color = MathUtil.Interpolate(color, Color.white, 0.8f);
#endif
            }
            Track.Object = this;
            Track.Behavior = this;
            Track.Name = "Track";
            Track.SupportsKeyframes = true;
            Track.CanAddRemoveKeys = true;
#if UNITY_EDITOR
            Track.VisibilityMode = TimeflowPreferences.Current.DefaultTrackMode;
            Track.AutoFullLength = true;
            Track.GUIColor = color;
            if (Track.Keys.Count == 0) {
                Track.Keys = new List<Keyframe>();
                if (Timeflow == null) Timeflow = Timeflow.Active;
                Track.SetKey(0f, Timeflow == null ? 10f : Timeflow.Duration, true);
            }
            if (Track.Keys.Count > 0) {
                Track.Keys[0].LockTime = TimeflowPreferences.Current.DefaultTracksLocked;
                Track.Keys[0].LockValue = TimeflowPreferences.Current.DefaultTracksLocked;
            }
#endif
            Track.OnSetup(this);
        }

        public virtual bool IsVisible()
        {
            bool visible = false;
            if (Track != null && Track.VisibilityMode == TimeflowTrack.VisibilityModes.Activate) {
                visible = gameObject.activeSelf;
            }
            else {
                if (TryGetComponent<Renderer>(out Renderer r)) visible = r.enabled;
                else visible = gameObject.activeSelf;
            }
            return visible;
        }

        public virtual void OnUpdateAutoFullLength()
        {
            UpdateAutoFullLengthChildren();
        }

        protected virtual void UpdateAutoFullLengthChildren()
        {
            if (Children != null) {
                foreach (TimeflowObject child in Children) {
                    if (child == null || child.Track == null) continue;
                    child.Track.UpdateAutoFullLength();
                    child.UpdateAutoFullLengthChildren();
                }
            }
        }


        #endregion

        #region RUNTIME

        public override void OnRenderEnable()
        {
            base.OnRenderEnable();
            if (BehaviorsEnabled && HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.OnRenderEnable();
                }
            }
        }

        protected override void OnAwake()
        {
            TimeflowGroup.RegisterObject(this);

            TryGetComponent<Animator>(out _animatorComp);
            hasAnimator = _animationComp != null;

            TryGetComponent<Animation>(out _animationComp);
            hasAnimation = _animationComp != null;

            Setup();

#if UNITY_EDITOR
            OnAwakeEditor();
#endif

            base.OnAwake();
        }

        protected override void OnDestruct()
        {
            TimeflowGroup.UnregisterObject(this);
            base.OnDestruct();
        }

        public List<TimeflowEvent> GetEvents()
        {
            Events = new List<TimeflowEvent>();
            TimeflowEvent[] events = GetComponents<TimeflowEvent>();
            foreach (TimeflowEvent e in events) {
                Events.Add(e);
            }
            return Events;
        }

        public void RemoveEvent(TimeflowEvent evt)
        {
            if (evt == null || Events == null) return;
            if (Events.Contains(evt)) {
                Events.Remove(evt);
            }
        }

        public override void OnStartPlayback()
        {
            OnUpdateTimingMode();

            TrackActivated = (Track.VisibilityMode == TimeflowTrack.VisibilityModes.Activate);

            Setup();

            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.OnStartPlayback();
                }
            }
        }

        public override void OnPlay()
        {
            base.OnPlay();
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.OnPlay();
                }
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.OnStop();
                }
            }
        }

        public void OnQueue()
        {
            if (Track != null) Track.OnQueue();
        }

        public override float GetTime()
        {
            if (ParentObject == null) CheckParent(false);
            if (ParentObject != null) {
                return ParentObject.GetTime() * TimeScaleWorld - TimeOffset;
            }
            else {
                return 0f;
            }
        }

        public override void SetTime(float time)
        {
            base.SetTime(time);
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    b.SetTime(time - b.TimeOffset);
                }
            }
        }

        public bool HasAnimator {
            get { return hasAnimator; }
        }

        public bool HasAnimation {
            get { return hasAnimation; }
        }

        public void UpdateBehaviors()
        {
            if (!gameObject.activeInHierarchy && !TrackActivated) return;

            bool anyLinked = false;
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b.UseUpdate && !b.IsUpdateAfter) {
                        b.DoUpdate();
                        anyLinked |= b.HasLinkedBehaviors;
                    }
                }
            }
            UpdateTimeChannels(UpdateMethods.Update);

            if (anyLinked) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b.UseUpdate && b.HasLinkedBehaviors) {
                        b.UpdateTimeLinked();
                    }
                }
            }
        }

        public void LateUpdateBehaviors()
        {
            if (!gameObject.activeInHierarchy && !TrackActivated) return;

            bool anyLinked = false;
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b.UseLateUpdate && !b.IsUpdateAfter) {
                        b.DoUpdate();
                        anyLinked |= b.HasLinkedBehaviors;
                    }
                }
            }
            UpdateTimeChannels(UpdateMethods.LateUpdate);

            if (anyLinked) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b.UseLateUpdate && b.HasLinkedBehaviors) {
                        b.UpdateTimeLinked();
                    }
                }
            }
        }

        public void FixedUpdateBehaviors()
        {
            if (!gameObject.activeInHierarchy && !TrackActivated) return;

            bool anyLinked = false;
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b.UseFixedUpdate && !b.IsUpdateAfter) {
                        b.OnFixedUpdate();
                        b.DoUpdate();
                        anyLinked |= b.HasLinkedBehaviors;
                    }
                }
            }
            UpdateTimeChannels(UpdateMethods.FixedUpdate);

            if (anyLinked) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b.UseFixedUpdate && b.HasLinkedBehaviors) {
                        b.OnFixedUpdate();
                        b.UpdateTimeLinked();
                    }
                }
            }
        }

        /// <summary>
        /// TimeflowObject handles all channel updating (rather than having the behaviors do it) so that
        /// order can be better controlled. Each behavior is updated first before its channels.
        /// </summary>
        public void UpdateTimeChannels(UpdateMethods updateMethod)
        {
            if (AllChannels != null && CanUpdate) {
                for (int i = 0; i < AllChannels.Count; i++) {
                    TimeflowChannel ch = AllChannels[i];
                    bool canUpdate = ch.IsEnabled && ch.Behavior.UpdateMethod == updateMethod && ch.Behavior.CanUpdate;
                    if (canUpdate && (ch.Behavior.gameObject.activeInHierarchy || ch.Behavior.TrackActivated)) {
                        ch.Behavior.UpdateTimeChannel(ch);
                    }
                }
            }
        }

        /// <summary>
        /// This update method is only used when using UpdateAfter to create a specific order of execution.
        /// This overrides the base behavior so that AllChannels on the object are affected, whereas if the
        /// update setting is applied to the behavior directly, only its Channels are updated.
        /// </summary>
        public override void UpdateTimeChannelsExplicit()
        {
            if (AllChannels != null) {
                for (int i = 0; i < AllChannels.Count; i++) {
                    TimeflowChannel ch = AllChannels[i];
                    //ch != null && 
                    if (ch.IsEnabled) {
                        UpdateTimeChannel(ch);
                    }
                }
            }

        }

        /// <summary>
        /// This is the central processor for channels, executing them in order. All behaviors that use
        /// channels must implement UpdateTimeChannel to be processed in the correct order.
        /// </summary>
        public override void UpdateTime()
        {
            if (!CanUpdate) return;

            /// This not only checks that the track is on but also handles the visiblity mode
            float v = Track.InterpolateValue(CurrentTime, true, true);
            if (BehaviorsEnabled) {
                if (ReloadBehaviors || (!HasBehaviors && !_checkedBehaviors)) {
                    _checkedBehaviors = true;
                    GetBehaviors();
                }

                UpdateBehaviors();

                if (UpdateAnimation) {
                    if (CurrentTime != _prevAnimTime) {
                        if (HasAnimator) {
                            float delta = LocalDeltaTime;
                            if (delta > 0) {
                                _animatorComp.speed = AnimationSpeed;
                                if (!Application.isPlaying) {
                                    _animatorComp.Update(delta);
                                }
                            }
                        }
                        else
                        if (HasAnimation) {
                            float time = AnimationSpeed * (CurrentTime + _animationRand + AnimationOffset);
                            time = MathUtil.Loop(time, 0, _animationComp.clip.length);
                            _animationComp.clip.SampleAnimation(gameObject, time);
                        }
                        _prevAnimTime = CurrentTime;
                    }
                }
            }
            if (UseUpdate) UpdateTimeLinked();
            base.UpdateTime();
        }

        /// <summary>
        /// TimeflowObjects ignore their own UpdateMethod setting and always pass along LateUpdate calls to
        /// the behaviors, which then determine whether to update or not based on the behavior's
        /// UpdateMethod.
        /// </summary>
        public virtual void LateUpdateTime()
        {
            if (Enabled) {
                if (BehaviorsEnabled) {
                    if (ReloadBehaviors || (!HasBehaviors && !_checkedBehaviors)) {
                        _checkedBehaviors = true;
                        GetBehaviors();
                    }
                    LateUpdateBehaviors();
                }
                if (UseLateUpdate) UpdateTimeLinked();
            }
        }

        public override void OnFinalUpdate()
        {
            base.OnFinalUpdate();
            if (HasBehaviors) {
                foreach (TimeflowBehavior behavior in Behaviors) {
                    behavior.OnFinalUpdate();
                }
            }
        }

        public virtual void OffsetTime(float offset)
        {
            if (AllChannels != null) {
                foreach (TimeflowChannel ch in AllChannels) {
                    ch.OffsetTime(offset);
                }
            }
        }

        public override void OnRewind()
        {
            if (HasBehaviors) {
                foreach (TimeflowBehavior b in Behaviors) {
                    if (b != null && b.Enabled && !b.UseLateUpdate && (b.gameObject.activeInHierarchy || b.TrackActivated)) {
                        b.OnRewind();
                    }
                }
            }
            if (AllChannels != null) {
                foreach (TimeflowChannel ch in AllChannels) {
                    if (ch.IsEnabled) {
                        ch.OnRewind();
                    }
                }
            }
        }

        public IPropertyLinkable GetLinkableByID(string id)
        {
            IPropertyLinkable linked = null;
            if (AllChannels != null && AllChannels.Count > 0) {
                foreach (TimeflowChannel ch in AllChannels) {
                    if (id == ch.UniqueID) {
                        linked = ch;
                        break;
                    }
                }
            }
            return linked;
        }

        #endregion
    }

    #region SORTING CLASSES

    /// <summary>
    /// Utility sorting class used to sort TimeflowObjects in the sorting order specified for the user. The
    /// order has not affect on animation or performance and is purely for visual grouping of objects in
    /// the Timeflow view.
    /// </summary>
    public class SortTimeflowObject : IComparer<TimeflowObject>
    {
        public int Compare(TimeflowObject a, TimeflowObject b)
        {
            int c = 0;
            if (a == null) return 1;
            if (b == null) return -1;
            if (a.SortOrder < b.SortOrder) {
                c = -1;
            }
            else
            if (a.SortOrder > b.SortOrder) {
                c = 1;
            }
            return c;
        }
    }

#if UNITY_EDITOR
    public class SortTimeflowObjectByDisplay : IComparer<TimeflowObject>
    {
        public int Compare(TimeflowObject a, TimeflowObject b)
        {
            int c = 0;
            if (a == null) return 1;
            if (b == null) return -1;
            if (a.GUIRect.y < b.GUIRect.y) {
                c = -1;
            }
            else
            if (a.GUIRect.y > b.GUIRect.y) {
                c = 1;
            }

            return c;
        }
    }
#endif

    public class SortTimeflowObjectByTime : IComparer<TimeflowObject>
    {
        public int Compare(TimeflowObject a, TimeflowObject b)
        {
            int c = 0;
            if (a == null) return 1;
            if (b == null) return -1;
            if (a.TrackStartTime < b.TrackStartTime) {
                c = -1;
            }
            else
            if (a.TrackStartTime > b.TrackStartTime) {
                c = 1;
            }

            return c;
        }
    }

    public class SortTimeflowObjectByTimeDesc : IComparer<TimeflowObject>
    {
        public int Compare(TimeflowObject a, TimeflowObject b)
        {
            int c = 0;
            if (a == null) return 1;
            if (b == null) return -1;
            if (a.TrackStartTime > b.TrackStartTime) {
                c = -1;
            }
            else
            if (a.TrackStartTime < b.TrackStartTime) {
                c = 1;
            }

            return c;
        }
    }

    public class SortTimeflowObjectByName : IComparer<TimeflowObject>
    {
        public int Compare(TimeflowObject a, TimeflowObject b)
        {
            if (a == null) return 1;
            if (b == null) return -1;
            return a.name.CompareTo(b.name);
        }
    }

    #endregion

}//AxonGenesis
