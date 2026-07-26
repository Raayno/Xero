// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
#if UNITY_EDITOR
#endif
namespace AxonGenesis
{
    [Serializable]
    public class TrackColorDefinition
    {
        public string Name = "Name";
        public Color Color = Color.white;

        [ComponentType] public string ComponentType;
        [ChannelType] public string ChannelType;

        public bool Hidden = false;
        public bool Skip = false;

        public int ColorSort = 0;
        public int TypeSort = 0;

        [NonSerialized] public Type Type;

        public TrackColorDefinition()
        {
            Name = "Color Name";
            Hidden = false;
            Skip = false;
            Color = Color.white;
            ComponentType = null;
            ChannelType = null;
        }

        public string DisplayName()
        {
            string name = Name;
            if (!string.IsNullOrEmpty(ComponentType)) {
                return $"{name} ({SimplifiedTypeName(ComponentType)})";
            }
            if (!string.IsNullOrEmpty(ChannelType)) {
                return $"{name} ({SimplifiedTypeName(ChannelType)})";
            }

            return name;
        }

        public static string SimplifiedTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            string[] sections = typeName.Split(',');
            string[] parts = sections[0].Split('.');
            return parts.Length > 0 ? parts[parts.Length - 1] : typeName;
        }


    }
}
