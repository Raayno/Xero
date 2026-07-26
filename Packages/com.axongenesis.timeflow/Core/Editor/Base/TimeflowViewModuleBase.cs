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
    public class TimeflowViewModuleBase : TimeflowViewBase
    {
        public TimeflowViewModuleBase(Timeflow timeflow) : base(timeflow) {}

        public TimeflowView View => Timeflow == null ? null : Timeflow.View;

        public TimeflowViewLayout Layout => Timeflow == null ? null : Timeflow.View.Layout;

        public TimeflowViewInput Input => Timeflow == null ? null : Timeflow.View.Input;

        public TimeflowViewMarkers Markers => Timeflow == null ? null : Timeflow.View.Markers;

        public TimeflowViewDisplay Display => Timeflow == null ? null : Timeflow.View.Display;

    }

}//AxonGenesis
#endif