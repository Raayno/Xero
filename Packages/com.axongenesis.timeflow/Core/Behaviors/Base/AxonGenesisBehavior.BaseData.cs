// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    public partial class AxonGenesisBehavior : MonoBehaviour
    {
        public BaseData GetBaseData()
        {
            return new BaseData(this);
        }

        public void ApplyBaseData(BaseData data)
        {
            data.Apply(this);
        }

        [Serializable]
        public class BaseData
        {
            public bool DebugEnabled;
            public AxonGenesisBehavior.PlaybackModes PlaybackMode;

#if UNITY_EDITOR
            public bool EditorShowUI;
#endif

            public BaseData(AxonGenesisBehavior obj)
            {
                DebugEnabled = obj.DebugEnabled;
                PlaybackMode = obj.PlaybackMode;

#if UNITY_EDITOR
                EditorShowUI = obj.EditorShowUI;
#endif
            }
            public void Apply(AxonGenesisBehavior obj)
            {
                obj.DebugEnabled = DebugEnabled;
                obj.PlaybackMode = PlaybackMode;

#if UNITY_EDITOR
                obj.EditorShowUI = EditorShowUI;
#endif
            }
        }
    }
}