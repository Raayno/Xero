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
    /// This defines the base for behaviors which generate data. It automatically defines a data channel
    /// which subclasses can customize, or add additional channels. All subclasses should implement
    /// CalculateOnly to generate data only and not apply it directly to any objects.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    public partial class TimeflowDataBehavior : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        #region PUBLIC MEMBERS

        public DataChannel _Channel = null;
        public bool CalculateOnly; // value is calculated but not applied

        #endregion

        #region ACCESSORS

        public override bool Enabled {
            get {
                base.Enabled = Channel.IsEnabled;
                return base.Enabled;
            }
            set {
                if (base.Enabled != value) {
                    base.Enabled = value;
                    Channel.IsEnabled = value;
                }
            }
        }

        public virtual DataChannel Channel {
            get {
                if (_Channel == null) {
                    _Channel = new DataChannel(this);
                    AddChannel(_Channel);
#if UNITY_EDITOR
                    ResetName();
#endif
                    //if (DebugEnabled) Debug.Log(name + ":TimeflowDataBehavior.Channel Add");
                }
                return _Channel;
            }
            set {
                _Channel = value;
                if (_Channel != null) {
                    _Channel.DataParent = this;
                    AddChannel(_Channel);
                    //if (DebugEnabled) Debug.Log(name + ":TimeflowDataBehavior.Channel Add");
                }
            }
        }

        public virtual Property ToProperty {
            get {
                if (Channel.ToProperty == null) {
                    Channel.ToProperty = new Property();
                    Channel.ToProperty.IsDataOnly = true;
                }
                return Channel.ToProperty;
            }
        }

        #endregion

        #region SETUP

        protected override void OnEnable()
        {
            base.OnEnable();
            //if (DebugEnabled) Debug.Log(name + ":TimeflowDataBehavior.OnEnable");
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ":TimeflowDataBehavior.OnDestruct");
            if (_Channel != null) {
                // Ensures removal from the AllChannels list
                base.RemoveChannel(_Channel);
            }
            base.OnDestruct();
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            //if (DebugEnabled) Debug.Log(name + $":TimeflowDataBehavior.SetupChannels:{(Channel == null ? "NULL": Channel.Name)}");
            if (Channel == null || string.IsNullOrEmpty(Channel.Name)) {
                Channel = new DataChannel(this);
            }
            if (Channel.ToProperty == null) {
                Channel.ToProperty = new Property();
            }
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsEnabled = true;
            Channel.SetParent(this);
            Channel.OnSetup(this);
            Channels = new List<TimeflowChannel>();
            Channels.Add(Channel);
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    obj.RegisterChannel(ch);
                }
            }
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":TimeflowDataBehavior.RemoveChannel");
            base.RemoveChannel(channel);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            if (channel != Channel) {
                return;
            }

            // Assume the component should also be removed
#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#else
			UnityEngine.Object.DestroyImmediate(this);
#endif
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            TimeflowDataBehavior t = (TimeflowDataBehavior)src;
            if (t != null) {
                //if (DebugEnabled) Debug.Log(name + ".TimeflowDataBehavior.Copy:" + src.name);
                base.Copy(src, false); // base takes care of majority of properties

                if (includeChannels) CopyChannel(t.Channel);
            }
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(name + ":TimeflowDataBehavior.CopyChannel");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
#endif
            if (Channel == null) {
                Channel = new DataChannel(this);
            }
            Channel.Copy(src);
            Channel.SetParent(this);
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.Comp = transform;
            return Channel;
        }

        #endregion

#if UNITY_EDITOR

        public override void ResetName()
        {
            //if (DebugEnabled) Debug.Log(Name + ".ResetName");
            if (Channel != null && Channel.ToProperty != null) {
                Channel.Name = Channel.ToProperty.GetNameAndAttribute("(Unassigned)", true, true, false);
            }
        }

#endif

    }

}//AxonGenesis
