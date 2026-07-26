// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public partial class AxonGUI
    {
        #region PROPERTIES

        public static void PropertySelect(AxonGenesisBehavior owner, Type type, GameObject obj, Property property)
        {
            PropertySelect(owner, type, obj, property, Property.PropertyFilters.All, null, false, false);
        }

        public static void PropertySelect(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, Property.PropertyFilters filter)
        {
            PropertySelect(owner, type, obj, property, filter, null, false, false);
        }

        public static void PropertySelect(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, string label, bool multiChannel)
        {
            PropertySelect(owner, type, obj, property, Property.PropertyFilters.All, label, multiChannel, false);
        }

        public static void PropertySelect(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, Property.PropertyFilters filter, string label, bool multiChannel)
        {
            PropertySelect(owner, type, obj, property, filter, label, multiChannel, false);
        }

        public static void PropertySelectInline(AxonGenesisBehavior owner, Type type, GameObject obj, Property property)
        {
            PropertySelect(owner, type, obj, property, Property.PropertyFilters.All, null, false, true);
        }

        public static void PropertySelectInline(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, string label, bool multiChannel)
        {
            PropertySelect(owner, type, obj, property, Property.PropertyFilters.All, label, multiChannel, true);
        }

        public static void PropertySelectInline(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, Property.PropertyFilters filter, string label, bool multiChannel)
        {
            PropertySelect(owner, type, obj, property, filter, label, multiChannel, true);
        }

        public static void PropertySelect(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, Property.PropertyFilters filter, string label, bool multiChannel, bool inline)
        {
            PropertySelect(owner, type, obj, property, filter, label, multiChannel, inline, false, true);
        }

        public static void PropertySelect(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, Property.PropertyFilters filter, string label, bool multiChannel, bool inline, bool showValue, bool showEnable)
        {
            BeginHorizontal();

            if (!string.IsNullOrEmpty(label)) {
                if (inline) {
                    AxonGUI.LabelInline(label);
                }
                else {
                    AxonGUI.Label(label, GUILayout.Width(AxonGUI.LabelWidth));
                }
            }

            Property.PropertyTypes originalType = property.PropertyType;
            int originalAttribute = property.Attribute;

            Type dataType = property.GetDataType();

            if (showEnable) {
                property.IsEnabled = FieldToggleEnabled(owner, property.IsEnabled, new RectOffset(1, 0, 2, 0));
            }

            if (property.CanBeAssigned) {
                AxonGUI.SetTooltip("Select a property from a component or material on this game object.");
            }
            else {
                AxonGUI.SetTooltip("This property mapping cannot be changed.");
            }

            if (dataType == null) {
                Warning("The assigned property does not exist. Please select a new property.");
            }
            EditorGUI.BeginDisabledGroup(!property.IsEnabled || !property.CanBeAssigned);

            property.Owner = owner;
            property.AssignToObject = obj;
            if (property.Comp == null && !property.IsDataOnly) {
                property.ShowPropertyObject = true;
            }
            if (!property.IsDataOnly && !ShowPropertyObjectField) {
                if (AxonGUI.ButtonTexture(property.ShowPropertyObject ? AxonUI.Icons.Ungrouped : AxonUI.Icons.Grouped, "Show the game object reference this property belongs to.")) {
                    property.ShowPropertyObject = !property.ShowPropertyObject;
                }
            }
            if (ShowPropertyObjectField || property.ShowPropertyObject) {
                UndoName = "Set Property Object";
                property.Comp = AxonGUI.FieldObjectInline(owner, property.Comp, typeof(Component), true, GUILayout.MaxWidth(300)) as Component;
                if (obj != null && property.Comp != null && obj != property.Comp.gameObject) {
                    if (ButtonRemove("Reset the property mapping to the current game object.")) {
                        property.Comp = obj.GetComponent(property.Comp.GetType());
                        property.Prepare();
                        property.ReadValue();
                    }
                }
            }

            string name = property.NameAndAttribute;
            if (!property.IsDataOnly && (string.IsNullOrEmpty(name) || dataType == null)) {
                name = null;
            }
            if (string.IsNullOrEmpty(name)) {
                GUI.backgroundColor = AxonColor.Error;
                name = "Please Select a Property";
                if (!warnings.Contains(property)) {
                    warnings.Add(property);
                    Debug.LogWarning($"The property mapping needs to be assigned for {owner.name}.{owner.GetType()}", owner);
                }
            }
            else {
                if (warnings.Contains(property)) {
                    warnings.Remove(property);
                }
            }
            EditorGUI.EndDisabledGroup();

            if (AxonGUI.Button(name)) {
                PropertySelectMenu(owner, type, property.Comp == null ? obj : property.Comp.gameObject, property, filter, null, true);
            }

            EditorGUI.BeginDisabledGroup(!property.IsEnabled || !property.CanBeAssigned);
            GUI.color = AxonColor.Default;
            GUI.backgroundColor = AxonColor.Default;

            EditorGUI.BeginChangeCheck();

            // Force data type should never be needed - only enable for debugging
            if (property.ShowPropertyObject) {
                UndoName = "Set Force Property Type";
                property.ForcePropertyType = (Property.PropertyTypes)AxonGUI.FieldEnumPopupInline(owner, property.ForcePropertyType);
            }
            if (dataType != null) {
                if (showValue) {
                    UndoName = "Set Property Value";
                    if (dataType == typeof(bool)) {
                        property.BoolValue = AxonGUI.FieldToggleInline(owner, "Value", property.BoolValue);
                    }
                    else
                    if (dataType == typeof(Int32)) {
                        property.IntValue = AxonGUI.FieldIntInline(owner, "Value", property.IntValue);
                    }
                    else
                    if (dataType == typeof(Single)) {
                        property.FloatValue = AxonGUI.FieldFloatInline(owner, "Value", property.FloatValue);
                    }
                    else
                    if (dataType == typeof(Vector2)) {
                        property.Vector2Value = AxonGUI.FieldVector2Inline(owner, "Value", property.Vector2Value);
                    }
                    else
                    if (dataType == typeof(Vector3)) {
                        property.Vector3Value = AxonGUI.FieldVector3Inline(owner, "Value", property.Vector3Value);
                    }
                    else
                    if (dataType == typeof(Vector4)) {
                        property.Vector4Value = AxonGUI.FieldVector4Inline(owner, "Value", property.Vector4Value);
                    }
                    else
                    if (dataType == typeof(Color)) {
                        property.ColorValue = AxonGUI.FieldColorInline(owner, "Value", property.ColorValue, true);
                    }
                    else
                    if (typeof(GameObject).IsAssignableFrom(dataType)) {
                        property.GameObjectValue = (GameObject)AxonGUI.FieldObjectInline(owner, "Value", property.GameObjectValue, typeof(GameObject), true);
                    }
                    else
                    if (typeof(Component).IsAssignableFrom(dataType)) {
                        property.ComponentValue = (Component)AxonGUI.FieldObjectInline(owner, "Value", property.ComponentValue, typeof(Component), true);
                    }
                }

                int attr = PropertySelectAttribute(property.PropertyType, property.Attribute, true);
                if (property.Attribute != attr) {
                    _Undo(owner, "Set Property Attribute", $"{attr}");
                    property.Attribute = attr;
                }
            }
            EditorGUI.EndDisabledGroup();

            property.DebugEnabled = FieldToggleDebug(property.DebugEnabled);

            if (EditorGUI.EndChangeCheck()) {
                property.NotifyChange(originalType, originalAttribute);
                property.Prepare();
            }

            EndHorizontal(false);
        }

        public static int PropertySelectAttribute(Property.PropertyTypes property, int index, bool canCombine)
        {
            return PropertySelectAttribute(":", property, index, canCombine, true);
        }

        public static int PropertySelectAttribute(string label, Property.PropertyTypes property, int index, bool canCombine, bool canEdit)
        {
            EditorGUI.BeginDisabledGroup(!canEdit);
            if (Property.HasMultipleAttributes(property)) {
                int count = Property.GetAttributeCount(property) - 1;
                string[] names = Property.GetAttributeNames(property, canCombine);
                if (names != null) {
                    int a = index;
                    if (a < 0) {
                        a *= -1;
                        a = a + count;
                    }

                    AxonGUI.SetTooltip("Select a single attribute (axis) or select Combined to assign a whole vector, color, or rect value. Uniform applies the same value to each axis, mostly used for applying uniform scale.");
                    int b = AxonGUI.FieldPopupInline(null, label, a, names);
                    if (b != a) {
                        if (b > count) {
                            b -= count;
                            b *= -1;
                        }
                        index = b;

                    }
                }
            }
            else {
                string[] names = { "Single" };
                AxonGUI.FieldPopupInline(null, label, 0, names);
            }
            EditorGUI.EndDisabledGroup();
            return index;
        }

        public static string PropertySelectNameFolder(string key)
        {
            string folder = "";
            int i = 0;
            if (!string.IsNullOrEmpty(key)) {
                if (key.StartsWith("m_")) {
                    i = 2;
                }
                else
                if (key[0] == '_') {
                    i = 1;
                }
                folder = (key[i] + "/").ToUpper();
            }
            return folder;
        }

        public static void PropertySelectMenu(AxonGenesisBehavior owner, Type type, GameObject obj, Property property, Property.PropertyFilters filter, string prefix, bool showChannels = true)
        {
            if (property != null && !property.CanBeAssigned) return;

            GenericMenu menu = new GenericMenu();

            PropertySelectMenu(menu, type, owner, obj, property, filter, prefix, showChannels, PropertySelectMenuSelected);

            Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
            menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
        }

        public static void PropertySelectMenu(GenericMenu menu, Type type, AxonGenesisBehavior owner, GameObject obj, Property property, Property.PropertyFilters filter, string prefix, bool showChannels, GenericMenu.MenuFunction2 onselect)
        {
            if (obj == null) {
                menu.AddItem(new GUIContent(prefix + "Please assign an object"), false, null, null);
            }
            else {

                string clipboard = "";
                if (Timeflow.Active != null && Timeflow.Active.View != null) {
                    clipboard = Timeflow.Active.View.Display.SearchTerm;
                }

                Component[] components = obj.GetComponents<Component>();
                Dictionary<Type, int> typesFound = new Dictionary<Type, int>();
                int i = 0;
                foreach (Component c in components) {
                    if (c == null || c == owner) continue;
                    // Count the components by type and separate multiple instances by an index number
                    string nameIndex = "";
                    Type cType = c.GetType();
                    if (typesFound.ContainsKey(cType)) {
                        typesFound[cType]++;
                        nameIndex = "" + typesFound[cType];
                    }
                    else {
                        typesFound.Add(cType, 0);
                    }

                    SDictionary<string, Type> props = Property.GetAvailablePropertyDataTypes(c, Property.PropertyFilters.All);// filter applied below to optimize list generation
                    if (props != null) {
                        int alphabetizeThreshold = 10;
                        Dictionary<string, int> folders = GetPropertyFolders(props, filter);

                        string name = i + ". " + c.GetType();
                        name = StringUtil.ClassName(name);

                        // Distinguish multiple instances of the same component type
                        if (typeof(IBehavior).IsAssignableFrom(c.GetType())) {
                            IBehavior m = (IBehavior)c;
                            if (m != null && !string.IsNullOrEmpty(m.Name)) {
                                nameIndex = m.Name;
                            }
                        }
                        if (!string.IsNullOrEmpty(nameIndex)) {
                            name += " [" + nameIndex + "] ";
                        }

                        foreach (KeyValuePair<string, Type> p in props) {
                            if (p.Key == "-") {
                                //menu.AddSeparator(prefix);
                            }
                            else {
                                bool canInclude = true;
                                if (filter == Property.PropertyFilters.NumericOnly) {
                                    canInclude = Property.IsNumeric(p.Value);
                                }
                                else
                                if (filter == Property.PropertyFilters.ObjectOnly) {
                                    canInclude = Property.IsObjectType(p.Value);
                                }
                                else
                                if (filter == Property.PropertyFilters.ColorOnly) {
                                    canInclude = Property.IsColorType(p.Value);
                                }
                                if (canInclude) {
                                    string folder = "";
                                    if (folders != null) {
                                        folder = PropertySelectNameFolder(p.Key);
                                        if (folders != null && folders.ContainsKey(folder)) {
                                            if (folders[folder] < alphabetizeThreshold) {
                                                folder = "";
                                            }
                                        }
                                    }
                                    Property prop = new Property();
                                    prop.AssignToObject = c.gameObject;
                                    prop.Owner = owner;
                                    prop.Comp = c;
                                    prop.Name = p.Key;
                                    prop.GetDataType();

                                    bool isThisProp = property != null && property.NameMatches(prop);

                                    // If all properties are exposed then show the original property name instead of the handler's fancy name
                                    string pname = TimeflowPreferences.Current.ExposeAllProperties ? p.Key : prop.Name;

                                    string baseName = prefix + name + "/" + folder + pname + " (" + StringUtil.ClassName("" + p.Value) + ")";
                                    PropertySelectMenuItem(baseName, owner, prop, property, showChannels, menu, onselect);

                                    if (!string.IsNullOrEmpty(clipboard)) {
                                        if (string.Compare(p.Key.Replace(" ", ""), clipboard, true) == 0 || p.Key.ToLower().Contains(clipboard.ToLower())) {
                                            baseName = prefix + "Search/" + pname + " (" + StringUtil.ClassName("" + p.Value) + ")";
                                            PropertySelectMenuItem(baseName, owner, prop, property, showChannels, menu, onselect);

                                        }
                                    }
                                }
                            }
                        }
                    }
                    i++;
                }

                PropertiesOfMaterial materialProp = new PropertiesOfMaterial();
                SDictionary<string, SDictionary<string, Type>> materials = materialProp.GetMaterialPropertiesForObject(obj);
                if (materials == null || materials.Count == 0) {
                    // Nothing to display
                }
                else {
                    i = 0;
                    foreach (KeyValuePair<string, SDictionary<string, Type>> list in materials) {
                        string name = "Material: " + list.Key;
                        int alphabetizeThreshold = 5;
                        Dictionary<string, int> folders = GetPropertyFolders(list.Value, filter);

                        foreach (KeyValuePair<string, Type> p in list.Value) {
                            string folder = "";
                            if (folders != null) {
                                folder = PropertySelectNameFolder(p.Key);
                                if (folders != null && folders.ContainsKey(folder)) {
                                    if (folders[folder] < alphabetizeThreshold) {
                                        folder = "";
                                    }
                                }
                            }

                            Property prop = new Property();
                            prop.Name = p.Key;
                            prop.Owner = owner;
                            prop.AssignToObject = obj;
                            prop.Comp = obj.transform;
                            prop.IsMaterial = true;
                            prop.DataType = p.Value;

                            PropertiesOfMaterial matProp = new PropertiesOfMaterial(materialProp);
                            matProp.GameObject = obj;
                            matProp.MaterialName = list.Key;
                            matProp.GetMaterial();
                            matProp.Name = p.Key;

                            prop.Handler = matProp;
                            prop.GetDataType(true);

                            string mname = prefix + name + "/" + folder + p.Key + " (" + StringUtil.ClassName("" + p.Value) + ")";
                            if (showChannels && Property.IsMultiNumeric(prop.DataType) && prop.DataType != typeof(Color)) {
                                PropertySelectMenuItem(mname, owner, prop, property, showChannels, menu, onselect);
                            }
                            else {
                                PropertyMenuItem sinfo = new PropertyMenuItem(owner, property, prop, -1, false, false);
                                menu.AddItem(new GUIContent(mname), false, onselect, sinfo);
                            }
                        }
                        i++;
                    }
                }

                // Add data only channels
                menu.AddSeparator(prefix);
                SDictionary<string, Type> dataProps = Property.GetDataOnlyPropertiesList(filter);
                foreach (KeyValuePair<string, Type> p in dataProps) {
                    Property prop = new Property();
                    prop.Owner = owner;
                    prop.AssignToObject = obj;
                    prop.Name = p.Key;
                    prop.DisplayName = p.Key;
                    prop.IsDataOnly = true;
                    prop.PropertyType = Property.DataTypeToPropertyType(p.Value);
                    prop.GetDataType();

                    bool isThisProp = property != null && property.NameMatches(prop);
                    string baseName = prefix + "Data Only/" + p.Key;
                    PropertySelectMenuItem(baseName, owner, prop, property, showChannels, menu, onselect);
                }

                // Disabled in favor of placing preset options in the channel context menu
                //PresetsMenu(menu, prefix, type, owner, obj);

                /// Display list of channel links (if any) to animate the link Blend values (a special case
                /// scenario)
                TimeflowObject tobj;
                obj.TryGetComponent<TimeflowObject>(out tobj);
                if (tobj != null && tobj.AllChannels != null && tobj.AllChannels.Count > 0) {
                    foreach (TimeflowChannel ch in tobj.AllChannels) {
                        if (ch.IsLinkEnabled) {
                            string linkName = ch.Name + " Blend";
                            Property prop = new Property();
                            prop.Owner = owner;
                            prop.AssignToObject = obj;
                            prop.Name = linkName;
                            prop.DisplayName = linkName;
                            prop.LinkID = ch.UniqueID;
                            prop.IsDataOnly = true;
                            prop.PropertyType = Property.PropertyTypes.Float;
                            prop.GetDataType();

                            bool isThisProp = property != null && property.NameMatches(prop);
                            string baseName = prefix + "Channel Links/" + linkName;
                            PropertySelectMenuItem(baseName, owner, prop, property, showChannels, menu, onselect);

                        }
                    }
                }

                if (!string.IsNullOrEmpty(clipboard)) {
                    menu.AddSeparator(prefix);
                    menu.AddItem(new GUIContent(prefix + "Search/:" + (clipboard == null ? "(copy a property name to find)" : clipboard)), clipboard != null, null);
                }
            }
        }

        public static void PropertySelectMenuItem(string baseName, AxonGenesisBehavior owner, Property menuProp, Property targetProp, bool showChannels, GenericMenu menu, GenericMenu.MenuFunction2 onselect)
        {
            bool isThisProp = targetProp != null && targetProp.NameMatches(menuProp);
            bool isSelected = false;

            if (showChannels && Property.IsMultiNumeric(menuProp.DataType)) {
                string[] channelNames = Property.GetAttributeNames(menuProp.DataType, false);
                for (int x = 0; x < channelNames.Length; x++) {
                    isSelected = isThisProp && targetProp.Attribute == x;
                    PropertyMenuItem sinfo = new PropertyMenuItem(owner, targetProp, new Property(menuProp), x, false, false);
                    menu.AddItem(new GUIContent(baseName + "/" + channelNames[x]), isSelected, onselect, sinfo);
                }
                menu.AddSeparator(baseName + "/");

                isSelected = isThisProp && targetProp.Attribute == -1;
                PropertyMenuItem uinfo = new PropertyMenuItem(owner, targetProp, new Property(menuProp), -1, false, false);
                menu.AddItem(new GUIContent(baseName + "/Combined"), isSelected, onselect, uinfo);

                isSelected = isThisProp && targetProp.Attribute == -2;
                uinfo = new PropertyMenuItem(owner, targetProp, new Property(menuProp), -2, true, false);
                menu.AddItem(new GUIContent(baseName + "/Uniform Value"), isSelected, onselect, uinfo);
            }
            else {
                isSelected = isThisProp;
                PropertyMenuItem sinfo = new PropertyMenuItem(owner, targetProp, new Property(menuProp), -1, false, false);
                menu.AddItem(new GUIContent(baseName), isSelected, onselect, sinfo);
            }
        }

        public static void PropertySelectMenuSelected(object info)
        {
            PropertyMenuItem prop = (PropertyMenuItem)info;
            if (prop != null && prop.ToProperty != null) {
                Property.PropertyTypes originalType = prop.ToProperty.PropertyType;
                int originalAttribute = prop.ToProperty.Attribute;

                prop.ToProperty.Copy(prop.FromProperty);
                prop.ToProperty.Name = prop.FromProperty.Name;
                prop.ToProperty.NotifyChange(originalType, originalAttribute);
            }
        }

        public static bool PropertySelectCopyButton(Component obj, string name, int attribute = -1)
        {
            bool button = ButtonTexture(AxonUI.Icons.PropertyCopy, "Copy Property");
            if (button) {
                copiedProperty = new Property();
                copiedProperty.Comp = obj;
                copiedProperty.Name = name;
                copiedProperty.Attribute = attribute;
            }
            return button;
        }

        public static bool PropertySelectPasteButton(Property targetProp)
        {
            if (copiedProperty != null) {
                bool button = ButtonTexture(AxonUI.Icons.PropertyPaste, "Paste Property");
                if (button) {
                    targetProp.Copy(copiedProperty);
                }
                return button;
            }
            return false;
        }

        public static Dictionary<string, int> GetPropertyFolders(SDictionary<string, Type> props, Property.PropertyFilters filter)
        {
            // Force long lists to break into sub menus by starting letter
            bool alphabetize = props.Count > 30;

            // Build a list of subfolders to organize long lists
            Dictionary<string, int> folders = null;
            if (alphabetize) {
                foreach (KeyValuePair<string, Type> p in props) {
                    bool canInclude = true;
                    if (filter == Property.PropertyFilters.NumericOnly) {
                        canInclude = Property.IsNumeric(p.Value);
                    }
                    else
                    if (filter == Property.PropertyFilters.ObjectOnly) {
                        canInclude = Property.IsObjectType(p.Value);
                    }
                    else
                    if (filter == Property.PropertyFilters.ColorOnly) {
                        canInclude = Property.IsColorType(p.Value);
                    }
                    if (canInclude) {
                        string folder = "";
                        folder = PropertySelectNameFolder(p.Key);

                        if (folders == null) folders = new Dictionary<string, int>();

                        if (folders.ContainsKey(folder)) {
                            folders[folder]++;
                        }
                        else {
                            folders.Add(folder, 1);
                        }
                    }
                }
            }
            return folders;
        }

        #endregion    }
    }
}
#endif
