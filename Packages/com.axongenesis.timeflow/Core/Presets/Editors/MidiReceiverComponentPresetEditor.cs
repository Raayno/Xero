#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiReceiverComponentPreset))]
    public class MidiReceiverComponentPresetEditor : ComponentPresetEditorBase<MidiReceiverComponentPreset, MidiReceiverComponentPresetEdit> { }

    public class MidiReceiverComponentPresetEdit : ComponentPresetEditBase<MidiReceiverComponentPreset> 
    {
    }
}

#endif
