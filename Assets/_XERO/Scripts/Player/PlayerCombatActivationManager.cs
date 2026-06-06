using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using Vastav.Utils.Input;
#endif

namespace StarterAssets
{
    public class PlayerCombatActivationManager : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private string[] attackAnimations = { "Attack1", "Attack2", "Attack3" };
        [SerializeField] private string idleBlendTreeStateName = "Idle Walk Run Blend";
        [SerializeField] private float attackCrossFadeDuration = 0.05f;
        [SerializeField] private float idleCrossFadeDuration = 0.1f;

        [Tooltip("If player is not moving when AttackFinish event is called, attack waits until this normalized time before returning to idle.")]
        [Range(0.75f, 1f)]
        [SerializeField] private float attackFullFinishNormalizedTime = 0.98f;

        [Tooltip("How much movement input is required to early return to idle/walk/run after AttackFinish event.")]
        [SerializeField] private float attackMoveInputThreshold = 0.1f;

        [Header("Attack Debug")]
        [SerializeField] private bool debugAttack = true;

        public bool IsAttacking { get; private set; }

        private bool isComboWindowOpen = false;
        private bool isNextAttackQueued = false;
        private bool isWaitingForAttackAnimationToEnd = false;
        private int attackAnimationIndex = 0;

        private ThirdPersonController _thirdPersonController;
        private PlayerAnimationManager _animationManager;
        private PlayerEffectManager _effectManager;
        private StarterAssetsInputs _input;

        public void Initialize(
            ThirdPersonController thirdPersonController,
            PlayerAnimationManager animationManager
        )
        {
            _thirdPersonController = thirdPersonController;
            _animationManager = animationManager;
            _effectManager = GetComponent<PlayerEffectManager>();
            _input = thirdPersonController.Input;
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem_PlayerActionsSO.OnAttackEvent += InputSystem_PlayerActionsSO_OnAttackEvent;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void InputSystem_PlayerActionsSO_OnAttackEvent(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
                return;

            Attack();
        }
#endif

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem_PlayerActionsSO.OnAttackEvent -= InputSystem_PlayerActionsSO_OnAttackEvent;
#endif
        }

        private void Attack()
        {
            if (_animationManager == null || !_animationManager.HasAnimator)
                return;

            if (attackAnimations == null || attackAnimations.Length == 0)
                return;

            if (_input.jump)
                return;

            if (isWaitingForAttackAnimationToEnd)
                return;

            if (!IsAttacking)
            {
                attackAnimationIndex = 0;
                PlayCurrentAttackAnimation();
                return;
            }

            if (!isComboWindowOpen)
                return;

            if (isNextAttackQueued)
                return;

            int nextAttackIndex = attackAnimationIndex + 1;

            if (nextAttackIndex >= attackAnimations.Length)
                return;

            isNextAttackQueued = true;

            if (debugAttack)
            {
                Debug.Log($"<color=yellow>[Attack]</color> Queued next attack: {attackAnimations[nextAttackIndex]}");
            }
        }

        private void PlayCurrentAttackAnimation()
        {
            IsAttacking = true;
            isComboWindowOpen = false;
            isNextAttackQueued = false;
            isWaitingForAttackAnimationToEnd = false;

            _input.jump = false;

            string attackStateName = attackAnimations[attackAnimationIndex];

            if (debugAttack)
            {
                Debug.Log($"<color=orange>[Attack]</color> Playing: {attackStateName}");
            }

            _animationManager.PlayAttackAnimation(
                attackStateName,
                attackCrossFadeDuration
            );
        }

        public void Attacking()
        {
            IsAttacking = true;
            _effectManager?.PlayAttackFeedback();
        }

        public void AttackFinish()
        {
            if (debugAttack)
            {
                Debug.Log($"<color=green>[Attack]</color> AttackFinish called on index: {attackAnimationIndex}");
            }

            if (isNextAttackQueued)
            {
                attackAnimationIndex++;
                PlayCurrentAttackAnimation();
                return;
            }

            if (IsMovementInputPressed())
            {
                if (debugAttack)
                {
                    Debug.Log("<color=cyan>[Attack]</color> Movement input detected. Returning to idle/walk/run blend immediately.");
                }

                ResetAttack();
                return;
            }

            isWaitingForAttackAnimationToEnd = true;
            isComboWindowOpen = false;

            if (debugAttack)
            {
                Debug.Log("<color=yellow>[Attack]</color> No movement input. Waiting for full attack animation to finish.");
            }
        }

        public void UpdateAttackAnimationEndWait()
        {
            if (!isWaitingForAttackAnimationToEnd)
                return;

            if (_animationManager == null || !_animationManager.HasAnimator)
            {
                ResetAttack();
                return;
            }

            if (attackAnimations == null || attackAnimations.Length == 0)
            {
                ResetAttack();
                return;
            }

            string currentAttackStateName = attackAnimations[attackAnimationIndex];

            AnimatorStateInfo currentState = _animationManager.GetCurrentBaseLayerStateInfo();

            if (!currentState.IsName(currentAttackStateName))
                return;

            if (_animationManager.IsInBaseLayerTransition())
                return;

            if (currentState.normalizedTime >= attackFullFinishNormalizedTime)
            {
                if (debugAttack)
                {
                    Debug.Log("<color=green>[Attack]</color> Full attack animation completed. Returning to idle.");
                }

                ResetAttack();
            }
        }

        private bool IsMovementInputPressed()
        {
            if (_input == null)
                return false;

            return _input.move.sqrMagnitude >= attackMoveInputThreshold * attackMoveInputThreshold;
        }

        private void ResetAttack()
        {
            if (debugAttack)
            {
                Debug.Log("<color=cyan>[Attack]</color> Combo finished.");
            }

            IsAttacking = false;
            isComboWindowOpen = false;
            isNextAttackQueued = false;
            isWaitingForAttackAnimationToEnd = false;
            attackAnimationIndex = 0;

            _animationManager.PlayIdleBlendTree(
                idleBlendTreeStateName,
                idleCrossFadeDuration
            );
        }

        public void SetComboWindowOpen()
        {
            if (!IsAttacking)
                return;

            if (isWaitingForAttackAnimationToEnd)
                return;

            isComboWindowOpen = true;

            if (debugAttack)
            {
                Debug.Log($"<color=lime>[Attack]</color> Combo window OPEN for: {attackAnimations[attackAnimationIndex]}");
            }
        }

        public void SetComboWindowClose()
        {
            isComboWindowOpen = false;

            if (debugAttack)
            {
                Debug.Log($"<color=red>[Attack]</color> Combo window CLOSE for: {attackAnimations[attackAnimationIndex]}");
            }
        }
    }
}