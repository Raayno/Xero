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

namespace AxonGenesis
{
    public partial class Keyframe : SerializableObject
    {
        public bool HasSameValue(Keyframe key)
        {
            if (IsTrack) {
                return key.IsTrack && key.KeyValue == KeyValue && key.KeyTime == KeyTime;
            }
            else 
            if (IsFloat || ForceFloat || IsUniformValue) {
                return key.KeyValue == KeyValue;
            }
            else 
            if (IsBool) {
                return key.KeyBool == KeyBool;
            }
            else 
            if (IsInt || IsEnum) {
                return (int)key.KeyValue == (int)KeyValue;
            }
            else 
            if (IsVector2) {
                return key.KeyVector2 == KeyVector2;
            }
            else 
            if (IsVector3) {
                return key.KeyVector3 == KeyVector3;
            }
            else 
            if (IsVector4) {
                return key.KeyVector == KeyVector;
            }
            else 
            if (IsColor) {
                return key.KeyColor == KeyColor;
            }
            else 
            if (IsRect) {
                return key.KeyRect == KeyRect;
            }
            else 
            if (IsRectOffset) {
                return key.KeyRectOffset == KeyRectOffset;
            }
            else 
            if (IsComponent) {
                return key.KeyComponent == KeyComponent;
            }
            else 
            if (IsGameObject) {
                return key.KeyGameObject == KeyGameObject;
            }
            else 
            if (IsObject) {
                return key.KeyObject == KeyObject;
            }
            else 
            if (IsString) {
                return key.KeyString == KeyString;
            }
            return false;
        }

        public override string ToString()
        {
            string value = "";

            if (IsTrack || IsFloat || ForceFloat || IsUniformValue) {
                value = $"{KeyValue}";
            }
            else
            if (IsBool) {
                value = $"{KeyBool}";
            }
            else
            if (IsInt || IsEnum) {
                value = $"{(int)KeyValue}";
            }
            else
            if (IsVector2) {
                value = $"{KeyVector2}";
            }
            else
            if (IsVector3) {
                value = $"{KeyVector3}";
            }
            else
            if (IsVector4) {
                value = $"{KeyVector}";
            }
            else
            if (IsColor) {
                value = $"{KeyColor}";
            }
            else
            if (IsRect) {
                value = $"{KeyRect}";
            }
            else
            if (IsRectOffset) {
                value = $"{KeyRectOffset}";
            }
            else
            if (IsComponent) {
                value = $"{(KeyComponent == null ? "NULL" : KeyComponent.GetType().ToString())}";
            }
            else
            if (IsGameObject) {
                value = $"{(KeyGameObject == null ? "NULL" : KeyGameObject.name)}";
            }
            else
            if (IsObject) {
                value = $"{(KeyObject == null ? "NULL" : KeyObject.name)}";
            }
            else
            if (IsString) {
                value = KeyString;
            }

            return value;
        }

        /// <summary>
        /// This sets the start and end time for a track style keyframe.
        /// </summary>
        /// <param name="startTime">The start time of the track section.</param>
        /// <param name="endTime">The end time of the track section</param>
        public void SetTrackTime(float startTime, float endTime)
        {
            _KeyTime = Timeflow.ApplyTimeTolerance(startTime);
            _KeyValue = Timeflow.ApplyTimeTolerance(endTime);
            if (_KeyTime >= _KeyValue) {
                /// Don't allow invalid times to be entered.  The minimimum duration is the time tolerance
                /// set in the preferences.
                _KeyValue = _KeyTime + TimeflowPreferences.Current.TimeTolerance;
            }
            ValueChanged();
        }

        /// <summary>
        /// Sets the value of each attribute depending on what is selected in the editor.
        /// </summary>
        /// <param name="value"></param>
        public void SetAtributeValues(float value)
        {
            if (!IsTrack) {
                if (!HasMultipleAttributes) {
                    KeyValue = value;
                }
                else {
#if UNITY_EDITOR
                    if (AttributeSelected0 && AttributeCount > 0) {
                        _KeyVector.x = value;
                    }
                    if (AttributeSelected1 && AttributeCount > 1) {
                        _KeyVector.y = value;
                    }
                    if (AttributeSelected2 && AttributeCount > 2) {
                        _KeyVector.z = value;
                    }
                    if (AttributeSelected3 && AttributeCount > 3) {
                        _KeyVector.w = value;
                    }
#else
                    if (AttributeCount > 0) {
                        _KeyVector.x = value;
                    }
                    if (AttributeCount > 1) {
                        _KeyVector.y = value;
                    }
                    if (AttributeCount > 2) {
                        _KeyVector.z = value;
                    }
                    if (AttributeCount > 3) {
                        _KeyVector.w = value;
                    }
#endif
                }
            }
#if UNITY_EDITOR
            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
#endif
        }

        public void SetInTangent(Vector2 inTangent)
        {
            _InTangent = inTangent;
            ValueChanged();
        }

        public void SetOutTangent(Vector2 outTangent)
        {
            _OutTangent = outTangent;
            ValueChanged();
        }

        public void SetTangents(Vector2 inTangent, Vector2 outTangent)
        {
            _InTangent = inTangent;
            _OutTangent = outTangent;
            ValueChanged();
        }

        /// <summary>
        /// This bypbases the KeyValue set method for special use cases where the value needs to be set
        /// directly without automatic validation.
        /// </summary>
        /// <param name="value"></param>
        public void SetKeyValueExplicit(float value)
        {
            _KeyValue = value;
        }

        /// <summary>
        /// Sets the internal _KeyTime value bypassing the acccesor method. This should only be used for
        /// special cases. It is recommended to use the KeyTime accessor instead.
        /// </summary>
        /// <param name="time"></param>
        public void SetKeyTimeExplicit(float time)
        {
            _KeyTime = time;
        }

        /// <summary>
        /// Instead of updating tangents immediately upon keyframe value changes, this is deferred by
        /// flagging the channel to update the tangents. This is necessary so that the tangents can be
        /// updated by the channel in context to its interpolation settings.
        /// </summary>
        public void SetTangentsNeedUpdate()
        {
            if (IsAutoTangents) {
                if (Channel != null) Channel.TangentsNeedUpdate = true;
            }
        }

        /// <summary>
        /// Changes the interpolation mode for this keyframe and optionally clears hold values.
        /// </summary>
        /// <param name="interp">Sets the Interpolations mode.</param>
        /// <param name="clearHolds">If true, Hold and Linear flags are disabled on the keyframe to revert
        ///     them to default interpolation.</param>
        public void SetInterpolation(Interpolations interp, bool clearHolds)
        {
            if (clearHolds) {
                Hold = false;
                Linear = false;
            }

            float timeScale = 1f;
#if UNITY_EDITOR
            if (Event.current != null && Event.current.alt && IsTimeflowActive) {
                // Calculate a new tangent length based on the current time framing of the Timeflow view
                timeScale = View.GetVisibleTimeRange() * 0.025f;
            }
#endif

            if (interp == Interpolations.Linear) {
                SetInterpolationLinear();
            }
            else
            if (interp == Interpolations.LinearLeft) {
                SetInterpolationLinearLeft();
            }
            else
            if (interp == Interpolations.LinearRight) {
                SetInterpolationLinearRight();
            }
            else
            if (interp == Interpolations.Hold) {
                SetInterpolationHold();
            }
            else
            if (interp == Interpolations.Flat) {
                SetInterpolationFlat(timeScale);
            }
            else
            if (interp == Interpolations.FlatLeft) {
                SetInterpolationFlatLeft(timeScale);
            }
            else
            if (interp == Interpolations.FlatRight) {
                SetInterpolationFlatRight(timeScale);
            }
            else
            if (interp == Interpolations.Vertical) {
                SetInterpolationVertical();
            }
            else
            if (interp == Interpolations.Auto) {
                SetInterpolationAuto();
            }
            OnVectorChanged();
            OnInterpolationChanged();
        }

        private void SetInterpolationLinear()
        {
            if (Linear) Linear = false;
            else {
                InTangent = Vector2.zero;
                OutTangent = Vector2.zero;
                Linear = true;
                Hold = false;
            }
        }

        private void SetInterpolationLinearLeft()
        {
            UnifyTangents = false;
            UnifyTangentLengths = false;
            InTangent = Vector2.zero;
        }

        private void SetInterpolationLinearRight()
        {
            UnifyTangents = false;
            UnifyTangentLengths = false;
            OutTangent = Vector2.zero;
        }

        private void SetInterpolationHold()
        {
            Hold = !Hold;
            if (Hold) Linear = false;
        }

        /// <summary>
        /// Flattens both the in and out tangents of the keyframe.
        /// </summary>
        /// <param name="timeScale">The time view scale to match user expectation in the GUI</param>
        private void SetInterpolationFlat(float timeScale)
        {
            UnifyTangents = true;
            Vector2 intan = new Vector2(-Mathf.Abs(InTangent.x), 0);
            Vector2 outan = new Vector2(Mathf.Abs(OutTangent.x), 0);

            if (timeScale != 1f) {
                intan.x = -timeScale;
                outan.x = timeScale;
            }

            InTangent = MathUtil.Validate(intan);
            OutTangent = outan;
        }

        /// <summary>
        /// Flattens only the in tangent (left side) of the keyframe.
        /// </summary>
        /// <param name="timeScale">The time view scale to match user expectation in the GUI</param>
        private void SetInterpolationFlatLeft(float timeScale)
        {
            IsAutoTangents = UnifyTangentLengths = UnifyTangents = false;
            Vector2 v = InTangent;
            v.y = 0f;
            if (timeScale != 1f) {
                v.x = MathUtil.Validate(-timeScale);
            }
            InTangent = v;
        }

        /// <summary>
        /// Flattens only the out tangent (right side) of the keyframe.
        /// </summary>
        /// <param name="timeScale">The time view scale to match user expectation in the GUI</param>
        private void SetInterpolationFlatRight(float timeScale)
        {
            IsAutoTangents = UnifyTangentLengths = UnifyTangents = false;
            Vector2 v = OutTangent;
            v.y = 0f;
            if (timeScale != 1f) {
                v.x = timeScale;
            }
            OutTangent = v;
        }

        /// <summary>
        /// Sets keyframes to auto tangent mode, which automatically adjusts tangents to the curve context.
        /// </summary>
        private void SetInterpolationVertical()
        {
            IsAutoTangents = false;
            Vector2 intan = new Vector2(0, InTangent.y);
            Vector2 outan = new Vector2(0, OutTangent.y);
            InTangent = intan;
            OutTangent = outan;

            SetTangentsNeedUpdate();
        }

        /// <summary>
        /// Sets keyframes to auto tangent mode, which automatically adjusts tangents to the curve context.
        /// </summary>
        private void SetInterpolationAuto()
        {
            IsAutoTangents = true;
            UnifyTangents = true;
            UnifyTangentLengths = true;
            SetTangentsNeedUpdate();
        }

        public void MirrorTangentsTime()
        {
            Vector2 inTan = InTangent;
            inTan.x = -inTan.x;
            _OutTangent.x = -_OutTangent.x;
            _InTangent = _OutTangent;
            _OutTangent = inTan;

            Vector3 vinTan = VectorInTangent;
            vinTan.x = -vinTan.x;
            _VectorOutTangent.x = -_VectorOutTangent.x;
            _VectorInTangent = -_VectorOutTangent;
            _VectorOutTangent = -vinTan;
        }

        public void MirrorTangentsValue()
        {
            _InTangent.y = -_InTangent.y;
            _OutTangent.y = -_OutTangent.y;

            _VectorInTangent.y = -_VectorInTangent.y;
            _VectorOutTangent.y = -_VectorOutTangent.y;
        }

        /// <summary>
        /// Copies the tangent settings and values from the input key to this keyframe.
        /// </summary>
        /// <param name="key"></param>
        public void CopyTangents(in Keyframe key)
        {
            IsAutoTangents = key.IsAutoTangents;
            _InTangent = key._InTangent;
            _OutTangent = key._OutTangent;
            _VectorInTangent = key._VectorInTangent;
            _VectorOutTangent = key._VectorOutTangent;
            UnifyTangents = key.UnifyTangents;
            UnifyTangentLengths = key.UnifyTangentLengths;
            UnifyTangentLengthRatio = key.UnifyTangentLengthRatio;
            Linear = key.Linear;
            Hold = key.Hold;

            ValueChanged();
        }

        /// <summary>
        /// Scales the in and out tangent times (2D only) by the scale value.
        /// </summary>
        /// <param name="scale">A multiplier value, applying to the x (time) attribute only.</param>
        public void ScaleTangentsX(float scale)
        {
            _InTangent.x *= scale;
            if (!UnifyTangents) {
                _OutTangent.x *= scale;
            }
        }

        /// <summary>
        /// Scales the in and out tangent values (2D only) by the scale value.
        /// </summary>
        /// <param name="scale">A multiplier value, applying to the y (value) attribute only.</param>
        public void ScaleTangentsY(float scale)
        {
            _InTangent.y *= scale;
            if (!UnifyTangents) {
                _OutTangent.y *= scale;
            }
        }

    }
}