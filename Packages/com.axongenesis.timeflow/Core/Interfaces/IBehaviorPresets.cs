// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System.Collections.Generic;

namespace AxonGenesis
{
    public interface IBehaviorPresets
    {
#if UNITY_EDITOR
        void OnBeforeSavePreset(ref List<ComponentPresetListItem> items);

        /// <summary>
        /// Invoked when a preset is being saved. This method should make any necessary changes to the preset before it is saved.
        /// This is only needed if the behavior implements custom preset behaviors or additional functions.
        /// </summary>
        /// <param name="objPreset"> The object preset being saved.</param>
        /// <param name="compPreset"> The component preset being saved.</param>
        void OnSavePreset(AdvancedPreset objPreset = null, ComponentPreset compPreset = null);

        /// <summary>
        /// Invoked when a preset is applied. This method should make any necessary updates to the behavior after the preset is applied.
        /// </summary>
        /// <param name="compPreset">The component preset being applied.</param>
        /// <param name="objPreset">The object preset being applied.</param>
        void OnPresetApplied(AdvancedPreset objPreset = null, ComponentPreset compPreset = null);

#if TIMEFLOW_LEGACY_PRESETS
        Component PresetTarget { get; }
        void LegacySavePreset();
        void LegacyOnSavePreset(BehaviorPreset preset);
        bool LegacyOnSavePreset(NameData userData);
        void LegacyOnPresetApplied(BehaviorPreset preset);
#endif
#endif
    }
}