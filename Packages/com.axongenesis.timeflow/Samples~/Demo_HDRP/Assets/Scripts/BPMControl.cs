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
    public class BPMControl : AxonGenesisBehavior
    {
        public Timeflow Timeflow = null;
        public float BPM = 120;

        private void Update()
        {
            if (Timeflow != null) {
                Timeflow.BPM = BPM;
            }
        }

    }
}
