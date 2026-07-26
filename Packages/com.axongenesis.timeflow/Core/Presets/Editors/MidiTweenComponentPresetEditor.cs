#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiTweenComponentPreset))]
    public class MidiTweenComponentPresetEditor : ComponentPresetEditorBase<MidiTweenComponentPreset, MidiTweenComponentPresetEdit> { }

    public class MidiTweenComponentPresetEdit : TimeflowChannelComponentPresetEdit<MidiTweenComponentPreset> 
    {
    }
}

#endif
