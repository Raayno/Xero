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
    public partial class TimeflowObject
    {
        public ObjectData GetObjectData()
        {
            return new ObjectData(this);
        }

        public void ApplyObjectData(ObjectData data)
        {
            data.Apply(this);
        }

        [Serializable]
        public class ObjectData
        {
            public BehaviorData TimeflowBehavior;

            public TimeflowTrack.TrackData Track;

            public bool UpdateAnimation = true;
            public float AnimationSpeed = 1f;
            public float AnimationOffset;
            public float AnimationRand;

            public int SortOrder;

#if UNITY_EDITOR
            public bool IsLocked;
            public bool EditorShowObject;
            public bool EditorShowTrack;
            public bool EditorShowTrackKeys;
            public bool EditorShowViewOptions;
            public bool EditorShowBehaviors;
            public bool EditorShowEvents;

            public bool DisplaySolo;
            public TransformOverrideData OverrideData;
            public bool DisplayChannels = true;
            public bool IsCollapsed;
            public bool ShowChildren = true;
            public int IconIndex = -1;
#endif
            public ObjectData(TimeflowObject obj)
            {
                TimeflowBehavior = new BehaviorData(obj);
                Track = new TimeflowTrack.TrackData(obj.Track);

                UpdateAnimation = obj.UpdateAnimation;
                AnimationSpeed = obj.AnimationSpeed;
                AnimationOffset = obj.AnimationOffset;
                AnimationRand = obj.AnimationRand;

                SortOrder = obj.SortOrder;

#if UNITY_EDITOR
                IsLocked = obj.IsLocked;
                EditorShowObject = obj.EditorShowObject;
                EditorShowTrack = obj.EditorShowTrack;
                EditorShowTrackKeys = obj.EditorShowTrackKeys;
                EditorShowViewOptions = obj.EditorShowViewOptions;
                EditorShowBehaviors = obj.EditorShowBehaviors;
                EditorShowEvents = obj.EditorShowEvents;

                DisplaySolo = obj._DisplaySolo;
                OverrideData = obj.OverrideData;
                DisplayChannels = obj._DisplayChannels;
                IsCollapsed = obj._IsCollapsed;
                ShowChildren = obj._ShowChildren;
                IconIndex = obj._IconIndex;
#endif
            }

            public void Apply(TimeflowObject obj)
            {
                TimeflowBehavior.Apply(obj);
                Track.Apply(obj.Track);

                obj.UpdateAnimation = UpdateAnimation;
                obj.AnimationSpeed = AnimationSpeed;
                obj.AnimationOffset = AnimationOffset;
                obj.AnimationRand = AnimationRand;
                obj.SortOrder = SortOrder;
#if UNITY_EDITOR
                obj.IsLocked = IsLocked;
                obj.EditorShowObject = EditorShowObject;
                obj.EditorShowTrack = EditorShowTrack;
                obj.EditorShowTrackKeys = EditorShowTrackKeys;
                obj.EditorShowViewOptions = EditorShowViewOptions;
                obj.EditorShowBehaviors = EditorShowBehaviors;
                obj.EditorShowEvents = EditorShowEvents;
                obj._DisplaySolo = DisplaySolo;
                obj.OverrideData = OverrideData;
                obj._DisplayChannels = DisplayChannels;
                obj._IsCollapsed = IsCollapsed;
                obj._ShowChildren = ShowChildren;
                obj._IconIndex = IconIndex;
#endif
                obj.Setup();
            }
        }
    }

}//AxonGenesis
#endif
