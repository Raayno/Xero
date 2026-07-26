#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(PlaceOnSurfaceComponentPreset))]
    public class PlaceOnSurfaceComponentPresetEditor : ComponentPresetEditorBase<PlaceOnSurfaceComponentPreset, PlaceOnSurfaceComponentPresetEdit> { }

    public class PlaceOnSurfaceComponentPresetEdit : ComponentPresetEditBase<PlaceOnSurfaceComponentPreset> 
    {
    }
}

#endif
