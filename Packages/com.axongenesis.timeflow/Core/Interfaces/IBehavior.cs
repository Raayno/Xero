// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

namespace AxonGenesis
{

    public interface IBehavior
    {
        bool Enabled { get; set; }
        bool DebugEnabled { get; set; }
        bool IsAwake { get; }
        bool IsEditorAndRuntime { get; }
        bool IsEditorOnly { get; }
        bool IsRuntimeOnly { get; }
        string Name { get; set; }
        Rotator Rotator { get; }
        void Refresh();
        void Copy(AxonGenesisBehavior src, bool includeChannels);

    }
}