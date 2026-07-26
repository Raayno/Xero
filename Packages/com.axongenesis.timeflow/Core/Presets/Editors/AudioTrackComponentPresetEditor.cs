#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioTrackComponentPreset))]
    public class AudioTrackComponentPresetEditor : ComponentPresetEditorBase<AudioTrackComponentPreset, AudioTrackComponentPresetEdit> { }

    public class AudioTrackComponentPresetEdit : ComponentPresetEditBase<AudioTrackComponentPreset> 
    {
    }
}

#endif
