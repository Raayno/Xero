using UnityEngine;
using UnityEngine.Playables;
using Vastav.Utils.Input;
using System.Collections.Generic;

public class PlayerBehavior_ParryModule : PlayerBehavior_Module
{

    protected override void EnableModule()
    {
        InputSystem_PlayerActionsSO.OnParryEvent += HandleParryInput;
        ShowIcon();
    }

    protected override void DisableModule()
    {
        InputSystem_PlayerActionsSO.OnParryEvent -= HandleParryInput;
        HideIcon();
        EndParry(); // Ensure that any ongoing parry is ended when the module is disabled
    }

    private void ShowIcon()
    {
        if (refs.parryIconCooldownController != null)
        {
            refs.parryIconCooldownController.gameObject.SetActive(true);
            refs.parryIconCooldownController.SkipCooldown();
        }
    }

    private void HideIcon()
    {
        if (refs.parryIconCooldownController != null)
        {
            refs.parryIconCooldownController.gameObject.SetActive(false);
        }
    }

    private void HandleParryInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("[PlayerBehavior_ParryModule] Parry input received.");

        if (CannotParry())
        {
            Debug.Log("Cannot parry at this time.");
            return;
        }

        Parry();
    }

    private bool CannotParry()
    {
        if (refs.playableDirector.state == PlayState.Playing)
        {
            Debug.Log("Parry is already in progress.");
            return true;
        }

        return false; // Player can parry
    }

#region Parry performing
    private bool isParryInProgress = false;
    private List<PlayerBehavior_Module> clearedModules;
    private void Parry()
    {
        isParryInProgress = true;
        refs.playerBehavior.ClearAllExcept(this, out clearedModules);

        refs.playableDirector.stopped += OnPlayableDirectorStopped;

        // Start the parry timeline
        refs.playableDirector.playableAsset = refs.parryTimelineAsset;
        refs.playableDirector.Play();

        // Start the cooldown for the parry ability
        if (refs.parryIconCooldownController != null) refs.parryIconCooldownController.StartCooldown((float)refs.parryTimelineAsset.duration);
    }

    private void EndParry()
    {
        isParryInProgress = false;

        if (refs.playableDirector != null)
        {
            refs.playableDirector.stopped -= OnPlayableDirectorStopped;
        }
        if (SpecialCombatDataCarrier.BattleEntryType == BattleEntryType.PlayerParry)
        {
            Debug.LogWarning("Parry module is being disabled while the BattleEntryType is still set to PlayerParry. This might indicate an unclosed parry window in the timeline.");
            ToggleParryBattleEntryType(); // ensure that we reset the state after parry
        }

        // Restore modules from before parry
        if (clearedModules != null && clearedModules.Count > 0)
        {
            refs.playerBehavior.TryAddModules(clearedModules.ToArray());
            clearedModules.Clear();
        }
    }
    
    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (director == refs.playableDirector)
        {
            EndParry();
        }
    }

    public override void OnSignalReceived(UnityEngine.Timeline.SignalAsset signal)
    {
        if (refs.parrySignalAsset == null)
        {
            Debug.LogError("[PlayerBehavior_ParryModule] Parry signal asset is not assigned.");
            return;
        }
        if (isParryInProgress && signal == refs.parrySignalAsset)
        {
            ToggleParryBattleEntryType();
        }
    }

    private BattleEntryType cashedBattleEntryType = BattleEntryType.EnemyAttack;
    private void ToggleParryBattleEntryType()
    {
        if (SpecialCombatDataCarrier.BattleEntryType != BattleEntryType.PlayerParry)
        {
            cashedBattleEntryType = SpecialCombatDataCarrier.BattleEntryType; // Cache the current state before parry
            SpecialCombatDataCarrier.BattleEntryType = BattleEntryType.PlayerParry; // Set to PlayerParry during the parry window (to carry that data into combat if it were entered during the parry window)
        }
        else
        {
            SpecialCombatDataCarrier.BattleEntryType = cashedBattleEntryType; // Reset to the previous state after parry
        }
    }
#endregion
}
