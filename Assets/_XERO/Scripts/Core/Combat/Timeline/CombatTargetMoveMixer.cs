using UnityEngine;
using UnityEngine.Playables;

public class CombatTargetMoveMixer : PlayableBehaviour
{
    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        CombatTarget actor = playerData as CombatTarget;

        if (actor == null)
        {
            return;
        }

        int inputCount = playable.GetInputCount();

        CombatTargetMoveBehaviour strongestBehaviour = null;
        Playable strongestInput = Playable.Null;
        float strongestWeight = 0f;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);

            ScriptPlayable<CombatTargetMoveBehaviour> inputPlayable =
                (ScriptPlayable<CombatTargetMoveBehaviour>)playable.GetInput(i);

            CombatTargetMoveBehaviour behaviour = inputPlayable.GetBehaviour();

            if (inputWeight <= 0.001f)
            {
                behaviour.ResetRuntimeState();
                continue;
            }

            if (inputWeight > strongestWeight)
            {
                strongestWeight = inputWeight;
                strongestBehaviour = behaviour;
                strongestInput = inputPlayable;
            }
        }

        if (strongestBehaviour == null)
        {
            return;
        }

        double duration = strongestInput.GetDuration();

        if (duration <= 0d)
        {
            return;
        }

        double currentTime = strongestInput.GetTime();
        float normalizedTime = Mathf.Clamp01((float)(currentTime / duration));

        IExposedPropertyTable resolver = playable.GetGraph().GetResolver();

        strongestBehaviour.Apply(actor, resolver, normalizedTime);
    }
}