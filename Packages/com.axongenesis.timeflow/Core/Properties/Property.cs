// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

//#define AXON_LEGACY_PROPERTIES

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{

    /// <summary>
    /// The Property class uses reflection to access virtually any property value of any component. Since
    /// Property is a serializable class, it can be used to securely store references to properties across
    /// objects. Property values can either be single attribute (ie. X, or Y, or Z) or they can be
    /// multi-attribute to work with complex values such as Vector4 and Rect. For more specialized cases, a
    /// property handler can be written to define custom read/write procedures for specific object types.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "Property")]
    public partial class Property : SerializableObject
    {
        #region CONSTANTS

        private const BindingFlags _bindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        private const BindingFlags _bindingFlagsStandard = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        #endregion

        #region ENUMS

        public enum PropertyTypes
        {
            Auto,
            Bool,
            Int,
            Float,
            Vector2,
            Vector3,
            Vector4,
            Color,
            Rect,
            RectOffset,
            GameObject,
            Component,
            String,
            Enum,
            Object
        }

        public enum PropertyFilters
        {
            All,
            NumericOnly,
            ObjectOnly,
            ColorOnly
        }

        #endregion

        #region PUBLIC SERIALIZED

        /// <summary>
        /// Determines whether the property is active. If not enabled, fields are read or written
        /// </summary>
        [SerializeField, FormerlySerializedAs("IsEnabled")]
        private bool _IsEnabled = true;

        /// <summary>
        /// Set this to false to disallow the PropertyMenu so that the user cannot change the mapping.
        /// </summary>
        public bool CanBeAssigned = true;

        /// <summary>
        /// Set this to true for any Property which is used for data only and does not map to an object or
        /// component property. 
        /// </summary>
        public bool IsDataOnly;

        /// <summary>
        /// Handlers can be disabled for properties that need custom data control. Otherwise this setting
        /// should be left on. See PropertiesHandler for more info.
        /// </summary>
        public bool EnableHandlers = true;

        /// <summary>
        /// Except when using data only, properties map to a specific component, whose reference is stored
        /// here.
        /// </summary>
        [FormerlySerializedAs("ObjRef")]
        public Component Component;

        /// <summary>
        /// If enabled, the component name is prefixed in front of the property name. This is useful when
        /// working with multiple properties to differentiate objects by showing their names.
        /// </summary>
        public bool ShowComponentName;

        /// <summary>
        /// The attribute defines a specific axis of a complex value. A value of -1 represents 
        /// a combined value, and -2 is a uniform value. Values 0-3 represent axes.
        /// </summary>
        public int Attribute = -1;

        /// <summary>
        /// Forcing the data type may be used to override the type provided by the field or property and
        /// force it to another format. This is only used in special cases to overcome incorrectly labeled
        /// fields, such as forcing a Range (XY) value to a float when working with materials.
        /// </summary>
        public PropertyTypes ForcePropertyType = PropertyTypes.Auto;

        /// <summary>
        /// Filters provide a way of grouping numeric properties separately from object properties. This
        /// allows behaviors to work on one or both types.
        /// </summary>
        public PropertyFilters PropertyFilter = PropertyFilters.All;

        [FormerlySerializedAs("ChannelLinkID")]
        public string LinkID = null;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool IsPrepared;

        [NonSerialized]
        public FieldInfo FieldInfo;

        [NonSerialized]
        public PropertyInfo PropertyInfo;

        [NonSerialized]
        public GameObject AssignToObject;

        /// <summary>
        /// The owner is the component which manages the property and is likely different from the target
        /// object. The main purpose of this is to provide the editor callback NotifyChange whenever
        /// property assignemnts are changed.
        /// </summary>
        [NonSerialized]
        public IBehaviorProperties Owner;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private string _Name;

        [SerializeField]
        private string _DisplayName;

        [SerializeField]
        private bool _DebugEnabled;

        [SerializeField]
        private bool _IsMaterial;

        [SerializeField, FormerlySerializedAs("PropertyType")]
        private PropertyTypes _PropertyType = PropertyTypes.Auto;

        /// <summary>
        /// This stores the data type as a string for reliable recovery of the data type after
        /// deserialization. During runtime, this is the only method used to determine the data type.
        /// </summary>
        [SerializeField, FormerlySerializedAs("DataTypeName")]
        private string _DataTypeName = "";

        #endregion

        #region PRIVATE NON-SERIALIZED

        /// <summary>
        /// Using SerializeReference allows this to store references and data for derrived classes.
        /// Otherwise it only serializes as the base class and loses everything else.
        /// </summary>
        [SerializeReference]
        private PropertiesHandler _Handler;

        [NonSerialized]
        private Type _dataType;

        [NonSerialized]
        private Vector4 _value = Vector4.zero;

        [NonSerialized]
        private Enum _enumValue;

        [NonSerialized]
        private string _stringValue;

        [NonSerialized]
        private UnityEngine.Object _objectValue;

        [NonSerialized]
        private GameObject _gameObjectValue;

        [NonSerialized]
        private Component _componentValue;

        [NonSerialized]
        private IPropertyLinkable _linkedTo;

        [NonSerialized]
        private string _PathName;

        [NonSerialized]
        private string _NameAndAttribute;

#if !AXON_LEGACY_PROPERTIES
        [NonSerialized] private PropertyWrapper<bool> BoolWrapper = null;
        [NonSerialized] private PropertyWrapper<int> IntWrapper = null;
        [NonSerialized] private PropertyWrapper<Enum> EnumWrapper = null;
        [NonSerialized] private PropertyWrapper<float> FloatWrapper = null;
        [NonSerialized] private PropertyWrapper<Color> ColorWrapper = null;
        [NonSerialized] private PropertyWrapper<Vector2> Vector2Wrapper = null;
        [NonSerialized] private PropertyWrapper<Vector3> Vector3Wrapper = null;
        [NonSerialized] private PropertyWrapper<Vector4> Vector4Wrapper = null;
        [NonSerialized] private PropertyWrapper<Rect> RectWrapper = null;
        [NonSerialized] private PropertyWrapper<RectOffset> RectOffsetWrapper = null;
        [NonSerialized] private PropertyWrapper<string> StringWrapper = null;
        [NonSerialized] private PropertyWrapper<UnityEngine.Object> ObjectWrapper = null;
        [NonSerialized] private PropertyWrapper<GameObject> GameObjectWrapper = null;
        [NonSerialized] private PropertyWrapper<Component> ComponentWrapper = null;
#endif

        #endregion

        #region CONSTRUCTORS

        public Property() { }

        public Property(GameObject obj)
        {
            _Name = null;
            Component = null;
            Prepare();
        }

        public Property(Component obj)
        {
            _Name = null;
            Component = obj;
            Prepare();
        }

        public Property(Component obj, string name)
        {
            _Name = name;
            Component = obj;
            Prepare();
        }

        public Property(Component obj, string name, int attribute)
        {
            //Debug.Log($"<color='green'>new Property:{name} attribute:{attribute}</color>");
            _Name = name;
            Component = obj;
            Attribute = attribute;
            Owner = obj as IBehaviorProperties;
            AssignToObject = obj.gameObject;
            Prepare();
        }

        public Property(Property copy)
        {
            Copy(copy);
        }

        public Property(GameObject obj, Property copy)
        {
            Copy(copy);
            if (obj != null) {
                SwitchGameObject(obj);
            }
        }

        public Property(IBehaviorProperties owner, Property copy)
        {
            Copy(copy);
            Owner = owner;
            if (owner != null) {
                SwitchGameObject(owner.GetGameObject());
            }
        }

        public void Copy(Property copy)
        {
            if (copy != null) {
                foreach (FieldInfo f in typeof(Property).GetFields(_bindingFlagsAll)) {
                    if (!f.IsStatic) {
                        f.SetValue(this, f.GetValue(copy));
                    }
                }
                Owner = copy.Owner;
                IsPrepared = false; // Causes deferred setup so other attributes can be set before Prepare()
                Comp = copy.Comp;
                DisplayName = copy.DisplayName;
                _Name = copy._Name;
                Attribute = copy.Attribute;
                _IsMaterial = copy.IsMaterial;
                DataTypeName = copy.DataTypeName;
                DataType = copy.DataType;
                PropertyType = copy.PropertyType;
                IsDataOnly = copy.IsDataOnly;
                LinkID = copy.LinkID;
                IsUniformValue = copy.IsUniformValue;
                _objectValue = copy._objectValue;
                _gameObjectValue = copy._gameObjectValue;
                _componentValue = copy._componentValue;
                _Handler = copy._Handler;

#if UNITY_EDITOR
                _displayNameType = null;
#endif
            }
        }

        #endregion

        #region ACCESSORS

        public bool IsEnabled {
            get { return _IsEnabled; }
            set {
                if (_IsEnabled != value) {
                    _IsEnabled = value;
                }
            }
        }

        /// <summary>
        /// If enabled, this property is mapped to a specific channel link. Since links are not components,
        /// this is handled differently than other property types.
        /// </summary>
        public bool IsLinked {
            get {
                return !string.IsNullOrEmpty(LinkID);
            }
        }

        /// <summary>
        /// Makes sure channel and link are assigned before attempting to access values.
        /// </summary>
        public bool IsLinkValid {
            get {
                return IsLinked && _linkedTo != null && _linkedTo.IsValid;
            }
        }

        private string DataTypeName {
            get {
                return _DataTypeName;
            }
            set {
                if (_DataTypeName != value) {
                    _DataTypeName = value;
                }
            }
        }

        public PropertyTypes PropertyType {
            get {
                return _PropertyType;
            }
            set {
                if (value != _PropertyType) {
                    _PropertyType = value;
                    //Debug.Log($"_PropertyType:{value}");
                }
            }
        }

        /// <summary>
        /// The main component to which values are being read/written. 
        /// </summary>
        public Component Comp {
            get {
                return Component;
            }
            set {
                if (Component != value) {
                    Handler = null;
                    Component = value;
                    _PathName = null;
                    GetDataType();
                    FindPropertiesHandler(true);
                }
            }
        }

        public GameObject GameObject {
            get {
                return Comp == null ? AssignToObject : Comp.gameObject;
            }
        }

        /// <summary>
        /// The exact name of the property being read and written. This name is the sole way in which
        /// property mappings are made, so if anything changes the names (such as updates to source code
        /// with changes to variable names) connections will be broken.
        /// </summary>
        public string Name {
            get {
                if (string.IsNullOrEmpty(_Name)) _Name = GetNameAndAttribute(null, true, true, false);
                return _Name;
            }
            set {
                _Name = value;
                if (string.IsNullOrEmpty(_Name)) _Name = GetNameAndAttribute(null, true, true, false);
                if (EnableHandlers && !IsDataOnly && Handler != null) {
                    Handler.Name = _Name;
                    _Name = Handler.Name;
                }
                Prepare();
            }
        }

        public string NameAndAttribute {
            get {
                if (string.IsNullOrEmpty(_NameAndAttribute)) {
                    _NameAndAttribute = GetNameAndAttribute(null, true, true, false);
                }
                return _NameAndAttribute;
            }
        }

        /// <summary>
        /// Presents a formatted name with attribute for display purposes. This is a way to show a more
        /// user-friendly name than the original property name.
        /// </summary>
        public string DisplayName {
            get {
                if (string.IsNullOrEmpty(_DisplayName)) {
                    _DisplayName = GetNameAndAttribute(null, true, true, false);
                }
                return _DisplayName;
            }
            set {
                if (_DisplayName != value) {
                    _DisplayName = value;
                }
            }
        }

        public bool DebugEnabled {
            get {
                return _DebugEnabled && TimeflowPreferences.DebugEnabled;
            }
            set {
                _DebugEnabled = value;
            }
        }

        /// <summary>
        /// Returns a reference to the current PropertiesHandler. Most often this is null and is only used
        /// for special component types for which a handler is defined. For performance reasons, this does
        /// not automatically look for the handler. FindPropertiesHandler does that and is called
        /// automatically upon setting up the Property or any changes to it's destination.
        /// </summary>
        public PropertiesHandler Handler {
            get {
                if (!EnableHandlers || IsDataOnly) _Handler = null;
                return _Handler;
            }
            set {
                _Handler = value;
                if (value == null) _Handler = null;
            }
        }

        #endregion

        #region PROPERTY MAPPING

        public void Refresh()
        {
            //Debug.Log($"<color=lime><Property.Refresh: {PathName()}</color>");
            if (Handler != null) {
                Handler.Refresh();
            }
        }

        /// <summary>
        /// Performs basic setup to prepare the property for reading and writing. Use this as bascially a
        /// WakeUp type function and call it once before doing any read/write operations.
        /// </summary>
        public void Prepare()
        {
            IsPrepared = true;
            _PathName = null;
            DataType = null;

            _NameAndAttribute = null;
            GetNameAndAttribute();

            if (IsLinked) {
                if (_linkedTo == null && GameObject != null) {
                    if (GameObject.TryGetComponent<IHasPropertyChannels>(out var obj)) {
                        _linkedTo = obj.GetLinkableByID(LinkID);
                    }
                }
                if (_linkedTo == null) {
                    Debug.LogWarning("Property failed to find linked channel: " + LinkID);
                }
            }
            else
            if (!IsDataOnly) {
                FindPropertiesHandler(true);
                GetDataType();
            }
            if (Handler != null) {
                Handler.Refresh();
            }

        }

        /// <summary>
        /// A property mapping only works if there is an object and the property. If either is missing then
        /// the connection cannot be made.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (IsDataOnly) return true;

            if (Comp == null) {
                return false;
            }
            if (!Comp.gameObject.activeSelf) {
                return false;
            }
            if (string.IsNullOrEmpty(_Name)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Looks for a component matching the current one on the new object. If the object has multiple
        /// instances of the same component, then it will get the one at the same index or closest match.
        /// Note too that if all you are doing is replacing the Obj reference with a different component,
        /// there is no need to use this method as you can simply assign via property.Obj = yourComponent.
        /// </summary>
        public bool SwitchGameObject(GameObject obj)
        {
            bool success = false;
            if (!IsDataOnly && Comp != null && obj != null) {
                AssignToObject = obj;
                Type componentType = Comp.GetType();
                Component[] srcComponents = Comp.GetComponents(componentType);
                Component[] dstComponents = obj.GetComponents(componentType);
                if (srcComponents != null && srcComponents.Length > 0 && dstComponents != null && dstComponents.Length > 0) {
                    int i = 0;
                    foreach (Component c in srcComponents) {
                        if (c == Comp) break;
                        i++;
                    }
                    if (i > dstComponents.Length) {
                        Comp = null;
                    }
                    else {
                        Comp = dstComponents[i];
                    }
                    success = true;
                }
                else {
                    Comp = obj.transform;
                }

                Handler = null; // will rebuild handler 
            }

#if UNITY_EDITOR
            ShowPropertyObject = true; // Show the game object assigned
#endif
            Prepare();
            //Debug.Log($"IsDataOnly:{IsDataOnly} success:{success} Comp:{(Comp == null ? "null" : Comp.GetType().Name)}");
            return success;
        }

        /// <summary>
        /// Searches for a PropertiesHandler that handles the component type and property. 
        /// </summary>
        /// <param name="mustSupportProperty">If set to true, the handler is only assigned if the property
        ///     Name is supported by the handler. Set to false if you want the handler for the type and
        ///     don't care about the specific property.name</param>
        private PropertiesHandler FindPropertiesHandler(bool mustSupportProperty)
        {
            mustSupportProperty = false;
            if (Handler != null && Handler.GetType() == typeof(PropertiesHandler)) {
                // Prevent loading the base class PropertiesHandler
                Handler = null;
            }
            if (Handler != null) {
                if (mustSupportProperty && !Handler.HasProperty(Name)) {
                    Handler = null;
                }
                if (Handler != null) {
                    // Make sure handler object is set
                    Handler.Object = Comp;
                }
                return Handler;
            }
            if (IsDataOnly || !EnableHandlers || Comp == null) {
                Handler = null;
                return null;
            }

            string typeName = IsMaterial ? "Material" : StringUtil.ClassName(Comp.GetType().ToString());
            if (string.IsNullOrEmpty(typeName)) {
                Handler = null;
            }
            else {
                GetHandlerForTypeName(mustSupportProperty, typeName);
                if (Handler == null && DataType != null) {
                    GetHandlerForTypeName(mustSupportProperty, DataType.ToString());
                }
            }

            if (Handler != null) {
                DataType = Handler.GetPropertyType(_Name);
                _Name = Handler.Name;
                if (Handler.Object != Comp) Handler.Object = Comp;
            }
            //Debug.Log($"FindPropertiesHandler:{Name} typeName:{typeName} DataType:{DataType} mustSupportProperty:{mustSupportProperty} IsDataOnly:{IsDataOnly} EnableHandlers:{EnableHandlers} Comp:{(Comp == null ? "NULL" : Comp.GetType())} Handler:{(Handler == null ? "NULL" : Handler.GetType())}");

            return Handler;
        }

        private void GetHandlerForTypeName(bool mustSupportProperty, string typeName)
        {
            if (typeName.Contains(".")) {
                typeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            }
            /// This assumes that every property handler has a name following the format:
            /// PropertiesOfType where Type is the name of the component it handles properties for.
            /// This applies then to all components of that type.
            string handlerName = "AxonGenesis.PropertiesOf" + typeName;

            Type handlerType = Type.GetType(handlerName);
            if (Handler != null && handlerType == Handler.GetType()) {
                //Debug.Log($"Handler already set to {handlerType}");
                return;
            }
            //Debug.Log($"<color='cyan'>handlerType:{handlerType}:{(handlerType == null ? "NULL" : "")} handlerName:{handlerName} Comp:{Comp}</color>");
            if (handlerType != null) {
                //Debug.Log($"<color='green'>GetHandlerForTypeName:{typeName} Name:{Name}</color>");
                Handler = Activator.CreateInstance(handlerType) as PropertiesHandler;
                Handler.Object = Comp;

                if (!mustSupportProperty || Handler.HasProperty(_Name)) {
                    //Debug.Log($"<color='yellow'>GetHandlerForTypeName:{typeName} Name:{Name}</color>");
                    Handler.Name = Name;
                    _Name = Handler.Name; // assign back in case value has been remapped by handler
                }
                else {
                    /// Only use the handler if it supports the target property. Just because a handler
                    /// was found for the component type does not mean the handler implements the
                    /// value.
                    Handler = null;
                }
            }
        }

        #endregion

        #region NAME AND PATH

        public void ResetName(bool showObjectName)
        {
            ShowComponentName = showObjectName;
            _DisplayName = Name = GetNameAndAttribute(null, true, true, false, showObjectName);
#if UNITY_EDITOR
            _displayNameType = null;
#endif
        }

        /// <summary>
        /// The name of the property plus the name of the value attribute (ex. 'Position X'), if one is
        /// specified.
        /// </summary>
        public string GetNameAndAttribute() { return GetNameAndAttribute("(Unassigned)", true, false, true); }

        public string GetNameAndAttribute(string unassignedValue) { return GetNameAndAttribute(unassignedValue, true, false, true); }

        public string GetNameAndAttribute(string unassignedValue, bool showAttribute, bool force, bool showType)
        {
            return GetNameAndAttribute(unassignedValue, showAttribute, force, showType, ShowComponentName);
        }

        private string GetAttributeName()
        {
            return GetAttributeName(Attribute);
        }

        private string GetAttributeName(int attribute)
        {
            string attributeName = "";
            if (IsMultiNumeric(DataType)) {
                if (attribute == -2) {
                    attributeName = " Uniform";
                }
                else
                if (attribute == -1) {
                    attributeName = "";
                }
                else {
                    string[] names = Property.GetAttributeNames(DataType, true);
                    if (names != null && names.Length >= 0) {
                        if (attribute > names.Length) attribute = -1;
                        if (attribute >= 0) attributeName = " " + names[attribute];
                    }
                }
            }
            return attributeName;
        }

        public string GetNameAndAttribute(string unassignedValue, bool showAttribute, bool force, bool showType, bool showObjectName)
        {
            if ((!IsDataOnly && Comp == null) || string.IsNullOrEmpty(_Name)) {
                //Debug.LogWarning($"Property.GetNameAndAttribute: Comp:{Comp} _Name:{_Name}");
                _NameAndAttribute = null;
                return unassignedValue;
            }
            else
            if (force || string.IsNullOrEmpty(_NameAndAttribute)) {
                FindPropertiesHandler(true);
                GetDataType(force);

                string attributeName = showAttribute ? GetAttributeName() : "";
                _NameAndAttribute = Name + attributeName;
                if (showType) {
                    string dname = StringUtil.ClassName("" + PropertyType);
                    _NameAndAttribute += " (" + dname + ")";
                }
            }
            if (!string.IsNullOrEmpty(_NameAndAttribute)) {
                if (string.IsNullOrEmpty(_DisplayName)) {
                    if (showObjectName && Comp != null) {
                        _DisplayName = Comp.name + " " + _NameAndAttribute;
                    }
                    else {
                        _DisplayName = _NameAndAttribute;
                    }
                }
#if UNITY_EDITOR
                string dname = StringUtil.ClassName("" + PropertyType);
                _displayNameType = _DisplayName + " (" + dname + ")";
            }
            else {
                _displayNameType = null;
#endif
            }
            //Debug.Log($"GetNameAndAttribute:{_NameAndAttribute}");
            return _NameAndAttribute;
        }

        /// <summary>
        /// Returns a longer name showing the object it belongs too, and optionally where it is in the
        /// scene. This is used to display property info to the user and is mostly used for debugging.
        /// </summary>
        public string PathName() { return PathName(false); }

        public string PathName(bool includeFullScenePath)
        {
            if (IsDataOnly || Comp == null) return DisplayName;
            if (string.IsNullOrEmpty(_PathName)) {
                _PathName = GetPathName(includeFullScenePath);
            }
            return _PathName;
        }

        public string GetPathName(bool fullPath, bool overrideAttribute = false, int attribute = -1)
        {
            if (Comp == null) return "NULL";
            string pathName = StringUtil.ClassName("" + Comp.GetType());
            int componentIndex = 0;
            Component[] list = Comp.GetComponents<Component>();
            for (int i = 0; i < list.Length; i++) {
                if (Comp == list[i]) {
                    componentIndex = i;
                    break;
                }
            }

            if (fullPath) {
                pathName = ObjectUtil.GetPath(Comp.gameObject) + ".Component[" + componentIndex + "]->" + pathName;
            }
            else {
                pathName = componentIndex + ": " + pathName;
            }

            string attributeName = GetAttributeName(overrideAttribute ? attribute : Attribute);
            if (string.IsNullOrEmpty(attributeName)) attributeName = $"({attributeName})";

            string dname = StringUtil.ClassName("" + DataType);
            pathName = pathName + "." + Name + attributeName;
            if (!string.IsNullOrEmpty(dname)) {
                pathName += " (" + dname + ")";
            }

            return pathName;
        }

        /// <summary>
        /// Comapres the name of this property with another to see if they are the same. This is used to
        /// quickly relate property selections.
        /// </summary>
        public bool NameMatches(Property prop)
        {
            bool matches = false;
            if (prop != null && _Name != null && prop != null && prop.Name != null) {
                matches = _Name.Equals(prop.Name);
            }
            return matches;
        }

        #endregion

        #region VALUE PROCESSING

        /// <summary>
        /// Copies the value of one property to another. The source value is automatically read, and the
        /// destination value applied.
        /// </summary>
        public void CopyValue(Property from)
        {
            if (!IsPrepared) Prepare();
            from.ReadValue();

            if (from.Attribute < 0) {
                _value = from._value;
            }
            else
            if (from.Attribute == 0) {
                _value = new Vector4(from._value.x, from._value.x, from._value.x, from._value.x);
            }
            else
            if (from.Attribute == 1) {
                _value = new Vector4(from._value.y, from._value.y, from._value.y, from._value.y);
            }
            else
            if (from.Attribute == 2) {
                _value = new Vector4(from._value.z, from._value.z, from._value.z, from._value.z);
            }
            else
            if (from.Attribute == 3) {
                _value = new Vector4(from._value.w, from._value.w, from._value.w, from._value.w);
            }

            _stringValue = from._stringValue;
            _enumValue = from._enumValue;
            _objectValue = from._objectValue;
            _gameObjectValue = from._gameObjectValue;
            _componentValue = from._componentValue;

            ApplyValue();
        }

        /// <summary>
        /// Retrieves the current value of the specified property. Use this to modify the current property
        /// value, or skip this step if your property is always overwritten.
        /// </summary>
        public void ReadValue()
        {
            if (!IsPrepared) Prepare();

            if (IsLinkValid) {
                FloatValue = _linkedTo.BlendValue;
            }

            if (IsDataOnly || !IsEnabled) return;

            if (string.IsNullOrEmpty(_Name)) {
                //Debug.LogWarning($"_Name is null");
                return;
            }

            System.Object obj = Comp;
            if (obj == null) {
                Debug.LogWarning($"obj is null");
                return;
            }

            bool handled = false;
            if (DataType == null) GetDataType();

            handled = ReadValueFromHandler(handled);

            if (!handled) {
#if AXON_LEGACY_PROPERTIES
                ReadValueFromFieldOrProperty(obj);
#else
                ReadValueFromWrapper();
#endif
            }
            if (IsUniformValue) {
                _value.y = _value.z = _value.w = _value.x;
            }
        }


#if AXON_LEGACY_PROPERTIES
        private void ReadValueFromFieldOrProperty(object obj)
        {
            try {
                if (FieldInfo != null) {
                    if (FieldInfo.FieldType == typeof(string)) {
                        _stringValue = (string)FieldInfo.GetValue(obj);
                    }
                    else
                    if (typeof(Enum).IsAssignableFrom(FieldInfo.FieldType)) {
                        _enumValue = (Enum)FieldInfo.GetValue(obj);
                        _value.x = (int)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Boolean)) {
                        _value.x = (bool)FieldInfo.GetValue(obj) ? 1f : 0f;
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Single)) {
                        _value.x = (Single)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Double)) {
                        _value.x = (float)(Double)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Decimal)) {
                        _value.x = (float)(Decimal)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Vector2)) {
                        _value = (Vector2)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Vector3)) {
                        _value = (Vector3)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Vector4) || FieldInfo.FieldType == typeof(RectOffset)) {
                        _value = (Vector4)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Color)) {
                        Color c = (Color)FieldInfo.GetValue(obj);
                        _value.x = c.r;
                        _value.y = c.g;
                        _value.z = c.b;
                        _value.w = c.a;
                    }
                    else
                    if (FieldInfo.FieldType == typeof(Rect)) {
                        Rect c = (Rect)FieldInfo.GetValue(obj);
                        _value.x = c.x;
                        _value.y = c.y;
                        _value.z = c.width;
                        _value.w = c.height;
                    }
                    else
                    if (FieldInfo.FieldType == typeof(int)) {
                        _value.x = (Int32)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(uint)) {
                        _value.x = (Int32)(uint)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(byte)) {
                        _value.x = (Int32)(byte)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(sbyte)) {
                        _value.x = (Int32)(sbyte)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(long)) {
                        _value.x = (Int32)(long)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(ulong)) {
                        _value.x = (Int32)(ulong)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(short)) {
                        _value.x = (Int32)(short)FieldInfo.GetValue(obj);
                    }
                    else
                    if (FieldInfo.FieldType == typeof(ushort)) {
                        _value.x = (Int32)(ushort)FieldInfo.GetValue(obj);
                    }
                    else
                    if (typeof(GameObject).IsAssignableFrom(FieldInfo.FieldType)) {
                        _gameObjectValue = (GameObject)FieldInfo.GetValue(obj);
                    }
                    else
                    if (typeof(Component).IsAssignableFrom(FieldInfo.FieldType)) {
                        _componentValue = (Component)FieldInfo.GetValue(obj);
                    }
                    else
                    if (typeof(UnityEngine.Object).IsAssignableFrom(FieldInfo.FieldType)) {
                        _objectValue = (UnityEngine.Object)FieldInfo.GetValue(obj);
                    }
                }
                else
                if (PropertyInfo != null) {
                    if (PropertyInfo.PropertyType == typeof(string)) {
                        _stringValue = (string)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (typeof(Enum).IsAssignableFrom(PropertyInfo.PropertyType)) {
                        _enumValue = (Enum)PropertyInfo.GetGetMethod().Invoke(obj, null);
                        _value.x = (int)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Boolean)) {
                        _value.x = (bool)PropertyInfo.GetGetMethod().Invoke(obj, null) ? 1f : 0f;
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Double)) {
                        _value.x = (float)(double)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Decimal)) {
                        _value.x = (float)(Decimal)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Single)) {
                        _value.x = (float)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Vector2)) {
                        _value = (Vector2)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Vector3)) {
                        _value = (Vector3)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Vector4) || PropertyInfo.PropertyType == typeof(RectOffset)) {
                        _value = (Vector4)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Color)) {
                        Color c = (Color)PropertyInfo.GetGetMethod().Invoke(obj, null);
                        _value.x = c.r;
                        _value.y = c.g;
                        _value.z = c.b;
                        _value.w = c.a;
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(Rect)) {
                        Rect c = (Rect)PropertyInfo.GetGetMethod().Invoke(obj, null);
                        _value.x = c.x;
                        _value.y = c.y;
                        _value.z = c.width;
                        _value.w = c.height;
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(int)) {
                        _value.x = (float)(int)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(uint)) {
                        _value.x = (float)(uint)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(byte)) {
                        _value.x = (float)(byte)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(sbyte)) {
                        _value.x = (float)(sbyte)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(long)) {
                        _value.x = (float)(long)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(ulong)) {
                        _value.x = (float)(ulong)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(short)) {
                        _value.x = (float)(short)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (PropertyInfo.PropertyType == typeof(ushort)) {
                        _value.x = (float)(ushort)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (typeof(GameObject).IsAssignableFrom(PropertyInfo.PropertyType)) {
                        _gameObjectValue = (GameObject)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (typeof(Component).IsAssignableFrom(PropertyInfo.PropertyType)) {
                        _componentValue = (Component)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                    else
                    if (typeof(UnityEngine.Object).IsAssignableFrom(PropertyInfo.PropertyType)) {
                        _objectValue = (UnityEngine.Object)PropertyInfo.GetGetMethod().Invoke(obj, null);
                    }
                }
                else {
                    IsEnabled = false;
                    //if (DebugEnabled) 
                    Debug.LogWarning("Property.ReadValue: Failed finding property:" + PathName(true));
                    //Debug.Log("Handler:" + (Handler == null ? "NULL" : "" + Handler.GetType()));
                }
            }
            catch (Exception exception) {
                IsEnabled = false; // prevents repeated erros
                Debug.LogException(exception);
            }
        }

#else
        private void ReadValueFromWrapper()
        {
            if (IsBool) {
                if (BoolWrapper == null) InitPropertyWrappers();
                if (BoolWrapper != null) _value.x = BoolWrapper.GetValue() ? 1f : 0f;
            }
            else
            if (IsInt) {
                if (IntWrapper == null) InitPropertyWrappers();
                if (IntWrapper != null) _value.x = IntWrapper.GetValue();
            }
            if (IsEnum) {
                if (EnumWrapper == null) InitPropertyWrappers();
                if (EnumWrapper != null) {
                    _enumValue = EnumWrapper.GetValue();
                    _value.x = Convert.ToInt32(_enumValue);
                }
            }
            else
            if (IsFloat) {
                if (FloatWrapper == null) InitPropertyWrappers();
                if (FloatWrapper != null) _value.x = FloatWrapper.GetValue();
            }
            else
            if (IsColor) {
                if (ColorWrapper == null) InitPropertyWrappers();
                if (ColorWrapper != null) _value = ColorWrapper.GetValue();
            }
            else
            if (IsVector2) {
                if (Vector2Wrapper == null) InitPropertyWrappers();
                if (Vector2Wrapper != null) _value = Vector2Wrapper.GetValue();
            }
            else
            if (IsVector3) {
                if (Vector3Wrapper == null) InitPropertyWrappers();
                if (Vector3Wrapper != null) _value = Vector3Wrapper.GetValue();
            }
            else
            if (IsVector4) {
                if (Vector4Wrapper == null) InitPropertyWrappers();
                if (Vector4Wrapper != null) _value = Vector4Wrapper.GetValue();
            }
            else
            if (IsRect) {
                if (RectWrapper == null) InitPropertyWrappers();
                if (RectWrapper != null) {
                    Rect c = RectWrapper.GetValue();
                    _value.x = c.x;
                    _value.y = c.y;
                    _value.z = c.width;
                    _value.w = c.height;
                }
            }
            else
            if (IsRectOffset) {
                if (RectOffsetWrapper == null) InitPropertyWrappers();
                if (RectOffsetWrapper != null) {
                    RectOffset c = RectOffsetWrapper.GetValue();
                    _value.x = c.left;
                    _value.y = c.right;
                    _value.z = c.top;
                    _value.w = c.bottom;
                }
            }
            else
            if (IsString) {
                if (StringWrapper == null) InitPropertyWrappers();
                if (StringWrapper != null) _stringValue = StringWrapper.GetValue();
            }
            else
            if (IsGameObject) {
                if (GameObjectWrapper == null) InitPropertyWrappers();
                if (GameObjectWrapper != null) _gameObjectValue = GameObjectWrapper.GetValue();
            }
            else
            if (IsComponent) {
                if (ComponentWrapper == null) InitPropertyWrappers();
                if (ComponentWrapper != null) _componentValue = ComponentWrapper.GetValue();
            }
            else
            if (IsObject) {
                if (ObjectWrapper == null) InitPropertyWrappers();
                if (ObjectWrapper != null) _objectValue = ObjectWrapper.GetValue();
            }

        }
#endif

        private bool ReadValueFromHandler(bool handled)
        {
            if (Handler == null) return false;
            if (IsFloatType(DataType)) {
                _value.x = Handler.GetFloat();
                handled = true;
            }
            else
            if (DataType == typeof(Color)) {
                _value = Handler.GetColor();
                handled = true;
            }
            else
            if (DataType == typeof(bool)) {
                _value.x = Handler.GetBool() ? 1f : 0f;
                handled = true;
            }
            else
            if (IsIntType(DataType)) {
                _value.x = Handler.GetInt();
                handled = true;
            }
            else
            if (IsVector) {
                _value = Handler.GetVector();
                handled = true;
            }
            else
            if (typeof(UnityEngine.Object).IsAssignableFrom(DataType)) {
                _objectValue = Handler.GetObject();
                handled = true;
            }
            else {
                //Debug.Log($"Handler:{Handler.GetType()} DataType:{DataType} Not Supported");
                handled = false;
            }
            if (!handled) handled = !Handler.ShowDefaultProperties;

            return handled;
        }

        /// <summary>
        /// No changes are made to the target property until ApplyValue is called. This allows a Property
        /// to act as a container during calculations and only apply the value when it is ready.
        /// </summary>
        public void ApplyValue()
        {
            if (!IsPrepared) Prepare();
            if (IsLinkValid) {
                _linkedTo.BlendValue = FloatValue;
            }
            else
            if (!IsDataOnly) {
                if (IsUniformValue) {
                    _value.y = _value.z = _value.w = _value.x;
                }

                bool handled = false;

                handled = ApplyValueByHandler(handled);

                if (handled) return;

#if AXON_LEGACY_PROPERTIES
                ApplyValueByFieldOrProperty();
#else
                ApplyValueByWrapper();
#endif
            }
        }


#if AXON_LEGACY_PROPERTIES
        private void ApplyValueByFieldOrProperty()
        {
            System.Object obj = Comp;
            System.Object v = null;
            if (FieldInfo != null) {
                if (FieldInfo.FieldType == typeof(string)) {
                    v = _stringValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Boolean)) {
                    v = BoolValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(int)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(uint)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (uint)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(byte)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (byte)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(sbyte)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (sbyte)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(long)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (long)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(ulong)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (ulong)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(short)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (short)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(ushort)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, (ushort)IntValue);
                }
                else
                if (FieldInfo.FieldType == typeof(Single)) {
                    v = FloatValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Double)) {
                    v = (Double)FloatValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Decimal)) {
                    v = (Decimal)FloatValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Vector2)) {
                    if (Attribute == -2) {
                        v = new Vector2(_value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = (Vector2)_value;
                    }
                    else {
                        v = FieldInfo.GetValue(obj);
                        Vector2 v2 = (Vector2)v;
                        if (Attribute == 0) {
                            v2.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            v2.y = FloatValue;
                        }
                        v = v2;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Vector3)) {
                    if (Attribute == -2) {
                        v = new Vector3(_value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = (Vector3)_value;
                    }
                    else {
                        v = FieldInfo.GetValue(obj);
                        Vector3 v3 = (Vector3)v;
                        if (Attribute == 0) {
                            v3.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            v3.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            v3.z = FloatValue;
                        }
                        v = v3;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Vector4)) {
                    if (Attribute == -2) {
                        v = new Vector4(FloatValue, FloatValue, FloatValue, FloatValue);
                    }
                    else
                    if (Attribute == -1) {
                        v = _value;
                    }
                    else {
                        v = FieldInfo.GetValue(obj);
                        Vector4 v4 = (Vector4)v;
                        if (Attribute == 0) {
                            v4.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            v4.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            v4.z = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            v4.w = FloatValue;
                        }
                        v = v4;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(RectOffset)) {
                    if (Attribute == -2) {
                        v = new RectOffset((int)_value.x, (int)_value.x, (int)_value.x, (int)_value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = new RectOffset((int)_value.x, (int)_value.y, (int)_value.z, (int)_value.w);
                    }
                    else {
                        v = FieldInfo.GetValue(obj);
                        Color c1 = (Color)v;
                        if (Attribute == 0) {
                            c1.r = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            c1.g = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            c1.b = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            c1.a = FloatValue;
                        }
                        v = c1;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Color)) {
                    if (Attribute == -2) {
                        v = new Color(_value.x, _value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = new Color(_value.x, _value.y, _value.z, _value.w);
                    }
                    else {
                        v = FieldInfo.GetValue(obj);
                        Color c1 = (Color)v;
                        if (Attribute == 0) {
                            c1.r = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            c1.g = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            c1.b = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            c1.a = FloatValue;
                        }
                        v = c1;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (FieldInfo.FieldType == typeof(Rect)) {
                    if (Attribute == -2) {
                        v = new Rect(_value.x, _value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = new Rect(_value.x, _value.y, _value.z, _value.w);
                    }
                    else {
                        v = FieldInfo.GetValue(obj);
                        Rect c1 = (Rect)v;
                        if (Attribute == 0) {
                            c1.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            c1.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            c1.width = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            c1.height = FloatValue;
                        }
                        v = c1;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (typeof(GameObject).IsAssignableFrom(FieldInfo.FieldType)) {
                    FieldInfo.SetValue(obj, _gameObjectValue);
                }
                else
                if (typeof(Component).IsAssignableFrom(FieldInfo.FieldType)) {
                    if (_componentValue == null) {
                        FieldInfo.SetValue(obj, null);
                    }
                    else {
                        FieldInfo.SetValue(obj, _componentValue);
                    }
                }
                else
                if (typeof(UnityEngine.Object).IsAssignableFrom(FieldInfo.FieldType)) {
                    FieldInfo.SetValue(obj, _objectValue);
                }
                else
                if (typeof(Enum).IsAssignableFrom(FieldInfo.FieldType)) {
                    v = EnumValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, v);
                }
                else
                if (typeof(LayerMask).IsAssignableFrom(FieldInfo.FieldType)) {
                    LayerMask m = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    FieldInfo.SetValue(obj, m);
                }
                else {
                    Debug.LogWarning($"Field type not handled:{FieldInfo.FieldType}");
                }
            }
            else
            if (PropertyInfo != null) {
                if (PropertyInfo.PropertyType == typeof(Boolean)) {
                    v = BoolValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Single)) {
                    v = FloatValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Double)) {
                    v = FloatValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Decimal)) {
                    v = FloatValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (Decimal)v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(string)) {
                    v = _stringValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Vector2)) {
                    if (Attribute == -2) {
                        v = new Vector2(_value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = (Vector2)_value;
                    }
                    else {
                        v = PropertyInfo.GetGetMethod().Invoke(obj, null);
                        Vector2 v2a = (Vector2)v;
                        if (Attribute == 0) {
                            v2a.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            v2a.y = FloatValue;
                        }
                        v = v2a;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Vector3)) {
                    if (Attribute == -2) {
                        v = new Vector3(_value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = (Vector3)_value;
                    }
                    else {
                        v = PropertyInfo.GetGetMethod().Invoke(obj, null);
                        Vector3 v3a = (Vector3)v;
                        if (Attribute == 0) {
                            v3a.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            v3a.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            v3a.z = FloatValue;
                        }
                        v = v3a;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Vector4)) {
                    if (Attribute == -2) {
                        v = new Vector4(_value.x, _value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = (Vector4)_value;
                    }
                    else {
                        v = PropertyInfo.GetGetMethod().Invoke(obj, null);
                        Vector4 v4a = (Vector4)v;
                        if (Attribute == 0) {
                            v4a.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            v4a.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            v4a.z = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            v4a.w = FloatValue;
                        }
                        v = v4a;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(RectOffset)) {
                    if (Attribute == -2) {
                        v = new RectOffset((int)_value.x, (int)_value.x, (int)_value.x, (int)_value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = new RectOffset((int)_value.x, (int)_value.y, (int)_value.z, (int)_value.w);
                    }
                    else {
                        v = PropertyInfo.GetGetMethod().Invoke(obj, null);
                        RectOffset v4a = (RectOffset)v;
                        if (Attribute == 0) {
                            v4a.left = IntValue;
                        }
                        else
                        if (Attribute == 1) {
                            v4a.right = IntValue;
                        }
                        else
                        if (Attribute == 2) {
                            v4a.top = IntValue;
                        }
                        else
                        if (Attribute == 3) {
                            v4a.bottom = IntValue;
                        }
                        v = v4a;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Color)) {
                    if (Attribute == -2) {
                        v = new Color(_value.x, _value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = (Color)_value;
                    }
                    else {
                        v = PropertyInfo.GetGetMethod().Invoke(obj, null);
                        Color c2 = (Color)v;
                        if (Attribute == 0) {
                            c2.r = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            c2.g = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            c2.b = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            c2.a = FloatValue;
                        }
                        v = c2;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue[" + Attribute + "]:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (PropertyInfo.PropertyType == typeof(int)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(uint)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (uint)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(byte)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (byte)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(sbyte)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (sbyte)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(long)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (long)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(ulong)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (ulong)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(short)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (short)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(ushort)) {
                    v = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: FieldInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, (ushort)IntValue);
                }
                else
                if (PropertyInfo.PropertyType == typeof(Rect)) {
                    if (Attribute == -2) {
                        v = new Rect(_value.x, _value.x, _value.x, _value.x);
                    }
                    else
                    if (Attribute == -1) {
                        v = new Rect(_value.x, _value.y, _value.z, _value.w);
                    }
                    else {
                        v = PropertyInfo.GetGetMethod().Invoke(obj, null);
                        Rect c2 = (Rect)v;
                        if (Attribute == 0) {
                            c2.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            c2.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            c2.width = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            c2.height = FloatValue;
                        }
                        v = c2;
                    }
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v, null);
                }
                else
                if (typeof(GameObject).IsAssignableFrom(PropertyInfo.PropertyType)) {
                    PropertyInfo.SetValue(obj, _gameObjectValue, null);
                }
                else
                if (typeof(Component).IsAssignableFrom(PropertyInfo.PropertyType)) {
                    PropertyInfo.SetValue(obj, _componentValue, null);
                }
                else
                if (typeof(UnityEngine.Object).IsAssignableFrom(PropertyInfo.PropertyType)) {
                    PropertyInfo.SetValue(obj, _objectValue, null);
                }
                else
                if (typeof(Enum).IsAssignableFrom(PropertyInfo.PropertyType)) {
                    v = EnumValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, v);
                }
                else
                if (typeof(LayerMask).IsAssignableFrom(PropertyInfo.PropertyType)) {
                    LayerMask m = IntValue;
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: PropertyInfo.SetValue:" + v);
                    PropertyInfo.SetValue(obj, m);
                }
                else {
                    Debug.LogWarning($"Property type not handled:{PropertyInfo.PropertyType}");
                }
            }
            //else {
            //    Debug.LogWarning("Property[" + _Name + "].ApplyValue: Failed to apply value", Obj == null ? null : Obj.gameObject);
            //}
        }
#else
        private void ApplyValueByWrapper()
        {
            if (IsBool) {
                if (BoolWrapper == null) InitPropertyWrappers();
                BoolWrapper?.SetValue(_value.x > 0);
            }
            else
            if (IsInt) {
                if (IntWrapper == null) InitPropertyWrappers();
                IntWrapper?.SetValue((int)_value.x);
            }
            else
            if (IsEnum) {
                if (EnumWrapper == null) InitPropertyWrappers();
                EnumWrapper?.SetValue((Enum)Enum.ToObject(_enumValue.GetType(), (int)_value.x));
            }
            else
            if (IsFloat) {
                if (FloatWrapper == null) InitPropertyWrappers();
                FloatWrapper?.SetValue(_value.x);
            }
            else
            if (IsColor) {
                if (ColorWrapper == null) InitPropertyWrappers();
                if (ColorWrapper != null) {
                    if (Attribute == -2) {
                        ColorWrapper?.SetValue(new Color(_value.x, _value.x, _value.x, _value.x));
                    }
                    else
                    if (Attribute == -1) {
                        ColorWrapper?.SetValue(_value);
                    }
                    else {
                        Color val = ColorWrapper.GetValue();
                        if (Attribute == 0) {
                            val.r = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            val.g = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            val.b = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            val.a = FloatValue;
                        }
                        ColorWrapper?.SetValue(val);
                    }
                }
            }
            else
            if (IsVector2) {
                if (Vector2Wrapper == null) InitPropertyWrappers();
                if (Vector2Wrapper != null) {
                    if (Attribute == -2) {
                        Vector2Wrapper?.SetValue(new Vector2(_value.x, _value.x));
                    }
                    else
                if (Attribute == -1) {
                        Vector2Wrapper?.SetValue(_value);
                    }
                    else {
                        Vector2 val = Vector2Wrapper.GetValue();
                        if (Attribute == 0) {
                            val.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            val.y = FloatValue;
                        }
                        Vector2Wrapper?.SetValue(val);
                    }
                }
            }
            else
            if (IsVector3) {
                if (Vector3Wrapper == null) InitPropertyWrappers();
                if (Vector3Wrapper != null) {
                    if (Attribute == -2) {
                        Vector3Wrapper?.SetValue(new Vector3(_value.x, _value.x, _value.x));
                    }
                    else
                if (Attribute == -1) {
                        Vector3Wrapper?.SetValue(_value);
                    }
                    else {
                        Vector3 val = Vector3Wrapper.GetValue();
                        if (Attribute == 0) {
                            val.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            val.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            val.z = FloatValue;
                        }
                        Vector3Wrapper?.SetValue(val);
                    }
                }
            }
            else
            if (IsVector4) {
                if (Vector4Wrapper == null) InitPropertyWrappers();
                if (Vector4Wrapper != null) {
                    if (Attribute == -2) {
                        Vector4Wrapper?.SetValue(new Vector4(_value.x, _value.x, _value.x, _value.x));
                    }
                    else
                if (Attribute == -1) {
                        Vector4Wrapper?.SetValue(_value);
                    }
                    else {
                        Vector4 val = Vector4Wrapper.GetValue();
                        if (Attribute == 0) {
                            val.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            val.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            val.z = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            val.w = FloatValue;
                        }
                        Vector4Wrapper?.SetValue(val);
                    }
                }
            }
            else
            if (IsRect) {
                if (RectWrapper == null) InitPropertyWrappers();
                if (RectWrapper != null) {
                    if (Attribute == -2) {
                        RectWrapper?.SetValue(new Rect(_value.x, _value.x, _value.x, _value.x));
                    }
                    else
                if (Attribute == -1) {
                        RectWrapper?.SetValue(new Rect(_value.x, _value.y, _value.z, _value.w));
                    }
                    else {
                        Rect val = RectWrapper.GetValue();
                        if (Attribute == 0) {
                            val.x = FloatValue;
                        }
                        else
                        if (Attribute == 1) {
                            val.y = FloatValue;
                        }
                        else
                        if (Attribute == 2) {
                            val.width = FloatValue;
                        }
                        else
                        if (Attribute == 3) {
                            val.height = FloatValue;
                        }
                        RectWrapper?.SetValue(val);
                    }
                }
            }
            else
            if (IsRectOffset) {
                if (RectOffsetWrapper == null) InitPropertyWrappers();
                if (RectOffsetWrapper != null) {
                    if (Attribute == -2) {
                        RectOffsetWrapper?.SetValue(new RectOffset((int)_value.x, (int)_value.x, (int)_value.x, (int)_value.x));
                    }
                    else
                if (Attribute == -1) {
                        RectOffsetWrapper?.SetValue(new RectOffset((int)_value.x, (int)_value.y, (int)_value.z, (int)_value.w));
                    }
                    else {
                        RectOffset val = RectOffsetWrapper.GetValue();
                        if (Attribute == 0) {
                            val.left = IntValue;
                        }
                        else
                        if (Attribute == 1) {
                            val.right = IntValue;
                        }
                        else
                        if (Attribute == 2) {
                            val.top = IntValue;
                        }
                        else
                        if (Attribute == 3) {
                            val.bottom = IntValue;
                        }
                        RectOffsetWrapper?.SetValue(val);
                    }
                }
            }
            else
            if (IsString) {
                if (StringWrapper == null) InitPropertyWrappers();
                StringWrapper?.SetValue(_stringValue);
            }
            else
            if (IsGameObject) {
                if (GameObjectWrapper == null) InitPropertyWrappers();
                GameObjectWrapper?.SetValue(_gameObjectValue);
            }
            else
            if (IsComponent) {
                if (ComponentWrapper == null) InitPropertyWrappers();
                ComponentWrapper?.SetValue(_componentValue);
            }
            else
            if (IsObject) {
                if (ObjectWrapper == null) InitPropertyWrappers();
                ObjectWrapper?.SetValue(_objectValue);
            }
            else
            if (DataType != null) {
                Debug.LogWarning($"Property type not handled:{DataType}");
            }
        }
#endif

        private bool ApplyValueByHandler(bool handled)
        {
            if (Handler == null || DataType == null) return false;
            if (IsFloatType(DataType)) {
                if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetFloat:" + FloatValue);
                Handler.SetFloat(FloatValue);
                handled = true;
            }
            else
            if (DataType == typeof(bool)) {
                if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetBool:" + BoolValue);
                Handler.SetBool(BoolValue);
                handled = true;
            }
            else
            if (IsIntType(DataType)) {
                if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetInt:" + IntValue);
                Handler.SetInt(IntValue);
                handled = true;
            }
            else
            if (DataType == typeof(Color)) {
                if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetColor:" + ColorValue);
                Handler.SetColor(ColorValue);
                handled = true;
            }
            else
            if (IsVector || IsRect || IsRectOffset) {
                if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetVector:" + Vector4Value);
                Handler.SetVector(Vector4Value, Attribute);
                handled = true;
            }
            else
            if (typeof(UnityEngine.Object).IsAssignableFrom(DataType)) {
                if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetObject:" + (ObjectValue == null ? "NULL" : ObjectValue.name));
                Handler.SetObject(ObjectValue);
                handled = true;
            }
            else {
                //if (DebugEnabled) Debug.Log("Property[" + _Name + "].ApplyValue: Handler.SetValue:" + _value + " attribute:" + Attribute);
                //Handler.SetVector(_value, Attribute);
                //handled = true;
                if (DebugEnabled) Debug.LogWarning("Property[" + _Name + "].ApplyValue: Unhandled type:" + DataType + " attribute:" + Attribute);
                handled = false;
            }
            if (!handled) handled = !Handler.ShowDefaultProperties;

            return handled;
        }

        #endregion

        #region TYPES

        /// <summary>
        /// Determines the type of value being read and written.
        /// </summary>
        /// <returns></returns>
        public Type GetDataType(bool force = false)
        {
            if (IsDataOnly) {
                DataType = PropertyTypeToDataType(PropertyType);

                //Debug.Log("Property[" + _Name + "].GetDataType:" + DataType + " IsDataOnly");
                return DataType;
            }

            if ((force || DataType == null) && !string.IsNullOrEmpty(_Name)) {
                if (ForcePropertyType != PropertyTypes.Auto) {
                    DataType = PropertyTypeToDataType(ForcePropertyType);
                }
                else {
                    _GetDataType();
                }
            }
            //if (DebugEnabled) 
            //Debug.Log("Property[" + _Name + "].GetDataType:" + DataType+ " PropertyType:"+ PropertyType);

            return DataType;
        }

        private void _GetDataType()
        {
            if (Application.isPlaying && !string.IsNullOrEmpty(DataTypeName)) {
                DataType = StringToDataType(DataTypeName);
                //Debug.Log($"DataType:{DataType}");
            }
            else {
                /// Property assignments are only updated in the editor in edit mode. This information is
                /// stored and serialzied for fast loading at runtime.
                if (Handler != null) {
                    //Debug.Log($"Handler:{Handler.GetType()}");
                    if (Handler.GetType() == typeof(PropertiesHandler)) {
                        /// Don't allow the base class - handler type must be derrived from
                        /// PropertiesHandler
                        Handler = null;
                    }
                    else
                    if (Handler.HasProperty(_Name)) {
                        DataType = Handler.GetPropertyType(_Name);
                        _Name = Handler.Name;
                    }
                    else {
                        Handler = null;
                    }
                }
            }

            if (Comp != null && (Handler == null || Handler.ShowDefaultProperties)) {
                Type compType = Comp.GetType();

                // First, try to get a writable property (i.e. one with a public setter)
                PropertyInfo[] properties = compType.GetProperties();
                foreach (var prop in properties) {
                    if (string.Equals(prop.Name, _Name, StringComparison.OrdinalIgnoreCase) && prop.CanWrite && prop.CanRead) {
                        PropertyInfo = prop;
                        break;
                    }
                }

                //Debug.Log($"_Name:{_Name} PropertyInfo:{(PropertyInfo == null ? "NULL" : PropertyInfo.PropertyType)}");

                if (PropertyInfo != null) {
                    _Name = PropertyInfo.Name;
                    DataType = PropertyInfo.PropertyType;
                }
                else {
                    // If no writable property is found, try to get a field that is not readonly or constant.
                    FieldInfo[] fields = compType.GetFields(BindingFlags);
                    foreach (var field in fields) {
                        if (string.Equals(field.Name, _Name, StringComparison.OrdinalIgnoreCase) && !field.IsInitOnly && !field.IsLiteral) {
                            FieldInfo = field;
                            break;
                        }
                    }
                    // Debug.Log($"_Name:{_Name} FieldInfo:{(FieldInfo == null ? "NULL" : FieldInfo.FieldType)}");
                    if (FieldInfo != null) {
                        DataType = FieldInfo.FieldType;
                    }
                }

#if !AXON_LEGACY_PROPERTIES
                InitPropertyWrappers();
#endif
            }
            DataTypeName = DataTypeToString(DataType);
        }

#if !AXON_LEGACY_PROPERTIES
        private void InitPropertyWrappers()
        {
            if (Handler != null) return; // Bypass if handler is set

            //Debug.Log($"InitPropertyWrappers: IsMaterial:{IsMaterial}");
            if (IsMaterial) return;// Cannot use property wrapper with materials

            if (IsBool) {
                if (BoolWrapper == null || !BoolWrapper.Matches(Comp, _Name))
                    BoolWrapper = new PropertyWrapper<bool>(Comp, _Name);
            }
            else
            if (IsInt) {
                if (IntWrapper == null || !IntWrapper.Matches(Comp, _Name))
                    IntWrapper = new PropertyWrapper<int>(Comp, _Name);
            }
            else
            if (IsEnum) {
                if (EnumWrapper == null || !EnumWrapper.Matches(Comp, _Name))
                    EnumWrapper = new PropertyWrapper<Enum>(Comp, _Name);
            }
            else
            if (IsFloat) {
                if (FloatWrapper == null || !FloatWrapper.Matches(Comp, _Name)) {
                    FloatWrapper = new PropertyWrapper<float>(Comp, _Name);
                }
            }
            else
            if (IsColor) {
                if (ColorWrapper == null || !ColorWrapper.Matches(Comp, _Name))
                    ColorWrapper = new PropertyWrapper<Color>(Comp, _Name);
            }
            else
            if (IsVector2) {
                if (Vector2Wrapper == null || !Vector2Wrapper.Matches(Comp, _Name))
                    Vector2Wrapper = new PropertyWrapper<Vector2>(Comp, _Name);
            }
            else
            if (IsVector3) {
                if (Vector3Wrapper == null || !Vector3Wrapper.Matches(Comp, _Name))
                    Vector3Wrapper = new PropertyWrapper<Vector3>(Comp, _Name);
            }
            else
            if (IsVector4) {
                if (Vector4Wrapper == null || !Vector4Wrapper.Matches(Comp, _Name))
                    Vector4Wrapper = new PropertyWrapper<Vector4>(Comp, _Name);
            }
            else
            if (IsRect) {
                if (RectWrapper == null || !RectWrapper.Matches(Comp, _Name))
                    RectWrapper = new PropertyWrapper<Rect>(Comp, _Name);
            }
            else
            if (IsRectOffset) {
                if (RectOffsetWrapper == null || !RectOffsetWrapper.Matches(Comp, _Name))
                    RectOffsetWrapper = new PropertyWrapper<RectOffset>(Comp, _Name);
            }
            else
            if (IsString) {
                if (StringWrapper == null || !StringWrapper.Matches(Comp, _Name))
                    StringWrapper = new PropertyWrapper<string>(Comp, _Name);
            }
            else
            if (IsGameObject) {
                if (GameObjectWrapper == null || !GameObjectWrapper.Matches(Comp, _Name))
                    GameObjectWrapper = new PropertyWrapper<GameObject>(Comp, _Name);
            }
            else
            if (IsComponent) {
                if (ComponentWrapper == null || !ComponentWrapper.Matches(Comp, _Name))
                    ComponentWrapper = new PropertyWrapper<Component>(Comp, _Name);
            }
            else
            if (IsObject) {
                if (ObjectWrapper == null || !ObjectWrapper.Matches(Comp, _Name))
                    ObjectWrapper = new PropertyWrapper<UnityEngine.Object>(Comp, _Name);
            }
            else {
                if (DataType != null) Debug.LogWarning($"Property type not handled:{DataType}");
            }
        }
#endif

        #endregion

        #region TYPE ACCESSORS

        /// <summary>
        /// This determines the type of data which affects how values are processed and displayed.
        /// </summary>
        public Type DataType {
            get {
                if (_dataType == null) {
                    //_GetDataType();
                }
                return _dataType;
            }
            set {
                if (_dataType != value) {
                    _dataType = value;
                    if (_dataType != null) {
                        PropertyType = GetPropertyType(_dataType);
                    }
                }
            }
        }

        /// <summary>
        /// A combined value simply means working with whole values such as a vector, with each attribute
        /// maintaining its own value.
        /// </summary>
        public bool IsCombinedValue {
            get {
                return Attribute == -1;
            }
            set {
                if (IsCombinedValue != value) {
                    if (value) {
                        Attribute = -1;
                    }
                    else {
                        Attribute = -2;
                    }
                }
            }
        }

        /// <summary>
        /// A uniform value applies the same value across all attirbutes of the type. For examples, this
        /// can be useful for scaling objects proportionally by setting the x, y, and z values the same.
        /// This setting can be applied to any multi-attribute value.
        /// </summary>
        public bool IsUniformValue {
            get {
                return Attribute == -2;
            }
            set {
                if (IsUniformValue != value) {
                    if (value) {
                        Attribute = -2;
                    }
                    else {
                        Attribute = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Note that IsMaterial does not define the value type, but is simply a way to tell whether the
        /// property value belongs to a material. Since materials are not components they require special
        /// treatment. When a material property is assigned, a PropertiesOfMaterial handler is created.
        /// </summary>
        public bool IsMaterial {
            get {
                return _IsMaterial;
            }
            set {
                _IsMaterial = value;
            }
        }

        public virtual bool IsNumber {
            get {
                return IsNumeric(ForcePropertyType == PropertyTypes.Auto ? PropertyType : ForcePropertyType);
            }
        }

        public virtual bool IsSingleAttribute {
            get {
                return Attribute != -1 || GetAttributeCount(ForcePropertyType == PropertyTypes.Auto ? PropertyType : ForcePropertyType) == 1;
            }
        }

        public bool IsBool {
            get {
                return PropertyType == PropertyTypes.Bool || GetPropertyType(_dataType) == PropertyTypes.Bool;
            }
        }

        public bool IsInt {
            get {
                return PropertyType == PropertyTypes.Int || GetPropertyType(_dataType) == PropertyTypes.Int;
            }
        }

        public bool IsLayerMask {
            get {
                return DataType == typeof(LayerMask);
            }
        }

        public bool IsEnum {
            get {
                return PropertyType == PropertyTypes.Enum || GetPropertyType(_dataType) == PropertyTypes.Enum;
            }
        }

        public bool IsFloat {
            get {
                return PropertyType == PropertyTypes.Float || GetPropertyType(_dataType) == PropertyTypes.Float;
            }
        }

        public bool IsVector {
            get {
                if (PropertyType == PropertyTypes.Vector2 ||
                    PropertyType == PropertyTypes.Vector3 ||
                    PropertyType == PropertyTypes.Vector4 ||
                    PropertyType == PropertyTypes.Rect ||
                    PropertyType == PropertyTypes.RectOffset) {
                    return true;
                }
                else {
                    PropertyTypes t = GetPropertyType(_dataType);
                    return t == PropertyTypes.Vector2 ||
                    t == PropertyTypes.Vector3 ||
                    t == PropertyTypes.Vector4 ||
                    t == PropertyTypes.Rect ||
                    t == PropertyTypes.RectOffset;
                }
            }
        }

        public bool IsVector2 {
            get {
                return PropertyType == PropertyTypes.Vector2 || GetPropertyType(_dataType) == PropertyTypes.Vector2;
            }
        }

        public bool IsVector3 {
            get {
                return PropertyType == PropertyTypes.Vector3 || GetPropertyType(_dataType) == PropertyTypes.Vector3;
            }
        }

        public bool IsVector4 {
            get {
                return PropertyType == PropertyTypes.Vector4 || GetPropertyType(_dataType) == PropertyTypes.Vector4;
            }
        }

        public bool IsColor {
            get {
                return PropertyType == PropertyTypes.Color || GetPropertyType(_dataType) == PropertyTypes.Color;
            }
        }

        public bool IsRect {
            get {
                return PropertyType == PropertyTypes.Rect || GetPropertyType(_dataType) == PropertyTypes.Rect;
            }
        }

        public bool IsRectOffset {
            get {
                return PropertyType == PropertyTypes.RectOffset || GetPropertyType(_dataType) == PropertyTypes.RectOffset;
            }
        }

        public bool IsComponent {
            get {
                return PropertyType == PropertyTypes.Component || GetPropertyType(_dataType) == PropertyTypes.Component;
            }
        }

        public bool IsGameObject {
            get {
                return PropertyType == PropertyTypes.GameObject || GetPropertyType(_dataType) == PropertyTypes.GameObject;
            }
        }

        public bool IsObject {
            get {
                return PropertyType == PropertyTypes.Object || GetPropertyType(_dataType) == PropertyTypes.Object;
            }
        }

        public bool IsString {
            get {
                return PropertyType == PropertyTypes.String || GetPropertyType(_dataType) == PropertyTypes.String;
            }
        }

        #endregion

        #region VALUE ACCESSORS

        /// <summary>
        /// To reduce serialization overhead, most numeric values are stored in
        /// </summary>
        public Vector4 Value {
            get {
                return _value;
            }
            set {
                _value = value;
                ApplyValue();
            }
        }

        /// <summary>
        /// This sets the value of the currently selected attribute, or as a whole combined value if no
        /// attribute is selected.
        /// </summary>
        public float AttributeValue {
            get {
                if (Attribute == 0) {
                    return _value.x;
                }
                else
                if (Attribute == 1) {
                    return _value.y;
                }
                else
                if (Attribute == 2) {
                    return _value.z;
                }
                else
                if (Attribute == 3) {
                    return _value.w;
                }
                else {
                    return _value.x;
                }
            }
            set {
                if (Attribute <= -1) {
                    _value.x = _value.y = _value.z = _value.w = value;
                }
                else {
                    _value.x = value;
                }
                ApplyValue();
            }
        }

        public string StringValue {
            get {
                return _stringValue;
            }
            set {
                _stringValue = value;
                ApplyValue();
            }
        }

        /// <summary>
        /// Note that in most cases enum values are treated as ints to simplify keyframing and
        /// interpolation.
        /// </summary>
        public Enum EnumValue {
            get {
                if (typeof(Enum).IsAssignableFrom(DataType)) {
                    _enumValue = (Enum)Enum.ToObject(DataType, IntValue);
                }
                return _enumValue;
            }
            set {
                if (_enumValue != value) {
                    _enumValue = value;
                    ApplyValue();
                }
            }
        }

        public int IntValue {
            get {
                return (int)_value.x;
            }

            set {
                _value.x = value;
                if (typeof(Enum).IsAssignableFrom(DataType)) {
                    EnumValue = (Enum)Enum.ToObject(DataType, value);
                }
                ApplyValue();
            }

        }

        public bool BoolValue {
            get {
                return _value.x != 0f;
            }
            set {
                _value.x = value ? 1f : 0f;
                ApplyValue();
            }
        }

        public float FloatValue {
            get {
                if (IsInt || IsEnum) {
                    return (int)_value.x;
                }
                else {
                    return _value.x;
                }
            }
            set {
                if (Attribute <= -1) {
                    _value.x = _value.y = _value.z = _value.w = value;
                }
                else {
                    _value.x = value;
                }
                ApplyValue();
            }
        }

        public Vector2Int Vector2IntValue {
            get {
                return new Vector2Int((int)_value.x, (int)_value.y);
            }
            set {
                _value.x = value.x;
                _value.y = value.y;
                ApplyValue();
            }
        }

        public Vector3Int Vector3IntValue {
            get {
                return new Vector3Int((int)_value.x, (int)_value.y, (int)_value.z);
            }
            set {
                _value.x = value.x;
                _value.y = value.y;
                _value.z = value.z;
                ApplyValue();
            }
        }

        public Quaternion QuaternionValue {
            get {
                return new Quaternion(_value.x, _value.y, _value.z, _value.w);
            }
            set {
                _value.x = value.x;
                _value.y = value.y;
                _value.z = value.z;
                _value.w = value.w;
                ApplyValue();
            }
        }

        public Vector2 Vector2Value {
            get {
                return new Vector2(_value.x, _value.y);
            }
            set {
                _value = value;
                ApplyValue();
            }
        }

        public Vector3 Vector3Value {
            get {
                return new Vector3(_value.x, _value.y, _value.z);
            }
            set {
                _value = value;
                ApplyValue();
            }
        }

        public Vector4 Vector4Value {
            get {
                return _value;
            }
            set {
                _value = value;
                ApplyValue();
            }
        }

        public Color ColorValue {
            get {
                return new Color(_value.x, _value.y, _value.z, _value.w);
            }
            set {
                _value = new Vector4(value.r, value.g, value.b, value.a);
                ApplyValue();
            }
        }

        public RectInt RectIntValue {
            get {
                return new RectInt((int)_value.x, (int)_value.y, (int)_value.z, (int)_value.w);
            }
            set {
                _value.x = value.xMin;
                _value.y = value.yMin;
                _value.z = value.xMax;
                _value.w = value.yMax;
                ApplyValue();
            }
        }

        public Rect RectValue {
            get {
                return new Rect(_value.x, _value.y, _value.z, _value.w);
            }
            set {
                _value.x = value.xMin;
                _value.y = value.yMin;
                _value.z = value.xMax;
                _value.w = value.yMax;
                ApplyValue();
            }
        }

        public UnityEngine.Object ObjectValue {
            get {
                return _objectValue;
            }
            set {
                if (_objectValue != value) {
                    _objectValue = value;
                    ApplyValue();
                }
            }
        }

        public GameObject GameObjectValue {
            get {
                return _gameObjectValue;
            }
            set {
                if (_gameObjectValue != value) {
                    _gameObjectValue = value;
                    ApplyValue();
                }
            }
        }

        public Component ComponentValue {
            get {
                return _componentValue;
            }
            set {
                if (_componentValue != value) {
                    _componentValue = value;
                    ApplyValue();
                }
            }
        }

        #endregion

#if UNITY_EDITOR

        private string _displayNameType;

        [NonSerialized]
        public bool ShowPropertyObject = false;

        /// <summary>
        /// Presents a formatted name with attribute and data type for display purposes. 
        /// </summary>
        public string DisplayNameType {
            get {
                if (string.IsNullOrEmpty(_displayNameType)) _displayNameType = GetNameAndAttribute("(Unassigned)", false, true, true);
                return _displayNameType;
            }
            set {
                _displayNameType = value;
            }
        }

        private void DebugListAllProperties()
        {
            if (Comp != null && Handler == null && FieldInfo == null && PropertyInfo == null) {
                Type t = Comp.GetType();
                FieldInfo = t.GetField(_Name, BindingFlags);
                if (FieldInfo != null) {
                    if (DebugEnabled) Debug.Log("Property[" + _Name + "].GetDataType: FieldInfo:" + DataType);
                }
                else {
                    PropertyInfo = Property.GetPropertyInfo(t, _Name);
                    if (PropertyInfo != null) {
                        if (DebugEnabled) Debug.Log("Property[" + _Name + "].GetDataType: PropertyInfo:" + DataType);
                    }
                }
            }
        }

        /// <summary>
        /// This method should be called after any new assignements are made and the property owner needs
        /// to be notified to make any necessary updates, such as updating the name of a channel to match
        /// the new property assignment.
        /// </summary>
        public void NotifyChange(Property.PropertyTypes originalType, int originalAttribute)
        {
            if (Owner != null) {
                if (DebugEnabled) Debug.Log(Name + ".Property.NotifyChange: " + Owner.GetType() + " originalType:" + originalType + " originalAttribute:" + originalAttribute);
                Owner.OnPropertyChanged(this, originalType, originalAttribute);
            }
            else {
                Debug.LogWarning(GetNameAndAttribute("(unassigned)") + ".NotifyChange: NO OWNER");
            }
        }

        /// <summary>
        /// Returns an indexed list (name, type) of all the writtable properties on the component.
        /// </summary>
        public SDictionary<string, Type> GetPropertyDataTypes(PropertyFilters filter, in string[] exclusions)
        {
            SDictionary<string, Type> list = null;

            if (Comp != null) {
                bool showDefaultProps = true;
                FindPropertiesHandler(false);
                if (Handler != null) {
                    /// Get the list of property names from the Handler. This assumes they are already
                    /// sorted
                    list = Handler.GetProperties();
                    showDefaultProps = Handler.ShowDefaultProperties;
                }
                if (list != null && list.Count > 0) {
                    if (list.Count > 1) list.Add("-", typeof(Nullable));
                }
                else list = null;

                if (showDefaultProps || TimeflowPreferences.Current.ExposeAllProperties) {
                    /// Each behavior can optionally define a list of property names it supports. This can
                    /// be helpful to expose relevant (animatable) properties with properly formatted names
                    /// while hiding all others properties. The handler can block this if it does not show
                    /// default properties. 
                    if (typeof(IBehaviorProperties).IsAssignableFrom(Comp.GetType())) {
                        IBehaviorProperties m = (IBehaviorProperties)Comp;
                        if (m != null) {
                            SDictionary<string, Type> mlist = m.GetProperties();
                            if (mlist == null || mlist.Count == 0) {
                                // No properties defined by the behavior
                            }
                            else {
                                showDefaultProps = false; // Prevent default props from showing
                                if (list == null) {
                                    list = mlist;
                                }
                                else {
                                    // Combine the handler list with the behavior list
                                    foreach (KeyValuePair<string, Type> k in mlist) {
                                        list.Add(k.Key, k.Value);
                                    }
                                }
                            }
                        }
                    }
                }


                if (showDefaultProps || list == null || TimeflowPreferences.Current.ExposeAllProperties) {
                    SDictionary<string, Type> defaults = GetDefaultProperties(Comp.GetType(), filter, PropertyExclusions);
                    if (defaults != null && defaults.Count > 0) {
                        if (list != null) {
                            if (list.Count > 1) list.Add("-", typeof(Nullable));

                            foreach (KeyValuePair<string, Type> k in defaults) {
                                list.Add(k.Key, k.Value);
                            }
                        }
                        else list = defaults;
                    }
                }

                if (list == null || list.Count == 0) {
                    list = null;
                }
                else {

                }
            }
            else {
                Handler = null;
            }
            return list;
        }

#endif

    }


}//AxonGenesis
