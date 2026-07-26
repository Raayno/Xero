// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region PRIVATE SERIALIZED

        [SerializeField, FormerlySerializedAs("_BPMSnap")]
        private int _GridSnap = 4; // 1 measure

        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private bool _snapTimeEnabled;

        [NonSerialized]
        private bool _snapValueEnabled;

        [NonSerialized]
        private float _snap = 0.1f;

        [NonSerialized]
        private float _snapDisplayed = 1;

        #endregion

        #region ACCESSORS

        public bool SnapTimeEnabled {
            get {
                bool snap = _snapTimeEnabled && GridEnabled;

                if (Event.current != null && Event.current.alt && Input.MouseConstrainAxis < 2) {
                    snap = !snap;
                }
                return snap;
            }
            set {
                _snapTimeEnabled = value;
            }
        }

        public bool SnapValueEnabled {
            get {
                bool snap = _snapValueEnabled && GridEnabled;

                if (Event.current != null) {
                    if (Input.IsMicroAdjustMode && Event.current.alt && (Event.current.control || Event.current.command) && Event.current.shift) {
                        /// combo used for key micro adjustments
                        snap = false;
                    }
                    else
                    if (Event.current.alt && Input.MouseConstrainAxis != 1) {
                        snap = true;
                    }
                }
                return snap;
            }
            set {
                _snapValueEnabled = value;
            }
        }

        public int GridSnap {
            get {
                if (_GridSnap < 0) _GridSnap = 4; // revert to 1:1
                return _GridSnap;
            }
            set {
                _GridSnap = value;
                RecalculateSnap();
            }
        }

        public float Snap {
            get {
                if (_snap < 0.001f || MathUtil.IsNaN(_snap)) _snap = 0.001f;
                return _snap;
            }
            set {
                if (_snap != value) {
                    _snap = value;
                }
            }
        }

        /// <summary>
        /// Returns the amount of time in 1 snap distance.
        /// </summary>
        public float SnapUnit {
            get {
                return Snap;// precalculated by RecalculateSnap 
            }
        }

        #endregion

        #region SNAP METHODS

        public void RecalculateSnap()
        {
            float bps = 1f;
            if (UseMusicalTiming && GridTimeDisplay == TimeDisplayModes.Measures) {
                bps = (60f / Timeflow.BPM) * 4f;
            }

            int end = GridSnapUnits.Length - 1;

            if (_GridSnap < 0) _GridSnap = 0;
            if (_GridSnap > end) _GridSnap = end;

            Snap = GetBPMSnap(_GridSnap);
        }

        public float GetBPMSnap(int snapIndex)
        {
            float snap = 1f;
            float bps = 1f;
            if (UseMusicalTiming && GridTimeDisplay == TimeDisplayModes.Measures) {
                bps = (60f / Timeflow.BPM) * 4f;
            }

            int end = GridSnapUnits.Length - 1;

            if (snapIndex < 0) snapIndex = 0;
            if (snapIndex > end) snapIndex = end;

            switch (snapIndex) {
                case 0: // 32
                    snap = bps * 32f;
                    break;
                case 1: // 16
                    snap = bps * 16f;
                    break;
                case 2: // 8
                    snap = bps * 8f;
                    break;
                case 3: // 4
                    snap = bps * 4f;
                    break;
                case 4: // 1
                    snap = bps;
                    break;
                case 5:
                    snap = bps / 2f;
                    break;
                case 6:
                    snap = bps / 3f;
                    break;
                case 7:
                    snap = bps / 4f;
                    break;
                case 8:
                    snap = bps / 5f;
                    break;
                case 9:
                    snap = bps / 6f;
                    break;
                case 10:
                    snap = bps / 8f;
                    break;
                case 11:
                    snap = bps / 10f;
                    break;
                case 12:
                    snap = bps / 12f;
                    break;
                case 13:
                    snap = bps / 16f;
                    break;
                case 14:
                    snap = bps / 20f;
                    break;
                case 15:
                    snap = bps / 24f;
                    break;
                case 16:
                    snap = bps / 30f;
                    break;
                case 17:
                    snap = bps / 32f;
                    break;
                case 18:
                    snap = bps / 48f;
                    break;
                case 19:
                    snap = bps / 60f;
                    break;
                case 20:
                    snap = bps / 64f;
                    break;
                case 21:
                    snap = bps / Timeflow.FPS;
                    break;
                case 22:
                    snap = Timeflow.CustomSnap;
                    break;
                default:
                    snap = bps;
                    break;
            }

            return snap;
        }

        public float SnapTime(float time, bool force = false, bool applyTolerance = true)
        {
            float snappedTime = time;
            if (SnapTimeEnabled || force) {
                if (_snapDisplayed <= 0) _snapDisplayed = 1;
                float snapped = Mathf.Round(time / _snapDisplayed) * _snapDisplayed;

                if (force) {
                    // Forcing snap only operates on the grid
                    snappedTime = snapped;
                }
                else {
                    // Snap to the current time
                    if (Input.EventMode == TimeflowViewInput.EventModes.DragPlayhead) {
                        // Dragging the playhead can't snap to itself
                        snappedTime = snapped;
                    }
                    else {
                        // Snap to the playhead if it is closer than the snap point
                        snappedTime = Mathf.Abs(time - Timeflow.CurrentTime) < Mathf.Abs(time - snapped) ? Timeflow.CurrentTime : snapped;
                    }

                    float dif = Mathf.Abs(snappedTime - time);
                    if (dif > 0) {
                        // Snap to a nearer keyframe or track
                        snappedTime = Display.SnapTimeToDisplayed(time, snappedTime, dif);
                    }
                }
            }
            if (!applyTolerance) return snappedTime;
            return Timeflow.ApplyTimeTolerance(snappedTime);
        }

        public float SnapTimePosition(float posx, bool force = false)
        {
            float time = TimeOfPosition(posx, false, force || SnapTimeEnabled);
            return PositionOfTime(time, false);
        }

        public float SnapValue(float value, bool force = false)
        {
            if (SnapValueEnabled || force) {
                float v = MathUtil.Snap(value, GraphSnap);
                value = v;
            }
            return value;
        }

        #endregion
    }

}//AxonGenesis

#endif
