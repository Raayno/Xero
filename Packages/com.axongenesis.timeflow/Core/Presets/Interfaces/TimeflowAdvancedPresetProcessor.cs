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
    public class TimeflowAdvancedPresetProcessor : IAdvancedPresetProcessor
    {
        private static int _ChannelSortOrder = 0;

        private static bool _IsTimeflowPlaying = false;

        public bool PreProcessComponent(AdvancedPresetProcessInfo info)
        {
            if (info.Mode != AdvancedPreset.Modes.Replace) return true;
            if (info.SourceComponent is Collider collider) {
                DestroyComponentsRecursive<Collider>(info.TargetObject);
                return true;
            }
            if (info.SourceComponent is Renderer renderer) {
                DestroyComponentsRecursive<Renderer>(info.TargetObject);
                return true;
            }
            if (info.SourceComponent is Timeflow timeflow) {
                DestroyComponentsRecursive<TimeflowObject>(info.TargetObject);
                return true;
            }
            if (info.SourceComponent is TimeflowObject tobj) {
                DestroyComponentsRecursive<Timeflow>(info.TargetObject);
                return true;
            }
            return true;
        }

        public static void DestroyComponentsRecursive<T>(GameObject target) where T : Component
        {
            if (target == null) return;
            // Find all components of type T in the target GameObject and its children
            var components = ObjectUtil.GetComponentsRecursive<T>(target);
            if (components != null && components.Count > 0) {
                foreach (var component in components) {
                    Undo.DestroyObjectImmediate(component);
                }
            }
        }

        public virtual bool Process(AdvancedPresetProcessInfo info)
        {
            //ebug.Log($"TimeflowAdvancedPresetProcessor.Process:{item.DisplayName}", target);
            return true; // Continue processing
        }

        public void GetDestination(AdvancedPresetProcessInfo info)
        {
            //Debug.Log($"TargetComponent:{(info.TargetComponent == null ? "NuLL" : info.TargetComponent.name)} Type:{info.Type} Mode:{info.Mode} IsTimeflowBehavior:{typeof(TimeflowBehavior).IsAssignableFrom(info.Type)}");
            // Map info.Destination to the selected channels in the Timeflow view 
            info.IsTargetChannel = false;
            if (info.TargetComponent == null && info.Mode == AdvancedPreset.Modes.Replace && typeof(TimeflowBehavior).IsAssignableFrom(info.Type)) {
                if (Timeflow.Active != null && Timeflow.Active.Display != null &&
                     Timeflow.Active.Display.SelectedChannels != null &&
                     Timeflow.Active.Display.SelectedChannels.Count > 0) {
                    foreach (var channel in Timeflow.Active.Display.SelectedChannels) {
                        if (channel == null || channel.Behavior == null) continue;
                        //Debug.Log($"<color=cyan>ProcessComponent:{channel.Name} info.Destination:{channel.Behavior.Name}</color>", channel.Behavior.gameObject);
                        if (channel.Behavior.gameObject == info.TargetObject && channel.Behavior.GetType() == info.Type) {
                            info.TargetComponent = channel.Behavior;
                            _ChannelSortOrder = channel.SortOrder;
                            info.IsTargetChannel = true;
                            break;
                        }
                    }
                }
            }
            if (info.TargetComponent == null) {
                // Map the target component to the corresponding source component by index
                info.TargetComponent = info.GetMappedComponent(info.SourceComponent);
            }
            if (info.TargetComponent == null) {
                // If not found yet, try getting the first component of the type
                info.TargetComponent = info.TargetObject.GetComponent(info.Type);
            }
            if (info.TargetComponent is TimeflowObject tobj) {
                // Only allow one TimeflowObject per GameObject
                return;
            }

            //TimeflowBehavior targetBehavior = null;
            //if (info.TargetComponent != null) {
            //    info.TargetComponent.TryGetComponent<TimeflowBehavior>(out targetBehavior);
            //    if (AdvancedPresetsGlobalConfig.CanSetTrackColors && targetBehavior != null) {
            //        Debug.Log($"targetBehavior:{targetBehavior.GetType().Name}");
            //        // If the destination is a TimeflowBehavior, set the color
            //        targetBehavior.GUIColor = info.Preset.Color;
            //    }
            //}
            if (info.Mode == AdvancedPreset.Modes.Replace) {
                //Debug.Log($"<color=magenta>{info.Type.Name}</color>");

                if (info.TargetComponent != null) {

                    if (!info.IsTargetChannel) {
                        // Determine if this is a multi-component situation
                        Component[] sourceComponents = info.SourceComponent.gameObject.GetComponents(info.Type);
                        Component[] targetComponents = info.TargetComponent.gameObject.GetComponents(info.Type);

                        int index = Array.IndexOf(sourceComponents, info.SourceComponent);
                        //Debug.Log($"<color=yellow>GetDestination:</color>{info.Type} Source Components: {sourceComponents.Length}, Target Components: {targetComponents.Length}, Index: {index}", info.TargetObject);

                        // Replaces the whole object in essence, maintaining the same number of components for each type as defined in the preset. 
                        // Any other components of the same type in the target will be destroyed if they exceed the number of components in the preset.
                        if (targetComponents.Length >= sourceComponents.Length) {
                            for (int i = sourceComponents.Length; i < targetComponents.Length; i++) {
                                // If the target has more components than the source, we can destroy them
                                if (targetComponents[i] != null) {
                                    //Debug.Log($"<color=red>GetDestination:</color> Destroying extra component {targetComponents[i].name}.{targetComponents[i].GetType().Name}", info.TargetObject);
                                    Undo.DestroyObjectImmediate(targetComponents[i]);
                                }
                            }
                            info.TargetComponent = targetComponents[index]; // Use existing instance
                        }
                        else {
                            info.TargetComponent = null; // Will create new instance
                        }
                    }
                    else
                    if (info.TargetComponent.GetType() != info.Type) {
                        // Don't replace an incompatible type
                        info.TargetComponent = null; // Will create new instance
                        return;
                    }
                }

                if (info.TargetObject == null) {
                    Debug.LogWarning("Target object is null. Cannot add component.");
                    return;
                }

                // Add a new instance of the component.
                if (info.TargetComponent == null) {
                    info.TargetComponent = Undo.AddComponent(info.TargetObject, info.Type);
                }
                return;
            }

            // If not Replace, then Combine. Instantiate is handled by a separate process.
            if (info.TargetComponent == null) {
                // If no component exists, create one.
                info.TargetComponent = Undo.AddComponent(info.TargetObject, info.Type);
                return;
            }

            if (info.Type == typeof(Transform) || info.Type == typeof(MeshFilter) || info.Type == typeof(MeshRenderer) || info.Type == typeof(RectTransform)) {
                // Ignore. Don't add multiple
                return;
            }
            // Create a new instance if multiple are allowed
            bool isMultipleAllowed = !Attribute.IsDefined(
                info.Type,
                typeof(DisallowMultipleComponent),
                inherit: true
            );
            if (isMultipleAllowed) {
                info.TargetComponent = Undo.AddComponent(info.TargetObject, info.Type);
            }
        }

        public virtual bool PrepareForProcessing(AdvancedPresetProcessInfo info)
        {
            //Debug.Log($"<color=blue>TimeflowAdvancedPresetProcessor.PrepareForProcessing:</color>{info.Type} mode:{info.Mode}", info.Destination.gameObject);

            // Stop playback so things can be applied cleanly
            _IsTimeflowPlaying = false;
            if (Timeflow.Active != null) {
                _IsTimeflowPlaying = Timeflow.Active.IsPlaying;
                Timeflow.Active.Stop();
            }
            return true; // Continue processing
        }

        public virtual bool ProcessComponent(AdvancedPresetProcessInfo info)
        {
            //Debug.Log($"<color=yellow>TimeflowAdvancedPresetProcessor.ProcessComponent:</color>{info.Type} mode:{info.Mode}", info.Destination.gameObject);

            if (info.TargetComponent.GetType() != info.Type) {
                //Debug.Log($"<color=orange>{info.TargetComponent.GetType().Name} != {info.Type.Name}</color>");
                return true;
            }
            if (info.TargetComponent is Keyframer destKeyframer) {
                // Copy keyframes from the source to the info.Target.
                Keyframer sourceKeyframer = info.SourceComponent as Keyframer;
                if (sourceKeyframer != null) {
                    if (info.Mode == AdvancedPreset.Modes.Combine) {
                        ProcessKeyframerCombine(info, sourceKeyframer, destKeyframer);
                    }
                    else {
                        ProcessKeyframerReplace(info, sourceKeyframer, destKeyframer);
                        //return true;
                    }
                }
                return false; // Stop processing, we handled the keyframer
            }
            if (info.Mode == AdvancedPreset.Modes.Replace && info.TargetComponent is Tween destTween) {
                // Copy keyframes from the source to the info.Target.
                ProcessTweenReplace(info, destTween);
                return false; // Stop processing, we handled the keyframer
            }

            return true; // Continue processing
        }

        public virtual void PostProcessComponent(AdvancedPresetProcessInfo info)
        {
            //Debug.Log($"<color=magenta>TimeflowAdvancedPresetProcessor.PostProcessComponent:</color>{info.Mode} ", info.TargetObject.gameObject);
            if (info.TargetComponent != null) {
                if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                    if (info.TargetComponent is TimeflowBehavior targetBehavior) {
                        // If the destination is a TimeflowBehavior, set the color
                        targetBehavior.SetupChannels(true);
                        targetBehavior.GUIColor = info.Preset.Color;
                    }
                    else
                    if (info.TargetComponent.TryGetComponent<TimeflowObject>(out TimeflowObject tobj)) {
                        if (tobj.IsSelected) {
                            // If the destination is a TimeflowObject, set the color
                            tobj.GUIColor = info.Preset.Color;
                        }
                    }
                }
            }
            if (info.Mode == AdvancedPreset.Modes.Replace && info.SourceComponent != null && typeof(TimeflowBehavior).IsAssignableFrom(info.SourceComponent.GetType())) {
                if (Timeflow.Active != null && Timeflow.Active.Display != null &&
                    Timeflow.Active.Display.SelectedChannels != null &&
                    Timeflow.Active.Display.SelectedChannels.Count > 0) {
                    foreach (var channel in Timeflow.Active.Display.SelectedChannels) {
                        if (channel == null || channel.Behavior == null) continue;
                        if (channel.Behavior.gameObject == info.TargetComponent.gameObject) {
                            channel.SortOrder = _ChannelSortOrder;
                            break;
                        }
                    }
                }
            }
            if (info.TargetComponent is TimeflowBehavior behavior) {
                behavior.RemapProperties();
            }
        }

        public void ProcessComplete(AdvancedPresetProcessInfo info)
        {
            //Debug.Log($"<color=red>TimeflowAdvancedPresetProcessor.ProcessComplete:</color>{info.Mode}", info.RootObject);
            info.TargetRoot.TryGetComponent<TimeflowObject>(out TimeflowObject obj);

            if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                if (info.Mode == AdvancedPreset.Modes.Instantiate) {
                    var timeflowObjects = ObjectUtil.GetComponentsRecursive<TimeflowObject>(info.TargetRoot);
                    if (timeflowObjects != null) {
                        foreach (var t in timeflowObjects) {
                            t.SetGUIColorRecursive(info.Preset.GUIColor);
                        }
                    }
                }
                else
                if (info.Preset.ApplyTransforms && obj != null) {
                    obj.SetGUIColorRecursive(info.Preset.GUIColor);
                    //Debug.Log($"<color=cyan>TimeflowAdvancedPresetProcessor.ProcessComplete: Set Track Color {obj.Track.GUIColor}</color>", info.Target);
                }
            }
            if (Timeflow.Active == null && obj != null) {
                Timeflow.CreateNewTimeflow();
            }
            if (_IsTimeflowPlaying && Timeflow.Active != null) {
                // If Timeflow was playing, resume playback
                Timeflow.Active.Play();
            }
            if (Timeflow.Active != null && Timeflow.Active.Display != null) {
                Timeflow.Active.Display.AddObjectToDisplay(info.TargetRoot);
            }
        }

        private static void ProcessKeyframerReplace(AdvancedPresetProcessInfo info, Keyframer sourceKeyframer, Keyframer destKeyframer)
        {
            if (sourceKeyframer == null || destKeyframer == null) return;

            //Debug.Log($"<color=lime>TimeflowAdvancedPresetProcessor.ProcessKeyframerReplace {sourceKeyframer.Name} to {destKeyframer.Name}</color>", info.Destination.gameObject);
            // Get the number of source keyframer channels to replace the selected channels

            List<int> channelOrders = new List<int>();
            List<TimeflowChannel> replaceChannels = new List<TimeflowChannel>();
            if (Timeflow.Active != null && Timeflow.Active.Display != null &&
                Timeflow.Active.Display.SelectedChannels != null &&
                Timeflow.Active.Display.SelectedChannels.Count > 0) {
                foreach (var channel in Timeflow.Active.Display.SelectedChannels) {
                    if (channel == null || channel.Behavior == null || !channel.IsSelected) continue;
                    if (channel.Behavior.gameObject == info.TargetRoot && channel.Behavior.GetType() == info.Type) {
                        //Debug.Log($"<color=cyan>ProcessKeyframerReplace:{channel.Name} :{channel.Behavior.Name}</color>", channel.Behavior);
                        replaceChannels.Add(channel);
                        channelOrders.Add(channel.SortOrder);
                    }
                }
            }

            if (replaceChannels.Count == 0) {
                // No selected channels, append the channels insteaed of replace
                foreach (var channel in sourceKeyframer.Channels) {
                    if (channel == null) continue;
                    TimeflowChannel newChannel = destKeyframer.CopyChannel(channel);
                    if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                        newChannel.GUIColor = info.Preset.Color;
                        newChannel.IsSelected = true;
                    }
                }
            }
            else {
                // Replace the selected channels with the source keyframer channels
                for (int i = 0; i < replaceChannels.Count; i++) {
                    TimeflowChannel channel = replaceChannels[i];
                    if (channel == null) continue;

                    // Store original data that will get overwritten by the copy process
                    string originalName = channel.Name;
                    string displayName = channel.ToProperty == null ? originalName : channel.ToProperty.DisplayName;

                    channel.Copy(sourceKeyframer.Channels[i], true); // Copy the channel data
                    channel.SortOrder = channelOrders[i];

                    if (!AdvancedPresetsGlobalConfig.CanRenameObjects) {
                        channel.Name = originalName; // Revert the name to the original
                        if (channel.ToProperty != null) {
                            channel.ToProperty.DisplayName = displayName; // Revert the display name
                        }
                    }
                    if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                        channel.GUIColor = info.Preset.Color;
                    }
                    channel.IsSelected = true; // Ensure the channel is selected
                }
            }
        }

        private static void ProcessKeyframerCombine(AdvancedPresetProcessInfo info, Keyframer source, Keyframer target)
        {
            if (source == null || target == null) return;

            // Copy keyframes from source to target
            foreach (var channel in source.Channels) {
                if (channel == null) continue;
                TimeflowChannel targetChannel = target.GetChannel(channel.Name);
                if (targetChannel == null) {
                    targetChannel = target.CopyChannel(channel);
                    if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                        //if (!channel.IsSelected) continue; // Skip unselected channels
                        targetChannel.GUIColor = info.Preset.Color;
                    }
                }
            }
        }

        private static void ProcessTweenReplace(AdvancedPresetProcessInfo info, Tween destTween)
        {
            Tween sourceTween = info.SourceComponent as Tween;
            if (sourceTween != null) {
                string name = destTween.Name;
                string displayName = destTween.Channel.ToProperty.DisplayName;
                Color color = destTween.Channel.GUIColor;

                //Debug.Log($"<color=lime>TimeflowAdvancedPresetProcessor.ProcessComponent:Copy Tween {sourceTween.Name} to {destTween.Name}</color>", destTween);
                destTween.Channel.ToProperty = new Property(sourceTween.ToProperty);
                destTween.RemapProperties();
                destTween.Copy(sourceTween, true);
                destTween.Channel.SortOrder = _ChannelSortOrder;

                // Revert settings
                destTween.Channel.IsSelected = true; // Ensure the channel is selected
                if (!AdvancedPresetsGlobalConfig.CanRenameObjects) {
                    //Debug.Log($"<color=yellow>TimeflowAdvancedPresetProcessor.ProcessComponent:Revert Tween Name:{displayName}</color>", info.Destination.gameObject);
                    destTween.Name = name;
                    destTween.Channel.Name = displayName;
                    destTween.Channel.ToProperty.DisplayName = displayName;
                }
                if (AdvancedPresetsGlobalConfig.CanSetTrackColors) {
                    destTween.Channel.GUIColor = info.Preset.Color;
                }
                else {
                    destTween.Channel.GUIColor = color;
                }
            }
        }

    }
}//AxonGenesis
#endif