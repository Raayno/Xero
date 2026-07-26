// Copyright 2025 AxonGenesis All rights reserved.
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
    /// <summary>
    /// Extend this base class to implement behaviors which want update calls from Timeflow but don't need
    /// any additional information or setup. This is useful for simple behaviors which you want to perform
    /// when Timeflow is playing.
    /// </summary>
    public class TimeBehavior : AxonGenesisBehavior
    {
        public static List<TimeBehavior> Instances = null;

        public static bool updateAlways = true;
        public static int updateFrame = 0;
        public static int lateUpdateFrame = 0;
        public static int fixedUpdateFrame = 0;

        public static void UpdateAll(int frame)
        {
            if (!updateAlways && updateFrame == frame) return;
            if (Instances == null || Instances.Count == 0) return;
            foreach (TimeBehavior behavior in Instances) {
                behavior.OnUpdate();
            }
        }

        public static void LateUpdateAll(int frame)
        {
            if (!updateAlways && lateUpdateFrame == frame) return;
            if (Instances == null || Instances.Count == 0) return;
            foreach (TimeBehavior behavior in Instances) {
                behavior.OnLateUpdate();
            }
        }

        public static void FixedUpdateAll(int frame)
        {
            if (!updateAlways && fixedUpdateFrame == frame) return;
            if (Instances == null || Instances.Count == 0) return;
            foreach (TimeBehavior behavior in Instances) {
                behavior.OnFixedUpdate();
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if(Instances == null) Instances = new List<TimeBehavior>();
            Instances.Add(this);
        }

        protected override void OnDestruct()
        {
            Instances.Remove(this);
            base.OnDestruct();
        }

        public virtual void OnUpdate() {}

        public virtual void OnLateUpdate() {}

        public virtual void OnFixedUpdate() {}

    }
}//AxonGenesis