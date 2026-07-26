// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(AnimatorInfo))]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/animator-info")]
    public class AnimatorInfoEditor : Editor
    {
        public AnimatorInfo info => (AnimatorInfo)target;

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup();
            AxonGUI.LabelInline("Optimizes animation data. Please do not remove.");
            if (!info.enabled) info.enabled = true; // keep enabled to funciton properly
        }
    }

}//AxonGenesis 

#endif