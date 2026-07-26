// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a class for AnimatorInfo to store basic information about animation tracks.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AnimatorData")]
    public class AnimatorData : SerializableObject
    {
        public string Name = "";
        public int Hash;
        public float Length;
        public AnimationClip Clip;

        public AnimatorData(string name, int hash, AnimationClip clip)
        {
            Name = name;
            Hash = hash;
            if (clip != null) {
                Length = clip.length;
            }
            Clip = clip;
        }
    }

}//AxonGenesis