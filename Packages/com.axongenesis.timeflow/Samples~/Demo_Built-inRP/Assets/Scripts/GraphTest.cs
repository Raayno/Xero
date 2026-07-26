// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is an example showing how to record Graph data from a custom script.
    /// </summary>
    [ExecuteInEditMode]
    public class GraphTest : MonoBehaviour
    {
        public Graph Graph;

        private void Update()
        {
            if (Graph != null) {
                Graph.RecordValue(Random.value);
            }
        }

    }

}//AxonGenesis

#endif