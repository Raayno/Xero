#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// A scriptable object to automatically configure Spine for use with Unity. This detects when the Spine
    /// package is installed and adds the scripting define symbol USING_SPINE to enable features in Timeflow.
    /// </summary>
    //[CreateAssetMenu()]
    public class SpineConfig : ScriptableObject
    {
#if USING_SPINE
        private static bool HasDialogDisplayed = false;
#endif

        public static bool IsSpineInstalled => EditorScriptingDefineUtils.NamespaceExists("Spine.Unity");

        private void OnEnable()
        {
            if (Application.isPlaying) return;
#if USING_SPINE
            if (!IsSpineInstalled) {
                if (HasDialogDisplayed) return; // Only display once per session
                HasDialogDisplayed = true;
                if (EditorUtil.ShowDialog("Spine-Unity not Installed", "The scripting define symbol USING_SPINE has been defined, " +
                    "but the Spine-Unity package is not installed. To get the package visit: https://en.esotericsoftware.com/spine-unity", "Remove Symbol", "Open Link")) {
                    EditorScriptingDefineUtils.RemoveScriptingDefineSymbol("USING_SPINE");
                }
                else {
                    Application.OpenURL("https://en.esotericsoftware.com/spine-unity");
                }
            }
#else
            if (IsSpineInstalled) {
                List<string> symbols = EditorScriptingDefineUtils.GetScriptingDefineSymbols();
                if (!symbols.Contains("USING_SPINE")) {
                    Debug.Log("The scripting define symbol #USING_SPINE has been added to the player settings to support Spine animations in Timeflow.");//--KEEP
                    EditorScriptingDefineUtils.AddScriptingDefineSymbol("USING_SPINE");
                }
            }
#endif
        }

    }
}
#endif
