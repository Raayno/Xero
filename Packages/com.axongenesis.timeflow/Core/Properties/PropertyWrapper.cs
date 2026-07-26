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
    public class PropertyWrapper<T>
    {
        private IPropertyAccessor accessor;
        private T defaultValue;

        public Type ComponentType {get; private set; }

        public Type ValueType {get; private set; }

        public PropertyWrapper(Component comp, string memberName)
        {
            //Debug.Log($"PropertyWrapper<{typeof(T).Name}>.ctor({comp.GetType().Name}, {memberName})");

            ComponentType = comp.GetType();
            ValueType = typeof(T);

            //Debug.Log($"Component type: {ComponentType.FullName}, Value type: {ValueType.FullName}");

            Type accessorType = typeof(PropertyAccessor<>).MakeGenericType(ComponentType);
            //Debug.Log($"Accessor type: {accessorType.FullName}");
            accessor = (IPropertyAccessor)Activator.CreateInstance(accessorType, comp, memberName);

            if(accessor == null) {
                Debug.LogWarning($"Failed to create PropertyAccessor<{ComponentType.Name}> for {comp.GetType().Name}.{memberName}");
                return;
            }
        }

        public bool Matches(Component comp, string memberName)
        {
            return ComponentType == comp.GetType() && ValueType == typeof(T);
        }

        public T GetValue()
        {
            if (accessor == null) return defaultValue;
            return accessor.GetValue<T>();
        }

        public void SetValue(T value)
        {
            if (accessor == null) return;
            accessor.SetValue(value);
        }
    }
}