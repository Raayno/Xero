using UnityEngine;
using UnityEngine.InputSystem;

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

    void Update()
    {
        if (controller.currentEnemy == null || !controller.currentEnemy.attackActive)
            parryConsumed = false;
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (!context.performed || !canParry)
            return;

        TryParry();
    }

    void TryParry()
    {
        if (parryConsumed)
            return;

        if (controller.currentEnemy == null)
            return;

        if (!controller.currentEnemy.attackActive)
            return;

        parryConsumed = true;

        controller.currentEnemy.OnParried();

        if (audioSource && parrySuccessSound)
            audioSource.PlayOneShot(parrySuccessSound);

        Debug.Log("Parry Successful");

        StartCoroutine(ParryCooldownRoutine());
    }

    System.Collections.IEnumerator ParryCooldownRoutine()
    {
        canParry = false;
        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }
}
