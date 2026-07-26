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
    /// <summary>
    /// Implements custom Timeflow GUI and menu options.
    /// </summary>
    [AddComponentMenu("Timeflow/Keyframer")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/keyframer")]
    sealed public partial class Keyframer : TimeflowBehavior
    {
        public static TimeflowChannel AddChannel(GameObject obj, Property property)
        {
            TimeflowChannel channel = null;

            Keyframer kf = ObjectUtil.GetOrAddComponent<Keyframer>(obj);
            if (kf != null) {
                kf.SetupChannels(false);
                channel = new TimeflowChannel(kf);
                channel.ToProperty = new Property(channel.Behavior, property);
                channel.ToProperty.ReadValue();
                channel.GetDataType();
                channel.ResetName();

                kf.AddChannel(channel);

                Timeflow.Active.View.SelectChannel(channel, false);
                Timeflow.Active.Refresh(true);
            }

            TimeflowObject tobj = obj.GetComponent<TimeflowObject>();
            if (tobj.DisplaySolo) channel.DisplayChannelSolo = tobj.DisplaySolo;

            return channel;
        }

        /// <summary>
        /// There are no properties in the Keyframer component to animate, so hide it from the property
        /// menus.
        /// </summary>
        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        #region CONTEXT MENU

        public override Texture2D Icon => AxonUI.Icons.Keyframer;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            AxonGUI.PropertySelectMenu(TimeflowContext.Menu, typeof(Keyframer), TimeflowContext.Owner, TimeflowContext.Obj.gameObject, null, Property.PropertyFilters.All, "Add Animation/Channel/", true, GUIMenu_AddProperty);
        }

        public static void GUIMenu_AddProperty(object info)
        {
            PropertyMenuItem prop = (PropertyMenuItem)info;
            if (prop != null) {
                List<TimeflowObject> objects = TimeflowContext.GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        if (obj.Keyframer == null) {
                            obj.Keyframer = Undo.AddComponent<Keyframer>(obj.gameObject);
                            obj.Keyframer.SetupChannels(false);
                        }
                        else {
                            UndoUtil.Undo(obj.Keyframer, "Add Property", true);
                        }

                        obj.BehaviorsEnabled = true;

                        if (prop.SeparateChannels) {
                            Property p = prop.FromProperty;
                            int c = Property.GetAttributeCount(p.PropertyType);
                            for (int i = 0; i < c; i++) {
                                TimeflowChannel ch = new TimeflowChannel(obj.Keyframer);
                                obj.Keyframer.AddChannel(ch);
                                ch.ToProperty = new Property(ch.Behavior, p);
                                ch.ToProperty.Attribute = i;
                                ch.ToProperty.ReadValue();
                                ch.GetDataType();
                                ch.ResetName();
                            }
                        }
                        else {
                            TimeflowChannel ch = new TimeflowChannel(obj.Keyframer);
                            obj.Keyframer.AddChannel(ch);
                            ch.ToProperty = new Property(ch.Behavior, prop.FromProperty);
                            ch.ToProperty.ReadValue();
                            ch.GetDataType();
                            ch.ResetName();
                            Timeflow.Active.View.SelectChannel(ch);
                        }
                        Timeflow.Active.Refresh(true);
                    }
                }
            }
        }

        #endregion
    }

}//AxonGenesis

#endif