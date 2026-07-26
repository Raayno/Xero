// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        public TimeflowEvent EventHit()
        {
            TimeflowEvent eventHit = null;

            if (Layout.TimeAreaInner.HitTest(MousePosition)) {
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);

                // First check to see if the user clicked a key that's already selected
                if (SelectedEvents != null) {
                    foreach (TimeflowEvent k in SelectedEvents) {
                        if (k.GUIRect.Contains(p)) {
                            eventHit = k;
                            break;
                        }
                    }
                }

                if (!eventHit) {
                    if (Display.Objects != null) {
                        foreach (TimeflowObject obj in Display.Objects) {
                            if (obj.Events != null && obj.Events.Count > 0 && !obj.IsLocked && obj.IsSelectable) {
                                foreach (TimeflowEvent k in obj.Events) {
                                    if (k.GUIRect.Contains(p)) {
                                        eventHit = k;
                                        break;
                                    }
                                }
                                if (eventHit != null) break;
                            }
                        }
                    }
                }
            }
            return eventHit;
        }

        public bool EventsSelected()
        {
            bool eventHit = false;
            bool selectionChanged = false;

            if (Layout.TimeAreaInner.HitTest(MousePosition)) {
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);

                // First check to see if the user clicked a key that's already selected
                if (SelectedEvents != null) {
                    foreach (TimeflowEvent k in SelectedEvents) {
                        if (k.GUIRect.Contains(p)) {
                            eventHit = true;
                            if (IsShift) {
                                k.IsSelected = false;
                                SelectedEvents.Remove(k);
                                selectionChanged = true;
                            }
                            break;
                        }
                    }
                }

                if (!eventHit) {
                    if (Display.Objects != null) {
                        foreach (TimeflowObject item in Display.Objects) {
                            if (item.Events != null && item.Events.Count > 0 && !item.IsLocked && item.IsSelectable) {
                                foreach (TimeflowEvent k in item.Events) {
                                    if (k.GUIRect.Contains(p)) {
                                        eventHit = true;
                                        if (!IsShift) {
                                            DeselectKeys();
                                            SelectionUtil.Select(k.gameObject);
                                        }
                                        k.IsSelected = true;
                                        if(SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
                                        SelectedEvents.Add(k);
                                        selectionChanged = true;
                                        break;
                                    }
                                }
                                if (eventHit) break;
                            }
                        }
                    }
                }
            }
            if (eventHit && !IsShift) {
                SelectedKeys = null;
            }

            if (selectionChanged) SelectedKeysChanged();
            return eventHit;
        }

        public void SelectEvent(TimeflowEvent evt, bool clear = true)
        {
            if (clear) DeselectKeys();
            if (SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
            if (!SelectedEvents.Contains(evt)) {
                evt.IsSelected = true;
                SelectedEvents.Add(evt);
            }
            Input.DragPrimaryEvent = evt;
            CommitSelection();
            SelectedKeysChanged();
        }

        public void SelectEventsForObject(TimeflowObject obj)
        {
            bool selectionChanged = false;
            if (SelectedEvents == null) {
                SelectedEvents = new List<TimeflowEvent>();
                selectionChanged = true;
            }
            if (obj != null) {
                if (obj.Events != null && obj.Events.Count > 0) {
                    foreach (TimeflowEvent evt in obj.Events) {
                        SelectedEvents.Add(evt);
                        evt.IsSelected = true;
                        selectionChanged = true;
                    }
                }
            }
            if (selectionChanged) SelectedKeysChanged();
        }

        public void DeleteEvents(List<TimeflowEvent> events)
        {
            if (events != null && events.Count > 0) {
                foreach (TimeflowEvent k in events) {
                    if (k != null) {
                        UndoUtil.UndoDestroy(k);
                        ObjectTouched = true;
                    }
                }
            }
        }

        public void DeleteSelectedEvents()
        {
            DeleteEvents(SelectedEvents);
            if (IsShift) {
                DeleteEvents(RelatedEvents);
                RelatedEvents = null;
            }
            SelectedEvents = new List<TimeflowEvent>();
            SelectedKeysChanged();
        }
    }

}//AxonGenesis

#endif
