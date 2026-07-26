// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace AxonGenesis
{
    /// <summary>
    /// Defines a specific group of objects to view in the Timeflow window.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "TimeflowDisplay")]
    public class TimeflowDisplayItem : SerializableObject
    {
        public string Name;
        public List<TimeflowObject> Objects;
        public bool ShowObjects = true;
        public bool IsTimeScopeEnabled = false;
        public bool IsTimeScopeLocalized = false;
        public float TimeScopeStart = 0;
        public float TimeScopeEnd = 1;
    }

}//AxonGenesis

#endif
