// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

namespace AxonGenesis
{
    public class TimeflowChannelComponentPreset : TimeflowComponentPreset
    {
        public TimeflowChannelLoopPreset Loop = new TimeflowChannelLoopPreset();

        public virtual void ApplyLoopSettings(TimeflowChannel target)
        {
            if (target == null) return;
            Loop.Apply(target);
        }

        public virtual void GetLoopSettings(TimeflowChannel target)
        {
            if (target == null) return;
            Loop.Get(target);
        }

        public override void GUI()
        {
            base.GUI();
            Loop.OnGUI(this);
        }
    }

}//AxonGenesis

#endif