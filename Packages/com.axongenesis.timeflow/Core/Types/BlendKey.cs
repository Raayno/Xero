// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    /// <summary>
    /// This extends Keyframe with additional information for performing blends.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "BlendKey")]
    public class BlendKey : CustomKey
    {
        #region PUBLIC

        [SerializeField, FormerlySerializedAs("FromSet")]
        private int _FromSet;

        [SerializeField]
        public int ToSet;

        [SerializeField, FormerlySerializedAs("Hold")]
        private bool _Hold = true;

        [SerializeField]
        public bool Reverse;

        [SerializeField]
        public float Duration = 3f;

        [SerializeField]
        public bool AutoDuration = true;

        [SerializeField]
        public float StartTime;

        public string Name {
            get {
                if (Key != null) {
                    return Key.KeyString;
                }
                return "";
            }
            set {
                if (Key != null) {
                    Key.KeyString = value;
                }
            }
        }

        [SerializeField]
        public UnityEvent Event;

        [SerializeField, FormerlySerializedAs("InterpolationMode")]
        private MathUtil.InterpolationModes _InterpolationMode = MathUtil.InterpolationModes.EaseInOut;
        public MathUtil.InterpolationModes InterpolationMode {
            get {
                if (Hold) return MathUtil.InterpolationModes.None;
                return _InterpolationMode;
            }
            set {
                if (_InterpolationMode != value) {
                    _InterpolationMode = value;
                }
            }
        }

        [SerializeField]
        public AnimationCurve InterpolateCurve;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool IsEditingCurve;

        [NonSerialized]
        public Blend Blend;

        #endregion
        
        public int FromSet {
            get { return _FromSet; }
            set {
                if (_FromSet != value) {
                    _FromSet = value;
                }
            }
        }

        public bool Hold {
            get { return _Hold; }
            set {
                if (_Hold != value) {
                    _Hold = value;
                    if (Key != null) Key.Hold = value;
                }
            }
        }

        public BlendKey()
        {
        }

        public static BlendKey CreateCopy(BlendKey from)
        {
            BlendKey copy = new BlendKey();
            copy.Copy(from);
            return copy;
        }

        public override void Copy(CustomKey from)
        {
            BlendKey orig = (BlendKey)from;
            if (orig != null) {
                FromSet = orig.FromSet;
                ToSet = orig.ToSet;
                Hold = orig.Hold;
                Reverse = orig.Reverse;
                Duration = orig.Duration;
                AutoDuration = orig.AutoDuration;
                StartTime = orig.StartTime;
                Event = orig.Event;
                InterpolationMode = orig.InterpolationMode;
                InterpolateCurve = orig.InterpolateCurve;
                IsEditingCurve = false;
                Blend = orig.Blend;
            }
        }

        public float InterpolateTime(float localTime, float value, bool apply)
        {
            float interp = 0f;
            if (Duration > 0f) {
                interp = (localTime - StartTime) / Duration;
                if (interp < 0f) interp = 0f;
                else
                if (interp > 1f) interp = 1f;

                if (InterpolationMode != MathUtil.InterpolationModes.Linear) {
                    if (InterpolationMode == MathUtil.InterpolationModes.UseChannelCurve) {
                        interp = value;
                        if (interp < 0f) interp = 0f;
                        else
                        if (interp > 1f) interp = 1f;
                    }
                    else
                    if (InterpolationMode == MathUtil.InterpolationModes.AnimationCurve) {
                        if (InterpolateCurve == null) InterpolateCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);
                        interp = InterpolateCurve.Evaluate(interp);
                    }
                    else {
                        interp = MathUtil.InterpolateMode(0f, 1f, interp, InterpolationMode);
                    }
                }
            }
            if (apply) ApplyBlend(interp);
            return interp;
        }

        public void ApplyBlend(float interp)
        {
            if (Blend != null) {
                Blend.From = Blend.GetIndex(FromSet);
                Blend.To = Blend.GetIndex(ToSet);
                Blend.Hold = Hold;
                Blend.Reverse = Reverse;
                Blend.BlendAmount = interp;
                Blend.CurrentKey = this;
                Blend.UpdateSet();
            }
        }

        public void PerformTrigger()
        {
            if (Event != null) Event.Invoke();
        }
    }

}//AxonGenesis
