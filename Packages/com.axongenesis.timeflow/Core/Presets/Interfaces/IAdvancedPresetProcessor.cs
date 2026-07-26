// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

namespace AxonGenesis
{

    public interface IAdvancedPresetProcessor
    {
        /// <summary>
        /// Perform any necessary preparation before processing the preset.
        /// </summary>
        bool PrepareForProcessing(AdvancedPresetProcessInfo info);

        /// <summary>
        /// Pre-process the component before it is added to the target GameObject.
        /// </summary>
        /// <param name="source">The existing original component from the preset prefab.</param>
        /// <param name="destination">The destination game object to apply the preset to.</param>
        bool PreProcessComponent(AdvancedPresetProcessInfo info);

        /// <summary>
        /// Processes the given entry and applies it to the target GameObject.
        /// </summary>
        /// <param name="entry">The preset item to process</param>
        /// <param name="target">The game object to apply the preset to</param>
        /// <returns>Return true to continue base processing, or return false to halt finalize processing on this item</returns>
        bool Process(AdvancedPresetProcessInfo info);

        /// <summary>
        /// Gets the destination component for the given source component based on the specified mode. This
        /// provides a default implementation that gets the first component of the same type on the target GameObject,
        /// </summary>
        /// <param name="Mode">The mode of the preset operation Replace or Combine. Instantiate mode does not invoke this method</param>
        /// <returns>Return the destination or null if not matched</returns>
        void GetDestination(AdvancedPresetProcessInfo info)
        {
            Type compType = info.SourceComponent.GetType();
            info.TargetComponent = info.TargetObject.GetComponent(compType);

            if (info.Mode == AdvancedPreset.Modes.Replace) {
                if (info.TargetComponent != null) {
                    // Remove an existing component if found.
                    Undo.DestroyObjectImmediate(info.TargetComponent);
                }
                // Add a new instance of the component.
                info.TargetComponent = Undo.AddComponent(info.TargetObject, compType);
                return;
            }
 
            if (info.TargetComponent == null) {
                // If no component exists, create one.
                info.TargetComponent = Undo.AddComponent(info.TargetObject, compType);
            }
            else
            if (compType == typeof(Transform) || compType == typeof(MeshFilter) || compType == typeof(MeshRenderer) || compType == typeof(RectTransform)) {
                // Ignore. Don't add multiple
                return;
            }
            else
            {
                // Create a new instance if multiple are allowed
                bool isMultipleAllowed = !Attribute.IsDefined(
                    compType,
                    typeof(DisallowMultipleComponent),
                    inherit: true
                );
                if (isMultipleAllowed) {
                    info.TargetComponent = Undo.AddComponent(info.TargetObject, compType);
                }
            }
        }

        /// <summary>
        /// Processes the given component and applies it to the target component.
        /// </summary>
        /// <param name="source">The existing original component from the preset prefab.</param>
        /// <param name="destination">The destination component newly added to the target object. The processor may also retarget the destination.</param>
        /// <returns></returns>
        bool ProcessComponent(AdvancedPresetProcessInfo info);

        /// <summary>
        /// Apply any final processing or setup to the component after it has been copied to the target object.
        /// </summary>
        /// <param name="source">The existing original component from the preset prefab.</param>
        /// <param name="destination">The destination component newly added to the target object.</param>
        /// <returns></returns>
        void PostProcessComponent(AdvancedPresetProcessInfo info);

        /// <summary>
        /// Preform any final processing or setup after the preset has been fully applied to the target object.
        /// </summary>
        /// <param name="entry">The preset item to process</param>
        /// <param name="target">The game object to apply the preset to</param>
        /// <returns>Return true to continue base processing, or return false to halt finalize processing on this item</returns>
        void ProcessComplete(AdvancedPresetProcessInfo info);

    }
}
#endif