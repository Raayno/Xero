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
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Place On Path")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/place-on-path")]
    sealed public class PlaceOnPath : TimeflowDataBehavior
    {
        public static void ProcessAll()
        {
            PlaceOnPath[] instances = UnityEngine.Object.FindObjectsByType(typeof(PlaceOnPath), FindObjectsInactive.Include, FindObjectsSortMode.None) as PlaceOnPath[];
            if (instances != null) {
                foreach (PlaceOnPath item in instances) {
                    item.Process();
                }
            }
        }

        #region PUBLIC

        public enum PathModes
        {
            MotionPath,
            Flyby,
            PathProvider
        }

        public PathModes PathMode = PathModes.MotionPath;
        public MotionPath MotionPath;
        public Flyby Flyby;
        public PathProvider PathProvider;

        public enum RelativeModes
        {
            FullPath,
            CurrentTimeOnPath,
            NodeOnPath
        }
        public RelativeModes RelativeMode = RelativeModes.FullPath;
        public MotionPathNode RelativeToNode;
        public bool UseWorldCoordinates;
        public float Time;
        public float Position;
        public int Marker;
        public bool WrapPosition;

        public float SmoothTime;
        public float SmoothTimeMax = 1f;
        public float RotationSmoothTime;

        public enum RotationModes
        {
            None,
            Interpolate,
            LookAhead,
            LookAt
        }
        public RotationModes RotationMode = RotationModes.LookAhead;

        public float LookAheadTime = 0.1f;
        public GameObject LookAtObject;
        public bool ApplyLookAheadToObject;

        public Vector3 Offset = Vector3.zero;
        public bool OffsetAfterRotation = true;
        public Vector3 Orientation = Vector3.zero;

        public bool LockPosX;
        public bool LockPosY;
        public bool LockPosZ;
        public Vector3 LockPosition = Vector3.zero;

        public bool LockRotX;
        public bool LockRotY;
        public bool LockRotZ;
        public Vector3 LockRotation = Vector3.zero;

        public DataChannel RotationChannel;

#endregion

        #region PRIVATE

        [NonSerialized]
        private Vector3 lookAheadStart = Vector3.zero;

        [NonSerialized]
        private Vector3 lookAheadEnd = Vector3.zero;

        [NonSerialized]
        private Vector3 pathLookAhead = Vector3.zero;

        [NonSerialized]
        private Vector3 lastPos = Vector3.zero;

        [NonSerialized]
        private Quaternion lastRot = Quaternion.identity;

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private Vector3 position = Vector3.zero;

        [NonSerialized]
        private Quaternion rotation = Quaternion.identity;

        #endregion

        #region ACCESSORS

        public bool IsRelative {
            get {
                return RelativeMode != RelativeModes.FullPath;
            }
        }

        #endregion

        #region SETUP

        public override void Refresh()
        {
            base.Refresh();
            Process();
        }

        protected override void OnAwake()
        {
            if (string.IsNullOrEmpty(Name)) Name = "MotionPath";
            base.OnAwake();
            UpdatePathMode();
        }

        public void UpdatePathMode()
        {
            if (PathMode == PathModes.MotionPath) {
                if (MotionPath == null) {
                    MotionPath = MotionPath.Primary;
                    if (MotionPath != null) {
                        /// On initial setup only
                        WrapPosition = MotionPath.IsPathClosed;
                    }
                }
                if (MotionPath != null) {
                    MotionPath.OnSetup.AddListener(OnPathUpdate);
                }
            }
            else
            if (PathMode == PathModes.Flyby) {
                if (Flyby == null) {
                    Flyby = UnityEngine.Object.FindFirstObjectByType<Flyby>();
                    if (Flyby != null) {
                        /// On initial setup only
                        WrapPosition = false;
                    }
                }
                if (Flyby != null) {
                    Flyby.OnSetup.AddListener(OnPathUpdate);
                }
            }
            else
            if (PathMode == PathModes.PathProvider) {
                if (PathProvider == null) {
                    PathProvider = UnityEngine.Object.FindFirstObjectByType<PathProvider>();
                    if (PathProvider != null) {
                        // On initial setup only
                        WrapPosition = false;
                    }
                }
                if (PathProvider != null) {
                    //PathProvider.OnSetup.AddListener(OnPathUpdate);
                }
            }
        }

        public void OnPathUpdate()
        {
            if (UpdateFrequency != UpdateFrequencies.Explicit) {
                Process();
            }
        }

        protected override void OnDestruct()
        {
            if (MotionPath != null) MotionPath.OnSetup.RemoveListener(Process);
            if (Flyby != null) Flyby.OnSetup.RemoveListener(Process);
            //if (PathProvider != null) PathProvider.OnSetup.RemoveListener(Process);
            if (RotationChannel != null) {
                base.RemoveChannel(RotationChannel);
            }
            base.OnDestruct();
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);

            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsEnabled = true;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.IsCombinedValue = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.ToProperty.Name = "Placed Position";
            }

            if (RotationChannel == null) {
                RotationChannel = new DataChannel(this);
            }
            if (RotationChannel.ToProperty == null) {
                RotationChannel.ToProperty = new Property();
            }
            RotationChannel.ToProperty.Owner = this;
            RotationChannel.ToProperty.IsEnabled = true;
            RotationChannel.ToProperty.IsDataOnly = true;
            RotationChannel.ToProperty.IsCombinedValue = true;
            RotationChannel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
            if (string.IsNullOrEmpty(RotationChannel.ToProperty.Name) || string.IsNullOrEmpty(RotationChannel.Name)) {
                RotationChannel.ToProperty.Name = "Placed Rotation";
            }
            RotationChannel.OnSetup(this);

            if (Channels == null) Channels = new List<TimeflowChannel>();
            Channels.Add(RotationChannel);
        }

        #endregion

        #region UPDATE

        public override void UpdateTime()
        {
            if (!CanUpdate) return;
            Process();
            base.UpdateTime();
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            //if (DebugEnabled) Debug.Log(name + ".PlaceOnPath.InterpolateVector3");
            if (channel == Channel) {
                if (UseWorldCoordinates) {
                    return position;
                }
                else {
                    return transform.localPosition;
                }
            }
            else {
                if (UseWorldCoordinates) {
                    return rotation.eulerAngles;
                }
                else {
                    return transform.localEulerAngles;
                }
            }
        }

        public void Process()
        {
            if (!Enabled) return;
            if (PathMode == PathModes.MotionPath) {
                ProcessMotionPath();
            }
            else
            if (PathMode == PathModes.Flyby) {
                ProcessFlyby();
            }
            else
            if (PathMode == PathModes.PathProvider) {
                ProcessPathProvider();
            }
        }

        private void ProcessLookAt()
        {
            if (LookAtObject != null) {
                Vector3 lookAt = LookAtObject.transform.position;
                if (lookAt != transform.position) {
                    transform.LookAt(lookAt);
                    rotation = transform.localRotation * Quaternion.Euler(Orientation);
                }
                else {
                    rotation = transform.localRotation;
                }
            }
        }

        private void ProcessLookAhead()
        {
            Vector3 lookAt = transform.parent == null ? pathLookAhead : transform.parent.TransformPoint(pathLookAhead);
            if (lookAt != transform.position) {
                transform.LookAt(lookAt);
                rotation = transform.localRotation * Quaternion.Euler(Orientation);
            }
            else {
                rotation = transform.localRotation;
            }

            if (ApplyLookAheadToObject && LookAtObject != null) {
                LookAtObject.transform.localPosition = pathLookAhead;
                LookAtObject.transform.localRotation = rotation;
            }
        }

        public void ProcessMotionPath()
        {
            if (Enabled && MotionPath != null && MotionPath.Channel != null && MotionPath.Duration > 0 &&
                MotionPath.Channel.Keys != null && MotionPath.Channel.Keys.Count > 0) {
                //if (DebugEnabled) Debug.Log(name + ".PlaceOnPath.Process:" + CurrentTime);

                float p = Position;
                if (RelativeMode == RelativeModes.CurrentTimeOnPath) {
                    p = MotionPath.CurrentInterpolation + p;
                }
                else
                if (RelativeMode == RelativeModes.NodeOnPath && RelativeToNode != null) {
                    if (MotionPath.Duration > 0) {
                        p += RelativeToNode.KeyTime / MotionPath.Duration;
                    }
                }

                p += Time / MotionPath.Duration;
                p = Wrap(p);

                // Scale normalized p to full path duration for seek position
                p *= MotionPath.Duration;

                Vector3 rEuler = Vector3.zero;
                Quaternion rQuat = Quaternion.identity;
                MotionPath.Channel.InterpolatePath(p, false, true, ref position, ref rEuler, ref rQuat, RotationMode == RotationModes.Interpolate, false, RotationMode == RotationModes.Interpolate);

                if (RotationMode == RotationModes.LookAhead) {
                    Quaternion r = Quaternion.identity;
                    pathLookAhead = MotionPath.Channel.InterpolatePath(p + LookAheadTime, false, true, null, false);
                    //if (DebugEnabled) Debug.Log(name + ".PlaceOnPath.Process:" + CurrentTime + " pathLookAhead:" + pathLookAhead + " pos:" + position);
                }

                ProcessPosition();

                if (RotationMode == RotationModes.LookAt) {
                    ProcessLookAt();
                }
                else
                if (RotationMode == RotationModes.LookAhead) {
                    ProcessLookAhead();
                }
                else
                if (RotationMode == RotationModes.Interpolate) {
                    rotation = Quaternion.Euler(rEuler) * Quaternion.Euler(Orientation);
                }
                else {
                    rotation = Quaternion.Euler(Orientation);
                }

                ProcessRotation();
            }
        }

        public void ProcessFlyby()
        {
            if (Enabled && Flyby != null && Flyby.FlybyChannel != null && Flyby.Duration > 0) {
                float p = Position;
                if (RelativeMode == RelativeModes.CurrentTimeOnPath) {
                    p = Flyby.Interpolate + p;
                }

                p += Time / Flyby.Duration;
                p = Wrap(p);

                // Scale normalized p to full path duration for seek position
                p *= Flyby.Duration;
                p += Flyby.FlybyStartTime;
                //if (DebugEnabled) Debug.Log(name + ".PlaceOnPath.Process:" + CurrentTime + " p:" + p);
                position = Flyby.FlybyChannel.InterpolateVector3(p, false, true, true);

                if (RotationMode == RotationModes.LookAhead) {
                    Quaternion r = Quaternion.identity;
                    pathLookAhead = Flyby.FlybyChannel.InterpolateVector3(p + LookAheadTime, false, true, true);
                    //if (DebugEnabled) Debug.Log(name + ".PlaceOnPath.Process:" + CurrentTime + " pathLookAhead:" + pathLookAhead + " pos:" + position);
                }

                ProcessPosition();

                if (RotationMode == RotationModes.LookAt) {
                    ProcessLookAt();
                }
                else
                if (RotationMode == RotationModes.LookAhead) {
                    ProcessLookAhead();
                }
                else
                if (RotationMode == RotationModes.Interpolate) {
                    rotation = Quaternion.Euler(Flyby.GetRotation(p)) * Quaternion.Euler(Orientation);
                }
                else {
                    rotation = Quaternion.Euler(Orientation);
                }

                ProcessRotation();
            }
        }

        public void ProcessPathProvider()
        {
            if (Enabled && PathProvider != null) {

                float p = Wrap(Position);
                PathProvider.Interpolate(p, out position, out rotation);

                if (RotationMode == RotationModes.LookAhead) {
                    Quaternion r = Quaternion.identity;
                    PathProvider.Interpolate(Wrap(p + LookAheadTime), out pathLookAhead, out r);
                    //if (DebugEnabled) Debug.Log(name + ".PlaceOnPath.Process:" + CurrentTime + " pathLookAhead:" + pathLookAhead + " pos:" + position);
                }

                ProcessPosition();

                if (RotationMode == RotationModes.LookAt) {
                    ProcessLookAt();
                }
                else
                if (RotationMode == RotationModes.LookAhead) {
                    ProcessLookAhead();
                }
                else
                if (RotationMode == RotationModes.Interpolate) {
                    rotation = rotation * Quaternion.Euler(Orientation);
                }
                else {
                    rotation = Quaternion.Euler(Orientation);
                }

                ProcessRotation();
            }
        }

        private void ProcessPosition()
        {
            if (LockPosX) {
                position.x = LockPosition.x;
            }
            else {
                LockPosition.x = position.x;
            }
            if (LockPosY) {
                position.y = LockPosition.y;
            }
            else {
                LockPosition.y = position.y;
            }
            if (LockPosZ) {
                position.z = LockPosition.z;
            }
            else {
                LockPosition.z = position.z;
            }

            if (!OffsetAfterRotation) {
                position += Offset;
            }

            if (SmoothTime > 0f && CurrentTime > lastTime && CurrentTime - lastTime < 1f) {
                float f = LocalDeltaTime / SmoothTime;
                position = MathUtil.Interpolate(lastPos, position, f);
            }
            if (!CalculateOnly) {
                if (UseWorldCoordinates) {
                    transform.position = position;
                }
                else {
                    transform.localPosition = position;
                }
            }

            Vector3 finalPos = UseWorldCoordinates ? transform.position : transform.localPosition;
            if (!CalculateOnly) {
                Channel.ToProperty.Vector3Value = finalPos;
                if (Channel.IsLinkEnabled) {
                    finalPos = Channel.Link.GetVector3(finalPos, Channel.WorldTime());
                }

                if (UseWorldCoordinates) {
                    transform.position = finalPos;
                }
                else {
                    transform.localPosition = finalPos;
                }
                Channel.ToProperty.Vector3Value = finalPos;
            }
            lastPos = position;
        }

        private void ProcessRotation()
        {
            Vector3 euler = rotation.eulerAngles;
            if (LockRotX) {
                euler.x = LockRotation.x;
            }
            else {
                LockRotation.x = euler.x;
            }
            if (LockRotY) {
                euler.y = LockRotation.y;
            }
            else {
                LockRotation.y = euler.y;
            }
            if (LockRotZ) {
                euler.z = LockRotation.z;
            }
            else {
                LockRotation.z = euler.z;
            }
            if (LockRotX || LockRotY || LockRotZ) {
                rotation = Quaternion.Euler(euler);
            }

            if (RotationSmoothTime > 0f && CurrentTime > lastTime && CurrentTime - lastTime < 1f) {
                float f = LocalDeltaTime / RotationSmoothTime;
                rotation = MathUtil.Interpolate(lastRot, rotation, f);
            }

            if (!CalculateOnly) {
                if (UseWorldCoordinates) {
                    transform.rotation = rotation;
                }
                else {
                    transform.localRotation = rotation;
                }
            }

            if (OffsetAfterRotation) {
                position += Offset;
                if (!CalculateOnly) {
                    if (UseWorldCoordinates) {
                        transform.position = position;
                    }
                    else {
                        transform.localPosition = position;
                    }
                }
            }

            Vector3 finalEul = UseWorldCoordinates ? transform.eulerAngles : transform.localEulerAngles;
            Quaternion finalRot = UseWorldCoordinates ? transform.rotation : transform.localRotation;

            if (!CalculateOnly) {
                RotationChannel.ToProperty.Vector3Value = finalEul;

                if (RotationChannel.IsLinkEnabled) {
                    finalEul = RotationChannel.Link.GetVector3(finalEul, Channel.WorldTime());
                }

                if (UseWorldCoordinates) {
                    transform.eulerAngles = finalEul;
                }
                else {
                    transform.localEulerAngles = finalEul;
                }
                RotationChannel.ToProperty.Vector3Value = finalEul;
            }

            lastTime = CurrentTime;
            lastRot = finalRot;
        }

        private float Wrap(float p)
        {
            if (WrapPosition) {
                p = MathUtil.Loop(p, 0f, 1f);
            }
            else {
                p = MathUtil.MinMax(p, 0f, 1f);
            }
            return p;
        }

        #endregion

#if UNITY_EDITOR

        public override Texture2D Icon => AxonUI.Icons.PlaceOnPath;

        public override void DrawGizmos()
        {
            if (Selection.activeGameObject == gameObject) {
                Handles.color = GUIColor;
                Handles.DotHandleCap(0, lookAheadStart, Quaternion.identity, HandleUtility.GetHandleSize(lookAheadStart) * 0.04f, EventType.Repaint);

                Handles.color = GUIColor;
                Handles.DotHandleCap(0, lookAheadEnd, Quaternion.identity, HandleUtility.GetHandleSize(lookAheadEnd) * 0.04f, EventType.Repaint);

                if (RotationMode == RotationModes.LookAhead && pathLookAhead != null) {
                    Handles.color = GUIColor;
                    Handles.SphereHandleCap(0, pathLookAhead, Quaternion.identity, HandleUtility.GetHandleSize(transform.position) * 0.15f, EventType.Repaint);
                }
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Place On Path"), false, GUIMenu_AddTween, null);
        }

        public static void GUIMenu_AddTween(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    PlaceOnPath place = Undo.AddComponent<PlaceOnPath>(obj.gameObject);
                    place.SetupChannels(true);
                    place.ResetName();

                    Timeflow.Active.View.SelectChannel(place.Channel);
                    Timeflow.Active.Refresh(true);
                }
            }
        }

#endif
    }

}//AxonGenesis