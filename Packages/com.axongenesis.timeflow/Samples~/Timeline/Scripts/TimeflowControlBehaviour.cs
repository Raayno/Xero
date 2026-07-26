// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using UnityEngine.Playables;

namespace AxonGenesis
{
    /// <summary>
    /// This class holds the properties used to link Timeflow and Timeline.
    /// </summary>
    [Serializable]
    public class TimeflowControlBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// If true, the start time of the clip always begins at the start of the Timeflow instance. This may
        /// be turned off to manually set a start time. 
        /// </summary>
        [Tooltip("If true, the start time of the clip always begins at the start of the Timeflow instance. This may be turned off to manually set a start time. ")]
        public bool AutoStartTime = true;

        /// <summary>
        /// If true, Timeflow instance is automatically displayed in the Timeflow view.
        /// </summary>
        [Tooltip("If true, this Timeflow instance is automatically displayed in the Timeflow view. This is an editor-only feature and has no affect on runtime behaviors.")]
        public bool ActivateTimeflow = true;

        /// <summary>
        /// Sets the start time within the Timeflow instance. Set this to start a clip in Timeline from a
        /// specific point within Timeflow.
        /// </summary>
        [Tooltip("Sets the start time within the Timeflow instance. Set this to start a clip in Timeline from a specific point within Timeflow.")]
        public float StartTime;

    }
}