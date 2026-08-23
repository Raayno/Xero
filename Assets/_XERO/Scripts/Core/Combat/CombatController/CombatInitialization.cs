using UnityEngine;
using Gaskellgames;

public partial class CombatController : MoreMountains.Tools.MMSingleton<CombatController>
{
    [Header("Combat Initialization")]
    [SerializeField] private CombatPositioning combatPositioning = new();
    [Button("Add Current Positioning of Participants")] private void AddCurrentPositioningOfParticipants() => combatPositioning.AddCurrentPositioningOfParticipants(GetPlayersAsParticipant(), GetEnemiesAsParticipant());
    
    [Header("Fallback if DataCarrier was not set")]
    [SerializeField] private PlayersCombatData fallbackPlayersData = new();
    [SerializeField] private EnemiesCombatData fallbackEnemiesData = new();
    private PlayersCombatData PlayersData
    {
        get
        {
            if (PlayerCombatDataCarrier.PlayersCombatData != null)
            {
                return PlayerCombatDataCarrier.PlayersCombatData;
            }
            else
            {
                Debug.LogWarning("[CombatInitialization] PlayerParticipants data is null or empty. Returning default arena preset.");
                return fallbackPlayersData;
            }
        }
    }
    private EnemiesCombatData EnemiesData
    {
        get
        {
            if (EnemyCombatDataCarrier.EnemiesCombatData != null)
            {
                return EnemyCombatDataCarrier.EnemiesCombatData;
            }
            else
            {
                Debug.LogWarning("[CombatInitialization] CombatParticipantsDataCarrier data is null. Returning default arena preset.");
                return fallbackEnemiesData;
            }
        }
    }
    
    private Transform playersTransform;
    private Transform PlayersTransform {
        get
        {
            if (playersTransform == null)
            {
                playersTransform = GetOrCreateChild(transform, "Players");
            }
            return playersTransform;
        }
    }
    private Transform enemiesTransform;
    private Transform EnemiesTransform {
        get
        {
            if (enemiesTransform == null)
            {
                enemiesTransform = GetOrCreateChild(transform, "Enemies");
            }
            return enemiesTransform;
        }
    }
    
    private void InitializeCombat()
    {
        SpecialCombatDataCarrier.VariablesLockedForTransition = false; // Unlock variables after transition is complete

        InstantiateParticipants(PlayersData.PlayerParticipants, true);
        InstantiateParticipants(EnemiesData.EnemyParticipants, false);

        void InstantiateParticipants(Participant[] prefabs, bool isPlayer)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                Pose poseData = combatPositioning.GetPose(isPlayer, i, PlayersData.PlayerParticipants.Length, EnemiesData.EnemyParticipants.Length);

                var instance = Instantiate(prefabs[i], poseData.position, poseData.rotation, isPlayer ? PlayersTransform : EnemiesTransform);


                // Add the instantiated participant to the appropriate list
                MoveParticipantToAlive(instance);

                instance.Damageable.Initialize(instance, this);
            }
        }
    }

    
    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);

        if (child == null)
        {
            Debug.LogWarning($"[CombatController] Child '{name}' not found under '{parent.name}'. Creating a new GameObject.");
            // faster and cleaner than using GameObject.Instantiate with a prefab, since we just need an empty GameObject to hold the participants
            child = new GameObject(name).transform;
            child.SetParent(parent);
        }
        return child;
    }

    private void OnValidateInitialization()
    {
        _ = PlayersTransform;
        _ = EnemiesTransform;
    }
}
