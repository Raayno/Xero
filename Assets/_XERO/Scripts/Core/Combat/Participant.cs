using UnityEngine;
using UnityEngine.Playables;
[RequireComponent(typeof(CombatDamageable))]

[RequireComponent(typeof(PlayableDirector))]
public abstract class Participant : MonoBehaviour
{
    [Header("Combat Participant")]
    [SerializeField] protected string combatantName;
    public string CombatantName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(combatantName))
            {
                return combatantName;
            }

            return gameObject.name;
        }
    }

    public CombatDamageable damageable;
    public TurnExec turnExec;
    [SerializeField] private ParticipantMovable participantMovable;

    [Header("Attack Sequence")]
    public PlayableDirector playableDirector;

    protected virtual void Reset()
    {
        damageable = damageable != null ? damageable : GetComponent<CombatDamageable>();
        participantMovable = participantMovable != null ? participantMovable : GetComponentInChildren<ParticipantMovable>();
        playableDirector = playableDirector != null ? playableDirector : GetComponent<PlayableDirector>();
        
        if (string.IsNullOrWhiteSpace(combatantName))
        {
            combatantName = gameObject.name;
        }
    }
}