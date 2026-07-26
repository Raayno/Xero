// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

namespace AxonGenesis
{

    [Serializable]
    public class AdvancedPresetsContainer
    {
        public Color Color = Color.white;

        [SerializeField, FormerlySerializedAs("Icon")] 
        protected Texture2D _Icon = null;

        public int Index;

        public Action<string> OnNameChanged;

        public float NameWarningTimeout = 0f;
        public string NameWarning = null;

        public bool ShowSettings = true;
        public bool IsExpanded = true;

        public Color GUIColor {
            get {
                if (!AdvancedPresetsGlobalConfig.ShowColoredHeadings) {
                    return Color.white;
                }
                else
                if (AdvancedPresetsGlobalConfig.HeadingSaturation < 1f) {
                    return Color.Lerp(Color.white, Color, AdvancedPresetsGlobalConfig.HeadingSaturation); // Adjust color for visibility
                }
                return Color;
            }
        }

        public virtual Texture2D Icon {
            get {
                if (_Icon == null) {
                    _Icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
                }
                return _Icon;
            }
            set {
                _Icon = value;
            }
        }

        [SerializeField] protected string _Name = null;

        [SerializeField] protected AdvancedPresetsLayout _Layout = null;

        public AdvancedPresetsLayout Layout {
            get {
                if (_Layout == null) {
                    _Layout = new AdvancedPresetsLayout();
                }
                return _Layout;
            }
            set {
                _Layout = value;
            }
        }
    }
}

#endif