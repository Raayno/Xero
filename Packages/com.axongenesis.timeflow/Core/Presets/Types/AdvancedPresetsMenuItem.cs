// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;

namespace AxonGenesis
{
    public class AdvancedPresetsMenuItem
    {
        public object Object;
        public string Name;
        public Color Color;
        public Texture2D Icon;
        public string GUID;

        public bool IsSelected = false;

        public AdvancedPresetsMenuItem(object obj, string name, Color color, Texture2D icon, string guid = null)
        {
            Object = obj;
            Name = name;
            Color = color;
            Icon = icon;
            GUID = guid;
        }

        public void SetName(string name)
        {
            Name = name;
        }
    }

}

#endif