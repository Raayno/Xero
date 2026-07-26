#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioSpectrumComponentPreset))]
    public class AudioSpectrumComponentPresetEditor : ComponentPresetEditorBase<AudioSpectrumComponentPreset, AudioSpectrumComponentPresetEdit> { }

    public class AudioSpectrumComponentPresetEdit : ComponentPresetEditBase<AudioSpectrumComponentPreset> 
    {
    }
}

#endif
