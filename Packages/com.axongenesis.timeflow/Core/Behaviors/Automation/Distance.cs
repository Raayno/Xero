// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is an example of implementing a custom data behavior with minimal setup. It calculates the
    /// distance between two objects and outputs the value to a data-only channel. A data-only channel
    /// simply means that the output value doesn't map to any object property but is exposed as a channel
    /// in Timeflow which may be linked with other channels to drive dynamic behavior. For more examples,
    /// see the code for AutoBank and other automation behavior scripts.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Distance")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/distance")]
    sealed public class Distance : TimeflowDataBehavior
    {
        public Transform From;
        public Transform To;
        public float Scale = 1f;
        public bool ApplyToTransform;

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            Channel.IsDataOnly = true;
            Channel.IsCombinedValue = true;
            Channel.PropertyType = Property.PropertyTypes.Float;
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.IsCombinedValue = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Float;
            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Distance";
            }
        }

        /// <summary>
        /// Since this behavior only processes float values, it only needs to override the InterpolateValue
        /// function to perform the distance calculation. 
        /// </summary>
        public override float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            /// Use the current transform as default
            if (From == null) From = transform;
            if (To == null) To = transform;

            float d = MathUtil.Distance(From.position, To.position) * Scale;
            channel.ToProperty.FloatValue = d;

            if (apply && ApplyToTransform) {
                transform.localScale = new Vector3(d, d, d);
            }

            return channel.ToProperty.FloatValue;
        }

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.Distance;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Distance"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Distance comp = Undo.AddComponent<Distance>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif
    }

}//AxonGenesis
