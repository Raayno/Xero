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

    public CombatDamageable Damageable;
    public TurnExec TurnExec;

    [Header("Attack Sequence")]
    public ParticipantMovable ParticipantMovable;
    public Animator Animator;

    [Header("Feedbacks")]
    public Feedbacks Feedbacks;

    [SerializeField] protected bool enableDebug = false;

    protected virtual void Awake()
    {
        Reset();
    }

    protected virtual void Reset()
    {
        Damageable = Damageable != null ? Damageable : GetComponent<CombatDamageable>();
        ParticipantMovable = ParticipantMovable != null ? ParticipantMovable : GetComponentInChildren<ParticipantMovable>();
        
        if (string.IsNullOrWhiteSpace(participantName))
        {
            participantName = gameObject.name;
        }

        Animator = Animator != null ? Animator : GetComponentInChildren<Animator>();
    }
}