using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatTimelineController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private List<EnemyCombatTarget> enemyTargets = new List<EnemyCombatTarget>();
    [SerializeField] private List<PlayerCombatTarget> playerTargets = new List<PlayerCombatTarget>();

    [Header("Runtime Timeline")]
    [SerializeField] private List<CombatTarget> timeline = new List<CombatTarget>();

    [Header("Timeline Data")]
    [SerializeField] private TimelineData timelineData;

    private int currentTurnIndex = 0;

    private void Awake()
    {
        CreateTimeline();
    }

    private void CreateTimeline()
    {
        timeline.Clear();

        AddValidTargetsToTimeline(playerTargets);
        AddValidTargetsToTimeline(enemyTargets);

        ClampCurrentTurnIndex();
    }

    private void AddValidTargetsToTimeline<T>(List<T> targets) where T : CombatTarget
    {
        if (targets == null)
        {
            return;
        }

        foreach (T target in targets)
        {
            if (target == null)
            {
                continue;
            }

            if (target.IsDefeated)
            {
                continue;
            }

            timeline.Add(target);
        }
    }

    public CombatTarget GetCurrentTarget()
    {
        if (timeline == null || timeline.Count == 0)
        {
            Debug.LogWarning("[CombatTimelineController] Timeline is empty. No current target available.");
            return null;
        }

        ClampCurrentTurnIndex();

        return timeline[currentTurnIndex];
    }

    public void OnTurnComplete()
    {
        if (timeline == null || timeline.Count == 0)
        {
            Debug.LogWarning("[CombatTimelineController] Cannot complete turn because timeline is empty.");
            return;
        }

        currentTurnIndex++;

        if (currentTurnIndex >= timeline.Count)
        {
            currentTurnIndex = 0;
        }
    }

    public void RefreshTimeline()
    {
        CombatTarget previousTarget = GetCurrentTarget();

        CreateTimeline();

        if (previousTarget == null)
        {
            return;
        }

        int previousTargetIndex = timeline.IndexOf(previousTarget);

        if (previousTargetIndex >= 0)
        {
            currentTurnIndex = previousTargetIndex;
            return;
        }

        ClampCurrentTurnIndex();
    }

    public List<CombatTarget> GetTimeline()
    {
        return new List<CombatTarget>(timeline);
    }

    public List<EnemyCombatTarget> GetEnemies()
    {
        return new List<EnemyCombatTarget>(enemyTargets);
    }

    public List<PlayerCombatTarget> GetPlayers()
    {
        return new List<PlayerCombatTarget>(playerTargets);
    }

    private void ClampCurrentTurnIndex()
    {
        if (timeline == null || timeline.Count == 0)
        {
            currentTurnIndex = 0;
            return;
        }

        currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, timeline.Count - 1);
    }
}

[Serializable]
public class TimelineData
{
    [Range(1, 10)]
    public int maxTimeline = 10;

    public int turnProbability = 1;
}