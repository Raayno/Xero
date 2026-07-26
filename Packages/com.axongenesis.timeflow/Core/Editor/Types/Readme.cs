// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a class for the editor to display a formatted readme file in the project view.
    /// </summary>
    public class Readme : ScriptableObject
    {
        public Texture2D icon;
        public string title;
        public Section[] sections;
        public bool loadedLayout;
        public bool isEditing = false;

        [Serializable]
        public class Section
        {
            public string heading;
            
            [TextArea] public string text;
            public string linkText, url;
        }
    }

}//AxonGenesis

#endif