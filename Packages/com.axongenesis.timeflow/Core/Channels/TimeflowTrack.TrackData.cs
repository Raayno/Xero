// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowTrack : TimeflowChannel
	{
		public TrackData GetTrackData()
		{
			return new TrackData(this);
		}

		public void ApplyTrackData(TrackData data)
		{
			data.Apply(this);
		}

        [Serializable]
        public class TrackData
		{
			public ChannelData Channel;

			public VisibilityModes VisibilityMode = VisibilityModes.On;
			public List<GameObject> ViewObjects;
			public bool AutoFullLength = true;

			public TrackData(TimeflowTrack obj)
			{
                Channel = new ChannelData(obj);

                VisibilityMode = obj.VisibilityMode;
				ViewObjects = obj.ViewObjects == null ? null : new List<GameObject>(obj.ViewObjects);
				AutoFullLength = obj.AutoFullLength;
			}

			public void Apply(TimeflowTrack obj)
			{
                Channel.Apply(obj);

                obj.VisibilityMode = VisibilityMode;
				obj.ViewObjects = ViewObjects == null ? null : new List<GameObject>(ViewObjects);
				obj.AutoFullLength = AutoFullLength;
			}
		}
	}
}