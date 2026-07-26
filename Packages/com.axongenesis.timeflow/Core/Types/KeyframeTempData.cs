// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Stores keyframe data temporarily during edit operations so as to preserve original states when the
    /// user cancels an operation.
    /// </summary>
    public class KeyframeTempData
    {
        public float KeyTime;
        public float KeyValue;
        public float KeyTimeInTimeflow;
        public float KeyEndInTimeflow;
        public Vector4 KeyVector = Vector4.zero;

        public KeyframeTempData() { }

        public KeyframeTempData(Keyframe key) { }

        public void StoreData(Keyframe key)
        {
            KeyTime = key.KeyTime;
            KeyValue = key.KeyValue;
            KeyTimeInTimeflow = key.KeyTimeWorld;
            KeyEndInTimeflow = key.KeyEndTimeWorld;
            KeyVector = key.KeyVector;
        }
    }

}//AxonGenesis
