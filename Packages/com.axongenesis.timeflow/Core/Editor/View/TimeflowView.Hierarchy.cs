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
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region CONSTANTS

        private const int _switchesLeftPad = 2;
        private const int _switchesTopPad = 4;
        private const int _columnLabelsTopPad = -6;
        private const int _valuesColumnToggleTopPad = 6;
        private const int _offsetColumnToggleLeftPad = 8;
        public const int IndentIncrement = 10;

        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private int _objectOrderInView;

        [NonSerialized]
        private int _channelOrderInView;

        [NonSerialized]
        private int _indentLevel;

        [NonSerialized]
        private Rect _switchesToggleRect;

        [NonSerialized]
        private Rect _valuesColumnToggleRect;

        [NonSerialized]
        private Rect _offsetsColumnToggleRect;

        [NonSerialized]
        private Rect _valuesColumnLabelRect;

        [NonSerialized]
        private Rect _offsetsColumnLabelRect;

        #endregion

        #region GUI

        public void GUIHierarchy()
        {
            if (Layout.ShowSwitches) {
                GUI.color = AxonColor.VScrollbar;
                Rect r = new Rect(0, Layout.Hierarchy.Top, Layout.Switches.Width, Layout.Hierarchy.Height);
                GUI.Box(r, GUIContent.none, AxonUI.SolidStyle);
                GUI.color = Color.white;
            }

            GUIHierarchyObjects();
            GUIHierarchyTools();
            GUIColumns();
            Display.GUIAddBar();
        }

        private void GUIColumns()
        {
            Layout.GUIColumns();
            if (Layout.ShowValues) {
                GUIChannelValues();
                if (Layout.ShowTimeOffset) {
                    GUITimeOffsetColumn();
                }
            }
        }

        private void GUIHierarchyTools()
        {
            GUI.color = AxonColor.Default;
            GUI.Box(Layout.HierarchyTools, "", AxonUI.HierarchyToolsStyle);
            GUIBeginGroup(Layout.HierarchyToolsInner);

            GUISwitchesPanel();
            Display.GUIMenu();
            GUIValuesColumnHeading();
            GUIOffsetsColumnHeading();

            GUIEndGroup();
        }

        private void GUISwitchesPanel()
        {
            if (IsLayout) {
                _switchesToggleRect = new GUIRect(_switchesLeftPad, _switchesTopPad, TimeflowViewLayout.SmallIconSize, TimeflowViewLayout.SmallIconSize);
                if (Layout.ShowSwitches) {
                    _switchesToggleRect.x = Layout.Switches.Width;
                }
                Layout.SwitchesAndFoldout.Height = Layout.Hierarchy.Height;
                Display.GUISwitches();
            }
            else {
                if (Layout.ShowSwitches) {
                    Display.GUISwitches();
                }

                GUIStyle style = Layout.ShowSwitches ? AxonUI.ColumnExpandOnStyle : AxonUI.ColumnExpandOffStyle;
                if (GUI.Button(_switchesToggleRect, AxonUI.ColumnExpandLabel, style)) {
                    Layout.ShowSwitches = !Layout.ShowSwitches;
                }
                if (Display.IsFiltered && !Layout.ShowSwitches) {
                    IndicateSwitchFilterIsOn(_switchesToggleRect);
                }
            }
        }

        private void GUIValuesColumnHeading()
        {
            if (IsLayout) {
                _valuesColumnToggleRect = Layout.HierarchyTools;
                _valuesColumnToggleRect.y = _switchesToggleRect.y;
                _valuesColumnToggleRect.x = Layout.SeparatorH1.Left - TimeflowViewLayout.SmallIconSize;
                _valuesColumnToggleRect.width = _valuesColumnToggleRect.height = TimeflowViewLayout.SmallIconSize;

                _valuesColumnLabelRect.y = _columnLabelsTopPad;
                _valuesColumnLabelRect.x = Layout.SeparatorH1.Left + _offsetColumnToggleLeftPad;
                _valuesColumnLabelRect.width = Layout.Values.Width;
                _valuesColumnLabelRect.height = Layout.Toolbar.Height;
            }
            else {
                GUIStyle style = Layout.ShowValues ? AxonUI.ColumnExpandRightOnStyle : AxonUI.ColumnExpandRightOffStyle;
                if (GUI.Button(_valuesColumnToggleRect, AxonUI.ColumnExpandRightLabel, style)) {
                    Layout.ShowValues = !Layout.ShowValues;
                }
                if (Layout.ShowValues) {
                    GUI.color = Color.gray;
                    EditorGUI.LabelField(_valuesColumnLabelRect, new GUIContent("Values"));
                    GUI.color = AxonColor.Default;
                }
            }
        }

        private void GUIOffsetsColumnHeading()
        {
            if (Layout.ShowValues) {
                if (IsLayout) {
                    _offsetsColumnToggleRect = _valuesColumnToggleRect;
                    _offsetsColumnToggleRect.x = Layout.SeparatorH2.Left - TimeflowViewLayout.SmallIconSize;
                    if (Layout.ShowTimeOffset) {
                        _offsetsColumnLabelRect = _valuesColumnLabelRect;
                        _offsetsColumnLabelRect.x = Layout.SeparatorH2.Left + _offsetColumnToggleLeftPad;
                        _offsetsColumnLabelRect.width = Layout.TimeOffset.Width;
                        _offsetsColumnLabelRect.height = Layout.Toolbar.Height;
                    }
                }
                else {
                    GUIStyle style = Layout.ShowTimeOffset ? AxonUI.ColumnExpandRightOnStyle : AxonUI.ColumnExpandRightOffStyle;
                    if (GUI.Button(_offsetsColumnToggleRect, AxonUI.ColumnExpandRightLabel, style)) {
                        Layout.ShowTimeOffset = !Layout.ShowTimeOffset;
                    }
                    if (Layout.ShowTimeOffset) {
                        GUI.color = Color.gray;
                        EditorGUI.LabelField(_offsetsColumnLabelRect, new GUIContent("Time Offset & Scale"));
                        GUI.color = AxonColor.Default;
                    }
                }
            }
        }

        private void GUIDragOverlay()
        {
            if (Input.IsDragging) {
                GUI.color = AxonColor.DragOver;

                // Draw the item being dragged. Note that item dragging is disabled with shift key to favor object selection instead
                if (Input.EventMode == TimeflowViewInput.EventModes.DragObjectOrder && Input.DragObject != null && !IsShift) {
                    Input.DragObjectOrderOverlay();
                }
                else
                if (Input.EventMode == TimeflowViewInput.EventModes.DragChannelOrder && Input.DragChannel != null && !IsShift) {
                    Input.DragChannelOrderOverlay();
                }
            }
        }

        private void GUIHierarchyObjectsLayout()
        {
            float scrollY = ScrollOffset.y;
            _indentLevel = 0;
            ScrollMax.y = 0f;
            Layout.TotalHeight = _totalHeight;
            Layout.TotalHierarchyWidth = _totalHierarchyWidth + Layout.SeparatorH1.Left + 100;

            ScrollMin.y = Layout.Hierarchy.Height - _totalHeight - 20f;

            // Reset for next draw
            _totalHeight = 0;
            _totalHierarchyWidth = 0;

            _rowOffset = (int)scrollY;
            Display.AnyObjectsHidden = false;

            // Restart the object view count from top to bottom
            _objectOrderInView = 0;
            _channelOrderInView = 0;
        }

        private void GUIHierarchyObjects()
        {
            bool isLayout = IsLayout;
            if (isLayout) GUIHierarchyObjectsLayout();

            GUIBeginGroup(Layout.Hierarchy);

            // Only recreate array when needed
            if (Timeflow.RootObjectsCached != null && Timeflow.RootObjectsCached.Length > 0) {
                foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                    if (obj == null) continue;
                    GUIHierarchyObjectsRecursive(obj);
                }
            }
            GUIEndGroup();
            GUIDragOverlay();
        }

        private void GUIHierarchyObjectsRecursive(TimeflowObject obj)
        {
            if (obj == null) return; // prevents error when undoing
            bool isLayout = IsLayout;

            if (isLayout) {
                obj.GUIRect = new GUIRect(0, _rowOffset, Layout.Hierarchy.Width, obj.Track.GUIHeight);
                obj.GUISelectRect = obj.GUIRect;
                if (Layout.ShowSwitches) {
                    obj.GUISelectRect.x = Timeflow.Layout.Switches.Width;
                    obj.GUISelectRect.width -= obj.GUISelectRect.x;
                }
            }

            if (obj.IsDisplayed) {
                obj.SortOrderInView = _objectOrderInView++;
                obj.GUIHierarchy();
                if (Layout.ShowSwitches) {
                    obj.GUIHierarchySwitches();
                }

                if (isLayout) {
                    _totalHeight += obj.GUIRect.height;
                    _rowOffset += obj.GUIRect.height;

                    if (_totalHierarchyWidth < _indentLevel) {
                        _totalHierarchyWidth = _indentLevel;
                    }
                    obj.GUICull = Layout.RelativeGroupRect.Height > 0 && !Layout.RelativeGroupRect.Overlaps(obj.GUIRect);

                    if (!obj.IsCollapsed) {
                        GUIHierarchyObjectChannels(obj);
                    }
                    obj.Track.GUIExpandRegionLayout();
                }

                if (!obj.IsCollapsed) {
                    if (obj.AllChannelsForDisplay != null && obj.AllChannelsForDisplay.Count > 0) {
                        _indentLevel++;

                        foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                            if (ch.IsHidden) continue;
                            if (!ch.IsTrack && !obj.GUICull) {
                                ch.GUIHierarchy();
                                if (Layout.ShowSwitches) ch.GUIHierarchySwitches();
                            }
                        }
                        _indentLevel--;
                    }
                }
            }
            if (!obj.IsCollapsed || Display.EnabledOnly) {
                if (obj.ShowChildren && obj.Children != null && obj.Children.Count > 0) {
                    _indentLevel++;
                    foreach (TimeflowObject child in obj.Children) {
                        GUIHierarchyObjectsRecursive(child);
                    }
                    _indentLevel--;
                }
            }
            if (obj.IsDisplayed) {
                obj.Track.GUIExpandRegion();
            }
        }

        private void GUIHierarchyObjectChannels(TimeflowObject obj)
        {
            if (obj.AllChannelsForDisplay != null && obj.AllChannelsForDisplay.Count > 0) {
                foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                    if (ch.IsHidden) continue;
                    if (!ch.IsTrack) {
                        ch.SortOrderInView = _channelOrderInView++;
                        ch.GUIRect = new GUIRect(0, _rowOffset, Layout.Hierarchy.Width, ch.GUIHeight);
                        ch.GUISelectRect = new GUIRect(obj.GUISelectRect.x, ch.GUIRect.y, Timeflow.Layout.Hierarchy.Width - ch.GUIControlsRect.width, ch.GUIRect.height);

                        ch.GUILinkRect = new GUIRect(Layout.Hierarchy.Width - 16f, _rowOffset, 16f, ch.GUIHeight);
                        ch.GUICull = !Layout.RelativeGroupRect.Overlaps(ch.GUIRect);
                        if (!ch.GUICull) {
                            obj.GUICull = false;
                        }
                        ch.GUIHierarchy();
                        if (Layout.ShowSwitches) ch.GUIHierarchySwitches();
                        _totalHeight += ch.GUIRect.height;
                        _rowOffset += ch.GUIRect.height;
                    }
                }
            }
        }

        #endregion

        #region HIERARCHY TOOLS	

        /// <summary>
        /// Returns the current indent level while the GUI is drawing. The indent is an x offset value in
        /// pixels, measured by counting the depth of recursion while drawing the hierarchy.
        /// </summary>
        public int GetIndent(TimeflowObject obj = null)
        {
            float size = Layout.ObjectScrollbar.Width - Layout.ObjectScrollbarHandle.Width;
            int inc = IndentIncrement;
            if (size > 0) {
                inc = (int)Mathf.Max(0, IndentIncrement - HierarchyScrollOffset);
            }
            if (obj == null) {
                // Return the current indentLevel, which is sensitive to the time in which this is called
                return _indentLevel * inc;
            }
            else {
                // Calculate the indent level based on the object's hierarchy position
                int indent = 0;
                Transform p = obj.transform.parent;
                while (p != null) {
                    if (Display.IsObjectDisplayed(p.gameObject)) {
                        indent += inc;
                        p = p.parent;
                    }
                    else {
                        break;
                    }
                }
                return indent;
            }
        }

        public void CollapseRecursive(TimeflowObject obj, bool collapse, bool recursive)
        {
            bool showChildren = false;
            if (IsAlt) recursive = true;
            if (IsControl) {
                showChildren = !obj._ShowChildren;
                collapse = obj.IsCollapsed; // Keep current state
                // This will hide children but keep channels visible, which is usually the desired behavior
            }
            //Debug.Log($"CollapseRecursive {obj.name} collapse:{collapse} recursive:{recursive} showChildren:{showChildren}");
            _CollapseRecursive(obj, collapse, recursive, showChildren);
            if (IsControl) obj.OnShowChildren();
        }

        private void _CollapseRecursive(TimeflowObject obj, bool collapse, bool recursive, bool showChildren)
        {
            if (IsControl) {
                obj._ShowChildren = showChildren;
            }

            obj.IsCollapsed = collapse;

            List<TimeflowObject> children = ObjectUtil.GetComponentsInChildren<TimeflowObject>(obj.gameObject);
            if (children != null && children.Count > 0) {
                foreach (TimeflowObject child in children) {
                    ParentCollapsedRecursive(child, collapse);
                    if (recursive) _CollapseRecursive(child, collapse, true, showChildren);
                }
            }
        }

        public void ParentCollapsedRecursive(TimeflowObject obj, bool collapsed)
        {
            obj.IsParentCollapsed = collapsed;
            List<TimeflowObject> children = ObjectUtil.GetComponentsInChildren<TimeflowObject>(obj.gameObject);
            if (children != null && children.Count > 0) {
                foreach (TimeflowObject child in children) {
                    ParentCollapsedRecursive(child, collapsed);
                }
            }
        }

        public void CollapseSelected(bool collapse = true, bool recursive = false)
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelected && obj.IsDisplayed) {
                        CollapseRecursive(obj, collapse, recursive);
                    }
                }
                if (IsControl) {
                    Refresh(true);
                }
                Display.ApplyFilter();
            }
        }

        public void HideChildrenOfSelected(bool hide = true)
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelected && obj.IsDisplayed) {
                        UndoUtil.Undo(obj, (hide ? "Hide" : "Show") + " Children");
                        obj.ShowChildren = !hide;
                    }
                }
                NeedsRefresh = true;
                Display.ApplyFilter();
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
