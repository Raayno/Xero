#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

namespace AxonGenesis
{
    public class ComponentPresetEditBase<T> where T : ComponentPreset
    {
        public static bool IsEditing = false;

        public T target;
        public Editor editor = null;

        //private ComponentPreset target = null;

        private SerializedObject _SerializedObject = null;
        private SerializedObject SerializedObject {
            get {
                if (_SerializedObject == null && target != null) {
                    _SerializedObject = new SerializedObject(target);
                }
                return _SerializedObject;
            }
        }
        private SerializedProperty Properties = null;
        private Texture2D _Icon;
        private bool iconChecked = false;
        private bool hasNameOrTypeChanged = false;
        private bool nameIsValidated = false;
        private bool typeIsValidated = false;

        public ComponentPresetEditBase() { }

        //public ComponentPresetEditBase(ComponentPreset preset, SerializedObject serializedObject)
        //{
        //    _Preset = preset;
        //    _SerializedObject = serializedObject;
        //}
        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        /// <summary>
        /// Sets the target component being edited in the inspector. Must be a component type derrived from
        /// AxonGenesisBehavior.
        /// </summary>
        public virtual void SetTarget<T1>(T1 targ, Editor edit)
        {
            target = targ as T;
            editor = edit;
        }

        public virtual void SetTarget(AxonGenesisBehavior targ)
        {
            target = targ as T;
        }

        public virtual bool HasTarget()
        {
            return target != null;
        }

        public virtual Type GetTargetType()
        {
            return target != null ? target.GetType() : typeof(AxonGenesisBehavior);
        }

        private bool IsValidType(bool isValid, string typeName)
        {
            if (isValid) {
                GUI.color = Color.green;
                AxonGUI.ButtonInline("Valid");
                GUI.color = Color.white;
            }
            else {
                Type type = Type.GetType(typeName);
                isValid = type != null;

                GUI.color = Color.yellow;
                AxonGUI.ButtonInline("Invalid");
                GUI.color = Color.white;
            }
            return isValid;
        }

        public virtual void MainGUI()
        {
            AxonGUI.Setup(120);

            GUI_Heading();
            GUI_Info();
            GUI_Custom();
            GUI_Properties();

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
                SerializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void GUI_Info()
        {
            if (IsEditing) {
                AxonGUI.BeginHorizontal();
                string compType = EditorGUILayout.TextField("Component Type", target.ComponentType);
                if (target.ComponentType != compType) {
                    target.ComponentType = compType;
                    typeIsValidated = false;
                    hasNameOrTypeChanged = true;
                }
                if (hasNameOrTypeChanged) {
                    typeIsValidated = IsValidType(typeIsValidated, target.ComponentType);
                    if (typeIsValidated && !nameIsValidated) {
                        Type type = Type.GetType(target.ComponentType);
                        target.ComponentName = type.Name;
                    }
                }
                AxonGUI.EndHorizontal();

                if (hasNameOrTypeChanged) {
                    AxonGUI.HelpBox("Please note that directly editing the component name and type affects which components the preset can be applied to. If entered incorrectly, it will not appear in the presets menu.", MessageType.Warning, true);
                }
            }
            else {
                AxonGUI.Label(target.ComponentType, AxonUI.SmallLabelStyle);
            }

            //AxonGUI.SetTooltip("If enabled, applying this preset will destroy and recreate the target component. Use this if the preset needs to start with a clean slate, otherwise leave this" +
            //    " option off to preserve existing component settings that are otherwise unaffected by the preset.");
            //target.CanReinitialize = AxonGUI.FieldToggle(null, "Reinitialize", target.CanReinitialize);

            // Display and edit the Properties list
            AxonGUI.Space();
        }

        protected virtual void GUI_Properties()
        {
            AxonGUI.BeginBoxPadded();
            if (target.Properties != null) {
                if (Properties == null) Properties = SerializedObject.FindProperty("Properties");

                EditorGUILayout.PropertyField(Properties);
            }
            AxonGUI.EndBoxPadded();
        }

        protected virtual void GUI_Heading()
        {
            GUI.color = target.GUIColor;
            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleDarkBig, GUILayout.Height(25));
            GUI.color = Color.white;

            if (!iconChecked) {
                iconChecked = true;
                var componentType = target.GetComponentType();
                if (componentType != null) {
                    _Icon = EditorGUIUtility.ObjectContent(null, componentType).image as Texture2D;
                }
            }
            if (_Icon == null) {
                _Icon = AxonUI.Icons.Presets;
            }
            AxonGUI.ButtonIcon(_Icon);
            if (IsEditing) {
                AxonGUI.SetTooltip("Full descriptive name for list displays");
                target.DisplayName = AxonGUI.FieldTextInline(null, "Preset Name", target.DisplayName);

                AxonGUI.SetTooltip("Abbreviated name for button displays");
                target.Label = AxonGUI.FieldTextInline(null, "Label", target.Label, GUILayout.Width(100));
            }
            else {
                AxonGUI.Label(target.ComponentName + " : " + target.DisplayName, EditorStyles.boldLabel);
                AxonGUI.LabelInline(target.Label);
            }

            if (IsEditing) {
                target.Color = AxonGUI.FieldColorInline(null, target.Color, false, GUILayout.Width(50));
            }
            if (AxonGUI.ButtonIcon(IsEditing ? AxonUI.Icons.EditOn : AxonUI.Icons.EditOff, 16, "Edit Preset")) {
                IsEditing = !IsEditing;
            }

            AxonGUI.EndHorizontal();
        }

        protected virtual void GUI_Custom() { }
    }
}

#endif
