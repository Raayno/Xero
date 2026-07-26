// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{

    [Serializable]
    public class TransformOverrideData
    {
        private static TransformOverrideGroup _sharedPosition;
        private static TransformOverrideGroup _sharedRotation;
        private static TransformOverrideGroup _sharedScale;

        public bool IsLocked;
        public bool IsUniformScale;
        public bool HasKeyframer;
        public bool HasMotionPath;
        public bool HasFlyby;
        public Vector3 EulerAngles;

        [NonSerialized] private Transform _Transform = null;
        [SerializeField] private TransformOverrideGroup _Position;
        [SerializeField] private TransformOverrideGroup _Rotation;
        [SerializeField] private TransformOverrideGroup _Scale;

        [NonSerialized] public bool IsTimeflowObject = false;
        [NonSerialized] public TimeflowObject TimeflowObject = null;
        [NonSerialized] public Rotator Rotator = null;
        [NonSerialized] public Keyframer Keyframer = null;
        [NonSerialized] public MotionPath MotionPath = null;
        [NonSerialized] public Flyby Flyby = null;
        [NonSerialized] private TransformOverrideGroup[] _Groups = null;

        [NonSerialized] public bool WantsRefresh = false;

        public bool CanAnimate => !HasMotionPath && !HasFlyby;

        public Transform Transform {
            get { return _Transform; }
            set {
                if (_Transform != value) {
                    _Transform = value;
                }
            }
        }

        public bool IsLocal {
            get { return Tools.pivotRotation == PivotRotation.Local; }
            set {
                if (value) { Tools.pivotRotation = PivotRotation.Local; }
                else { Tools.pivotRotation = PivotRotation.Global; }
            }
        }

        private TransformOverrideGroup sharedPosition {
            get {
                if (_sharedPosition == null) {
                    _sharedPosition = new TransformOverrideGroup(TransformOverrideGroup.Types.Position);
                }
                return _sharedPosition;
            }
        }

        private TransformOverrideGroup sharedRotation {
            get {
                if (_sharedRotation == null) {
                    _sharedRotation = new TransformOverrideGroup(TransformOverrideGroup.Types.Rotation);
                }
                return _sharedRotation;
            }
        }

        private TransformOverrideGroup sharedScale {
            get {
                if (_sharedScale == null) {
                    _sharedScale = new TransformOverrideGroup(TransformOverrideGroup.Types.Scale);
                }
                return _sharedScale;
            }
        }

        public TransformOverrideGroup Position {
            get {
                if (!IsTimeflowObject) {
                    return sharedPosition;
                }
                return _Position;
            }
            set { _Position = value; }
        }

        public TransformOverrideGroup Rotation {
            get {
                if (!IsTimeflowObject) {
                    return sharedRotation;
                }
                return _Rotation;
            }
            set { _Rotation = value; }
        }

        public TransformOverrideGroup Scale {
            get {
                if (!IsTimeflowObject) {
                    return sharedScale;
                }
                return _Scale;
            }
            set { _Scale = value; }
        }

        public TransformOverrideGroup[] Groups {
            get {
                if (_Groups == null) {
                    _Groups = new TransformOverrideGroup[3];
                    _Groups[0] = Position;
                    _Groups[1] = Rotation;
                    _Groups[2] = Scale;
                }
                return _Groups;
            }
        }

        public TransformOverrideData(Transform t)
        {
            Transform = t;
            //Debug.Log($"TransformOverrideData:{Transform.name}");
            IsLocked = false;
            IsUniformScale = false;
            HasKeyframer = false;
            HasMotionPath = false;
            HasFlyby = false;

            InstantiateGroups(true);

            Refresh();
        }

        public TransformOverrideData(TransformOverrideData data)
        {
            Transform = data.Transform;
            //Debug.Log($"TransformOverrideData:{Transform.name}");

            IsLocked = data.IsLocked;
            IsUniformScale = data.IsUniformScale;
            HasKeyframer = data.HasKeyframer;
            HasMotionPath = data.HasMotionPath;
            HasFlyby = data.HasFlyby;
            EulerAngles = data.EulerAngles;

            _sharedPosition = data.Position;
            _sharedRotation = data.Rotation;
            _sharedScale = data.Scale;

            InstantiateGroups(true);

            Refresh();
        }

        public void Refresh(Transform transform = null)
        {
            WantsRefresh = false;
            //Debug.Log($"Refresh");
            if (transform != null) Transform = transform;
            if (Transform == null) {
                Debug.LogWarning($"Transform is null");
                return;
            }

            ResetEulerAngles();
            InstantiateGroups();

            Transform.TryGetComponent<Rotator>(out Rotator);
            Transform.TryGetComponent<TimeflowObject>(out TimeflowObject);

            IsTimeflowObject = TimeflowObject != null;
            if (IsTimeflowObject) {
                Transform.TryGetComponent<Keyframer>(out Keyframer);
                HasKeyframer = Keyframer != null;

                Transform.TryGetComponent<MotionPath>(out MotionPath);
                HasMotionPath = MotionPath != null;

                Transform.TryGetComponent<Flyby>(out Flyby);
                HasFlyby = Flyby != null;

                Position.HasAnimationConflict = HasMotionPath || HasFlyby;

                RemapChannels();
            }
        }

        private void OnChannelDestroyed()
        {
            WantsRefresh = true;
        }

        public void ResetEulerAngles()
        {
            if (Tools.pivotRotation == PivotRotation.Global) {
                EulerAngles = Transform.eulerAngles;
            }
            else {
                EulerAngles = Transform.localEulerAngles;
            }
            //Debug.Log($"ResetEulerAngles:{EulerAngles}");
        }

        private void InstantiateGroups(bool create = false)
        {
            if (create || _Position == null) _Position = new TransformOverrideGroup(TransformOverrideGroup.Types.Position, _sharedPosition);
            if (create || _Rotation == null) _Rotation = new TransformOverrideGroup(TransformOverrideGroup.Types.Rotation, _sharedRotation);
            if (create || _Scale == null) _Scale = new TransformOverrideGroup(TransformOverrideGroup.Types.Scale, _sharedScale);

            _Position.Type = TransformOverrideGroup.Types.Position;
            _Rotation.Type = TransformOverrideGroup.Types.Rotation;
            _Scale.Type = TransformOverrideGroup.Types.Scale;

            sharedPosition.Type = TransformOverrideGroup.Types.Position;
            sharedRotation.Type = TransformOverrideGroup.Types.Rotation;
            sharedScale.Type = TransformOverrideGroup.Types.Scale;

            Position.Refresh();
            Rotation.Refresh();
            Scale.Refresh();

            // Check and fix any collapsed min-max values
            if (Position.Min == Vector3.zero && Position.Max == Vector3.zero) {
                Position.Reset();
            }
            if (Rotation.Min == Vector3.zero && Rotation.Max == Vector3.zero) {
                Rotation.Reset();
            }
            if (Scale.Min == Vector3.zero && Scale.Max == Vector3.zero) {
                Scale.Reset();
            }
        }

        private void RemapChannels()
        {
            if (TimeflowObject.AllChannels == null) return;
            foreach (TimeflowChannel ch in TimeflowObject.AllChannels) {
                if (ch == null || ch.ToProperty == null || ch.IsTrack) continue;
                //Debug.Log($"ch:{ch.Name}");

                // Associate the channels with the data
                if (!MapGroupToChannel("Position", Position, ch)) {
                    if (!MapGroupToChannel("Rotation", Rotation, ch)) {
                        MapGroupToChannel("Scale", Scale, ch);
                    }
                }
            }
        }

        private bool MapGroupToChannel(string name, TransformOverrideGroup group, TimeflowChannel ch)
        {
            bool isMapped = false;
            if (ch.ToProperty.Name == name || ch.ToProperty.Name == "Local " + name || ch.ToProperty.Name == "World " + name) {
                group.IsSeparate = ch.ToProperty.Attribute > -1;
                if (ch.ToProperty.Attribute == -1) {
                    group.XYZ = ch;
                    isMapped = true;
                }
                else
                if (ch.ToProperty.Attribute == 0) {
                    group.X = ch;
                    isMapped = true;
                }
                else
                if (ch.ToProperty.Attribute == 1) {
                    group.Y = ch;
                    isMapped = true;
                }
                else
                if (ch.ToProperty.Attribute == 2) {
                    group.Z = ch;
                    isMapped = true;
                }

                if (isMapped) {
                    ch.OnDestruct += () => { OnChannelDestroyed(); group.Refresh(); };
                }
            }

#if !TIMEFLOW_OVERRIDES_DISABLED
            TransformOverride.UpdateGroupAutoKeyframing(group);
#endif
            return isMapped;
        }

    }
}
#endif