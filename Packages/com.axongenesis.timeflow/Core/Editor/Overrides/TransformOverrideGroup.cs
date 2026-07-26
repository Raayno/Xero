// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
#if UNITY_EDITOR
using System;
using UnityEngine;

namespace AxonGenesis
{
    [Serializable]
    public class TransformOverrideGroup
    {
        public const float MicroAdjustScaleMin = 0.0001f;
        private static bool IsFoldoutOpen = false;

        public static string AttributeName(int attribute) => attribute == 0 ? "xyz" : attribute == 1 ? "x" : attribute == 2 ? "y" : "z";

        public enum Types
        {
            Position,
            Rotation,
            Scale
        }

        public Types Type;
        public Vector3 Min;
        public Vector3 Max;
        public bool IsUniformMinMax;

        public bool IsSeparate;
        public bool ShowSettings;
        public bool HasAnimationConflict = false;

        [SerializeField] private bool _IsLocked;
        [SerializeField] private bool _IsLockedX;
        [SerializeField] private bool _IsLockedY;
        [SerializeField] private bool _IsLockedZ;

        [SerializeField] private bool _IsFoldout;
        [SerializeField] private bool _ShowCombined;
        [SerializeField] private bool _IsMicroAdjust;
        [SerializeField] private float _MicroAdjustScale;

        [SerializeField] public Vector3 SliderMin;
        [SerializeField] public Vector3 SliderMax;

        [NonSerialized] public TimeflowChannel XYZ = null;
        [NonSerialized] public TimeflowChannel X = null;
        [NonSerialized] public TimeflowChannel Y = null;
        [NonSerialized] public TimeflowChannel Z = null;

        public bool IsFoldout {
            get {
                return _IsFoldout;
            }
            set {
                if (_IsFoldout != value) {
                    _IsFoldout = value;
                    //Debug.Log($"{Type}.IsFoldout:{value}");
                    IsFoldoutOpen = value;
                }
            }
        }

        public bool IsPosition => Type == Types.Position;

        public bool IsRotation => Type == Types.Rotation;

        public bool IsScale => Type == Types.Scale;

        public bool IsLocked {
            get {
                if (XYZ != null) {
                    return XYZ.IsLocked;
                }
                return _IsLocked;
            }
            set {
                if (XYZ != null) {
                    if (XYZ.IsLocked != value) {
                        XYZ.IsLocked = value;
                    }
                }
                if (_IsLocked != value) {
                    _IsLocked = value;
                }
            }
        }

        public bool IsLockedX {
            get {
                if (X != null) {
                    return X.IsLocked;
                }
                return _IsLockedX;
            }
            set {
                if (X != null) {
                    if (X.IsLocked != value) {
                        X.IsLocked = value;
                    }
                }
                if (_IsLockedX != value) {
                    _IsLockedX = value;
                }
            }
        }

        public bool IsLockedY {
            get {
                if (Y != null) {
                    return Y.IsLocked;
                }
                return _IsLockedY;
            }
            set {
                if (Y != null) {
                    if (Y.IsLocked != value) {
                        Y.IsLocked = value;
                    }
                }
                if (_IsLockedY != value) {
                    _IsLockedY = value;
                }
            }
        }

        public bool IsLockedZ {
            get {
                if (Z != null) {
                    return Z.IsLocked;
                }
                return _IsLockedZ;
            }
            set {
                if (Z != null) {
                    if (Z.IsLocked != value) {
                        Z.IsLocked = value;
                    }
                }
                if (_IsLockedZ != value) {
                    _IsLockedZ = value;
                }
            }
        }

        public bool IsMicroAdjust {
            get {
                return _IsMicroAdjust;
            }
            set {
                if (_IsMicroAdjust != value) {
                    _IsMicroAdjust = value;
                    RecalculateSliderMinMax = true;
                }
            }
        }

        public float MicroAdjustScale {
            get {
                return _MicroAdjustScale;
            }
            set {
                if (_MicroAdjustScale != value) {
                    if (value < MicroAdjustScaleMin) value = MicroAdjustScaleMin;
                    _MicroAdjustScale = value;
                    //Debug.Log($"_MicroAdjustScale:{value} MicroAdjustScaleMin:{MicroAdjustScaleMin}");
                    RecalculateSliderMinMax = true;
                }
            }
        }

        public Vector3 DefaultMin {
            get {
                if (Type == Types.Position) {
                    return new Vector3(_defaultPositionMin, _defaultPositionMin, _defaultPositionMin);
                }
                else
                if (Type == Types.Rotation) {
                    return new Vector3(_defaultRotationMin, _defaultRotationMin, _defaultRotationMin);
                }
                else
                if (Type == Types.Scale) {
                    return new Vector3(_defaultScaleMin, _defaultScaleMin, _defaultScaleMin);
                }
                else {
                    return new Vector3(_defaultPositionMin, _defaultPositionMin, _defaultPositionMin);
                }
            }
        }
        public Vector3 DefaultMax {
            get {
                if (Type == Types.Position) {
                    return new Vector3(_defaultPositionMax, _defaultPositionMax, _defaultPositionMax);
                }
                else
                if (Type == Types.Rotation) {
                    return new Vector3(_defaultRotationMax, _defaultRotationMax, _defaultRotationMax);
                }
                else
                if (Type == Types.Scale) {
                    return new Vector3(_defaultScaleMax, _defaultScaleMax, _defaultScaleMax);
                }
                else {
                    return new Vector3(_defaultPositionMax, _defaultPositionMax, _defaultPositionMax);
                }
            }
        }

        private float _defaultPositionMin => TimeflowPreferences.Current.TransformOverride.DefaultPositionMin;
        private float _defaultPositionMax => TimeflowPreferences.Current.TransformOverride.DefaultPositionMax;
        private float _defaultRotationMin => TimeflowPreferences.Current.TransformOverride.DefaultRotationMin;
        private float _defaultRotationMax => TimeflowPreferences.Current.TransformOverride.DefaultRotationMax;
        private float _defaultScaleMin => TimeflowPreferences.Current.TransformOverride.DefaultScaleMin;
        private float _defaultScaleMax => TimeflowPreferences.Current.TransformOverride.DefaultScaleMax;


        [NonSerialized] public bool RecalculateSliderMinMax = false;

        [NonSerialized] private TimeflowChannel[] _Channels = null;

        public TimeflowChannel[] Channel {
            get {
                if (_Channels == null) {
                    _Channels = new TimeflowChannel[4];
                    _Channels[0] = XYZ;
                    _Channels[1] = X;
                    _Channels[2] = Y;
                    _Channels[3] = Z;
                }
                return _Channels;
            }
        }

        public bool HasAnimation => XYZ != null || X != null || Y != null || Z != null;

        public bool IsMixed => HasAnimation && (XYZ != null && (X != null || Y != null || Z != null));

        public bool IsCombined => XYZ != null && X == null && Y == null && Z == null;

        public bool ShowCombined {
            get {
                if (HasAnimation && !IsMixed) _ShowCombined = IsCombined;
                return _ShowCombined;
            }
            set {
                _ShowCombined = value;
                //Debug.Log($"{Type}.ShowCombined:{value} HasAnimation:{HasAnimation} IsMixed:{IsMixed} IsCombined:{IsCombined}");
                //Debug.Log($"{Type}.XYZ:{(XYZ == null ? "NULL" : XYZ.Name)} X:{(X == null ? "NULL" : X.Name)} Y:{(Y == null ? "NULL" : Y.Name)} Z:{(Z == null ? "NULL" : Z.Name)} ");
            }
        }

        public TransformOverrideGroup(Types type, TransformOverrideGroup copy = null)
        {
            if (type == Types.Position) {
                Setup(type, _defaultPositionMin, _defaultPositionMax, copy);
            }
            else
            if (type == Types.Rotation) {
                Setup(type, _defaultRotationMin, _defaultRotationMax, copy);
            }
            else
            if (type == Types.Scale) {
                Setup(type, _defaultScaleMin, _defaultScaleMax, copy);
            }
        }

        public TransformOverrideGroup(Types type, float min, float max, TransformOverrideGroup copy)
        {
            Setup(type, min, max, copy);
        }

        public void Setup(Types type, float min, float max, TransformOverrideGroup copy)
        {
            //Debug.Log($"TransformOverrideGroup:{type} min:{min}, max:{max}====================");
            Type = type;

            if (copy != null) {
                Min = copy.Min;
                Max = copy.Max;
                IsUniformMinMax = copy.IsUniformMinMax;
                IsSeparate = copy.IsSeparate;
                ShowSettings = copy.ShowSettings;
                ShowCombined = copy.ShowCombined;

                IsLocked = copy.IsLocked;
                IsLockedX = copy.IsLockedX;
                IsLockedY = copy.IsLockedY;
                IsLockedZ = copy.IsLockedZ;

                // Use last user setting
                IsFoldout = copy.IsFoldout;

                MicroAdjustScale = copy.MicroAdjustScale;
            }
            else {
                Min = new Vector3(min, min, min);
                Max = new Vector3(max, max, max);
                IsUniformMinMax = true;
                IsSeparate = false;
                ShowSettings = false;
                ShowCombined = true;

                IsLocked = false;
                IsLockedX = false;
                IsLockedY = false;
                IsLockedZ = false;

                IsFoldout = IsFoldoutOpen;

                MicroAdjustScale = 0.1f;
                RecalculateSliderMinMax = true;
            }
        }

        public void Reset()
        {
            if (Type == Types.Position) {
                Setup(Type, _defaultPositionMin, _defaultPositionMax, null);
            }
            else
            if (Type == Types.Rotation) {
                Setup(Type, _defaultRotationMin, _defaultRotationMax, null);
            }
            else
            if (Type == Types.Scale) {
                Setup(Type, _defaultScaleMin, _defaultScaleMax, null);
            }
        }

        public void Refresh()
        {
            //Debug.Log($"{Type}.Refresh");
            _Channels = null; // Force to rebuild
            XYZ = null;
            X = null;
            Y = null;
            Z = null;

            RecalculateSliderMinMax = true;
        }

        public void CalculateSliderMinMax(Vector3 value)
        {
            RecalculateSliderMinMax = false;

            if (MicroAdjustScale <= 0) MicroAdjustScale = 0.1f;
            if (MicroAdjustScale < MicroAdjustScaleMin) {
                MicroAdjustScale = MicroAdjustScaleMin;
            }

            Vector3 min = Min;
            Vector3 max = Max;

            if (min == max) Reset();

            if (IsMicroAdjust) {
                min *= MicroAdjustScale;
                max *= MicroAdjustScale;
            }

            if (IsMicroAdjust) {
                SliderMin.x = value.x - Mathf.Abs(min.x);
                SliderMin.y = value.y - Mathf.Abs(min.y);
                SliderMin.z = value.z - Mathf.Abs(min.z);

                SliderMax.x = value.x + max.x;
                SliderMax.y = value.y + max.y;
                SliderMax.z = value.z + max.z;
            }
            else {
                SliderMin.x = Mathf.Min(value.x, min.x);
                SliderMin.y = Mathf.Min(value.y, min.y);
                SliderMin.z = Mathf.Min(value.z, min.z);

                SliderMax.x = Mathf.Max(value.x, max.x);
                SliderMax.y = Mathf.Max(value.y, max.y);
                SliderMax.z = Mathf.Max(value.z, max.z);
            }

            //Debug.Log($"{Type}.CalculateSliderMinMax:{value} min:{SliderMin} max:{SliderMax}");

        }

        public void SelectChannel(int attribute)
        {
            if (attribute == 0 && XYZ != null && !XYZ.IsSelected) XYZ.Select();
            else
            if (attribute == 1 && X != null && !X.IsSelected) X.Select();
            else
            if (attribute == 2 && Y != null && !Y.IsSelected) Y.Select();
            else
            if (attribute == 3 && Z != null && !Z.IsSelected) Z.Select();
        }
    }
}
#endif
