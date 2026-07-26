// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Apply this component to have this object look at or follow another object. This can be used to
    /// virtually attach one object to another, or with delay for fluid following movement. 
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Follow")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/follow")]
    sealed public class Follow : TimeflowDataBehavior
    {
        #region PUBLIC

        public GameObject ObjectToFollow;

        public bool EnablePosition = true;
        public bool EnablePositionX = true;
        public bool EnablePositionY = true;
        public bool EnablePositionZ = true;

        public bool LimitPosition;
        public bool LimitPositionX;
        public bool LimitPositionY;
        public bool LimitPositionZ;

        public Vector3 PostionMin = Vector3.zero;
        public Vector3 PositionMax = Vector3.zero;
        public Vector3 TargetOffset = Vector3.zero;
        public bool TargetOffsetWorld;

        public enum Modes
        {
            Direct,
            Lerp,
            SmoothDamp,
            SmoothApproach,
            LerpLocalAxis,
            Physics
        }
        public Modes Mode = Modes.Direct;
        public Modes EditorMode = Modes.Direct;

        public float TargetDistance;
        public float ApproachSpeed = 10f;
        public float SmoothSeconds = 1f;
        public float SmoothMax = 1f;
        public float RotationSmoothTime;
        public float StartAtTime;
        public Vector3 AxisLerpSeconds = Vector3.zero;

        public enum StartModes
        {
            None,
            Original,
            Set,
            OnTarget
        }
        public StartModes StartPosition = StartModes.None;
        public Vector3 StartAtPosition = Vector3.zero;

        public StartModes StartRotation = StartModes.None;
        public Vector3 StartAtRotation = Vector3.zero;

        public enum RotationModes
        {
            None,
            MatchRotation,
            LookAtObject,
            DirectionOfMovement
        }
        public RotationModes RotationMode = RotationModes.None;

        public bool EnableRotationX = true;
        public bool EnableRotationY = true;
        public bool EnableRotationZ = true;

        public bool LimitRotation;
        public Vector3 RotationMin = Vector3.zero;
        public Vector3 RotationMax = Vector3.zero;

        public bool LimitDistance;
        public float MinDistance;
        public float MaxDistance = 100f;

        public bool LimitVelocity;
        public float MaxVelocity = 1f;
        public bool LimitAngularVelocity;
        public float MaxAngularVelocity = 1f;

        public GameObject LookAtObject;
        public Vector3 UpVector = Vector3.up;
        public Vector3 Orientation = Vector3.zero;
        public float ForceCloseGap;

        public float OverallBlend = 1f;

        public ForceMode Force = ForceMode.Force;
        public ForceMode AngularForce = ForceMode.Force;
        public bool ApplyToRigidbody;
        public bool UseAngularForce;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private Vector3 lastPos = Vector3.zero;

        [NonSerialized]
        private Quaternion lastRot = Quaternion.identity;

        [NonSerialized]
        private Vector3 origLocalPos = Vector3.zero;

        [NonSerialized]
        private Quaternion origLocalRot = Quaternion.identity;

        [NonSerialized]
        private Vector3 velocity = Vector3.zero;

        [NonSerialized]
        private uint frameID;

        [NonSerialized]
        private RigidbodyHelper body;

        [NonSerialized]
        private float distanceRange;

        #endregion

        #region ACCESSORS

        public RigidbodyHelper Body {
            get {
                if (body == null) {
                    body = new RigidbodyHelper(gameObject);
                }
                return body;
            }
        }

        public Modes CurrentMode {
            get {
                /// Only use the fallback in the editor when physics is used
                if (Application.isPlaying || Mode != Modes.Physics) {
                    return Mode;
                }
                return EditorMode;
            }
        }

        public bool UsePhysics {
            get {
                return Mode == Modes.Physics;
            }
        }

        public bool EnableRotation {
            get {
                return RotationMode != RotationModes.None;
            }
        }

        public bool MatchRotation {
            get {
                return RotationMode == RotationModes.MatchRotation;
            }
        }

        public bool EnableLookAt {
            get {
                return RotationMode == RotationModes.LookAtObject && LookAtObject != null;
            }
        }

        public bool EnableDirection {
            get {
                return RotationMode == RotationModes.DirectionOfMovement;
            }
        }

        public Vector3 TargetPosition {
            get {
                Vector3 p = transform.position;
                Vector3 target = TargetOffsetWorld ? TranslatePosition(p, ObjectToFollow.transform.position + TargetOffset) : TranslatePosition(p, ObjectToFollow.transform.TransformPoint(TargetOffset));
                if (TargetDistance > 0f) {
                    if (target == p) {
                        return target;
                    }
                    else {
                        // Calculate and scale vector
                        Vector3 v = p - target;
                        v = v.normalized * TargetDistance;
                        return target + v;
                    }
                }
                return target;
            }
        }

        #endregion

        #region SETUP

        public override void Refresh()
        {
            base.Refresh();
            OnRestart();
            Calculate(true);
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            origLocalPos = transform.localPosition;
            origLocalRot = transform.localRotation;
            //if (DebugEnabled) Debug.Log(name + ".Follow.OnAwake: origLocalPos:" + origLocalPos);
        }

        protected override void OnStart()
        {
            base.OnStart();
            //if (DebugEnabled) Debug.Log(name + ".Follow.OnStart:" + transform.position);
            OnRestart();
        }

        public void SetupPhysics()
        {
            if (!Body.HasBody) {
                Body.AddRigidbody(gameObject);
                Body.useGravity = false;
            }
            ObjectUtil.SetupBoxCollider(gameObject);
        }

        public void OnRestart()
        {
            if (EnablePosition && StartPosition != StartModes.None) {
                if (ApplyToRigidbody && Body != null) {
                    Body.velocity = Vector3.zero;
                }
                if (StartPosition == StartModes.Set) {
                    transform.localPosition = StartAtPosition;
                }
                else
                if (StartPosition == StartModes.OnTarget && ObjectToFollow != null) {
                    transform.position = TargetPosition;
                }
                else {
                    transform.localPosition = origLocalPos;
                }
            }

            if (EnableRotation && StartRotation != StartModes.None) {
                if (ApplyToRigidbody && Body != null) {
                    Body.angularVelocity = Vector3.zero;
                }
                if (StartRotation == StartModes.Set) {
                    transform.localEulerAngles = StartAtRotation;
                }
                else
                if (StartRotation == StartModes.OnTarget && ObjectToFollow != null) {
                    if (EnableLookAt) {
                        transform.LookAt(LookAtObject.transform.position, UpVector);
                        transform.rotation = transform.rotation * Quaternion.Euler(Orientation);
                    }
                    else
                    if (MatchRotation) {
                        transform.rotation = ObjectToFollow.transform.rotation;
                    }
                }
                else {
                    transform.localRotation = origLocalRot;
                }
            }
        }

        public override void OnRewind()
        {
            base.OnRewind();
            //if (DebugEnabled) Debug.Log(name + ".OnRewind");
            OnRestart();
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            Channel.PropertyType = Property.PropertyTypes.Vector3;
            Channel.Attribute = -1;
            Channel.IsCombinedValue = true;
            Channel.ToProperty.CanBeAssigned = false;
            Channel.ToProperty.EnableHandlers = false;
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
            Channel.ToProperty.IsCombinedValue = true;
            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Follow";
            }
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false); // base takes care of majority of properties
            //if (DebugEnabled) Debug.Log(name + ".Follow.Copy:" + src.name);

            SetupPhysics();
            SetupChannels(true);
        }

        #endregion

        #region TRANSFORM METHDOS

        /// <summary>
        /// This handles assigning a position value while maintaining original values for any disabled
        /// axis. Only enabled axis values are assigned.
        /// </summary>
        /// <param name="from">The original position</param>
        /// <param name="to">The desired target position</param>
        private Vector3 TranslatePosition(Vector3 from, Vector3 to)
        {
            if (EnablePositionX) {
                from.x = to.x;
            }
            if (EnablePositionY) {
                from.y = to.y;
            }
            if (EnablePositionZ) {
                from.z = to.z;
            }
            return from;
        }

        public void SetPosition(Vector3 pos)
        {
            if (Application.isPlaying && ApplyToRigidbody && Body != null) {
                Body.MovePosition(pos);
            }
            else {
                transform.position = pos;
            }
        }

        public void SetRotation(Vector3 rot)
        {
            if (Application.isPlaying && ApplyToRigidbody && Body != null) {
                Body.MoveRotation(Quaternion.Euler(rot));
            }
            else {
                transform.eulerAngles = rot;
            }
        }

        public void SetRotation(Quaternion rot)
        {
            if (Application.isPlaying && ApplyToRigidbody && Body != null) {
                Body.MoveRotation(rot);
            }
            else {
                transform.rotation = rot;
            }
        }

        public bool IsOutOfRange(Vector3 pos)
        {
            distanceRange = LimitDistance ? Vector3.Distance(pos, transform.position) : 0f;
            return LimitDistance && (distanceRange < MinDistance || distanceRange > MaxDistance);
        }

        public bool CanCalculate {
            get {
                return Enabled && Timeflow != null && OverallBlend > 0f && ObjectToFollow != null;
            }
        }

        #endregion

        #region UPDATE

        public override void OnFixedUpdate()
        {
            // Only process once per frame
            if (Timeflow.FrameID == frameID) return;
            frameID = Timeflow.FrameID;

            if (!CanCalculate || !UsePhysics || Body == null) return;
            Vector3 oPos = transform.position;
            Vector3 pos = TargetPosition;
            if (IsOutOfRange(pos)) return;
            if (!Application.isPlaying) {
                // Since physics doesn't operate in edit mode use alternate preview
                if (EnablePosition) {
                    SetPosition(pos);
                }
                if (MatchRotation) {
                    SetRotation(ObjectToFollow.transform.rotation * Quaternion.Euler(Orientation));
                }
            }
            else {
                if (EnablePosition) {
                    if (CurrentMode == Modes.Direct) {
                        Body.MovePosition(pos);
                    }
                    else {
                        Vector3 force = pos - transform.position;
                        if (LimitVelocity) {
                            if (force.magnitude > MaxVelocity) {
                                force = force.normalized * MaxVelocity;
                            }
                        }
                        Body.AddForce(force, Force);
                    }

                    if (LimitPosition) {
                        Vector3 p = pos;
                        if (LimitPositionX) {
                            pos.x = MathUtil.MinMax(pos.x, PostionMin.x, PositionMax.x);
                        }
                        if (LimitPositionY) {
                            pos.y = MathUtil.MinMax(pos.y, PostionMin.y, PositionMax.y);
                        }
                        if (LimitPositionZ) {
                            pos.z = MathUtil.MinMax(pos.z, PostionMin.z, PositionMax.z);
                        }
                        if (p != pos) {
                            Body.MovePosition(pos);
                        }
                    }
                }
                if (EnableRotation) {
                    if (!UseAngularForce) {
                        CalculateRotation(pos, oPos);
                    }
                    else {
                        if (CurrentMode == Modes.Direct) {
                            Body.MoveRotation(ObjectToFollow.transform.rotation);
                        }
                        if (RotationMode == RotationModes.MatchRotation) {
                            Vector3 force = ObjectToFollow.transform.eulerAngles - transform.eulerAngles;
                            if (LimitAngularVelocity) {
                                if (force.magnitude > MaxAngularVelocity) {
                                    force = force.normalized * MaxAngularVelocity;
                                }
                            }
                            Body.AddTorque(force, AngularForce);
                        }
                        else
                        if (EnableLookAt) {
                            Quaternion oRot = transform.rotation;
                            transform.LookAt(LookAtObject.transform.position, UpVector);
                            Quaternion look = transform.rotation * Quaternion.Euler(Orientation);
                            transform.rotation = oRot;
                            Body.MoveRotation(look);
                        }
                        if (LimitRotation) {
                            Quaternion q = transform.rotation;
                            q.eulerAngles = MathUtil.Clamp(q.eulerAngles, RotationMin, RotationMax);
                            Body.MoveRotation(q);
                        }
                    }
                }
            }
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            base.UpdateTime();
            if (CurrentMode != Modes.Physics || !Application.isPlaying) {
                //if (DebugEnabled) Debug.Log(name + ".InterpolateVector3:" + time + " apply:" + apply);
                Calculate();
            }
            return lastPos;
        }

        #endregion

        #region CALCULATE

        public void Calculate(bool force = false)
        {
            // Only process once per frame. This prevents duplicate calls that can occur with certain editor functions.
            if (!force && Timeflow.FrameID == frameID) return;
            frameID = Timeflow.FrameID;

            if (!CanCalculate) return;
            if (UsePhysics && Application.isPlaying) return; // handled in late update above

            Vector3 oPos = transform.position;
            Vector3 pos = transform.position;
            Vector3 targetPos = TargetPosition;
            if (IsOutOfRange(targetPos)) return;

            if (Timeflow != null && !Timeflow.IsPlaying) {
                /// force full update when not playing
                force = true;
            }

            //if (DebugEnabled) Debug.Log(name + ".Follow.Calculate: " + frameID + " LocalDeltaTime:" + LocalDeltaTime);
            if (EnablePosition) {

                if (CurrentMode == Modes.Direct || force || LocalDeltaTime == 0f) {
                    pos = TranslatePosition(pos, targetPos);
                    //if (DebugEnabled) Debug.Log("Direct:" + targetPos + " force:" + force);
                }
                else {
                    if (CurrentMode == Modes.SmoothDamp) {
                        if (SmoothSeconds > 0) {
                            Vector3 sd = Vector3.SmoothDamp(pos, targetPos, ref velocity, SmoothSeconds, LimitVelocity ? MaxVelocity : Mathf.Infinity, LocalDeltaTime);
                            pos = TranslatePosition(pos, sd);
                        }
                    }
                    else
                    if (CurrentMode == Modes.Lerp) {
                        if (SmoothSeconds > 0) {
                            float interp = LocalDeltaTime / SmoothSeconds;
                            pos = MathUtil.Interpolate(pos, targetPos, interp);
                        }
                    }
                    else
                    if (CurrentMode == Modes.LerpLocalAxis) {
                        Vector3 local = ObjectToFollow.transform.InverseTransformPoint(pos);
                        if (AxisLerpSeconds.x <= 0f) {
                            local.x = 0f;
                        }
                        else {
                            local.x = MathUtil.Interpolate(local.x, 0f, LocalDeltaTime / AxisLerpSeconds.x);
                        }
                        if (AxisLerpSeconds.y <= 0f) {
                            local.y = 0f;
                        }
                        else {
                            local.y = MathUtil.Interpolate(local.y, 0f, LocalDeltaTime / AxisLerpSeconds.y);
                        }
                        if (AxisLerpSeconds.z <= 0f) {
                            local.z = 0f;
                        }
                        else {
                            local.z = MathUtil.Interpolate(local.z, 0f, LocalDeltaTime / AxisLerpSeconds.z);
                        }
                        pos = ObjectToFollow.transform.TransformPoint(local);
                    }
                    else
                    if (CurrentMode == Modes.SmoothApproach) {
                        pos = MathUtil.SmoothApproach(pos, lastPos, targetPos, LimitVelocity ? Mathf.Min(MaxVelocity, ApproachSpeed) : ApproachSpeed);
                    }
                    else {
                        pos = TranslatePosition(pos, targetPos);
                    }
                }

                if (LimitVelocity) {
                    Vector3 vel = pos - transform.position;
                    if (vel.magnitude > MaxVelocity) {
                        pos = transform.position + (vel.normalized * MaxVelocity);
                    }
                }
                if (LimitPosition) {
                    if (LimitPositionX) {
                        pos.x = MathUtil.MinMax(pos.x, PostionMin.x, PositionMax.x);
                    }
                    if (LimitPositionY) {
                        pos.y = MathUtil.MinMax(pos.y, PostionMin.y, PositionMax.y);
                    }
                    if (LimitPositionZ) {
                        pos.z = MathUtil.MinMax(pos.z, PostionMin.z, PositionMax.z);
                    }
                }
                if (ForceCloseGap > 0f) {
                    Vector3 fp = MathUtil.Interpolate(pos, targetPos, ForceCloseGap);
                    pos = TranslatePosition(pos, fp);
                }
                if (OverallBlend < 1f) pos = MathUtil.Interpolate(oPos, pos, OverallBlend);

                if (!EnablePositionX) pos.x = oPos.x;
                if (!EnablePositionY) pos.y = oPos.y;
                if (!EnablePositionZ) pos.z = oPos.z;

                SetPosition(pos);
                lastPos = pos;

            }

            CalculateRotation(pos, oPos);
        }

        public void CalculateRotation(Vector3 pos, Vector3 oPos)
        {
            if (EnableRotation) {
                Quaternion oRot = transform.rotation;
                Quaternion rot = transform.rotation;
                Quaternion targetRot = transform.rotation;
                if (MatchRotation) {
                    targetRot = ObjectToFollow.transform.rotation * Quaternion.Euler(Orientation);
                }
                else
                if (EnableLookAt) {
                    transform.LookAt(LookAtObject.transform.position, UpVector);
                    targetRot = transform.rotation * Quaternion.Euler(Orientation);
                }
                else
                if (EnableDirection) {
                    Vector3 look = pos - oPos;
                    if (look != Vector3.zero) {
                        targetRot = Quaternion.LookRotation(look, UpVector);
                    }
                }

                if (RotationSmoothTime > 0) {
                    rot = MathUtil.Interpolate(rot, targetRot, LocalDeltaTime / RotationSmoothTime);
                }
                else {
                    rot = targetRot;
                }

                if (LimitRotation) {
                    rot.eulerAngles = MathUtil.Clamp(rot.eulerAngles, RotationMin, RotationMax);
                }

                if (ForceCloseGap > 0f) {
                    rot = MathUtil.Interpolate(rot, targetRot, ForceCloseGap);
                }
                if (OverallBlend < 1f) rot = MathUtil.Interpolate(oRot, rot, OverallBlend);

                if (!EnableRotationX || !EnableRotationY || !EnableRotationZ) {
                    //TODO: Is there a better way to lock rotation axis?
                    // Less than ideal and may cause unpredictable rotations
                    Vector3 euler = rot.eulerAngles;
                    Vector3 oEuler = oRot.eulerAngles;
                    if (!EnableRotationX) euler.x = oEuler.x;
                    if (!EnableRotationY) euler.y = oEuler.y;
                    if (!EnableRotationZ) euler.z = oEuler.z;
                    rot = Quaternion.Euler(euler);
                }

                SetRotation(rot);
                lastRot = rot;
            }
        }

        #endregion

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.Follow;

        public override void ResetName()
        {
            if (_Channel == null) return;
            _Channel.Name = "Follow";
            if (_Channel.ToProperty != null) _Channel.ToProperty.Name = _Channel.Name;
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Follow"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Follow comp = Undo.AddComponent<Follow>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif

    }


    public class TimeflowContextMenuFollowType
    {
        public Type FollowType = null;
        public bool InHierarchy = false;
    }

}//AxonGenesis
