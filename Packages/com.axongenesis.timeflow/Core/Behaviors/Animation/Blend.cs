// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Blend is a way of creating a collection of transform placements and/or property values with the
    /// ability to switch or interpolate between them.  One use case for Blend is to manage camera angles
    /// and then switch between them. This allows a single camera rig to be reparented and repositioned
    /// anywhere in the scene to create any type of camera movement. The set can then be choreographed in
    /// Timeflow Using the Blend component by essentially creating an edit sequence. Once the initial setup
    /// is done, camera edits are quick and easy, allowing greater creativity and a fast workflow.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Blend")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/blend")]
    sealed public class Blend : TimeflowBehavior
    {
        #region PUBLIC

        public enum GatherModes
        {
            None,
            LocalPositions,
            WorldPositions,
            LocalRotations,
            WorldRotations,
            LocalScales
        }

        /// <summary>
        /// Each BlendSet in Sets defines a specific placement for the target object. Only the properties
        /// enabled in Options are used.
        /// </summary>
        public List<BlendSet> Sets;

        /// <summary>
        /// A value from 0 to 1 defining the blend amount between From and To. Note that blend presently
        /// only works with like coordinates, so will ignore blends attempted between local and world
        /// coordinates.
        /// </summary>
        public float BlendAmount;

        /// <summary>
        /// The mode defines how the blend is interpolated, such as linear or ease in and out.
        /// </summary>
        public MathUtil.InterpolationModes BlendMode = MathUtil.InterpolationModes.EaseInOut;

        /// <summary>
        /// If set to true, Hold ensures that no blend takes place and remains fixed on the currently
        /// active set.
        /// </summary>
        public bool Hold = true;

        /// <summary>
        /// If set to true, the blend will swap the direction of To and From.
        /// </summary>
        public bool Reverse;

        /// <summary>
        /// Specifies the target transform to receive the XYZ position in either local or world
        /// coordinates.
        /// </summary>
        public Transform PositionTransform;

        /// <summary>
        /// This transform receives the XYZ rotation. This is decoupled from the PositionTransform to
        /// better work with complex rigs.
        /// </summary>
        public Transform RotationTransform;

        /// <summary>
        /// The transform receives the local scale.
        /// </summary>
        public Transform ScaleTransform;

        /// <summary>
        /// When enabled, position and rotation are applied using a Rigidbody (if present) instead of the Transform,
        /// for physics-friendly movement. Falls back to Transform when no Rigidbody exists.
        /// </summary>
        public bool UsePhysics;

        /// <summary>
        /// If reparenting is enabled, this specifies which transform in the scene will be moved.
        /// </summary>
        public Transform ReparentTransform;

        /// <summary>
        /// If reparenting is enabled, this specifies the default parent for world transforms.
        /// </summary>
        public Transform WorldParent;

        public List<Property> Properties;

        /// <summary>
        /// The camera to apply field of view to. Defaults to main camera if none assigned
        /// </summary>
        public Camera Camera;

        public List<BlendObjectActivate> ActivateObjects;

        /// <summary>
        /// </summary>
        public List<BlendUpdate> ObjectsToNotify;

        public bool EnableUpdate = true;

        public bool EnablePosition = true;
        public bool EnableRotation = true;
        public bool EnableQuaternions = false;
        public bool EnableScale;
        public bool EnableReparent;
        public bool EnableUpdateAfter;
        public bool EnableFieldOfView;
        public bool EnableActivateObjects;
        public bool EnableEvents;

        /// <summary>
        /// This should only be enabled if you need to force this Blend to update every frame. Otherwise,
        /// it will only update on a change of values. LiveUpdate is usually not needed and should be left
        /// off.
        /// </summary>
        //public bool LiveUpdate = false;

        /// <summary>
        /// At times during previewing and editing, it is helpful to override the current set being viewed
        /// and to activate another, regardless of any animation. Enabling Manual Override lets you
        /// activate any set directly.
        /// </summary>
        public bool ManualOverride;

        /// <summary>
        /// When ManualOverride is enabled, you can use DirectControl to set the position, rotation, and
        /// scale directly in world coordinates, which may be helpful when editing the scene or setting up
        /// a new set in world coordinates.
        /// </summary>
        public bool DirectControl;

        public bool ForceWorld;

        public float TransitionDuration = 1f;

        public Vector3 OverridePosition = Vector3.zero;
        public Vector3 OverrideRotation = Vector3.zero;
        public Vector3 OverrideScale = Vector3.one;
        public int OverrideFrom;
        public int OverrideTo;
        public float OverrideBlend;
        public MathUtil.InterpolationModes OverrideBlendMode = MathUtil.InterpolationModes.EaseInOut;
        public bool OverrideHold = true;
        public bool OverrideReverse;

        public bool EditKeyframes = true;

        public bool Categorize;

        /// <summary>
        /// This defines the current set and is the starting point of the interpolation, when Blend is at
        /// 0.
        /// </summary>
        [SerializeField]
        private int _From;

        /// <summary>
        /// This specifies the end set, when Blend is at 1.
        /// </summary>
        [SerializeField]
        private int _To;

        [SerializeField]
        private BlendChannel _Channel;

        [NonSerialized]
        public BlendKey CurrentKey = null;

        [NonSerialized]
        public bool IsEditing;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private Keyframe prevKey;

        [NonSerialized]
        private int lastActiveSet = -1;

        [NonSerialized]
        private List<string> _SetNames;

        [NonSerialized]
        private List<string> _SetToNames;

        [NonSerialized]
        private Rotator _TargetRotator;

        [NonSerialized]
        private bool isTransitioning;

        [NonSerialized]
        private float transitionDuration;

        [NonSerialized]
        private float transitionFromTime;

        [NonSerialized]
        private float transitionToTime;

        [NonSerialized]
        private int lastFrom = -1;

        [NonSerialized]
        private int lastTo = -1;

        [NonSerialized]
        private bool hasCamera = false;

        [NonSerialized]
        private bool hasPositionTransform = false;

        [NonSerialized]
        private bool hasRotationTransform = false;

        [NonSerialized]
        private bool hasScaleTransform = false;

        [NonSerialized]
        private bool hasReparentTransform = false;

        #endregion

        #region ACCESSORS

        public int From {
            get {
                return _From;
            }
            set {
                if (_From != value) {
                    lastFrom = _From;
                    _From = value;
                }
            }
        }

        public int To {
            get {
                return _To;
            }
            set {
                if (_To != value) {
                    lastTo = _To;
                    _To = value;
                }
            }
        }

        public bool HasAnimation {
            get {
                return Channel != null && Channel.IsEnabled && Channel.Keys != null && Channel.Keys.Count > 0;
            }
        }

        public List<string> SetNames {
            get {
                _SetNames = new List<string>();
                if (Sets != null && Sets.Count > 0) {
                    foreach (BlendSet n in Sets) {
                        if (string.IsNullOrEmpty(n.Name)) {
                            n.Name = "New Set";
                        }
                        string prefix = "";
                        if (Categorize) {
                            if (n.TransformType == BlendSet.TransformTypes.World) {
                                prefix = "World/";
                            }
                            else {
                                if (n.TransformType == BlendSet.TransformTypes.Parent) {
                                    string pname = n.Parent != null ? n.Parent.name : "NULL";
                                    prefix = "Parent/" + pname + "/";
                                }
                                else {
                                    prefix = "Local/";
                                }
                            }
                        }
                        _SetNames.Add(prefix + n.Name);
                    }
                }
                return _SetNames;
            }
        }

        public List<string> SetToNames {
            get {
                _SetToNames = new List<string>();
                if (Sets != null && Sets.Count > 0) {
                    if (From <= 0 || From >= Sets.Count) From = 0;

                    BlendSet from = Sets[From];
                    foreach (BlendSet n in Sets) {
                        if (string.IsNullOrEmpty(n.Name)) {
                            n.Name = "New Set";
                        }

                        string prefix = "";
                        if (Categorize) {
                            if (n.TransformType == BlendSet.TransformTypes.World) {
                                prefix = "World/";
                            }
                            else {
                                if (n.Parent != null) {
                                    prefix = "Parent/" + n.Parent.name + "/";
                                }
                                else {
                                    prefix = "Local/";
                                }
                            }
                        }
                        _SetToNames.Add(prefix + n.Name);
                    }
                }
                return _SetToNames;
            }
        }

        public Rotator TargetRotator {
            get {
                if (_TargetRotator == null && RotationTransform != null) {
                    _TargetRotator = Rotator.Setup(RotationTransform);
                }
                return _TargetRotator;
            }
        }

        #endregion

        #region SETUP

        protected override void OnEnable()
        {
            if (Enabled) {
                //if (DebugEnabled) Debug.Log(Name + ".Blend.OnEnable");
                Setup();
                ValidateSets();
                UpdateSet();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ".Blend.OnDestruct");
            // Ensures removal from the AllChannels list and base ChannelList
            base.RemoveChannel(_Channel);
            base.OnDestruct();
        }

        public BlendChannel Channel {
            get {
                if (_Channel == null) {
                    _Channel = new BlendChannel(this);
                    AddChannel(_Channel);
                    //if (DebugEnabled) Debug.Log(name + ".Channel Add");
                }
                return _Channel;
            }
            set {
                _Channel = value;
                if (_Channel != null) {
                    _Channel.Blend = this;
                    AddChannel(_Channel);
                    //if (DebugEnabled) Debug.Log(name + ".Channel Add");
                }
            }
        }

        //public override void OnTimeflowSetup()
        //{
        //    //if (DebugEnabled) Debug.Log(name + ".OnTimeflowSetup");
        //    base.OnTimeflowSetup();
        //    Setup();
        //}

        public override void Refresh()
        {
            base.Refresh();
            _SetNames = null;
            Setup();
            UpdateSet();
            UpdateTimeChannel(Channel);
        }

        public void Setup()
        {
            if (EnableFieldOfView && Camera == null) Camera = Camera.main;
            hasCamera = Camera != null;

            if (EnableReparent && ReparentTransform == null) ReparentTransform = transform;
            if (EnablePosition && PositionTransform == null) PositionTransform = transform;
            if (EnableRotation && RotationTransform == null) RotationTransform = transform;
            if (EnableScale && ScaleTransform == null) ScaleTransform = transform;

            _TargetRotator = null;
            hasPositionTransform = PositionTransform != null;
            hasRotationTransform = RotationTransform != null;
            hasScaleTransform = ScaleTransform != null;
            hasReparentTransform = ReparentTransform != null;

            if (Channel == null) SetupChannels(true);
            Channel.Blend = this;
            Channel.ShowValue = false;

            ValidateSets();

            if (Channel.Keys != null) {
                Keyframe lastKey = null;
                foreach (Keyframe key in Channel.Keys) {
                    if (key.IsKeyEnabled) {
                        if (lastKey != null && lastKey.CustomKey != null) {
                            BlendKey k = (BlendKey)lastKey.CustomKey;
                            if (k != null && k.AutoDuration) {
                                k.Duration = (key.KeyTime - lastKey.KeyTime) - k.StartTime;
                            }
                        }

                        lastKey = key;
                        SetupKey(key);
                    }
                }

                /// Use the linear flag to prevent Bezier handles from being displayed unless channel
                /// curve is in use. This does not mean that interpolation is actually linear however.
                bool showBezier = false;
                for (int i = 0; i < Channel.Keys.Count; i++) {
                    BlendKey k = (BlendKey)Channel.Keys[i].CustomKey;
                    bool isBezier = k.InterpolationMode == MathUtil.InterpolationModes.UseChannelCurve;
                    Channel.Keys[i].Linear = !showBezier && !isBezier;

                    showBezier = isBezier;
                }
            }

            // If a Rotator exists, pass through the physics option so it can respect it when used
            if (TargetRotator != null) {
                TargetRotator.UsePhysics = UsePhysics;
            }
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false); // base takes care of majority of properties
            //if (DebugEnabled) Debug.Log(name + ".Blend.Copy:" + src.name);

            _Channel = null;
            Properties = null;
            Sets = new List<BlendSet>();

            Blend srcBlend = src as Blend;
            if (srcBlend != null) {
                // Rebuild the sets to avoid any cross references to the original
                if (srcBlend.Sets != null && srcBlend.Sets.Count > 0) {
                    foreach (BlendSet set in srcBlend.Sets) {
                        if (set != null) {
                            BlendSet newSet = new BlendSet(set);
                            ValidateSet(newSet);
                            Sets.Add(newSet);
                        }
                    }
                }

                _Channel = (BlendChannel)CopyChannel((TimeflowChannel)srcBlend.Channel);
                _Channel.Blend = this;

                if (srcBlend.Properties != null && srcBlend.Properties.Count > 0) {
                    Properties = new List<Property>();
                    foreach (Property prop in srcBlend.Properties) {
                        Property newProp = new Property(this, prop);
                    }
                }

                SetupChannels(true);
            }
        }

        public void Gather(GatherModes mode)
        {
            List<Transform> transforms = new List<Transform>();
            ObjectUtil.GetChildrenRecursive(transform, ref transforms);
            if (transforms == null || transforms.Count == 0) {
                Debug.LogWarning("No child transforms found to gather");
                return;
            }

            int count = 0;
            if (Properties == null) Properties = new List<Property>();
            foreach (Transform t in transforms) {
                if (t == transform) continue;

                Property prop = null;
                if (mode == GatherModes.LocalPositions) {
                    prop = new Property(t, "Local Position");
                }
                else
                if (mode == GatherModes.WorldPositions) {
                    prop = new Property(t, "World Position");
                }
                else
                if (mode == GatherModes.LocalRotations) {
                    prop = new Property(t, "Local Rotation");
                }
                else
                if (mode == GatherModes.WorldRotations) {
                    prop = new Property(t, "World Rotation");
                }
                else
                if (mode == GatherModes.LocalScales) {
                    prop = new Property(t, "Local Scale");
                }
                if (Properties.Count > 0) {
                    bool exists = false;
                    foreach (Property p in Properties) {
                        if (p.GameObject == prop.GameObject && p.Name == prop.Name) {
                            exists = true;
                            break;
                        }
                    }
                    if (exists) prop = null;
                }
                if (prop != null) {
                    count++;
                    Properties.Add(prop);
                }
            }

            if (count == 0) {
                Debug.LogWarning("No new properties found to gather");
            }
            else {
                Debug.Log("Gathered " + count + " new properties");
            }
        }

        #endregion

        #region CHANNELS

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (Channel == null) {
                Channel = new BlendChannel(this);
                Channel.Interpolation = TimeflowChannel.Interpolations.None;
            }
            Channel.SetParent(this);
            Channel.OnSetup(this);
            Channel.IsNameCustom = true;
            Channel.Name = "Blend";
            Channel.LimitValue = false;
            Channel.SupportsKeyframes = true;
            Channel.CanAddRemoveKeys = true;
#if UNITY_EDITOR
            //Channel.GUIHeight = 40;
            Channel.ShowGameObject = false;
            Channel.ShowValue = false;
            Channel.ShowVector = false;
#endif
            Channels = new List<TimeflowChannel>();
            Channels.Add(Channel);

        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            obj.RegisterChannel(Channel);
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ".RemoveChannel");
            base.RemoveChannel(channel);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ".RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);

            // Assume the component should also be removed
#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#else
            UnityEngine.Object.DestroyImmediate(this);
#endif
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(name + ".CopyChannel");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
#endif
            BlendChannel copy = new BlendChannel(this);
            copy.Blend = this;
            copy.Copy(src);
            copy.SetParent(this);
            Channel = copy;

            AddChannel(Channel);
            return copy;
        }

        public override void OnUpdateTimingMode()
        {
            Setup();
        }

        public BlendKey SetupKey(Keyframe key)
        {
            key.IsCustomType = true;

            BlendKey k = key.CustomKey == null ? null : (BlendKey)key.CustomKey;
            if (k == null) {
                k = new BlendKey();
                k.FromSet = GetID(From);
                k.ToSet = GetID(To);

                key.CustomKey = k;
            }
            k.Key = key;
            k.Blend = this;
            k.Key.Hold = k.Hold;
            if (k.Hold) {
                k.Name = GetSetName(k.FromSet);
            }
            else {
                k.Name = GetSetName(k.FromSet) + "->" + GetSetName(k.ToSet);
            }
            return k;
        }

        #endregion

        #region SETS

        public void AddSet()
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Add Set", true);
            EditorTab = 0;
#endif
            BlendSet set = new BlendSet();
            set.ID = GetNewIndex();
            ValidateSet(set);
            Sets.Add(set);

            // Show the new set with manual override enabled
            StartEdit(Sets.Count - 1);
            Refresh();
        }

        public int GetNewIndex()
        {
            int index = 0;
            foreach (BlendSet set in Sets) {
                if (index < set.ID) {
                    index = set.ID;
                }
            }
            index++;
            return index;
        }

        public string GetSetName(int id)
        {
            int index = GetIndex(id);
            if (index < 0 || Sets == null || index >= Sets.Count) return null;
            return Sets[index].Name;
        }

        public int GetID(int index)
        {
            int id = -1;
            if (Sets != null && Sets.Count > 0) {
                return Sets[index].ID;
            }
            return id;
        }

        public int GetIndex(int id)
        {
            int index = -1;
            if (Sets != null && Sets.Count > 0) {
                for (int i = 0; i < Sets.Count; i++) {
                    if (Sets[i].ID == id) {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public int GetIndex(string name)
        {
            int index = -1;
            if (Sets != null && Sets.Count > 0) {
                int i = 0;
                foreach (BlendSet set in Sets) {
                    if (set.Name == name) {
                        index = i;
                        break;
                    }
                    i++;
                }
            }
            return index;
        }

        public void Capture(bool add = false, bool worldCoords = false)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Capture", true);
#endif

            if (ForceWorld) worldCoords = true;

            if (ManualOverride) {
                Hold = OverrideHold;
                From = OverrideFrom;
                To = OverrideTo;
            }

            if (Sets == null || Sets.Count == 0) {
                Sets = new List<BlendSet>();
                BlendSet set = new BlendSet();
                ValidateSet(set);
                Sets.Add(set);
                From = 0;
            }
            else
            if (add) {
                BlendSet newSet = null;
                if (From >= 0 && From < Sets.Count) {
                    newSet = new BlendSet(Sets[From]);
                }
                else {
                    newSet = new BlendSet();
                }
                ValidateSet(newSet);
                Sets.Add(newSet);
                From = Sets.Count - 1;
            }

            if (From < 0) From = 0;
            else
            if (From >= Sets.Count) From = Sets.Count - 1;

            if (To < 0) To = 0;
            else
            if (To >= Sets.Count) To = Sets.Count - 1;


            Transform sharedParent = null;
            if (!worldCoords) {
                if (!Hold) {
                    sharedParent = Sets[From].Parent;
                }
                else
                if (Sets[To].Parent == Sets[From].Parent) {
                    sharedParent = Sets[From].Parent;
                }
            }

            BlendSet n = Sets[this.From];
            if (n != null) {
                n.TransformType = worldCoords ? BlendSet.TransformTypes.World : sharedParent == null ? BlendSet.TransformTypes.Local : BlendSet.TransformTypes.Parent;
                if (worldCoords) {
                    if (hasPositionTransform) n.Position = PositionTransform.position;
                    if (hasRotationTransform) n.Rotation = RotationTransform.eulerAngles;
                    n.Parent = null;
                }
                else {
                    if (hasPositionTransform) n.Position = PositionTransform.localPosition;
                    if (hasRotationTransform) {
                        n.Rotation = RotationTransform.localEulerAngles;// TargetRotator.Euler;
                        //Debug.Log("localEulerAngles:" + n.Rotation);
                    }
                    n.Parent = sharedParent;
                }
                n.Scale = transform.localScale;
                n.UseTransform = false;
                n.Transform = null;
                n.Rotator = null;


#if UNITY_EDITOR

                EditorTab = 0; // Show Active

#endif

                if (Properties != null && Properties.Count > 0) {
                    if (n.Values == null) n.Values = new List<PropertyValue>();
                    for (int x = 0; x < Properties.Count; x++) {
                        if (x < n.Values.Count) {
                            n.Values[x] = new PropertyValue(Properties[x]);
                        }
                        else {
                            n.Values.Add(new PropertyValue(Properties[x]));
                        }
                    }
                }

                //if (DebugEnabled) Debug.Log(Name + ".Blend.Capture:" + n.Position + " r:" + n.Rotation + " s:" + n.Scale);
            }

            StartEdit(From);

            Refresh();
        }

        public void SetActive(int set)
        {
            if (ManualOverride) {
                OverrideFrom = set;
            }
            else {
                From = set;
            }

            UpdateSet();
        }

        /// <summary>
        /// Converts the coordinates of a set to a transform and assigns it to the set.
        /// </summary>
        /// <param name="set"></param>
        public void ConvertToTransform(int set)
        {
            if (Sets == null || Sets.Count == 0 || set < 0 || set >= Sets.Count) return;
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Create Transform", true);
#endif

            BlendSet n = Sets[set];
            if (n != null) {
                GameObject obj = new GameObject(n.Name);
                obj.transform.parent = WorldParent;

                if (n.TransformType == BlendSet.TransformTypes.Parent && n.Parent != null) {
                    obj.transform.parent = n.Parent;
                    if (n.ApplyPosition) obj.transform.localPosition = n.GetPosition(true);
                    if (n.ApplyRotation) {
                        Rotator.SetValue(obj, n.GetRotation(true));
                    }
                    if (n.ApplyScale) obj.transform.localScale = MathUtil.Multiply(n.Parent.localScale, n.Scale);
                }
                else {
                    if (n.ApplyPosition) obj.transform.localPosition = n.GetPosition(true);
                    if (n.ApplyRotation) {
                        Rotator.SetValue(obj, n.GetRotation(true));
                    }
                    if (n.ApplyScale) obj.transform.localScale = n.Scale;
                }

                n.TransformType = BlendSet.TransformTypes.World;
                n.UseTransform = true;
                n.Transform = obj.transform;
                n.Rotator = Rotator.Setup(n.Transform);

                n.Position = Vector3.zero;
                n.Rotation = Vector3.zero;
                n.Scale = Vector3.one;

#if UNITY_EDITOR
                UndoUtil.UndoCreate(obj, "Create Transform");
#endif
            }
        }

        /// <summary>
        /// Converts a set to world coordinates without using a transform.
        /// </summary>
        public void ConvertToWorld(int set)
        {
            if (Sets == null || Sets.Count == 0 || set < 0 || set >= Sets.Count) return;
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Convert to World", true);
#endif

            BlendSet n = Sets[set];
            if (n != null) {
                n.TransformType = BlendSet.TransformTypes.World;
                n.UseTransform = false;
                n.Transform = null;
                n.Rotator = null;

                if (n.ApplyPosition) n.Position = n.GetPosition(true);
                if (n.ApplyRotation) n.Rotation = n.GetRotation(true);
                /// No need to apply scale since it is always local
            }
        }

        /// <summary>
        /// Check that the BlendSet Activates array matches the ActivateObjects array and perform
        /// operations to match them if needed. Only returns true if the arrays match.
        /// </summary>
        public bool ValidateSet(BlendSet set)
        {
            bool isValid = true;
            if (set == null) return false;
            if (ActivateObjects != null && ActivateObjects.Count > 0) {
                if (set.Activates == null) {
                    isValid = false;
                    set.Activates = new List<bool>();
                    foreach (BlendObjectActivate obj in ActivateObjects) {
                        set.Activates.Add(obj.Default);
                    }
                }
                else
                if (set.Activates.Count < ActivateObjects.Count) {
                    isValid = false;
                    for (int i = set.Activates.Count; i < ActivateObjects.Count; i++) {
                        set.Activates.Insert(i, ActivateObjects[i].Default);
                    }
                }
                else
                if (set.Activates.Count > ActivateObjects.Count) {
                    isValid = false;
                    for (int i = set.Activates.Count; i >= ActivateObjects.Count; i--) {
                        set.Activates.RemoveAt(i);
                    }
                }
            }
            else {
                isValid = false;
                set.Activates = null;
            }

            return isValid;
        }

        /// <summary>
        /// Ensures that all sets have value lists to match the Properties list.
        /// </summary>
        public void ValidateSets()
        {
            if (Sets != null && Sets.Count > 0) {
                for (int x = 0; x < Sets.Count; x++) {
                    if (Sets[x] == null) {
                        Sets[x] = new BlendSet();
                    }
                    ValidateSet(Sets[x]);
                    if (Sets[x].Values == null) Sets[x].Values = new List<PropertyValue>();

                    /// Set the initial ID based on the index value
                    if (Sets[x].ID == -1) Sets[x].ID = x;

                    if (Properties != null && Properties.Count > 0f) {
                        int i = 0;
                        foreach (Property prop in Properties) {
                            if (i >= Sets[x].Values.Count) {
                                Sets[x].Values.Add(new PropertyValue(prop));
                            }
                            if (Sets[x].Values[i] == null) {
                                Sets[x].Values[i] = new PropertyValue(prop);
                            }
                            i++;
                        }
                    }
                }
            }
        }

        public void RandomSet()
        {
            From = Mathf.FloorToInt(UnityEngine.Random.value * (float)Sets.Count);
            UpdateSet();
        }

        public void StartEdit(int x)
        {
#if UNITY_EDITOR
            EditorTab = 0;
            Timeflow.Active.View.SelectChannel(Channel);
            SelectionUtil.Select(gameObject);

#endif
            IsEditing = true;
            ManualOverride = true;
            OverrideFrom = x;
            OverrideHold = true;
            UpdateSet();
        }

        public void StopEdit()
        {
            IsEditing = false;
            ManualOverride = false;
            UpdateSet();
        }

        public void SwapSets()
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Swap Blend Sets");
#endif
            if (ManualOverride) {
                int x = OverrideFrom;
                OverrideFrom = OverrideTo;
                OverrideTo = x;
            }
            else {
                int x = From;
                From = To;
                To = x;
            }
            UpdateSet();
        }

        #endregion

        #region UPDATE

        public override void UpdateTime()
        {
            base.UpdateTime();
            if (Sets == null || Sets.Count == 0 || !Enabled) return;

            if (isTransitioning) {
                if (CurrentTime <= transitionToTime && transitionDuration > 0f) {
                    BlendAmount = (CurrentTime - transitionFromTime) / transitionDuration;
                }
                else {
                    isTransitioning = false;
                    BlendAmount = 1f;
                }
                OverrideBlend = BlendAmount;
            }
            UpdateSet();
        }

        private void UpdateFromTo()
        {
            if (OverrideFrom >= Sets.Count) OverrideFrom = Sets.Count - 1;
            if (OverrideTo >= Sets.Count) OverrideTo = Sets.Count - 1;

            if (ManualOverride) {
                From = OverrideFrom;
                To = OverrideTo;
                Hold = OverrideHold;
                if (!isTransitioning) {
                    BlendAmount = OverrideBlend;
                }
                BlendMode = OverrideBlendMode;
            }
            else {
                OverrideFrom = From;
                OverrideTo = To;
                OverrideHold = Hold;
                OverrideBlend = BlendAmount;
                OverrideBlendMode = BlendMode;
            }
            if (Hold || To < 0 || To >= Sets.Count) To = From;
        }

        // Helpers to apply position/rotation taking physics into account
        private Rigidbody GetRigidbody(Transform t)
        {
            if (!UsePhysics || t == null) return null;
            Rigidbody rb;
            return t.TryGetComponent<Rigidbody>(out rb) ? rb : null;
        }

        private void ApplyPosition(Vector3 value, bool isWorld)
        {
            if (!hasPositionTransform || PositionTransform == null) return;
            var rb = GetRigidbody(PositionTransform);
            if (rb != null) {
                Vector3 world = isWorld ? value : (PositionTransform.parent != null ? PositionTransform.parent.TransformPoint(value) : value);
#if UNITY_EDITOR
                if (!Application.isPlaying) rb.position = world; else rb.MovePosition(world);
#else
                rb.MovePosition(world);
#endif
            }
            else {
                if (isWorld) PositionTransform.position = value; else PositionTransform.localPosition = value;
            }
        }

        private void ApplyRotation(Vector3 euler, Quaternion? quatOpt, bool isWorld)
        {
            if (!hasRotationTransform || RotationTransform == null) return;
            var rb = GetRigidbody(RotationTransform);
            if (rb != null) {
                Quaternion q = quatOpt.HasValue ? quatOpt.Value : Quaternion.Euler(euler);
                Quaternion worldQ = isWorld ? q : (RotationTransform.parent != null ? RotationTransform.parent.rotation * q : q);
#if UNITY_EDITOR
                if (!Application.isPlaying) rb.rotation = worldQ; else rb.MoveRotation(worldQ);
#else
                rb.MoveRotation(worldQ);
#endif
            }
            else {
                var rot = TargetRotator;
                if (rot != null) {
                    rot.IsWorldSpace = isWorld;
                    rot.UsePhysics = UsePhysics;
                    if (quatOpt.HasValue && EnableQuaternions) {
                        rot.Rotation = quatOpt.Value;
                    }
                    else {
                        rot.Euler = euler;
                    }
                }
            }
        }

        public void UpdateSet()
        {
            if (!EnableUpdate) return;
            if (Sets == null || Sets.Count == 0 || !Enabled) return;

            UpdateFromTo();

            if (From >= 0 && From < Sets.Count) {
                if (ManualOverride && DirectControl) {
                    if (EnablePosition && hasPositionTransform) {
                        // Direct control values are in world space per documentation
                        ApplyPosition(OverridePosition, true);
                    }
                    if (EnableRotation && hasRotationTransform) {
                        ApplyRotation(OverrideRotation, null, true);
                    }
                    if (EnableScale && hasScaleTransform) {
                        ScaleTransform.localScale = OverrideScale;
                    }
                }
                else {
                    BlendSet fromNode = Sets[From];
                    if (fromNode == null) {
                        Debug.LogWarning(base.Name + ".Blend.UpdateSet:" + this.From + " NULL");
                    }
                    else {
                        bool hold = To == From || ManualOverride ? OverrideHold : Hold;
                        BlendSet toNode = hold ? null : Sets[To];

                        // Determine whether to calculate in local or world space
                        bool isWorld = ForceWorld || fromNode.IsWorld || fromNode.TransformType == BlendSet.TransformTypes.World;
                        if (!hold) {
                            isWorld = isWorld || toNode.IsWorld;
                            if (!isWorld && EnableReparent) {
                                // Check to see if the blend between the two sets is in different spaces
                                if (fromNode.SetParent && toNode.SetParent && fromNode.Parent != toNode.Parent) {
                                    isWorld = true;
                                }
                            }
                        }

                        float blend = BlendAmount;// ManualOverride ? OverrideBlend : BlendAmount;
                        bool reverse = toNode != null && ManualOverride ? OverrideReverse : Reverse;

                        bool doBlend = !hold && From != To && toNode != null;

                        MathUtil.InterpolationModes mode = ManualOverride ? OverrideBlendMode : BlendMode;

                        if (EnableActivateObjects) {
                            if (ActivateObjects != null && ActivateObjects.Count > 0) {
                                int i = 0;
                                ValidateSet(fromNode);
                                ValidateSet(toNode);
                                foreach (BlendObjectActivate obj in ActivateObjects) {
                                    if (obj != null && obj.Object != null) {
                                        bool activate = false;
                                        if (obj.Transition == BlendObjectActivate.Transitions.EitherOn) {
                                            activate = fromNode.Activates[i] || (toNode != null && toNode.Activates[i]);
                                        }
                                        else
                                        if (obj.Transition == BlendObjectActivate.Transitions.EitherOff) {
                                            activate = !(!fromNode.Activates[i] || (toNode != null && !toNode.Activates[i]));
                                        }
                                        else
                                        if (obj.Transition == BlendObjectActivate.Transitions.Midpoint) {
                                            if (BlendAmount < obj.Midpoint || toNode == null) {
                                                activate = fromNode.Activates[i];
                                            }
                                            else {
                                                activate = toNode.Activates[i];
                                            }
                                        }
                                        else
                                        if (obj.Transition == BlendObjectActivate.Transitions.ActivateAtEnd) {
                                            if (BlendAmount < 1f || toNode == null) {
                                                activate = fromNode.Activates[i];
                                            }
                                            else {
                                                activate = toNode.Activates[i];
                                            }
                                        }
                                        else
                                        if (obj.Transition == BlendObjectActivate.Transitions.ActivateAtStart) {
                                            if (BlendAmount == 0f || toNode == null) {
                                                activate = fromNode.Activates[i];
                                            }
                                            else {
                                                activate = toNode.Activates[i];
                                            }
                                        }

                                        if (obj.Object.activeSelf != activate) {
                                            obj.Object.SetActive(activate);
                                        }
                                    }
                                    i++;
                                }
                            }
                        }

                        if (EnableReparent && hasReparentTransform) {
                            Transform parent = null;
                            if (!isWorld) {
                                parent = fromNode.Parent;
                                if (!hold && parent == null) {
                                    parent = toNode.Parent;
                                }
                            }

                            if (toNode != null && toNode.Parent != null) {
                                /// Only perform reparenting if changed
                                Timeflow.Reparent(ReparentTransform, toNode.Parent);
                            }
                            else {
                                // Use the default parent
                                if (parent == null) parent = WorldParent;
                                Timeflow.Reparent(ReparentTransform, parent);
                            }
                        }

                        if (EnableFieldOfView && hasCamera) {
                            float fromFOV = fromNode.SetFieldOfView ? fromNode.FieldOfView : Camera.fieldOfView;
                            if (doBlend && toNode != null && toNode.SetFieldOfView) {
                                if (reverse) {
                                    Camera.fieldOfView = MathUtil.InterpolateMode(toNode.FieldOfView, fromFOV, blend, mode);
                                }
                                else {
                                    Camera.fieldOfView = MathUtil.InterpolateMode(fromFOV, toNode.FieldOfView, blend, mode);
                                }
                            }
                            else {
                                if (reverse) {
                                    if (toNode != null && toNode.SetFieldOfView) {
                                        Camera.fieldOfView = toNode.FieldOfView;
                                    }
                                }
                                else {
                                    if (fromNode.SetFieldOfView) {
                                        Camera.fieldOfView = fromFOV;
                                    }
                                }
                            }
                        }

                        if (EnablePosition && hasPositionTransform) {
                            Vector3 fromPos = Vector3.zero;
                            Vector3 toPos = Vector3.zero;

                            if (fromNode.ApplyPosition) {
                                fromPos = fromNode.GetPosition(isWorld);
                            }
                            else {
                                /// Use the target objects current position
                                fromPos = isWorld ? PositionTransform.position : PositionTransform.localPosition;
                            }

                            bool toPosApply = false;
                            if (toNode != null) {
                                toPosApply = toNode.ApplyPosition;
                                if (toPosApply) {
                                    toPos = toNode.GetPosition(isWorld);
                                    if (isWorld) {
                                        if (toNode.IsLocal) {
                                            toPos += fromPos;
                                        }
                                        else
                                        if (fromNode.IsLocal) {
                                            fromPos += toPos;
                                        }
                                    }
                                }
                            }

                            bool posAssigned = false;
                            Vector3 finalPos = Vector3.zero;
                            if (doBlend && toPosApply) {
                                if (reverse) {
                                    if (isWorld) {
                                        finalPos = MathUtil.InterpolateMode(toPos, fromPos, blend, mode);
                                    }
                                    else {
                                        finalPos = MathUtil.InterpolateMode(toPos, fromPos, blend, mode);
                                    }
                                }
                                else {
                                    if (isWorld) {
                                        finalPos = MathUtil.InterpolateMode(fromPos, toPos, blend, mode);
                                    }
                                    else {
                                        finalPos = MathUtil.InterpolateMode(fromPos, toPos, blend, mode);
                                    }
                                }
                                posAssigned = true;
                            }
                            else if (fromNode.ApplyPosition) {
                                finalPos = fromPos;
                                posAssigned = true;
                            }
                            else if (toPosApply) {
                                finalPos = toPos;
                                posAssigned = true;
                            }

                            if (posAssigned) {
                                ApplyPosition(finalPos, isWorld);

                                // Update override position as local space for display
                                Vector3 world = isWorld ? finalPos : (PositionTransform.parent != null ? PositionTransform.parent.TransformPoint(finalPos) : finalPos);
                                OverridePosition = PositionTransform.parent != null ? PositionTransform.parent.InverseTransformPoint(world) : world;
                            }
                        }

                        if (EnableRotation && TargetRotator != null) {
                            Vector3 fromRot = Vector3.zero;
                            Vector3 toRot = Vector3.zero;

                            // This flag controls how we interpret from/to values
                            bool rotIsWorld = isWorld;

                            if (fromNode.ApplyRotation) {
                                fromRot = fromNode.GetRotation(rotIsWorld);
                            }
                            else {
                                /// Use the target objects current rotation
                                // We need current rotation in the same space we are calculating in
                                if (rotIsWorld) {
                                    fromRot = RotationTransform.rotation.eulerAngles;
                                }
                                else {
                                    fromRot = RotationTransform.localEulerAngles;
                                }
                            }

                            bool toRotApply = false;
                            if (toNode != null) {
                                toRotApply = toNode.ApplyRotation;
                                if (toRotApply) {
                                    toRot = toNode.GetRotation(rotIsWorld);
                                    if (rotIsWorld) {
                                        if (toNode.IsLocal) {
                                            toRot += fromRot;
                                        }
                                        else
                                        if (fromNode.IsLocal) {
                                            fromRot += toRot;
                                        }
                                    }
                                }
                            }

                            bool rotAssigned = false;
                            Vector3 finalEuler = Vector3.zero;
                            Quaternion? finalQuat = null;

                            if (doBlend && toRotApply) {
                                if (reverse) {
                                    if (EnableQuaternions) {
                                        finalQuat = MathUtil.InterpolateMode(Quaternion.Euler(toRot), Quaternion.Euler(fromRot), blend, mode);
                                    }
                                    else {
                                        finalEuler = MathUtil.InterpolateMode(toRot, fromRot, blend, mode);
                                    }
                                }
                                else {
                                    if (EnableQuaternions) {
                                        finalQuat = MathUtil.InterpolateMode(Quaternion.Euler(fromRot), Quaternion.Euler(toRot), blend, mode);
                                    }
                                    else {
                                        finalEuler = MathUtil.InterpolateMode(fromRot, toRot, blend, mode);
                                    }
                                }
                                rotAssigned = true;
                            }
                            else if (fromNode.ApplyRotation) {
                                if (EnableQuaternions) {
                                    finalQuat = Quaternion.Euler(fromRot);
                                }
                                else {
                                    finalEuler = fromRot;
                                }
                                rotAssigned = true;
                            }
                            else if (toRotApply) {
                                if (EnableQuaternions) {
                                    finalQuat = Quaternion.Euler(toRot);
                                }
                                else {
                                    finalEuler = toRot;
                                }
                                rotAssigned = true;
                            }

                            if (rotAssigned) {
                                ApplyRotation(finalEuler, finalQuat, rotIsWorld);

                                // Update override rotation equivalent (local space values for display consistency)
                                Quaternion worldQ = finalQuat.HasValue ? finalQuat.Value : Quaternion.Euler(finalEuler);
                                if (!rotIsWorld && RotationTransform.parent != null) {
                                    // if we computed in local, convert to world to then back to local for safety
                                    worldQ = RotationTransform.parent.rotation * worldQ;
                                }
                                Quaternion localQ = RotationTransform.parent != null ? Quaternion.Inverse(RotationTransform.parent.rotation) * worldQ : worldQ;
                                OverrideRotation = localQ.eulerAngles;
                            }
                        }

                        if (EnableScale && hasScaleTransform) {
                            Vector3 fromScale = Vector3.one;
                            Vector3 toScale = Vector3.one;

                            if (fromNode.ApplyScale) {
                                if (fromNode.UseTransform && fromNode.Transform != null) {
                                    fromScale = MathUtil.Multiply(fromNode.Transform.localScale, fromNode.Scale);

                                }
                                else {
                                    fromScale = fromNode.Scale;
                                }
                            }
                            else {
                                /// Use the target objects current scale
                                fromScale = ScaleTransform.localScale;
                            }

                            bool toScaleApply = false;
                            if (toNode != null) {
                                toScaleApply = toNode.ApplyScale;
                                if (toScaleApply) {
                                    if (toNode.UseTransform && toNode.Transform != null) {
                                        toScale = MathUtil.Multiply(toNode.Transform.localScale, toNode.Scale);

                                    }
                                    else {
                                        toScale = toNode.Scale;
                                    }
                                }
                            }

                            bool scaleAssigned = false;
                            Vector3 finalScale = Vector3.one;
                            if (doBlend && toScaleApply) {
                                if (reverse) {
                                    finalScale = MathUtil.InterpolateMode(toScale, fromScale, blend, mode);
                                }
                                else {
                                    finalScale = MathUtil.InterpolateMode(fromScale, toScale, blend, mode);
                                }
                                scaleAssigned = true;
                            }
                            else
                            if (fromNode.ApplyScale) {
                                finalScale = fromScale;
                                scaleAssigned = true;
                            }
                            else
                            if (toScaleApply) {
                                finalScale = toScale;
                                scaleAssigned = true;
                            }

                            if (scaleAssigned) {
                                // Rigidbody has no scale; always apply to Transform
                                ScaleTransform.localScale = finalScale;
                                OverrideScale = ScaleTransform.localScale;
                            }
                        }

                        if (Properties != null && Properties.Count > 0) {
                            int i = 0;
                            foreach (Property prop in Properties) {
                                if (prop != null) {
                                    if (doBlend) {
                                        if (reverse) {
                                            PropertyValue.Interpolate(prop, toNode.Values[i], fromNode.Values[i], blend, mode);
                                        }
                                        else {
                                            PropertyValue.Interpolate(prop, fromNode.Values[i], toNode.Values[i], blend, mode);
                                        }
                                    }
                                    else
                                    if (fromNode.Values[i].ApplyValue) {
                                        fromNode.Values[i].SetValue(prop);
                                    }
                                    else
                                    if (toNode != null && toNode.Values[i].ApplyValue) {
                                        toNode.Values[i].SetValue(prop);
                                    }
                                }
                                i++;
                            }
                        }

                        if (EnableEvents) {
                            if (lastTo > -1 && lastTo != lastFrom) {
                                /// If the previous blend was a hold then To == From and should skip this
                                BlendSet lastNode = Sets[lastTo];
                                if (lastNode != null && !lastNode.HasExited) {
                                    lastNode.Exit();
                                }
                            }
                            else
                            if (lastFrom > -1) {
                                /// Make sure the last active node is exited
                                BlendSet lastNode = Sets[lastFrom];
                                if (lastNode != null && !lastNode.HasExited) {
                                    lastNode.Exit();
                                }
                            }
                            if (hold || fromNode == toNode) {
                                /// Enter the current blend set. Exit will occur when new set is selected
                                if (!fromNode.HasEntered) {
                                    fromNode.Enter();
                                }
                            }
                            else {
                                /// When not holding, exit the From node and enter the To node
                                if (!fromNode.HasExited) {
                                    fromNode.Exit();
                                }
                                if (!toNode.HasEntered) {
                                    toNode.Enter();
                                }
                            }
                        }
                    }
                }
                if (lastActiveSet != From) {
                    lastActiveSet = From;
                    OnBlendChange();
                }
            }
        }

        public void OnBlend(float time, Keyframe keyA, Keyframe keyB, float blend)
        {
            if (ObjectsToNotify != null) {
                foreach (BlendUpdate obj in ObjectsToNotify) {
                    if (obj != null) {
                        obj.OnBlend(time, keyA, keyB, blend);
                    }
                }
            }
        }

        public void OnBlendChange()
        {
            if (ObjectsToNotify != null) {
                foreach (BlendUpdate obj in ObjectsToNotify) {
                    if (obj != null) {
                        obj.OnBlendChange();
                    }
                }
            }
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (Enabled) {
                if (Channel.IsEnabled) {
                    Channel.InterpolateValue(channel.CurrentTime, true, true);
                }
                else {
                    UpdateSet();
                }
            }
        }

        public float Interpolate(Keyframe keyA, Keyframe keyB, float time, float value, bool apply)
        {
            if (keyA == null && keyB == null) return 0f;

            float keyAtime = -1f;
            float keyBtime = -1f;

            if (keyA != null) {
                if (keyA.CustomKey == null) {
                    SetupKey(keyA);
                }
                keyAtime = keyA.KeyTime;
            }
            if (keyB != null) {
                if (keyB.CustomKey == null) {
                    SetupKey(keyB);
                }
                keyBtime = keyB.KeyTime;
            }

            BlendKey blendKeyA = null;
            BlendKey blendKeyB = null;

            if (keyA != null && keyB != null) {
                blendKeyA = keyA.CustomKey == null ? null : (BlendKey)keyA.CustomKey;
                if (blendKeyA != null) {
                    float t = time - keyA.KeyTime;
                    value = blendKeyA.InterpolateTime(t, value, apply);
                    //Debug.Log($"Interpolate:{t} time:{time} keyA.KeyTime:{keyA.KeyTime} value:{value}");

                    if (apply && keyA != prevKey) {
                        blendKeyA.PerformTrigger();
                        prevKey = keyA;
                    }
                }
            }
            else {
                if (keyA == null) {
                    blendKeyB = keyB.CustomKey == null ? null : (BlendKey)keyB.CustomKey;
                    if (blendKeyB != null) {
                        value = 0f;
                        if (apply) blendKeyB.ApplyBlend(0f);

                        if (apply && keyB != prevKey) {
                            blendKeyB.PerformTrigger();
                            prevKey = keyB;
                        }
                    }
                }
                else
                if (keyB == null) {
                    blendKeyA = keyA.CustomKey == null ? null : (BlendKey)keyA.CustomKey;
                    if (blendKeyA != null) {
                        float t = time - keyA.KeyTime;
                        value = blendKeyA.InterpolateTime(t, value, apply);

                        if (apply && keyA != prevKey) {
                            blendKeyA.PerformTrigger();
                            prevKey = keyA;
                        }
                    }
                }
            }

            if (apply) {
                OnBlend(time, keyA, keyB, value);
            }

            return value;
        }

        /// <summary>
        /// Use this method to refer to
        /// </summary>
        /// <param name="to"></param>
        public void TransitionToID(int to)
        {
            TransitionTo(GetIndex(to), TransitionDuration);
        }

        public void TransitionTo(int to)
        {
            TransitionTo(to, TransitionDuration);
        }

        public void TransitionTo(int to, float duration)
        {
            if (duration <= 0 || !Timeflow.IsPlaying) {
                if (ManualOverride) {
                    OverrideFrom = OverrideTo = to;
                    OverrideHold = true;
                    OverrideBlend = 1f;
                }
                else {
                    From = To = to;
                    Hold = true;
                    BlendAmount = 1f;
                }
            }
            else {
                isTransitioning = true;
                if (ManualOverride) {
                    OverrideFrom = To;
                    OverrideTo = to;
                    OverrideHold = false;
                    OverrideBlend = 0f;
                }
                else {
                    From = To;
                    To = to;
                    Hold = false;
                    BlendAmount = 0f;
                }
                transitionDuration = duration;
                transitionFromTime = CurrentTime;
                transitionToTime = transitionFromTime + duration;
            }
        }

        public void TransitionTo(string toName, float duration)
        {
            int index = GetIndex(toName);
            if (index > -1) TransitionTo(index, duration);
        }

        #endregion

#if UNITY_EDITOR

        public bool EditorShowKeys = true;
        public bool EditorShowSets = true;
        public bool EditorShowBlend = true;
        public bool EditorShowTransitions = true;
        public int EditorTab = 1;
        public int EditorSetsTab;
        public bool EditorColorCoded = true;

        public override Texture2D Icon => AxonUI.Icons.Blend;

        public override bool IsSelected {
            get {
                bool sel = false;
                if (Channel != null && Channel.IsSelected) {
                    sel = true;
                }
                return sel;
            }
        }

        public override void ResetName()
        {
            Channel.Name = Channel.ToProperty.DisplayName = "Blend";
        }

        public override void OnPropertyChanged(Property property, Property.PropertyTypes originalType, int originalAttribute)
        {
            if (Properties != null && Properties.Count > 0 && Sets != null && Sets.Count > 0) {
                int i = 0;
                foreach (Property p in Properties) {
                    if (p == property) {
                        foreach (BlendSet set in Sets) {
                            if (set.Values.Count <= i) {
                                set.Values.Add(new PropertyValue(p));
                            }
                            else {
                                set.Values[i] = new PropertyValue(p);
                            }
                        }
                    }
                    i++;
                }
            }
        }

        public void SortSets()
        {
            UndoUtil.Undo(this, "Sort Sets", true);
            Sets.Sort((a, b) => {
                if (a.Name == null) {
                    return -1;
                }
                if (b == null) {
                    return 1;
                }
                if (a.TransformType == b.TransformType) {
                    return a.Name.CompareTo(b.Name);
                }
                else {
                    if ((int)a.TransformType < (int)b.TransformType) {
                        return -1;
                    }
                    else {
                        return 1;
                    }
                }
            });
        }

        public void SortSetsByID()
        {
            UndoUtil.Undo(this, "Sort Sets by ID", true);
            Sets.Sort((a, b) => {
                if (a == null) {
                    return -1;
                }
                if (b == null) {
                    return 1;
                }
                return a.ID.CompareTo(b.ID);
            });
        }

        public void ReassignSetIDs()
        {
            if (Sets == null) return;
            UndoUtil.Undo(this, "Reassign Set IDs", true);
            for (int i = 0; i < Sets.Count; i++) {
                Sets[i].ID = i;
            }
        }


        #region TIMEFLOW GUI

        public override void GUIGraph(Rect rect)
        {
        }

        public override void GUIGraphFit(bool init, bool selectedOnly)
        {
        }

        public void GUIChannelValues()
        {
            float time = CurrentTime;
            float w = (Timeflow.Active.Layout.Values.Width - 10);
            bool showBool = w > 100;
            if (showBool) w -= 40f;

            EditorGUI.BeginChangeCheck();
            float labelWidth = AxonGUI.LabelWidth;
            AxonGUI.SetLabelWidth(5);


            Rect rect = new Rect(Channel.GUIRect);
            rect.y = rect.y + (rect.height * 0.5f - 8f);
            rect.x = 8;
            rect.width = Hold ? w : w / 2f;
            rect.height = 20;

            //int newValue = EditorGUI.IntField(rect, new GUIContent("f:"), value);
            int newValue = EditorGUI.Popup(rect, "f:", From, SetNames.ToArray());
            if (From != newValue) {
                From = newValue;

                Keyframe key = Channel.GetKeyAtTime(time);
                if (key != null && key.CustomKey != null) {
                    BlendKey k = (BlendKey)key.CustomKey;
                    if (k != null) {
                        k.FromSet = GetID(newValue);
                    }
                }
            }
            rect.x += rect.width;

            if (!Hold) {
                //newValue = EditorGUI.IntField(rect, new GUIContent("t:"), value);
                newValue = EditorGUI.Popup(rect, "t:", To, SetToNames.ToArray());
                if (To != newValue) {
                    To = newValue;

                    Keyframe key = Channel.GetKeyAtTime(time);
                    if (key != null && key.CustomKey != null) {
                        BlendKey k = (BlendKey)key.CustomKey;
                        if (k != null) {
                            k.ToSet = GetID(newValue);
                        }
                    }
                }
                rect.x += rect.width;
            }

            if (showBool) {
                rect.width = 40;
                bool inBool = Hold;
                bool outBool = EditorGUI.ToggleLeft(rect, new GUIContent("h"), inBool);
                if (inBool != outBool) {
                    Hold = outBool;

                    Keyframe key = Channel.GetKeyAtTime(time);
                    if (key != null && key.CustomKey != null) {
                        BlendKey k = (BlendKey)key.CustomKey;
                        if (k != null) {
                            k.Hold = outBool;
                        }
                    }
                }
            }

            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) {
                Channel.SetKey(time);
            }
        }

        public void GUIInfoValues(List<Keyframe> selectedKeys)
        {
            AxonGUI.BeginBox();

            int fromID = 0;
            int toID = 0;
            float durVal = 0;
            float startVal = 0;
            bool holdVal = false;
            bool revVal = false;
            bool autoDurVal = false;
            MathUtil.InterpolationModes mode = MathUtil.InterpolationModes.EaseInOut;

            bool first = true;
            bool isFromSame = true;
            bool isToSame = true;
            bool isModeSame = true;
            bool isDurSame = true;
            bool isAutoDurSame = true;
            bool isStartSame = true;
            bool isHoldSame = true;
            bool isRevSame = true;

            foreach (Keyframe key in selectedKeys) {
                if (key != null && key.CustomKey != null) {
                    BlendKey k = (BlendKey)key.CustomKey;
                    if (k != null) {
                        if (first) {
                            fromID = k.FromSet;
                            toID = k.ToSet;
                            durVal = k.Duration;
                            mode = k.InterpolationMode;
                            startVal = k.StartTime;
                            holdVal = k.Hold;
                            revVal = k.Reverse;
                            autoDurVal = k.AutoDuration;

                            first = false;
                        }
                        else {
                            if (isFromSame && fromID != k.FromSet) {
                                isFromSame = false;
                            }
                            if (isToSame && toID != k.ToSet) {
                                isToSame = false;
                            }
                            if (isDurSame && durVal != k.Duration) {
                                isDurSame = false;
                            }
                            if (isStartSame && startVal != k.StartTime) {
                                isStartSame = false;
                            }
                            if (isModeSame && mode != k.InterpolationMode) {
                                isModeSame = false;
                            }
                            if (isHoldSame && holdVal != k.Hold) {
                                isHoldSame = false;
                            }
                            if (isRevSame && revVal != k.Reverse) {
                                isRevSame = false;
                            }
                            if (isAutoDurSame && autoDurVal != k.AutoDuration) {
                                isAutoDurSame = false;
                            }
                        }
                    }
                }
            }

            AxonGUI.BeginChangeCheck();
            AxonGUI.BeginHorizontal();
            int inID = 0;
            if (isFromSame) inID = fromID;
            int inIndex = GetIndex(inID);
            AxonGUI.UndoName = "Set Blend From";
            int outIndex = AxonGUI.FieldPopup(this, "From", inIndex, SetNames.ToArray(), GUILayout.Width(200));
            int outID = GetID(outIndex);
            if (inID != outID) {
                int outIndex2 = GetIndex(outID);
                foreach (Keyframe key in selectedKeys) {
                    if (key != null && key.CustomKey != null) {
                        BlendKey k = (BlendKey)key.CustomKey;
                        if (k != null) {
                            k.FromSet = outID;
                        }
                    }
                }
            }
            if (selectedKeys.Count == 1 && selectedKeys[0] != null && selectedKeys[0].CustomKey != null) {
                BlendKey k = (BlendKey)selectedKeys[0].CustomKey;
                if (k != null) {
                    Blend set = k.Blend;
                    if (set.IsEditing && set.OverrideFrom == outIndex) {
                        GUI.color = AxonColor.EditingOverride;
                        if (GUILayout.Button("Done", GUILayout.Width(50))) {
                            set.StopEdit();
                        }
                        GUI.color = AxonColor.Default;
                    }
                    else {
                        if (GUILayout.Button("Edit", GUILayout.Width(50))) {
                            set.StartEdit(outIndex);
                        }
                    }
                }
            }

            bool inHold = false;
            if (isToSame) inHold = holdVal;
            AxonGUI.UndoName = "Set Blend Hold";
            bool outHold = AxonGUI.FieldToggleInline(this, "Hold", inHold);
            if (inHold != outHold) {
                foreach (Keyframe key in selectedKeys) {
                    if (key != null && key.CustomKey != null) {
                        BlendKey k = (BlendKey)key.CustomKey;
                        if (k != null) {
                            k.Hold = outHold;
                        }
                    }
                }
            }
            AxonGUI.EndHorizontal(false);

            if (!outHold) {
                AxonGUI.BeginHorizontal();
                if (isToSame) inID = toID;
                inIndex = GetIndex(inID);
                AxonGUI.UndoName = "Set Blend To";
                outIndex = AxonGUI.FieldPopup(this, "To", inIndex, SetToNames.ToArray(), GUILayout.Width(200));
                outID = GetID(outIndex);
                if (inID != outID) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.ToSet = outID;
                            }
                        }
                    }
                }
                if (selectedKeys.Count == 1 && selectedKeys[0] != null && selectedKeys[0].CustomKey != null) {
                    BlendKey k = (BlendKey)selectedKeys[0].CustomKey;
                    if (k != null) {
                        Blend set = k.Blend;
                        if (set.IsEditing && set.OverrideFrom == outIndex) {
                            GUI.color = AxonColor.EditingOverride;
                            if (GUILayout.Button("Done", GUILayout.Width(50))) {
                                set.StopEdit();
                            }
                            GUI.color = AxonColor.Default;
                        }
                        else {
                            if (GUILayout.Button("Edit", GUILayout.Width(50))) {
                                set.StartEdit(outIndex);
                            }
                        }
                    }
                }

                bool inRev = false;
                if (isToSame) inRev = revVal;
                AxonGUI.UndoName = "Set Blend Reverse";
                bool outRev = AxonGUI.FieldToggleInline(this, "Reverse", inRev);
                if (inRev != outRev) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.Reverse = outRev;
                            }
                        }
                    }
                }

                AxonGUI.EndHorizontal(false);

                AxonGUI.BeginHorizontal();
                AxonGUI.BeginDisabledGroup(autoDurVal);
                float inValf = 0;
                if (isDurSame) inValf = durVal;
                AxonGUI.UndoName = "Set Blend Duration";
                float outValf = AxonGUI.FieldFloat(this, "Duration", inValf);
                if (inValf != outValf) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.Duration = outValf;
                            }
                        }
                    }
                }
                AxonGUI.EndDisabledGroup();

                bool inAuto = false;
                if (isToSame) inAuto = autoDurVal;
                AxonGUI.UndoName = "Set Blend Auto Duration";
                bool outAuto = AxonGUI.FieldToggleInline(this, "Auto", inAuto);
                if (inAuto != outAuto) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.AutoDuration = outAuto;
                            }
                        }
                    }
                }
                if (GUILayout.Button("Set", GUILayout.Width(50))) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.AutoDuration = false;
                                k.Duration = CurrentTime - key.KeyTime - k.StartTime;
                            }
                        }
                    }
                }

                AxonGUI.EndHorizontal(false);

                AxonGUI.BeginHorizontal();
                inValf = 0;
                if (isDurSame) inValf = startVal;
                AxonGUI.UndoName = "Set Blend Start Time";
                outValf = AxonGUI.FieldFloat(this, "Start", inValf);
                if (inValf != outValf) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.StartTime = outValf;
                            }
                        }
                    }
                }
                AxonGUI.EndHorizontal(false);

                AxonGUI.BeginHorizontal();
                MathUtil.InterpolationModes inMode = 0;
                if (isModeSame) inMode = mode;
                AxonGUI.UndoName = "Set Blend Interpolation Mode";
                MathUtil.InterpolationModes outMode = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopup(this, "Interp", inMode);
                if (inMode != outMode) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key != null && key.CustomKey != null) {
                            BlendKey k = (BlendKey)key.CustomKey;
                            if (k != null) {
                                k.InterpolationMode = outMode;
                            }

                            // setting the keyframe on hold prevents the bezier handles from showing
                            key.Hold = (outMode != MathUtil.InterpolationModes.UseChannelCurve);
                        }
                    }
                }
                if (outMode == MathUtil.InterpolationModes.AnimationCurve) {
                    BlendKey k = selectedKeys[0].CustomKey == null ? null : (BlendKey)selectedKeys[0].CustomKey;
                    if (k != null) {
                        if (k.InterpolateCurve == null) {
                            k.InterpolateCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);
                        }
                        k.InterpolateCurve = EditorGUILayout.CurveField(k.InterpolateCurve);
                        if (selectedKeys.Count > 1) {
                            foreach (Keyframe key in selectedKeys) {
                                if (key != null && key.CustomKey != null) {
                                    k.InterpolationMode = outMode;
                                }
                            }
                        }
                    }
                }
                AxonGUI.EndHorizontal(false);
                AxonGUI.Space();
            }

            AxonGUI.Space();
            AxonGUI.EndBox();
            if (AxonGUI.EndChangeCheck()) {
                Refresh();
            }
        }

        #endregion

        #region MENU

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContextMenuBlendType info = new TimeflowContextMenuBlendType();
            info.BlendType = typeof(Blend);
            info.InHierarchy = inHierarchy;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Blend"), false, GUIMenu_Add, info);
        }

        public static void GUIMenu_Add(object obj)
        {
            TimeflowContextMenuBlendType info = (TimeflowContextMenuBlendType)obj;
            if (info != null) {
                Blend blend = Undo.AddComponent(TimeflowContext.Obj.gameObject, info.BlendType) as Blend;
                float clickTime = Timeflow.Active.View.TimeOfPosition(TimeflowContext.MenuPosition.x, false);
                float dif = Mathf.Abs(Timeflow.Active.CurrentTime - clickTime);

                TimeflowObject tobj;
                if (TimeflowContext.Obj.TryGetComponent<TimeflowObject>(out tobj)) {
                    tobj.Enabled = true;
                }
            }
        }

        #endregion

#if AXON_EXPERIMENTAL

        public void CopyAEMB()
        {
            if (Channel.Keys != null && Channel.Keys.Count > 0) {
                string data = "Adobe After Effects 8.0 Keyframe Data\n";
                data += "\tUnits Per Second\t" + Mathf.RoundToInt(Timeflow.FPS) + "\n";
                data += "\tSource Width\t4096\n";
                data += "\tSource Height\t4096\n";
                data += "\tSource Pixel Aspect Ratio\t1\n";
                data += "\tComp Pixel Aspect Ratio\t1\n";
                data += "\n";

                data += "Effects\tPixel Motion Blur #1\tShutter Angle #3\n";
                data += "\tFrame\t\t\n";

                int f = 0;

                foreach (Keyframe key in Channel.Keys) {
                    if (key.IsKeyEnabled) {
                        f = Mathf.RoundToInt(Timeflow.FPS * key.KeyTime);
                        if (f > 5) {
                            data += "\t" + (f - 3) + "\t180\n";
                            data += "\t" + (f - 1) + "\t0\n";
                            data += "\t" + f + "\t0\n";
                            data += "\t" + (f + 3) + "\t180\n";
                        }
                    }
                }


                data += "\n";
                data += "\n";
                data += "End of Keyframe Data";

                GUIUtility.systemCopyBuffer = data;
            }
        }
#endif

#endif
    }

}//AxonGenesis
