using UnityEngine;

[RequireComponent(typeof(CombatDamageable))]
public abstract class Participant : MonoBehaviour
{
    [Header("Combat Participant")]
    [SerializeField] protected string participantName;
    public string CombatantName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(participantName))
            {
                return participantName;
            }

            return gameObject.name;
        }
    }

    public CombatDamageable damageable;
    public TurnExec turnExec;

    [Header("Attack Sequence")]
    public ParticipantMovable ParticipantMovable;
    public Animator Animator;

    [SerializeField] protected bool enableDebug = false;

    protected virtual void Awake()
    {
        Reset();
    }

    protected virtual void Reset()
    {
        damageable = damageable != null ? damageable : GetComponent<CombatDamageable>();
        ParticipantMovable = ParticipantMovable != null ? ParticipantMovable : GetComponentInChildren<ParticipantMovable>();
        
        if (string.IsNullOrWhiteSpace(participantName))
        {
            participantName = gameObject.name;
        }

        Animator = Animator != null ? Animator : GetComponentInChildren<Animator>();
    }
}