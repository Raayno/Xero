// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    //[CreateAssetMenu(fileName = "CustomChannelLink", menuName = "AxonGenesis/Create Custom Channel Link", order = 1)]
    /// <summary>
    /// This is the base class to define a custom processing script for channel links. Subclass this type
    /// to implement custom link operations. Note that the virtual methods below can act as a fallback
    /// performing a simple blend in case of unimplemented channel types. Though any custom channel links
    /// scripts should implement all types when possible as standard practice. Please see the user guide
    /// for setup instructions:
    /// https://axongenesis.gitbook.io/timeflow/user-guide/timeflow-view/channel-link#custom-channel-link
    /// </summary>
    public class CustomChannelLink : ScriptableObject
    {

        protected virtual void OnEnable()
        {
            // Keeps the scriptable object loaded, otherwise it can unexpectedly reset data during runtime
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        /// <summary>
        /// Override to implement any setup logic or precalculations before interpolation
        /// </summary>
        /// <param name="receiver">The channel which owns this link and outputs the calculated value.</param>
        public virtual void Setup(TimeflowChannel receiver) {}

        public virtual float Interpolate(float a, float b, float blend, TimeflowChannelLink link)
        {
            return MathUtil.Interpolate(a, b, blend);
        }

        public virtual Vector2 Interpolate(Vector2 a, Vector2 b, float blend, TimeflowChannelLink link)
        {
            return MathUtil.Interpolate(a, b, blend);
        }

        public virtual Vector3 Interpolate(Vector3 a, Vector3 b, float blend, TimeflowChannelLink link)
        {
            return MathUtil.Interpolate(a, b, blend);
        }

        public virtual Vector4 Interpolate(Vector4 a, Vector4 b, float blend, TimeflowChannelLink link)
        {
            return MathUtil.Interpolate(a, b, blend);
        }

        public virtual Color Interpolate(Color a, Color b, float blend, TimeflowChannelLink link)
        {
            return MathUtil.Interpolate(a, b, blend);
        }

        public virtual string Interpolate(string a, string b, float blend, TimeflowChannelLink link)
        {
            string v = b;
            return v;
        }

        public virtual Component Interpolate(Component a, Component b, float blend, TimeflowChannelLink link)
        {
            Component v = b;
            return v;
        }

        public virtual GameObject Interpolate(GameObject a, GameObject b, float blend, TimeflowChannelLink link)
        {
            GameObject v = b;
            return v;
        }

        public virtual UnityEngine.Object Interpolate(UnityEngine.Object a, UnityEngine.Object b, float blend, TimeflowChannelLink link)
        {
            UnityEngine.Object v = b;
            return v;
        }
    }

}//AxonGenesis