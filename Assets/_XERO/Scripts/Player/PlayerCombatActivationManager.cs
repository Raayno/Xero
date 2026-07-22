using System.Collections.Generic;
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
        [SerializeField] private string[] attackAnimations = { "Attack1" };
        [SerializeField] private string idleBlendTreeStateName = "Idle Walk Run Blend";
        [SerializeField] private float attackCrossFadeDuration = 0.05f;
        [SerializeField] private float idleCrossFadeDuration = 0.1f;

        [Tooltip("If player is not moving when AttackFinish event is called, attack waits until this normalized time before returning to idle.")]
        [Range(0.75f, 1f)]
        [SerializeField] private float attackFullFinishNormalizedTime = 0.98f;

        [Tooltip("How much movement input is required to early return to idle/walk/run after AttackFinish event.")]
        [SerializeField] private float attackMoveInputThreshold = 0.1f;

        [Header("Attack Hit Detection")]
        [SerializeField] private LayerMask attackableLayers;

        [Tooltip("Local offset from player position where the box cast starts.")]
        [SerializeField] private Vector3 boxCastOriginOffset = new Vector3(0f, 1f, 0.5f);

        [Tooltip("Size of the attack box.")]
        [SerializeField] private Vector3 boxCastSize = new Vector3(1.2f, 1.2f, 1.2f);

        [Tooltip("How far the box cast checks in front of the player.")]
        [SerializeField] private float boxCastDistance = 1.25f;

        [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Attack Hit Rules")]
        [SerializeField] private bool hitOnlyOneTarget = false;

        [Header("Attack Debug")]
        [SerializeField] private bool debugAttack = true;
        [SerializeField] private bool debugAttackCast = true;
        [SerializeField] private bool drawAttackCastGizmos = true;

        public bool IsAttacking { get; private set; }

        private bool isComboWindowOpen = false;
        private bool isNextAttackQueued = false;
        private bool isWaitingForAttackAnimationToEnd = false;
        private int attackAnimationIndex = 0;

        private ThirdPersonController _thirdPersonController;
        private PlayerAnimationManager _animationManager;
        private PlayerEffectManager _effectManager;
        private StarterAssetsInputs _input;

        private readonly HashSet<IAttackable> _alreadyHitAttackables = new HashSet<IAttackable>();

        public void Initialize(
            ThirdPersonController thirdPersonController,
            PlayerAnimationManager animationManager
        )
        {
            _thirdPersonController = thirdPersonController;
            _animationManager = animationManager;
            _effectManager = GetComponent<PlayerEffectManager>();
            //_input = thirdPersonController.Input;
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

        public void AttackingHit()
        {
            _effectManager?.PlayAttackHitFeedback();
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

        /// <summary>
        /// Call this from the attack animation event on the exact hit frame.
        /// </summary>
        public void TryAttackHit()
        {
            if (!IsAttacking)
                return;

            _alreadyHitAttackables.Clear();

            Vector3 castOrigin = GetBoxCastOrigin();
            Vector3 castDirection = transform.forward;
            Vector3 halfExtents = boxCastSize * 0.5f;
            Quaternion castRotation = transform.rotation;

            RaycastHit[] hits = Physics.BoxCastAll(
                castOrigin,
                halfExtents,
                castDirection,
                castRotation,
                boxCastDistance,
                attackableLayers,
                queryTriggerInteraction
            );

            if (debugAttackCast)
            {
                Debug.Log($"<color=orange>[Attack Hit]</color> Hit count: {hits.Length}");
            }

            bool hasHit = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hitCollider = hits[i].collider;

                if (hitCollider == null)
                    continue;

                IAttackable attackable = hitCollider.GetComponentInParent<IAttackable>();

                if (attackable == null)
                    continue;

                if (!hasHit)
                {
                    AttackingHit();
                    hasHit = true;
                }

                if (_alreadyHitAttackables.Contains(attackable))
                    continue;

                _alreadyHitAttackables.Add(attackable);
                attackable.OnAttack();

                if (debugAttackCast)
                {
                    Debug.Log($"<color=red>[Attack Hit]</color> Attacked: {hitCollider.name}");
                }

                if (hitOnlyOneTarget)
                    break;
            }
        }

        private Vector3 GetBoxCastOrigin()
        {
            return transform.position +
                   transform.right * boxCastOriginOffset.x +
                   transform.up * boxCastOriginOffset.y +
                   transform.forward * boxCastOriginOffset.z;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawAttackCastGizmos)
                return;

            Vector3 castOrigin = GetBoxCastOrigin();
            Vector3 castEnd = castOrigin + transform.forward * boxCastDistance;

            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(castOrigin, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxCastSize);

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(castEnd, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxCastSize);

            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(castOrigin, castEnd);
        }
    }
}
