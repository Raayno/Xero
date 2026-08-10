using UnityEngine;
using System.Collections.Generic;
using Gaskellgames;
using System;
using StarterAssets;
using UnityEngine.Timeline;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SignalReceiver))]
public partial class PlayerBehavior : MonoBehaviour
{
    [SerializeField, ReadOnly] private List<PlayerBehavior_Module> activeModules = new();
    [Tooltip("Asleep modules can are like active modules, but they are not called on Update()")]
    [SerializeField, ReadOnly] private List<PlayerBehavior_Module> asleepModules = new();
    

    [SerializeField] private PlayerBehavior_Module[] startingModules;

    [Tooltip("The default modules should be first in the list, and will be transitioned to if no other modules are playing (first of which conditions are met).")]
    [SerializeField] private SerializedDictionary<PlayerBehavior_Module, TransitionConditionType[]> availableModules = new();

    [SerializeField] private PlayerBehavior_References refs;

    private void Awake()
    {
        refs.animationManager.Initialize();
    }

    private void Start()
    {
        foreach (var module in startingModules)
        {
            TryTransition(null, module);
        }
        if (activeModules.Count == 0)
        {
            TransitionToFirstAvailableModule();
        }
    }

    private void Update()
    {
        for (int i = 0; i < activeModules.Count; i++)
        {
            PlayerBehavior_Module module = activeModules[i];
            module.UpdatePublic();
        }
    }

    /// <returns>false if any of the modules cannot be transitioned to</returns>
    public bool TryAddModules(PlayerBehavior_Module[] newModules)
    {
        bool anyFailed = false;
        foreach (var newModule in newModules)
        {
            if (!TryTransition(null, newModule))
            {
                anyFailed = true;
            }
        }

        return !anyFailed;
    }

    public bool TryTransition(PlayerBehavior_Module oldModule = null, PlayerBehavior_Module newModule = null, bool transitionNewAsAsleep = false)
    {
        if (newModule != null && !CanTransition(newModule)) return false;

        Transition(oldModule, newModule, transitionNewAsAsleep);
        return true;
    }

    /// <summary>
    /// Leaving oldModule or newModule as null will just add a newModule or disable the oldModule respectively.
    /// </summary>
    private void Transition(PlayerBehavior_Module oldModule = null, PlayerBehavior_Module newModule = null, bool transitionNewAsAsleep = false)
    {
        if (oldModule != null)
        {
            activeModules.Remove(oldModule);
            asleepModules.Remove(oldModule);
            oldModule.Disable();
        }

        if (newModule != null)
        {
            if (transitionNewAsAsleep)
            {
                asleepModules.Add(newModule);
            }
            else
            {
                activeModules.Add(newModule);
            }
            newModule.Enable(refs);
        }
        else if (activeModules.Count == 0) // If no modules are active, transition to the first available module
        {
            Debug.LogWarning("[PlayerBehavior] No modules are active after transition. Transitioning to the first available module.");
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
                Debug.LogWarning("[PlayerBehavior] Transitioning to module: " + module.GetType().Name);
                return;
            }
        }

        Debug.Log("[PlayerBehavior] No available modules met the transition conditions.");
    }

    private void OnDestroy()
    {
        foreach (var module in activeModules)
        {
            module.Disable();
        }
    }

    public void WakeUpAsleepModules(PlayerBehavior_Module moduleCallingThis = null)
    {
        if (moduleCallingThis != null && !activeModules.Contains(moduleCallingThis))
        {
            Debug.Log("[PlayerBehavior] WakeUpAsleepModules called by a module that is not active. Ignoring.");
            return;
        }

        for (int i = asleepModules.Count - 1; i >= 0; i--)
        {
            PlayerBehavior_Module module = asleepModules[i];
            asleepModules.RemoveAt(i);
            activeModules.Add(module);
        }
    }

    public void PutToSleepAllExcept(PlayerBehavior_Module moduleToKeep)
    {
        bool moduleToKeepFound = false;
        for (int i = 0; i < activeModules.Count; i++)
        {
            PlayerBehavior_Module module = activeModules[i];
            if (module == moduleToKeep)
            {
                moduleToKeepFound = true;
                continue; // Skip the module to keep
            }
            asleepModules.Add(module);
        }

        activeModules.Clear();
        if (!moduleToKeepFound)
        {
            Debug.LogWarning("[PlayerBehavior] The module to keep was not found in the active modules list.");
        }
        activeModules.Add(moduleToKeep); // Add the module to keep back to the active list, 
    }

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
    public UnityEngine.InputSystem.PlayerInput playerInput;
    public UnityEngine.Playables.PlayableDirector playableDirector;
    public TimelineAsset parryTimelineAsset;
    public SignalAsset parrySignalAsset;
    public IconCooldownController parryIconCooldownController;

    public void OnValidate(PlayerBehavior playerBehavior)
    {
        if (this.playerBehavior == null) this.playerBehavior = playerBehavior;
        if (playerTransform == null) playerTransform = playerBehavior.transform;
        if (characterController == null) characterController = playerBehavior.GetComponent<CharacterController>();
        if (animationManager == null) animationManager = playerBehavior.GetComponent<PlayerAnimationManager>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (feedbacks == null) feedbacks = playerBehavior.GetComponent<Feedbacks>();
        if (playerInput == null) playerInput = playerBehavior.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playableDirector == null) playableDirector = playerBehavior.GetComponent<UnityEngine.Playables.PlayableDirector>();
    }
}
