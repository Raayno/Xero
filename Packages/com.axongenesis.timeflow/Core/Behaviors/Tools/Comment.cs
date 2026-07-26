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
    /// A simple utlity to add comments in the inspector on a specific game object. This can be used for
    /// documentation and conveying important notes to other team members. 
    /// </summary>
    [AddComponentMenu("Timeflow/Comment")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/comment")]
    sealed public class Comment : MonoBehaviour
    {
        public string Comments = "";
        public string Warning = "";
        public string URLTitle = "";
        public string URL = "";
        public string URL2Title = "";
        public string URL2 = "";

        void Awake()
        {
            if (Application.isPlaying) DestroyImmediate(this);
        }
    }
}