// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    public interface IBehaviorProperties
    {
#if UNITY_EDITOR
        bool ArePropertiesHidden { get; }
        List<string> PropertiesList { get; }
        void OnPropertyChanged(Property property, Property.PropertyTypes originalType, int originalAttribute);
        SDictionary<string, Type> GetProperties();
#endif
        Component GetComponent();
        GameObject GetGameObject();
        Type GetComponentType();
    }
}