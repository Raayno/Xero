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
    /// <summary>
    /// Base class for implementing custom property read/write behavior. The Property class by default uses
    /// reflection to find the read/write fields and properties of an object, however this may not be
    /// suitable in all cases. So instead, you may create a derrived version of this class to implement
    /// custom behavior.  IMPORTANT: Your class must be in the AxonGenesis namespace to work and be named
    /// in the format: PropertiesOfType, where Type is the name of your component. This handler then is
    /// used for all instances of that component type.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesHandler")]
    public class PropertiesHandler : SerializableObject
    {
        #region PRIVATE SERIALIZED

        /// <summary>
        /// Name refers to the target property name, such as "Local Position". This is serialized so that
        /// handlers remember their assignments once created. Each property that uses a handler will have
        /// its own instance of the handler.
        /// </summary>
        [SerializeField]
        protected string _Name;

        [SerializeField]
        protected Component _Object;

        #endregion

        #region ACCESSORS

        /// <summary>
        /// This is a list of property names and types supported by the handler. Each handler must publish
        /// its own list and implement read and writing to those property values. The Name is used to
        /// identify which property in the list is being targetted. 
        /// </summary>
        public virtual SDictionary<string, Type> List {
            get {
                return null;
            }
        }

        /// <summary>
        /// This method can be overridden to return customized names for specific properties. For example,
        /// instead of displaying "localPosition", the property handler can rename it "Local Position",
        /// which works both when reading and writing the property value. For a working example, see the
        /// TransformProperties class.
        /// </summary>
        public virtual string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        /// <summary>
        /// A reference to the component which the handler operates on.
        /// </summary>
        public virtual Component Object {
            get {
                return _Object;
            }
            set {
                _Object = value;
            }
        }

        /// <summary>
        /// Derrived classes must override this to return their type, which is how the Property knows what
        /// the handler is for.
        /// </summary>
        public virtual Type ObjectType {
            get {
                return null;
            }
        }

        /// <summary>
        /// Override to enable showing default object properties, in addition to properties provied by this
        /// handler.
        /// </summary>
        public virtual bool ShowDefaultProperties {
            get {
                return false;
            }
        }

        /// <summary>
        /// Returns whether the specified property is handled by this handler
        /// </summary>
        public virtual bool HasProperty(string name)
        {
            return false;
        }

        public virtual string PathName {
            get {
                if (Object != null) {
                    return ObjectUtil.GetPath(Object.gameObject);
                }
                return "" + GetType();
            }
        }

        #endregion

        #region SETUP

        /// TODO: Add a CreateDefaultList() option to build a list of all existing properties using
        /// reflection, so one could create a handler which only changes/omits certain properties, but lets
        /// all others pass through as they are.

        /// <summary>
        /// Override to define a custom list of supported properties. Each property must also be
        /// implemented in GetValue and SetValue
        /// </summary>
        /// <returns>A hashtable listing name/type pairs of the supported properties. Ex. "localPosition",
        ///     typeof(Vector3)</returns>
        public virtual SDictionary<string, Type> GetProperties()
        {
            //Debug.Log("PropertiesHandler.GetProperties");
            return List;
        }

        /// <summary>
        /// Returns the type specified by the name, looking up the type defined in the CreateList method.
        /// </summary>
        public virtual Type GetPropertyType(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Type type = null;
            if (List != null && List.ContainsKey(name)) {
                type = (Type)List[name];
                Name = name;
            }
            else {
                string altName = name.Replace("(Clone)", "");

                if (List != null && List.ContainsKey(altName)) {
                    type = (Type)List[altName];
                    Name = altName;
                }
                //else {
                //    ShowWarning("PropertiesHandler(" + GetType() + ").GetPropertyType '" + name + "' is missing from List");
                //}
            }
            return type;
        }

        public virtual void Refresh()
        {
            //Debug.Log($"<color=yellow><PropertyHandler.Refresh (base)</color>");
        }

        #endregion

        #region VALUES

        /// <summary>
        /// Each handler type must override the following functions as needed to implement the supported
        /// the data types. 
        /// </summary>
        /// <returns></returns>

        public virtual UnityEngine.Object GetObject()
        {
            LogWarning("PropertiesHandler.GetValue", "unimplemented virtual method: " + PathName);
            return null;
        }

        public virtual Vector4 GetVector()
        {
            LogWarning("PropertiesHandler.GetValue", "unimplemented virtual method: " + PathName);
            return Vector4.zero;
        }

        public virtual bool GetBool()
        {
            LogWarning("PropertiesHandler.GetBool", "unimplemented virtual method: " + PathName);
            return false;
        }

        public virtual int GetInt()
        {
            LogWarning("PropertiesHandler.GetInt", "unimplemented virtual method: " + PathName);
            return 0;
        }

        public virtual float GetFloat()
        {
            LogWarning("PropertiesHandler.GetFloat", "unimplemented virtual method: " + PathName);
            return 0f;
        }

        public virtual Color GetColor()
        {
            LogWarning("PropertiesHandler.GetColor", "unimplemented virtual method: " + PathName);
            return Color.black;
        }

        public virtual void SetObject(UnityEngine.Object value)
        {
            LogWarning("PropertiesHandler.SetVector", "unimplemented virtual method: " + PathName);
        }

        public virtual void SetVector(Vector4 value, int attribute)
        {
            LogWarning("PropertiesHandler.SetVector", "unimplemented virtual method: " + PathName);
        }

        public virtual void SetBool(bool value)
        {
            LogWarning("PropertiesHandler.SetBool", "unimplemented virtual method: " + PathName);
        }

        public virtual void SetInt(int value)
        {
            LogWarning("PropertiesHandler.SetInt", "unimplemented virtual method: " + PathName);
        }

        public virtual void SetFloat(float value)
        {
            LogWarning("PropertiesHandler.SetFloat", "unimplemented virtual method: " + PathName);
        }

        public virtual void SetColor(Color value)
        {
            LogWarning("PropertiesHandler.SetColor", "unimplemented virtual method: " + PathName);
        }

        #endregion

        private List<string> _Warnings = new List<string>();

        // Logs warning just once per instance to avoid excessive logging
        protected void LogWarning(string name, string warning, UnityEngine.Object context = null)
        {
            if (!_Warnings.Contains(name)) {
                _Warnings.Add(name);
                if (context == null) context = _Object;
                Debug.LogWarning($"{name} : {warning}", context);
            }
        }
    }

}//AxonGenesis
