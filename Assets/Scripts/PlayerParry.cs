using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry")]
    public float parryCooldown = 0.25f;
    public AudioSource audioSource;
    public AudioClip parrySuccessSound;

    bool canParry = true;
    bool parryConsumed;

    PlayerController controller;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (!context.performed || !canParry || parryConsumed)
            return;

        TryParry();
    }

    void TryParry()
    {
        if (controller.currentEnemy == null)
            return;

        if (!controller.currentEnemy.attackActive)
            return;

        parryConsumed = true;

        controller.currentEnemy.OnParried();

        if (audioSource && parrySuccessSound)
            audioSource.PlayOneShot(parrySuccessSound);

        Debug.Log("Parry Successful");

        StartCoroutine(Cooldown());
    }

    // 🔑 CALLED BY ENEMY WHEN A NEW WINDOW OPENS
    public void ResetParryWindow()
    {
        parryConsumed = false;
    }

    IEnumerator Cooldown()
    {
        canParry = false;
        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }
}
