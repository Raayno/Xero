// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Apply this component on an object in the scene to have it become the global target for all LookAt
    /// components. This may be applied to muliple objects, however only 1 object may be the active target
    /// at a time. The last (most recent) object to be enabled becomes the global target.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    [AddComponentMenu("Timeflow/Look At Target")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/look-at-target")]
    sealed public class LookAtTarget : AxonGenesisBehavior
    {

        public float TestValue = 0f;

        protected override void OnAwake()
        {
            LookAt.GlobalTarget = transform;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LookAt.GlobalTarget = transform;
        }

#if UNITY_EDITOR

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Look At Target"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    LookAtTarget comp = Undo.AddComponent<LookAtTarget>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif
    }

}//AxonGenesis