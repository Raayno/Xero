#if UNITY_EDITOR

namespace AxonGenesis
{
    public class TimeflowChannelComponentPresetEdit<T> : ComponentPresetEditBase<T> where T : TimeflowChannelComponentPreset
    {
        protected override void GUI_Custom()
        {
            GUI_Loop();
            base.GUI_Custom();
        }

        protected virtual void GUI_Loop()
        {
            target.Loop.OnGUI(target);
        }
    }

}

#endif
