#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(LookAtTargetComponentPreset))]
    public class LookAtTargetComponentPresetEditor : ComponentPresetEditorBase<LookAtTargetComponentPreset, LookAtTargetComponentPresetEdit> { }

    public class LookAtTargetComponentPresetEdit : ComponentPresetEditBase<LookAtTargetComponentPreset> 
    {
    }
}

#endif
