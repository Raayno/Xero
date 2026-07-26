// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
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
    public partial class ComponentPreset : ScriptableObject
    {
        public string ComponentType;
        public string ComponentName;


        public ComponentPresetData Data;
        public bool CanReinitialize = false;

        [SerializeField] private bool _isSetup = false;

        public Type GetComponentType() => ComponentType == null ? typeof(Component) : Type.GetType(ComponentType);

        public List<ComponentPresetProperty> Properties = new List<ComponentPresetProperty>();

        public virtual void OnFirstSetup()
        {
            if (_isSetup) return;
            _isSetup = true;
            Color = TimeflowPreferences.GetRandomTrackColor();
        }

        public void ApplySelect()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("➕ Instantiate"), false, () => { Apply(AdvancedPreset.Modes.Instantiate); });
            menu.AddItem(new GUIContent("⏬ Replace"), false, () => { Apply(AdvancedPreset.Modes.Replace); });
            menu.AddItem(new GUIContent("🔀 Combine"), false, () => { Apply(AdvancedPreset.Modes.Combine); });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("✏️ Edit..."), false, () => { Edit(); });

            menu.ShowAsContext();
        }

        public void Edit()
        {
            // Select the asset to edit in the insepctor
            SelectionUtil.Select(this);
        }

        public void Apply(AdvancedPreset.Modes mode)
        {
            AdvancedPreset.Mode = mode;
            Apply();
        }

        public void ApplyClick()
        {
            if (Event.current != null && Event.current.button == 1) {
                ApplySelect();
            }
            else {
                Apply();
            }
        }

        public void Apply()
        {
            bool applied = false;
            if (AdvancedPreset.Mode != AdvancedPreset.Modes.Instantiate) {
                // If there are selected channels, apply to them directly
                if (Timeflow.Active != null && Timeflow.Active.Display != null && Timeflow.Active.Display.SelectedChannels != null &&
                    Timeflow.Active.Display.SelectedChannels.Count > 0) {
                    var selection = Timeflow.Active.Display.SelectedChannels.ToArray();
                    foreach (var channel in selection) {
                        if (channel == null) {
                            continue;
                        }
                        if (channel.Behavior == null) {
                            Debug.LogWarning($"<color=orange>ComponentPreset '{DisplayName}' cannot be applied. Channel '{channel.Name}' has no Behavior.</color>");
                            continue;
                        }
                        if (Type.GetType(ComponentType).IsAssignableFrom(channel.Behavior.GetType())) {
                            Apply(channel.Behavior);
                            applied = true;
                        }
                    }
                    if (applied) {
                        EditorGUIUtility.ExitGUI();
                        return;
                    }
                }
            }


            if (Selection.activeGameObject == null || AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate) {
                Instantiate();
            }
            else {
                foreach (GameObject obj in Selection.gameObjects) {
                    Apply(obj);
                }
            }
        }

        private string UndoName => "Apply Preset " + DisplayName;

        public void Instantiate(GameObject target = null)
        {
            GameObject obj = new GameObject(DisplayName);
            Undo.RegisterCreatedObjectUndo(obj, UndoName);
            SelectionUtil.Select(obj);

            bool insertParent = Event.current != null && Event.current.alt && Event.current.control;
            if (insertParent && target != null) {
                string undoName = "Insert Parent " + target.name;
                Undo.SetTransformParent(obj.transform, target.transform.parent, undoName);
                obj.transform.localPosition = target.transform.localPosition;
                obj.transform.localRotation = target.transform.localRotation;
                obj.transform.localScale = target.transform.localScale;

                Undo.SetTransformParent(target.transform, obj.transform, undoName);
                ObjectUtil.ResetTransform(obj);
            }

            if (Timeflow.Active != null) {
                obj.transform.SetParent(target == null ? null : target.transform);
                ObjectUtil.ResetTransform(obj);

                Timeflow.Active.Display.AddObjectToDisplay(obj);
            }

            Apply(obj);
        }

        public virtual void Apply(GameObject target)
        {
            if (target == null) {
                Instantiate();
                return;
            }

            Type type = Type.GetType(ComponentType);
            //Debug.Log($"Applying ComponentPreset '{DisplayName}' to GameObject '{target.name}' Mode:{AdvancedPreset.Mode}");
            var component = target.GetComponent(type);
            if (component == null) {
                component = Undo.AddComponent(target, type);
            }
            Apply(component);
        }

        public virtual void Apply(TimeflowChannel target)
        {
            if (target == null) {
                Instantiate();
                return;
            }
            //Debug.Log($"ApplyToChannel:{target.Name} - {target.Behavior?.GetType().Name} - {ComponentType}");
            if (target.Behavior is Component comp) {
                Apply(comp);
            }
            else {
                Instantiate();
            }
        }

        public virtual void Apply(Component target)
        {
            Undo.RegisterCompleteObjectUndo(target, UndoName);
            Type type = Type.GetType(ComponentType);

            //if (AdvancedPreset.Mode == AdvancedPreset.Modes.Replace) {
            //    if (target != null && CanReinitialize) {
            //        GameObject obj = target.gameObject;
            //        Undo.DestroyObjectImmediate(target);
            //        target = Undo.AddComponent(obj, type);
            //    }
            //}

            if (AdvancedPreset.Mode == AdvancedPreset.Modes.Combine) {
                bool isMultipleAllowed = !Attribute.IsDefined(
                    type,
                    typeof(DisallowMultipleComponent),
                    inherit: true
                );
                if (isMultipleAllowed) {
                    target = Undo.AddComponent(target.gameObject, type);
                }
            }
            //else
            //if (AdvancedPreset.Mode == AdvancedPreset.Modes.Replace) {
            //    if (!Type.GetType(ComponentType).IsAssignableFrom(target.GetType())) {
            //        if (target.gameObject.TryGetComponent(type, out var existingComponent)) {
            //            Undo.DestroyObjectImmediate(existingComponent);
            //        }
            //        target = Undo.AddComponent(target.gameObject, type);
            //    }
            //}

            if (target == null) {
                EditorUtility.DisplayDialog("Error", $"Could not find or add component of type '{ComponentType}' to the target GameObject.", "OK");
                return;
            }

            //Debug.Log($"<color=yellow>Applying ComponentPreset '{DisplayName}'</color> to component of type '{target.GetType().Name}'");
            if (!Type.GetType(ComponentType).IsAssignableFrom(target.GetType())) {
                EditorUtility.DisplayDialog("Error", $"Preset type '{ComponentType}' and target component type '{target.GetType()}' are not compatible.", "OK");
                return;
            }

            Undo.RecordObject(target, UndoName);

            ApplyProperties(target);

            IBehaviorPresets[] objects = target.GetComponents<IBehaviorPresets>();
            foreach (IBehaviorPresets p in objects) {
                //Debug.Log($"<color=green>OnPresetApplied '{p.GetType().Name}'</color>");
                p.OnPresetApplied(null, this);
            }

            if (AdvancedPresetsGlobalConfig.CanRenameObjects) {
                if (AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate) {
                    target.name = DisplayName;
                }
            }
            // Set the color of the object if it has a TimeflowObject component
            if (target.TryGetComponent<TimeflowObject>(out var timeflowObject)) {
                bool anyChannels = false;
                if (timeflowObject.AllChannels != null && timeflowObject.AllChannels.Count > 0) {
                    foreach (var channel in timeflowObject.AllChannels) {
                        if (channel == null || channel.IsTrack) continue;
                        if (channel.IsSelected && target == channel.Behavior) {
                            anyChannels = true;
                            if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                                channel.GUIColor = GUIColor;
                            }
                            if (AdvancedPresetsGlobalConfig.CanRenameObjects) {
                                if (AdvancedPreset.Mode != AdvancedPreset.Modes.Instantiate) {
                                    channel.Name = DisplayName;
                                }
                            }
                        }
                    }
                }
                if (!anyChannels && AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                    if (timeflowObject.IsSelected) timeflowObject.GUIColor = GUIColor;
                }
            }

            ComponentPresetWindow.OnPresetApplied(this);
        }

        private void ApplyProperties(Component target)
        {
            var so = new SerializedObject(target);
            so.Update();
            foreach (var data in Properties) {
                var prop = so.FindProperty(data.PropertyPath);
                if (prop == null) continue;
                switch (data.PropertyType) {
                    case ComponentPresetProperty.PropertyTypes.Boolean:
                        prop.boolValue = data.BoolValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Integer:
                        prop.intValue = data.IntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Float:
                        prop.floatValue = data.FloatValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.String:
                        prop.stringValue = data.StringValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Color:
                        prop.colorValue = data.ColorValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.LayerMask:
                        prop.intValue = data.LayerMaskValue.value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Enum:
                        // Enum value is carried in IntValue
                        prop.enumValueIndex = data.IntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector2:
                        prop.vector2Value = data.Vector2Value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector3:
                        prop.vector3Value = data.Vector3Value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector4:
                        prop.vector4Value = data.Vector4Value;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Quaternion:
                        prop.quaternionValue = data.QuaternionValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.ObjectReference:
                        prop.objectReferenceValue = data.ObjectReference;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Rect:
                        prop.rectValue = data.RectValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.RectInt:
                        prop.rectIntValue = data.RectIntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector2Int:
                        prop.vector2IntValue = data.Vector2IntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Vector3Int:
                        prop.vector3IntValue = data.Vector3IntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.Bounds:
                        prop.boundsValue = data.BoundsValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.BoundsInt:
                        prop.boundsIntValue = data.BoundsIntValue;
                        break;
                    case ComponentPresetProperty.PropertyTypes.AnimationCurve:
                        prop.animationCurveValue = data.AnimationCurveValue;
                        break;
                    default:
                        Debug.LogWarning($"Unhandled property type: {data.PropertyType}");
                        break;
                }
            }
            so.ApplyModifiedProperties();
        }

    }

}//AxonGenesis

#endif
