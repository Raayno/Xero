#if UNITY_EDITOR && USING_SPINE

using UnityEditor;
using UnityEngine;
using System;

namespace AxonGenesis
{
    public static class SpineQuickMenu
    {
        private static bool isHooked = false;

        [InitializeOnLoadMethod]
        private static void HookIntoQuickMenu()
        {
            if (isHooked) return;

            TimeflowQuickMenu.OnMenuBuild += AddCustomMenuItems;
            isHooked = true;
        }

        private static void AddCustomMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(SpineAnimatorEdit.kAddSpineAnimator), false, () => SpineAnimatorEdit.AddSpineTimeflow());
        }

        private static void ExampleAction()
        {
            Debug.Log("Example Action triggered from Timeflow Quick Menu!");
        }
    }
}

#endif
