#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(TimeflowEventComponentPreset))]
    public class TimeflowEventComponentPresetEditor : ComponentPresetEditorBase<TimeflowEventComponentPreset, TimeflowEventComponentPresetEdit> { }

    public class TimeflowEventComponentPresetEdit : ComponentPresetEditBase<TimeflowEventComponentPreset> 
    {
    }
}

#endif
