#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioReactiveComponentPreset))]
    public class AudioReactiveComponentPresetEditor : ComponentPresetEditorBase<AudioReactiveComponentPreset, AudioReactiveComponentPresetEdit> { }

    public class AudioReactiveComponentPresetEdit : ComponentPresetEditBase<AudioReactiveComponentPreset> 
    {
    }
}

#endif
