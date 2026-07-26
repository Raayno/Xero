#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(TweenComponentPreset))]
    public class TweenComponentPresetEditor : ComponentPresetEditorBase<TweenComponentPreset, TweenComponentPresetEdit> { }
    public class TweenComponentPresetEdit : ComponentPresetEditBase<TweenComponentPreset>
    {
        protected override void GUI_Custom()
        {
            AxonGUI.BeginHorizontalBox();
            target.DefaultPropertyName = AxonGUI.FieldText(null, "Default Property", target.DefaultPropertyName);
            target.DefaultPropertyAttribute = AxonGUI.PropertySelectAttribute(Property.PropertyTypes.Vector4, target.DefaultPropertyAttribute, true);
            AxonGUI.EndHorizontal();

            base.GUI_Custom();
        }
    }
}

#endif
