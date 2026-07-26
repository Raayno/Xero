// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This demonstrates how to create a custom channel link function. This class must override the
    /// Interpolate methods defined in base class to implement each data type (if supported), otherwise no
    /// processing is done and the link has no effect. Whenever possible, please implement all Interpolate
    /// methods, or as many as can be logically supported. In this example, object and string types are
    /// ignored since there is no meaningful way to calculate a "difference" between objects in the same
    /// way as numerical values. Please see the user guide for setup instructions:
    /// https://axongenesis.gitbook.io/timeflow/user-guide/timeflow-view/channel-link#custom-channel-link
    /// </summary>
    [CreateAssetMenu(fileName = "Difference", menuName = "Timeflow/Create Channel Link/Difference", order = 1)]
    sealed public class CustomChannelLinkDifference : CustomChannelLink
    {
        public float Multiplier = 1f;

        public override float Interpolate(float a, float b, float blend, TimeflowChannelLink link)
        {
            return MathUtil.Interpolate(a, Mathf.Abs(a - b) * Multiplier, blend);
        }

        public override Vector2 Interpolate(Vector2 a, Vector2 b, float blend, TimeflowChannelLink link)
        {
            float d = MathUtil.Distance(a, b) * Multiplier;
            return MathUtil.Interpolate(a, new Vector2(d, d), blend);
        }

        public override Vector3 Interpolate(Vector3 a, Vector3 b, float blend, TimeflowChannelLink link)
        {
            float d = MathUtil.Distance(a, b) * Multiplier;
            return MathUtil.Interpolate(a, new Vector3(d, d, d), blend);
        }

        public override Vector4 Interpolate(Vector4 a, Vector4 b, float blend, TimeflowChannelLink link)
        {
            float d = MathUtil.Distance(a, b) * Multiplier;
            return MathUtil.Interpolate(a, new Vector4(d, d, d), blend);
        }

        public override Color Interpolate(Color a, Color b, float blend, TimeflowChannelLink link)
        {
            float d = MathUtil.Distance(a, b) * Multiplier;
            return MathUtil.Interpolate(a, new Color(d, d, d), blend);
        }

        //public override string Interpolate(string a, string b, float blend, TimeflowChannelLink link)
        //{
        //    string v = b;
        //    return v;
        //}

        //public override Component Interpolate(Component a, Component b, float blend, TimeflowChannelLink link)
        //{
        //    Component v = b;
        //    return v;
        //}

        //public override GameObject Interpolate(GameObject a, GameObject b, float blend, TimeflowChannelLink link)
        //{
        //    GameObject v = b;
        //    return v;
        //}
    }

}//AxonGenesis