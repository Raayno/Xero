#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiClonerComponentPreset))]
    public class MidiClonerComponentPresetEditor : ComponentPresetEditorBase<MidiClonerComponentPreset, MidiClonerComponentPresetEdit> { }

    public class MidiClonerComponentPresetEdit : ComponentPresetEditBase<MidiClonerComponentPreset> 
    {
    }
}

#endif
