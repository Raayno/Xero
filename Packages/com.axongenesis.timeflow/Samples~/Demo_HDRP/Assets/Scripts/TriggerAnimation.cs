// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    public class TriggerAnimation : TimeflowPlayback
    {
        public KeyCode InputKey = KeyCode.Space;
        public TimeflowObject TrackToStart = null;
        public TimeflowObject TrackToStop = null;

        private void Awake()
        {
            Reset();
        }

        public override void OnRewind()
        {
            Reset();
        }

        private void Reset()
        {
            // Reset the tracks to their starting state
            TrackToStop.Enabled = true;
            TrackToStart.Enabled = false;
        }

        private void Update()
        {
            // Get the player input to trigger the event
            if (Input.GetKeyDown(InputKey)) {
                QueueTracks();
            }
        }

        /// <summary>
        /// Stop one track and start the other at the current time.
        /// </summary>
        private void QueueTracks()
        {
            if (TrackToStart == null) return;
            TrackToStop.Enabled = false;
            TrackToStart.Enabled = true;
            TrackToStart.TimeOffset = Timeflow.Active.CurrentTime;
        }
    }
}
