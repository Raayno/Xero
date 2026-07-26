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
    /// This is a class to test all property types that can be animated with Timelow. Note that while all
    /// these types may be supported, value conversions may occur that affect precision. This was a
    /// decision to maintain overall functionality without bloating the basis for keyframes and channels.
    /// In the majority of cases the precision is negligble. However, if this is an issue with a specific
    /// type of property, a custom behavior may be needed to handle it.
    /// </summary>
    public class PropertyTest : MonoBehaviour
    {
        public bool BoolValue;
        public float FloatValue;

        /// <summary>
        /// Double and decimal values are supported however are converted to float and will not maintain
        /// precision. 
        /// </summary>
        public double DoubleValue;
        public decimal DecimalValue = 0;

        /// <summary>
        /// All int type values are convert to int for processing.
        /// </summary>
        public int IntValue;
        public uint UIntValue;
        public long LongValue;
        public ulong ULongValue;
        public short ShortValue;
        public ushort UShortValue;
        public byte ByteValue;
        public sbyte SByteValue;


        public enum EnumValues
        {
            A,
            B,
            C,
            D,
            E
        }
        public EnumValues EnumValue = EnumValues.A;

        public Vector2 Vector2Value = Vector2.zero;
        public Vector3 Vector3Value = Vector3.zero;
        public Vector4 Vector4Value = Vector4.zero;
        public Color ColorValue = Color.black;
        public Rect RectValue = Rect.zero;

        public string StringValue;
        public Object ObjectValue;
        public GameObject GameObjectValue;
        public Component ComponentValue;

    }

}//AxonGenesis