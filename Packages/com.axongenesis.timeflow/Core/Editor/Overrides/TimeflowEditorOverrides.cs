// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using AxonGenesis;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public static class TimeflowEditorOverrides
    {
#if TIMEFLOW_PRO
        public const string kDisableTimeflow = "⚙️ Editor/⚒️ Disable Timeflow Pro";
#else
        public const string kEnableTimeflow = "⚒️ Enable Timeflow Pro"; // Placed at root for visibility
#endif
        static readonly public string _TIMEFLOW_OVERRIDES_DISABLED = "TIMEFLOW_OVERRIDES_DISABLED";
        static readonly public string _TIMEFLOW_PRO = "TIMEFLOW_PRO";

#if TIMEFLOW_OVERRIDES_DISABLED
    public static bool IsOverrideDisabled => true;

    [MenuItem("CONTEXT/Transform/Enable Timeflow Overrides")]
    public static void EnableOverrides(MenuCommand command)
    {
        EnableOverrides(true);
    }
#else
        public static bool IsOverrideDisabled => false;

        [UnityEditor.MenuItem("CONTEXT/Transform/Disable Timeflow Overrides")]
        public static void DisableOverrides(MenuCommand command)
        {
            EnableOverrides(false);
        }
#endif

        public static void EnableOverrides(bool enable)
        {
            // Deselect so the inspector view can refresh when reselected
            SelectionUtil.Clear();

            if (enable) {
                Debug.Log("Timeflow editor overrides have been enabled. The scripting define symbol TIMEFLOW_OVERRIDES_DISABLED has been added in Player Settings.");//--KEEP
                EditorScriptingDefineUtils.RemoveScriptingDefineSymbol(_TIMEFLOW_OVERRIDES_DISABLED);
            }
            else {
                Debug.Log("Timeflow editor overrides have been disabled. The scripting define symbol TIMEFLOW_OVERRIDES_DISABLED has been added in Player Settings.");//--KEEP
                EditorScriptingDefineUtils.AddScriptingDefineSymbol(_TIMEFLOW_OVERRIDES_DISABLED);
            }
        }


#if TIMEFLOW_PRO
        public const bool IsTimeflowPro = true;

        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kDisableTimeflow, priority = 3000)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kDisableTimeflow, priority = 3000)]
        public static void DisableTimeflowPro(MenuCommand command)
        {
            SetTimeflowPro(false);
        }
#else
        public const bool IsTimeflowPro = false;

        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kEnableTimeflow, priority = 50000)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kEnableTimeflow, priority = 50000)]
        public static void EnableTimeflowPro(MenuCommand command)
        {
            SetTimeflowPro(true);
        }
#endif

        public static void ToggleTimeflowPro()
        {
#if TIMEFLOW_PRO
            SetTimeflowPro(false);
#else
            SetTimeflowPro(true);
#endif
        }

        public static void SetTimeflowPro(bool enable)
        {
            if (enable) {
                Debug.Log("Timeflow Pro has been enabled. The scripting define symbol TIMEFLOW_PRO has been added in Player Settings.");//--KEEP
                EditorScriptingDefineUtils.AddScriptingDefineSymbol(_TIMEFLOW_PRO);
            }
            else {
                Debug.Log("Timeflow Pro has been disabled. The scripting define symbol TIMEFLOW_PRO has been removed from the Player Settings.");//--KEEP
                EditorScriptingDefineUtils.RemoveScriptingDefineSymbol(_TIMEFLOW_PRO);
            }
        }
    }
}
#endif