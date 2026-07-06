using UnityEngine;
using NaughtyAttributes;
using System.Text;

public class DebugPlayerCombatDataCarrier : MonoBehaviour
{
    [SerializeField] private PlayerParticipant[] debugPlayerParticipants;

    [Button("Get Player Participants (Console)")] private void GetDebugPlayerParticipantsConsole()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[DebugPlayerCombatDataCarrier] Player Participants:");
        foreach (var participant in PlayerCombatDataCarrier.PlayerParticipants)
        {
            sb.AppendLine($"- {participant.name}");
        }
        Debug.Log(sb.ToString());
    }

    [Button("Get Player Participants (overwrite this array)")] private void GetDebugPlayerParticipantsArray()
    {
        debugPlayerParticipants = PlayerCombatDataCarrier.PlayerParticipants;
        Debug.Log($"[DebugPlayerCombatDataCarrier] Player Participants array overwritten with {debugPlayerParticipants.Length} participants.");
    }

    [Button("Set Player Participants")] private void SetDebugPlayerParticipants()
    {
        if (debugPlayerParticipants == null || debugPlayerParticipants.Length == 0)
        {
            Debug.LogError("[DebugPlayerCombatDataCarrier] Debug Player Participants is null or empty. Cannot set.");
            return;
        }

        PlayerCombatDataCarrier.PlayerParticipants = debugPlayerParticipants;
    }

    private void Start() => SetDebugPlayerParticipants();
}
