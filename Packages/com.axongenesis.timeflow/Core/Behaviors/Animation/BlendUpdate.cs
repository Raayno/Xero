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
    /// This is a base class for extending Blend behavior. Each time the view of the
    /// Blend Set changes, OnBlendChange is called for each registered class. Override this class to
    /// implement custom behaviors.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    public class BlendUpdate : MonoBehaviour
    {
        /// <summary>
        /// Override this method to receive calls every frame of Blend transitions. Note that the BlendKey
        /// data is stored in Keyframe.CustomKey, requiring a cast to access it. Be sure to check that the
        /// key and CustomKey is not null.
        /// </summary>
        /// <param name="blend">The interpolation (0-1) of the current blend</param>
        public virtual void OnBlend(float time, Keyframe keyA, Keyframe keyB, float blend)
        {
            // Examples of how to retrieve the BlendKey data
            //BlendKey blendA = keyA == null || keyA.CustomKey == null ? null : (BlendKey)keyA.CustomKey;
            //BlendKey blendB = keyB == null || keyB.CustomKey == null ? null : (BlendKey)keyB.CustomKey;
        }

        /// <summary>
        /// Override to implement behavior whenever the blend has changed to a new view.
        /// </summary>
        public virtual void OnBlendChange()
        {
        }
    }

}//AxonGenesis
