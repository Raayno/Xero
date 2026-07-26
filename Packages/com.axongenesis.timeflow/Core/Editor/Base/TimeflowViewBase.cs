// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;

namespace AxonGenesis
{

    [Serializable]
    public class TimeflowViewBase : EditorInput
    {
        //[NonSerialized]
        public Timeflow Timeflow => Timeflow.Active;

        public TimeflowViewBase(Timeflow timeflow)
        {
            Setup(timeflow);            
        }

        public virtual void Setup(Timeflow timeflow)
        {
            //Timeflow = timeflow;
        }
    }

}//AxonGenesis
#endif