// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class CodeExample : MonoBehaviour
    {
        public float A = 0;
        public string B = "";
        public Color C = Color.black;
        public Vector3 D = Vector3.zero;
        public Color E = Color.black;

        private Keyframer keyframer = null;
        private TimeflowChannel chA = null;
        private TimeflowChannel chB = null;
        private TimeflowChannel chC = null;
        private TimeflowChannel chD = null;
        private TimeflowChannel chE = null;

        private void OnEnable()
        {
            Rebuild();
        }

        private void Clear()
        {
            keyframer.DeleteAllChannels();
        }

        private void Rebuild()
        {
            // Get or adde the Keyframer component (note that TimeflowObject is added automatically)
            keyframer = ObjectUtil.GetOrAddComponent<Keyframer>(gameObject);

            // Remove any previously created channels
            Clear();

            // Add new channels for each of the properties - their types are auto-detected 
            chA = AddChannel("A");
            chB = AddChannel("B");
            chC = AddChannel("C");
            chD = AddChannel("D");
            chE = AddChannel("E");

            // Add float keyframes (in local time)
            chA.SetKeyValue(0f, Random.value * 10f);
            chA.SetKeyValue(2.5f, Random.value * 10f);
            chA.SetKeyValue(5f, Random.value * 10f);
            chA.SetKeyValue(7.5f, Random.value * 10f);
            chA.SetKeyValue(10f, Random.value * 10f);
            chA.Interpolation = TimeflowChannel.Interpolations.Quadratic;

            // Add string keyframes
            chB.SetKeyString(0f, "Hello");
            chB.SetKeyString(5f, "World");
            chB.SetKeyString(10f, "Complete!");
            chB.Interpolation = TimeflowChannel.Interpolations.None;

            // Add color keyframes 
            chC.SetKeyColor(0f, Color.red);
            chC.SetKeyColor(5f, Color.green);
            chC.SetKeyColor(10f, Color.blue);
            chC.Interpolation = TimeflowChannel.Interpolations.Linear;

            // Add color keyframes 
            chD.SetKeyVector(0f, Vector3.up);
            chD.SetKeyVector(5f, Vector3.down);
            chD.SetKeyVector(10f, Vector3.right);
            chD.Interpolation = TimeflowChannel.Interpolations.Quadratic;

            // Add color keyframes 
            chE.SetKeyColor(0f, Color.black);
            chE.SetKeyColor(10f, Color.white);
            chE.Interpolation = TimeflowChannel.Interpolations.Linear;

            // Create a new channel link
            chE.Link = new TimeflowChannelLink(chE, chC);
            chE.Link.Mode = TimeflowChannelLink.Modes.Add;

        }

        private TimeflowChannel AddChannel(string channelName)
        {
            // Check if the channel has already been created
            if (keyframer.HasChannelNamed(channelName)) {
                return keyframer.GetChannel(channelName);
            }

            // Create the new channel
            TimeflowChannel channel = new TimeflowChannel(keyframer);
            channel.Name = channelName;

            // Map the channel to the named property on this component
            channel.ToProperty = new Property(this, channelName);

            keyframer.AddChannel(channel);
            return channel;
        }
    }
}
