// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class OptimizedBehavior : MonoBehaviour
    {
        public bool DebugEnabled = false;
        public bool CanUpdateWithoutTimeflow = false;

        public bool CanUpdate {
            get {
                if (CanUpdateWithoutTimeflow) return true;
                if (Time.captureFramerate > 0) return true; // Update while rendering
                if (Timeflow.Active == null) return false;
                if (Timeflow.Active.IsPlaying) return true;
                return false;
            }
        }
    }
}
