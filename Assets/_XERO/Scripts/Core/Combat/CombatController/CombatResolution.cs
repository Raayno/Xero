using UnityEngine;

public partial class CombatController : MoreMountains.Tools.MMSingleton<CombatController>
{
    private void CheckForCombatEnd()
    {
        if (alivePlayerParticipants.Count == 0)
        {
            Debug.Log("[CombatController] All players have been defeated. Combat ends.");
            PlayersDefeatedResolution();
        }
        else if (aliveEnemyParticipants.Count == 0)
        {
            Debug.Log("[CombatController] All enemies have been defeated. Combat ends.");
            EnemiesDefeatedResolution();
        }
    }

    private void EnemiesDefeatedResolution()
    {
        Debug.Log("[CombatController] <color=green>All enemies have been defeated.</color> As a placeholder, Combat is Reset.");
        // Implement logic for resolving combat when all enemies are defeated
        // e.g. slowdown time, show victory screen, etc.
        ResetCombat(); // TODO: Replace this with a proper combat resolution when enemies are defeated
    }

    private void PlayersDefeatedResolution()
    {
        Debug.Log("[CombatController] <color=red>Player lost.</color As a placeholder, Combat is Reset.");
        // Implement logic for resolving combat when all players are defeated
        ResetCombat(); // TODO: Replace this with a proper combat resolution when players are defeated
    }
}
