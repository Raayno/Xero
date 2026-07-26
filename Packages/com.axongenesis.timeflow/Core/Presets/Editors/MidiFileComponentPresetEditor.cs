#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiFileComponentPreset))]
    public class MidiFileComponentPresetEditor : ComponentPresetEditorBase<MidiFileComponentPreset, MidiFileComponentPresetEdit> { }

    public class MidiFileComponentPresetEdit : ComponentPresetEditBase<MidiFileComponentPreset> 
    {
    }
}

#endif
