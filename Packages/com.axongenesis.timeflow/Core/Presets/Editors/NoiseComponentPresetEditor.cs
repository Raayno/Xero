#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(NoiseComponentPreset))]
    public class NoiseComponentPresetEditor : ComponentPresetEditorBase<NoiseComponentPreset, NoiseComponentPresetEdit> { }

    public class NoiseComponentPresetEdit : ComponentPresetEditBase<NoiseComponentPreset> 
    {
    }
}

#endif
