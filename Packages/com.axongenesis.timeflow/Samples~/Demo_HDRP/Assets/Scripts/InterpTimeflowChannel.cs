// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{

    /// <summary>
    /// This is an example script showing how to get a reference to a Timeflow channel and arbitrarily interplate its value.
    /// </summary>
    [ExecuteInEditMode]
    public class InterpTimeflowChannel : MonoBehaviour
    {
        [SerializeField]
        private TimeflowObject obj = null;

        [Tooltip("Set the channel name or ID to find a channel")]
        [SerializeField]
        private string channelName = null;

        [SerializeField]
        private string channelID = null;

        [Tooltip("Check this box to relocate the channel by name or id")]
        public bool GetChannelNow = false;

        [Tooltip("Adjust the input time to sample the channel value")]
        public float inputTime = 0;

        [Tooltip("The interpolated value from the channel")]
        public float outputValue = 0;

        private TimeflowChannel channel = null;

        private void OnEnable()
        {
            if (obj == null) {
                TryGetComponent<TimeflowObject>(out obj);
            }
            GetChannel();           
        }

        private void GetChannel()
        {
            GetChannelNow = false;
            if (obj != null) {
                if (channelID != null) {
                    channel = obj.GetChannelByID(channelID);
                }
                if (channel == null && channelName != null) {
                    channel = obj.GetChannel(channelName);
                }
                if (channel == null) {
                    Debug.LogWarning("No channel found. Please enter a valid channel name or ID");
                }
                else {
                    channelID = channel.UniqueID;
                }
            }
            else {
                Debug.LogWarning("Please assign a TimeflowBehavior or TimeflowObject");
            }
        }

        private void Update()
        {
            if (GetChannelNow) {
                GetChannel();
            }
            if (channel != null) {
                outputValue = channel.InterpolateValue(inputTime, false, true);

                // Use variations of interpolate for the desired data type
                //outputValue = channel.InterpolateVector3(inputTime, false, true);
                //outputValue = channel.InterpolateColor(inputTime, false, true);
            }
        }
    }

}//AxonGenesis