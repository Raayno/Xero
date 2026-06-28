using System;
using UnityEngine;

public class CombatActionResolver : MonoBehaviour
{
    private CombatActionContext currentActionContext;

    public event Action<CombatActionContext> ActionFinished;

    public void PlayAction(CombatActionContext actionContext)
    {
        ClearCurrentActionContext();

        currentActionContext = actionContext;

        ApplyCurrentActionContextToTargets(currentActionContext);

        currentActionContext.Attacker.AttackSequenceFinished -= CombatTarget_AttackSequenceFinished;
        currentActionContext.Attacker.AttackSequenceFinished += CombatTarget_AttackSequenceFinished;

        currentActionContext.Attacker.PlayAttackSequence(currentActionContext.AttackData);

        Debug.Log(
            $"<color=#55AAFF>[CombatActionResolver]</color> " +
            $"{currentActionContext.Attacker.CombatantName} is using {currentActionContext.AttackData.name}.");
    }

    public void StopCurrentAction()
    {
        if (currentActionContext == null)
        {
            return;
        }

        if (currentActionContext.Attacker != null)
        {
            currentActionContext.Attacker.StopAttackSequence();
        }

        ClearCurrentActionContext();
    }

    private void CombatTarget_AttackSequenceFinished(Participant combatTarget)
    {
        CombatActionContext finishedContext = currentActionContext;

        ClearCurrentActionContext();

        if (finishedContext == null)
        {
            return;
        }

        ActionFinished?.Invoke(finishedContext);
    }

    private void ApplyCurrentActionContextToTargets(CombatActionContext actionContext)
    {
        if (actionContext == null)
        {
            return;
        }

        if (actionContext.Attacker != null)
        {
            actionContext.Attacker.SetCurrentActionContext(actionContext);
        }

        foreach (Participant receiver in actionContext.Targets)
        {
            if (receiver == null)
            {
                continue;
            }

            receiver.SetCurrentActionContext(actionContext);
        }
    }

    private void ClearCurrentActionContext()
    {
        if (currentActionContext == null)
        {
            return;
        }

        if (currentActionContext.Attacker != null)
        {
            currentActionContext.Attacker.AttackSequenceFinished -= CombatTarget_AttackSequenceFinished;
            currentActionContext.Attacker.ClearCurrentActionContext();
        }

        foreach (Participant receiver in currentActionContext.Targets)
        {
            if (receiver == null)
            {
                continue;
            }

            receiver.ClearCurrentActionContext();
        }

        currentActionContext = null;
    }

    private void OnDisable()
    {
        ClearCurrentActionContext();
    }
}