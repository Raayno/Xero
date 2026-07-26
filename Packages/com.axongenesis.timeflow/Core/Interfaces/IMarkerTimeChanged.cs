// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;

#if UNITY_EDITOR
#endif


namespace AxonGenesis
{
    public interface IMarkerTimeChanged
    {
        public static List<IMarkerTimeChanged> Instances = new List<IMarkerTimeChanged>();

        public static void Register(IMarkerTimeChanged instance)
        {
            if (!Instances.Contains(instance)) Instances.Add(instance);
        }

        public static void Unregister(IMarkerTimeChanged instance)
        {
            if (Instances.Contains(instance)) Instances.Remove(instance);
        }

        public static void TimeChanged()
        {
            if(Instances != null) {
                foreach (IMarkerTimeChanged instance in Instances) {
                    if (instance == null || instance.Behavior == null) continue;
                    instance.OnMarkerTimeChanged();
                }
            }
        }

        TimeflowBehavior Behavior { get; }

        void OnMarkerTimeChanged();
    }

}//AxonGenesis
