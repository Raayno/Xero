// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Stores precalculated data for each child object.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AlignChild")]
    public class AlignChild : SerializableObject
    {
        public AlignChildren Parent;
        public Transform Transform;
        public Bounds Bounds;
        public int Index;

        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public Vector3 Scale = Vector3.zero;

        public Vector3 PositionRand = Vector3.zero;
        public Vector3 RotationRand = Vector3.zero;
        public Vector3 ScaleRand = Vector3.zero;

        public AlignChild(AlignChildren parent, Transform child, int index)
        {
            Parent = parent;
            Transform = child;
            Bounds = ObjectUtil.GetObjectBounds(child.gameObject, true);
            Index = index;

            Position = Transform.localPosition;
            Rotation = Transform.localEulerAngles;
            Scale = Transform.localScale;

            Randomize();
        }

        public void Randomize()
        {
            PositionRand = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
            RotationRand = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
            ScaleRand = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
        }

        public void UpdatePosition()
        {
            float intervalIndex = Parent.PositionReverse ? (Parent.Count - 1) - Index : Index;
            if (Parent.PositionCenter) {
                intervalIndex = ((float)intervalIndex - ((float)(Parent.Count - 1) / 2f)) * 2f;
                if (Parent.PositionAbs) intervalIndex = Mathf.Abs(intervalIndex);
            }
            Vector3 childPos = Vector3.zero;
            Vector3 pos = Parent.Position + MathUtil.Multiply(Parent.PositionEach, intervalIndex);

            if (!Parent.PositionLockX) {
                childPos.x = pos.x;
                if (Parent.PositionRelative) childPos.x += Position.x;
                childPos.x += PositionRand.x * Parent.PositionRandomize.x;
            }
            if (!Parent.PositionLockY) {
                childPos.y = pos.y;
                if (Parent.PositionRelative) childPos.y += Position.y;
                childPos.y += PositionRand.y * Parent.PositionRandomize.y;
            }
            if (!Parent.PositionLockZ) {
                childPos.z = pos.z;
                if (Parent.PositionRelative) childPos.z += Position.z;
                childPos.z += PositionRand.z * Parent.PositionRandomize.z;
            }
            if(Transform != null) Transform.localPosition = childPos;
        }

        public void UpdateRotation()
        {
            float intervalIndex = Parent.RotationReverse ? (Parent.Count - 1) - Index : Index;
            if (Parent.RotationCenter) {
                intervalIndex = (intervalIndex - ((float)(Parent.Count - 1) / 2f)) * 2f;
                if (Parent.RotationAbs) intervalIndex = Mathf.Abs(intervalIndex);
            }

            Vector3 childPos = Vector3.zero;
            Vector3 pos = Parent.Rotation + MathUtil.Multiply(Parent.RotationEach, intervalIndex);

            if (!Parent.RotationLockX) {
                childPos.x = pos.x;
                if (Parent.RotationRelative) childPos.x += Rotation.x;
                childPos.x += RotationRand.x * Parent.RotationRandomize.x;
            }
            if (!Parent.RotationLockY) {
                childPos.y = pos.y;
                if (Parent.RotationRelative) childPos.y += Rotation.y;
                childPos.y += RotationRand.y * Parent.RotationRandomize.y;
            }
            if (!Parent.RotationLockZ) {
                childPos.z = pos.z;
                if (Parent.RotationRelative) childPos.z += Rotation.z;
                childPos.z += RotationRand.z * Parent.RotationRandomize.z;
            }
            if (Transform != null) Transform.localEulerAngles = childPos;
        }

        public void UpdateScale()
        {
            float intervalIndex = Parent.ScaleReverse ? (Parent.Count - 1) - Index : Index;
            if (Parent.ScaleCenter) {
                intervalIndex = (intervalIndex - ((float)(Parent.Count - 1) / 2f)) * 2f;
                if (Parent.ScaleAbs) intervalIndex = Mathf.Abs(intervalIndex);
            }
            Vector3 childScale = Vector3.one;
            Vector3 scale = Parent.Scale + MathUtil.Multiply(Parent.ScaleEach, intervalIndex);

            float x = scale.x;
            if (Parent.ScaleRelative) x *= Scale.x;
            x += ScaleRand.x * Parent.ScaleRandomize.x;

            if (!Parent.ScaleLockX) {
                childScale.x = x;
            }
            if (!Parent.ScaleLockY) {
                if (Parent.ScaleUniform) {
                    childScale.y = x;
                }
                else {
                    childScale.y = scale.y;
                    if (Parent.ScaleRelative) childScale.y *= Scale.y;
                    childScale.y += ScaleRand.y * Parent.ScaleRandomize.y;
                }
            }
            if (!Parent.ScaleLockZ) {
                if (Parent.ScaleUniform) {
                    childScale.z = x;
                }
                else {
                    childScale.z = scale.z;
                    if (Parent.ScaleRelative) childScale.z *= Scale.z;
                    childScale.z += ScaleRand.z * Parent.ScaleRandomize.z;
                }
            }
            if (Transform != null) Transform.localScale = childScale;
        }
    }

}//AxonGenesis