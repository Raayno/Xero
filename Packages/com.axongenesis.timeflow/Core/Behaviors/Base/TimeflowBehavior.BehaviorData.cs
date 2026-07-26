// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine.Events;

namespace AxonGenesis
{
    public partial class TimeflowBehavior
    {
        public BehaviorData GetBehaviorData()
        {
            return new BehaviorData(this);
        }

        public void ApplyBehaviorData(BehaviorData data)
        {
            data.Apply(this);
        }

        [Serializable]
        public class BehaviorData
        {
            public BaseData BaseBehavior;

            public UpdateMethods UpdateMethod;
            public UpdateFrequencies UpdateFrequency;
            public TimeflowBehavior UpdateAfter;

            public bool TrackActivated;
            public float ForceFramerate;
            public float TimeInterval;
            public float DefaultValue;
            public float MinValue;
            public float MaxValue;

            public float CurrentTime;
            public float TimeOffset;
            public bool CanDragTimeOffset;

            public UnityEvent TrackOn;
            public UnityEvent TrackOff;
            public UnityEvent<bool> TrackVisibilityChanged;

            //public event OnUpdateTimeDelegate OnUpdateTime; // referenced by CopyEventHandlers

#if UNITY_EDITOR
            public bool EditorShowTime;
            public bool EditorShowChannels;
            public bool IsGraphLocked;
#endif

            public BehaviorData(TimeflowBehavior obj)
            {
                BaseBehavior = new BaseData(obj);

                UpdateMethod = obj.UpdateMethod;
                UpdateFrequency = obj.UpdateFrequency;
                UpdateAfter = obj.UpdateAfter;

                TrackActivated = obj.TrackActivated;
                ForceFramerate = obj.ForceFramerate;
                TimeInterval = obj.TimeInterval;
                DefaultValue = obj.DefaultValue;
                MinValue = obj.MinValue;
                MaxValue = obj.MaxValue;
                CurrentTime = obj.CurrentTime;
                TimeOffset = obj.TimeOffset;
                CanDragTimeOffset = obj.CanDragTimeOffset;

                TrackOn = obj.TrackOn;
                TrackOff = obj.TrackOff;
                TrackVisibilityChanged = obj.TrackVisibilityChanged;

                //EventUtil.CopyEventHandlers(obj, this, "OnUpdateTime");

#if UNITY_EDITOR
                EditorShowTime = obj.EditorShowTime;
                EditorShowChannels = obj.EditorShowChannels;
                IsGraphLocked = obj.IsGraphLocked;
#endif
            }

            public void Apply(TimeflowBehavior obj)
            {
                BaseBehavior.Apply(obj);

                obj.UpdateMethod = UpdateMethod;
                obj.UpdateFrequency = UpdateFrequency;
                obj.UpdateAfter = UpdateAfter;
                obj.TrackActivated = TrackActivated;
                obj.ForceFramerate = ForceFramerate;
                obj.TimeInterval = TimeInterval;
                obj.DefaultValue = DefaultValue;
                obj.MinValue = MinValue;
                obj.MaxValue = MaxValue;
                obj.CurrentTime = CurrentTime;
                obj.TimeOffset = TimeOffset;
                obj.CanDragTimeOffset = CanDragTimeOffset;
                obj.TrackOn = TrackOn;
                obj.TrackOff = TrackOff;
                obj.TrackVisibilityChanged = TrackVisibilityChanged;

                //EventUtil.CopyEventHandlers(this, obj, "OnUpdateTime");

#if UNITY_EDITOR
                obj.EditorShowTime = EditorShowTime;
                obj.EditorShowChannels = EditorShowChannels;
                obj.IsGraphLocked = IsGraphLocked;
#endif
            }
        }
    }

}//AxonGenesis
