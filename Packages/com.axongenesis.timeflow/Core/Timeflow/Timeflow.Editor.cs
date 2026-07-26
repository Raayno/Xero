// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        #region STATIC CONFIG

        public static EditorWindow Editor = null;

        #endregion

        #region PUBLIC

        [TimeflowIgnore]
        public List<TimeflowDisplayItem> Displays;

        [TimeflowIgnore]
        [FormerlySerializedAs("DisplayAutoSave")]
        public bool AutoSaveDisplay;

        [TimeflowIgnore]
        public bool EditorShowSettings;

        [TimeflowIgnore]
        public bool EditorShowWorkArea;

        [TimeflowIgnore]
        public bool EditorShowTimeScope;

        [TimeflowIgnore]
        public bool EditorShowTimeSettings = true;

        [TimeflowIgnore]
        public bool EditorShowParentSettings = true;

        [TimeflowIgnore]
        public bool EditorShowPlaySettings = true;

        [TimeflowIgnore]
        public bool EditorShowMarkers = false;

        [TimeflowIgnore]
        public bool EditorShowOptions = false;

        [TimeflowIgnore]
        public bool EditorShowAudio = true;

        [TimeflowIgnore]
        public bool EditorShowDisplayLists = false;

        [TimeflowIgnore]
        public bool EditorShowTools = false;

        [TimeflowIgnore]
        public bool EditorShowTimeflowObj = false;

        [TimeflowIgnore]
        public bool ShowKeyframeValues = false;

        [TimeflowIgnore]
        public bool IsEditingMarkerIndices = false;

        #endregion

        private float nextFixedUpdateTime;

        #region MODULES

        public TimeflowView View;

        public TimeflowViewLayout Layout => View == null ? null : View.Layout;

        public TimeflowViewInput Input => View == null ? null : View.Input;

        public TimeflowViewDisplay Display => View == null ? null : View.Display;

        public bool IsDisplayingPrefab => View != null && View.Display != null && View.Display.IsDisplayingPrefab;

        #endregion

        #region GUI

        public override bool CanDragTimeOffset {
            get { return true; }
            set { }
        }

        public override void Refresh() => Refresh(true);

        public void Refresh(bool refreshView = true)
        {
            //if (!IsActive) {
            //    return;
            //}
            //Debug.Log($"{name}.Refresh");
            enabled = true;
            ResetObjects = true; // force refresh objects

            _SetupObjectInstances();

            GetObjects();
            RunSetup();

            UpdateAutoFullLengthTracksRecursively();

            if (IsActive) {
                CurrentTime = CurrentTime;// Force time update
            }

            if (refreshView && View != null && IsActive) View.OnRefresh(true);
            base.Refresh();
        }

        public override void OnHierarchyChange()
        {
            //Debug.Log($"OnHierarchyChange");
            base.OnHierarchyChange();
            //ResetObjects = true; // force refresh objects
            //GetObjects();
            
            if (!IsActive) return;
            View.OnHierarchyChange();

            if (Groups != null) {
                foreach (TimeflowGroup group in Groups) {
                    group.OnHierarchyChange();
                }
            }
        }

        private void EditorSetup()
        {
            if (!IsActive) return;
            if (View == null) View = new TimeflowView(this);

            View.Setup(this);
            View.Input.SetFocus(null);

            // Force time to refresh when instance is re-enabled
            SetTime(CurrentTime);

            /// When Reload Scene is disabled (in Project Settings/Editor/Enter Play Mode Options), OnAwake
            /// and the usual setup calls are not invoked since the objects remain loaded in memory. The
            /// following check ensures that Timeflow plays automatically from the beginning when entering
            /// playmode. This can occur when using ECS configurations.
            if (EditorSettings.enterPlayModeOptionsEnabled && (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableSceneReload) != 0) {
                //if (DebugEnabled) Debug.Log(name + ".OnEnable -> OnAwake mode:" + EditorSettings.enterPlayModeOptions);
                IsReady = false;
            }
        }

        private void EditorSetdown()
        {
        }

        public override void OnNewGUIColor()
        {
            if (Instances == null || Instances.Count < 2 || Track == null) {
                GUIColor = TimeflowPreferences.GetRandomTrackColor();
            }
            else {
                // Make Timeflow track colors lighter, almost white
                GUIColor = Track.GUIColor = MathUtil.Interpolate(Track.GUIColor, Color.white, 0.75f);
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
