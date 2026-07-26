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
    [AddComponentMenu("Timeflow/FPS Counter")]
    sealed public class FPSCounter : AxonGenesisBehavior
    {
        public Property DisplayProperty = null;
        public string TextPrefix = "FPS:";

        public bool ShowTargetFPS = false;
        public int TargetFPS = 60;
        public Color LowFPSColor = Color.red;
        public Color HighFPSColor = Color.green;
        public Property ColorProperty = null;

        #region PRIVATE

        [NonSerialized]
        private int fps;

        [NonSerialized]
        private int frameCount;

#if UNITY_EDITOR

        public bool GraphFPS = false;
        public string GraphName = "FPSGraph";

        [NonSerialized]
        private Graph graph;

        [NonSerialized]
        private bool graphEnabled = false;

#endif

        [NonSerialized]
        private float time = 0;

        #endregion

        protected override void OnDisable()
        {
            base.OnDisable();
            //StopAllCoroutines();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //StartCoroutine(_UpdateText());
#if UNITY_EDITOR
            if (GraphFPS) {
                graph = Graph.FindGraph(GraphName);
                graphEnabled = graph != null && graph.Enabled;
                if (graphEnabled) {
                    graph.ClearData(false);
                    graph.StopRecording();
                }
            }
#endif
        }

        private void Update()
        {
            frameCount++;
            if (Time.time > time) {
                time = Time.time + 1f;

                if (DisplayProperty != null) {
                    if (DisplayProperty.IsString) {
                        DisplayProperty.StringValue = TextPrefix + frameCount;
                    }
                    else {
                        DisplayProperty.IntValue = frameCount;
                    }
                }

                if (ShowTargetFPS && ColorProperty != null) {
                    ColorProperty.ColorValue = frameCount < TargetFPS ? LowFPSColor : HighFPSColor;
                }

#if UNITY_EDITOR
                if (GraphFPS && graphEnabled) {
                    graph.RecordValue(fps);
                }
#endif
                frameCount = 0;
            }
        }

//        private IEnumerator _UpdateText()
//        {
//            while (enabled) {
//                yield return new WaitForSecondsRealtime(1f);
//                yield return new WaitForEndOfFrame();

//                if (DisplayProperty != null) {
//                    if (DisplayProperty.IsString) {
//                        DisplayProperty.StringValue = TextPrefix + frameCount;
//                    }
//                    else {
//                        DisplayProperty.IntValue = frameCount;
//                    }
//                }

//                if (ShowTargetFPS && ColorProperty != null) {
//                    ColorProperty.ColorValue = frameCount < TargetFPS ? LowFPSColor : HighFPSColor;
//                }

//#if UNITY_EDITOR
//                if (GraphFPS && graphEnabled) {
//                    graph.RecordValue(fps);
//                }
//#endif
//                frameCount = 0;
//            }

//            yield break;
//        }

    }

}//AxonGenesis