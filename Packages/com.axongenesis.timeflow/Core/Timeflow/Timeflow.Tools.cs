// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

#if TMPRO_3_OR_NEWER
#endif

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {

        public enum EditorTools
        {
            None,
            AddTimeflow,
            GenerateTitles,
            ScaleGlobalTime,
            CropTime
        }
        [TimeflowIgnore]
        public EditorTools EditorTool = EditorTools.None;

        [TimeflowIgnore]
        public float ScaleToolValue = 1f;

        [TimeflowIgnore]
        public bool AddEndKeysOnCrop = true;

        public Timeflow AddTimeflow()
        {
            return TimeflowEdit.AddTimeflow();
        }

    }

}//AxonGenesis

#endif