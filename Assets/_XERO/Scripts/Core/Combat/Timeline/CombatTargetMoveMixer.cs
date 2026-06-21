using UnityEngine;
using UnityEngine.Playables;

public class CombatTargetMoveMixer : PlayableBehaviour
{
    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        if (playerData is not CombatTarget actor)
        {
            return;
        }

        int inputCount = playable.GetInputCount();

        CombatTargetMoveBehaviour selectedBehaviour = null;
        Playable selectedInput = Playable.Null;
        float highestWeight = 0f;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);

            Playable input = playable.GetInput(i);

            if (!input.IsValid())
            {
                continue;
            }

            ScriptPlayable<CombatTargetMoveBehaviour> inputPlayable =
                (ScriptPlayable<CombatTargetMoveBehaviour>)input;

            CombatTargetMoveBehaviour behaviour = inputPlayable.GetBehaviour();

            if (behaviour == null)
            {
                continue;
            }

            if (inputWeight <= 0.001f)
            {
                behaviour.ResetRuntimeState();
                continue;
            }

            if (inputWeight > highestWeight)
            {
                highestWeight = inputWeight;
                selectedBehaviour = behaviour;
                selectedInput = input;
            }
        }

        if (selectedBehaviour == null)
        {
            return;
        }

        double duration = selectedInput.GetDuration();

        if (duration <= 0d)
        {
            return;
        }

        double currentTime = selectedInput.GetTime();
        float normalizedTime = Mathf.Clamp01((float)(currentTime / duration));

        IExposedPropertyTable resolver = playable.GetGraph().GetResolver();

        selectedBehaviour.Apply(actor, resolver, normalizedTime);
    }
}