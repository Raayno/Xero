// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

namespace AxonGenesis
{
    [CreateAssetMenu(fileName = "TrackColorPalette", menuName = "Timeflow/Track Color Palette", order = 1)]
    public partial class TrackColorPalette : ScriptableObject
    {
        public bool ColorByComponentType = false;
        public bool ConformChannelColors = false;
        public bool ColorByChannelType = false;
        public bool AllowFullyRandomColors = true;

        public enum TypeAssignmentModes
        {
            UserControlled,
            AutomaticYield,
            AutomaticForced
        }

        public enum AssignmentModes
        {
            Auto,
            ByType,
            Sequential,
            Random,
            Explicit
        }

        [SerializeField, FormerlySerializedAs("AssignmentMode")]
        private TypeAssignmentModes _AssignmentMode = TypeAssignmentModes.UserControlled;

        public string TypeFilter = "";

        public Color DefaultColor = Color.white;
        public bool IsDefaultRandom = false;

        // Saturation and lightness are relative, so 0 is no change
        public bool EnableColorAdjustment = false;
        public int Hue = 0;
        public int Saturation = 0;
        public int Lightness = 0;

        public bool EditorShowSettings = true;
        public bool EditorShowGlobalAdjustment = true;
        public bool EditorShowColorPalette = true;

        [TrackColorDefinitionAttribute] public List<TrackColorDefinition> Colors = new List<TrackColorDefinition>();

        private int _CurrentIndex = 0;

        [SerializeField]
        private bool _SortByType = false;

        public TypeAssignmentModes AssignmentMode {
            get { return _AssignmentMode; }
            set {
                if (_AssignmentMode != value) {
                    _AssignmentMode = value;
                    //Debug.Log($"AssignmentMode:{value}");
                    Save();
                }
            }
        }

        public bool SortByType {
            get { return _SortByType; }
            set {
                if (_SortByType != value) {
                    _SortByType = value;
                    if (_SortByType) Colors.Sort((a, b) => a.TypeSort.CompareTo(b.TypeSort));
                    else Colors.Sort((a, b) => a.ColorSort.CompareTo(b.ColorSort));
                }
            }
        }

        public int CurrentIndex {
            get { return _CurrentIndex; }
            set {
                if (_CurrentIndex != value) {
                    _CurrentIndex = value;
                    //Debug.Log($"CurrentIndex:{value}");
                }
            }
        }

        public bool IsAutomatic => (AssignmentMode == TypeAssignmentModes.AutomaticYield || AssignmentMode == TypeAssignmentModes.AutomaticForced) && (ColorByComponentType || ColorByChannelType);

        public bool IsAutomaticForced => AssignmentMode == TypeAssignmentModes.AutomaticForced;

        private void OnValidate()
        {
            ComponentTypeDrawer.SearchFilter = TypeFilter;
            ChannelTypeDrawer.SearchFilter = TypeFilter;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);

            // Save the changes to the Asset Database
            AssetDatabase.SaveAssets();
        }

        #region GET COLORS

        public Color GetColor(int index)
        {
            TrackColorDefinition def = GetColorDefinition(index);
            if (def != null) return def.Color;
            else return Color.white;
        }

        public Color GetNextColor()
        {
            TrackColorDefinition def = GetNextColorDefinition();
            if (def != null) return def.Color;
            else return Color.white;
        }

        public Color GetRandomColor()
        {
            CurrentIndex = MathUtil.Random(0, Colors.Count - 1);
            return GetColor(CurrentIndex);
        }

        #endregion

        #region GET COLOR DEFINITIONS

        public TrackColorDefinition GetDefault()
        {
            TrackColorDefinition def = new TrackColorDefinition();
            def.Color = DefaultColor;
            def.Name = "Default Color";
            return def;
        }

        public TrackColorDefinition GetColorDefinition(int index)
        {
            CurrentIndex = index;
            if (Colors == null || Colors.Count == 0) {
                Debug.LogWarning("No colors have been defined.", this);
                return null;
            }
            if (CurrentIndex >= Colors.Count || CurrentIndex < 0) CurrentIndex = 0;
            return Colors[CurrentIndex];
        }

        public TrackColorDefinition GetNextColorDefinition()
        {
            TrackColorDefinition def = GetColorDefinition(CurrentIndex);
            CurrentIndex++;

            //Debug.Log($"GetNextColorDefinition:{CurrentIndex} {def.Name}");
            def = SkipColorDefinition(def);
            return def;
        }

        private TrackColorDefinition SkipColorDefinition(TrackColorDefinition def)
        {
            int i = 0;
            while (def == null || def.Skip) {
                CurrentIndex++;
                def = GetColorDefinition(CurrentIndex);
                i++;
                if (i > Colors.Count) break;
            }
            return def;
        }

        public TrackColorDefinition GetRandomColorDefinition()
        {
            TrackColorDefinition def;
            if (AllowFullyRandomColors) {
                def = new TrackColorDefinition();
                def.Color = ColorUtil.Random();
                //Debug.Log(def.Color);
                return def;
            }
            int nextIndex = MathUtil.Random(0, Colors.Count - 1);
            def = Colors[nextIndex];
            while (def.Skip) {
                nextIndex = MathUtil.Random(0, Colors.Count - 1);
                def = Colors[nextIndex];
            }
            CurrentIndex = nextIndex;

            //Debug.Log($"CurrentIndex:{CurrentIndex}"); 
            def = Colors[CurrentIndex];
            def = SkipColorDefinition(def);
            return def;
        }

        public TrackColorDefinition GetColorDefinitionByComponentType(TimeflowObject obj)
        {
            if (obj == null) return null;

            Component comp;

            Sort(true);

            int i = 1;
            foreach (TrackColorDefinition def in Colors) {
                if (string.IsNullOrEmpty(def.ComponentType)) continue;
                def.Type = Type.GetType(def.ComponentType);
                if (def.Type == null) {
                    //Debug.LogWarning($"Unknown type:{def.ComponentType}");
                    continue;
                }
                if (obj.TryGetComponent(def.Type, out comp)) {
                    return def;
                }
                i++;
            }
            return null;
        }

        public TrackColorDefinition GetColorDefinitionByChannelType(TimeflowChannel ch)
        {
            if (ch == null) return null;

            Sort(true);

            foreach (TrackColorDefinition def in Colors) {
                if (string.IsNullOrEmpty(def.ChannelType)) continue;

                Type channelType = Type.GetType(def.ChannelType);
                if (channelType == null) {
                    //Debug.LogError($"Invalid channel type:{def.ChannelType}");
                    continue;
                }
                if (channelType.IsAssignableFrom(ch.GetType())) {
                    return def;
                }
            }

            if (ch.Behavior == null) return null;

            // Try to match by the channel owner's component type
            foreach (TrackColorDefinition def in Colors) {
                if (string.IsNullOrEmpty(def.ComponentType)) continue;

                Type compType = Type.GetType(def.ComponentType);
                if (compType == null) {
                    //Debug.LogError($"Invalid channel type:{def.ChannelType}");
                    continue;
                }
                if (compType.IsAssignableFrom(ch.Behavior.GetType())) {
                    return def;
                }
            }

            return null;
        }

        public TrackColorDefinition GetTrackColorDefinition(AssignmentModes mode, TimeflowObject obj, TimeflowChannel ch)
        {
            TrackColorDefinition def = null;

            //Debug.Log($"GetTrackColorDefinition:{mode} obj:{AssignmentMode} CurrentIndex:{CurrentIndex}");
            if (mode == AssignmentModes.Auto) {
                if (AssignmentMode == TypeAssignmentModes.AutomaticYield || AssignmentMode == TypeAssignmentModes.AutomaticForced) {
                    mode = AssignmentModes.ByType;
                }
                else
                if (IsDefaultRandom) {
                    mode = AssignmentModes.Random;
                }
                else {
                    def = GetDefault();
                    return def;
                }
            }
            if (mode == AssignmentModes.ByType) {
                if (ColorByChannelType && ch != null) {
                    def = GetColorDefinitionByChannelType(ch);
                }
                if (ColorByComponentType && def == null && obj != null) {
                    def = GetColorDefinitionByComponentType(obj);
                }
            }
            else {
                Sort();
                if (mode == AssignmentModes.Sequential) {
                    def = GetNextColorDefinition();
                }
                else
                if (mode == AssignmentModes.Random) {
                    def = GetRandomColorDefinition();
                }
            }

            return def;
        }

        #endregion

        #region ASSIGN COLORS

        public void AutoAssignColor(TimeflowObject obj)
        {
            if (!IsAutomatic) return;
            //Debug.Log($"{obj.name}.GUIColorAuto:{obj.GUIColorAuto} IsAutomaticForced:{IsAutomaticForced}");
            if (obj.GUIColorAuto || IsAutomaticForced) {
                if (ColorByComponentType) AutoAssignColorsByType(obj);
            }
        }

        public void AutoAssignColor(TimeflowChannel ch)
        {
            if (ch.GUIColorAuto && ConformChannelColors && !ch.IsTrack) {
                ch.GUIColor = ch.Object.GUIColor;
                return;
            }
            if (!IsAutomatic) return;
            if (!ch.GUIColorAuto && !IsAutomaticForced) return;
            if (ColorByChannelType) AutoAssignColorsByType(ch);
        }

        public void AutoAssignColorsByType(TimeflowObject obj)
        {
            if (!obj.GUIColorAuto && !IsAutomaticForced) return;
            //Debug.Log($"AutoAssignColorsByType: Object:{obj.name}", obj.gameObject);
            TrackColorDefinition def = GetTrackColorDefinition(AssignmentModes.ByType, obj, null);
            if (def != null) {
                obj.GUIColor = def.Color;
                if (obj.AllChannelsForDisplay != null) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        if (ch == null || ch.IsLocked) continue;
                        if (ConformChannelColors || !ColorByChannelType) {
                            ch.GUIColor = obj.GUIColor;
                            ch.GUIColorAuto = true;
                            continue;
                        }
                        TrackColorDefinition defch = GetTrackColorDefinition(AssignmentModes.ByType, null, ch);
                        if (defch != null) {
                            ch.GUIColor = defch.Color;
                            ch.GUIColorAuto = true;
                        }
                        else {
                            ch.GUIColor = def.Color;
                            ch.GUIColorAuto = true;
                        }
                    }
                }
            }
        }

        public void AutoAssignColorsByType(TimeflowChannel ch)
        {
            if (!ch.GUIColorAuto && !IsAutomaticForced) return;
            //Debug.Log($"AutoAssignColorsByType: Channel:{ch.Name}", ch.Object.gameObject);
            TrackColorDefinition def = GetTrackColorDefinition(AssignmentModes.ByType, null, ch);
            if (def != null) ch.GUIColor = def.Color;
        }

        public void AssignColorsByType(TimeflowObject obj, TimeflowChannel ch)
        {
            AssignColors(AssignmentModes.ByType, obj, ch);
        }

        public void AssignColorsRandom()
        {
            if (IsAutomaticForced) return;
            AssignColors(AssignmentModes.Random);
        }

        public void AssignColorsSequential()
        {
            if (IsAutomaticForced) return;
            AssignColors(AssignmentModes.Sequential);
        }

        public void AssignColorToObject(AssignmentModes mode, TimeflowObject obj, TrackColorDefinition def, Color color)
        {
            if (IsAutomaticForced) mode = AssignmentModes.ByType;

            UndoUtil.Undo(obj, $"Assign Track Color {mode}", true);
            obj.GUIColor = def == null ? color : def.Color;
            obj.GUIColorAuto = mode == AssignmentModes.ByType;

            if (obj.AllChannelsForDisplay == null) return;
            foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                if (ch == null || ch.IsLocked) continue;
                if (ch.IsTrack || (!ch.IsSelected && ColorByChannelType)) continue;
                if (ConformChannelColors) {
                    ch.GUIColor = obj.GUIColor;
                    ch.GUIColorAuto = false;
                    continue;
                }
                //Debug.Log($"ch:{ch.Name} {ch.UniqueID}");
                TrackColorDefinition defch = GetTrackColorDefinition(mode, null, ch);
                if (defch != null) {
                    ch.GUIColor = defch.Color;
                    ch.GUIColorAuto = mode == AssignmentModes.ByType;
                }
                else
                if (def != null) {
                    ch.GUIColor = def.Color;
                    ch.GUIColorAuto = mode == AssignmentModes.ByType;
                }
                else {
                    ch.GUIColor = color;
                    ch.GUIColorAuto = false;
                }
            }
        }

        public void AssignColors(AssignmentModes mode, TimeflowObject objContext = null, TimeflowChannel chContext = null)
        {
            if (IsAutomaticForced) mode = AssignmentModes.ByType;

            //Debug.Log($"AssignColors:{mode} obj:{(objContext == null ? "NULL" : objContext.name)} ch:{(chContext == null ? "NULL" : chContext.Name)}");
            CurrentIndex = 0; // Reset index for sequential assignment

            if (objContext == null && chContext == null && Timeflow.Active != null && Timeflow.Active.Display != null) {
                // Apply to all objects in the view
                if (Timeflow.Active.Display.Objects != null && Timeflow.Active.Display.Objects.Count > 0) {
                    foreach (TimeflowObject obj in Timeflow.Active.Display.Objects) {
                        if (obj.IsLocked || !obj.IsDisplayed) continue;
                        TrackColorDefinition def = GetTrackColorDefinition(mode, obj, null);
                        if (def != null) {
                            AssignColorToObject(mode, obj, def, def.Color);
                        }
                        if (obj.AllChannelsForDisplay != null) {
                            foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                                if (ch.IsLocked || ch.IsHidden || !ch.IsDisplayed) continue;
                                AssignColorToChannel(mode, ch);
                            }
                        }
                    }
                }

                return;
            }

            bool applyToSelected = true;
            if (objContext != null) {
                if (!objContext.IsSelected) applyToSelected = false;
                TrackColorDefinition def = GetTrackColorDefinition(mode, objContext, null);
                if (def != null) {
                    AssignColorToObject(mode, objContext, def, def.Color);
                }
            }
            if (chContext != null) {
                if (!chContext.IsSelected) applyToSelected = false;
                TrackColorDefinition def = GetTrackColorDefinition(mode, null, chContext);
                if (def != null) {
                    UndoUtil.Undo(chContext.Behavior, "Assign Track Color", true);

                    //chContext.GUIColor = def.Color;
                    //chContext.GUIColorAuto = mode == AssignmentModes.ByType;
                }
            }

            //Debug.Log($"AssignColors:{mode} applyToSelected:{applyToSelected}");
            if (applyToSelected && Timeflow.Active != null && Timeflow.Active.View != null) {
                if (Timeflow.Active.View.SelectedObjects != null && Timeflow.Active.View.SelectedObjects.Count > 0) {
                    Timeflow.Active.View.SortSelectedObjects();
                    foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                        if (obj == null || obj == objContext || obj.IsLocked) continue;
                        TrackColorDefinition def = GetTrackColorDefinition(mode, obj, null);
                        //Debug.Log($"AssignColors: obj:{obj.name} def:{def.Color} {def.Name}");
                        if (def != null) AssignColorToObject(mode, obj, def, def.Color);
                    }
                }
                else
                if (Timeflow.Active.View.SelectedChannels != null) {
                    Timeflow.Active.View.SortSelectedChannels();
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch == null || ch == chContext || ch.IsLocked) continue;
                        AssignColorToChannel(mode, ch);
                    }
                }
            }
        }

        public void AssignColorToChannel(AssignmentModes mode, TimeflowChannel ch)
        {
            if (ch == null || ch.IsLocked || ch.IsTrack) return;
            if (IsAutomaticForced) mode = AssignmentModes.ByType;
            UndoUtil.Undo(ch.Behavior, "Assign Track Color", true);
            if (ConformChannelColors) {
                ch.GUIColor = ch.Object.GUIColor;
                return;
            }
            TrackColorDefinition def = GetTrackColorDefinition(mode, null, ch);
            if (def != null) {
                ch.GUIColor = def.Color;
                ch.GUIColorAuto = mode == AssignmentModes.ByType;
            }
        }

        #endregion

        #region SORTING

        public void Sort(bool byType = false)
        {
            if (byType) {
                Colors.Sort((a, b) => a.TypeSort.CompareTo(b.TypeSort));
            }
            else {
                Colors.Sort((a, b) => a.ColorSort.CompareTo(b.ColorSort));
            }
        }

        public void SortAlphabetical()
        {
            Colors.Sort((a, b) => a.Name.CompareTo(b.Name));
            ReindexSort();
        }

        public void SortByTypeName()
        {
            Colors.Sort((a, b) => a.ComponentType.CompareTo(b.ComponentType));
            ReindexSort();
        }

        public void SortByHue()
        {
            Colors.Sort((a, b) => {
                float aa = ColorUtil.GetHue(a.Color);
                float bb = ColorUtil.GetHue(b.Color);
                return aa.CompareTo(bb);
            });
            ReindexSort();
        }

        public void SortByLightness()
        {
            Colors.Sort((a, b) => {
                float aa = ColorUtil.GetLightness(a.Color);
                float bb = ColorUtil.GetLightness(b.Color);
                return aa.CompareTo(bb);
            });
            ReindexSort();
        }

        public void SortBySaturation()
        {
            Colors.Sort((a, b) => {
                float aa = ColorUtil.GetSaturation(a.Color);
                float bb = ColorUtil.GetSaturation(b.Color);
                return aa.CompareTo(bb);
            });
            ReindexSort();
        }

        public void ReverseSort()
        {
            Colors.Reverse();
            ReindexSort();
        }

        public void ReindexSort()
        {
            int i = 0;
            foreach (TrackColorDefinition color in Colors) {
                if (SortByType) {
                    //Debug.Log($"Type:{i} {color.ComponentType} ch:{color.ChannelType} {color.Name}");
                    color.TypeSort = i;
                }
                else {
                    //Debug.Log($"Color:{i} Sort:{color.Name}");
                    color.ColorSort = i;
                }
                EditorUtil.SetDirty(this);
                i++;
            }
        }

        #endregion
    }
}
#endif
