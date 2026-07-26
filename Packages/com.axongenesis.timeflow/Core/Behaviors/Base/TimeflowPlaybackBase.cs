// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    public abstract class TimeflowPlaybackBase : AxonGenesisBehavior, ITimeflowPlayback
    {
        public virtual Timeflow TimeflowParent { get; set; }

        public virtual void OnRewind() {}

        public virtual void OnPlay() { }

        public virtual void OnUpdate() { }

        public virtual void OnStop() { }

        public virtual void OnLoop() { }
    }
}
