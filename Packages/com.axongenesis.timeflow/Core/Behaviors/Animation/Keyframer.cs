// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    ////[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    sealed public partial class Keyframer : TimeflowBehavior, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("Channels")]
        public List<TimeflowChannel> KeyChannels;

        /// <summary>
        /// This ensures that the channels gets serialized and saved, since the base class does not
        /// serialize them. NOTE: this method gets called rapidly in Unity if the object is displayed in an
        /// inspector window.
        /// </summary>
        public void OnBeforeSerialize()
        {
            KeyChannels = null;
            if (Channels != null && Channels.Count > 0) {
                KeyChannels = new List<TimeflowChannel>(Channels);
            }
        }

        public void OnAfterDeserialize()
        {
            if (KeyChannels != null && KeyChannels.Count > 0) {
                Channels = new List<TimeflowChannel>(KeyChannels);
            }
        }

        public override bool SupportsMultipleChannels()
        {
            return true;
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false);

            if (includeChannels) {
                Keyframer kf = (Keyframer)src;
                if (kf != null && kf.Channels != null) {
                    foreach (TimeflowChannel channel in kf.Channels) {
                        if (channel.IsSelected || !includeChannels) {
                            CopyChannel(channel);
                        }
                    }
                }
                SetupChannels(true);
            }
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":Keyframer.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);

            if (Channels.Count == 0) {
#if UNITY_EDITOR
                UndoUtil.UndoDestroy(this);
#else
			    UnityEngine.Object.DestroyImmediate(this);
#endif
            }
        }

    }

}//AxonGenesis
