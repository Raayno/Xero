// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Reflection;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Defines custom handling of transform properties to facilitate in animation and interpolation. This
    /// employs Rotation to stabilize euler rotations.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfLayerMask")]
    public class PropertiesOfLayerMask : PropertiesHandler
    {
        public LayerMask Value;

        private static SDictionary<string, Type> _List;

        private FieldInfo FieldInfo;
        private PropertyInfo PropertyInfo;

        public PropertiesOfLayerMask() { }

        public override Type GetPropertyType(string name)
        {
            return typeof(LayerMask);
        }

        public override SDictionary<string, Type> List {
            get {
                return _List;
            }
        }

        public override bool ShowDefaultProperties {
            get {
                return false;
            }
        }

        public static bool SupportsProperty(string name)
        {
            return true;
        }

        public override bool HasProperty(string name)
        {
            return true;
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                if (_Object != value) {
                    _Object = value;
                }
                if (Object != null) {
                    // Get either field info or property info
                    FieldInfo = Property.GetFieldInfoFromPropertyPath(_Object.GetType(), ref _Name);
                    if (FieldInfo == null) {
                        PropertyInfo = Property.GetPropertyInfoFromPropertyPath(_Object.GetType(), ref _Name);
                    }
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(LayerMask);
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public override int GetInt()
        {
            if (FieldInfo != null) {
                LayerMask m = (LayerMask)FieldInfo.GetValue(Object);
                return m.value;
            }
            else
            if (PropertyInfo != null) {
                LayerMask m = (LayerMask)PropertyInfo.GetValue(Object, null);
                return m.value;
            }
            return base.GetInt();
        }

        public override void SetInt(int value)
        {
            LayerMask layerMask = new LayerMask();
            layerMask.value = value;
            if (FieldInfo != null) {
                FieldInfo.SetValue(Object, layerMask);
                return;
            }
            else
            if (PropertyInfo != null) {
                PropertyInfo.SetValue(Object, layerMask, null);
                return;
            }
            base.SetInt(value);
        }
    }

}//AxonGenesis
