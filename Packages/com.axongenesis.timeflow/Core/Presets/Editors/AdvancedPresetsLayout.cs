// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [Serializable]
    public class AdvancedPresetsLayout
    {
        public enum Modes
        {
            Auto,
            Grid,
            List
        }

        public enum Labels
        {
            Auto,
            FullName,
            ShortName
        }

        [SerializeField] private Modes _Mode = Modes.Auto;
        [SerializeField] private Labels _Label = Labels.Auto;

        [SerializeField] private int _ButtonWidth = 100;
        [SerializeField] private int _ButtonHeight = 20;
        [SerializeField] private int _GridTabIconSize = 26;
        [SerializeField] private int _ButtonSpacing = 0;
        [SerializeField] private int _ItemsPerRow = 4;
        [SerializeField] private bool _AutoWidth = false;
        [SerializeField] private bool _AutoItemsPerRow = true;
        [SerializeField] private bool _OverrideLayout = false;

        public bool OverrideMode = false;

        public UnityEngine.Object Object { get; set; }

        [NonSerialized]
        public AdvancedPresetsLayout Parent = null;

        public AdvancedPresetsLayout() { }

        public AdvancedPresetsLayout(AdvancedPresetsLayout other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            _Mode = other._Mode;
            _ButtonWidth = other._ButtonWidth;
            _ButtonHeight = other._ButtonHeight;
            _GridTabIconSize = other._GridTabIconSize;
            _ButtonSpacing = other._ButtonSpacing;
            _ItemsPerRow = other._ItemsPerRow;
            _AutoWidth = other._AutoWidth;
            _AutoItemsPerRow = other._AutoItemsPerRow;
            _OverrideLayout = other._OverrideLayout;
            OverrideMode = other.OverrideMode;
            Parent = other.Parent;
        }

        public Modes Mode {
            get {
                if (!OverrideMode && Parent != null && (!OverrideLayout || _Mode == Modes.Auto)) {
                    return Parent.Mode;
                }
                return _Mode;
            }
            set {
                _Mode = value;
            }
        }

        public Labels Label {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.Label;
                }
                return _Label;
            }
            set {
                _Label = value;
            }
        }

        public bool IsGrid {
            get {
                return Mode == AdvancedPresetsLayout.Modes.Grid;
            }
            set {
                if (value) {
                    Mode = AdvancedPresetsLayout.Modes.Grid;
                }
                else {
                    Mode = AdvancedPresetsLayout.Modes.List;
                }
            }
        }

        public int GridTabIconSize {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.GridTabIconSize;
                }
                return _GridTabIconSize;
            }
            set {
                _GridTabIconSize = value;
            }
        }

        public int ButtonWidth {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.ButtonWidth;
                }
                return _ButtonWidth;
            }
            set {
                _ButtonWidth = value;
            }
        }

        public int ButtonHeight {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.ButtonHeight;
                }
                return _ButtonHeight;
            }
            set {
                _ButtonHeight = value;
            }
        }

        public int ButtonSpacing {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.ButtonSpacing;
                }
                return _ButtonSpacing;
            }
            set {
                _ButtonSpacing = value;
            }
        }

        public int ItemsPerRow {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.ItemsPerRow;
                }
                return _ItemsPerRow;
            }
            set {
                _ItemsPerRow = value;
            }
        }

        public bool AutoWidth {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.AutoWidth;
                }
                return _AutoWidth;
            }
            set {
                _AutoWidth = value;
            }
        }

        public bool AutoItemsPerRow {
            get {
                if (Parent != null && !OverrideLayout) {
                    return Parent.AutoItemsPerRow;
                }
                return _AutoItemsPerRow;
            }
            set {
                _AutoItemsPerRow = value;
            }
        }

        public bool OverrideLayout {
            get {
                if (Parent != null && !_OverrideLayout) {
                    return Parent.OverrideLayout;
                }
                return _OverrideLayout;
            }
            set {
                _OverrideLayout = value;
            }
        }

        public void GUI(bool enableOverride = false)
        {
            AxonGUI.BeginBox();
            if (enableOverride || OverrideMode) {
                AxonGUI.BeginHorizontal();
                _OverrideLayout = AxonGUI.FieldToggleInline(Object, "Override Layout", _OverrideLayout);
                AdvancedPresetsWindow.MinifiedRowBreak();
                if (OverrideLayout || OverrideMode) {
                    _Mode = (AdvancedPresetsLayout.Modes)AxonGUI.FieldEnumPopupInline(Object, _Mode);
                }
                AxonGUI.EndHorizontal();
            }
            else {
                OverrideLayout = false;
            }
            
            if (!enableOverride || OverrideLayout) {
                if (!enableOverride) {
                    Mode = (AdvancedPresetsLayout.Modes)AxonGUI.FieldEnumPopup(Object, "Layout", Mode);
                }

                AxonGUI.BeginDisabledGroup(enableOverride && !OverrideLayout);
                AxonGUI.BeginHorizontal();
                if (AutoWidth) {
                    AxonGUI.Label("Width", "", GUILayout.Width(70));
                }
                else {
                    ButtonWidth = AxonGUI.FieldInt(Object, "Width", ButtonWidth);
                    if (ButtonWidth < 10) {
                        ButtonWidth = 10;
                    }
                }
                AutoWidth = AxonGUI.FieldToggleInline(Object, "Auto", AutoWidth);

                AdvancedPresetsWindow.MinifiedRowBreak();
                ButtonHeight = AxonGUI.FieldIntInline(Object, "Height", ButtonHeight, GUILayout.Width(100));
                if (ButtonHeight < 10) {
                    ButtonHeight = 10;
                }

                AdvancedPresetsWindow.MinifiedRowBreak();
                ButtonSpacing = AxonGUI.FieldIntInline(Object, "Spacing", ButtonSpacing, GUILayout.Width(100));
                if (ButtonSpacing < 0) {
                    ButtonSpacing = 0;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                if (AutoItemsPerRow) {
                    AxonGUI.Label("Per Row", "", GUILayout.Width(70));
                }
                else {
                    AxonGUI.BeginDisabledGroup(AutoItemsPerRow);
                    ItemsPerRow = AxonGUI.FieldInt(Object, "Per Row", ItemsPerRow);
                    if (ItemsPerRow < 1) {
                        ItemsPerRow = 1;
                    }
                    AxonGUI.EndDisabledGroup();
                }

                AutoItemsPerRow = AxonGUI.FieldToggleInline(Object, "Auto", AutoItemsPerRow);
                _Label = (AdvancedPresetsLayout.Labels)AxonGUI.FieldEnumPopupInline(Object, "Label", _Label);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.SetTooltip("Determine the size of the icon button tabs when displaying in grid layout.");
                GridTabIconSize = AxonGUI.FieldIntInline(Object, "Grid Button Tab Size", GridTabIconSize);
                AxonGUI.EndHorizontal();


                AxonGUI.EndDisabledGroup();
            }
            
            AxonGUI.EndBox();
        }
    }
}

#endif