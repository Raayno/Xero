using UnityEngine;
using MoreMountains.Feedbacks;

public partial class CombatController : MoreMountains.Tools.MMSingleton<CombatController>
{
    [SerializeField] private MMF_Player LoadSceneFeedback;
    [HideInInspector, SerializeField] private PersistenceRegistry persistenceRegistry;

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
        DisappearEnemyGroupInFreeRoam();
        LoadSceneAfterCombat();

        void DisappearEnemyGroupInFreeRoam()
        {
            if (enemiesData != null && !string.IsNullOrEmpty(enemiesData.FreeRoamEnemyPersistenceKey))
            {
                persistenceRegistry.ActivatePersistenceKey(enemiesData.FreeRoamEnemyPersistenceKey, isClearable: enemiesData.CanReappearInFreeRoam, value: true);
            }
            else
            {
                Debug.LogWarning("[CombatController] No Free Roam Enemy Persistence Key found. Cannot disable enemy group in Free Roam.");
            }
        }

        void LoadSceneAfterCombat()
        {
            if (LoadSceneFeedback == null)
            {
                Debug.LogWarning("[CombatController] LoadSceneFeedback is not assigned. Cannot load scene after combat.");
                return;
            }

            var loadFeedback = LoadSceneFeedback.GetFeedbackOfType<MMF_LoadScene>();

            if (loadFeedback == null)
            {
                Debug.LogWarning("[CombatController] No MMF_LoadScene feedback found in LoadSceneFeedback. Cannot load scene after combat.");
                return;
            }

            loadFeedback.DestinationSceneAddressibleKey = enemiesData.SceneToLoadAfterCombatAddressibleKey;

            LoadSceneFeedback.PlayFeedbacks();
        }
    }

    private void PlayersDefeatedResolution()
    {
        Debug.Log("[CombatController] <color=red>Player lost.</color As a placeholder, Combat is Reset.");
        // Implement logic for resolving combat when all players are defeated
        ResetCombat(); // TODO: Replace this with a proper combat resolution when players are defeated
    }
}
