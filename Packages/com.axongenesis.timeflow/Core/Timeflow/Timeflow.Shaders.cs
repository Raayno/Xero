// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        [TimeflowIgnore]
        public bool SetShaderTime;

        [TimeflowIgnore]
        public string ShaderTimeName = "_TimeflowTime";

        [TimeflowIgnore]
        public bool SetShaderFrame;

        [TimeflowIgnore]
        public string ShaderFrameName = "_TimeflowFrame";

        [NonSerialized]
        public int ShaderTimeID;

        [NonSerialized]
        public int ShaderFrameID;

        private void SetupShaderPropertyIDs()
        {
            if (SetShaderTime && ShaderTimeID == 0) {
                ShaderTimeID = Shader.PropertyToID(ShaderTimeName);
            }
            if (SetShaderFrame && ShaderFrameID == 0) {
                ShaderFrameID = Shader.PropertyToID(ShaderFrameName);
            }
        }

        public void UpdateShaderValues()
        {
            if (!SetShaderTime && !SetShaderFrame) return;

            SetupShaderPropertyIDs();

            bool changed = false;
            if (SetShaderTime) {
                changed = true;
                Shader.SetGlobalFloat(ShaderTimeID, CurrentTime);
            }
            if (SetShaderFrame) {
                changed = true;
                Shader.SetGlobalFloat(ShaderFrameID, CurrentFrame);
            }

            if (changed) {
#if UNITY_EDITOR
                /// Repainting is needed to force the shaders to update in edit mode.
                if (!Application.isPlaying) {
                    SceneView.RepaintAll();
                    if (View.GameView != null) {
                        View.GameView.Repaint();
                    }
                }
#endif
            }
        }
    }

}//AxonGenesis