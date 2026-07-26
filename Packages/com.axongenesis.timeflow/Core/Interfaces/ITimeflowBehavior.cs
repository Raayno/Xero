// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{

    public interface ITimeflowBehavior
    {
        bool CanUpdate { get; }
        TimeflowBehavior UpdateAfter { get; set; }
        TimeflowBehavior.UpdateFrequencies UpdateFrequency { get; set; }
        bool UseUpdate { get; set; }
        bool UseFixedUpdate { get; set; }
        bool UseLateUpdate { get; set; }
        int CurrentFrame { get; set; }
        float CurrentTime { get; set; }
        float CurrentTimeWorld { get; set; }
        float TimeOffset { get; set; }
        float TimeOffsetWorld { get; set; }
        float EndTime { get; set; }
        float LocalDeltaTime { get; }
        Timeflow Timeflow { get; set; }
        TimeflowObject ParentObject { get; set; }
        List<TimeflowChannel> Channels { get; }
        bool HasLinkedBehaviors { get; }

        event TimeflowBehavior.OnUpdateTimeDelegate OnUpdateTime;

        void AddChannel(TimeflowChannel channel);
        void CheckLinkedBehaviors();
        void CheckParent(bool force);
        void CleanUp();
        void Copy(AxonGenesisBehavior src, bool includeChannels);
        TimeflowChannel CopyChannel(TimeflowChannel src);
        void DoUpdate();
        void DoUpdate(bool explicitChannels);
        TimeflowChannel GetChannel(string name);
        TimeflowChannel GetChannelByID(string id);
        bool HasChannel(Component obj, string name);
        bool HasChannel(Component obj, string name, int index);
        bool HasChannelNamed(string name);
        bool HasChannelNamed(string name, int index);
        bool HasChannel(string pathName);
        bool HasComponent(Component obj);
        void LinkBehavior(TimeflowBehavior link);
        void OnInterpolationChanged();
        void OnPlay();
        void OnRenderEnable();
        void OnRewind();
        void OnSortChannels();
        void OnStop();
        void OnStartPlayback();
        void OnTrackEnd();
        void OnTrackStart();
        void OnUpdateTimingMode();
        void OnVectorChanged();
        void Refresh();
        void RegisterChannels(TimeflowObject obj);
        void RemoveChannel(TimeflowChannel channel);
        void RemoveChannelWithUndo(TimeflowChannel channel);
        float GetTime();
        void SetTime(float time);
        void SetupChannels(bool forceSetup);
        void SortChannels();
        bool SupportsMultipleChannels();
        void UnlinkBehavior(TimeflowBehavior link);
        void UpdateTime();
        void UpdateTimeChannel(TimeflowChannel channel);
        void UpdateTimeChannelsExplicit();
        void UpdateTimeLinked();
    }
}