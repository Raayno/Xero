// Copyright 2025 AxonGenesis All rights reserved.
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
    /// <summary>
    /// Do not apply this script manually to any game objects. The sole purpose of this class is to handle
    /// OnPostRender for RenderToDisk, which manages instances of this class. This behavior should not be
    /// used directly by end-users.
    /// </summary>
    [ExecuteAlways]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    [AddComponentMenu("Timeflow/Rendering/Render Camera")]
    sealed public class RenderCamera : AxonGenesisBehavior
    {
        /// <summary>
        /// Prevents component reference from being listed in property lists, since there's nothing to
        /// animate here.
        /// </summary>
        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (!Application.isPlaying) {
                EditorUtility.DisplayDialog("Invalid Render Camera Instance", "This script must not be applied directly to game objects. It is used only during runtime by RenderToDisk to receive render callbacks. This instance has been removed.", "Ok");
                DestroyImmediate(this);
            }
        }

        public void OnPostRender()
        {
            //if (DebugEnabled) Debug.Log(name + ".RenderCamera.OnPostRender");
            //RenderToDisk.Instance.OnPostRender();
        }
    }

} // AxonGenesis

#endif