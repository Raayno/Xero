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
    /// This defines a scriptable object for storing information used in the Timeflow Demo App.
    /// </summary>
    [CreateAssetMenu]
    public class DemoPanelContent : ScriptableObject
    {
        [Header("SIZE")]
        public float BaseScale = 1f;
        public float Width = 1f;
        public float Height = 1f;

        [Header("CONTENT")]
        public string TitleText = "TITLE";
        public float TitleFontSize = 24f;
        public float TitleOffset = 0.1f;

        [TextArea]
        public string BodyText = "Description goes here";
        public float BodyFontSize = 24f;
        public float BodyOffset = -0.3f;

        public bool ShowFooter = true;
        public float FooterScale = 1f;
        public float FooterOffset;

        public bool ShowLoadButton = true;
        public string LoadText = "LOAD DEMO";
        public bool DimScreen;
    }

}//AxonGenesis