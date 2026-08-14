using UnityEngine;
using System.Text;
using Gaskellgames;

public class DebugPlayerCombatDataCarrier : MonoBehaviour
{
    [SerializeField] private PlayerParticipant[] debugPlayerParticipants;

    [Button] private void GetDebugPlayerParticipantsConsole()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[DebugPlayerCombatDataCarrier] Player Participants:");
        foreach (var participant in PlayerCombatDataCarrier.PlayersCombatData.PlayerParticipants)
        {
            sb.AppendLine($"- {participant.name}");
        }
        Debug.Log(sb.ToString());
    }

    [Button] private void GetDebugPlayerParticipantsArray()
    {
        debugPlayerParticipants = PlayerCombatDataCarrier.PlayersCombatData.PlayerParticipants;
        Debug.Log($"[DebugPlayerCombatDataCarrier] Player Participants array overwritten with {debugPlayerParticipants.Length} participants.");
    }

    [Button] private void SetDebugPlayerParticipants()
    {
        if (debugPlayerParticipants == null || debugPlayerParticipants.Length == 0)
        {
            Debug.LogError("[DebugPlayerCombatDataCarrier] Debug Player Participants is null or empty. Cannot set.");
            return;
        }

        PlayerCombatDataCarrier.PlayersCombatData = new(debugPlayerParticipants);
    }

    private void Start() => SetDebugPlayerParticipants();
}
