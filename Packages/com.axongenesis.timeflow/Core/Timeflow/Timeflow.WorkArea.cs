// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        #region PUBLIC

        [TimeflowIgnore]
        public bool WorkAreaDisableOnStart = true;

        [TimeflowIgnore]
        public bool WorkAreaAllowsLeadIn;

        [TimeflowIgnore]
        public bool WorkAreaLocked;

        [TimeflowIgnore]
        public bool WorkAreaPlayPastEnd;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private bool _WorkAreaEnabled;

        [SerializeField, FormerlySerializedAs("WorkAreaStart")]
        [TimeflowIgnore]
        private float _WorkAreaStart;

        [SerializeField, FormerlySerializedAs("WorkAreaEnd")]
        [TimeflowIgnore]
        private float _WorkAreaEnd;

        #endregion

        public float WorkAreaStart {
            get {
                return _WorkAreaStart;
            }
            set {
                _WorkAreaStart = value;
                if (_WorkAreaEnd <= _WorkAreaStart) {
                    _WorkAreaEnd = value + 1;
                }
            }
        }

        public float WorkAreaEnd {
            get {
                return _WorkAreaEnd;
            }
            set {
                _WorkAreaEnd = value;
                if (_WorkAreaStart >= _WorkAreaEnd) {
                    _WorkAreaStart = value - 1;
                }
            }
        }

        public bool WorkAreaEnabled {
            get {
                return _WorkAreaEnabled;
            }
            set {
                if (_WorkAreaEnabled != value) {
                    _WorkAreaEnabled = value;
                    //Debug.Log("WorkAreaEnabled: " + value);
                    ValidateWorkArea();
                }
            }
        }

        public float WorkAreaEndTimeExact {
            get {
                return WorkAreaEnd - FrameDuration;
            }
        }

        public void SetWorkArea(float start, float end, bool overrideLock)
        {
            WorkAreaEnabled = true;
            if (overrideLock && WorkAreaLocked) {
                WorkAreaLocked = false;
            }
            if (!WorkAreaLocked) {
                WorkAreaStart = start;
                WorkAreaEnd = end;
                ValidateWorkArea();
            }
        }

        public void ValidateWorkArea()
        {
            if (WorkAreaEnd < StartTime) WorkAreaEnd = StartTime;
            if (WorkAreaEnd > EndTime) WorkAreaEnd = EndTime;

            if (WorkAreaStart < StartTime) WorkAreaStart = StartTime;
            if (WorkAreaStart > EndTime) WorkAreaStart = EndTime;

            if (WorkAreaStart >= WorkAreaEnd) {
                WorkAreaEnd = WorkAreaStart + (1f / FPS);
            }
        }

        public void ToggleLoop()
        {
            LoopEnabled = !LoopEnabled;
        }

        public void LoopSelected()
        {
            LoopEnabled = true;
#if UNITY_EDITOR
            if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                View.SetWorkAreaWithSelected();
            }
#endif
        }

        public void ToggleWorkArea()
        {
            WorkAreaEnabled = !WorkAreaEnabled;
        }

        public void SetWorkAreaStart()
        {
            SetWorkAreaStart(false);
        }

        public void SetWorkAreaStartKeepDuration()
        {
            SetWorkAreaStart(true);
        }

        public void SetWorkAreaStart(bool keepDuration)
        {
            WorkAreaEnabled = true;
            if (keepDuration) {
                WorkAreaEnd = CurrentTime + (WorkAreaEnd - WorkAreaStart);
            }
            WorkAreaStart = CurrentTime;
            if (WorkAreaEnd < WorkAreaStart) {
                WorkAreaEnd = WorkAreaStart + 1;
            }
            ValidateWorkArea();
        }

        public void SetWorkAreaEnd()
        {
            SetWorkAreaEnd(false);
        }

        public void SetWorkAreaKeepDuration()
        {
            SetWorkAreaEnd(true);
        }

        public void SetWorkAreaEnd(bool keepDuration)
        {
            WorkAreaEnabled = true;
            if (keepDuration) {
                WorkAreaStart = CurrentTime - (WorkAreaEnd - WorkAreaStart);
            }
            WorkAreaEnd = CurrentTime;
            if (WorkAreaEnd <= 0) WorkAreaEnd = 1f / FPS;
            if (WorkAreaEnd < WorkAreaStart) {
                WorkAreaStart = WorkAreaEnd - 1;
            }
            if (WorkAreaStart < 0) WorkAreaStart = 0;
            ValidateWorkArea();
        }

    }

}//AxonGenesis
