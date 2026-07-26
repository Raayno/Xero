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
    /// This interface provides a way for custom scripts to implement basic Timeflow playback events.
    /// Any script that implements this interface must call Register and Unregister to be notified.
    /// </summary>
    public interface ITimeflowPlayback
    {
        /// <summary>
        /// This determines which Timeflow instance drives the playback events. 
        /// </summary>
        Timeflow TimeflowParent { get; set; }

        /// <summary>
        /// Event received any time playback starts or resumes.
        /// </summary>
        void OnPlay();

        /// <summary>
        /// Event received whenever playback reaches the end and loops back to the start
        /// </summary>
        void OnLoop();

        /// <summary>
        /// Event received when playback is paused or stopped.
        /// </summary>
        void OnStop();

        /// <summary>
        /// Event received whenever playback jumps back in time
        /// </summary>
        void OnRewind();

        /// <summary>
        /// Called every frame that the TimeflowParent updates
        /// </summary>
        void OnUpdate();

    }
}