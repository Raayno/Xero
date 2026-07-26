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
    public partial class TimeflowChannel : SerializableObject
    {
        public ChannelData GetChannelData()
        {
            return new ChannelData(this);
        }

        public void ApplyChannelData(ChannelData data)
        {
            data.Apply(this);
        }

        [Serializable]
        public class ChannelData
        {
            public bool IsNameCustom;
            public int SortOrder;
            public List<Keyframe> Keys = null;
            public bool CanAddRemoveKeys = true;
            public Property ToProperty;
            [SerializeReference] public TimeflowChannelLink Link;
            public bool LimitValue;
            public Vector4 MinValue = Vector4.zero;
            public Vector4 MaxValue = Vector4.one;
            public bool SetGlobalShaderProperty;
            public string GlobalShaderProperty = "";
            public float LoopStart;
            public float LoopEnd = 1f;
            public float LoopLimit;
            public bool LoopPingPong;
            public bool LoopMatchEnds;
            public bool EnableAutoLoop = true;
            public bool EnableLoopIn = true;
            public bool EnableLoopOut = true;
            public float VectorLength;
            public bool IsVectorLoop;
            public bool IsVectorExtended;
            public bool AlwaysUpdate;
            public bool AlwaysShowValues;
            public bool IsUpdated;
            public string LegacyVisibility;
            public bool ShowValue;
            public bool ShowFloat;
            public bool ShowColor;
            public bool ShowVector;
            public bool ShowComponent;
            public bool ShowGameObject;
            public bool ShowObject;
            public bool ShowString;

            public SortingModes SortingMode = SortingModes.None;

            // Use this delegate to add custom processing to kefyrame interpolation while keeping the base behavior
            //public InterpolateDelegate OnInterpolate;
            //public Action OnDestruct;

            protected bool DebugEnabled;
            protected bool IsLoopSupported = true;
            protected bool IsEnabled = true;
            protected string Name;
            protected float TimeOffset;
            protected bool EnableLoop;
            protected bool HasProperty = true;
            protected bool IsCustomType;
            protected Interpolations Interpolation = Interpolations.Quadratic;
            private string UniqueID;
            protected Property.PropertyTypes PropertyType = Property.PropertyTypes.Auto;
            protected float CurrentTime = 0;

#if UNITY_EDITOR
            public bool DisplayChannel = true;
            public bool DisplayChannelSolo;
            public bool EditorShowChannel;
            public bool EditorShowKeys;
            public bool ShowTangents = true;
            public bool GraphFloatValueOnly;
            public bool ExportEveryFrame = true;

            public Color GUIColor = new Color(0, 0, 0, 1f);
            public Color GUITextColor = new Color(0, 0, 0, 1f);

            public Color GUIColorComputed = new Color(0, 0, 0, 0);
            public bool GUIColorAdustment = false;
            public int GUIColorGlobalHue = 0;
            public int GUIColorGlobalSaturation = 0;
            public int GUIColorGlobalLightness = 0;
            public float GraphMinValue;
            public float GraphMaxValue = 100f;

            public Color GUIHandles = Color.red;
            public bool GUICanDraw = true;
            public bool DrawPath;

            public bool GUIHeightLocked = false;
            public int GUIHeightOffset = 0;
            public bool IsHiddenInGraph;
            private bool IsHidden;
            private bool IsLocked;
            private bool IsGraphLocked;
#endif
            public ChannelData(TimeflowChannel obj)
            {
                IsNameCustom = obj.IsNameCustom;
                SortOrder = obj._SortOrder;
                Keys = new List<Keyframe>(obj._Keys);
                CanAddRemoveKeys = obj.CanAddRemoveKeys;
                ToProperty = obj._ToProperty;
                Link = obj.Link;
                LimitValue = obj.LimitValue;
                MinValue = obj.MinValue;
                MaxValue = obj.MaxValue;
                SetGlobalShaderProperty = obj.SetGlobalShaderProperty;
                GlobalShaderProperty = obj.GlobalShaderProperty;
                LoopStart = obj.LoopStart;
                LoopEnd = obj.LoopEnd;
                LoopLimit = obj.LoopLimit;
                LoopPingPong = obj.LoopPingPong;
                LoopMatchEnds = obj.LoopMatchEnds;
                EnableAutoLoop = obj.EnableAutoLoop;
                EnableLoopIn = obj.EnableLoopIn;
                EnableLoopOut = obj.EnableLoopOut;
                VectorLength = obj.VectorLength;
                IsVectorLoop = obj.IsVectorLoop;
                IsVectorExtended = obj.IsVectorExtended;
                AlwaysUpdate = obj.AlwaysUpdate;
                AlwaysShowValues = obj.AlwaysShowValues;
                IsUpdated = obj.IsUpdated;
                LegacyVisibility = obj.LegacyVisibility;
                ShowValue = obj.ShowValue;
                ShowFloat = obj.ShowFloat;
                ShowColor = obj.ShowColor;
                ShowVector = obj.ShowVector;
                ShowComponent = obj.ShowComponent;
                ShowGameObject = obj.ShowGameObject;
                ShowObject = obj.ShowObject;
                ShowString = obj.ShowString;
                SortingMode = obj.SortingMode;

                //EventUtil.CopyEventHandlers(obj, this, "OnInterpolate");

                //OnDestruct = obj.OnDestruct;
                DebugEnabled = obj._DebugEnabled;
                IsLoopSupported = obj._IsLoopSupported;
                IsEnabled = obj._IsEnabled;
                Name = obj.__Name;
                TimeOffset = obj._TimeOffset;
                EnableLoop = obj._EnableLoop;
                HasProperty = obj._HasProperty;
                IsCustomType = obj._IsCustomType;
                Interpolation = obj._Interpolation;
                UniqueID = obj._UniqueID;
                PropertyType = obj._PropertyType;
                CurrentTime = obj._CurrentTime;

#if UNITY_EDITOR
                DisplayChannel = obj._DisplayChannel;
                DisplayChannelSolo = obj.DisplayChannelSolo;
                EditorShowChannel = obj.EditorShowChannel;
                EditorShowKeys = obj.EditorShowKeys;
                ShowTangents = obj.ShowTangents;
                GraphFloatValueOnly = obj.GraphFloatValueOnly;
                ExportEveryFrame = obj.ExportEveryFrame;
                GUIColor = obj._GUIColor;
                GUITextColor = obj.GUITextColor;
                GUIColorComputed = obj._GUIColorComputed;
                GUIColorAdustment = obj._GUIColorAdustment;
                GUIColorGlobalHue = obj._GUIColorGlobalHue;
                GUIColorGlobalSaturation = obj._GUIColorGlobalSaturation;
                GUIColorGlobalLightness = obj._GUIColorGlobalLightness;
                GraphMinValue = obj._GraphMinValue;
                GraphMaxValue = obj._GraphMaxValue;
                GUIHandles = obj.GUIHandles;
                GUICanDraw = obj.GUICanDraw;
                DrawPath = obj.DrawPath;
                GUIHeightLocked = obj.GUIHeightLocked;
                GUIHeightOffset = obj._GUIHeightOffset;
                IsHiddenInGraph = obj.IsHiddenInGraph;
                IsHidden = obj._IsHidden;
                IsLocked = obj._IsLocked;
                IsGraphLocked = obj._IsGraphLocked;
#endif
            }

            public void Apply(TimeflowChannel obj)
            {
                obj.IsNameCustom = IsNameCustom;
                obj._SortOrder = SortOrder;
                obj._Keys = new List<Keyframe>(Keys);
                foreach(Keyframe key in obj._Keys) {
                    key.Channel = obj;
                }

                obj.CanAddRemoveKeys = CanAddRemoveKeys;
                obj._ToProperty = ToProperty;
                obj.Link = Link;
                obj.LimitValue = LimitValue;
                obj.MinValue = MinValue;
                obj.MaxValue = MaxValue;
                obj.SetGlobalShaderProperty = SetGlobalShaderProperty;
                obj.GlobalShaderProperty = GlobalShaderProperty;
                obj.LoopStart = LoopStart;
                obj.LoopEnd = LoopEnd;
                obj.LoopLimit = LoopLimit;
                obj.LoopPingPong = LoopPingPong;
                obj.LoopMatchEnds = LoopMatchEnds;
                obj.EnableAutoLoop = EnableAutoLoop;
                obj.EnableLoopIn = EnableLoopIn;
                obj.EnableLoopOut = EnableLoopOut;
                obj.VectorLength = VectorLength;
                obj.IsVectorLoop = IsVectorLoop;
                obj.IsVectorExtended = IsVectorExtended;
                obj.AlwaysUpdate = AlwaysUpdate;
                obj.AlwaysShowValues = AlwaysShowValues;
                obj.IsUpdated = IsUpdated;
                obj.LegacyVisibility = LegacyVisibility;
                obj.ShowValue = ShowValue;
                obj.ShowFloat = ShowFloat;
                obj.ShowColor = ShowColor;
                obj.ShowVector = ShowVector;
                obj.ShowComponent = ShowComponent;
                obj.ShowGameObject = ShowGameObject;
                obj.ShowObject = ShowObject;
                obj.ShowString = ShowString;
                obj.SortingMode = SortingMode;

                //EventUtil.CopyEventHandlers(this, obj, "OnInterpolate");

                //obj.OnDestruct = OnDestruct;
                obj._DebugEnabled = DebugEnabled;
                obj._IsLoopSupported = IsLoopSupported;
                obj._IsEnabled = IsEnabled;
                obj.__Name = Name;
                obj._TimeOffset = TimeOffset;
                obj._EnableLoop = EnableLoop;
                obj._HasProperty = HasProperty;
                obj._IsCustomType = IsCustomType;
                obj._Interpolation = Interpolation;
                obj._UniqueID = UniqueID;
                obj._PropertyType = PropertyType;
                obj._CurrentTime = CurrentTime;

#if UNITY_EDITOR
                obj._DisplayChannel = DisplayChannel;
                obj.DisplayChannelSolo = DisplayChannelSolo;
                obj.EditorShowChannel = EditorShowChannel;
                obj.EditorShowKeys = EditorShowKeys;
                obj.ShowTangents = ShowTangents;
                obj.GraphFloatValueOnly = GraphFloatValueOnly;
                obj.ExportEveryFrame = ExportEveryFrame;
                obj._GUIColor = GUIColor;
                obj.GUITextColor = GUITextColor;
                obj._GUIColorComputed = GUIColorComputed;
                obj._GUIColorAdustment = GUIColorAdustment;
                obj._GUIColorGlobalHue = GUIColorGlobalHue;
                obj._GUIColorGlobalSaturation = GUIColorGlobalSaturation;
                obj._GUIColorGlobalLightness = GUIColorGlobalLightness;
                obj._GraphMinValue = GraphMinValue;
                obj._GraphMaxValue = GraphMaxValue;
                obj.GUIHandles = GUIHandles;
                obj.GUICanDraw = GUICanDraw;
                obj.DrawPath = DrawPath;
                obj.GUIHeightLocked = GUIHeightLocked;
                obj._GUIHeightOffset = GUIHeightOffset;
                obj.IsHiddenInGraph = IsHiddenInGraph;
                obj._IsHidden = IsHidden;
                obj._IsLocked = IsLocked;
                obj._IsGraphLocked = IsGraphLocked;
#endif
            }
        }
    }
}
