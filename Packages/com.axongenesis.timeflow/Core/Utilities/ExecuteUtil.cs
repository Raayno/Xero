// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.EventSystems;

namespace AxonGenesis
{
    public static class ExecuteUtil
    {
        public static void ExecuteInChildren<T>(GameObject target, BaseEventData eventData, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
        {
            if (target.transform.childCount > 0) {
                foreach (Transform child in target.transform) {
                    ExecuteEvents.Execute(child.gameObject, eventData, functor);
                }
            }
        }

        public static void ExecuteInChildrenResursive<T>(GameObject target, BaseEventData eventData, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
        {
            ExecuteEvents.Execute(target, eventData, functor);

            if (target.transform.childCount > 0) {
                foreach (Transform child in target.transform) {
                    ExecuteInChildrenResursive(child.gameObject, eventData, functor);
                }
            }
        }

    }
}
