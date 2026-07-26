#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioSampleComponentPreset))]
    public class AudioSampleComponentPresetEditor : ComponentPresetEditorBase<AudioSampleComponentPreset, AudioSampleComponentPresetEdit> { }

    public class AudioSampleComponentPresetEdit : ComponentPresetEditBase<AudioSampleComponentPreset> 
    {
    }
}

#endif
