#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{

    [CustomEditor(typeof(AdvancedPresetsGlobalConfig))]
    public class AdvancedPresetsConfigEditor : Editor
    {
        private bool _hasLoaded = false;

        private void Setup()
        {
            if (!_hasLoaded) {
                _hasLoaded = true;
            }
            AxonGUI.Setup(70);
        }

        public override void OnInspectorGUI()
        {
            Setup();
            AdvancedPresetsGlobalConfig.Instance = (AdvancedPresetsGlobalConfig)target;           
            AdvancedPresetsGlobalConfig.GUI();
        }
    }
}

#endif
