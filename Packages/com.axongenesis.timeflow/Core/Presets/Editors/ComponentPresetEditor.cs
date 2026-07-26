#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{

    [CustomEditor(typeof(ComponentPreset))]
    public class ComponentPresetEditor : ComponentPresetEditorBase<ComponentPreset, ComponentPresetEdit> { }

    public class ComponentPresetEdit : ComponentPresetEditBase<ComponentPreset>
    {
    }
}

#endif
