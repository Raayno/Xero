// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public class ComponentPresetListItem
    {
        public int Index;
        public string OwnerClass;
        public string Name;
        public SerializedPropertyType Type;
        public SerializedProperty Property;
        public bool IsInherited;
        public bool IsSelected;

        public ComponentPresetListItem(int index, string owner, string name, SerializedPropertyType type, SerializedProperty property, bool inherited, bool selected)
        {
            Index = index;
            OwnerClass = owner;
            Name = name;
            Type = type;
            Property = property;
            IsInherited = inherited;
            IsSelected = selected;
        }
    }

    public class ComponentPresetWindow : EditorWindow
    {
        public static readonly Color SelectionColor = new Color(0.5f, 0.8f, 1f, 1f);

        private const int Width = 600;
        private const int MinHeight = 300;
        private const string kSelectedOnlyPref = "ComponentPresetWindow_SelectedOnly";
        private const string kInheritedFoldout = "ComponentPresetWindow_InheritedFoldout";
        private static readonly string EditorPrefsFilePathKey = "ComponentPresetSavePath";
        private static bool[] presetSelection = null;
        private static bool? _SelectedOnly = null;
        private static List<ComponentPresetListItem> _Items;
        private static Type _PriorType;

        private static string _Search = "";

        public static string PresetName { get; set; } = "";

        public static void Open(UnityEngine.Object comp)
        {
            if (comp == null) {
                Debug.LogError("No component selected.");
                return;
            }
            var window = CreateInstance<ComponentPresetWindow>();
            window.titleContent = new GUIContent("Save Preset");
            window._TargetComponent = comp;
            window._SerializedObject = new SerializedObject(comp);
            window.LoadProperties();
            window.ShowUtility();
        }

        public static void OpenFromMenu(object obj)
        {
            if (obj is UnityEngine.Object component) {
                Open(component);
            }
            else {
                Debug.LogError("Invalid object type. Expected a UnityEngine.Object.");
            }
        }

        public static void OnPresetApplied(ComponentPreset preset)
        {
            if (preset == null) {
                Debug.LogError("Preset is null.");
                return;
            }

            PresetName = preset.DisplayName;

            if (preset.Properties.Count > 0) {
                // Base the next save preset selection on the preset applied. This facilitates editing and resaving presets.
                int maxIndex = preset.Properties.Max(p => p.Index);
                presetSelection = new bool[maxIndex + 1];

                foreach (var property in preset.Properties) {
                    int index = property.Index;
                    if (index >= 0 && index < presetSelection.Length) {
                        presetSelection[index] = true;
                    }
                }
            }
        }

        public bool ShowSelectedOnly {
            get {
                if (_SelectedOnly == null) _SelectedOnly = EditorPrefs.GetBool(kSelectedOnlyPref, true);
                return (bool)_SelectedOnly;
            }
            set {
                _SelectedOnly = value;
                EditorPrefs.SetBool(kSelectedOnlyPref, value);
                PositionWindow(true);
            }
        }


        private UnityEngine.Object _TargetComponent;
        private SerializedObject _SerializedObject;
        private List<Type> _DeclaringTypes;
        private Vector2 _Position;
        private Vector2 _ScrollPos;
        private Texture2D _Icon;

        private int _PrimaryTotal;
        private int _PrimarySelected;
        private int _InheritedTotal;
        private int _InheritedSelected;
        private int _InheritedDisplayed;
        private int _PrimaryDisplayed;

        private bool isPositioned = false;
        private bool propertiesFoldout = true;
        private bool hasInheritedProperties = false;

        private ComponentPreset Preset;

        private bool InhertiedFoldout {
            get {
                return EditorPrefs.GetBool(kInheritedFoldout, false);
            }
            set {
                EditorPrefs.SetBool(kInheritedFoldout, value);
            }
        }

        public string Name {
            get {
                if (string.IsNullOrEmpty(PresetName)) {
                    PresetName = _TargetComponent.GetType().Name + " Preset";
                }
                return PresetName;
            }
            set {
                if (PresetName != value) {
                    PresetName = value;
                    //Debug.Log($"Name:{PresetName}");
                }
            }
        }

        public Color Color { get; set; } = Color.white;

        public bool AnySelected => _Items != null && _Items.Any(c => c.IsSelected);

        private bool IsSearching => !string.IsNullOrEmpty(_Search);

        private void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        }

        private void OnBeforeAssemblyReload()
        {
            Close();
        }

        private void PositionWindow(bool force = false)
        {
            bool alreadyPositioned = isPositioned;
            if (isPositioned && !force) return;

            bool search = IsSearching;
            isPositioned = Event.current != null;
            if (isPositioned) {
                int itemCount = _Items.Count;
                if (ShowSelectedOnly) {
                    itemCount = 0;
                    foreach (var item in _Items) {
                        if (search && item.Name.Contains(_Search, StringComparison.OrdinalIgnoreCase) == false) continue;
                        if (item.IsSelected) itemCount++;
                    }
                }
                float itemsHeight = (itemCount * (EditorGUIUtility.singleLineHeight + 7)) + 250;
                float height = Mathf.Max(itemsHeight, MinHeight);

                if (!alreadyPositioned) {
                    var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    _Position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                }

                float halfHeight = Screen.height / 2;
                if (height > halfHeight) {
                    _Position.y = (Screen.height / 2f) - (height / 2f) - 100; // Center vertically while keeping x position  
                }
                position = new Rect(_Position.x, _Position.y, Width, height);
            }
            else {
                position = new Rect(Screen.width / 2 - (Width / 2), Screen.height / 2 - (MinHeight / 2), Width, MinHeight);
            }
        }

        private bool IsSupportedPropertyType(SerializedPropertyType propertyType)
        {
            switch (propertyType) {
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.AnimationCurve:
                    return true;
                default:
                    return false;
            }
        }

        private bool[] GetSelection()
        {
            if (_PriorType == _TargetComponent.GetType()) {
                if (presetSelection != null && presetSelection.Length > 0) {
                    // Use the preset selection if available
                    return presetSelection;
                }
            }

            if (_Items == null || _Items.Count == 0) {
                return Array.Empty<bool>();
            }

            bool[] selection = new bool[_Items.Count];
            for (int i = 0; i < _Items.Count; i++) {
                selection[i] = _Items[i].IsSelected;
            }
            return selection;
        }

        public void SelectAll(bool select, bool searchOnly, bool inheritedOnly = false)
        {
            bool search = searchOnly && IsSearching;
            foreach (var item in _Items) {
                if (item.IsInherited != inheritedOnly) continue;
                if (search && !item.Name.Contains(_Search, StringComparison.OrdinalIgnoreCase)) continue;
                item.IsSelected = select;
                //Debug.Log($"SelectAll: {item.Name} - {select} (Search: {search})");
            }
        }

        private void ApplySelection(bool[] selection)
        {
            if (selection == null || _Items == null) {
                return;
            }

            for (int i = 0; i < _Items.Count; i++) {
                if (i >= selection.Length) continue;
                _Items[i].IsSelected = selection[i];
            }
        }

        private void LoadProperties()
        {
            bool[] priorSelection = GetSelection();

            _Items = new List<ComponentPresetListItem>();
            _DeclaringTypes = new List<Type>();

            Color = TimeflowPreferences.GetRandomTrackColor();

            var prop = _SerializedObject.GetIterator();
            bool enterChildren = true;

            int index = 0;

            while (prop.NextVisible(enterChildren)) {
                enterChildren = false;

                if (prop.name == "m_Script") continue;
                if (!IsSupportedPropertyType(prop.propertyType)) {
                    //Debug.LogWarning($"Skipping unsupported property type {prop.propertyType} for {prop.name}");
                    continue;
                }

                // Check if the property is defined in the target component's class or inherited  
                var targetType = _TargetComponent.GetType();
                var declaringType = targetType.GetProperty(prop.name)?.DeclaringType;
                if (declaringType == null) declaringType = targetType.GetField(prop.name)?.DeclaringType ?? targetType;
                bool isDefinedInClass = declaringType == targetType;

                if (!_DeclaringTypes.Contains(declaringType)) {
                    _DeclaringTypes.Add(declaringType);
                }

                if (!isDefinedInClass) hasInheritedProperties = true;
                //Debug.Log($"{targetType}.{prop.name}, Declaring Type: {declaringType}, Is Defined In Class: {isDefinedInClass} ");

                _Items.Add(new ComponentPresetListItem(index++, declaringType.Name, prop.displayName, prop.propertyType, prop.Copy(), !isDefinedInClass, isDefinedInClass));
            }

            // Sort declaring types by inheritance order  
            _DeclaringTypes.Sort((a, b) => a.IsSubclassOf(b) ? 1 : b.IsSubclassOf(a) ? -1 : 0);

            // Sort candidates by declaring type order and then by name  
            _Items.Sort((a, b) => {
                int typeComparison = _DeclaringTypes.IndexOf(a.Property.serializedObject.targetObject.GetType())
                    .CompareTo(_DeclaringTypes.IndexOf(b.Property.serializedObject.targetObject.GetType()));
                return typeComparison != 0 ? typeComparison : string.Compare(a.Name, b.Name, System.StringComparison.Ordinal);
            });

            // Give the component type a chance to process the items and selections
            if (_TargetComponent is IBehaviorPresets target) {
                target.OnBeforeSavePreset(ref _Items);
            }

            // Restore the previous selection to help the user save variations of the same preset
            ApplySelection(priorSelection);

            // Update the layout now that the items are loaded
            PositionWindow(true);
        }

        private void SetupPreset()
        {
            Type targetType = _TargetComponent.GetType();
            string presetTypeName = targetType.FullName + "ComponentPreset";
            Type presetType = Type.GetType(presetTypeName);
            if (presetType == null) {
                presetType = typeof(ComponentPreset);
            }

            if(Preset != null && Preset.GetType() != presetType) {
                Preset = null;
            }
            if (Preset == null) {
                Preset = ScriptableObject.CreateInstance(presetType) as ComponentPreset;
            }
            if (Preset == null) {
                Debug.LogError($"Failed to create preset instance of type {presetType}. Ensure the preset type is defined and valid.");
                return;
            }
            Preset.DisplayName = Name;
            Preset.Color = Color;
            Preset.ComponentType = targetType.AssemblyQualifiedName;
            Preset.ComponentName = targetType.Name;
        }

        private void SavePreset()
        {
            string assetName = EditorUtil.SanitizeAssetFileName(Name);
            //Debug.Log($"SavePreset {assetName}");

            // Retrieve the last used filepath from EditorPrefs
            string lastFilePath = EditorPrefs.GetString(EditorPrefsFilePathKey, "");
            //Debug.Log("lastFilePath: " + lastFilePath);

            string Filepath = EditorUtility.SaveFilePanelInProject(
                "Save Component Preset",
                assetName,
                "asset",
                "Enter a file name for the preset",
                lastFilePath
            );

            if (string.IsNullOrEmpty(Filepath)) return;
            AssetDatabase.Refresh();

            // Save the selected filepath to EditorPrefs
            EditorPrefs.SetString(EditorPrefsFilePathKey, Filepath);

            SetupPreset();
            
            Type targetType = _TargetComponent.GetType();
            //string presetTypeName = targetType.FullName + "ComponentPreset";
            //Type presetType = Type.GetType(presetTypeName);
            //if (presetType == null) {
            //    presetType = typeof(ComponentPreset);
            //}

            //var Preset = ScriptableObject.CreateInstance(presetType) as ComponentPreset;
            //if (Preset == null) {
            //    Debug.LogError($"Failed to create preset instance of type {presetType}. Ensure the preset type is defined and valid.");
            //    return;
            //}
            //Preset.DisplayName = Name;
            //Preset.Color = Color;
            //Preset.ComponentType = targetType.AssemblyQualifiedName;
            //Preset.ComponentName = targetType.Name;

            _SerializedObject.Update();

            presetSelection = new bool[_Items.Count];

            for (int i = 0; i < _Items.Count; i++) {
                presetSelection[i] = _Items[i].IsSelected;
                if (!_Items[i].IsSelected) continue;

                var displayProp = _Items[i];
                var data = new ComponentPresetProperty {
                    Index = displayProp.Index,
                    OwnerClass = displayProp.IsInherited ? displayProp.Property.serializedObject.targetObject.GetType().Name : targetType.Name,
                    PropertyPath = displayProp.Property.propertyPath,
                    PropertyType = ComponentPresetProperty.ToPropertyType(displayProp.Type)
                };

                bool isSupported = true;
                switch (data.PropertyType) {
                    case ComponentPresetProperty.PropertyTypes.Boolean:
                        data.BoolValue = displayProp.Property.boolValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Integer:
                        data.IntValue = displayProp.Property.intValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Float:
                        data.FloatValue = displayProp.Property.floatValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.String:
                        data.StringValue = displayProp.Property.stringValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Color:
                        data.ColorValue = displayProp.Property.colorValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.LayerMask:
                        data.IntValue = displayProp.Property.intValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Enum:
                        data.IntValue = displayProp.Property.enumValueIndex;
                        data.EnumNames = displayProp.Property.enumDisplayNames.ToArray();
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector2:
                        data.Vector2Value = displayProp.Property.vector2Value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector3:
                        data.Vector3Value = displayProp.Property.vector3Value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector4:
                        data.Vector4Value = displayProp.Property.vector4Value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Quaternion:
                        data.QuaternionValue = displayProp.Property.quaternionValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.ObjectReference:
                        data.ObjectReference = displayProp.Property.objectReferenceValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Rect:
                        data.RectValue = displayProp.Property.rectValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.RectInt:
                        data.RectIntValue = displayProp.Property.rectIntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector2Int:
                        data.Vector2IntValue = displayProp.Property.vector2IntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector3Int:
                        data.Vector3IntValue = displayProp.Property.vector3IntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Bounds:
                        data.BoundsValue = displayProp.Property.boundsValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.BoundsInt:
                        data.BoundsIntValue = displayProp.Property.boundsIntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.AnimationCurve:
                        if (displayProp.Property.animationCurveValue != null) {
                            data.AnimationCurveValue = new AnimationCurve(displayProp.Property.animationCurveValue.keys);
                        }
                        else {
                            data.AnimationCurveValue = null;
                        }
                        break;

                    default:
                        Debug.LogWarning($"Skipping unsupported type {displayProp.Type} for {displayProp.Property.propertyPath}");
                        isSupported = false;
                        break;
                }
                if (isSupported) Preset.Properties.Add(data);
            }

            if (_TargetComponent is IBehaviorPresets behaviorPresets) {
                behaviorPresets.OnSavePreset(null, Preset);
            }

            AssetDatabase.CreateAsset(Preset, Filepath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            SelectionUtil.Select(Preset);

            Debug.Log($"Saved preset '{assetName}' for {targetType.Name} at {Filepath}");//--KEEP

            Close();
        }

        private void OnGUI()
        {
            AxonGUI.Setup(200);
            PositionWindow();
            SetupPreset();

            if (_TargetComponent == null) {
                AxonGUI.Label("No component selected.");
                return;
            }

            GUI_Header();
            GUI_Properties();
            GUI_Footer();
        }

        private void GUI_Header()
        {
            if (_Icon == null) _Icon = EditorGUIUtility.ObjectContent(_TargetComponent, typeof(Transform)).image as Texture2D;

            GUI.color = Color;
            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleOpen, GUILayout.Height(25));
            GUI.color = Color.white;
            AxonGUI.Space(2);
            AxonGUI.ButtonIcon(_Icon);
            AxonGUI.Label(_TargetComponent.GetType().Name + " Preset", EditorStyles.boldLabel);

            AxonGUI.FlexibleSpace();

            GUI.color = IsSearching ? Color.cyan : Color.white;
            _Search = AxonGUI.FieldTextInline(null, "Search", _Search, GUILayout.Width(200));
            if (IsSearching && AxonGUI.ButtonRemove("Clear Search")) {
                _Search = null;
            }
            GUI.color = Color.white;
            if (AxonGUI.ButtonRefresh("Refresh View")) {
                PositionWindow(true);
            }
            AxonGUI.EndHorizontal();
        }

        private void GUI_Properties()
        {
            GUI.color = Color;
            AxonGUI.BeginBoxPadded();
            AxonGUI.BeginHorizontal();
            GUI.color = Color.white;
            Name = AxonGUI.FieldTextInline(null, "Name", Name);
            Color = AxonGUI.FieldColorInline(null, Color, false, GUILayout.Width(50));
            AxonGUI.EndHorizontal();
            AxonGUI.EndBoxPadded();

            _ScrollPos = EditorGUILayout.BeginScrollView(_ScrollPos);

            AxonGUI.Space();
            AxonGUI.BeginHorizontal();
            propertiesFoldout = AxonGUI.Foldout(propertiesFoldout, $"Showing {_PrimaryDisplayed} of {_PrimaryTotal} Properties ({_PrimarySelected} Selected)");
            if (AxonGUI.ButtonInline(ShowSelectedOnly ? "Show All" : "Show Selected Only")) {
                ShowSelectedOnly = !ShowSelectedOnly;
            }
            if (AxonGUI.ButtonInline("Select All")) {
                SelectAll(true, IsSearching, false);
            }
            if (AxonGUI.ButtonInline("None")) {
                SelectAll(false, IsSearching, false);
            }
            AxonGUI.EndHorizontal();

            if (propertiesFoldout) {
                GUI_Properties_List(false);
            }

            AxonGUI.BeginBoxPadded();
            Preset.GUI();
            AxonGUI.EndBoxPadded();

            if (hasInheritedProperties) {
                AxonGUI.BeginHorizontal();
                InhertiedFoldout = AxonGUI.Foldout(InhertiedFoldout, $"{_InheritedDisplayed} of {_InheritedTotal} Inherited Properties ({_InheritedSelected} Selected)");
                if (AxonGUI.ButtonInline("Select All")) {
                    SelectAll(true, IsSearching, true);
                }
                if (AxonGUI.ButtonInline("None")) {
                    SelectAll(false, IsSearching, true);
                }
                AxonGUI.EndHorizontal();
                if (InhertiedFoldout) {
                    GUI_Properties_List(true);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void GUI_Properties_List(bool inheritedOnly)
        {
            AxonGUI.BeginBoxPadded();

            if (inheritedOnly) {
                _InheritedTotal = 0;
                _InheritedSelected = 0;
                _InheritedDisplayed = 0;
            }
            else {
                _PrimaryTotal = 0;
                _PrimarySelected = 0;
                _PrimaryDisplayed = 0;
            }
            
            bool search = IsSearching;

            for (int i = 0; i < _Items.Count; i++) {
                if (_Items[i].IsInherited != inheritedOnly) continue;
                if (inheritedOnly) {
                    _InheritedTotal++;
                }
                else {
                    _PrimaryTotal++;
                }

                if (_Items[i].IsSelected) {
                    if (inheritedOnly) {
                        _InheritedSelected++;
                    }
                    else {
                        _PrimarySelected++;
                    }
                }

                if (!_Items[i].IsSelected && ShowSelectedOnly) continue;

                if(search && _Items[i].Name.Contains(_Search, StringComparison.OrdinalIgnoreCase) == false) continue;

                if (inheritedOnly) {
                    _InheritedDisplayed++;
                }
                else {
                    _PrimaryDisplayed++;
                }

                var displayProp = _Items[i];

                GUI.color = displayProp.IsSelected ? SelectionColor : Color.white;
                AxonGUI.BeginHorizontalBox();

                string name = displayProp.Name;
                if (Event.current != null && Event.current.shift) {
                    name = $"{displayProp.OwnerClass}.{displayProp.Name}";
                }
                displayProp.IsSelected = AxonGUI.FieldToggleLeft(null, name, displayProp.IsSelected);
                GUI.color = Color.white;

                AxonGUI.BeginDisabledGroup(!displayProp.IsSelected);

                // Add a value field based on the property type  
                switch (displayProp.Type) {
                    case SerializedPropertyType.Boolean:
                        displayProp.Property.boolValue = EditorGUILayout.Toggle(displayProp.Property.boolValue);
                        break;
                    case SerializedPropertyType.Integer:
                        displayProp.Property.intValue = EditorGUILayout.IntField(displayProp.Property.intValue);
                        break;
                    case SerializedPropertyType.Float:
                        displayProp.Property.floatValue = EditorGUILayout.FloatField(displayProp.Property.floatValue);
                        break;
                    case SerializedPropertyType.String:
                        displayProp.Property.stringValue = EditorGUILayout.TextField(displayProp.Property.stringValue);
                        break;
                    case SerializedPropertyType.Color:
                        displayProp.Property.colorValue = EditorGUILayout.ColorField(displayProp.Property.colorValue);
                        break;
                    case SerializedPropertyType.LayerMask:
                        displayProp.Property.intValue = AxonGUI.FieldLayerMask(null, displayProp.Property.intValue);
                        break;
                    case SerializedPropertyType.Enum:
                        displayProp.Property.enumValueIndex = EditorGUILayout.Popup(displayProp.Property.enumValueIndex, displayProp.Property.enumDisplayNames);
                        break;
                    case SerializedPropertyType.Vector2:
                        displayProp.Property.vector2Value = EditorGUILayout.Vector2Field("", displayProp.Property.vector2Value);
                        break;
                    case SerializedPropertyType.Vector3:
                        displayProp.Property.vector3Value = EditorGUILayout.Vector3Field("", displayProp.Property.vector3Value);
                        break;
                    case SerializedPropertyType.Vector4:
                        displayProp.Property.vector4Value = EditorGUILayout.Vector4Field("", displayProp.Property.vector4Value);
                        break;
                    case SerializedPropertyType.Quaternion:
                        Vector3 rot = displayProp.Property.quaternionValue.eulerAngles;
                        Vector3 r = EditorGUILayout.Vector3Field("", rot);
                        if(r != rot) {
                            displayProp.Property.quaternionValue = Quaternion.Euler(r);
                        }
                        break;
                    case SerializedPropertyType.ObjectReference:
                        displayProp.Property.objectReferenceValue = EditorGUILayout.ObjectField(displayProp.Property.objectReferenceValue, typeof(UnityEngine.Object), true);
                        break;
                    case SerializedPropertyType.Rect:
                        displayProp.Property.rectValue = EditorGUILayout.RectField(displayProp.Property.rectValue);
                        break;
                    case SerializedPropertyType.RectInt:
                        displayProp.Property.rectIntValue = EditorGUILayout.RectIntField(displayProp.Property.rectIntValue);
                        break;
                    case SerializedPropertyType.Vector2Int:
                        displayProp.Property.vector2IntValue = EditorGUILayout.Vector2IntField("", displayProp.Property.vector2IntValue);
                        break;
                    case SerializedPropertyType.Vector3Int:
                        displayProp.Property.vector3IntValue = EditorGUILayout.Vector3IntField("", displayProp.Property.vector3IntValue);
                        break;
                    case SerializedPropertyType.Bounds:
                        displayProp.Property.boundsValue = EditorGUILayout.BoundsField(displayProp.Property.boundsValue);
                        break;
                    case SerializedPropertyType.BoundsInt:
                        displayProp.Property.boundsIntValue = EditorGUILayout.BoundsIntField(displayProp.Property.boundsIntValue);
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        displayProp.Property.animationCurveValue = EditorGUILayout.CurveField(displayProp.Property.animationCurveValue);
                        break;
                    default:
                        EditorGUILayout.LabelField("Unsupported Type");
                        break;
                }

                AxonGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBoxPadded();

            if (GUI.changed) {
                _SerializedObject.ApplyModifiedProperties();
            }
        }

        private void GUI_Footer()
        {
            AxonGUI.Space();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.Label($"Selected Properties: {_Items.Count(c => c.IsSelected)} / {_Items.Count}");
            AxonGUI.FlexibleSpace();
            if (AxonGUI.ButtonInline(ShowSelectedOnly ? "Show All" : "Show Selected Only")) {
                ShowSelectedOnly = !ShowSelectedOnly;
            }
            if (AxonGUI.ButtonInline("Select All")) {
                SelectAll(true, IsSearching);
            }
            if (AxonGUI.ButtonInline("None")) {
                SelectAll(false, IsSearching);
            }
            AxonGUI.EndHorizontal();

            float buttonHeight = 25;
            AxonGUI.BeginHorizontalHeight(GUILayout.MinHeight(buttonHeight));
            if (AxonGUI.Button("Cancel", GUI.skin.button, GUILayout.Width(60), GUILayout.MinHeight(buttonHeight))) {
                Close();
            }
            AxonGUI.BeginDisabledGroup(!AnySelected);
            GUI.color = AnySelected ? SelectionColor : Color.white;
            if (AxonGUI.Button("Save Preset", GUI.skin.button, GUILayout.MinHeight(buttonHeight))) {
                SavePreset();
            }
            AxonGUI.EndDisabledGroup();
            AxonGUI.EndHorizontal();
        }


    }

}//AxonGenesis

#endif