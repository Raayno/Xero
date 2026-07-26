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

namespace AxonGenesis
{
    /// <summary>
    /// This class stores additional data for each keyframe (node) on a Motion Path. This expands keyframes
    /// to provide a game object per each with precalculated values for interpolation.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/motion-path#motion-path-nodes")]
    sealed public class MotionPathNode : AxonGenesisBehavior
    {
        public float Distance;

        public float AutoTangentWeight = 1f;

        public bool UseEvents = false;
        public UnityEvent OnNodeTriggered = null;

        [SerializeField]
        private bool _IsAutoTangents = true;

#if AXON_DEVELOPMENT

        public float Velocity = 0f;
        public float VelocityTime = 0f;
        public float VelocityMidpoint = 0f;

#endif

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public MotionPath MotionPath = null;

        [NonSerialized]
        private Keyframe _Key = null;

        [NonSerialized]
        public MotionPathNode Next = null;

        [NonSerialized]
        public MotionPathNode Previous = null;

        #endregion

        #region ACCESSORS

        public bool IsAutoTangents {
            get {
                return _IsAutoTangents;
            }
            set {
                if (_IsAutoTangents != value) {
                    _IsAutoTangents = value;
                }
            }
        }

        public bool Locked {
            get {
                /// Only need value lock to determine node lock
                if (Key != null) return Key.LockValue;
                return false;
            }
            set {
                if (Key != null) {
                    /// Lock both time and value
                    Key.LockTime = Key.LockValue = value;
                }
            }
        }

        public Keyframe Key {
            get { return _Key; }
            set {
                if (_Key != value) {
                    _Key = value;
                    _Key.OnValueChanged += OnKeyValueChanged;
                }
            }
        }

        public float KeyTime {
            get {
                if (Key != null) return Key.KeyTime;
                return 0f;
            }
            set {
                if (Key != null) {
                    Key.KeyTime = value;
                }
            }
        }

        public Vector3 Position {
            get {
                if (Key.KeyVector3 != transform.localPosition) {
                    Key.KeyVector3 = transform.localPosition;
                }
                return transform.localPosition;
            }
            set {
                if (!Locked && transform.localPosition != value) {
                    Key.KeyVector3 = value;
                    transform.localPosition = value;
                }
            }
        }

        public Vector3 Euler {
            get {
                return Rotator.Euler;
            }
            set {
                if (!Locked && Rotator.Euler != value) {
                    Rotator.Euler = value;
                }
            }
        }

        public Quaternion Rotation {
            get {
                return Rotator.Rotation;
            }
            set {
                if (!Locked && Rotator.Rotation != value) {
                    Rotator.Rotation = value;
                }
            }
        }

        #endregion

        public void CalculateVelocity()
        {
            if (Key != null && Previous != null) {
                if (MotionPath.VelocityMode == MotionPath.VelocityModes.Fixed) {
                    float d = Mathf.Abs(Key.KeyTime - Previous.Key.KeyTime);
                    if (d > 0) {
                        Key.KeyValue = Previous.Key.KeyValue + (Distance / d);

                        bool l = Key.LockValue;
                        Key.LockValue = false;
                        Key.KeyValue = Key.KeyValue;
                        Key.LockValue = l;
                    }
                    //else {
                    //    Debug.LogWarning("Keyframes have same value as previous keyframe. d:" + d + " kt:" + Key.KeyTime + " - p:" + Previous.Key.KeyTime);
                    //}
                }
#if AXON_DEVELOPMENT
                else
                if (MotionPath.VelocityMode == MotionPath.VelocityModes.Flexible) {
                    // This code isn't working properly and needs to be fixed
                    float inTime = Previous.KeyTime;
                    float outTime = KeyTime;

                    VelocityTime = (inTime + outTime) / 2f;

                    float duration = (outTime - inTime);
                    if (Distance > 0f && duration > 0) {
                        // Calculate the triangular area 
                        Vector2 t1 = new Vector2(inTime, 0f);
                        Vector2 t2 = new Vector2(outTime, 0f);
                        Vector2 t3 = new Vector2(inTime, Previous.Key.KeyValue);
                        float areaA = MathUtil.AreaOfTriangle(t1, t2, t3);

                        Vector2 y1 = new Vector2(inTime, 0f);
                        Vector2 y2 = new Vector2(outTime, 0f);
                        Vector2 y3 = new Vector2(outTime, Key.KeyValue);
                        float areaB = MathUtil.AreaOfTriangle(y1, y2, y3);

                        float yAvg = (Previous.Key.KeyValue + Key.KeyValue) / 2f;

                        float total = areaA + areaB;
                        float gap = Distance - total;
                        float h = ((gap * 2f) / duration) + yAvg;
                        VelocityMidpoint = h;
                    }
                    else {
                        VelocityMidpoint = 0f;
                    }
                }
#endif
            }
        }


        private void OnKeyValueChanged()
        {
            if (Key == null) return;
            if (MotionPath == null || !MotionPath.IsInitialized) {
                //Debug.Log("OnKeyValueChanged Not Init");
                return;
            }
            if (transform.localPosition != Key.KeyVector3) {
                //Debug.Log("OnKeyValueChanged");
                transform.localPosition = Key.KeyVector3;
                MotionPath.Refresh();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Key != null) {
                _Key.OnValueChanged -= OnKeyValueChanged;
                _Key.OnValueChanged += OnKeyValueChanged;
            }
        }

        protected override void OnDisable()
        {
            if (Key == null) return;
            Key.OnValueChanged -= OnKeyValueChanged;
            base.OnDisable();
        }

#if UNITY_EDITOR

        public bool EditorShowDetails;
        public bool EditorShowKeyframeDetails;
        public bool _IsSelected;

        public override bool Enabled {
            get {
                return Key == null || Key.IsKeyEnabled;
            }
            set {
                if (Key != null) {
                    Key.IsKeyEnabled = value;
                }
            }
        }
        public override bool ShowSelected { get { return false; } }

        /// <summary>
        /// This is a virtual method allowing objects to implement selection behavior.
        /// </summary>
        public override bool IsSelected {
            get {
                return _IsSelected && Enabled;
            }
            set {
                if (_IsSelected != value) {
                    _IsSelected = value;
                }
            }
        }

        public bool ShowTangents {
            get {
                if (Key != null) return Key.ShowTangents;
                return true;
            }
            set {
                if (Key != null) {
                    Key.ShowTangents = value;
                }
            }
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            MotionPath.CustomDrawGizmos(MotionPath);
        }

#endif
    }

    public class SortMotionPathNode : IComparer<MotionPathNode>
    {
        public int Compare(MotionPathNode a, MotionPathNode b)
        {
            int c = 0;

            if(a == null || b == null) {
                return c;
            }
            else
            if (a.KeyTime < b.KeyTime) {
                c = -1;
            }
            else
            if (a.KeyTime > b.KeyTime) {
                c = 1;
            }

            return c;
        }
    }

}//AxonGenesis
