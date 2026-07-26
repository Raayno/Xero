// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace AxonGenesis
{
    public static class UnityEventExtension
    {
        /// <summary>
        /// Creates a copy of a UnityEvent
        /// </summary>
        public static T Clone<T>(this T ev) where T : UnityEventBase
        {
            return ReflectionUtil.DeepCopy(ev);
        }
    }

    public static class EventUtil
    {
        public static void CopyEventHandlers<T1, T2>(T1 source, T2 target, string eventName)
        {
            if (source == null || target == null || string.IsNullOrEmpty(eventName)) {

                Debug.LogWarning("Source, target, and event name must be provided.");
                return;
            }

            // Get the event info from the type
            EventInfo eventInfo = typeof(T2).GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (eventInfo == null) {
                Debug.LogWarning($"Event '{eventName}' not found on type '{typeof(T2)}'.");
                return;
            }
            // Get the source field delegate (using reflection to access the private backing field)
            FieldInfo eventField = typeof(T1).GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            if (eventField == null) {
                Debug.LogWarning($"Backing field for event '{eventName}' not found on type '{typeof(T1)}'.");
                return;
            }

            // Get the delegate from the source object
            Delegate sourceDelegate = (Delegate)eventField.GetValue(source);

            if (sourceDelegate != null) {
                foreach (Delegate handler in sourceDelegate.GetInvocationList()) {
                    // Add each handler to the target event
                    eventInfo.AddEventHandler(target, handler);
                }
            }
        }
    }

}//AxonGenesis