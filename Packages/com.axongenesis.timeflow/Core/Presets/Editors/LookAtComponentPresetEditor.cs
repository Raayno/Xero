#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(LookAtComponentPreset))]
    public class LookAtComponentPresetEditor : ComponentPresetEditorBase<LookAtComponentPreset, LookAtComponentPresetEdit> { }

    public class LookAtComponentPresetEdit : ComponentPresetEditBase<LookAtComponentPreset> 
    {
    }
}

#endif
