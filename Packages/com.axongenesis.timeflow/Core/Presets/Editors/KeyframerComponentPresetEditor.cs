#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(KeyframerComponentPreset))]
    public class KeyframerComponentPresetEditor : ComponentPresetEditorBase<KeyframerComponentPreset, KeyframerComponentPresetEdit> { }

    public class KeyframerComponentPresetEdit : TimeflowChannelComponentPresetEdit<KeyframerComponentPreset> 
    {
    }
}

#endif
