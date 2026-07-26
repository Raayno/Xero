// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    public interface ITimeflowInterpolate
    {
        Color InterpolateColor(TimeflowChannel channel, float time, bool apply);
        Component InterpolateComponent(TimeflowChannel channel, float time, bool apply);
        GameObject InterpolateGameObject(TimeflowChannel channel, float time, bool apply);
        string InterpolateString(TimeflowChannel channel, float time, bool apply);
        float InterpolateValue(TimeflowChannel channel, float time, bool apply);
        Vector2 InterpolateVector2(TimeflowChannel channel, float time, bool apply);
        Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply);
        Vector4 InterpolateVector4(TimeflowChannel channel, float time, bool apply);
    }
}