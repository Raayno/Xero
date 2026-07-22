using UnityEngine;
using System.Collections.Generic;
using Gaskellgames;
using System;
using StarterAssets;

// [AlchemySerialize]
public partial class PlayerBehavior : MonoBehaviour
{
    [SerializeField, ReadOnly] private List<PlayerBehavior_Module> activeModules = new();

    [Tooltip("The default modules should be first in the list, and will be transitioned to if no other modules are playing (first of which conditions are met).")]
    [SerializeField] private SerializedDictionary<PlayerBehavior_Module, TransitionConditionType[]> availableModules = new();

    [SerializeField, ReadOnly] private HashSet<TransitionConditionType> conditionsToUpdate = new();

    [SerializeField] private PlayerBehavior_References refs;

    private bool IsBlockForcedTarnsition => activeModules.Exists(module => module.IsBlockForcedTransition);

    private void Awake()
    {
        refs.animationManager.Initialize();
        TransitionToFirstAvailableModule();
    }

    private void Update()
    {
        for (int i = 0; i < activeModules.Count; i++)
        {
            PlayerBehavior_Module module = activeModules[i];
            module.UpdatePublic();
        }
    }

    public bool TryTransition(PlayerBehavior_Module oldModule = null, PlayerBehavior_Module newModule = null)
    {
        if (newModule != null && !CanTransition(newModule)) return false;

        Transition(oldModule, newModule);
        return true;
    }

    /// <summary>
    /// Leaving oldModule or newModule as null will just add a newModule or disable the oldModule respectively.
    /// </summary>
    private void Transition(PlayerBehavior_Module oldModule = null, PlayerBehavior_Module newModule = null)
    {
        if (oldModule != null)
        {
            activeModules.Remove(oldModule);
            oldModule.Disable();
        }

        if (newModule != null)
        {
            activeModules.Add(newModule);
            newModule.Enable(refs);
        }
        else
        {
            TransitionToFirstAvailableModule();
        }
    }

    private void TransitionToFirstAvailableModule()
    {
        if (availableModules.Count == 0)
        {
            Debug.Log("[PlayerBehavior] No available modules to transition to.");
            return;
        }

        foreach (var module in availableModules.Keys)
        {
            if (TryTransition(null, module))
            {
                Debug.LogWarning("[PlayerBehavior] No module was active after the transition to module: " + module.GetType().Name);
                return;
            }
        }

        Debug.Log("[PlayerBehavior] No available modules met the transition conditions.");
    }



    // /// <summary>
    // /// CAREFUL: This will clear the activeModules list and force transition to the new modules.
    // /// </summary>
    // /// <param name="newModules"></param>
    // public void ForceTransitionTo(PlayerBehavior_Module[] newModules)
    // {
    //     if (newModules == null) return;

    //     if (IsBlockForcedTarnsition)
    //     {
    //         Debug.Log("[PlayerBehavior] Transition blocked, one or more of active modules are blocking forced transitions.");
    //         return;
    //     }

    //     activeModules.ForEach(module => module.Disable());
    //     activeModules.Clear();

    //     foreach (var module in newModules)
    //     {
    //         TryTransition(null, module);
    //     }
    // }

    private void OnValidate()
    {
        refs.OnValidate(this);
    }
}

[Serializable]
public class PlayerBehavior_References
{
    public PlayerBehavior playerBehavior;
    public Transform playerTransform;
    public CharacterController characterController;
    public PlayerAnimationManager animationManager;
    public Camera mainCamera;
    public Feedbacks feedbacks;

    public void OnValidate(PlayerBehavior playerBehavior)
    {
        if (this.playerBehavior == null) this.playerBehavior = playerBehavior;
        if (playerTransform == null) playerTransform = playerBehavior.transform;
        if (characterController == null) characterController = playerBehavior.GetComponent<CharacterController>();
        if (animationManager == null) animationManager = playerBehavior.GetComponent<PlayerAnimationManager>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (feedbacks == null) feedbacks = playerBehavior.GetComponent<Feedbacks>();
    }
}
