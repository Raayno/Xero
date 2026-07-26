// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a utility that forces a ParticleSytem to update with Timeflow in edit mode. This is
    /// automatically applied whenever an object with a TrailRenderer component is added to Timeflow. It is
    /// not required, however provided improved trail behavior with Timeflow. 
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [RequireComponent(typeof(TrailRenderer))]
    [AddComponentMenu("Timeflow/Trail Renderer Update")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/trail-renderer-update")]
    sealed public class TrailRendererUpdate : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        #region PUBLIC

        public TrailRenderer Trail;
        public bool ClearOnRewind = true;
        public bool ClearOnStop = true;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private bool needsCleared;

        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            Trail = GetComponent<TrailRenderer>();
        }

        public override void OnPlay()
        {
            base.OnPlay();
            if (Enabled && Trail != null) {
                Trail.Clear();
                Trail.emitting = true;
            }
            //if (DebugEnabled) Debug.Log(name + ".TrailRendererUpdate.OnPlay");
        }

        public override void OnStop()
        {
            base.OnStop();
            if (Enabled && ClearOnStop  && Trail != null && !Application.isPlaying) {
                //if (DebugEnabled) Debug.Log(name + ".TrailRendererUpdate.OnStop");
                Trail.Clear();
            }
        }

        public override void OnRewind()
        {
            base.OnRewind();
            //if (DebugEnabled) Debug.Log(name + ".TrailRendererUpdate.OnRewind");
            if (Enabled && ClearOnRewind && Trail != null) {
                //if (DebugEnabled) Debug.Log(name + ".TrailRendererUpdate.OnRewind Clear");
                /// This defers clearing until late udpate in the following method to fully clear any
                /// transform updates.
                needsCleared = true;
            }
        }

        public override void OnFinalUpdate()
        {
            base.OnFinalUpdate();
            if (needsCleared && Enabled && Trail != null) {
                //if (DebugEnabled) Debug.Log(name + ".TrailRendererUpdate.OnFinalUpdate needsCleared");
                Trail.Clear();
                needsCleared = false;
            }
        }

        public void Pause()
        {
            if (Enabled && Trail != null) {
                Trail.emitting = false;
            }
        }

#if UNITY_EDITOR

        public override Texture2D Icon => AxonUI.Icons.TrailRendererUpdate;

        /// <summary>
        /// Prevents component reference from being listed in property lists, since there's nothing to
        /// animate here.
        /// </summary>
        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Trail Renderer Update"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<TrailRendererUpdate>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif


    }

}//AxonGenesis
