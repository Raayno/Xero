using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatTimelineController : MonoBehaviour
{
    [SerializeField] private List<EnemyCombatTarget> enemyTargets;
    [SerializeField] private List<PlayerCombatTarget> playerTargets;

    [SerializeField] private List<CombatTarget> timeline;

    [SerializeField] private TimelineData timelineData;

    private int currentTurnIndex = 0;

    private void Awake()
    {
        CreateTimeline();
    }

    private void CreateTimeline()
    {
        foreach (var target in playerTargets)
        {
            timeline.Add(target);
        }
        foreach (var target in enemyTargets)
        {
            timeline.Add(target);
        }
    }

    public CombatTarget GetCurrentTarget()
    {
        if (timeline.Count == 0)
            return null;
        return timeline[currentTurnIndex];
    }

    public void OnTurnComplete()
    {
        currentTurnIndex = (currentTurnIndex + 1) % timeline.Count;
    }

}


[Serializable]
public class TimelineData
{
    [Range(1, 10)]
    public int maxTimeline = 10;

    public int turnProbability = 1;
}