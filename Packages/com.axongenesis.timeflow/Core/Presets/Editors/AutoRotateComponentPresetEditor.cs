#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AutoRotateComponentPreset))]
    public class AutoRotateComponentPresetEditor : ComponentPresetEditorBase<AutoRotateComponentPreset, AutoRotateComponentPresetEdit> { }

    public class AutoRotateComponentPresetEdit : ComponentPresetEditBase<AutoRotateComponentPreset> 
    {
    }
}

#endif
