// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{

    public partial class ComponentPreset : ScriptableObject
    {

        [SerializeField, FormerlySerializedAs("Color")]
        private Color _Color = Color.white;

        [SerializeField, FormerlySerializedAs("_Name")] private string _DisplayName;
        [SerializeField] private string _Label;

        public string DisplayName {
            get {
                OnFirstSetup();
                if (string.IsNullOrEmpty(_DisplayName)) {
                    _DisplayName = name;
                }
                return _DisplayName;
            }
            set {
                _DisplayName = value;
            }
        }

        public string Label {
            get {
                if (string.IsNullOrEmpty(_Label)) {
                    _Label = name;
                }
                return _Label;
            }
            set {
                _Label = value;
            }
        }

        public Color Color {
            get {
                return _Color;
            }
            set {
                _Color = value;
            }
        }

        public Color GUIColor {
            get {
                if (!AdvancedPresetsGlobalConfig.ShowColoredButtons) {
                    return Color.white;
                }
                else
                if (AdvancedPresetsGlobalConfig.ButtonSaturation < 1f) {
                    return Color.Lerp(Color.white, _Color, AdvancedPresetsGlobalConfig.ButtonSaturation); // Adjust color for visibility
                }
                return _Color;
            }
        }

        public virtual void GUI()
        {
            // Override to provide custom GUI for the preset
        }

    }

}//AxonGenesis

#endif