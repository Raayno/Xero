using UnityEngine;

public class EyesTag : MonoBehaviour
{
    [Header("References for Enemy Behavior Graph (Player Eyes)")]
    [SerializeField] private PlayerBehavior playerBehavior;
    public PlayerBehavior PlayerBehavior
    {
        get
        {
            if (playerBehavior == null)
            {
                playerBehavior = GetComponentInParent<PlayerBehavior>();
                if (playerBehavior == null)
                {
                    Debug.LogError("[EyesTag] PlayerBehavior component not found in parent hierarchy.");
                }
            }
            return playerBehavior;
        }
    }
    public PlayerBehavior_ParryModule ParryModule;
    public int NumbersOfEnemiesChasingThisPlayer { get; set;} = 0;

    void Reset()
    {
        playerBehavior = GetComponentInParent<PlayerBehavior>();
        if (playerBehavior == null)
        {
            Debug.LogError("These eyes either do not belong to a player or there is a PlayerBehavior component missing in its parent/parent's children.");
        }
    }
}
