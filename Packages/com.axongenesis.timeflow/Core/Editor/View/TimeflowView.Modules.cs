// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        [NonSerialized]
        private bool _IsKeyframeTools;

        #region MODULES

        [SerializeField]
        private TimeflowViewLayout _Layout;

        [SerializeField]
        private TimeflowViewDisplay _Display;

        [NonSerialized]
        private TimeflowViewInput _Input;

        [NonSerialized]
        private TimeflowViewInfo _Info;

        [NonSerialized]
        private TimeflowViewMarkers _Markers;

        [NonSerialized]
        private TimeflowViewAlignTools _AlignTools;

        [NonSerialized]
        private TimeflowViewKeyframeTools _KeyframeTools;

        public TimeflowViewLayout Layout {
            get {
                if (_Layout == null) _Layout = new TimeflowViewLayout(Timeflow);
                return _Layout;
            }
        }

        public TimeflowViewDisplay Display {
            get {
                if (_Display == null) _Display = new TimeflowViewDisplay(Timeflow);
                return _Display;
            }
        }

        public TimeflowViewInput Input {
            get {
                if (_Input == null) _Input = new TimeflowViewInput(Timeflow);
                return _Input;
            }
        }

        public TimeflowViewInfo Info {
            get {
                if (_Info == null) _Info = new TimeflowViewInfo(Timeflow);
                return _Info;
            }
        }

        public TimeflowViewMarkers Markers {
            get {
                if (_Markers == null) _Markers = new TimeflowViewMarkers(Timeflow);
                return _Markers;
            }
        }

        public TimeflowViewAlignTools AlignTools {
            get {
                if (_AlignTools == null) _AlignTools = new TimeflowViewAlignTools(Timeflow);
                return _AlignTools;
            }
        }

        public TimeflowViewKeyframeTools KeyframeTools {
            get {
                if (_KeyframeTools == null) _KeyframeTools = new TimeflowViewKeyframeTools(Timeflow);
                return _KeyframeTools;
            }
        }
        #endregion

        private void SetupModules()
        {
            if (Timeflow == null) return;
            Timeflow.View = this;

            Layout.Setup(Timeflow);
            Display.Setup(Timeflow);
            Input.Setup(Timeflow);
            Info.Setup(Timeflow);
            Markers.Setup(Timeflow);
            AlignTools.Setup(Timeflow);
            KeyframeTools.Setup(Timeflow);
        }

        public bool IsKeyframeTools {
            get {
                return _IsKeyframeTools;
            }
            set {
                if (_IsKeyframeTools != value) {
                    _IsKeyframeTools = value;
                    if (_IsKeyframeTools) {
                        KeyframeTools.Setup();
                    }
                }
            }
        }
    }

}//AxonGenesis

#endif
