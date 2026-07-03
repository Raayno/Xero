using UnityEngine;
using UnityEngine.Playables;
[RequireComponent(typeof(CombatDamageable))]
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

    [Header("Attack Sequence")]
    public ParticipantMovable ParticipantMovable;
    public Animator Animator;

    [SerializeField] protected bool enableDebug = false;

    protected virtual void Reset()
    {
        Awake();
    }

    protected virtual void Awake()
    {
        damageable = damageable != null ? damageable : GetComponent<CombatDamageable>();
        ParticipantMovable = ParticipantMovable != null ? ParticipantMovable : GetComponentInChildren<ParticipantMovable>();
        
        if (string.IsNullOrWhiteSpace(combatantName))
        {
            combatantName = gameObject.name;
        }

        Animator = Animator != null ? Animator : GetComponentInChildren<Animator>();
    }
}