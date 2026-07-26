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

using Random = UnityEngine.Random;

namespace AxonGenesis
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "MotionPathChannel")]
    sealed public class MotionPathChannel : TimeflowChannel
    {
        public enum PathInterpolations
        {
            Hold,
            Linear,
            Bezier
        }
        public PathInterpolations PathInterpolation = PathInterpolations.Bezier;

#if UNITY_EDITOR
        public bool ShowPathHandles = true;
#endif
        [NonSerialized]
        public MotionPath MotionPath;

        #region PRIVATE

        [NonSerialized]
        private float startTime;

        [NonSerialized]
        private float endTime;

        [NonSerialized]
        private Vector3 currentEuler = Vector3.zero;

        [NonSerialized]
        private Quaternion currentRotation = Quaternion.identity;

        #endregion

        public MotionPathChannel(MotionPath parent) : base(parent)
        {
            Behavior = parent;
            ToProperty = null;
            MotionPath = parent;
            ClearKeys(false);
            PropertyType = Property.PropertyTypes.Vector3;

#if UNITY_EDITOR
            GUIColor = new Color(Random.value, Random.value, Random.value, 1f);
            if (EditorGUIUtility.isProSkin) {
                GUIColor = new Color(Mathf.Min(1f, GUIColor.r * 1.5f), Mathf.Min(1f, GUIColor.g * 1.5f), Mathf.Min(1f, GUIColor.b * 1.5f));
            }
            else {
                GUIColor = new Color(Mathf.Min(0.9f, GUIColor.r), Mathf.Min(0.9f, GUIColor.g), Mathf.Min(0.9f, GUIColor.b));
            }
#endif
        }

        public Quaternion CurrentRotation {
            get {
                return currentRotation;
            }
        }

        public Vector3 CurrentEuler {
            get {
                return currentEuler;
            }
        }

        public override Vector4 CurrentVector {
            get {
                return _currentVector;
            }
            set {
                if (_currentVector != value) {
                    _currentVector = value;
                }
            }
        }

        public override bool IsEnabled {
            get {
                if (MotionPath != null) {
                    bool enabled = MotionPath.Enabled;
                    if (ToProperty != null) enabled &= ToProperty.IsEnabled;
                    return enabled;
                }
                return _IsEnabled;
            }
            set {
                if (MotionPath != null) {
                    MotionPath.Enabled = value;
                    if (ToProperty != null) ToProperty.IsEnabled = value;
                }
            }
        }

        public override Property.PropertyTypes GetPropertyType()
        {
            PropertyType = Property.PropertyTypes.Vector3;
            if (HasProperty) {
                ShowValue = true; // required to show velocity value curve in graph view
            }
            return PropertyType;
        }

        public override Property.PropertyTypes KeyPropertyType {
            get {
                return Property.PropertyTypes.Vector3;
            }
            set {
                // Unimplemented but can be overridden if needed
            }
        }

        public override Keyframe SetKey(float time) { return SetKey(time, 0f, true); }

        public override Keyframe SetKey(float time, float endTime, bool isLocalTime)
        {
            if (!IsEnabled || IsLocked) return null; // don't set new keyframes on locked or disabled channels
            return SetKeyVector(LocalTime(time, isLocalTime), Behavior.transform.localPosition, true);
        }

        public override Keyframe SetKeyValue(float localTime, float value)
        {
            if (!IsEnabled || IsLocked) return null; // don't set new keyframes on locked or disabled channels
            if (!CanSetKey()) return null;
            return SetKeyValue(localTime, value);
        }

        public override Keyframe SetKeyVector(float localTime, Vector4 value)
        {
            /// This is a bit of a hack to add more logic to the base class override method
            return SetKeyVector(localTime, value, false);
        }

        public Keyframe SetKeyVector(float localTime, Vector4 value, bool isValueValid)
        {
            if (!IsEnabled || IsLocked) return null; // don't set new keyframes on locked or disabled channels
            if (!CanSetKey()) return null;

            if (!isValueValid) {
                /// Retrieve a new value since the input is unreliable for this channel type due to the use
                /// of velocity in the graph display.
                value = MotionPath.InterpolatePath(localTime, true);
            }

            MotionPathNode node = MotionPath.AddNode(localTime);
            if (node != null) {
                node.Position = value;
                MotionPath.Refresh();
                return node.Key;
            }

            return null;
        }

        public override void Copy(TimeflowChannel src, bool includeStyle = true)
        {
            base.Copy(src);

            MotionPathChannel ch = (MotionPathChannel)src;
            if (ch != null) {
                // Parent and MotionPath references are set on initialization
#if UNITY_EDITOR
                if (includeStyle) {
                    ShowPathHandles = ch.ShowPathHandles;
                }
#endif
                PathInterpolation = ch.PathInterpolation;
            }
        }

        public override Keyframe CopyKey(Keyframe key, float timeOffset = 0f, bool doSetup = true, bool forceCopy = false)
        {
            if (!CanSetKey()) return null;
            float t = key.KeyTime + timeOffset;
            Keyframe k = SetKeyVector(t, key.KeyVector3, true);
            KeysAdd(k);
            return k;
        }

        public override void SetupKeyframes()
        {
            base.SetupKeyframes();
            if (Keys != null && Keys.Count > 0) {
                startTime = Keys[0].KeyTime;
                endTime = Keys[Keys.Count - 1].KeyTime;
                BuildBezierCurves();
            }
        }

        public override void UpdateTangents()
        {
            base.UpdateTangents();
            TangentsNeedUpdate = false;
            BuildBezierCurves();
        }

        public void BuildBezierCurves()
        {
            if (MotionPath != null && MotionPath.Nodes != null && MotionPath.Nodes.Count > 1) {
                // Precalculate bezier curves
                List<MotionPathNode> nodes = new List<MotionPathNode>();
                foreach (MotionPathNode node in MotionPath.Nodes) {
                    if (node == null) continue;
                    if (node.Enabled) {
                        nodes.Add(node);
                    }
                }

                for (int i = 0; i < nodes.Count - 1; i++) {
                    MotionPathNode nodeA = nodes[i];
                    MotionPathNode nodeB = nodes[i + 1];

                    nodeA.MotionPath = MotionPath;
                    nodeB.MotionPath = MotionPath;

                    if (nodeA == null || nodeB == null || nodeA.Key == null || nodeB.Key == null) continue;

                    if (i == 0) {
                        nodeA.Key.KeyValue = 0; // Always start at 0
                        nodeA.Distance = 0;
                    }

                    {
                        float scale = 2f;
                        Vector2 p0 = new Vector2(nodeA.KeyTime, nodeA.Key.KeyValue);
                        Vector2 p3 = new Vector2(nodeB.KeyTime, nodeB.Key.KeyValue);
                        Vector2 p1 = p0 + (nodeA.Key.OutTangent * scale);
                        Vector2 p2 = p3 + (nodeB.Key.InTangent * scale);

                        if (nodeA.Key.Bezier2D == null) {
                            nodeA.Key.Bezier2D = new BezierCurve2D(p0, p1, p2, p3);
                        }
                        else {
                            nodeA.Key.Bezier2D.P0 = p0;
                            nodeA.Key.Bezier2D.P1 = p1;
                            nodeA.Key.Bezier2D.P2 = p2;
                            nodeA.Key.Bezier2D.P3 = p3;
                        }
                    }
                    {
                        Vector3 p0 = nodeA.Key.KeyVector3;
                        Vector3 p3 = nodeB.Key.KeyVector3;
                        Vector3 p1 = p0 + (nodeA.Key.VectorOutTangent);
                        Vector3 p2 = p3 + (nodeB.Key.VectorInTangent);

                        if (nodeA.Key.Bezier3D == null) {
                            nodeA.Key.Bezier3D = new BezierCurve3D(p0, p1, p2, p3);
                        }
                        else {
                            nodeA.Key.Bezier3D.P0 = p0;
                            nodeA.Key.Bezier3D.P1 = p1;
                            nodeA.Key.Bezier3D.P2 = p2;
                            nodeA.Key.Bezier3D.P3 = p3;
                            nodeA.Key.Bezier3D.Bake();
                        }
                        nodeB.Distance = nodeA.Key.Bezier3D.BakedLength;
                    }

                    nodeB.CalculateVelocity();
                }
            }
        }

        public override void PasteKeys(bool merge)
        {
            base.PasteKeys(merge);
            MotionPath.Refresh();
        }

        public override void PrepareLoop()
        {
            if (LoopMatchEnds) {
                if (!MotionPath.ClosePath) MotionPath.ClosePath = true;
            }
            base.PrepareLoop();
            MotionPath.Setup();
        }

        public override bool HasValueChanged(float localTime)
        {
            bool changed = false;
#if UNITY_EDITOR
            if (Timeflow.Active.Input.IsDragging) {
                LastKnownSetCount = 0;
                return false;
            }

            if (HasProperty) {
                if (LastKnownTime != localTime) LastKnownSetCount = 0;

                Vector4 pos = MotionPath.transform.localPosition;

                bool detectChange = LastKnownSetCount > 1;
                if (detectChange) changed = MathUtil.IsKeyDifferent(pos, LastKnownVector);
                //if (changed) Debug.Log($"{ToProperty.PathName()} new:{pos} prev:{LastKnownVector} LastKnownSetCount:{LastKnownSetCount}");
                LastKnownVector = pos;

                LastKnownSetCount++;
                LastKnownSet = true;
                LastKnownTime = localTime;
            }
#endif
            return changed;
        }

        public override bool UnsetKey(float localTime)
        {
            Keyframe key = GetKeyAtTime(localTime);
            bool unset = UnsetKey(key);
            MotionPath.Refresh();
            return unset;
        }

        public override bool UnsetKey(Keyframe key)
        {
            //if (DebugEnabled) Debug.Log(Name + ".UnsetKey:" + key.KeyTime);
            bool isUnset = false;
            if (key != null && key.KeyGameObject != null) {
#if UNITY_EDITOR
                LastKnownSetCount = -1;
                MotionPath.Nodes.Remove(key.KeyGameObject.GetComponent<MotionPathNode>());
                UndoUtil.UndoDestroy(key.KeyGameObject);
#else
                GameObject.DestroyImmediate(key.KeyGameObject);
#endif
            }
            if (Keys != null && Keys.Contains(key)) {
#if UNITY_EDITOR
                UndoUtil.Undo(Behavior, "Unset Key", true);
#endif
                isUnset = KeysRemove(key);
                TangentsNeedUpdate = true;
                MotionPath.SortNodes();
                PrepareLoop();
            }
            bool unset = base.UnsetKey(key);
            MotionPath.Refresh();
            return unset;
        }

        public override Vector3 ApplyLimit(Vector3 value)
        {
            // Override to not limit the vector values, while float values are limited
            return value;
        }

        #region INTERPOLATION

        public override float InterpolateValue(float time, bool apply, bool isLocalTime)
        {
            if (!isLocalTime) time -= TimeOffsetWorld;
            time = LoopTime(time);

            float value = ToProperty != null ? ToProperty.FloatValue : 0f;
            if (MotionPath != null && MotionPath.Nodes != null && MotionPath.Nodes.Count > 0) {
                MotionPathNode nodeA = null;
                MotionPathNode nodeB = null;

                float prevTime = float.MaxValue;
                if (IsTrack) {
                    value = GetTrackOn(time) ? 1f : 0f;
                }
                else {
                    foreach (MotionPathNode k in MotionPath.Nodes) {
                        if (k.Enabled && k.KeyTime <= time && (nodeA == null || nodeA.KeyTime < k.KeyTime)) {
                            nodeA = k;
                        }
                    }
                    foreach (MotionPathNode k in MotionPath.Nodes) {
                        if (k.Enabled && k.KeyTime >= time && k.KeyTime < prevTime && k != nodeA) {
                            nodeB = k;
                            prevTime = nodeB.KeyTime;
                        }
                    }

                    if (nodeA == null && nodeB == null) {
                        // SKIP
                    }
                    else {
                        if (nodeA != null && nodeB != null && nodeA.Key != null && nodeB.Key != null) {

                            if (MotionPath.VelocityMode == MotionPath.VelocityModes.Fixed) {
                                float nt = nodeB.KeyTime - nodeA.KeyTime;
                                float t = nt <= 0 ? 0 : (time - nodeA.KeyTime) / nt;

                                if (nodeA.Key.Hold) {
                                    value = nodeA.Key.KeyValue;
                                }
                                else
                                if (Interpolation == Interpolations.Linear) {
                                    value = MathUtil.Interpolate(nodeA.Key.KeyValue, nodeB.Key.KeyValue, t);
                                }
                                else
                                if (Interpolation == Interpolations.Bezier) {
                                    if (nodeA.Key.Bezier2D == null) {
                                        // This shouldn't occur. No warning is shown since it should correct itself
                                    }
                                    else {
                                        value = nodeA.Key.Bezier2D.GetValue(time);// uses time (not t) because input x values are time based
                                    }
                                }
                                else
                                if (Interpolation == Interpolations.Quadratic) {
                                    value = MathUtil.EaseInOutQuad(nodeA.Key.KeyValue, nodeB.Key.KeyValue, t);
                                }

                                if (value < nodeA.Key.KeyValue) {
                                    value = nodeA.Key.KeyValue;
                                }
                                else
                                if (value > nodeB.Key.KeyValue) {
                                    value = nodeB.Key.KeyValue;
                                }
                            }
#if AXON_DEVELOPMENT
                            else
                            if (MotionPath.VelocityMode == MotionPath.VelocityModes.Flexible) {
                                if (nodeA.Key.Hold) {
                                    value = nodeA.Key.KeyValue;
                                }
                                else {
                                    Vector2 left = new Vector2(nodeA.KeyTime, nodeA.Key.KeyValue);
                                    Vector2 mid = new Vector2(nodeB.VelocityTime, nodeB.VelocityMidpoint);
                                    Vector2 right = new Vector2(nodeB.KeyTime, nodeB.Key.KeyValue);

                                    Vector2 midleft = Vector3.zero;
                                    Vector2 midright = Vector3.zero;
                                    BezierCurve2D.CalculateTangents(left, mid, right,
                                        nodeA.Key._OutTangent, nodeB.Key._InTangent,
                                        ref midleft, ref midright, true);

                                    midleft = mid + (midleft * 2f);
                                    midright = mid + (midright * 2f);

                                    if (time < nodeB.VelocityTime) {
                                        float t = (time - nodeA.KeyTime) / (nodeB.VelocityTime - nodeA.KeyTime);

                                        if (Interpolation == Interpolations.Linear || nodeA.Key.Linear || nodeB.Key.Linear) {
                                            value = MathUtil.Interpolate(nodeA.Key.KeyValue, nodeB.VelocityMidpoint, t);
                                        }
                                        else
                                        if (Interpolation == Interpolations.Bezier) {
                                            Vector2 ta = MathUtil.Interpolate(mid, left, 0.2f);
                                            Vector2 tb = MathUtil.Interpolate(mid, right, 0.2f);
                                            Vector2 leftTan = left + (nodeA.Key.OutTangent * 2f);

                                            BezierCurve2D bz = new BezierCurve2D(left, leftTan, midleft, mid);
                                            value = bz.GetValue(time);
                                        }
                                        else
                                        if (Interpolation == Interpolations.Quadratic) {
                                            value = MathUtil.EaseInOutQuad(nodeA.Key.KeyValue, nodeB.VelocityMidpoint, t);
                                            //TODO: Calculate average of ease in out
                                        }
                                    }
                                    else {
                                        float t = (time - nodeB.VelocityTime) / (nodeB.KeyTime - nodeB.VelocityTime);

                                        if (Interpolation == Interpolations.Linear) {
                                            value = MathUtil.Interpolate(nodeB.VelocityMidpoint, nodeB.Key.KeyValue, t);
                                            //value = (value + keyA.Key.KeyValue) / 2f; // Average
                                        }
                                        else
                                        if (Interpolation == Interpolations.Bezier) {
                                            //Vector2 tan = new Vector2((nodeB.KeyTime - nodeB.VelocityTime) / 4f, 0f);
                                            //Vector2 midright = mid + (tan * 2f);
                                            Vector2 rightTan = right + (nodeB.Key.InTangent * 2f);

                                            BezierCurve2D bz = new BezierCurve2D(mid, midright, rightTan, right);
                                            value = bz.GetValue(time);
                                            //value = bz.GetAverage(time);
                                        }
                                        else
                                        if (Interpolation == Interpolations.Quadratic) {
                                            value = MathUtil.EaseInOutQuad(nodeB.VelocityMidpoint, nodeB.Key.KeyValue, t);
                                            //TODO: Calculate average of ease in out
                                        }
                                    }
                                }
                            }

#endif
                        }
                        else {
                            if (nodeA == null && nodeB != null && nodeB.Key != null) {
                                value = nodeB.Key.KeyValue;
                            }
                            else
                            if (nodeB == null && nodeA != null && nodeA.Key != null) {
                                value = nodeA.Key.KeyValue;
                            }
                        }
                    }
                }
            }


            //if (DebugEnabled) Debug.Log("MotionPath.InterpolateValue: " + time + " value:" + value);

            if (apply) UpdateGlobalShaderProperty(value);
            return ApplyLimit(value);
        }

        public float InterpolateVelocity(float time, MotionPathNode nodeA, MotionPathNode nodeB)
        {
            float vel = 0f;
            if (MotionPath.VelocityMode == MotionPath.VelocityModes.Fixed) {
                float nt = nodeB.KeyTime - nodeA.KeyTime;
                float t = nt <= 0 ? 0 : (time - nodeA.KeyTime) / nt;

                if (nodeA.Key.Hold) {
                    vel = nodeA.Key.KeyValue;
                }
                else
                if (Interpolation == Interpolations.Linear) {
                    vel = MathUtil.Interpolate(nodeA.Key.KeyValue, nodeB.Key.KeyValue, t);
                }
                else
                if (Interpolation == Interpolations.Bezier) {
                    if (nodeA.Key.Bezier2D == null) {
                        Debug.LogWarning("Bezier2D is null! " + nodeA.name);
                    }
                    else {
                        vel = nodeA.Key.Bezier2D.GetValue(time);// uses time (not t) because input x values are time based
                    }
                }
                else
                if (Interpolation == Interpolations.Quadratic) {
                    vel = MathUtil.EaseInOutQuad(nodeA.Key.KeyValue, nodeB.Key.KeyValue, t);
                }
            }
#if AXON_DEVELOPMENT
            else
            if (MotionPath.VelocityMode == MotionPath.VelocityModes.Flexible) {
                // Returns average velocity over time

                if (nodeA.Key.Hold) {
                    vel = nodeA.Key.KeyValue;
                }
                else
                if (time < nodeB.VelocityTime) {
                    float t = (time - nodeA.KeyTime) / (nodeB.VelocityTime - nodeA.KeyTime);
                    float firstHalf = (nodeA.Key.KeyValue + nodeB.VelocityMidpoint) / 2f;

                    //Debug.Log("Interpolation:" + Interpolation);
                    if (Interpolation == Interpolations.Linear || nodeA.Key.Linear || nodeB.Key.Linear) {
                        vel = MathUtil.Interpolate(nodeA.Key.KeyValue, firstHalf, t);
                        //vel = vel / 2f; // Average
                        //if (Event.current != null && Event.current.shift) {
                        //    Debug.Log("A) vel:" + vel + " time:" + time + " mid:" + nodeB.VelocityMidpoint + " firstHalf:" + firstHalf);
                        //}
                    }
                    else
                    if (Interpolation == Interpolations.Bezier) {
                        Vector2 p0 = new Vector2(nodeA.KeyTime, nodeA.Key.KeyValue);
                        Vector2 p3 = new Vector2(nodeB.VelocityTime, firstHalf);
                        Vector2 p1 = p0 + (nodeA.Key.OutTangent * 2f);
                        Vector2 tan = new Vector2((nodeA.KeyTime - nodeB.VelocityTime) / 4f, 0f);
                        Vector2 p2 = p3 + (tan * 2f);


                        BezierCurve2D bz = new BezierCurve2D(p0, p1, p2, p3);
                        /*
                        float min = Mathf.Min(keyA.KeyValue, keyB.VelocityMidpoint);
                        float v = (bz.GetValue(time) - min) / Mathf.Abs(keyB.VelocityMidpoint - keyA.KeyValue);
                        if (v < 0f) v = 0f;
                        else
                        if (v > 1f) v = 1f;

                        if (keyA.KeyValue > keyB.VelocityMidpoint) v = 1f - v;
                        vel = MathUtil.Interpolate(keyA.KeyValue, firstHalf, v);
                        //vel = v;
                        */
                        vel = bz.GetValue(time);
                        //vel = bz.GetAverage(time);
                        //Debug.Log("A) v:" + v + " vel:" + vel + " firstHalf:" + firstHalf + " time:" + time + " mid:" + keyB.VelocityMidpoint + "\n p0:" + p0 + "\n p3:" + p3);
                    }
                    else
                    if (Interpolation == Interpolations.Quadratic) {
                        vel = MathUtil.EaseInOutQuad(nodeA.Key.KeyValue, nodeB.VelocityMidpoint, t);
                        //TODO: Calculate average of ease in out
                    }
                }
                else {
                    float t = (time - nodeB.VelocityTime) / (nodeB.KeyTime - nodeB.VelocityTime);
                    float firstHalf = (nodeA.Key.KeyValue + nodeB.VelocityMidpoint) / 2f;

                    if (Interpolation == Interpolations.Linear) {
                        //vel = MathUtil.Interpolate(keyB.VelocityMidpoint, keyB.KeyValue, t);// keyB.VelocityMidpoint , t);
                        vel = MathUtil.Interpolate(firstHalf, nodeB.Key.KeyValue, t);
                        //float delta = (vel - keyB.VelocityMidpoint) / (time - keyB.VelocityTime);
                        //vel += delta;
                        //if (Event.current != null && Event.current.shift) {
                        //    Debug.Log("B) vel:" + vel + " time:" + time + " mid:" + nodeB.VelocityMidpoint + " firstHalf:" + firstHalf);
                        //}
                        //vel = (firstHalf + (vel + keyA.KeyValue) / 2f) / 2f; // Average
                        //vel = firstHalf + (vel * (time - keyB.VelocityTime) / (keyB.VelocityTime - keyA.KeyTime));
                        //float bvel = MathUtil.Interpolate(firstHalf, keyB.KeyValue, t); //(vel + firstHalf) / 3f;
                        //Debug.Log("B) vel:" + vel + " bvel:" + bvel + " keyB.v:" + keyB.KeyValue + " time:" + time + " mid:" + keyB.VelocityMidpoint + " firstHalf:" + firstHalf);
                        //vel = bvel;
                    }
                    else
                    if (Interpolation == Interpolations.Bezier) {
                        Vector2 p0 = new Vector2(nodeB.VelocityTime, firstHalf);// keyB.VelocityMidpoint);
                        Vector2 p3 = new Vector2(nodeB.KeyTime, nodeB.Key.KeyValue);// keyB.Key.KeyValue);
                        Vector2 tan = new Vector2((nodeB.KeyTime - nodeB.VelocityTime) / 4f, 0f);
                        Vector2 p1 = p0 + (tan * 2f);
                        Vector2 p2 = p3 + (nodeB.Key.InTangent * 2f);

                        BezierCurve2D bz = new BezierCurve2D(p0, p1, p2, p3);
                        /*
                        float a = bz.GetValue(time);
                        float min = Mathf.Min(keyB.Key.KeyValue, keyB.VelocityMidpoint);
                        float dif = Mathf.Abs(keyB.VelocityMidpoint - keyB.Key.KeyValue);
                        float v = (a - min) / dif;
                        if (v < 0f) v = 0f;
                        else
                        if (v > 1f) v = 1f;

                        if (keyB.Key.KeyValue < keyB.VelocityMidpoint) v = 1f - v;
                        vel = MathUtil.Interpolate(firstHalf, keyB.Key.KeyValue, v);
                        //vel = MathUtil.EaseOutExpo(firstHalf, keyB.Key.KeyValue, v);
                        */

                        //vel = v;
                        vel = bz.GetValue(time);
                        //vel = bz.GetAverage(time);
                        //vel = firstHalf + (vel * (time - keyB.VelocityTime) / (keyB.VelocityTime - keyA.KeyTime));
                        //Debug.Log("B) v:" + v + " a:" + a + " min:" + min + " dif:" + dif + " vel:" + vel + " firstHalf:" + firstHalf + " time:" + time + " mid:" + keyB.VelocityMidpoint + "\n inTime:" + keyB.VelocityTime + " outTime:" + keyB.KeyTime + "\n p0:" + p0 + "\n p3:" + p3);
                    }
                    else
                    if (Interpolation == Interpolations.Quadratic) {
                        vel = MathUtil.EaseInOutQuad(nodeA.Key.KeyValue, nodeB.VelocityMidpoint, t);
                        //TODO: Calculate average of ease in out
                    }
                }
            }
#endif

            return vel;
        }

        public override Vector3 InterpolateVector3(float time, bool apply, bool isLocalTime, bool canLink)
        {
            return InterpolatePath(time, apply, isLocalTime, MotionPath.transform, false, canLink);
        }

        public Vector3 InterpolatePath(float time, bool apply, bool isLocalTime, Transform applyTo, bool calculateRotation)
        {
            return InterpolatePath(time, apply, isLocalTime, applyTo, calculateRotation, true);
        }

        public Vector3 InterpolatePath(float intime, bool apply, bool isLocalTime, Transform applyTo, bool calculateRotation, bool canLink)
        {
            float time = intime;
            if (!isLocalTime) {
                time -= TimeOffsetWorld;
            }
            if (IsCachedVector(time)) return CurrentVector;

            Vector3 value = Vector3.zero;
            Vector3 euler = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            InterpolatePath(time, apply, true, ref value, ref euler, ref rotation, calculateRotation, canLink);

            if (apply) {
                //if (Behavior.DebugEnabled) Debug.Log(_Name + ".InterpolateVector:" + value + " euler:" + euler + " time:" + time + " startTime:" + startTime + " endTime:" + endTime);
                applyTo.localPosition = value;
                if (calculateRotation) {
                    if (MotionPath.RotationMode == MotionPath.RotationModes.Interpolate) {
                        Behavior.Rotator.Euler = euler;
                    }
                    else
                    if (MotionPath.RotationMode != MotionPath.RotationModes.LookAhead) {
                        applyTo.localRotation = rotation;
                    }
                }
                if (Application.isEditor) {
                    EditorUtil.SetDirty(applyTo);
                }

                ToProperty.Vector3Value = value;
                UpdateGlobalShaderProperty(value);

                currentEuler = euler;
                currentRotation = rotation;


                value = ApplyLimit(value);
                if (canLink && IsLinkEnabled) {
                    value = ApplyLimit(Link.GetVector3(value, WorldTime(intime, isLocalTime)));
                }
                SetCurrentVector(value, time);
            }

            return value;
        }

        public void InterpolatePath(float time, bool apply, bool isLocalTime, ref Vector3 value, ref Vector3 euler, ref Quaternion rotation, bool calculateRotation)
        {
            InterpolatePath(time, apply, isLocalTime, ref value, ref euler, ref rotation, calculateRotation, true);
        }

        public void InterpolatePath(float intime, bool apply, bool isLocalTime, ref Vector3 value, ref Vector3 euler, ref Quaternion rotation, bool calculateRotation, bool canLink)
        {
            InterpolatePath(intime, apply, isLocalTime, ref value, ref euler, ref rotation, calculateRotation, canLink, false);
        }

        public void InterpolatePath(float intime, bool apply, bool isLocalTime, ref Vector3 value, ref Vector3 euler, ref Quaternion rotation, bool calculateRotation, bool canLink, bool interpolateRotation)
        {
            if (TangentsNeedUpdate) {
                TangentsNeedUpdate = false;
                UpdateTangents();
            }

            float time = intime;
            if (!isLocalTime) time -= TimeOffsetWorld;
            time = LoopTime(time);

            if (MotionPath != null && MotionPath.Nodes != null && MotionPath.Nodes.Count > 0 && MotionPath.IsSetup) {
                calculateRotation = calculateRotation && MotionPath.RotationMode != MotionPath.RotationModes.None;

                MotionPathNode nodeA = null;
                MotionPathNode nodeB = null;

                if (time < startTime) {
                    nodeA = MotionPath.Nodes[0];
                    value = nodeA.Key.KeyVector;
                    euler = nodeA.Rotator.Euler;
                }
                else
                if (time > endTime) {
                    nodeA = MotionPath.LastNode;
                    value = nodeA.Key.KeyVector;
                    euler = nodeA.Rotator.Euler;
                }
                else {
                    float prevTime = float.MaxValue;
                    foreach (MotionPathNode k in MotionPath.Nodes) {
                        if (k == null) continue;
                        else
                        if (k.Key == null) continue;
                        else
                        if (k.Enabled && k.KeyTime <= time && (nodeA == null || nodeA.KeyTime < k.KeyTime)) {
                            nodeA = k;
                        }
                    }
                    if (MotionPath == null) {
                        Debug.LogWarning("MotionPath is null!");
                        return;
                    }
                    if (MotionPath.Nodes == null) {
                        Debug.LogWarning("MotionPath.Nodes is null!");
                        return;
                    }
                    foreach (MotionPathNode k in MotionPath.Nodes) {
                        if (k.Enabled && k.KeyTime >= time && k.KeyTime < prevTime && k != nodeA) {
                            nodeB = k;
                            prevTime = nodeB.KeyTime;
                        }
                    }

                    if (nodeA == null && nodeB == null) {
                        // SKIP
                    }
                    else {
                        if (nodeA != null && nodeB != null) {
                            float vel = InterpolateVelocity(time, nodeA, nodeB);

                            float t = 0;
                            if (MotionPath.VelocityMode == MotionPath.VelocityModes.Flexible) {
                                if (nodeB.Distance <= 0) {
                                    t = 0;
                                }
                                else {
                                    // Calculate distance traveled relative to key distance
                                    float dist = vel * (time - nodeA.KeyTime);
                                    if (dist == 0f || nodeB.Distance <= 0f) {
                                        t = 0f;
                                    }
                                    else {
                                        t = dist / nodeB.Distance;
                                    }
                                }
                            }
                            else {
                                // Normalize velocity value between keys
                                float ta = (vel - nodeA.Key.KeyValue);
                                float tb = (nodeB.Key.KeyValue - nodeA.Key.KeyValue);
                                if (ta == 0f || tb == 0f) {
                                    t = 0f;
                                }
                                else {
                                    t = ta / tb;
                                }
                            }

                            if (t >= 1f) {
                                value = nodeB.Key.KeyVector;
                            }
                            else
                            if (t <= 0f) {
                                value = nodeA.Key.KeyVector;
                            }
                            else {
                                if (PathInterpolation == PathInterpolations.Hold || nodeA.Key.Hold) {
                                    value = nodeA.Key.KeyVector;
                                }
                                else
                                if (PathInterpolation == PathInterpolations.Linear) {
                                    value = MathUtil.Interpolate(nodeA.Key.KeyVector3, nodeB.Key.KeyVector3, t);
                                }
                                else
                                if (PathInterpolation == PathInterpolations.Bezier) {
                                    value = nodeA.Key.Bezier3D.GetPointAtLinearTime(t);
                                }
                            }

                            if (calculateRotation) {
                                if (interpolateRotation || MotionPath.RotationMode == MotionPath.RotationModes.Interpolate) {
                                    euler = MathUtil.Interpolate(nodeA.Rotator.Euler, nodeB.Rotator.Euler, t);
                                    rotation = Quaternion.Euler(euler);
                                }
                                else {
                                    rotation = rotation * Quaternion.Euler(MotionPath.Orientation);
                                }
                            }

                        }
                        else {
                            if (nodeA == null) {
                                value = nodeB.Key.KeyVector;
                                if (calculateRotation) rotation = nodeB.Key.KeyGameObject.transform.localRotation;
                            }
                            else
                            if (nodeB == null) {
                                value = nodeA.Key.KeyVector;
                                if (calculateRotation) rotation = nodeA.Key.KeyGameObject.transform.localRotation;
                            }

                        }
                    }
                }
            }

            value = ApplyLimit(value);
            if (canLink && IsLinkEnabled) {
                value = ApplyLimit(Link.GetVector3(value, WorldTime(intime, isLocalTime)));
            }


            SetCurrentVector(value, time);
            currentEuler = euler;
            currentRotation = rotation;
        }

        public float InterpolateTime(float time)
        {
            time = LoopTime(time);
            if (MotionPath != null && MotionPath.Nodes != null && MotionPath.Nodes.Count > 0) {
                MotionPathNode nodeA = null;
                MotionPathNode nodeB = null;

                if (time < startTime) {
                    time = Keys[0].KeyTime;
                }
                else
                if (time > endTime) {
                    time = Keys[Keys.Count - 1].KeyTime;
                }
                else {
                    float prevTime = float.MaxValue;
                    foreach (MotionPathNode k in MotionPath.Nodes) {
                        if (k.Enabled && k.KeyTime <= time && (nodeA == null || nodeA.KeyTime < k.KeyTime)) {
                            nodeA = k;
                        }
                    }
                    foreach (MotionPathNode k in MotionPath.Nodes) {
                        if (k.Enabled && k.KeyTime >= time && k.KeyTime < prevTime && k != nodeA) {
                            nodeB = k;
                            prevTime = nodeB.KeyTime;
                        }
                    }

                    if (nodeA == null && nodeB == null) {
                        // SKIP
                    }
                    else {
                        if (nodeA != null && nodeB != null) {
                            float nt = nodeB.KeyTime - nodeA.KeyTime;
                            float t = nt <= 0 ? 0 : (time - nodeA.KeyTime) / nt;
                            float vel = 0f;
                            if (nodeA.Key.Hold) {
                                vel = nodeA.Key.KeyValue;
                            }
                            else
                            if (Interpolation == Interpolations.Linear) {
                                vel = MathUtil.Interpolate(nodeA.Key.KeyValue, nodeB.Key.KeyValue, t);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                Vector2 p0 = new Vector2(nodeA.KeyTime, nodeA.Key.KeyValue);
                                Vector2 p3 = new Vector2(nodeB.KeyTime, nodeB.Key.KeyValue);

                                float r = nodeB.KeyTime - nodeA.KeyTime;
                                if (r <= 0) r = 1;
                                Vector2 t1 = new Vector2(nodeA.Key.OutTangent.y * r, nodeA.Key.OutTangent.x / r);
                                Vector2 t2 = new Vector2(nodeB.Key.InTangent.y * r, -nodeB.Key.InTangent.x / r);
                                Vector2 p1 = p0 + (t1 * 2f);
                                Vector2 p2 = p3 + (t2 * 2f);

                                BezierCurve2D bz2 = new BezierCurve2D(p0, p1, p2, p3);
                                vel = bz2.GetValue(time);
                            }
                            else
                            if (Interpolation == Interpolations.Quadratic) {
                                vel = MathUtil.EaseInOutQuad(nodeA.Key.KeyValue, nodeB.Key.KeyValue, t);
                            }

                            float nv = nodeB.Key.KeyValue - nodeA.Key.KeyValue;
                            t = nv <= 0 ? 0 : (vel - nodeA.Key.KeyValue) / nv;

                            if (t >= 1f) {
                                time = nodeB.KeyTime;
                            }
                            else
                            if (t <= 0f) {
                                time = nodeA.KeyTime;
                            }
                            else {
                                time = MathUtil.Interpolate(nodeA.KeyTime, nodeB.KeyTime, t);
                            }
                        }
                        else {
                            if (nodeA == null) {
                                time = nodeB.KeyTime;
                            }
                            else
                            if (nodeB == null) {
                                time = nodeA.KeyTime;
                            }

                        }
                    }
                }

            }
            return time;
        }

        public Vector3 InterpolateVectorProgress(float progress, bool apply, ref Quaternion rotation, bool calculateRotation)
        {
            Vector3 value = Vector3.zero;

            if (Behavior != null && Keys != null && Keys.Count > 0) {
                if (IsVectorChanged || VectorPathPoly == null || VectorPathPoly.Vertices == null || VectorPathPoly.Vertices.Length == 0) {
                    BuildVectorPath();
                }

                if (VectorPathPoly == null) return value;

                value = VectorPathPoly.GetPointAtPercent(progress);
                if (calculateRotation) {
                    float tt = progress + 0.01f;
                    if (tt > 1f) tt = 1f;
                    Vector3 tpos = VectorPathPoly.GetPointAtPercent(tt);

                    if (tpos != value) {
                        rotation = Quaternion.LookRotation(tpos - value, Vector3.up);
                    }
                    else
                    if (tt >= 1f) {
                        rotation = Keys[Keys.Count - 1].KeyGameObject.transform.localRotation;
                    }
                    else {
                        rotation = Keys[0].KeyGameObject.transform.localRotation;
                    }
                }
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateVectorProgress:" + progress + " value:" + value);

                if (apply) {
                    Behavior.transform.localPosition = value;
                    if (calculateRotation) {
                        Behavior.transform.localRotation = rotation;
                    }
                    if (Application.isEditor) {
                        EditorUtil.SetDirty(Behavior.transform);
                    }
                }
            }

            return value;
        }

        #endregion

#if UNITY_EDITOR

        public override void OnKeySelected(Keyframe key)
        {
            //if (DebugEnabled) Debug.Log("MotionPathChannel.OnKeySelected:" + key.KeyTime);
            MotionPath.SelectNodeFromKey(key, false);
        }

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override void GUIGraphPass2()
        {
            if (Keys != null && Keys.Count > 0) {
                SortBy(TimeflowChannel.SortingModes.TimeAsc);

                float yOffset = GUIRect.y;

                Vector3[] line = null;

                int i = 0;
                int k = 0;
                int first = -1;
                int count = 0;

                // Find the starting and ending indices for the keys in view
                foreach (Keyframe key in Keys) {
                    float keyTime = key.KeyTimeWorld;
                    if (keyTime >= Timeflow.Active.View.ViewStartTime && keyTime <= Timeflow.Active.View.ViewEndTime) {
                        if (first == -1) first = i;
                        count++;
                    }
                    i++;
                }
                if (first > 0) {
                    // Start one keyframe left of offscreen so the line draws continuously
                    first--;
                    count++;
                }

                int steps = 1024;
                line = new Vector3[steps];

                float time = Timeflow.Active.View.ScrollTimeMin;
                float timeStep = (Timeflow.Active.View.ScrollTimeMax - time) / (float)steps;
                float worldOffset = TimeOffsetWorld * TimeScaleWorld;
                for (i = 0; i < steps; i++) {
                    float vx = Timeflow.Active.View.PositionOfTime(time, true);
                    line[i].x = vx;
                    line[i].y = Timeflow.Active.View.PositionOfValue(InterpolateValue(time * TimeScaleWorld - worldOffset, false, true), true);
                    time += timeStep;
                }
                Handles.color = GUIColor;
                Handles.DrawAAPolyLine(line);

                i = 0;

                int ki = 0; // use key index to ignore inTan of first key, and outTan of last key
                foreach (Keyframe key in Keys) {
                    float keyTime = key.KeyTimeWorld;
                    if (keyTime >= Timeflow.Active.View.ViewStartTime && keyTime <= Timeflow.Active.View.ViewEndTime) {
                        float keyValue = key.KeyValue;

                        float x = Timeflow.Active.View.PositionOfTime(keyTime, true);
                        float y = Timeflow.Active.View.PositionOfValue(keyValue, true);
                        key.GUIRect = new GUIRect(x - 8, y - 8, 16, 16);
                        key.GUILabelRect = new GUIRect(x + 2, y, 300, 20);

                        bool isSelected = false;
                        if (Timeflow.Active.View.SelectedKeys != null) {
                            isSelected = Timeflow.Active.View.SelectedKeys.Contains(key);
                        }

                        if (key.IsKeyEnabled && Interpolation == TimeflowChannel.Interpolations.Bezier
                            && !key.Hold && key.ShowTangents && (isSelected || Timeflow.Active.View.GraphShowBezierHandles)) {

                            Handles.color = key.IsAutoTangents ? AxonColor.KeyAutoTangents :
                                TimeflowView.GUIGraphPassNumber == 0 ? AxonColor.KeyTangents :
                                TimeflowView.GUIGraphPassNumber == 1 ? AxonColor.KeyTangents2 : AxonColor.KeyTangents3;
                            GUI.color = Handles.color;

                            line = new Vector3[3];

                            GUIStyle style = AxonUI.BezierBrokenHandleStyle;
                            if (key.IsAutoTangents) {
                                style = AxonUI.BezierUnifiedHandleStyle;
                            }
                            else
                            if (key.UnifyTangents) {
                                style = AxonUI.BezierUnifiedHandleStyle;
                            }
                            else
                            if (key.UnifyTangentLengths) {
                                style = AxonUI.BezierEqualHandleStyle;
                            }
                            else {
                                style = AxonUI.BezierBrokenHandleStyle;
                            }


                            if (ki > 0) {
                                line[0].x = Timeflow.Active.View.PositionOfTime(keyTime + key.InTangent.x, true);
                                line[0].y = Timeflow.Active.View.PositionOfValue(keyValue + key.InTangent.y, true);
                                key.InPointRect = new GUIRect(line[0].x - 10f, line[0].y - 10f, 20, 20);

                                GUI.Box(key.InPointRect, GUIContent.none, style);
                            }
                            else {
                                line[0].x = x;
                                line[0].y = y;
                            }

                            line[1].x = x;
                            line[1].y = y;

                            if (ki < Keys.Count - 1) {
                                line[2].x = Timeflow.Active.View.PositionOfTime(keyTime + key.OutTangent.x, true);
                                line[2].y = Timeflow.Active.View.PositionOfValue(keyValue + key.OutTangent.y, true);
                                key.OutPointRect = new GUIRect(line[2].x - 10f, line[2].y - 10f, 20, 20);
                                GUI.Box(key.OutPointRect, GUIContent.none, style);
                            }
                            else {
                                line[2].x = x;
                                line[2].y = y;
                            }

                            Handles.DrawAAPolyLine(line);
                        }
                        i++;


                    }
                    ki++;
                }

                bool isFaded = !IsEnabled || (!IsSelected && Timeflow.View.IsGraphMode && Timeflow.View.IsGraphSolo);

                k = first;
                for (i = 0; i < count; i++) {
                    Keyframe key = Keys[k]; k++;
                    float keyTime = key.KeyTimeWorld;
                    if (keyTime >= Timeflow.Active.View.ViewStartTime && keyTime <= Timeflow.Active.View.ViewEndTime) {
                        if (Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(key)) {
                            GUI.color = key.OverrideGUIColor ? key.GUIColor : GUIColor;
                            GUI.Box(key.GUIRect, GUIContent.none, GUIKeyframeStyle(key, false));

                            GUI.color = AxonColor.Default;
                            GUI.Box(key.GUIRect, GUIContent.none, GUIKeyframeStyle(key, true));

                            if (Timeflow.Active.Input.IsDraggingCopy && Timeflow.Active.Input.DraggingTimeOffset != 0f) {
                                Rect r = new Rect(key.GUIRect);
                                r.x = Timeflow.Active.View.PositionOfTime(keyTime + Timeflow.Active.Input.DraggingTimeOffset, true) - 8;
                                GUI.Box(r, GUIContent.none, GUIKeyframeStyle(key, true));
                            }

                            if ((key.LockTime || key.LockValue) && Timeflow.Active.Input.IsDragging) {
                                GUI.color = Color.white;
                                Rect lockRect = key.GUIRect;
                                lockRect.x -= 8;
                                lockRect.width = lockRect.height = 16;
                                GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);
                            }

                        }
                        else {
                            GUI.color = key.OverrideGUIColor ? key.GUIColor : GUIColor;
                            if (!key.IsKeyEnabled) GUI.color = Color.gray;
                            GUI.Box(key.GUIRect, GUIContent.none, GUIKeyframeStyle(key, false));
                        }

                        GUIDrawKeyframeLabelGraph(key, isFaded);
                    }
                }
            }
        }

        public override void GUIDrawKeyframeLabelGraph(Keyframe key, bool isFaded = false, bool forceShow = false)
        {
            GUIDrawKeyframeLabelGraph(key, key.KeyVector3.ToString(), isFaded, forceShow);
        }

        public override void GUIDrawKeyframeLabel(Keyframe key, GUIRect keyRect, GUIRect labelRect, string value, bool isGraph, bool isFaded = false, bool forceShow = false)
        {
            value = key.KeyVector3.ToString();
            base.GUIDrawKeyframeLabel(key, keyRect, labelRect, value, isGraph, isFaded, forceShow);
        }

        public override void GUIInfoValueChanged(List<Keyframe> selectedKeys)
        {
            UndoUtil.Undo(Behavior, "Set Key Values", true);
            foreach (Keyframe key in selectedKeys) {
                if (key != null && key.KeyGameObject != null) {
                    UndoUtil.Undo(key.KeyGameObject.transform, "Set Key Values", true);
                    key.KeyGameObject.transform.localPosition = key.KeyVector;
                }
            }
            MotionPath.Refresh();
        }

        /// <summary>
        /// Displays information for selected keys in the Info panel in the Timeflow view.
        /// </summary>
        public override void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            if (tracksOnly) return;
            base.GUIInfoValues(selectedKeys, tracksOnly);
            Vector3 euler = Vector3.zero;
            float val = 0f;
            bool lv = false;

            bool first = true;
            bool isSameEuler = true;
            bool isSameValue = true;
            bool isSameLock = true;
            foreach (Keyframe key in selectedKeys) {
                if (key != null && key.KeyGameObject != null) {
                    if (first) {
                        val = key.KeyValue;
                        euler = Rotator.GetValue(key.KeyGameObject);
                        first = false;
                        lv = key.LockValue;
                    }
                    else {
                        if (isSameEuler && euler != Rotator.GetValue(key.KeyGameObject)) {
                            isSameEuler = false;
                        }
                        if (isSameValue && val != key.KeyValue) {
                            isSameValue = false;
                        }
                        if (isSameLock && lv != key.LockValue) {
                            isSameLock = false;
                        }
                    }
                }
            }

            Vector3 inEuler = Vector3.zero;
            Vector3 outEuler = inEuler;
            if (isSameEuler) inEuler = euler;

            if (MotionPath.ShowRotationChannel) {
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, "Lock Value")) {
                    lv = !lv;
                    foreach (Keyframe key in selectedKeys) {
                        key.LockValue = lv;
                    }
                }
                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Key Euler";
                outEuler = AxonGUI.FieldVector3(MotionPath, "Euler", inEuler);
                if (outEuler != inEuler) {
                    euler = outEuler;
                    foreach (Keyframe key in selectedKeys) {
                        Rotator.SetValue(key.KeyGameObject, euler);
                    }
                    MotionPath.Interpolate();
                }
                EditorGUI.EndDisabledGroup();

                AxonGUI.EndHorizontal(false);
            }

            if (DebugEnabled) {
                // Show the internal relative velocity value 
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonLock(lv, "Lock Value")) {
                    lv = !lv;
                    foreach (Keyframe key in selectedKeys) {
                        key.LockValue = lv;
                    }
                }
                EditorGUI.BeginDisabledGroup(lv);
                AxonGUI.UndoName = "Set Key Value";
                float outVal = AxonGUI.FieldFloat(Behavior, "Value", val);
                if (outVal != val) {
                    foreach (Keyframe key in selectedKeys) {
                        key.KeyValue = outVal;
                    }
                    MotionPath.Interpolate();
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal(false);
            }

        }

#endif
    }

}//AxonGenesis
