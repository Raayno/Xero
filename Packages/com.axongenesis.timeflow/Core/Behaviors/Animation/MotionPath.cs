// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace AxonGenesis
{
    /// <summary>
    /// Presets are not allowed for motion paths since the settings are distributed across multiple game
    /// objects. Use prefabs instead.
    /// </summary>
    [ExcludeFromPreset]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Motion Path")]
    sealed public partial class MotionPath : TimeflowBehavior
    {
        public static MotionPath Primary;

        #region PUBLIC

        public bool IsPrimary;
        public MotionPathNodes NodeContainer;
        public List<MotionPathNode> Nodes;

        public MotionPathChannel _Channel;

        public enum RotationModes
        {
            None,
            LookAhead,
            Interpolate
        }
        public RotationModes RotationMode = RotationModes.None;
        public bool RotationSmoothing;
        public float RotationSmoothTime = 0.25f;

        public bool ShowRotationChannel;
        public DataChannel RotationChannel;

        public bool ShowVelocityChannel;
        public enum VelocityChannelModes
        {
            Vector,
            Speed,
            Interpolation
        }
        public VelocityChannelModes VelocityChannelMode = VelocityChannelModes.Vector;
        public bool ShowVelocityAsInterpolation;
        public DataChannel VelocityChannel;

        public bool PositionSmoothing;
        public float PositionSmoothTime = 0.25f;
        public Vector3 Orientation = Vector3.zero;

        public enum VelocityModes
        {
            Fixed,
            Flexible
        }
        public VelocityModes VelocityMode = VelocityModes.Fixed;

        public Transform LookTarget;
        public float LookAheadTime = 1f;
        public bool ExposeLookTarget;
        public Vector3 LookOrientation = Vector3.up;

        public UnityEvent OnSetup;
        public bool NotifyOnSetup = true;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool IsSetup;

        [NonSerialized]
        public float Length;

        [NonSerialized]
        public float Duration;

        [NonSerialized]
        public float EndAtTime = 0f;

        [NonSerialized]
        public MotionPathNode LastNode;

        [NonSerialized]
        public float CurrentInterpolation;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private bool _ClosePath;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private Vector3 lastPos = Vector3.zero;

        [NonSerialized]
        private float lastTime = 0;

        [NonSerialized]
        private Vector3 finalLookAtPos = Vector3.zero;

        #endregion

        #region ACCESSORS

        public bool IsInitialized {
            get {
                return _Channel != null && _Channel.Keys != null && _Channel.Keys.Count > 0;
            }
        }

        public MotionPathChannel Channel {
            get {
                if (_Channel == null) {
                    _Channel = new MotionPathChannel(this);
                    _Channel.Interpolation = TimeflowChannel.Interpolations.Bezier;
                    _Channel.PathInterpolation = MotionPathChannel.PathInterpolations.Bezier;
                    AddChannel(_Channel);
                    //if (DebugEnabled) Debug.Log(name + ":MotionPath.Channel NEW");
                }
                return _Channel;
            }
            set {
                _Channel = value;
                if (_Channel != null) {
                    _Channel.MotionPath = this;
                    AddChannel(_Channel);
                    //if (DebugEnabled) Debug.Log(name + ":MotionPath.Channel Add");
                }
            }
        }

        public bool ApplyRotation => RotationMode != RotationModes.None;

        public Quaternion CurrentRotation {
            get {
                return Channel.CurrentRotation;
            }
        }

        public bool CanClosePath {
            get {
                return Channel != null && Channel.Keys != null && Channel.Keys.Count > 2;
            }
        }

        public bool ClosePath {
            get {
                return _ClosePath;
            }
            set {
                if (_ClosePath != value) {
#if UNITY_EDITOR
                    UndoUtil.Undo(this, "Loop Path", true);
#endif
                    _ClosePath = value;
                    SetupNodes();
                }
            }
        }

        public bool IsPathClosed {
            get {
                return _ClosePath && CanClosePath;
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log(name + ".MotionPath.OnAwake");
            base.OnAwake();

            if (IsPrimary || Primary == null) {
                Primary = this;
                IsPrimary = true;
            }
            else {
                IsPrimary = false;
            }
        }

        protected override void OnEnable()
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.OnEnable");
            base.OnEnable();
            if (IsInitialized) Setup();
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.OnDestruct");
            // Ensures removal from the AllChannels list

            if (NodeContainer != null) {
                NodeContainer.hideFlags = HideFlags.None;
                //ObjectUtil.Destroy(NodeContainer.gameObject);
                //NodeContainer = null;
            }
            base.RemoveChannel(_Channel);
            base.OnDestruct();
        }

        public override void Refresh()
        {
            base.Refresh();

            Setup();
            DoUpdate();

            TimeflowObject tobj;
            TryGetComponent<TimeflowObject>(out tobj);
            if (tobj != null && tobj.CanUpdate) {
                tobj.DoUpdate();
            }
        }

        private void Reset()
        {
            Setup();
            Channel.PathInterpolation = MotionPathChannel.PathInterpolations.Bezier;
            SetupPathCurves();
        }

        public void Setup()
        {
            if (Timeflow == null) return;

            IsSetup = true;

#if UNITY_EDITOR
            if (EditorWarnAnimator) {
                if (gameObject.TryGetComponent<Animator>(out Animator anim)) {
                    EditorWarnAnimator = false;
                    EditorUtil.ShowDialog("Animator detected on object '" + name + "'!",
                        "It is strongly suggested to avoid adding MotionPath to the same object containing an Animator component. " +
                        "Instead consider adding MotionPath to a parent object, otherwise the animation on the object may conflict with " +
                        "the path interpolation and cause anomolies. This is especially true for character animations with root motion applied. " +
                        "This warning will only be displayed once per object and you may choose to ignore it.");
                }
            }
#endif

            SetupChannels(true);
            SetupNodeContainer();
            SetupNodes();
            SetupLookAt();

            if (!IsInitialized) {
                CreateDefaultPath();
            }

            if (NotifyOnSetup && OnSetup != null) {
                OnSetup.Invoke();
            }
        }

        public void CheckRelatedObjects()
        {
            //Debug.Log($"{name}.CheckRelatedObjects", gameObject);
            // Force names of managed objects to avoid confusion
            if (LookTarget != null) {
                //Debug.Log($"LookTarget:{LookTarget.name}", LookTarget);
                // Make sure object shares same parent as the main motion path object
                if (LookTarget.parent != transform.parent) LookTarget.SetParent(transform.parent);
                LookTarget.name = gameObject.name + "LookTarget";
            }
            if (NodeContainer != null) {
                //Debug.Log($"NodeContainer:{NodeContainer.name}", LookTarget);
                if (NodeContainer.transform.parent != transform.parent) NodeContainer.transform.SetParent(transform.parent);
                NodeContainer.name = gameObject.name + "Nodes";
            }
        }

        public void CreateDefaultPath()
        {
            //if (DebugEnabled) Debug.Log(name + ".MotionPath.CreateDefaultPath");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Create Default Motion Path", true);
#endif
            Channel.ClearKeys(true);

            Vector3 pos = transform.localPosition;
            SetKey(CurrentTime, pos);
            pos.z += 20f;
            SetKey(CurrentTime + 10f, pos);

            SetupPathCurves();

#if UNITY_EDITOR
            /// Select a newly added path channel
            Timeflow.Active.View.SelectChannel(Channel, true);
#endif
        }

        public void InitKey(Keyframe key, MotionPathNode prev)
        {
            key.IsVector3 = true;
            key.KeyVector3 = transform.localPosition;
            key.Linear = false;
            key.UnifyTangents = true;
            key.UnifyTangentLengthRatio = false;
            key.UnifyTangentLengths = true;
            key.OutTangent = new Vector2(1f, 0f);
            key.InTangent = new Vector2(-1f, 0f);

            float tanLength = 0.5f;
            if (prev == null) {
                prev = GetPrevNode(key.KeyTime);
            }
            if (prev != null && prev.Key != null) {
                /// Calculate the tangent length as 1/5 of the distance between the nodes
                tanLength = MathUtil.Distance(key.KeyVector3, prev.Key.KeyVector3) / 5f;
            }
            key.VectorOutTangent = transform.TransformPoint(Vector3.forward * tanLength) - key.KeyVector3;
            key.UnifyTangentLengths = true;
            key.IsAutoTangents = true;
        }

        public Keyframe SetupKey(MotionPathNode node, MotionPathNode prev, float time, bool create)
        {
            //if (DebugEnabled) Debug.Log(name + ".MotionPath.SetupKey:" + node.name + " prev:" + (prev == null ? "NULL" : prev.name));
            Keyframe key = null;

            if (!Nodes.Contains(node)) {
                Nodes.Add(node);
            }

            Rotator.Setup(node.gameObject, true);

#if UNITY_EDITOR
            _Channel.ShowTangents = true;
#endif

            if (_Channel.Keys == null) {
                _Channel.Keys = new List<Keyframe>();
            }
            if (_Channel.Keys.Count > 0) {
                foreach (Keyframe k in _Channel.Keys) {
                    if (k.KeyGameObject == node.gameObject) {
                        key = k;
                        break;
                    }
                }
            }

            if (key == null && create) {
                key = _Channel.SetKeyGameObject(time, node.gameObject, false);
                InitKey(key, prev);
            }
            key.Channel = Channel;
            node.Key = key;
            node.Previous = prev;

            return key;
        }

        /// <summary>
        /// Motion path keyframes are associated with 'nodes' which are game objects created to externally
        /// store transform data. By default nodes are hidden but may be exposed if the user chooses to.
        /// Exposed nodes can be used to aid in scene building and relating other object placements with
        /// specific points on the path.
        /// </summary>
        public void SetupNodeContainer()
        {
            if (NodeContainer == null) {
                GameObject nodes = new GameObject(name + "Nodes");
                NodeContainer = ObjectUtil.AddComponent<MotionPathNodes>(nodes);
                NodeContainer.MotionPath = this;
            }
            if (NodeContainer != null) {
                if (NodeContainer.MotionPath != this) {
                    /// This condition will occur when a motion path is duplicated. This will require
                    /// reconstructing the nodes to pertain to this current path and not get confused with
                    /// the other motion path the nodes originally belonged to.
                    NodeContainer = GameObject.Instantiate(NodeContainer); // Duplicate
                    NodeContainer.MotionPath = this;
                    NodeContainer.name = name + "Nodes";

                    ObjectUtil.SortChildrenByName(NodeContainer.gameObject);

                    /// Assume look target also needs to be rebuilt
                    LookTarget = null;
                    SetupLookAt();

                    /// Rebuild nodes list from children of new duplicate
                    Nodes = new List<MotionPathNode>();
                    foreach (Transform child in NodeContainer.transform) {
                        MotionPathNode node;
                        if (child.TryGetComponent<MotionPathNode>(out node)) {
                            node.MotionPath = this;
                            Nodes.Add(node);
                        }
                        else {
                            Debug.LogWarning("Unexpected null MotionPathNode during reconstruction of duplicate motion path");
                        }
                    }
                    if (_Channel.Keys == null) {
                        _Channel.Keys = new List<Keyframe>();
                    }
                    if (_Channel.Keys.Count > 0) {
                        foreach (Keyframe k in _Channel.Keys) {
                            /// Reassign keys to the duplicate nodes by name
                            foreach (Transform child in NodeContainer.transform) {
                                if (k.KeyGameObject.name == child.gameObject.name) {
                                    k.KeyGameObject = child.gameObject;
                                    MotionPathNode node;
                                    child.TryGetComponent<MotionPathNode>(out node);
                                    node.Key = k;
                                    break;
                                }
                            }
                        }
                    }
                    _Channel.TangentsNeedUpdate = true;
                }

                NodeContainer.transform.parent = transform.parent;
                NodeContainer.transform.localPosition = Vector3.zero;
                NodeContainer.transform.localRotation = Quaternion.identity;
#if UNITY_EDITOR
                if (Nodes != null) {
                    /// As an extra precaution, make sure the node objects are parented to the container
                    foreach (MotionPathNode node in Nodes) {
                        if (node == null) continue;
                        if (node.transform.parent != NodeContainer.transform) {
                            node.transform.SetParent(NodeContainer.transform);
                        }
                    }
                }
                NodeContainer.gameObject.hideFlags = _ExposeNodes ? HideFlags.None : HideFlags.HideInHierarchy;
                foreach (Transform child in NodeContainer.transform) {
                    child.gameObject.hideFlags = NodeContainer.gameObject.hideFlags;
                }

                /// Make sure the children are displayed in order
                ObjectUtil.SortChildrenByName(NodeContainer.gameObject);
#endif
            }
        }

        /// <summary>
        /// This checks that the Nodes have been set up and associated with keyframes. It also prepares the
        /// nodes for interpolatation and looping.
        /// </summary>
        public void SetupNodes()
        {
            if (Nodes == null || Nodes.Count == 0) {
                GatherNodes(false);
            }
            if (Nodes != null) {
                //SortNodes(); // Disabled because it causes OnHierarchyChange in loop
                int p = 0;
                MotionPathNode last = null;
                foreach (MotionPathNode node in Nodes) {
                    if (node != null && node.Enabled) {
                        //if (DebugEnabled) Debug.Log("Setup Node:" + node.gameObject.name);
                        node.MotionPath = this;
                        node.Previous = last;

                        MotionPathNode prev = p == 0 ? null : Nodes[p - 1];
                        SetupKey(node, prev, CurrentTime, true);

                        Duration = node.Key.KeyTime;
                        last = node;
                    }
                    p++;
                }
            }

            /// The last node is precalculated rather than provided in an accessor since key ordering may
            /// change volatily while dragging or performing ops. Precaulcating it prevents undesired
            /// behavior since it cannot be assumed the last node is always the last key in time.
            LastNode = null;
            if (Nodes.Count > 0) {
                SortNodes();
                /// Must find the last node that is enabled
                for (int x = Nodes.Count - 1; x >= 0; x--) {
                    LastNode = Nodes[x];
                    if (LastNode != null && LastNode.Key.IsKeyEnabled) break;
                }
            }

#if UNITY_EDITOR
            if (_ClosePath && CanClosePath && LastNode != null) {
                MotionPathNode first = Nodes[0];
                if (DragNode != null && DragNode == LastNode) {
                    first.Position = LastNode.Position;
                    first.Euler = LastNode.Euler;
                    first.Key._VectorInTangent = LastNode.Key._VectorInTangent;
                    first.Key._VectorOutTangent = LastNode.Key._VectorOutTangent;
                }
                else {
                    LastNode.Position = first.Position;
                    LastNode.Euler = first.Euler;
                    LastNode.Key._VectorInTangent = first.Key._VectorInTangent;
                    LastNode.Key._VectorOutTangent = first.Key._VectorOutTangent;
                }
            }
#endif

            SetupPathCurves();
        }

        public void SetupLookAt()
        {
            if (RotationMode == RotationModes.LookAhead) {
                if (LookTarget == null) {
                    GameObject obj = new GameObject(name + "LookTarget");
                    LookTarget = obj.transform;
                    LookTarget.parent = transform.parent;
                }

                if (Channel != null && Channel.Keys != null && Channel.Keys.Count > 0) {
                    Keyframe endKey = Channel.Keys[Channel.Keys.Count - 1];
                    Vector3 beforeEndKey = Channel.InterpolateVector3(endKey.KeyTime - LookAheadTime, false, true);
                    Vector3 offset = endKey.KeyVector3 - beforeEndKey;
                    finalLookAtPos = endKey.KeyVector3 + offset;
                }
                else finalLookAtPos = Vector3.zero;
            }

#if UNITY_EDITOR
            if (LookTarget != null) {
                LookTarget.transform.SetParent(transform.parent);
                LookTarget.gameObject.hideFlags = ExposeLookTarget ? HideFlags.None : HideFlags.HideInHierarchy;
            }
#endif
        }

        public void SetupPathCurves()
        {
            //if (DebugEnabled) Debug.Log("SetupPathCurves");
            Channel.MotionPath = this; // set again to avoid errors during setup calls
            Channel.SetupKeyframes();

#if UNITY_EDITOR
            if (AutoUpdateTangents && Nodes != null && Nodes.Count > 0) {
                foreach (MotionPathNode node in Nodes) {
                    if (node.IsAutoTangents) {
                        CalculateAutoTangent(node);
                    }
                }
            }
#endif

            Channel.BuildVectorPath();
            Length = Channel.GetVectorLength();

            if (!Application.isPlaying) {
                UpdateTimeChannel(Channel);
            }
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            if (typeof(MotionPath).IsAssignableFrom(src.GetType())) {
                MotionPath mp = (MotionPath)src;
                if (mp != null) {
                    // includeChannels ignored since there is a strict channel setup
                    CopyChannel(mp.Channel);
                }
            }
            base.Copy(src, includeChannels);
            //if (NodeContainer != null) {
            //    Debug.Log($"Copy NodeContainer:{NodeContainer.name}");
            //    NodeContainer.MotionPath = this;
            //}
        }

        #endregion

        #region CHANNELS

        /// <summary>
        /// Although technically motion path does show 2 channels, it does not support any additional
        /// channels, so this value must be false.
        /// </summary>
        /// <returns></returns>
        public override bool SupportsMultipleChannels()
        {
            return false;
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            //if (DebugEnabled) Debug.Log(name + ":MotionPath.SetupChannels");

            if (Channel.ToProperty == null) {
                Channel.ToProperty = new Property();
            }
            if (!IsInitialized) {
                IsPrimary = true;
                Channel.IsEnabled = true;
#if UNITY_EDITOR
                Channel.DisplayChannel = true;
#endif
                Channel.EnableAutoLoop = true;
                Channel.Interpolation = TimeflowChannel.Interpolations.Bezier;
            }
            Channel.HasProperty = true;
            Channel.ShowValue = true;
            Channel.ShowFloat = true;
#if UNITY_EDITOR
            Channel.ShowPathHandles = true;
#endif
            Channel.SupportsKeyframes = true;
            Channel.IsLoopSupported = true;
            Channel.CanAddRemoveKeys = true;
            Channel.PropertyType = Property.PropertyTypes.Vector3;
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.CanBeAssigned = false;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
            Channel.ToProperty.EnableHandlers = false;
            Channel.ToProperty.Comp = null;
            Channel.ToProperty.Handler = null;

            if (string.IsNullOrEmpty(Channel.Name) || string.IsNullOrEmpty(Channel.ToProperty.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Path Position";
            }

            if (string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Path Position";
            }
            Channel.LimitValue = false;
#if UNITY_EDITOR
            Channel.GraphFloatValueOnly = true;
#endif
            Channel.ShowValue = false;
            Channel.ShowVector = true;
            Channel.ShowGameObject = true;
            Channel.OnSetup(this);

            Channels = new List<TimeflowChannel>();
            Channels.Add(Channel);

            if (ShowRotationChannel) {
                if (RotationChannel == null) {
                    RotationChannel = new DataChannel(this);
                }
                RotationChannel.IsEnabled = true;
                if (RotationChannel.ToProperty == null) {
                    RotationChannel.ToProperty = new Property();
                }
                RotationChannel.DataParent = this;
                RotationChannel.ShowValue = true;
                RotationChannel.ShowVector = true;
                RotationChannel.IsDataOnly = true;
                RotationChannel.SupportsKeyframes = false;
                RotationChannel.PropertyType = Property.PropertyTypes.Vector3;

                RotationChannel.ToProperty.IsEnabled = true;
                RotationChannel.ToProperty.CanBeAssigned = false;
                RotationChannel.ToProperty.PropertyType = RotationChannel.PropertyType;
                RotationChannel.ToProperty.IsDataOnly = true;
                RotationChannel.ToProperty.IsCombinedValue = true;
                RotationChannel.OnSetup(this);

                if (string.IsNullOrEmpty(RotationChannel.Name) || string.IsNullOrEmpty(RotationChannel.ToProperty.Name)) {
                    RotationChannel.Name = RotationChannel.ToProperty.Name = "Path Rotation";
                }

                Channels.Add(RotationChannel);
            }
            else {
                if (RotationChannel != null && RotationChannel.IsEnabled) {
                    RemoveChannelWithUndo(RotationChannel);
                }
                RotationChannel = null;
            }

            if (ShowVelocityChannel) {
                if (VelocityChannel == null) {
                    VelocityChannel = new DataChannel(this);
                    //if (DebugEnabled) Debug.Log("VelocityChannel new TimeflowChannel");
                }
                VelocityChannel.DataParent = this;
                VelocityChannel.IsEnabled = true;
                if (VelocityChannel.ToProperty == null) {
                    VelocityChannel.ToProperty = new Property();
                }
                VelocityChannel.ShowValue = VelocityChannelMode != VelocityChannelModes.Vector;
                VelocityChannel.ShowVector = VelocityChannelMode == VelocityChannelModes.Vector;
                VelocityChannel.IsDataOnly = true;
                VelocityChannel.SupportsKeyframes = false;
                VelocityChannel.PropertyType = VelocityChannelMode == VelocityChannelModes.Vector ? Property.PropertyTypes.Vector3 : Property.PropertyTypes.Float;

                VelocityChannel.ToProperty.IsEnabled = true;
                VelocityChannel.ToProperty.CanBeAssigned = false;
                VelocityChannel.ToProperty.PropertyType = VelocityChannel.PropertyType;
                VelocityChannel.ToProperty.IsDataOnly = true;
                VelocityChannel.ToProperty.IsCombinedValue = true;
                VelocityChannel.OnSetup(this);

                if (string.IsNullOrEmpty(VelocityChannel.Name) || string.IsNullOrEmpty(VelocityChannel.ToProperty.Name)) {
                    VelocityChannel.Name = VelocityChannel.ToProperty.Name = "Path Velocity";
                }

                Channels.Add(VelocityChannel);
            }
            else {
                if (VelocityChannel != null && VelocityChannel.IsEnabled) {
                    RemoveChannelWithUndo(VelocityChannel);
                }
                VelocityChannel = null;
            }

            Channel.PropertyType = Property.PropertyTypes.Vector3;
            Channel.MotionPath = this;
            Channel.IsVectorLoop = _ClosePath && CanClosePath;

            //Setup();
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.RegisterChannels");
            obj.RegisterChannel(Channel);
            if (ShowRotationChannel && RotationChannel != null) obj.RegisterChannel(RotationChannel);
            if (ShowVelocityChannel && VelocityChannel != null) obj.RegisterChannel(VelocityChannel);
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.RemoveChannel");
            bool isMainChannel = channel == Channel;
            base.RemoveChannel(channel); // remove references from other objects first
            if (isMainChannel) {
                // Remove the behavior if the main motion path channel is removed but leave it in place if the rotation or other channel is removed
                Object.DestroyImmediate(this);
            }
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.RemoveChannelWithUndo");
            bool isMainChannel = channel == Channel;
            base.RemoveChannelWithUndo(channel);

            if (isMainChannel) {
                // Don't need to destroy here since it will be done by RemoveChannel()
            }
            else
            if (channel == RotationChannel) {
                // Channel was already removed and destroyed in the base method above, so just clean up here
                RotationChannel = null;
                ShowRotationChannel = false;
            }
            else
            if (channel == VelocityChannel) {
                // Channel was already removed and destroyed in the base method above, so just clean up here
                VelocityChannel = null;
                ShowVelocityChannel = false;
            }
        }

        /// <summary>
        /// This copies an entire motion path channel with keyframes and nodes. This is a bit tricky since
        /// nodes and keys have a special relationship and there are cross references that need to be
        /// resetup on the new object.
        /// </summary>
        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.CopyChannel");

#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
            if (NodeContainer != null) UndoUtil.UndoDestroy(NodeContainer);
#else
            if(NodeContainer != null) DestroyImmediate(NodeContainer);
#endif
            // Copies over bulk settings and keyframes
            Channel.Copy(src);
            Channel.Behavior = this;
            Channel.MotionPath = this;

            // Copy the node container and nodes and reconnect to new duplicated keys
            Nodes = null;
            NodeContainer = null;
            MotionPathChannel ch = (MotionPathChannel)src;
            if (ch != null && ch.MotionPath != null) {
                if (ch.MotionPath.NodeContainer != null && ch.MotionPath.NodeContainer.transform.childCount > 0) {
                    NodeContainer = GameObject.Instantiate(ch.MotionPath.NodeContainer);
                    NodeContainer.name = name + "Nodes";
                    SetupNodeContainer(); // default setup

                    MotionPathNode prev = null;
                    Nodes = new List<MotionPathNode>();
                    int i = 0;
                    foreach (Transform child in NodeContainer.transform) {
                        MotionPathNode node;
                        child.TryGetComponent<MotionPathNode>(out node);
                        if (node != null) {
                            Keyframe k = Channel.Keys[i];
                            // Keys using the game object reference to relate to nodes
                            k.KeyGameObject = node.gameObject;
                            node.Key = k;
                            node.Previous = prev;
                            if (prev != null) prev.Next = node;
                            node.MotionPath = this;
                            Nodes.Add(node);
                            prev = node;
                        }
                        i++;
                    }
                    SortNodes();
                    Setup();
                }
            }

            return Channel;
        }

        #endregion

        #region NODES

        public Keyframe SetKey(float time)
        {
            if (Channel == null) SetupChannels(true);
            if (!Channel.IsEnabled || Channel.IsLocked) return null; // don't set new keyframes on locked or disabled channels
            Keyframe key = Channel.SetKey(time);
            if (key != null) {
                InitKey(key, null);
            }
            return key;
        }

        public Keyframe SetKey(float time, Vector3 position)
        {
            if (Channel == null) SetupChannels(true);
            if (!Channel.IsEnabled || Channel.IsLocked) return null; // don't set new keyframes on locked or disabled channels
            Keyframe key = Channel.SetKeyVector(time, position, true);
            if (key != null) {
                InitKey(key, null);
            }
            return key;
        }

        /// <summary>
        /// Forces update of tangent curves after value change
        /// </summary>
        public override void OnInterpolationChanged()
        {
            if (Channel != null) Channel.UpdateTangents();
        }

        public void GatherNodes(bool runSetup = false)
        {
            if (NodeContainer == null) return;
            //if (DebugEnabled) Debug.Log("MotionPath.GatherNodes:" + NodeContainer.name);

            if (Channel.Keys != null) {
                // Remove any keyframes not associated with a game object (ie orphaned)
                List<Keyframe> remove = null;
                foreach (Keyframe k in Channel.Keys) {
                    if (k.KeyGameObject == null) {
                        if (remove == null) remove = new List<Keyframe>();
                        remove.Add(k);
                    }
                }
                if (remove != null) {
                    foreach (Keyframe k in remove) {
                        Channel.KeysRemove(k);
                    }
                }
            }

            Nodes = new List<MotionPathNode>();
            foreach (Transform child in NodeContainer.transform) {
                MotionPathNode node;
                if (child.TryGetComponent<MotionPathNode>(out node)) {
                    Nodes.Add(node);
                }
            }
            SortNodes();

            if (runSetup) Setup();
        }

        public void GenerateFromChildPositions()
        {
            if (NodeContainer == null) {
                GameObject container = new GameObject(name + "Nodes");
                NodeContainer = ObjectUtil.AddComponent<MotionPathNodes>(container);
            }
            float time = 0f;
            foreach (Transform child in transform) {
                MotionPathNode node = AddNode(time);
                node.name = node.name + " " + child.name;
                node.transform.SetPositionAndRotation(child.position, child.rotation);
                time += 1f;
            }
        }

        public MotionPathNode AddNode(GameObject obj, float time)
        {
            return AddNode(obj, time, true);
        }

        public MotionPathNode AddNode(GameObject obj, float time, bool runSetup)
        {
            //if (DebugEnabled) Debug.Log("MotionPath.AddNode:" + obj.name);

#if UNITY_EDITOR
            UndoUtil.Undo(this, "Add Node Point", true);
#endif
            MotionPathNode node;
            if (!obj.TryGetComponent<MotionPathNode>(out node)) {
                node = ObjectUtil.AddComponent<MotionPathNode>(obj);
#if UNITY_EDITOR
                UndoUtil.UndoCreate(node, "Add Node Point");
#endif
            }
            node.MotionPath = this;

            bool sort = false;
            if (node != null) {
                if (!Nodes.Contains(node)) {
                    Nodes.Add(node);
                    sort = true;
                }
                SetupKey(node, GetPrevNode(time), time, true);
            }
            if (sort) SortNodes();

#if UNITY_EDITOR
            SelectNode(node, true);
#endif

            if (runSetup) Setup();
            return node;
        }

        public MotionPathNode AddNode(float time)
        {
            MotionPathNode node = null;

            if (Nodes != null && Nodes.Count > 0) {
                foreach (MotionPathNode n in Nodes) {
                    if (n == null || !n.Enabled) continue;
                    if (!MathUtil.IsTimeDifferent(n.Key.KeyTime, time)) {
                        node = n;
                        break;
                    }
                }
            }

            if (node == null) {
                string name = "Node";
                MotionPathNode prev = GetPrevNode(time);
                if (prev != null) {
                    string[] parts = prev.name.Split(' ');
                    float num = StringUtil.ParseFloat(parts[0]) + 0.01f;
                    num = Mathf.Round(num * 100f) / 100f;
                    if (parts.Length > 1) {
                        name = num + " " + parts[1];
                    }
                    else {
                        name = "" + num;
                    }
                }

                GameObject obj = new GameObject(name);
#if UNITY_EDITOR
                SelectNode(node);
                UndoUtil.UndoCreate(obj, "Add Node");
#endif

                obj.transform.SetParent(NodeContainer.transform);
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f); // To correct for autorotate

                node = AddNode(obj, time);
            }
            return node;
        }

        public MotionPathNode GetPrevNode(float time)
        {
            MotionPathNode point = null;
            if (Nodes != null && Nodes.Count > 1) {
                foreach (MotionPathNode node in Nodes) {
                    if (node != null && node.Enabled && node.Key != null && node.Key.KeyTime < time) {
                        if (point == null) {
                            point = node;
                        }
                        else
                        if (point.Key.KeyTime < node.Key.KeyTime) {
                            point = node;
                        }
                    }
                }
                if (IsPathClosed && point == null) {
                    point = Nodes[Nodes.Count - 1];
                }
            }
            return point;
        }

        public MotionPathNode GetNextNode(float time)
        {
            MotionPathNode point = null;
            if (Nodes != null && Nodes.Count > 1) {
                foreach (MotionPathNode node in Nodes) {
                    if (!node.Enabled) continue;
                    if (node != null && node.Enabled && node.Key != null && node.Key.KeyTime > time) {
                        if (point == null) {
                            point = node;
                        }
                        else
                        if (point.Key.KeyTime > node.Key.KeyTime) {
                            point = node;
                        }
                    }
                }
                if (IsPathClosed && point == null) {
                    point = Nodes[0];
                }
            }
            return point;
        }

        public int GetIndex(Keyframe key)
        {
            int index = 0;

            if (key != null && Nodes != null) {
                int i = 0;
                foreach (MotionPathNode n in Nodes) {
                    if (n.Key == key) {
                        index = i;
                        break;
                    }
                    i++;
                }
            }

            return index;
        }

        public MotionPathNode GetNode(Keyframe key)
        {
            MotionPathNode node = null;

            if (key != null && Nodes != null) {
                foreach (MotionPathNode n in Nodes) {
                    if (n.Key == key) {
                        node = n;
                        break;
                    }
                }
            }

            return node;
        }

        public MotionPathNode GetNode(int index)
        {
            MotionPathNode node = null;

            if (Nodes != null) {
                int i = 0;
                foreach (MotionPathNode n in Nodes) {
                    if (index == i) {
                        node = n;
                        break;
                    }
                    i++;
                }
            }

            return node;
        }

        public MotionPathNode GetNode(float time)
        {
            MotionPathNode node = null;

            if (Nodes != null) {
                foreach (MotionPathNode n in Nodes) {
                    if (!MathUtil.IsTimeDifferent(n.Key.KeyTime, time)) {
                        node = n;
                        break;
                    }
                }
            }

            return node;
        }

        public MotionPathNode GetNearestNode(Vector3 worldPos)
        {
            MotionPathNode nearest = null;

            if (Nodes != null) {
                float nearestDist = float.MaxValue;
                foreach (MotionPathNode p in Nodes) {
                    if (!p.Enabled) continue;
                    float dist = MathUtil.Distance(worldPos, p.transform.position);
                    if (dist < nearestDist) {
                        nearestDist = dist;
                        nearest = p;
                    }
                }
            }

            return nearest;
        }

        public void RemoveNode(int index)
        {
            RemoveNode(GetNode(index));
        }

        public void RemoveNode(MotionPathNode node)
        {
            if (node == null) return;
            Channel.UnsetKey(node.Key);
            Refresh();
        }

        public void UpdateNodes()
        {
            if (Nodes != null) {
                int i = 1;
                float prevTime = -100f;

                RemoveInvalidNodes();

                foreach (MotionPathNode p in Nodes.ToArray()) {
                    string n = "Node_" + StringUtil.PadNumber3(i);
                    if (p.gameObject.name != n) {
                        p.gameObject.name = n;
                    }
                    if (p.KeyTime == prevTime && prevTime != -100f) {
                        p.KeyTime = prevTime + 0.1f;
                        Debug.LogWarning($"The motion path node '{n}' has the same keyframe time as the preceding key. Please remove the keyframe or adjust its time.");
                    }
                    prevTime = p.KeyTime;
                    i++;
                }
            }
        }

        public void SortNodes()
        {
            if (Nodes != null && Nodes.Count > 0) {
                //if (DebugEnabled) Debug.Log(name + ".SortNodes");
                Nodes.Sort(new SortMotionPathNode());
                UpdateNodes();
            }
        }

        public void ClearNodes()
        {
            if (Nodes == null || Nodes.Count == 0) return;

#if UNITY_EDITOR
            if (EditorUtil.ShowDialog("Clear All Nodes?", "Are you sure you want to delete all the keyframes and nodes and reset the path?", "Ok", "Cancel")) {
                UndoUtil.Undo(this, "Clear All Nodes", true);
                foreach (MotionPathNode node in Nodes) {
                    UndoUtil.UndoDestroy(node.gameObject);
                }
            }
#endif
            Nodes = new List<MotionPathNode>();
            Channel.ClearKeys(false);
            Channel.VectorPathPoly = null;
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// Only updates with the primary channel. The rotation and velocity channels are meant to be
        /// output only and are lumped together with the primary motion path channel.
        /// </summary>
        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (!Enabled) return;//wasUpdatedThisFrame
            float time = channel.CurrentTime;

            if (Rotator == null) return;
            if (channel == Channel) {
                Vector3 p = transform.localPosition;
                Quaternion r = Rotator.Rotation;
                Interpolate(time, transform); // note that this modifies the position, so it is different than p

                float d = LocalDeltaTime;

                if (PositionSmoothing && d > 0f && PositionSmoothTime > 0) {
                    transform.localPosition = MathUtil.Interpolate(p, transform.localPosition, LocalDeltaTime / PositionSmoothTime);
                }
                if (RotationMode != RotationModes.None) {
                    if (RotationMode == RotationModes.LookAhead && LookTarget != null && Channel.Keys.Count > 1) {
                        if (LookAheadTime == 0f) LookAheadTime = 0.01f;
                        float lookTime = time + LookAheadTime;

                        int lastKeyIndex = Channel.Keys.Count - 1;

                        if ((!Channel.EnableLoop || !Channel.EnableLoopOut) && lookTime >= Channel.Keys[lastKeyIndex].KeyTime) {
                            // Fixes issue with object and look-at position being at same position on or after end keyframe
                            LookTarget.transform.localPosition = finalLookAtPos;
                        }
                        else {
                            LookTarget.transform.localPosition = Channel.InterpolateVector3(lookTime, false, true);
                        }

                        if (LookTarget.transform.position == transform.position) {
                            if (transform.parent != null) {
                                LookTarget.transform.position = transform.TransformDirection(Vector3.forward);
                            }
                            else {
                                LookTarget.transform.position = transform.position + new Vector3(0f, 0f, 0.01f);
                            }
                        }
                        transform.LookAt(LookTarget, LookOrientation);
                    }
                    Rotator.Rotation = transform.rotation * Quaternion.Euler(Orientation);
                    if (RotationSmoothing && d > 0f && RotationSmoothTime > 0) {
                        Rotator.Rotation = Quaternion.Lerp(r, Rotator.Rotation, LocalDeltaTime / RotationSmoothTime);
                    }
                }
            }
            else
            if (RotationChannel != null && RotationChannel != channel) {
                RotationChannel.CurrentVector = Rotator.Euler;
                RotationChannel.ToProperty.Vector3Value = Rotator.Euler;
            }
        }

        public override void OnUpdateTimingMode()
        {
            Setup();
        }

        public void Interpolate()
        {
            Interpolate(CurrentTime, transform);
        }

        public void Interpolate(float time, Transform applyTo)
        {
            if (Enabled && gameObject.activeInHierarchy) {
                if (Nodes == null || Nodes.Count == 0) return;
                if (Nodes.Count < 2) {
                    MotionPathNode node = Nodes[0];
                    applyTo.localPosition = lastPos = node.transform.localPosition;
                    lastTime = time;
                    if (RotationMode != RotationModes.None) {
                        applyTo.localRotation = node.transform.localRotation;
                    }
                    CurrentInterpolation = 0;
                    //if (DebugEnabled) Debug.Log("InterpolateValue Nodes:" + Nodes.Count);
                    return;
                }

                // Time looping is handled by the channel base class
                time = Channel.LoopTime(time);

                float interp = 0f;
                float thisStartTime = 0f;

                if (Duration != 0) {
                    interp = (time - thisStartTime) / Duration;
                }

                if (interp > 1f) interp = 1f;
                else
                if (interp < 0f) interp = 0f;

                CurrentInterpolation = interp;
                float delta = time - lastTime;

                Vector3 pos = _Channel.InterpolatePath(_Channel.CurrentTime, true, true, transform, RotationMode != RotationModes.None);

                if (ShowVelocityChannel && VelocityChannel != null) {
                    if (VelocityChannelMode == VelocityChannelModes.Interpolation) {
                        VelocityChannel.ToProperty.FloatValue = CurrentInterpolation;
                        VelocityChannel.CurrentValue = CurrentInterpolation;
                    }
                    else
                    if (LocalDeltaTime != 0f) {
                        Vector3 dif = pos - lastPos;
                        //if (DebugEnabled) Debug.Log($"time:{_Channel.CurrentTime} pos:{pos} last:{lastPos} delta:{LocalDeltaTime} d2:{delta}");

                        if (VelocityChannelMode == VelocityChannelModes.Speed) {
                            VelocityChannel.CurrentValue = dif.magnitude / LocalDeltaTime;
                            //if (DebugEnabled) Debug.Log("Velocity:" + VelocityChannel.CurrentValue + " LocalDeltaTime:" + LocalDeltaTime + " dif:" + dif);
                            VelocityChannel.ToProperty.FloatValue = VelocityChannel.CurrentValue;
                        }
                        else {
                            VelocityChannel.CurrentVector = MathUtil.Divide(dif, LocalDeltaTime);
                            VelocityChannel.ToProperty.Vector3Value = VelocityChannel.CurrentVector;
                        }
                    }
                    //else Debug.Log("LocalDeltaTime:0");
                }
                lastPos = pos;
                lastTime = time;
            }
        }

        public Vector3 InterpolatePath(float time, bool isLocalTime)
        {
            return _Channel.InterpolatePath(time, false, isLocalTime, null, false);
        }

        public float VelocityAdjustedTime(float time)
        {
            // Map linear time to curved velocity of path
            if (Channel != null) {
                time = Channel.InterpolateTime(time);
            }

            return time;
        }

        public void RemoveInvalidNodes()
        {
            if (Nodes != null) {
                List<MotionPathNode> validNodes = new List<MotionPathNode>();

                foreach (MotionPathNode node in Nodes) {
                    if (node != null && node.gameObject != null) {
                        validNodes.Add(node);
                    }
                }

                Nodes = validNodes;
            }
        }

        #endregion

    }

}//AxonGenesis
