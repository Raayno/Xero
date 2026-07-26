#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(PlaceOnPathComponentPreset))]
    public class PlaceOnPathComponentPresetEditor : ComponentPresetEditorBase<PlaceOnPathComponentPreset, PlaceOnPathComponentPresetEdit> { }

    public class PlaceOnPathComponentPresetEdit : ComponentPresetEditBase<PlaceOnPathComponentPreset> 
    {
    }
}

#endif
