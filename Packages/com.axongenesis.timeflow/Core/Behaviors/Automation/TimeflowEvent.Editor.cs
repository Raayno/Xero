// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
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
    /// <summary>
    /// Implements custom Timeflow GUI and menu options.
    /// </summary>
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/event")]
    public partial class TimeflowEvent : TimeflowBehavior
    {
        public bool ShowLabel = true;
        public GUIRect GUIRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public float TriggerTimeTemp;

        #region TIMEFLOW GUI

        public void GetTempValues()
        {
            TriggerTimeTemp = _TriggerTime;
        }

        public override void OnDragStart()
        {
            _dragTime = TriggerTime;
        }

        public override void OnDragCancel()
        {
            base.OnDragCancel();
            TriggerTime = _dragTime;
        }

        public float SetDragTime(float offset, bool canSnap) { return SetDragTime(offset, canSnap, Vector2.zero, false); }

        public float SetDragTime(float offset, bool canSnap, Vector2 timeLimits) { return SetDragTime(offset, canSnap, timeLimits, true); }

        public float SetDragTime(float offset, bool canSnap, Vector2 timeLimits, bool useLimits)
        {
            float t = _dragTime + offset;
            if (canSnap && Timeflow != null) {
                t = Timeflow.Active.View.SnapTime(t);
            }
            if (!Timeflow.Active.Input.IsDraggingCopy) {
                // Calculate as preview only
                TriggerTime = t;
            }
            return t - _dragTime;
        }

        public override void InsertTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (_TriggerTime > start) {
                _TriggerTime += end - start;
            }
        }

        public override void DuplicateTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (_TriggerTime > start) {
                _TriggerTime += end - start;
            }
        }

        public override void DeleteTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (_TriggerTime >= start && _TriggerTime <= end) {
                Destroy(this);
            }
            else
            if (_TriggerTime > end) {
                _TriggerTime -= end - start;
            }
        }

        public override void ClearTime(float start, float end, bool isLocalTime, bool isGlobal, TimeflowView.SelectionModes mode)
        {
        }

        private SerializedObject serializedObject;
        public virtual void OnEditorGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Show Label";
            ShowLabel = AxonGUI.FieldToggle(this, "ShowLabel ", ShowLabel);
            AxonGUI.EndHorizontal(false);

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Function";
            Function = AxonGUI.FieldText(this, "Function ", Function);
            AxonGUI.EndHorizontal(false);

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Parameter";
            Parameter = AxonGUI.FieldText(this, "Param ", Parameter);
            AxonGUI.EndHorizontal(false);

            if(serializedObject == null) serializedObject = new SerializedObject(this);
            serializedObject.FindProperty("OnTrigger");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTrigger"), new GUIContent("On Trigger"), true);
        }

        public override void OnBeforeSavePreset(ref List<ComponentPresetListItem> items)
        {
            base.OnBeforeSavePreset(ref items);
            if (items == null || items.Count == 0) return;

            List<ComponentPresetListItem> toremove = new List<ComponentPresetListItem>();
            foreach (ComponentPresetListItem item in items) {
                if (item.Name == "Obj" || item.Name == "Was Triggered") {
                    toremove.Add(item);
                }
            }

            if (toremove.Count > 0) {
                foreach (ComponentPresetListItem item in toremove) {
                    items.Remove(item);
                }
            }
        }

        #endregion

        #region UI

        public virtual void GUIKeyframes()
        {
            if (Timeflow.Active != null) {
                GUIStyle style = null;

                if (Timeflow.Active.View.SelectedEvents != null && Timeflow.Active.View.SelectedEvents.Contains(this)) {
                    if (Enabled) {
                        style = AxonUI.EventSelectedStyle;
                    }
                    else {
                        style = AxonUI.EventDisabledSelStyle;
                    }
                }
                else {
                    if (Enabled) {
                        style = AxonUI.EventStyle;
                    }
                    else {
                        style = AxonUI.EventDisabledStyle;
                    }
                }

                GUI.color = Color.white;
                GUIContent content = new GUIContent("");

                if (ShowLabel) {

                    GUIContent eventName = new GUIContent(Name);
                    Vector2 size = GUI.skin.label.CalcSize(eventName);

                    GUI.Box(new Rect(GUIRect.x + 16f, GUIRect.y, size.x, 16), eventName, AxonUI.EventLabelStyle);
                }
                else {
                    content.tooltip = Name;
                }

                if (TimeflowView.UseRelatedKeys && Timeflow.Active.View.RelatedEvents != null && Timeflow.Active.View.RelatedEvents.Contains(this)) {
                    GUI.color = AxonColor.RelatedKeys;
                }
                else {
                    GUI.color = Color.white;
                }

                GUI.Box(GUIRect, content, style);
                GUI.color = AxonColor.Default;
            }
        }

        #endregion

        #region MENU

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (inHierarchy) {
                var eventTypes = ReflectionUtil.GetTypes<TimeflowEvent>().ToArray();

                foreach (System.Type type in eventTypes) {
                    TimeflowContextMenuEventType info = new TimeflowContextMenuEventType();
                    info.EventType = type;
                    info.InHierarchy = inHierarchy;

                    string name = StringUtil.ClassName("" + type);
                    name = name.Replace("Event", " Event"); // to format a little nicer  
                    int a = name.IndexOf(".");
                    if (a != -1) {
                        name = name.Substring(a + 1);
                    }
                    TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Event/Add " + name), false, GUIMenu_AddEvent, info);
                }
            }
            else
            if (TimeflowContext.HasTracks) {
                try {
                    var eventTypes = ReflectionUtil.GetTypes<TimeflowEvent>().ToArray();

                    foreach (System.Type type in eventTypes) {
                        TimeflowContextMenuEventType info = new TimeflowContextMenuEventType();
                        info.EventType = type;
                        info.InHierarchy = inHierarchy;

                        string name = StringUtil.ClassName("" + type);
                        name = name.Replace("Event", " Event"); // to format a little nicer
                        int a = name.IndexOf(".");
                        if (a != -1) {
                            name = name.Substring(a + 1);
                        }
                        TimeflowContext.Menu.AddItem(new GUIContent("Events/Add " + name), false, GUIMenu_AddEvent, info);
                    }
                }
                catch (Exception ex) {
                    Debug.LogError("An error occurred while adding menu items: " + ex.Message);
                }
            }
        }

        public static void GUIMenu_AddEvent(object obj)
        {

            TimeflowContextMenuEventType info = (TimeflowContextMenuEventType)obj;
            if (info != null) {
                List<TimeflowObject> objects = TimeflowContext.GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject tobj in objects) {
                        TimeflowEvent evt = Undo.AddComponent(tobj.gameObject, info.EventType) as TimeflowEvent;
                        evt.TriggerTimeWorld = Timeflow.Active.CurrentTime;
                        tobj.GetEvents();
                        tobj.Enabled = true;
                        Timeflow.Active.View.SelectEvent(evt, true);
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

        #endregion
    }

}//AxonGenesis

#endif